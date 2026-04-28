using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using Il2CppInterop.Runtime;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(ModSettingsQuickNav.ModMain), "ModSettingsQuickNav", "1.2.1", "user")]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: MelonAdditionalDependencies("ModSettings")]

namespace ModSettingsQuickNav;

// v1.2.1:F2 手动切换(用户改 backquote) + 保留打开 ModSettings tab 自动显示
// v1.2.0:自动显示 + +/- 缩放 + 存 json
// v1.1.0:去 Harmony,纯轮询 FindObjectsOfType(ModSettingsGUI)
public class ModMain : MelonMod
{
    internal static MelonLogger.Instance Log;

    public override void OnInitializeMelon()
    {
        Log = LoggerInstance;
        NavOverlay.LoadConfig();
        Log.Msg($"ModSettingsQuickNav v1.2.1 loaded — auto-show on ModSettings tab, {NavOverlay.Hotkey} toggles, scale={NavOverlay.Scale:F1}");
    }

    private int _poll = 0;
    private bool _wasActive = false;

    public override void OnUpdate()
    {
        if (++_poll >= 30)
        {
            _poll = 0;
            NavOverlay.RefreshPanelState();
        }

        // ModSettings tab 激活时,热键(默认 F2)切换浮层显隐
        if (NavOverlay.IsActive && NavOverlay.Hotkey != KeyCode.None && Input.GetKeyDown(NavOverlay.Hotkey))
            NavOverlay.Hidden = !NavOverlay.Hidden;

        // 边沿触发:ModSettings tab 从关到开时重置 Hidden = false(让浮层自动出现)
        if (NavOverlay.IsActive && !_wasActive)
            NavOverlay.Hidden = false;
        _wasActive = NavOverlay.IsActive;
    }

    public override void OnGUI()
    {
        NavOverlay.Draw();
    }
}

internal static class NavOverlay
{
    public static bool IsActive;
    public static bool Hidden;           // 用户按热键手动隐藏;下次进 ModSettings tab 会重置
    public static KeyCode Hotkey = KeyCode.F2;

    private static object _gui;
    private static Type _guiType;
    private static Type _menuType;
    private static MethodInfo _selectMod;
    private static FieldInfo _modTabsField;
    private static FieldInfo _settingsByNameField;
    private static bool _typesResolved = false;

    private static Vector2 _scroll;
    private static Rect _win = new Rect(20f, 80f, 320f, 560f);
    private const int WinId = 0x35514E41;

    private static string _filter = "";
    private static readonly char[] AlphaLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

    // —— UI 缩放(4K 显示器友好)——
    public static float Scale = 1.5f;  // default reasonable for 2K+;4K 可调到 2.0+
    private const float ScaleMin = 0.8f;
    private const float ScaleMax = 3.0f;
    private const float ScaleStep = 0.1f;
    private static string ConfigPath => Path.Combine(
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".",
        "ModSettingsQuickNav.json");

    public static void LoadConfig()
    {
        try
        {
            if (!File.Exists(ConfigPath)) { SaveConfig(); return; }
            var text = File.ReadAllText(ConfigPath);
            var mScale = System.Text.RegularExpressions.Regex.Match(text, "\"Scale\"\\s*:\\s*(\\d+(?:\\.\\d+)?)");
            if (mScale.Success && float.TryParse(mScale.Groups[1].Value, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float s))
                Scale = Mathf.Clamp(s, ScaleMin, ScaleMax);

            var mKey = System.Text.RegularExpressions.Regex.Match(text, "\"Hotkey\"\\s*:\\s*\"([A-Za-z0-9]+)\"");
            if (mKey.Success && Enum.TryParse<KeyCode>(mKey.Groups[1].Value, true, out var k))
                Hotkey = k;
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[LoadConfig] {ex.Message}"); }
    }

    public static void SaveConfig()
    {
        try
        {
            File.WriteAllText(ConfigPath,
                $"{{\n" +
                $"  \"Scale\": {Scale.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)},\n" +
                $"  \"Hotkey\": \"{Hotkey}\"\n" +
                $"}}\n");
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[SaveConfig] {ex.Message}"); }
    }

    private static void ResolveTypes()
    {
        if (_typesResolved) return;
        try
        {
            _guiType  = AccessTools.TypeByName("ModSettings.ModSettingsGUI");
            _menuType = AccessTools.TypeByName("ModSettings.ModSettingsMenu");
            if (_guiType != null)
            {
                _selectMod    = _guiType.GetMethod("SelectMod", BindingFlags.Instance | BindingFlags.NonPublic);
                _modTabsField = _guiType.GetField("modTabs",    BindingFlags.Instance | BindingFlags.NonPublic);
            }
            if (_menuType != null)
                _settingsByNameField = _menuType.GetField("settingsByModName", BindingFlags.Static | BindingFlags.NonPublic);
            _typesResolved = true;
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[ResolveTypes] {ex.Message}"); _typesResolved = true; }
    }

    public static void RefreshPanelState()
    {
        try
        {
            ResolveTypes();
            if (_guiType == null) { IsActive = false; _gui = null; return; }
            var all = UnityEngine.Object.FindObjectsOfType(Il2CppType.From(_guiType));
            _gui = null;
            IsActive = false;
            if (all == null) return;
            foreach (var obj in all)
            {
                if (obj == null) continue;
                var comp = obj as Component;
                if (comp == null) continue;
                if (!comp.gameObject.activeInHierarchy) continue;
                _gui = obj;
                IsActive = true;
                break;
            }
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[Refresh] {ex.Message}"); IsActive = false; }
    }

    private static Rect R(float x, float y, float w, float h)
        => new Rect(x * Scale, y * Scale, w * Scale, h * Scale);

    public static void Draw()
    {
        if (!IsActive || Hidden) return;

        // 窗口大小随 Scale 变
        _win.width  = 320f * Scale;
        _win.height = 600f * Scale;
        _win.x = Mathf.Clamp(_win.x, 0f, Screen.width  - _win.width);
        _win.y = Mathf.Clamp(_win.y, 0f, Screen.height - 60f);

        // 字体跟缩放(IMGUI 全局 skin,OnGUI 结束会恢复)
        ApplyFontScale();

        _win = GUI.Window(WinId, _win, (GUI.WindowFunction)DrawWindow, "Mod 快速跳转");
    }

    private static void ApplyFontScale()
    {
        try
        {
            int fs = Mathf.Max(10, Mathf.RoundToInt(14f * Scale));
            var sk = GUI.skin;
            if (sk == null) return;
            if (sk.label  != null) sk.label.fontSize  = fs;
            if (sk.button != null) sk.button.fontSize = fs;
            if (sk.box    != null) sk.box.fontSize    = fs;
            if (sk.window != null) sk.window.fontSize = fs;
        }
        catch { }
    }

    private static void DrawWindow(int id)
    {
        try
        {
            float w = _win.width / Scale;  // 内部用逻辑坐标,R() 负责乘 Scale

            // —— 顶栏:× 关闭 + 缩放 +/- ——
            // × 关闭整次会话,按热键 (Hotkey,默认 F2) 或离开再进 ModSettings tab 会再现
            if (GUI.Button(new Rect((w - 30f) * Scale, 4f * Scale, 24f * Scale, 18f * Scale), "×"))
            {
                Hidden = true;
                return;
            }
            // + / - 按钮:调 UI scale
            if (GUI.Button(new Rect((w - 110f) * Scale, 4f * Scale, 24f * Scale, 18f * Scale), "-"))
            { Scale = Mathf.Clamp(Scale - ScaleStep, ScaleMin, ScaleMax); SaveConfig(); }
            GUI.Label(new Rect((w - 82f) * Scale, 4f * Scale, 44f * Scale, 18f * Scale), $"{Scale:F1}x");
            if (GUI.Button(new Rect((w - 38f) * Scale, 4f * Scale, 24f * Scale, 18f * Scale), "+"))
            { Scale = Mathf.Clamp(Scale + ScaleStep, ScaleMin, ScaleMax); SaveConfig(); }

            var modNames = GetModNames();
            if (modNames == null || modNames.Count == 0)
            {
                GUI.Label(R(12f, 30f, 280f, 22f), "(mod 列表取不到,重开 options 面板试试)");
                GUI.DragWindow(new Rect(0, 0, 10000, 24f * Scale));
                return;
            }

            // —— A-Z 跳段 13×2 ——
            float y = 28f;
            float bw = 22f, bh = 18f;
            for (int i = 0; i < AlphaLetters.Length; i++)
            {
                int col = i % 13;
                int row = i / 13;
                if (GUI.Button(R(8f + col * (bw + 1f), y + row * (bh + 2f), bw, bh),
                               AlphaLetters[i].ToString()))
                {
                    _filter = AlphaLetters[i].ToString();
                    _scroll = Vector2.zero;
                }
            }
            y += 2 * (bh + 2f) + 2f;

            GUI.Label(R(8f, y, 200f, 20f),
                string.IsNullOrEmpty(_filter) ? "筛选:(全部)" : $"筛选:首字母 = {_filter}");
            if (GUI.Button(R(240f, y - 2f, 60f, 22f), "清除"))
            {
                _filter = "";
                _scroll = Vector2.zero;
            }
            y += 26f;

            var displayList = new List<string>(modNames.Count);
            foreach (var n in modNames)
                if (string.IsNullOrEmpty(_filter) ||
                    n.StartsWith(_filter, StringComparison.OrdinalIgnoreCase))
                    displayList.Add(n);

            GUI.Label(R(8f, y, 300f, 18f), $"共 {modNames.Count} 个 mod,当前显示 {displayList.Count}");
            y += 20f;

            var listRect = R(8f, y, w - 16f, (_win.height / Scale) - y - 12f);
            var contentH = displayList.Count * 24f * Scale + 4f;
            var contentRect = new Rect(0, 0, listRect.width - 18f * Scale, contentH);
            _scroll = GUI.BeginScrollView(listRect, _scroll, contentRect, false, true);

            for (int i = 0; i < displayList.Count; i++)
            {
                var btnRect = new Rect(0f, i * 24f * Scale, contentRect.width, 22f * Scale);
                if (GUI.Button(btnRect, displayList[i]))
                {
                    JumpTo(displayList[i]);
                    // 不自动关闭,方便连续跳
                }
            }
            GUI.EndScrollView();

            GUI.DragWindow(new Rect(0, 0, 10000, 24f * Scale));
        }
        catch (Exception ex) { ModMain.Log?.Error($"[DrawWindow] {ex}"); }
    }

    private static List<string> GetModNames()
    {
        try
        {
            ResolveTypes();
            if (_gui != null && _modTabsField != null)
            {
                var modTabs = _modTabsField.GetValue(_gui);
                if (modTabs != null)
                {
                    var keys = modTabs.GetType().GetProperty("Keys")?.GetValue(modTabs) as IEnumerable;
                    if (keys != null)
                    {
                        var list = new List<string>();
                        foreach (var k in keys) if (k is string s) list.Add(s);
                        list.Sort(StringComparer.OrdinalIgnoreCase);
                        if (list.Count > 0) return list;
                    }
                }
            }
            if (_settingsByNameField != null)
            {
                var dict = _settingsByNameField.GetValue(null);
                var keys = dict?.GetType().GetProperty("Keys")?.GetValue(dict) as IEnumerable;
                if (keys != null)
                {
                    var list = new List<string>();
                    foreach (var k in keys) if (k is string s) list.Add(s);
                    list.Sort(StringComparer.OrdinalIgnoreCase);
                    return list;
                }
            }
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[GetModNames] {ex.Message}"); }
        return null;
    }

    private static void JumpTo(string modName)
    {
        try
        {
            if (_gui == null || _selectMod == null) { ModMain.Log?.Warning("[JumpTo] gui/selectMod null"); return; }
            _selectMod.Invoke(_gui, new object[] { modName });
            ModMain.Log?.Msg($"[JumpTo] → {modName}");
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[JumpTo] {ex.Message}"); }
    }
}
