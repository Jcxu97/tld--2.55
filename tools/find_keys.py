"""Search all asset files in tld_Data for missing keys."""
import os, UnityPy
TLD_DATA = r"D:\Steam\steamapps\common\TheLongDark\tld_Data"
KEYS = [b"GAMEPLAY_RifleBroken", b"GAMEPLAY_Flashlight_LongLasting", b"FlashlightLongLasting"]

CANDIDATE = []
for root, _, files in os.walk(TLD_DATA):
    for f in files:
        if f.endswith((".assets", ".bundle")) or f in ("globalgamemanagers", "level0"):
            CANDIDATE.append(os.path.join(root, f))

print(f"scan {len(CANDIDATE)}")
seen = set()
for i, p in enumerate(CANDIDATE):
    fn = os.path.relpath(p, TLD_DATA)
    try:
        env = UnityPy.load(p)
        for obj in env.objects:
            try:
                raw = obj.get_raw_data()
                for k in KEYS:
                    if k in raw:
                        sig = (fn, k)
                        if sig not in seen:
                            seen.add(sig)
                            print(f"{fn} -> {k.decode()}  path_id={obj.path_id} type={obj.type.name}")
            except Exception: pass
    except Exception: pass
print("done")
