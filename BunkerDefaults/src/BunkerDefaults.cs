using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using MelonLoader;

[assembly: MelonInfo(typeof(BunkerDefaults.Mod), "BunkerDefaults", "1.0.3", "Claude")]
[assembly: MelonGame("Hinterland", "TheLongDark")]

namespace BunkerDefaults;

public class Mod : MelonMod
{
    private readonly HashSet<string> _processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private string _modDataDir;

    // v1.0.3 (2026-04-27) 按用户偏好重排 9 个 slot,只注入 5 个有真实坐标的。
    // 另外 4 个 slot(0/2/5/8)留空 — 用户走到 Airfield / Coastal / WhalingStation /
    // MountainPass 后按 FastTravel 的 Equals + 对应数字键覆盖
    //
    // 被删的 entry:
    //   Rural(坐标正常,但用户不要了)
    //   Marsh(原坐标 Y=-83 坏,用户自己走到已存)— 不在新 layout 里
    //   RiverValley(用户不要)
    //   MountainPass(原坐标 Y=207 掉崖,用户走到存)— 挪到 slot 8 占位
    private static readonly string DefaultFastTravelJson =
        "{\"Version\":\"0.2.0\",\"ReturnPoint\":null,\"Destinations\":{" +
        "\"1\":" + Dest("LakeRegion",          "神秘湖地堡",   1029.06, 91.99,   -52.52)   + "," +
        "\"3\":" + Dest("MountainTownRegion",  "山间小镇地堡", 1828.20, 444.39, 1771.27)   + "," +
        "\"4\":" + Dest("CanneryRegion",       "荒凉水湾地堡",  328.37, 344.50,  833.16)   + "," +
        "\"6\":" + Dest("BlackrockRegion",     "黑岩地区地堡",  705.04, 373.98,  816.38)   + "," +
        "\"7\":" + Dest("AshCanyonRegion",     "灰烬峡谷地堡",  -42.12, 172.95, -796.68)   +
        "}}";

    private static string Dest(string scene, string name, double x, double y, double z)
    {
        // Invariant culture to force '.' as decimal separator.
        var inv = System.Globalization.CultureInfo.InvariantCulture;
        return "{\"Region\":{\"Id\":\"" + scene + "\",\"NameLocalizationId\":\"SCENENAME_" + scene +
               "\",\"Name\":\"" + name + "\"}," +
               "\"Scene\":{\"Name\":\"" + scene + "\",\"Guid\":\"00000000000000000000000000000000\"," +
               "\"Path\":\"Assets/Scenes/_Zones/" + scene + "/" + scene + ".unity\",\"IsSubScene\":true}," +
               "\"Position\":{\"X\":" + x.ToString(inv) + ",\"Y\":" + y.ToString(inv) + ",\"Z\":" + z.ToString(inv) + "}," +
               "\"CameraPitch\":0.0,\"CameraYaw\":0.0," +
               "\"LastTransition\":{\"FromSceneId\":\"" + scene + "\",\"ToSceneId\":\"" + scene + "\"," +
               "\"ToSpawnPoint\":null,\"ToSpawnPointAudio\":null,\"RestorePlayerPosition\":false," +
               "\"LastOutdoorScene\":\"" + scene + "\"," +
               "\"LastOutdoorPosition\":{\"X\":" + x.ToString(inv) + ",\"Y\":" + y.ToString(inv) + ",\"Z\":" + z.ToString(inv) + "}," +
               "\"GameRandomSeed\":0,\"ForceNextSceneLoadTriggerScene\":null," +
               "\"SceneLocationLocIdOverride\":\"SCENENAME_" + scene + "\",\"Location\":null}}";
    }

    public override void OnInitializeMelon()
    {
        try
        {
            // Relative to game working directory (TLD install root when launched normally).
            _modDataDir = Path.GetFullPath(Path.Combine("Mods", "ModData"));
            LoggerInstance.Msg($"Watching {_modDataDir}");
            ScanAll();
        }
        catch (Exception e)
        {
            LoggerInstance.Error(e.ToString());
        }
    }

    public override void OnSceneWasInitialized(int buildIndex, string sceneName)
    {
        // Scan again at main menu / between scenes to catch saves created mid-session.
        ScanAll();
    }

    private void ScanAll()
    {
        if (string.IsNullOrEmpty(_modDataDir) || !Directory.Exists(_modDataDir)) return;

        foreach (var file in Directory.GetFiles(_modDataDir, "*.moddata"))
        {
            if (_processed.Contains(file)) continue;
            try
            {
                if (TryInject(file))
                    _processed.Add(file);
            }
            catch (Exception e)
            {
                LoggerInstance.Warning($"{Path.GetFileName(file)}: {e.Message}");
            }
        }
    }

    // Returns true on success (either injected or intentionally skipped).
    // Returns false for transient failures so we retry later.
    private bool TryInject(string path)
    {
        bool needsInject;

        using (var zip = ZipFile.OpenRead(path))
        {
            var ft = zip.GetEntry("FastTravel");
            if (ft == null)
            {
                needsInject = true;
            }
            else
            {
                string existing;
                using (var sr = new StreamReader(ft.Open(), Encoding.UTF8))
                    existing = sr.ReadToEnd();

                // Respect user customization: if Destinations has any real entry,
                // leave the file alone. Only inject if empty/missing.
                needsInject = HasNoDestinations(existing);
            }
        }

        if (!needsInject)
        {
            LoggerInstance.Msg($"{Path.GetFileName(path)}: FastTravel already populated, skipping");
            return true;
        }

        using (var zip = ZipFile.Open(path, ZipArchiveMode.Update))
        {
            var existing = zip.GetEntry("FastTravel");
            existing?.Delete();

            var entry = zip.CreateEntry("FastTravel", CompressionLevel.Optimal);
            using var sw = new StreamWriter(entry.Open(), new UTF8Encoding(false));
            sw.Write(DefaultFastTravelJson);
        }

        LoggerInstance.Msg($"{Path.GetFileName(path)}: injected 9 default bunker destinations");
        return true;
    }

    private static bool HasNoDestinations(string json)
    {
        // Quick check without a full JSON parser — good enough for the two shapes
        // FastTravel actually emits: "Destinations":{} or "Destinations":null.
        int idx = json.IndexOf("\"Destinations\"", StringComparison.Ordinal);
        if (idx < 0) return true;

        int colon = json.IndexOf(':', idx);
        if (colon < 0) return true;

        // Skip whitespace after colon
        int i = colon + 1;
        while (i < json.Length && char.IsWhiteSpace(json[i])) i++;
        if (i >= json.Length) return true;

        if (json[i] == 'n') return true;                    // null
        if (json[i] == '{' && i + 1 < json.Length)
        {
            // Skip whitespace inside braces
            int j = i + 1;
            while (j < json.Length && char.IsWhiteSpace(json[j])) j++;
            if (j < json.Length && json[j] == '}') return true;  // empty object
        }
        return false;
    }
}
