"""Filter the 466 'missing' to only those that are REAL items (have vanilla zh-Hans).

Excludes:
- *_Mat, *_Dif, *_MAT, *_dif (material/texture variants)
- *_Old, *_old, *_Transparent, *_ForAnim (variants)
- Items with no zh-Hans (internal placeholders)
- *_A, *_B, *_C single-letter suffix on _Acorn etc (placement variants)
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

EXCLUDE_SUFFIX = re.compile(r"_(Mat|Dif|MAT|dif|mat|Old|old|Transparent|ForAnim|complete)$")
EXCLUDE_LETTER_SUFFIX = re.compile(r"_[A-Z]$")  # _A _B _C etc

def is_meaningful(prefab):
    if EXCLUDE_SUFFIX.search(prefab): return False
    suffix = prefab[len("GEAR_"):]
    key = f"GAMEPLAY_{suffix}"
    e = table.get(key)
    if not e: return False
    zh = e.get("zh-Hans", "")
    en = e.get("en", "")
    if not zh or not en: return False
    # Some have zh only (UI strings) — but if both en and zh present, item-like
    return True

def categorize(name, en, zh):
    s = name[len("GEAR_"):].lower()
    en_l = en.lower()
    if any(k in s for k in ["rifle","revolver","gun","bow","arrow","stone","slingshot","pistol","crossbow","spear","bear_spear","bearspear"]): return "武器"
    if any(k in s for k in ["knife","hatchet","axe","saw","hammer","wrench","prybar","sharpen","whetstone","cleaningkit","fishingtackle","gunsmith","forge","kit"]): return "工具"
    if any(k in s for k in ["meat","fish","steak","cooked","raw","cured","salted","stew","soup","broth","cookie","pancake","bannock","pie","jerky","stout","ribs"]): return "食物"
    if any(k in s for k in ["water","tea","coffee","soda","juice","milk","alcohol","rum","whisky","beer","drink","cocoa"]): return "饮料"
    if any(k in s for k in ["pill","antiseptic","bandage","painkill","antibiotic","medical","first","aid","tonic","reishi","rosehip","syringe","insulin","stim"]): return "医疗"
    if any(k in s for k in ["parka","coat","jacket","vest","sweater","shirt","pant","hat","tuque","cap","glove","mitten","scarf","balaclava","hood","sock","boot","shoe","mukluk","longjohn","cape","cowichan","accessory","mackinaw","bomber","earmuff"]): return "衣物"
    if any(k in s for k in ["hide","gut","fur","feather","claw","tooth","horn","fat","leather","cloth","sapling","branch","limb","bark","stone","coal","tinder","reclaimed","scrap","wood","stick"]): return "材料"
    if any(k in s for k in ["bait","decoy","trap","lure","ammo","bullet","cartridge","shell","gunpowder","casing","powder","primer"]): return "狩猎"
    if any(k in s for k in ["lamp","lantern","torch","flare","flashlight","battery","kerosene","lampoil","fuel","candle","matches","flint","fire","striker","bedroll","sleeping","backpack"]): return "工具"
    if any(k in s for k in ["page","note","book","map","journal","diary","letter","memo","photo","poster","cassette","record","key","clue","clueboard"]): return "装饰"
    return "?"

real = []
filtered_out = []
for p in missing:
    if is_meaningful(p):
        suffix = p[len("GEAR_"):]
        e = table[f"GAMEPLAY_{suffix}"]
        en = e["en"]
        zh = e["zh-Hans"]
        cat = categorize(p, en, zh)
        real.append((cat, p, en, zh))
    else:
        filtered_out.append(p)

print(f"After filter: {len(real)} REAL missing items (had {len(missing)} raw)")

from collections import Counter
counts = Counter(x[0] for x in real)
print(f"By category: {dict(counts.most_common())}")

real.sort(key=lambda x: (x[0], x[1]))
current = None
for cat, p, en, zh in real:
    if cat != current:
        current = cat
        print(f"\n--- {cat} ({counts[cat]}) ---")
    print(f"  {p:48s}  {zh:18s}  ({en})")

print(f"\n[filtered as non-items: {len(filtered_out)} variants/no-loc prefabs]")
