using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Il2CppInterop.Runtime;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(ModSettingsQuickNav.ModMain), "ModSettingsQuickNav", "1.1.0", "user")]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: MelonAdditionalDependencies("ModSettings")]

namespace ModSettingsQuickNav;

// v1.1.0 完全去掉 Harmony patch —— 前一版 patch internal [RegisterTypeInIl2Cpp] 类的
// OnEnable/OnDisable 引发游戏启动卡死(Il2CppInterop 下对 Il2Cpp 注册类的 patch 不稳)。
// 改纯轮询:OnUpdate 每帧/每 30 帧查一次当前是否在 Options 面板,backquote 切换浮层。
public class ModMain : MelonMod
{
    internal static MelonLogger.Instance Log;

    public override void OnInitializeMelon()
    {
        Log = LoggerInstance;
        Log.Msg("ModSettingsQuickNav v1.1.0 loaded (polling mode, no Harmony) — press ` in ModSettings tab");
    }

    private int _poll = 0;
    public override void OnUpdate()
    {
        // 30 帧查一次面板状态,避免每帧反射
        if (++_poll >= 30)
        {
            _poll = 0;
            NavOverlay.RefreshPanelState();
        }
        if (NavOverlay.IsActive && Input.GetKeyDown(KeyCode.BackQuote))
            NavOverlay.Visible = !NavOverlay.Visible;
    }

    public override void OnGUI()
    {
        NavOverlay.Draw();
    }
}

internal static class NavOverlay
{
    public static bool IsActive;   // ModSettingsGUI MonoBehaviour 当前 enabled
    public static bool Visible;    // 用户按 ` 显示列表

    private static object _gui;                     // ModSettingsGUI 实例
    private static Type _guiType;
    private static Type _menuType;                  // ModSettingsMenu (static)
    private static MethodInfo _selectMod;           // ModSettingsGUI.SelectMod(string)
    private static FieldInfo _modTabsField;         // ModSettingsGUI.modTabs (instance)
    private static FieldInfo _settingsByNameField;  // ModSettingsMenu.settingsByModName (static)
    private static bool _typesResolved = false;

    private static Vector2 _scroll;
    private static Rect _win = new Rect(20f, 80f, 320f, 560f);
    private const int WinId = 0x35514E41;

    private static string _filter = "";
    private static readonly char[] AlphaLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

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
            {
                _settingsByNameField = _menuType.GetField("settingsByModName", BindingFlags.Static | BindingFlags.NonPublic);
            }
            _typesResolved = true;
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[ResolveTypes] {ex.Message}"); _typesResolved = true; }
    }

    // 轮询当前是否有 ModSettingsGUI 实例处于 enabled 状态 —— 等价于"用户在 Options 的 Mod Settings tab"
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
            if (!IsActive) Visible = false;
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[Refresh] {ex.Message}"); IsActive = false; Visible = false; }
    }

    public static void Draw()
    {
        if (!IsActive) return;

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
                GUI.Label(new Rect(12f, 30f, 280f, 22f), "(mod 列表取不到,重开 options 面板试试)");
                GUI.DragWindow(new Rect(0, 0, 10000, 24));
                return;
            }

            if (GUI.Button(new Rect(_win.width - 30f, 4f, 24f, 18f), "×"))
            {
                Visible = false;
                return;
            }

            // A-Z 跳段 13×2
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

            var displayList = new List<string>(modNames.Count);
            foreach (var n in modNames)
            {
                if (string.IsNullOrEmpty(_filter) ||
                    n.StartsWith(_filter, StringComparison.OrdinalIgnoreCase))
                    displayList.Add(n);
            }

            GUI.Label(new Rect(8f, y, 300f, 18f), $"共 {modNames.Count} 个 mod,当前显示 {displayList.Count}");
            y += 20f;

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
        catch (Exception ex) { ModMain.Log?.Error($"[DrawWindow] {ex}"); }
    }

    // 优先从当前 GUI 实例的 modTabs dict 取;拿不到就从 ModSettingsMenu.settingsByModName 静态表取
    private static List<string> GetModNames()
    {
        try
        {
            ResolveTypes();

            // 1) 实例层:ModSettingsGUI.modTabs
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

            // 2) 静态 fallback:ModSettingsMenu.settingsByModName
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
