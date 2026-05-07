using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace TldHacks;

// ═══════════════════════════════════════════════════════════════════
//   v3.0.4 IntegratedPatches7
//   chunk 3b: UT 手电筒(去颜色) — 6 patches
//   chunk 3c: StackManager v1.0.6 — 核心 GearItem.Awake + 默认列表
// ═══════════════════════════════════════════════════════════════════

// ─── UT 手电筒: Awake 随机电量 ───
[HarmonyPatch(typeof(FlashlightItem), "Awake")]
internal static class Patch_UT_FlashAwake
{
    static void Prefix(FlashlightItem __instance)
    {
        try
        {
            if (ModMain.Settings == null || !ModMain.Settings.UT_FlashRandomBattery) return;
            __instance.m_CurrentBatteryCharge = Random.Range(0f, 1f);
        }
        catch { }
    }
}

// ─── UT 手电筒: 远光限制 ───
[HarmonyPatch(typeof(FlashlightItem), "IsLit")]
internal static class Patch_UT_FlashIsLit
{
    static bool Prefix(FlashlightItem __instance, ref bool __result)
    {
        try
        {
            if (ModMain.Settings == null || !ModMain.Settings.UT_FlashHighBeamRestrict) return true;
            if ((int)__instance.m_State != 2) return true;
            var am = GameManager.GetAuroraManager();
            if (am != null && am.AuroraIsActive()) return true;
            // 远光在非极光时被锁
            try
            {
                GameAudioManager.PlayGUIError();
                HUDMessage.AddMessage(Localization.Get("GAMEPLAY_StateHighFail"), false, false);
            }
            catch { }
            __result = __instance.IsOn();
            return false;
        }
        catch { return true; }
    }
}

// ─── UT 手电筒: ExtendedFunctionality 保持电量 ───
[HarmonyPatch(typeof(FlashlightItem), "GetNormalizedCharge")]
internal static class Patch_UT_FlashCharge
{
    static bool Prefix(FlashlightItem __instance, ref float __result)
    {
        try
        {
            if (ModMain.Settings == null || !ModMain.Settings.UT_FlashExtended) return true;
            __result = __instance.m_CurrentBatteryCharge;
            return false;
        }
        catch { return true; }
    }
}

// ─── UT 手电筒: Update 调耗电时间 / InfiniteBattery / HighBeam 限制 ───
// v3.0.4r4 性能: 任何 UT_Flash 字段全 vanilla 时整段 early return,避免每帧 3 setter + name 比较
[HarmonyPatch(typeof(FlashlightItem), "Update")]
internal static class Patch_UT_FlashUpdate
{
    static void Postfix(FlashlightItem __instance)
    {
        try
        {
            var s = ModMain.Settings;
            if (s == null || __instance == null) return;

            // 性能 early return: 所有 UT_Flash 都是 vanilla 默认 → 整段 skip
            bool anyActive = s.UT_FlashHighBeamRestrict || s.UT_FlashInfiniteBattery
                || Mathf.Abs(s.UT_FlashLowBeam - 1f) > 0.001f
                || Mathf.Abs(s.UT_FlashHighBeam - 0.0833f) > 0.001f
                || Mathf.Abs(s.UT_FlashRecharge - 2f) > 0.001f
                || Mathf.Abs(s.UT_MinerFlashLowBeam - 1.5f) > 0.001f
                || Mathf.Abs(s.UT_MinerFlashHighBeam - 0.0833f) > 0.001f
                || Mathf.Abs(s.UT_MinerFlashRecharge - 1.75f) > 0.001f;
            if (!anyActive) return;

            // 无限电量
            if (s.UT_FlashInfiniteBattery)
            {
                __instance.m_CurrentBatteryCharge = 1f;
            }

            // 自定义持续时间
            bool isMiner = __instance.m_GearItem != null
                && ((UnityEngine.Object)__instance.m_GearItem).name == "GEAR_Flashlight_LongLasting";
            __instance.m_LowBeamDuration = isMiner ? s.UT_MinerFlashLowBeam : s.UT_FlashLowBeam;
            __instance.m_HighBeamDuration = isMiner ? s.UT_MinerFlashHighBeam : s.UT_FlashHighBeam;
            __instance.m_RechargeTime = isMiner ? s.UT_MinerFlashRecharge : s.UT_FlashRecharge;
        }
        catch { }
    }
}

// v3.0.4r4 性能: 删除 Patch_UT_FlashFlicker — Prefix 永远 return true 是空 patch,
// 但 LightRandomIntensity.Update 每帧每个极光灯实例触发 Harmony bridge = 纯浪费
// UT_FlashAuroraFlicker 字段保留(未来要做时再加 patch)

// ─── UT 手电筒: ExtendedFunctionality 切换光源同步(从 EnableLights 简化) ───
// v3.0.4 注: ExtendedFunctionality 完整版涉及 Light 子组件 enable 切换,
// 这里只做电量保持(GetNormalizedCharge 已实现),光源切换暂跳(原版逻辑可用)

// ═══════════════════════════════════════════════════════════════════
//   StackManager — GearItem.Awake 给指定物品添加 StackableItem 组件
// ═══════════════════════════════════════════════════════════════════

internal static class StackManagerData
{
    // 复制自 StackManager v1.0.6 SetupDefaultConfig (StackManager.decompiled.cs:225-229)
    internal static readonly System.Collections.Generic.HashSet<string> AddStackableComponent = new()
    {
        "GEAR_Potato",
        "GEAR_StumpRemover",
    };

    internal static readonly System.Collections.Generic.HashSet<string> StackMerge = new()
    {
        "GEAR_BirchSaplingDried", "GEAR_BearHideDried", "GEAR_BottleAntibiotics", "GEAR_BottlePainKillers",
        "GEAR_Carrot", "GEAR_CoffeeTin", "GEAR_GreenTeaPackage", "GEAR_GutDried",
        "GEAR_LeatherDried", "GEAR_LeatherHideDried", "GEAR_MapleSaplingDried", "GEAR_MooseHideDried",
        "GEAR_PackMatches", "GEAR_Potato", "GEAR_RabbitPeltDried", "GEAR_StumpRemover",
        "GEAR_WolfPeltDried", "GEAR_WoodMatches",
    };

    internal static string Normalize(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        // 去 (Clone) 后缀
        int idx = name.IndexOf("(Clone)");
        return idx >= 0 ? name.Substring(0, idx) : name;
    }
}

[HarmonyPatch(typeof(GearItem), "Awake")]
internal static class Patch_StackMgr_GearAwake
{
    static void Prefix(GearItem __instance)
    {
        try
        {
            if (ModMain.Settings == null || !ModMain.Settings.Stack_AddComponent) return;
            if (__instance == null || GameManager.IsMainMenuActive()) return;
            string n = ((UnityEngine.Object)__instance).name;
            if (string.IsNullOrEmpty(n)) return;
            n = StackManagerData.Normalize(n);
            if (__instance.m_StackableItem != null) return;
            if (((UnityEngine.Component)__instance).gameObject.GetComponent<StackableItem>() != null) return;
            if (!StackManagerData.AddStackableComponent.Contains(n)) return;

            var si = ((UnityEngine.Component)__instance).gameObject.AddComponent<StackableItem>();
            si.m_DefaultUnitsInItem = 1;
            si.m_StackConditionDifferenceConstraint = 100f;
            si.m_StackSpriteName = string.Empty;
            si.m_ShareStackWithGear = new Il2CppReferenceArray<StackableItem>(0L);
            if (si.m_Units == 0) si.m_Units = 1;
            __instance.m_StackableItem = si;
        }
        catch { }
    }

    static void Postfix(GearItem __instance)
    {
        try
        {
            if (ModMain.Settings == null) return;
            if (__instance == null || GameManager.IsMainMenuActive()) return;
            string n = ((UnityEngine.Object)__instance).name;
            if (string.IsNullOrEmpty(n)) return;
            n = StackManagerData.Normalize(n);
            var si = ((UnityEngine.Component)__instance).gameObject.GetComponent<StackableItem>();

            if (ModMain.Settings.Stack_UseMaxHP
                && StackManagerData.StackMerge.Contains(n) && si != null)
            {
                si.m_StackConditionDifferenceConstraint = 100f;
            }

            // 防腐物品(高 HP)
            if (n == "GEAR_CoffeeTin") { __instance.SetHaltDecay(true); __instance.CurrentHP = 1000f; }
            else if (n == "GEAR_GreenTeaPackage") { __instance.SetHaltDecay(true); __instance.CurrentHP = 1500f; }
            else if (n == "GEAR_Carrot") { __instance.SetHaltDecay(true); __instance.CurrentHP = 50f; }
            else if (n == "GEAR_Potato") { __instance.SetHaltDecay(true); __instance.CurrentHP = 100f; }
        }
        catch { }
    }
}
