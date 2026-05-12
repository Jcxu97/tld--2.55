using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MotionTrackerLite;

internal static class TrackerConfig
{
    public static KeyCode ToggleKey = KeyCode.U;
    public static bool OnlyOutdoors = true;
    public static int DetectionRange = 100;
    public static float Scale = 1.0f;
    public static float Opacity = 0.7f;

    public static float AnimalIconScale = 1.0f;
    public static float GearIconScale = 1.0f;
    public static float PlayerDotOpacity = 0.85f;

    public static bool ShowWolves = true;
    public static bool ShowTimberwolves = true;
    public static bool ShowBears = true;
    public static bool ShowMoose = true;
    public static bool ShowCougars = true;
    public static bool ShowStags = true;
    public static bool ShowDoes = true;
    public static bool ShowRabbits = true;
    public static bool ShowPtarmigan = true;
    public static bool ShowArrows = true;
    public static bool ShowCoal = true;
    public static bool ShowRawFish = true;
    public static bool ShowLostAndFound = true;
    public static bool ShowSaltDeposit = true;
    public static bool ShowBeachLoot = true;
    public static bool ShowSpraypaint = true;

    private static string ConfigPath => Path.Combine(
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".",
        "MotionTrackerLite.json");

    public static void Load()
    {
        try
        {
            if (!File.Exists(ConfigPath)) { Save(); return; }
            var text = File.ReadAllText(ConfigPath);
            ToggleKey = ReadEnum(text, "ToggleKey", ToggleKey);
            OnlyOutdoors = ReadBool(text, "OnlyOutdoors", OnlyOutdoors);
            DetectionRange = ReadInt(text, "DetectionRange", DetectionRange);
            Scale = Mathf.Clamp(ReadFloat(text, "Scale", Scale), 0.5f, 3.0f);
            Opacity = Mathf.Clamp(ReadFloat(text, "Opacity", Opacity), 0.1f, 1.0f);
            AnimalIconScale = Mathf.Clamp(ReadFloat(text, "AnimalIconScale", AnimalIconScale), 0.5f, 3.0f);
            GearIconScale = Mathf.Clamp(ReadFloat(text, "GearIconScale", GearIconScale), 0.5f, 3.0f);
            PlayerDotOpacity = Mathf.Clamp(ReadFloat(text, "PlayerDotOpacity", PlayerDotOpacity), 0.3f, 1.0f);
            ShowWolves = ReadBool(text, "ShowWolves", ShowWolves);
            ShowTimberwolves = ReadBool(text, "ShowTimberwolves", ShowTimberwolves);
            ShowBears = ReadBool(text, "ShowBears", ShowBears);
            ShowMoose = ReadBool(text, "ShowMoose", ShowMoose);
            ShowCougars = ReadBool(text, "ShowCougars", ShowCougars);
            ShowStags = ReadBool(text, "ShowStags", ShowStags);
            ShowDoes = ReadBool(text, "ShowDoes", ShowDoes);
            ShowRabbits = ReadBool(text, "ShowRabbits", ShowRabbits);
            ShowPtarmigan = ReadBool(text, "ShowPtarmigan", ShowPtarmigan);
            ShowArrows = ReadBool(text, "ShowArrows", ShowArrows);
            ShowCoal = ReadBool(text, "ShowCoal", ShowCoal);
            ShowRawFish = ReadBool(text, "ShowRawFish", ShowRawFish);
            ShowLostAndFound = ReadBool(text, "ShowLostAndFound", ShowLostAndFound);
            ShowSaltDeposit = ReadBool(text, "ShowSaltDeposit", ShowSaltDeposit);
            ShowBeachLoot = ReadBool(text, "ShowBeachLoot", ShowBeachLoot);
            ShowSpraypaint = ReadBool(text, "ShowSpraypaint", ShowSpraypaint);
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[Config] {ex.Message}"); }
    }

    public static void Save()
    {
        try
        {
            var inv = CultureInfo.InvariantCulture;
            File.WriteAllText(ConfigPath,
                "{\n" +
                $"  \"ToggleKey\": \"{ToggleKey}\",\n" +
                $"  \"OnlyOutdoors\": {B(OnlyOutdoors)},\n" +
                $"  \"DetectionRange\": {DetectionRange},\n" +
                $"  \"Scale\": {Scale.ToString("F2", inv)},\n" +
                $"  \"Opacity\": {Opacity.ToString("F2", inv)},\n" +
                $"  \"AnimalIconScale\": {AnimalIconScale.ToString("F2", inv)},\n" +
                $"  \"GearIconScale\": {GearIconScale.ToString("F2", inv)},\n" +
                $"  \"PlayerDotOpacity\": {PlayerDotOpacity.ToString("F2", inv)},\n" +
                $"  \"ShowWolves\": {B(ShowWolves)},\n" +
                $"  \"ShowTimberwolves\": {B(ShowTimberwolves)},\n" +
                $"  \"ShowBears\": {B(ShowBears)},\n" +
                $"  \"ShowMoose\": {B(ShowMoose)},\n" +
                $"  \"ShowCougars\": {B(ShowCougars)},\n" +
                $"  \"ShowStags\": {B(ShowStags)},\n" +
                $"  \"ShowDoes\": {B(ShowDoes)},\n" +
                $"  \"ShowRabbits\": {B(ShowRabbits)},\n" +
                $"  \"ShowPtarmigan\": {B(ShowPtarmigan)},\n" +
                $"  \"ShowArrows\": {B(ShowArrows)},\n" +
                $"  \"ShowCoal\": {B(ShowCoal)},\n" +
                $"  \"ShowRawFish\": {B(ShowRawFish)},\n" +
                $"  \"ShowLostAndFound\": {B(ShowLostAndFound)},\n" +
                $"  \"ShowSaltDeposit\": {B(ShowSaltDeposit)},\n" +
                $"  \"ShowBeachLoot\": {B(ShowBeachLoot)},\n" +
                $"  \"ShowSpraypaint\": {B(ShowSpraypaint)}\n" +
                "}\n");
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[SaveConfig] {ex.Message}"); }
    }

    private static string B(bool v) => v ? "true" : "false";

    private static float ReadFloat(string text, string key, float def)
    {
        var m = Regex.Match(text, $"\"{key}\"\\s*:\\s*([\\d.]+)");
        return m.Success && float.TryParse(m.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float v) ? v : def;
    }

    private static int ReadInt(string text, string key, int def)
    {
        var m = Regex.Match(text, $"\"{key}\"\\s*:\\s*(\\d+)");
        return m.Success && int.TryParse(m.Groups[1].Value, out int v) ? v : def;
    }

    private static bool ReadBool(string text, string key, bool def)
    {
        var m = Regex.Match(text, $"\"{key}\"\\s*:\\s*(true|false)");
        return m.Success ? m.Groups[1].Value == "true" : def;
    }

    private static T ReadEnum<T>(string text, string key, T def) where T : struct, Enum
    {
        var m = Regex.Match(text, $"\"{key}\"\\s*:\\s*\"([A-Za-z0-9]+)\"");
        return m.Success && Enum.TryParse<T>(m.Groups[1].Value, true, out var v) ? v : def;
    }
}
