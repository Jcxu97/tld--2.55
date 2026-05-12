"""Look at items in catalog but NOT in DB AND with no GAMEPLAY_ vanilla key.

These are usually:
- Texture/material subassets (_Mat, _Dif, _MAT)
- Story-only items (BearSpearStory, BunkerClue)
- DLC items with non-standard localization keys
- Animation/transparent variants

Filter out the obvious non-items, list the rest for human review.
"""
import re, json
DB = r"D:\Github\tld--2.55\TldHacks\src\ItemDatabase.cs"
LOC = r"D:\Github\tld--2.55\tools\tld_localization.json"
CATALOG = r"D:\Github\tld--2.55\tools\all_gear_prefabs.txt"

with open(LOC, encoding="utf-8") as f: table = json.load(f)
with open(DB, encoding="utf-8") as f: src = f.read()
with open(CATALOG, encoding="utf-8") as f:
    catalog = sorted({line.strip() for line in f if line.strip()})

PATTERN = re.compile(r'new ItemEntry\("(GEAR_[A-Za-z0-9_]+)"', re.U)
db_prefabs = set(PATTERN.findall(src))
catalog_set = set(catalog)
missing = sorted(catalog_set - db_prefabs)

# Tags that mean "not an item to spawn"
SUBASSET = re.compile(r"_(Mat|Dif|MAT|dif|mat|Transparent|ForAnim)$")

# For prefabs without exact GAMEPLAY_<suffix>, also check fuzzy matches
def find_fuzzy_loc(prefab):
    """Try alternative localization key patterns."""
    suffix = prefab[len("GEAR_"):]
    # Strip common variant suffixes
    for strip in ["_A","_B","_C","_D","_E","_Story","_Old","_old","_complete"]:
        if suffix.endswith(strip):
            base = suffix[:-len(strip)]
            for pattern in [f"GAMEPLAY_{base}", f"GAMEPLAY_{base}Description"]:
                e = table.get(pattern)
                if e and e.get("zh-Hans"): return pattern, e
    # Already-tried direct GAMEPLAY_suffix is none, so look for partial
    return None, None

interesting = []
for p in missing:
    if SUBASSET.search(p): continue
    suffix = p[len("GEAR_"):]
    direct = table.get(f"GAMEPLAY_{suffix}")
    if direct and direct.get("zh-Hans"):
        continue  # already handled by previous script
    # No direct GAMEPLAY_ — try fuzzy
    fkey, fe = find_fuzzy_loc(p)
    if fe:
        interesting.append((p, fkey, fe.get("en",""), fe.get("zh-Hans","")))
    else:
        # No localization at all → include w/ blank
        interesting.append((p, None, "", ""))

print(f"Catalog-missing without direct GAMEPLAY_ key: {len(interesting)}")
print("(After excluding *_Mat/_Dif/_MAT/_Transparent/_ForAnim)")
print()
print(f"=== Items with FUZZY-matched localization (likely real items) ===")
fuzz = [x for x in interesting if x[1]]
for p, fk, en, zh in fuzz:
    print(f"  {p:48s}  via {fk}  zh={zh}  en={en}")

print(f"\n=== Items with NO localization at all ({len([x for x in interesting if not x[1]])}) ===")
print("(usually unspawnable: triggers, story-bound, animation rigs, region-internal)")
no_loc = [x[0] for x in interesting if not x[1]]
for p in no_loc[:80]:
    print(f"  {p}")
if len(no_loc) > 80:
    print(f"  ... +{len(no_loc)-80} more")
