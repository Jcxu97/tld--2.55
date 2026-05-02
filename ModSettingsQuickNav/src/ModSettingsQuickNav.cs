using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(ModSettingsQuickNav.ModMain), "ModSettingsQuickNav", "3.4.0", "user")]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: MelonAdditionalDependencies("ModSettings")]

namespace ModSettingsQuickNav;

public class ModMain : MelonMod
{
    public override void OnInitializeMelon()
    {
        QuickNav.Apply(HarmonyInstance);
        LoggerInstance.Msg("v3.4.0 — NGUI collider click detection");
    }
}

internal static class QuickNav
{
    private static FieldInfo _modSelectorField;
    private static MethodInfo _selectMod;
    private static object _gui;
    private static bool _panelActive;

    private static bool _dropdownOpen;
    private static bool _guiSubscribed;
    private static List<string> _modNames = new();
    private static Vector2 _scroll;
    private static string _filter = "";

    private static GameObject _arrowGO;

    public static void Apply(HarmonyLib.Harmony harmony)
    {
        var guiType = AccessTools.TypeByName("ModSettings.ModSettingsGUI");
        if (guiType == null) return;

        _modSelectorField = guiType.GetField("modSelector", BindingFlags.Instance | BindingFlags.NonPublic);
        _selectMod = guiType.GetMethod("SelectMod", BindingFlags.Instance | BindingFlags.NonPublic);

        var onEnable = guiType.GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        var onDisable = guiType.GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        var update = guiType.GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        var self = typeof(QuickNav);
        var flags = BindingFlags.Static | BindingFlags.NonPublic;

        if (onEnable != null)
            harmony.Patch(onEnable, postfix: new HarmonyMethod(self.GetMethod(nameof(OnEnablePost), flags)));
        if (onDisable != null)
            harmony.Patch(onDisable, postfix: new HarmonyMethod(self.GetMethod(nameof(OnDisablePost), flags)));
        if (update != null)
            harmony.Patch(update, postfix: new HarmonyMethod(self.GetMethod(nameof(UpdatePost), flags)));
    }

    private static void OnEnablePost(object __instance)
    {
        _gui = __instance;
        _panelActive = true;
        RefreshModList();
        EnsureArrowLabel(__instance);
    }

    private static void OnDisablePost()
    {
        _panelActive = false;
        _gui = null;
        CloseDropdown();
    }

    private static void EnsureArrowLabel(object guiInstance)
    {
        try
        {
            if (_arrowGO != null) return;

            var selector = _modSelectorField?.GetValue(guiInstance);
            if (selector == null) return;

            var selectorTransform = ((Component)selector).transform;
            var increaseBtn = selectorTransform.Find("Button_Increase");
            if (increaseBtn == null) return;

            _arrowGO = new GameObject("QuickNav_Arrow");
            _arrowGO.transform.SetParent(selectorTransform, false);
            _arrowGO.layer = increaseBtn.gameObject.layer;

            var pos = increaseBtn.localPosition;
            _arrowGO.transform.localPosition = new Vector3(pos.x + 40f, pos.y, pos.z);

            var srcLabel = increaseBtn.GetComponentInChildren<UILabel>(true);
            var label = _arrowGO.AddComponent<UILabel>();
            if (srcLabel != null) label.trueTypeFont = srcLabel.trueTypeFont;
            label.fontSize = 20;
            label.text = "▼";
            label.color = new Color(0.9f, 0.75f, 0.4f, 1f);
            ((UIWidget)label).width = 32;
            ((UIWidget)label).height = 32;
            label.alignment = NGUIText.Alignment.Center;

            var col = _arrowGO.AddComponent<BoxCollider>();
            col.size = new Vector3(32f, 32f, 1f);

            MelonLogger.Msg($"[QuickNav] Arrow created at layer {_arrowGO.layer}, pos {_arrowGO.transform.localPosition}");
        }
        catch (Exception ex)
        {
            MelonLogger.Warning($"[QuickNav] Arrow setup: {ex.Message}");
        }
    }

    private static void UpdatePost(object __instance)
    {
        if (!_panelActive) return;

        if (_dropdownOpen)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                CloseDropdown();
            return;
        }

        if (Input.GetMouseButtonDown(0) && _arrowGO != null)
        {
            var hovered = UICamera.hoveredObject;
            if (hovered != null && hovered == _arrowGO)
            {
                OpenDropdown();
                GameAudioManager.PlayGUIButtonClick();
            }
        }
    }

    private static void OpenDropdown()
    {
        RefreshModList();
        if (_modNames.Count == 0) return;
        _dropdownOpen = true;
        _filter = "";
        _scroll = Vector2.zero;
        if (!_guiSubscribed)
        {
            MelonEvents.OnGUI.Subscribe(DrawDropdown, 200);
            _guiSubscribed = true;
        }
    }

    private static void CloseDropdown()
    {
        _dropdownOpen = false;
        if (_guiSubscribed)
        {
            MelonEvents.OnGUI.Unsubscribe(DrawDropdown);
            _guiSubscribed = false;
        }
    }

    private static void RefreshModList()
    {
        try
        {
            if (_gui == null || _modSelectorField == null) return;
            var selector = _modSelectorField.GetValue(_gui);
            if (selector == null) return;
            var itemsField = selector.GetType().GetField("items");
            if (itemsField == null) return;
            var items = itemsField.GetValue(selector);
            if (items is not IEnumerable enumerable) return;
            var list = new List<string>();
            foreach (var item in enumerable)
                if (item is string s) list.Add(s);
            list.Sort(StringComparer.OrdinalIgnoreCase);
            _modNames = list;
        }
        catch { }
    }

    public static void DrawDropdown()
    {
        if (!_dropdownOpen || _modNames.Count == 0) return;

        if (Event.current.type == EventType.KeyDown)
        {
            if (Event.current.keyCode == KeyCode.Escape)
            {
                CloseDropdown();
                Event.current.Use();
                return;
            }
            if (Event.current.keyCode == KeyCode.Return)
            {
                var f = GetFiltered();
                if (f.Count > 0)
                {
                    JumpTo(f[0]);
                    CloseDropdown();
                }
                Event.current.Use();
                return;
            }
        }

        float dpi = Mathf.Max(1f, Screen.height / 1080f);
        float panelW = 300f * dpi;
        float itemH = 26f * dpi;
        float filterH = 28f * dpi;
        float maxH = Mathf.Min(20f, _modNames.Count) * itemH + filterH + 8f;
        float panelX = (Screen.width - panelW) / 2f;
        float panelY = (Screen.height - maxH) / 2f;

        var panelRect = new Rect(panelX, panelY, panelW, maxH);
        GUI.Box(panelRect, "");
        GUI.Box(panelRect, "");

        int fs = Mathf.RoundToInt(14f * dpi);
        var style = new GUIStyle(GUI.skin.button) { fontSize = fs, alignment = TextAnchor.MiddleLeft };
        var filterStyle = new GUIStyle(GUI.skin.textField) { fontSize = fs };

        float y = panelY + 4f;
        GUI.SetNextControlName("QNFilter");
        _filter = GUI.TextField(new Rect(panelX + 4f, y, panelW - 8f, filterH), _filter, filterStyle);
        GUI.FocusControl("QNFilter");
        y += filterH + 4f;

        var filtered = GetFiltered();
        float listH = maxH - filterH - 8f;
        var viewRect = new Rect(panelX, y, panelW, listH);
        var contentRect = new Rect(0f, 0f, panelW - 20f * dpi, filtered.Count * itemH);
        _scroll = GUI.BeginScrollView(viewRect, _scroll, contentRect, false, true);

        for (int i = 0; i < filtered.Count; i++)
        {
            if (GUI.Button(new Rect(0f, i * itemH, contentRect.width, itemH - 2f), filtered[i], style))
            {
                JumpTo(filtered[i]);
                CloseDropdown();
            }
        }
        GUI.EndScrollView();
    }

    private static List<string> GetFiltered()
    {
        if (string.IsNullOrEmpty(_filter)) return _modNames;
        return _modNames.FindAll(n => n.IndexOf(_filter, StringComparison.OrdinalIgnoreCase) >= 0);
    }

    private static void JumpTo(string modName)
    {
        try
        {
            if (_gui == null || _selectMod == null) return;
            if (_modSelectorField != null)
            {
                var selector = _modSelectorField.GetValue(_gui);
                selector?.GetType().GetProperty("value")?.SetValue(selector, modName);
            }
            _selectMod.Invoke(_gui, new object[] { modName });
        }
        catch { }
    }
}
