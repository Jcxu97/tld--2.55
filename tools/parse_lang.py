"""Parse the LanguageDataAsset MonoBehaviours by raw byte structure.
Unity serialized layout: PPtr<MonoScript>(12B: file_id i32 + path_id i64) + m_Name(aligned-string) + payload.

The payload for TLD's LanguageDataAsset (educated guess): an array of localization rows.
We'll just extract ALL aligned-strings from the payload — the keys (GAMEPLAY_*) and
values (localized text) come out together in order, and we can pair them by alternation.
"""
import os, struct, UnityPy
ASSETS = r"D:\Steam\steamapps\common\TheLongDark\tld_Data\resources.assets"
TARGETS = [5825, 5828]  # the two MonoBehaviours that contain Chinese item names

env = UnityPy.load(ASSETS)
objs = {o.path_id: o for o in env.objects}

def read_aligned_string(buf, off):
    if off + 4 > len(buf): return None, off
    (length,) = struct.unpack_from("<i", buf, off); off += 4
    if length < 0 or length > len(buf) - off: return None, off
    s = buf[off:off+length]
    off += length
    # 4-byte align
    pad = (4 - (off % 4)) % 4
    off += pad
    try:
        return s.decode("utf-8"), off
    except UnicodeDecodeError:
        return None, off

def extract_strings(buf):
    """Walk through buffer and yield (offset, string) for every plausible aligned-string."""
    out = []
    off = 0
    while off + 4 <= len(buf):
        (length,) = struct.unpack_from("<i", buf, off)
        if 0 <= length <= 8192 and off + 4 + length <= len(buf):
            blob = buf[off+4:off+4+length]
            try:
                s = blob.decode("utf-8")
                # Heuristic: only keep strings that look like localized content (key or non-trivial text)
                if length == 0 or any(0x20 <= b < 0x7F for b in blob[:1]) or length >= 2:
                    if all(b >= 0x09 for b in blob):
                        out.append((off, length, s))
            except UnicodeDecodeError:
                pass
        off += 1
    return out

for pid in TARGETS:
    obj = objs.get(pid)
    if not obj:
        print(f"path_id {pid} not found"); continue
    raw = obj.get_raw_data()
    print(f"\n=== path_id={pid} ({len(raw)} bytes) ===")
    print("first 64 bytes hex:", raw[:64].hex())
    # PPtr (12) + name(aligned) skip
    off = 12
    name, off = read_aligned_string(raw, off)
    print(f"name={name!r} payload_off={off}")
    # Dump first 50 strings from payload
    payload = raw[off:]
    strings = extract_strings(payload)
    print(f"found {len(strings)} candidate strings")
    # Save full extract
    with open(f"strings_{pid}.txt", "w", encoding="utf-8") as f:
        for o,l,s in strings:
            f.write(f"@{o:6d} L{l:4d}: {s}\n")
    # Print first 20 + a slice around RifleBroken
    for o,l,s in strings[:20]:
        print(f"  @{o:6d} L{l:4d}: {s!r}")
