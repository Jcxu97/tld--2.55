"""Scan everywhere — bundles + tld_Data root files — for Chinese loc strings."""
import os, UnityPy
TLD_DATA = r"D:\Steam\steamapps\common\TheLongDark\tld_Data"
needles_text = ["美洲狮", "损毁", "猎枪", "RifleBroken", "LanguageDataAsset", "GAMEPLAY_RifleBroken"]
NEEDLES = []
for s in needles_text:
    NEEDLES.append((s, s.encode("utf-8")))
    NEEDLES.append((s+"[u16]", s.encode("utf-16-le")))

CANDIDATE_FILES = []
for root, _, files in os.walk(TLD_DATA):
    for f in files:
        if f.endswith((".assets", ".bundle")) or f in ("globalgamemanagers", "level0"):
            CANDIDATE_FILES.append(os.path.join(root, f))

print(f"scan {len(CANDIDATE_FILES)}", flush=True)
seen = set()
for i, p in enumerate(CANDIDATE_FILES):
    fn = os.path.relpath(p, TLD_DATA)
    try:
        env = UnityPy.load(p)
        for obj in env.objects:
            try:
                raw = obj.get_raw_data()
                for label, n in NEEDLES:
                    if n in raw:
                        sz = os.path.getsize(p)/1024/1024
                        key = (fn, label)
                        if key not in seen:
                            seen.add(key)
                            print(f"{fn} ({sz:.1f}MB) {obj.type.name} path_id={obj.path_id} -> {label}")
            except Exception: pass
    except Exception: pass
    if i % 50 == 0: print(f"  [{i}/{len(CANDIDATE_FILES)}]", flush=True)
print("done")
