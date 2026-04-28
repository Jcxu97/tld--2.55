using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(ModSettingsQuickNav.ModMain), "ModSettingsQuickNav", "1.0.0", "user")]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: MelonAdditionalDependencies("ModSettings")]

namespace ModSettingsQuickNav;

public class ModMain : MelonMod
{
    internal static MelonLogger.Instance Log;

    public override void OnInitializeMelon()
    {
        Log = LoggerInstance;
        Log.Msg("ModSettingsQuickNav v1.0.0 loaded — press ` (backquote) in ModSettings tab to open mod jump list");
    }

    public override void OnGUI()
    {
        NavOverlay.Draw();
    }

    public override void OnUpdate()
    {
        if (NavOverlay.IsModSettingsActive && Input.GetKeyDown(KeyCode.BackQuote))
            NavOverlay.Visible = !NavOverlay.Visible;
    }
}

// ———————————————————————————————————————————————————————————————
// Harmony patches — 反射定位 internal ModSettings.ModSettingsGUI 的 OnEnable/OnDisable
// ———————————————————————————————————————————————————————————————
[HarmonyPatch]
internal static class Patch_OnEnable
{
    private static MethodBase TargetMethod()
    {
        var t = AccessTools.TypeByName("ModSettings.ModSettingsGUI");
        return t?.GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
    }
    private static bool Prepare() => AccessTools.TypeByName("ModSettings.ModSettingsGUI") != null;
    private static void Postfix(object __instance) => NavOverlay.Bind(__instance);
}

[HarmonyPatch]
internal static class Patch_OnDisable
{
    private static MethodBase TargetMethod()
    {
        var t = AccessTools.TypeByName("ModSettings.ModSettingsGUI");
        return t?.GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
    }
    private static bool Prepare() => AccessTools.TypeByName("ModSettings.ModSettingsGUI") != null;
    private static void Postfix() => NavOverlay.Unbind();
}


// ———————————————————————————————————————————————————————————————
// NavOverlay — IMGUI 浮层:列出所有 mod,点击跳转
// ———————————————————————————————————————————————————————————————
internal static class NavOverlay
{
    public static bool IsModSettingsActive;
    public static bool Visible;

    private static object _gui;                 // ModSettingsGUI 实例(internal 类,用 object)
    private static MethodInfo _selectMod;       // ModSettingsGUI.SelectMod(string)
    private static FieldInfo _modTabsField;     // ModSettingsGUI.modTabs Dictionary<string, ModTab>

    private static Vector2 _scroll;
    private static Rect _win = new Rect(20f, 80f, 320f, 560f);
    private const int WinId = 0x35514E41;

    private static string _filter = "";
    private static readonly char[] AlphaLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

    public static void Bind(object gui)
    {
        _gui = gui;
        IsModSettingsActive = true;
        try
        {
            var t = gui.GetType();
            _selectMod    = t.GetMethod("SelectMod", BindingFlags.Instance | BindingFlags.NonPublic);
            _modTabsField = t.GetField("modTabs",    BindingFlags.Instance | BindingFlags.NonPublic);
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[Bind] {ex.Message}"); }
    }

    public static void Unbind()
    {
        IsModSettingsActive = false;
        Visible = false;
        _gui = null;
    }

    public static void Draw()
    {
        if (!IsModSettingsActive) return;

        // 右上角小按钮:提示 + 切换
        var hintRect = new Rect(Screen.width - 280f, 10f, 270f, 24f);
        string hint = Visible ? "Mod 跳转 [点关闭] ( ` 切换)" : "Mod 跳转列表 [ ` 键打开]";
        if (GUI.Button(hintRect, hint)) Visible = !Visible;

        if (!Visible) return;
        _win = GUI.Window(WinId, _win, (GUI.WindowFunction)DrawWindow, "Mod 快速跳转");
    }

    private static void DrawWindow(int id)
    {
        try
        {
            var modNames = GetModNames();
            if (modNames == null || modNames.Count == 0)
            {
                GUI.Label(new Rect(12f, 30f, 280f, 22f), "(modTabs 没取到 / 列表空)");
                GUI.DragWindow(new Rect(0, 0, 10000, 24));
                return;
            }

            if (GUI.Button(new Rect(_win.width - 30f, 4f, 24f, 18f), "×"))
            {
                Visible = false;
                return;
            }

            // 字母跳段按钮栏(每行 13 个,两行 A-Z)
            float y = 28f;
            float bw = 22f, bh = 18f;
            for (int i = 0; i < AlphaLetters.Length; i++)
            {
                int col = i % 13;
                int row = i / 13;
                var r = new Rect(8f + col * (bw + 1f), y + row * (bh + 2f), bw, bh);
                if (GUI.Button(r, AlphaLetters[i].ToString()))
                {
                    _filter = AlphaLetters[i].ToString();
                    _scroll = Vector2.zero;
                }
            }
            y += 2 * (bh + 2f) + 2f;

            GUI.Label(new Rect(8f, y, 200f, 20f),
                string.IsNullOrEmpty(_filter) ? "筛选:(无,显示全部)" : $"筛选:首字母 = {_filter}");
            if (GUI.Button(new Rect(240f, y - 2f, 60f, 22f), "清除"))
            {
                _filter = "";
                _scroll = Vector2.zero;
            }
            y += 26f;

            // 过滤
            var displayList = new List<string>(modNames.Count);
            foreach (var n in modNames)
            {
                if (string.IsNullOrEmpty(_filter) ||
                    n.StartsWith(_filter, StringComparison.OrdinalIgnoreCase))
                    displayList.Add(n);
            }

            // 计数
            GUI.Label(new Rect(8f, y, 300f, 18f), $"共 {modNames.Count} 个 mod,当前显示 {displayList.Count}");
            y += 20f;

            // 滚动列表
            var listRect = new Rect(8f, y, _win.width - 16f, _win.height - y - 12f);
            var contentH = displayList.Count * 24f + 4f;
            var contentRect = new Rect(0, 0, listRect.width - 18f, contentH);
            _scroll = GUI.BeginScrollView(listRect, _scroll, contentRect, false, true);

            for (int i = 0; i < displayList.Count; i++)
            {
                var btnRect = new Rect(0f, i * 24f, contentRect.width, 22f);
                if (GUI.Button(btnRect, displayList[i]))
                {
                    JumpTo(displayList[i]);
                    Visible = false;
                }
            }
            GUI.EndScrollView();

            GUI.DragWindow(new Rect(0, 0, 10000, 24));
        }
        catch (Exception ex)
        {
            ModMain.Log?.Error($"[DrawWindow] {ex}");
        }
    }

    private static List<string> GetModNames()
    {
        try
        {
            if (_gui == null || _modTabsField == null) return null;
            var modTabs = _modTabsField.GetValue(_gui);
            if (modTabs == null) return null;

            // modTabs 是 System.Collections.Generic.Dictionary<string, ModTab>,走反射拿 Keys
            var dictType = modTabs.GetType();
            var keysProp = dictType.GetProperty("Keys");
            var keys = keysProp?.GetValue(modTabs) as System.Collections.IEnumerable;
            if (keys == null) return null;

            var list = new List<string>();
            foreach (var k in keys) if (k is string s) list.Add(s);
            list.Sort(StringComparer.OrdinalIgnoreCase);
            return list;
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[GetModNames] {ex.Message}"); return null; }
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
