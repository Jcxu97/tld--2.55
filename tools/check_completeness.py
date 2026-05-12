"""Check ItemDatabase coverage vs vanilla GAMEPLAY_* keys.

Outputs:
- DB entries (count)
- vanilla GAMEPLAY_<x> keys that have NO matching DB entry (potential missing items)
- DB entries with no vanilla key (mod-only or wrong prefab name)

Filters out non-item GAMEPLAY_ keys (UI strings, etc.) by heuristics:
- Skip keys ending in 'Description'
- Skip keys with whitespace in zh value (likely full sentences)
"""
import re, json
DB = r"D:\Github\tld--2.55\TldHacks\src\ItemDatabase.cs"
LOC = r"D:\Github\tld--2.55\tools\tld_localization.json"

with open(LOC, encoding="utf-8") as f:
    table = json.load(f)
with open(DB, encoding="utf-8") as f:
    src = f.read()

PATTERN = re.compile(r'new ItemEntry\("(GEAR_[A-Za-z0-9_]+)"', re.U)
db_prefabs = set(PATTERN.findall(src))
print(f"DB has {len(db_prefabs)} GEAR_ entries")

# Vanilla GAMEPLAY_ keys that look like item names
# Heuristic: short value (<20 chars), no period, no question mark
vanilla_items = {}
for key, langs in table.items():
    if not key.startswith("GAMEPLAY_"):
        continue
    if key.endswith("Description"):
        continue
    en = langs.get("en", "")
    zh = langs.get("zh-Hans", "")
    if not en or not zh:
        continue
    # Skip likely non-item entries: too long, contains punctuation
    if len(en) > 50 or len(zh) > 30:
        continue
    if any(c in en for c in ".?!\n") or any(c in zh for c in "。?!\n"):
        continue
    suffix = key[len("GAMEPLAY_"):]
    vanilla_items[suffix] = (en, zh, key)

print(f"Vanilla has ~{len(vanilla_items)} likely-item GAMEPLAY_ keys")

# Find what's in vanilla but not in DB
missing = []
for suffix, (en, zh, key) in vanilla_items.items():
    prefab = f"GEAR_{suffix}"
    if prefab not in db_prefabs:
        missing.append((prefab, en, zh))

# Find DB entries with no vanilla key
db_only = []
for prefab in sorted(db_prefabs):
    suffix = prefab[len("GEAR_"):]
    if suffix not in vanilla_items:
        db_only.append(prefab)

print(f"\n=== MISSING from DB (vanilla has, DB doesn't) — {len(missing)} ===")
# Sort by category-ish guesses for readability
missing.sort(key=lambda x: x[0])
for prefab, en, zh in missing:
    print(f"  {prefab:40s}  en={en!r:30s}  zh={zh!r}")

print(f"\n=== DB entries NOT matching any vanilla GAMEPLAY_ key — {len(db_only)} ===")
print("(may be mod-only items, DLC variants, or prefab name typos)")
for prefab in db_only[:50]:
    print(f"  {prefab}")
if len(db_only) > 50:
    print(f"  ... +{len(db_only)-50} more")
