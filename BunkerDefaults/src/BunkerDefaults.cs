using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using MelonLoader;

[assembly: MelonInfo(typeof(BunkerDefaults.Mod), "BunkerDefaults", "1.0.1", "Claude")]
[assembly: MelonGame("Hinterland", "TheLongDark")]

namespace BunkerDefaults;

public class Mod : MelonMod
{
    private readonly HashSet<string> _processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private string _modDataDir;

    // Full 9-bunker FastTravel SaveModel, pre-serialized. Matches the format
    // produced by Pathoschild's FastTravel mod (Version 0.2.0, SaveModel shape).
    //
    // 2026-04-27: MountainPassRegion (dest 2) 和 MarshRegion (dest 3) 的 CheatEngine
    // 社区表坐标实测有问题 — MarshRegion Y=-83 传到地下,MountainPass 传到崖边空中
    // 一直下落。这两个 entry 从默认注入里移除,玩家需要自己走到地堡按 FastTravel
    // 保存键把真实坐标覆盖进 moddata
    private static readonly string DefaultFastTravelJson =
        "{\"Version\":\"0.2.0\",\"ReturnPoint\":null,\"Destinations\":{" +
        "\"0\":" + Dest("LakeRegion",          "神秘湖地堡",   1029.06, 91.99,   -52.52)   + "," +
        "\"1\":" + Dest("RuralRegion",         "怡人山谷地堡",  423.89, 177.93,  1458.51)  + "," +
        "\"4\":" + Dest("MountainTownRegion",  "山间小镇地堡", 1828.20, 444.39, 1771.27)   + "," +
        "\"5\":" + Dest("RiverValleyRegion",   "寂静河谷地堡",  363.44, 238.61,  375.49)   + "," +
        "\"6\":" + Dest("CanneryRegion",       "荒凉水湾地堡",  328.37, 344.50,  833.16)   + "," +
        "\"7\":" + Dest("AshCanyonRegion",     "灰烬峡谷地堡",  -42.12, 172.95, -796.68)   + "," +
        "\"8\":" + Dest("BlackrockRegion",     "黑岩地区地堡",  705.04, 373.98,  816.38)   +
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
