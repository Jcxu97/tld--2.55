using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.BigCarry;
using Il2CppTLD.Gear;
using Il2CppTLD.IntBackedUnit;
using Il2CppTLD.Interactions;
using Il2CppTLD.Placement;
using UnityEngine;

namespace TldHacks;

// ═══════════════════════════════════════════════════════════════════
//   v3.0.4 IntegratedPatches6 — UniversalTweaks v1.4.8 chunk 3a
//   常用修改 / 食物 / 岩石贮藏 / 喷漆 / 雪橇 (~15 patches)
//   跳过 chunk 3b 的: 手电筒(12 patches) / 容器容量(40+ 项)
// ═══════════════════════════════════════════════════════════════════

// ─── 哈气可见性 (DynamicPatch — 默认开启时不需要 patch) ───
internal static class Patch_UT_BreathVisibility
{
    public static void Postfix(Breath __instance)
    {
        try
        {
            __instance.m_SuppressEffects = true;
        }
        catch { }
    }
}

// ─── 自定义模式徽章 ───
[HarmonyPatch(typeof(Feat), "ShouldBlockIncrement")]
internal static class Patch_UT_FeatCustom
{
    static void Postfix(ref bool __result)
    {
        if (ModMain.Settings != null && ModMain.Settings.UT_FeatProgressInCustom) __result = false;
    }
}

// ─── 左轮改良: 瞄准时可走 (DynamicPatch 注册,不常驻) ───
internal static class Patch_UT_RevolverWalk
{
    public static void Postfix(vp_FPSPlayer __instance)
    {
        try
        {
            var pm = GameManager.GetPlayerManagerComponent();
            if (pm == null) return;
            if ((int)pm.GetControlMode() != 18) return;
            if (!GameManager.IsMoveInputUnblocked()) return;
            __instance.InputWalk();
        }
        catch { }
    }
}

// ─── 左轮改良: 隐藏限制 UI (DynamicPatch 注册,不常驻) ───
internal static class Patch_UT_RevolverUI
{
    public static void Postfix(Panel_HUD __instance)
    {
        try
        {
            var pm = GameManager.GetPlayerManagerComponent();
            if (pm == null) return;
            if ((int)pm.GetControlMode() != 18) return;
            if (__instance.m_AimingLimitedMobility != null)
                ((UnityEngine.Component)__instance.m_AimingLimitedMobility).gameObject.SetActive(false);
        }
        catch { }
    }
}

// ─── 物品掉落随机旋转 ───
[HarmonyPatch(typeof(GearItem), "Drop")]
internal static class Patch_UT_DropRotation
{
    static void Postfix(GearItem __instance)
    {
        try
        {
            if (ModMain.Settings == null || !ModMain.Settings.UT_RandomizedItemRotation) return;
            if (__instance == null) return;
            float yaw = Random.Range(0f, 360f);
            var t = ((UnityEngine.Component)__instance).transform;
            string n = ((UnityEngine.Object)__instance).name;
            t.eulerAngles = n.Contains("GEAR_Rifle")
                ? new Vector3(t.eulerAngles.x, yaw, 90f)
                : new Vector3(0f, yaw, 0f);
        }
        catch { }
    }
}

// ─── 制噪器参数 + 头疼 debuff + 绷带重量 + 炖汤疲劳 + MRE 纹理 ───
[HarmonyPatch(typeof(GearItem), "Deserialize")]
internal static class Patch_UT_GearItemDeserialize
{
    static void Postfix(GearItem __instance)
    {
        try
        {
            var s = ModMain.Settings;
            if (s == null || __instance == null) return;
            string n = ((UnityEngine.Object)((UnityEngine.Component)__instance).gameObject).name;

            // 制噪器
            if (n == "GEAR_NoiseMaker" && __instance.m_NoiseMakerItem != null)
            {
                __instance.m_NoiseMakerItem.m_BurnLifetimeMinutes = s.UT_NoisemakerBurnLength;
                __instance.m_NoiseMakerItem.m_ThrowForce = s.UT_NoisemakerThrowForce;
            }

            // 炖汤疲劳调整
            if (n == "GEAR_CookedStewMeat" || n == "GEAR_CookedStewVegetables")
            {
                try
                {
                    var fx = ((UnityEngine.Component)__instance).gameObject.GetComponentInParent<FoodStatEffect>();
                    if (fx != null) fx.m_Effect = s.UT_StewFatigueLoss;
                }
                catch { }
            }

            // 绷带重量
            if (s.UT_ConsistantDressingWeight && n == "GEAR_OldMansBeardDressing"
                && __instance.m_GearItemData != null)
            {
                __instance.m_GearItemData.m_BaseWeight = ItemWeight.FromKilograms(0.03f);
            }
        }
        catch { }
    }
}

// ─── 滤罐持续时间 ───
[HarmonyPatch(typeof(Respirator), "AttachCanister")]
internal static class Patch_UT_CanisterDuration
{
    static void Postfix(RespiratorCanister canister)
    {
        try
        {
            if (ModMain.Settings == null || canister == null) return;
            canister.m_ProtectionDurationRTSeconds = ModMain.Settings.UT_RespiratorCanisterDuration;
        }
        catch { }
    }
}

// ─── 岩石贮藏: 室内 + 数量/距离 ───
[HarmonyPatch(typeof(RockCacheManager), "CanAttemptToPlaceRockCache")]
internal static class Patch_UT_RockCacheCheck
{
    static bool Prefix(ref bool __result)
    {
        if (ModMain.Settings != null && ModMain.Settings.UT_RockCacheIndoors)
        {
            __result = true;
            return false;
        }
        return true;
    }

    static void Postfix(RockCacheManager __instance)
    {
        try
        {
            var s = ModMain.Settings;
            if (s == null) return;
            __instance.m_MaxRockCachesPerRegion = s.UT_RockCacheMaxPerRegion;
            __instance.m_MinDistanceBetweenRockCaches = s.UT_RockCacheMinDistance;
        }
        catch { }
    }
}

// ─── 雪屋衰减 ───
[HarmonyPatch(typeof(SnowShelterManager), "InstantiateSnowShelter")]
internal static class Patch_UT_SnowShelterDecay
{
    static void Postfix(ref SnowShelter __result)
    {
        try
        {
            if (ModMain.Settings == null || __result == null) return;
            __result.m_DailyDecayHP = ModMain.Settings.UT_SnowShelterDecay;
        }
        catch { }
    }
}

// ─── 雪橇: 互动限制 ───
[HarmonyPatch(typeof(TravoisBigCarryItem), "CanPerformInteractionWhileCarrying")]
internal static class Patch_UT_TravoisInteract
{
    static void Postfix(ref bool __result, IInteraction interaction)
    {
        try
        {
            if (ModMain.Settings == null || !ModMain.Settings.UT_TravoisOverrideInteraction) return;
            __result = !(GameManager.GetPlayerInVehicle().IsEntering()
                      || GameManager.GetSnowShelterManager().PlayerEnteringShelter());
        }
        catch { }
    }
}

// ─── 雪橇: 移动限制 (CarryDisplayError 类型 IL2CPP 不公开,用反射方式后续补) ───
// v3.0.4 chunk 3a 暂跳过 — UT_TravoisOverrideMovement toggle 当前无效
// chunk 3b 用 DynamicPatch/反射方式注册

// ─── 雪橇: 全参数 (OnCarried) ───
[HarmonyPatch(typeof(TravoisBigCarryItem), "OnCarried")]
internal static class Patch_UT_TravoisParams
{
    static void Postfix(TravoisBigCarryItem __instance)
    {
        try
        {
            var s = ModMain.Settings;
            if (s == null || __instance == null || __instance.m_TravoisMovement == null) return;
            __instance.m_TravoisMovement.m_TurnSpeed = s.UT_TravoisTurnSpeed;
            __instance.m_TravoisMovement.m_MaxSlopeClimbAngle = s.UT_TravoisMaxSlope;
            __instance.m_TravoisMovement.m_MaxSlopeDownhillAngle = s.UT_TravoisMaxSlope;
            __instance.m_BlizzardDecayPerHour = s.UT_TravoisDecayBlizzard;
            __instance.m_DecayHPPerHour = s.UT_TravoisDecayHourly / 1000f;
            __instance.m_MovementDecayPerUnit = s.UT_TravoisDecayMovement / 100f;
        }
        catch { }
    }
}

// ─── 喷漆: 高亮 (DynamicPatch 注册,不常驻 — 渲染路径,每帧每喷漆标记触发一次) ───
internal static class Patch_UT_GlowingDecals
{
    private static float _lastMult;
    private static bool _applied;

    public static void Prefix(DynamicDecalsManager __instance)
    {
        try
        {
            if (__instance == null || __instance.m_GlowMaterial == null) return;
            float mult = ModMain.Settings.UT_GlowingDecalMult;
            if (_applied && mult == _lastMult) return;
            _lastMult = mult;
            _applied = true;
            __instance.m_GlowMaterial.SetColor("_GlowColor", new Color(1f, 0.45f, 0f, 0f));
            __instance.m_GlowMaterial.SetFloat("_GlowMult", mult);
            __instance.m_AnimatedRevealMaterial = __instance.m_GlowMaterial;
        }
        catch { }
    }
}

// ─── 喷漆: 重叠率 ───
[HarmonyPatch(typeof(Panel_SprayPaint), "Enable", new System.Type[] { typeof(bool) })]
internal static class Patch_UT_DecalOverlap
{
    static void Postfix()
    {
        try
        {
            if (ModMain.Settings == null) return;
            var dm = GameManager.GetDynamicDecalsManager();
            if (dm != null) dm.m_DecalOverlapLeniencyPercent = ModMain.Settings.UT_DecalOverlap;
        }
        catch { }
    }
}

// ─── 马桶水可饮用 (DynamicPatch 注册,不常驻) ───
internal static class Patch_UT_ToiletWater
{
    public static void Postfix(WaterSource __instance)
    {
        try
        {
            if (__instance == null) return;
            if ((int)__instance.m_CurrentLiquidQuality == 0) return;
            __instance.m_CurrentLiquidQuality = (LiquidQuality)0;
        }
        catch { }
    }
}
