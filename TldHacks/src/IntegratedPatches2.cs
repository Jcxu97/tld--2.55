using System;
using System.Collections.Generic;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppTLD.IntBackedUnit;
using Il2CppTLD.News;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TldHacks;

// ——— 1. PauseInJournal: Panel_Log.Enable Postfix ———
internal static class Patch_PanelLog_Pause
{
    internal static void Postfix(Panel_Log __instance)
    {
        if (!ModMain.Settings.PauseInJournal) return;
        if (__instance.isActiveAndEnabled)
            GameManager.m_GlobalTimeScale = 0f;
        else
            GameManager.m_GlobalTimeScale = 1f;
    }
}

// ——— 2. SkipIntroRedux: Panel_Boot.Update Postfix ———
internal static class Patch_PanelBoot_SkipIntro
{
    internal static void Postfix(Panel_Boot __instance)
    {
        if (!ModMain.Settings.SkipIntro) return;
        try
        {
            __instance.m_HoldLoadingUntilInputGiven = false;
            __instance.m_StartShowingDisclaimers = false;

            if (__instance.m_DisclaimerScreenList != null)
                __instance.m_DisclaimerScreenIndex = ((Il2CppArrayBase<GameObject>)(object)__instance.m_DisclaimerScreenList).Count;

            try { if (__instance.m_MainDisclaimer != null) __instance.m_MainDisclaimer.SetActive(false); } catch { }

            // 每帧强制推进
            try { __instance.OnNextDisclaimerScreen(); } catch { }

            // 核弹方案:禁用 Panel_Boot 的全部子对象(声明无论叫什么名字都会被隐藏)
            try
            {
                var parent = ((Component)__instance).transform;
                for (int i = 0; i < parent.childCount; i++)
                {
                    var child = parent.GetChild(i);
                    if (child != null && child.gameObject.activeSelf)
                        child.gameObject.SetActive(false);
                }
            }
            catch { }

            // 全局搜索并隐藏
            DisclaimerKiller.KillAll();

            __instance.ReadyToActivateScene = true;
        }
        catch { }
    }
}

// SkipIntro: Panel_MainMenu.Enable Prefix — mark intro played + kill remaining disclaimers
internal static class Patch_PanelMainMenu_SkipIntro
{
    internal static void Prefix(Panel_MainMenu __instance)
    {
        if (!ModMain.Settings.SkipIntro) return;
        try
        {
            MoviePlayer.m_HasIntroPlayedForMainMenu = true;
            __instance.m_PlayedIntroMovie = true;
        }
        catch { }
        // 全局扫除残留声明面板
        DisclaimerKiller.KillAll();
    }
}

internal static class DisclaimerKiller
{
    private static bool _killed;
    public static void KillAll()
    {
        if (_killed) return;
        try
        {
            // 按名称关键词搜索
            string[] names = { "MainDisclaimer", "Disclaimer", "AIDisclaimer", "AntiAI", "Statement", "Panel_Disclaimer" };
            for (int i = 0; i < names.Length; i++)
            {
                var go = GameObject.Find(names[i]);
                if (go != null) go.SetActive(false);
            }
            // 搜索所有根对象中带 "disclaimer"/"statement" 字样的(不区分大小写)
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                string n = roots[i].name.ToLowerInvariant();
                if (n.Contains("disclaimer") || n.Contains("statement") || n.Contains("antiai"))
                    roots[i].SetActive(false);
            }
            _killed = true;
        }
        catch { }
    }
    public static void Reset() => _killed = false;
}

// ——— 3. VehicleFov: PlayerInVehicle.EnterVehicle Postfix ———
internal static class Patch_VehicleFov_Enter
{
    internal static void Postfix(PlayerInVehicle __instance)
    {
        if (!ModMain.Settings.VehicleKeepFov) return;
        try
        {
            var cam = GameManager.GetVpFPSCamera();
            if (cam != null)
            {
                float playerFov = cam.GetComponent<Camera>().fieldOfView;
                __instance.m_FOV = playerFov;
            }
        }
        catch { }
    }
}

// ——— 4. DroppableUndroppables: GearItem.Awake Postfix ———
internal static class Patch_GearItem_Awake_Droppable
{
    internal static void Postfix(GearItem __instance)
    {
        try
        {
            var s = ModMain.Settings;
            if (s == null) return;

            var data = __instance.m_GearItemData;

            // AllNote: 收藏品可丢弃(移除 NarrativeCollectibleItem 组件)
            if (s.AllNote)
            {
                var narrative = __instance.GetComponent<Il2Cpp.NarrativeCollectibleItem>();
                if (narrative != null)
                    UnityEngine.Object.DestroyImmediate(narrative);
            }

            if (!s.DroppableUndroppables) return;

            // 解除不可丢弃标志
            __instance.m_CantDropItem = false;
            if (data != null)
            {
                data.m_CantDrop = false;
                data.m_RemainInInventoryOnDrop = false;
                if (s.AllNote || s.DroppableUndroppables)
                    data.m_DisableMove = false;
            }

            // 关键物品重量调整
            if (!s.ImportantWeight && data != null)
            {
                string name = __instance.gameObject.name;
                if (name.Contains("Camera") || name.Contains("HandheldShortwave") ||
                    name.Contains("BoltCutter") || name.Contains("Respirator"))
                {
                    data.m_BaseWeight = Il2CppTLD.IntBackedUnit.ItemWeight.FromKilograms(0f);
                }
            }
        }
        catch { }
    }
}

// ——— 5. RememberBreakDownItem ———
internal static class BreakDownToolMemory
{
    internal static Dictionary<string, int> LastToolIndex = new();
}

internal static class Patch_BreakDown_RememberTool_Enable
{
    internal static void Postfix(Panel_BreakDown __instance)
    {
        if (!ModMain.Settings.RememberBreakdownTool) return;
        try
        {
            var bd = __instance.m_BreakDown;
            if (bd == null) return;
            string key = bd.m_LocalizedDisplayName?.m_LocalizationID ?? bd.name;
            if (BreakDownToolMemory.LastToolIndex.TryGetValue(key, out int idx))
            {
                if (idx >= 0 && idx < __instance.m_Tools.Count)
                {
                    __instance.m_SelectedToolItemIndex = idx;
                    __instance.RefreshTools();
                }
            }
        }
        catch { }
    }
}

internal static class Patch_BreakDown_RememberTool_OnBreakDown
{
    internal static void Prefix(Panel_BreakDown __instance)
    {
        if (!ModMain.Settings.RememberBreakdownTool) return;
        try
        {
            var bd = __instance.m_BreakDown;
            if (bd == null) return;
            string key = bd.m_LocalizedDisplayName?.m_LocalizationID ?? bd.name;
            BreakDownToolMemory.LastToolIndex[key] = __instance.m_SelectedToolItemIndex;
        }
        catch { }
    }
}


// ——— 7. ExtraGraphicsSettings: VehicleFreeLook ———
internal static class Patch_Vehicle_FreeLook
{
    internal static void Postfix(PlayerInVehicle __instance)
    {
        if (!ModMain.Settings.VehicleFreeLook) return;
        try
        {
            float yaw = ModMain.Settings.VehicleFreeLookYaw;
            float pitch = ModMain.Settings.VehicleFreeLookPitch;
            __instance.m_YawLimitDegrees = new Vector2(-yaw, yaw);
            __instance.m_PitchLimitDegrees = new Vector2(-pitch, pitch);
        }
        catch { }
    }
}


// ——— 9. CougarSoundBegone (scene-load based) ———
internal static class CougarSoundKiller
{
    public static void OnSceneLoad(string sceneName)
    {
        if (sceneName != "MainMenu") return;
        try
        {
            if (ModMain.Settings.MuteCougarMenuSound)
            {
                var obj = GameObject.Find("CougarProwlOneShot");
                if (obj != null) UnityEngine.Object.DestroyImmediate(obj);
                var obj2 = GameObject.Find("CougarIdleOneShot");
                if (obj2 != null) UnityEngine.Object.DestroyImmediate(obj2);
            }
            // SkipIntro: 主菜单加载后再次清除可能残留的声明面板
            if (ModMain.Settings.SkipIntro)
            {
                DisclaimerKiller.Reset();
                DisclaimerKiller.KillAll();
            }
        }
        catch { }
    }
}

// ——— 9b. SkipIntro: NewsCarousel.Awake Postfix — hide news panel ———
internal static class Patch_NewsCarousel_Hide
{
    internal static void Postfix(NewsCarousel __instance)
    {
        if (!ModMain.Settings.SkipIntro) return;
        try
        {
            var parent = ((Component)__instance).transform.parent;
            if (parent != null)
                ((Component)parent).gameObject.SetActive(false);
        }
        catch { }
    }
}

// ——— 10. Crosshair: HUDManager.UpdateCrosshair Postfix ———
internal static class Patch_Crosshair_Show
{
    internal static void Postfix()
    {
        try
        {
            var s = ModMain.Settings;
            if (s == null || !s.CrosshairEnabled) return;
            var pm = GameManager.GetPlayerManagerComponent();
            if (pm == null || !pm.PlayerIsZooming()) return;
            var item = pm.m_ItemInHands;
            if (item == null) return;

            bool show = false;
            if (s.CrosshairStone && item.m_StoneItem != null) show = true;
            if (s.CrosshairRifle && item.m_GunItem != null) show = true;
            if (s.CrosshairBow && item.m_BowItem != null) show = true;

            if (show)
            {
                var hud = InterfaceManager.GetPanel<Panel_HUD>();
                if (hud != null && hud.m_Sprite_Crosshair != null)
                {
                    hud.m_Sprite_Crosshair.gameObject.SetActive(true);
                    hud.m_Sprite_Crosshair.alpha = s.CrosshairAlpha;
                }
            }
        }
        catch { }
    }
}

