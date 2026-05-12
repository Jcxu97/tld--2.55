"""Probe TLD bundles to find which contains localization tables.
Looks for TextAsset/MonoBehaviour referencing GAMEPLAY_* keys."""
import os, sys, UnityPy
BUNDLE_DIR = r"D:\Steam\steamapps\common\TheLongDark\tld_Data\StreamingAssets\aa\StandaloneWindows64"
PROBE_KEYS = [b"GAMEPLAY_RifleBroken", b"GAMEPLAY_CougarClawKnife", b"GAMEPLAY_Rifle_Trader",
              b"Localization", b"LanguageDataAsset", b"GAMEPLAY_"]

hits = {}
files = sorted(os.listdir(BUNDLE_DIR))
print(f"Scanning {len(files)} bundles", flush=True)
for i, fn in enumerate(files):
    if not fn.endswith(".bundle"): continue
    p = os.path.join(BUNDLE_DIR, fn)
    try:
        env = UnityPy.load(p)
        for obj in env.objects:
            try:
                if obj.type.name in ("TextAsset", "MonoBehaviour"):
                    raw = obj.get_raw_data()
                    for k in PROBE_KEYS:
                        if k in raw:
                            hits.setdefault(fn, set()).add((obj.type.name, k.decode()))
            except Exception:
                pass
    except Exception as e:
        pass
    if i % 50 == 0:
        print(f"  [{i}/{len(files)}]", flush=True)

print("\n=== HITS ===")
for fn, found in hits.items():
    print(f"{fn}: {found}")
