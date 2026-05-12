"""Rewrite ItemDatabase.cs entries to use official vanilla zh-Hans names.

For each `new ItemEntry("GEAR_X", "现填", "category", ...)` line:
- If GAMEPLAY_X exists in vanilla and zh-Hans differs from current → replace.
- Otherwise leave untouched.
"""
import re, json
DB = r"D:\Github\tld--2.55\TldHacks\src\ItemDatabase.cs"
LOC = r"D:\Github\tld--2.55\tools\tld_localization.json"
with open(LOC, encoding="utf-8") as f:
    table = json.load(f)
with open(DB, encoding="utf-8") as f:
    src = f.read()

# Pattern: capture full ItemEntry call so we can rewrite the 2nd quoted string
PATTERN = re.compile(r'(new ItemEntry\("(GEAR_[A-Za-z0-9_]+)", ")([^"]+)(",)', re.U)

changed = 0
def repl(m):
    global changed
    prefix, prefab, my_zh, suffix = m.group(1), m.group(2), m.group(3), m.group(4)
    suffix_name = prefab[len("GEAR_"):]
    key = f"GAMEPLAY_{suffix_name}"
    e = table.get(key)
    if not e: return m.group(0)
    vz = e.get("zh-Hans", "")
    if not vz or vz == my_zh: return m.group(0)
    # Avoid breaking C# string: vanilla strings should already be safe (no \" or \\)
    if '"' in vz or '\\' in vz:
        print(f"  SKIP unsafe chars: {prefab} -> {vz!r}")
        return m.group(0)
    changed += 1
    print(f"  {prefab}: {my_zh!r} -> {vz!r}")
    return prefix + vz + suffix

new_src = PATTERN.sub(repl, src)
with open(DB, "w", encoding="utf-8") as f:
    f.write(new_src)
print(f"\nReplaced {changed} entries. Done.")
