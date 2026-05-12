"""Compare ItemDatabase.cs prefabs against catalog.json's 848 real GEAR_ prefabs.

Reports:
1. Real prefabs (in catalog) NOT in DB → these are missing items
2. DB entries NOT in catalog → mod-only / typos / removed

For missing items, look up vanilla zh-Hans (GAMEPLAY_<suffix>) and en for context.
"""
import re, json
DB = r"D:\Github\tld--2.55\TldHacks\src\ItemDatabase.cs"
LOC = r"D:\Github\tld--2.55\tools\tld_localization.json"
CATALOG_PREFABS = r"D:\Github\tld--2.55\tools\all_gear_prefabs.txt"

with open(LOC, encoding="utf-8") as f:
    table = json.load(f)
with open(DB, encoding="utf-8") as f:
    src = f.read()
with open(CATALOG_PREFABS, encoding="utf-8") as f:
    catalog = sorted({line.strip() for line in f if line.strip()})

PATTERN = re.compile(r'new ItemEntry\("(GEAR_[A-Za-z0-9_]+)"', re.U)
db_prefabs = set(PATTERN.findall(src))
print(f"Catalog: {len(catalog)} prefabs")
print(f"DB     : {len(db_prefabs)} prefabs")

catalog_set = set(catalog)
missing_in_db = sorted(catalog_set - db_prefabs)
db_extra = sorted(db_prefabs - catalog_set)

# Categorize missing items by guessed type from prefab name
def categorize(name):
    s = name[len("GEAR_"):].lower()
    if any(k in s for k in ["rifle","revolver","gun","bow","arrow","stone","slingshot","pistol","crossbow"]): return "武器"
    if any(k in s for k in ["knife","hatchet","axe","saw","hammer","wrench","screwdriver","prybar","sharpen","tool","kit","whetstone"]): return "工具"
    if any(k in s for k in ["meat","fish","steak","cooked","raw","cured","salted","stew","soup","broth","cookie","pancake","bannock","pie","jerky"]): return "食物"
    if any(k in s for k in ["water","tea","coffee","soda","juice","milk","alcohol","rum","whisky","beer","drink"]): return "饮料"
    if any(k in s for k in ["pill","antiseptic","bandage","painkill","antibiotic","medical","first","aid","tonic","reishi","rosehip","syringe","insulin"]): return "医疗"
    if any(k in s for k in ["parka","coat","jacket","vest","sweater","shirt","pant","hat","tuque","cap","glove","mitten","scarf","balaclava","hood","sock","boot","shoe","mukluk","longjohn","cape","cowichan","accessory"]): return "衣物"
    if any(k in s for k in ["hide","gut","fur","feather","claw","tooth","horn","fat","leather","wood","stick","tinder","reclaimed","scrap","metal","cloth","cured","sapling","branch","limb","bark","stone","coal","chunk"]): return "材料"
    if any(k in s for k in ["bait","decoy","trap","lure","arrow","ammo","bullet","cartridge","shell","gunpowder","casing","powder","primer"]): return "狩猎"
    if any(k in s for k in ["lamp","lantern","torch","flare","flashlight","battery","kerosene","lampoil","fuel","candle","matches","flint","fire","striker"]): return "工具"
    if any(k in s for k in ["bedroll","sleeping","blanket","pillow","backpack","container","crate","box","package"]): return "工具"
    if any(k in s for k in ["page","note","book","map","journal","diary","letter","memo","photo","poster","cassette","record"]): return "装饰"
    return "?"

# Category counts of missing
from collections import Counter
cat_counts = Counter()
missing_with_meta = []
for prefab in missing_in_db:
    suffix = prefab[len("GEAR_"):]
    key = f"GAMEPLAY_{suffix}"
    e = table.get(key)
    en = e.get("en", "") if e else ""
    zh = e.get("zh-Hans", "") if e else ""
    cat = categorize(prefab)
    cat_counts[cat] += 1
    missing_with_meta.append((cat, prefab, en, zh))

print(f"\n=== {len(missing_in_db)} prefabs in CATALOG missing from DB ===")
print("Categorized counts:", dict(cat_counts.most_common()))

# Group by category for easier review
missing_with_meta.sort(key=lambda x: (x[0], x[1]))
current_cat = None
for cat, prefab, en, zh in missing_with_meta:
    if cat != current_cat:
        current_cat = cat
        print(f"\n--- {cat} ({cat_counts[cat]}) ---")
    en_short = (en[:30] + "..") if len(en) > 32 else en
    zh_short = (zh[:20] + "..") if len(zh) > 22 else zh
    print(f"  {prefab:50s}  zh={zh_short:24s}  en={en_short}")

print(f"\n\n=== {len(db_extra)} DB entries NOT in catalog (mod-only / typo / removed) ===")
for prefab in db_extra:
    print(f"  {prefab}")
