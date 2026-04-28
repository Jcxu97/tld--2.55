using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using Il2CppInterop.Runtime;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(ModSettingsQuickNav.ModMain), "ModSettingsQuickNav", "1.2.0", "user")]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: MelonAdditionalDependencies("ModSettings")]

namespace ModSettingsQuickNav;

// v1.2.0:
//  - 自动显示,不用热键 —— ModSettings tab 一激活就出浮层
//  - +/- 按钮调 UI 缩放(字体跟着变),4K 显示器用
//  - 缩放值存 Mods/ModSettingsQuickNav.json,重启保留
// v1.1.0:去 Harmony,纯轮询 FindObjectsOfType(ModSettingsGUI)
public class ModMain : MelonMod
{
    internal static MelonLogger.Instance Log;

    public override void OnInitializeMelon()
    {
        Log = LoggerInstance;
        NavOverlay.LoadConfig();
        Log.Msg($"ModSettingsQuickNav v1.2.0 loaded — auto-show on ModSettings panel, scale={NavOverlay.Scale:F1}");
    }

    private int _poll = 0;
    public override void OnUpdate()
    {
        if (++_poll >= 30)
        {
            _poll = 0;
            NavOverlay.RefreshPanelState();
        }
    }

    public override void OnGUI()
    {
        NavOverlay.Draw();
    }
}

internal static class NavOverlay
{
    public static bool IsActive;

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
            // 简单 regex 式解析,避免拖 json 库
            var m = System.Text.RegularExpressions.Regex.Match(text, "\"Scale\"\\s*:\\s*(\\d+(?:\\.\\d+)?)");
            if (m.Success && float.TryParse(m.Groups[1].Value, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float s))
                Scale = Mathf.Clamp(s, ScaleMin, ScaleMax);
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[LoadConfig] {ex.Message}"); }
    }

    public static void SaveConfig()
    {
        try
        {
            File.WriteAllText(ConfigPath,
                $"{{\n  \"Scale\": {Scale.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}\n}}\n");
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
        if (!IsActive) return;

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
            // 关闭做成本帧暂时隐藏;实际只要 ModSettings tab 还在就会下一帧自动回来
            if (GUI.Button(new Rect((w - 30f) * Scale, 4f * Scale, 24f * Scale, 18f * Scale), "×"))
            {
                IsActive = false;  // 手动隐藏本帧,下次 RefreshPanelState 会重置
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
