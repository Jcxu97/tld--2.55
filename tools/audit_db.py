"""Cross-check ItemDatabase.cs entries against extracted vanilla localization.

For each `new ItemEntry("GEAR_X", "现填中文", ...)` line in ItemDatabase.cs:
- Look up GAMEPLAY_X in tld_localization.json
- If zh-Hans differs (case-insensitive ignoring punctuation), report mismatch.
"""
import re, json, os
DB = r"D:\Github\tld--2.55\TldHacks\src\ItemDatabase.cs"
LOC = r"D:\Github\tld--2.55\tools\tld_localization.json"
with open(LOC, encoding="utf-8") as f:
    table = json.load(f)
with open(DB, encoding="utf-8") as f:
    src = f.read()

PATTERN = re.compile(r'new ItemEntry\("(GEAR_[A-Za-z0-9_]+)", "([^"]+)"', re.U)
entries = PATTERN.findall(src)
print(f"DB has {len(entries)} entries")

mismatches = []
missing_in_vanilla = []
for prefab, my_zh in entries:
    suffix = prefab[len("GEAR_"):]
    candidates = [f"GAMEPLAY_{suffix}", f"GAMEPLAY_{suffix}Description"]
    found = None
    for ck in candidates:
        if ck in table:
            found = ck; break
    if not found:
        missing_in_vanilla.append((prefab, my_zh))
        continue
    vz = table[found].get("zh-Hans", "")
    if not vz:
        continue
    if vz != my_zh:
        mismatches.append((prefab, my_zh, vz, table[found].get("en","")))

print(f"\n=== MISMATCHES (different zh-Hans) — {len(mismatches)} ===")
for prefab, mine, vanilla, en in mismatches:
    print(f"  {prefab}")
    print(f"    mine    : {mine}")
    print(f"    vanilla : {vanilla}  (en={en})")

print(f"\n=== KEY NOT IN VANILLA TABLE — {len(missing_in_vanilla)} (probably mod-only or no GAMEPLAY_ key) ===")
for prefab, mine in missing_in_vanilla[:20]:
    print(f"  {prefab}  '{mine}'")
