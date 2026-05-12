using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Il2Cpp;
using Il2CppInterop.Runtime;
using UnityEngine;

namespace MotionTrackerLite;

internal enum EntityCategory : byte { Animal, Gear, Spraypaint, Structure }
internal enum AnimalKind : byte { Wolf, Timberwolf, Bear, Moose, Cougar, Stag, Doe, Rabbit, Ptarmigan }

internal struct TrackedEntity
{
    public Transform Transform;
    public EntityCategory Category;
    public AnimalKind AnimalKind;
    public Vector2 RadarPos;
    public bool Visible;
}

internal static class Tracker
{
    public static bool Enabled;
    public static bool IsInGame;

    internal static readonly Dictionary<int, TrackedEntity> Entities = new(256);
    internal static readonly List<GameObject> SprayGOs = new();

    private static int _frame;
    private static Texture2D _bgTex;
    private static Texture2D _playerTex;
    private static Texture2D _fallbackTex;
    private static readonly Dictionary<string, Texture2D> _icons = new();

    private static readonly Dictionary<AnimalKind, string> AnimalIconMap = new()
    {
        { AnimalKind.Wolf, "Icon_Wolf_Brown" },
        { AnimalKind.Timberwolf, "Icon_Wolf_Gray" },
        { AnimalKind.Bear, "Icon_Bear_Grizly" },
        { AnimalKind.Moose, "Icon_Moose_Brown" },
        { AnimalKind.Cougar, "cougar" },
        { AnimalKind.Stag, "Icon_Deer_White" },
        { AnimalKind.Doe, "Icon_Deer_Female" },
        { AnimalKind.Rabbit, "Icon_Rabbit_Black" },
        { AnimalKind.Ptarmigan, "Icon_Ptarmigan" },
    };

    private static readonly Dictionary<AnimalKind, string> GearIconMap = new()
    {
        { AnimalKind.Wolf, "arrow" },
        { AnimalKind.Timberwolf, "coal" },
        { AnimalKind.Bear, "rawcohosalmon" },
    };

    private static readonly Dictionary<AnimalKind, string> StructureIconMap = new()
    {
        { AnimalKind.Wolf, "LostAndFound" },
        { AnimalKind.Timberwolf, "ico_GearItem__Salt" },
        { AnimalKind.Bear, "treasurechest" },
    };


    public static void Register(int id, Transform t, EntityCategory cat, AnimalKind kind = AnimalKind.Wolf)
    {
        if (t == null || Entities.ContainsKey(id)) return;
        Entities[id] = new TrackedEntity { Transform = t, Category = cat, AnimalKind = kind };
    }

    public static void Unregister(int id) => Entities.Remove(id);

    public static void Clear()
    {
        Entities.Clear();
        foreach (var go in SprayGOs)
        {
            if (go != null) UnityEngine.Object.Destroy(go);
        }
        SprayGOs.Clear();
    }

    public static bool ShouldShow(EntityCategory cat, AnimalKind kind)
    {
        if (cat == EntityCategory.Spraypaint) return TrackerConfig.ShowSpraypaint;
        if (cat == EntityCategory.Gear)
        {
            return kind switch
            {
                AnimalKind.Wolf => TrackerConfig.ShowArrows,
                AnimalKind.Timberwolf => TrackerConfig.ShowCoal,
                AnimalKind.Bear => TrackerConfig.ShowRawFish,
                _ => true
            };
        }
        if (cat == EntityCategory.Structure)
        {
            return kind switch
            {
                AnimalKind.Wolf => TrackerConfig.ShowLostAndFound,
                AnimalKind.Timberwolf => TrackerConfig.ShowSaltDeposit,
                AnimalKind.Bear => TrackerConfig.ShowBeachLoot,
                _ => true
            };
        }
        return kind switch
        {
            AnimalKind.Wolf => TrackerConfig.ShowWolves,
            AnimalKind.Timberwolf => TrackerConfig.ShowTimberwolves,
            AnimalKind.Bear => TrackerConfig.ShowBears,
            AnimalKind.Moose => TrackerConfig.ShowMoose,
            AnimalKind.Cougar => TrackerConfig.ShowCougars,
            AnimalKind.Stag => TrackerConfig.ShowStags,
            AnimalKind.Doe => TrackerConfig.ShowDoes,
            AnimalKind.Rabbit => TrackerConfig.ShowRabbits,
            AnimalKind.Ptarmigan => TrackerConfig.ShowPtarmigan,
            _ => true
        };
    }

    public static void ScanExisting()
    {
        try
        {
            var aiType = Il2CppType.From(typeof(BaseAi));
            var all = UnityEngine.Object.FindObjectsOfType(aiType);
            if (all != null)
            {
                foreach (var obj in all)
                {
                    var ai = obj?.TryCast<BaseAi>();
                    if (ai == null) continue;
                    if ((int)ai.m_CurrentMode == 2) continue;
                    TrackerPatches.RegisterAnimal(ai);
                }
            }
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[ScanAi] {ex.Message}"); }

        try
        {
            var containerType = Il2CppType.From(typeof(Container));
            var containers = UnityEngine.Object.FindObjectsOfType(containerType);
            if (containers != null)
            {
                foreach (var obj in containers)
                {
                    var c = obj?.TryCast<Container>();
                    if (c == null) continue;
                    var go = ((Component)c).gameObject;
                    if (go == null) continue;
                    string name = ((UnityEngine.Object)go).name ?? "";
                    if (!name.Contains("InaccessibleGear", StringComparison.OrdinalIgnoreCase)) continue;
                    Register(go.GetInstanceID(), go.transform, EntityCategory.Structure, AnimalKind.Wolf);
                }
            }
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[ScanContainers] {ex.Message}"); }

        try
        {
            var harvType = Il2CppType.From(typeof(Harvestable));
            var harvs = UnityEngine.Object.FindObjectsOfType(harvType);
            if (harvs != null)
            {
                foreach (var obj in harvs)
                {
                    var h = obj?.TryCast<Harvestable>();
                    if (h == null) continue;
                    var go = ((Component)h).gameObject;
                    if (go == null) continue;
                    string name = ((UnityEngine.Object)go).name ?? "";
                    if (!name.Contains("SaltDeposit", StringComparison.OrdinalIgnoreCase)) continue;
                    Register(go.GetInstanceID(), go.transform, EntityCategory.Structure, AnimalKind.Timberwolf);
                }
            }
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[ScanHarvestables] {ex.Message}"); }

        ScanByTypeName("Il2Cpp.BeachcombingSpawner", EntityCategory.Structure, AnimalKind.Bear);
    }

    private static void ScanByTypeName(string typeName, EntityCategory cat, AnimalKind kind)
    {
        try
        {
            var type = HarmonyLib.AccessTools.TypeByName(typeName);
            if (type == null) return;
            var il2cppType = Il2CppType.From(type);
            if (il2cppType == null) return;
            var found = UnityEngine.Object.FindObjectsOfType(il2cppType);
            if (found == null) return;
            foreach (var obj in found)
            {
                var comp = obj?.TryCast<Component>();
                if (comp == null) continue;
                var go = comp.gameObject;
                if (go == null) continue;
                Register(go.GetInstanceID(), go.transform, cat, kind);
            }
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[Scan:{typeName}] {ex.Message}"); }
    }

    public static void Tick()
    {
        if (!Enabled || !IsInGame) return;

        if (TrackerConfig.OnlyOutdoors)
        {
            var weather = GameManager.GetWeatherComponent();
            if (weather != null && weather.IsIndoorEnvironment()) return;
        }

        if (++_frame % 5 != 0) return;

        var player = GameManager.GetVpFPSPlayer();
        if (player == null) return;
        var playerPos = ((Component)player).transform.position;
        var forward = Vector3.ProjectOnPlane(((Component)player).transform.forward, Vector3.up).normalized;
        float angle = Mathf.Atan2(forward.x, forward.z);

        float range = TrackerConfig.DetectionRange;
        float rangeSq = range * range;

        var toRemove = new List<int>();
        var keys = new List<int>(Entities.Keys);
        foreach (var id in keys)
        {
            var ent = Entities[id];
            if (ent.Transform == null) { toRemove.Add(id); continue; }

            var diff = ent.Transform.position - playerPos;
            float distSq = diff.x * diff.x + diff.z * diff.z;

            if (distSq > rangeSq)
            {
                ent.Visible = false;
            }
            else
            {
                float nx = diff.x / range;
                float nz = diff.z / range;
                float rx = nx * Mathf.Cos(angle) - nz * Mathf.Sin(angle);
                float ry = nx * Mathf.Sin(angle) + nz * Mathf.Cos(angle);
                ent.RadarPos = new Vector2(rx, -ry);
                ent.Visible = true;
            }
            Entities[id] = ent;
        }
        foreach (var id in toRemove) Entities.Remove(id);
    }

    private static float DpiScale => Screen.height / 1080f;

    public static void Draw()
    {
        if (Event.current.type != EventType.Repaint) return;
        if (!Enabled || !IsInGame) return;
        if (TrackerConfig.OnlyOutdoors)
        {
            var weather = GameManager.GetWeatherComponent();
            if (weather != null && weather.IsIndoorEnvironment()) return;
        }

        EnsureTextures();

        float dpi = DpiScale;
        float scale = TrackerConfig.Scale * dpi;
        float size = 160f * scale;
        float margin = 20f * dpi;
        float cx = margin + size / 2f;
        float cy = margin + size / 2f;

        var bgColor = new Color(0f, 0f, 0f, TrackerConfig.Opacity * 0.6f);
        GUI.color = bgColor;
        GUI.DrawTexture(new Rect(cx - size / 2f, cy - size / 2f, size, size), _bgTex);
        GUI.color = Color.white;

        float halfSize = size / 2f;
        float iconSize = 10f * scale;

        GUI.color = new Color(1f, 1f, 1f, TrackerConfig.PlayerDotOpacity);
        float ps = iconSize * 0.45f;
        GUI.DrawTexture(new Rect(cx - ps / 2f, cy - ps / 2f, ps, ps), _playerTex);

        foreach (var kv in Entities)
        {
            var ent = kv.Value;
            if (!ent.Visible) continue;
            if (!ShouldShow(ent.Category, ent.AnimalKind)) continue;

            float px = cx + ent.RadarPos.x * halfSize * 0.9f;
            float py = cy + ent.RadarPos.y * halfSize * 0.9f;

            float ds;
            Texture2D icon = GetIcon(ent.Category, ent.AnimalKind);
            if (ent.Category == EntityCategory.Animal)
                ds = iconSize * 1.4f * TrackerConfig.AnimalIconScale;
            else
                ds = iconSize * 1.1f * TrackerConfig.GearIconScale;

            GUI.color = new Color(1f, 1f, 1f, TrackerConfig.Opacity);
            GUI.DrawTexture(new Rect(px - ds / 2f, py - ds / 2f, ds, ds), icon);
        }
        GUI.color = Color.white;
    }

    private static Texture2D GetIcon(EntityCategory cat, AnimalKind kind)
    {
        string key = null;
        if (cat == EntityCategory.Animal)
            AnimalIconMap.TryGetValue(kind, out key);
        else if (cat == EntityCategory.Gear)
            GearIconMap.TryGetValue(kind, out key);
        else if (cat == EntityCategory.Structure)
            StructureIconMap.TryGetValue(kind, out key);
        else if (cat == EntityCategory.Spraypaint)
            key = "ico_SprayPaint_Direction";

        if (key != null && _icons.TryGetValue(key, out var tex))
            return tex;
        return _fallbackTex;
    }


    private static void EnsureTextures()
    {
        if (_bgTex != null) return;

        var dllDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".";
        var iconDir = Path.Combine(dllDir, "MotionTrackerLite_icons");

        _bgTex = LoadPng(Path.Combine(iconDir, "background.png"));
        if (_bgTex == null)
        {
            _bgTex = MakeCircleTex(128);
        }

        _playerTex = MakeCircleTex(64);
        _fallbackTex = MakeCircleTex(32);

        if (Directory.Exists(iconDir))
        {
            foreach (var file in Directory.GetFiles(iconDir, "*.png"))
            {
                string name = Path.GetFileNameWithoutExtension(file);
                if (name == "background") continue;
                var tex = LoadPng(file);
                if (tex != null) _icons[name] = tex;
            }
            ModMain.Log?.Msg($"[Icons] Loaded {_icons.Count} icons from {iconDir}");
        }
        else
        {
            ModMain.Log?.Warning($"[Icons] Folder not found: {iconDir}");
        }
    }

    private static Texture2D LoadPng(string path)
    {
        if (!File.Exists(path)) return null;
        try
        {
            var data = File.ReadAllBytes(path);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.hideFlags = HideFlags.HideAndDontSave;
            tex.filterMode = FilterMode.Bilinear;
            if (ImageConversion.LoadImage(tex, data))
                return tex;
            UnityEngine.Object.Destroy(tex);
        }
        catch { }
        return null;
    }

    private static Texture2D MakeCircleTex(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.hideFlags = HideFlags.HideAndDontSave;
        tex.filterMode = FilterMode.Bilinear;
        float center = size / 2f;
        float r = center - 2f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dist = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center));
                float a = Mathf.Clamp01(1f - (dist - r) / 2f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        tex.Apply();
        return tex;
    }

    private static Texture2D MakePlayerArrow(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.hideFlags = HideFlags.HideAndDontSave;
        tex.filterMode = FilterMode.Bilinear;
        float s = size;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float u = (x + 0.5f) / s;
                float v = 1f - (y + 0.5f) / s;
                float halfW = v * 0.35f;
                float dist = Mathf.Abs(u - 0.5f) - halfW;
                float d = Mathf.Max(dist, -v + 0.08f);
                float a = Mathf.Clamp01(1f - d * s / 1.5f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        tex.Apply();
        return tex;
    }
}
