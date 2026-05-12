"""Extract complete TLD localization table → JSON {key: {lang: text}}.

Layout (verified empirically):
  [MonoBehaviour header bytes ...]
  m_Name : aligned-string
  entry_count : i32
  for each entry:
    key : aligned-string
    value_count : i32  (= 15)
    values[15] : aligned-string × 15

Lang slot order (15 slots):
  0=en, 1=?, 2=de, 3=ru, 4=fr, 5=ja, 6=ko, 7=zh-Hans, 8=sv, 9=zh-Hant,
  10=tr, 11=no, 12=es, 13=pt-BR, 14=pt-PT
"""
import os, struct, json, UnityPy

ASSETS = r"D:\Steam\steamapps\common\TheLongDark\tld_Data\resources.assets"
TARGETS = [5825, 5827, 5828]
LANGS = ["en", "lang1", "de", "ru", "fr", "ja", "ko", "zh-Hans", "sv", "zh-Hant",
         "tr", "no", "es", "pt-BR", "pt-PT", "nl", "fi", "it", "pl", "uk", "lang20"]
NUM_LANGS = 21

env = UnityPy.load(ASSETS)
objs = {o.path_id: o for o in env.objects}

def read_aligned_string(buf, off):
    if off + 4 > len(buf): return None, off
    (length,) = struct.unpack_from("<i", buf, off); off += 4
    if length < 0 or length > 1024 * 16 or length > len(buf) - off: return None, off
    s_bytes = buf[off:off+length]
    off += length
    pad = (4 - (off % 4)) % 4
    off += pad
    try:
        return s_bytes.decode("utf-8"), off
    except UnicodeDecodeError:
        return None, off

def find_payload_start(buf):
    """The MonoBehaviour starts with header bytes then m_Name. Try common header
    sizes and pick the one where m_Name reads as a recognized SO name."""
    for hdr in [12, 16, 20, 24, 28, 32]:
        n, off = read_aligned_string(buf, hdr)
        if n is None: continue
        if n.startswith("Localization") or "OverflowData" in n or "LanguageData" in n:
            return off, n
    # Last resort: brute search 0..256
    for hdr in range(0, 256, 4):
        n, off = read_aligned_string(buf, hdr)
        if n and (n.startswith("Localization") or "LanguageData" in n):
            return off, n
    return None, None

def parse_table(buf, start):
    off = start
    if off + 4 > len(buf): return {}
    (entry_count,) = struct.unpack_from("<i", buf, off); off += 4
    if entry_count < 0 or entry_count > 100000: return {}
    out = {}
    for ei in range(entry_count):
        key, off2 = read_aligned_string(buf, off)
        if key is None: break
        if off2 + 4 > len(buf): break
        (vc,) = struct.unpack_from("<i", buf, off2); off2 += 4
        if vc != NUM_LANGS:
            # entry may use a different value count; try read anyway
            pass
        values = []
        cursor = off2
        for _ in range(NUM_LANGS):
            v, cursor = read_aligned_string(buf, cursor)
            if v is None: v = ""
            values.append(v)
        out[key] = {LANGS[i]: values[i] for i in range(NUM_LANGS)}
        off = cursor
    return out

table = {}
for pid in TARGETS:
    obj = objs.get(pid)
    if not obj: continue
    raw = obj.get_raw_data()
    payload_off, name = find_payload_start(raw)
    if payload_off is None:
        print(f"path_id={pid}: payload start not found"); continue
    print(f"path_id={pid} name={name!r} payload@{payload_off} size={len(raw)}")
    sub = parse_table(raw, payload_off)
    print(f"  parsed {len(sub)} entries")
    table.update(sub)

print(f"\nTOTAL: {len(table)} keys")
with open(r"D:\Github\tld--2.55\tools\tld_localization.json", "w", encoding="utf-8") as f:
    json.dump(table, f, ensure_ascii=False, indent=1)
print("wrote tld_localization.json")

ITEMS = ["GAMEPLAY_RifleBroken", "GAMEPLAY_Rifle_Trader", "GAMEPLAY_Bow_Bushcraft",
        "GAMEPLAY_CougarClawKnife", "GAMEPLAY_CougarClaw", "GAMEPLAY_CougarClaws",
        "GAMEPLAY_CougarHide", "GAMEPLAY_CougarHideDried",
        "GAMEPLAY_RawMeatCougar", "GAMEPLAY_CookedMeatCougar",
        "GAMEPLAY_CuredMeatPtarmigan", "GAMEPLAY_SaltedMeatPtarmigan",
        "GAMEPLAY_Flashlight_LongLasting", "GAMEPLAY_PtarmiganFeathers"]
print("\n=== Target items ===")
for k in ITEMS:
    e = table.get(k)
    if e: print(f"  {k:40s} en={e['en']!r:30s} zh={e['zh-Hans']!r}")
    else: print(f"  {k:40s} NOT FOUND")
