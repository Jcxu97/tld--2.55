"""Dump TLD localization MonoBehaviours from resources.assets.
TLD uses LanguageDataAsset with parallel arrays: m_LocalizationEntries[].m_Key + m_Localizations[].
We dump everything that contains GAMEPLAY_ or 美洲狮 markers.
"""
import os, json, UnityPy
ASSETS = r"D:\Steam\steamapps\common\TheLongDark\tld_Data\resources.assets"
OUT = r"D:\Github\tld--2.55\tools\localization_dump"
os.makedirs(OUT, exist_ok=True)

env = UnityPy.load(ASSETS)
print(f"loaded {len(env.objects)} objects", flush=True)

dumped = 0
for obj in env.objects:
    if obj.type.name != "MonoBehaviour": continue
    try:
        raw = obj.get_raw_data()
        # Filter: must have GAMEPLAY_ or Chinese loc marker
        if b"GAMEPLAY_" not in raw and "美洲狮".encode("utf-8") not in raw:
            continue
        # Try read_typetree (will give structured dict if MonoScript known)
        try:
            tree = obj.read_typetree()
            with open(os.path.join(OUT, f"mb_{obj.path_id}.json"), "w", encoding="utf-8") as f:
                json.dump(tree, f, ensure_ascii=False, indent=1, default=str)
            dumped += 1
        except Exception as e:
            # Fallback: dump raw bytes
            with open(os.path.join(OUT, f"mb_{obj.path_id}.bin"), "wb") as f:
                f.write(raw)
            print(f"raw-dumped path_id={obj.path_id} ({len(raw)} bytes): {e}")
    except Exception as e:
        pass
print(f"dumped {dumped} typed jsons")
