using System;
using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppTLD.Gear;
using UnityEngine;

namespace TldHacks;

// —— 武器相关的方法级 patch(比 tick 改字段更可靠)——

// 无限弹药:每次 RemoveNextFromClip 被调用,若 InfiniteAmmo 则 skip
[HarmonyPatch(typeof(GunItem), "RemoveNextFromClip")]
internal static class Patch_Gun_RemoveNextFromClip
{
    private static bool Prefix() => !CheatState.InfiniteAmmo;
}

// 瞄准晃动归零(sway) —— 带式 patch 作双保险。真正干活的是 TickCamera 的 m_DisableAimSway
[HarmonyPatch(typeof(GunItem), "GetSwayIncreasePerSecond")]
internal static class Patch_Gun_SwayIncrease
{
    private static void Postfix(ref float __result)
    {
        if (CheatState.NoAimSway) __result = 0f;
    }
}

// 无后坐力(belt-n-suspenders):Getter patch + TickCamera 的 m_RecoilSpring 重置
[HarmonyPatch(typeof(GunItem), "GetRecoilPitch")]
internal static class Patch_Gun_RecoilPitch
{
    private static void Postfix(ref float __result)
    {
        if (CheatState.NoRecoil) __result = 0f;
    }
}

[HarmonyPatch(typeof(GunItem), "GetRecoilYaw")]
internal static class Patch_Gun_RecoilYaw
{
    private static void Postfix(ref float __result)
    {
        if (CheatState.NoRecoil) __result = 0f;
    }
}

// 关闭瞄准体力消耗:Stamina percent 返回 1(满)—— 配合 TickCamera 的 m_DisableAimStamina
[HarmonyPatch(typeof(GunItem), "GetCurrentStaminaPercent")]
internal static class Patch_Gun_StaminaPercent
{
    private static void Postfix(ref float __result)
    {
        if (CheatState.NoAimStamina) __result = 1f;
    }
}

// 可以开瞄即使体力不足
[HarmonyPatch(typeof(GunItem), "CanStartAiming")]
internal static class Patch_Gun_CanStartAiming
{
    private static void Postfix(ref bool __result)
    {
        if (CheatState.NoAimStamina) __result = true;
    }
}

// ——— Condition 伤害过滤总闸 ———
// v2.7.29:AddHealth 有 3 个重载(2 参 / 3 参 / AddHealthWithNoHudNotification)
//   之前只 patch 2 参,狼咬 BloodLoss 持续掉血走的是 3 参版 → 绕过 filter
//   "真隐身彻底无效" / "免动物伤害失效" 根因
//   现在 3 个重载全 patch,共用 HurtFilter 逻辑
internal static class DamageFilter
{
    // 返回 false = 跳过原伤害;true = 放行(治疗或未配 toggle 的伤害)
    public static bool ShouldBlock(float hp, DamageSource cause)
    {
        if (hp > 0f) return false;  // 治疗放行 (v2.7.29 从 >= 改 > 更严格)

        if (CheatState.TrueInvisible) return true;  // 真隐身 = 绝对无敌

        if (CheatState.NoFallDamage && cause == DamageSource.Falling) return true;
        if (CheatState.NoSuffocating && cause == DamageSource.Suffocating) return true;
        if (CheatState.ImmuneAnimalDamage)
        {
            if (cause == DamageSource.Wolf || cause == DamageSource.Bear || cause == DamageSource.Cougar)
                return true;
        }
        return false;
    }
}

[HarmonyPatch(typeof(Condition), "AddHealth", new System.Type[] { typeof(float), typeof(DamageSource) })]
internal static class Patch_Condition_AddHealth_2
{
    private static bool Prefix(float hp, DamageSource cause)
        => !DamageFilter.ShouldBlock(hp, cause);
}

[HarmonyPatch(typeof(Condition), "AddHealth", new System.Type[] { typeof(float), typeof(DamageSource), typeof(bool) })]
internal static class Patch_Condition_AddHealth_3
{
    private static bool Prefix(float hp, DamageSource cause)
        => !DamageFilter.ShouldBlock(hp, cause);
}

[HarmonyPatch(typeof(Condition), "AddHealthWithNoHudNotification", new System.Type[] { typeof(float), typeof(DamageSource) })]
internal static class Patch_Condition_AddHealthNoHud
{
    private static bool Prefix(float hp, DamageSource damageSource)
        => !DamageFilter.ShouldBlock(hp, damageSource);
}

// ——— 快速采集 v2.7.19:彻底换方案 —— 延迟调 HarvestSuccessful/QuarterSuccessful 跳过整个时间流逝 + fade
// 之前 Patch_Harvest_Accelerate 把 minutes=0 → panel 等时间但时间不走 = 卡死
// 之前 Patch_Harvest_Update 每帧强推字段 + CameraFade FinishFade = 黑屏 + 递归 = 点取消游戏都卡死
// 现在:Postfix 只记录"2 帧后完成",由 ModMain.OnUpdate 调 QuickHarvestRunner.Tick 完成
[HarmonyPatch(typeof(Panel_BodyHarvest), "StartHarvest", new System.Type[] { typeof(int), typeof(string) })]
internal static class Patch_Harvest_Start
{
    private static void Postfix(Panel_BodyHarvest __instance)
    {
        if (!CheatState.QuickAction) return;
        FadeSuppressionWindow.Arm();  // v2.7.29:采集过程内吃 fade
        QuickHarvestRunner.Queue(__instance, QuickHarvestRunner.Action.Harvest);
    }
}

[HarmonyPatch(typeof(Panel_BodyHarvest), "StartQuarter", new System.Type[] { typeof(int), typeof(string) })]
internal static class Patch_Harvest_StartQuarter
{
    private static void Postfix(Panel_BodyHarvest __instance)
    {
        if (!CheatState.QuickAction) return;
        FadeSuppressionWindow.Arm();
        QuickHarvestRunner.Queue(__instance, QuickHarvestRunner.Action.Quarter);
    }
}

// 延迟完成执行器 —— 每帧在 ModMain.OnUpdate 里 tick。
// 分离成 runner 是因为在 StartHarvest Postfix 直接调 HarvestSuccessful,此时 panel state 还未稳定,必报错
internal static class QuickHarvestRunner
{
    internal enum Action { None, Harvest, Quarter }
    internal static Panel_BodyHarvest Instance;
    internal static Action PendingAction = Action.None;
    internal static int Countdown = 0;

    public static void Queue(Panel_BodyHarvest inst, Action a)
    {
        // v2.7.29:防连采覆盖。正在等第一次完成时,第二次 Queue 会覆盖 Instance 导致第一次丢失
        if (PendingAction != Action.None) return;
        Instance = inst; PendingAction = a; Countdown = 2;
    }

    public static void Tick()
    {
        if (PendingAction == Action.None) return;
        if (Instance == null) { Reset(); return; }
        // v2.7.29:panel 已关或 disabled 时不调完成方法,避免 panel 生命周期错乱
        try
        {
            if (!Instance.isActiveAndEnabled) { Reset(); return; }
        }
        catch { Reset(); return; }

        Countdown--;
        if (Countdown > 0) return;
        try
        {
            if (PendingAction == Action.Harvest) Instance.HarvestSuccessful();
            else if (PendingAction == Action.Quarter) Instance.QuarterSuccessful();
            ModMain.Log?.Msg($"[QuickHarvest] {PendingAction} done");
        }
        catch (System.Exception ex) { ModMain.Log?.Warning($"[QuickHarvest] {ex.Message}"); }
        finally { Reset(); }
    }

    private static void Reset() { PendingAction = Action.None; Instance = null; Countdown = 0; }
}

// ——— 快速修理 v2.7.29:只保留 StartRepair Postfix 一次 set,删 Update Postfix ———
// 每帧 Update Postfix 是 FPS 杀手(v2.7.25 漏删),StartRepair 一次性把进度拉满 + 设 TimeAccel 就够
[HarmonyPatch(typeof(Panel_Repair), "StartRepair", new System.Type[] { typeof(int), typeof(string) })]
internal static class Patch_Repair_StartRepair
{
    private static void Postfix(Panel_Repair __instance)
    {
        if (!CheatState.QuickAction) return;
        FadeSuppressionWindow.Arm();
        try
        {
            __instance.m_ElapsedProgressBarSeconds = 999f;
            __instance.m_RepairTimeSeconds = 0.01f;
            __instance.m_ProgressBarTimeSeconds = 0.01f;
            __instance.m_RepairWillSucceed = true;
            __instance.m_TimeAccelerated = true;
        }
        catch (System.Exception ex) { ModMain.Log?.Warning($"[QuickRepair] {ex.Message}"); }
    }
}

// ——— 快速拆解 v2.7.29:改 OnBreakDown Postfix 一次 set,删 Update Postfix ———
// v2.7.61 扩展条件:QuickAction UI 已删(和 QuickBreakDown 重复),所以加 QuickBreakDown 分支
//   — 否则 OnBreakDown 永远不触发,v2.7.59 为此加的 Update_ForceFinish 路径缺 Fade.Arm 导致留黑屏
[HarmonyPatch(typeof(Panel_BreakDown), "OnBreakDown")]
internal static class Patch_BreakDown_OnBreakDown
{
    private static void Postfix(Panel_BreakDown __instance)
    {
        if (!CheatState.QuickAction && !CheatState.QuickBreakDown) return;
        FadeSuppressionWindow.Arm();
        try
        {
            __instance.m_TimeSpentBreakingDown = __instance.m_SecondsToBreakDown + 1f;
            __instance.m_TimeIsAccelerated = true;
        }
        catch (System.Exception ex) { ModMain.Log?.Warning($"[QuickBreakDown] {ex.Message}"); }
    }
}

// v2.7.19 撤掉所有 CameraFade patch + Panel_BodyHarvest.Update Patch:
//   前者递归风险,后者每帧强推字段让游戏状态机错乱 → 2.7.18 卡死根因
//   新方案靠 QuickHarvestRunner 延迟完成 = 直接跳过整个 fade + time 流程,根本不让黑屏出现

// v2.7.29 —— CameraFade 吃 fade 加时间窗口,避免死亡/切场景的正常黑屏被误吃
//   Panel_BodyHarvest / Panel_Crafting / Panel_Repair / Panel_BreakDown 的 Start/Postfix 里
//   调 FadeSuppressionWindow.Arm(1.5f) 打开 1.5s 吃 fade 窗口
//   1.5s 外的 fade 正常显示(死亡/切场景/烟雾效果不受影响)
internal static class FadeSuppressionWindow
{
    private static float _expiresAt = 0f;
    public static void Arm(float seconds = 1.5f)
    {
        try { _expiresAt = UnityEngine.Time.realtimeSinceStartup + seconds; } catch { }
    }
    public static bool IsActive
    {
        get
        {
            try { return UnityEngine.Time.realtimeSinceStartup < _expiresAt; }
            catch { return false; }
        }
    }
}

[HarmonyPatch(typeof(CameraFade), "FadeOut", new System.Type[] { typeof(float), typeof(float), typeof(Il2CppSystem.Action) })]
internal static class Patch_CameraFade_FadeOut
{
    private static void Prefix(ref float time, ref float delay)
    {
        if (FadeSuppressionWindow.IsActive) { time = 0f; delay = 0f; }
    }
}

[HarmonyPatch(typeof(CameraFade), "FadeIn", new System.Type[] { typeof(float), typeof(float), typeof(Il2CppSystem.Action) })]
internal static class Patch_CameraFade_FadeIn
{
    private static void Prefix(ref float time, ref float delay)
    {
        if (FadeSuppressionWindow.IsActive) { time = 0f; delay = 0f; }
    }
}

[HarmonyPatch(typeof(CameraFade), "FadeTo", new System.Type[] { typeof(float), typeof(float), typeof(float), typeof(Il2CppSystem.Action) })]
internal static class Patch_CameraFade_FadeTo
{
    private static void Prefix(ref float time, ref float delay)
    {
        if (FadeSuppressionWindow.IsActive) { time = 0f; delay = 0f; }
    }
}

[HarmonyPatch(typeof(CameraFade), "Fade", new System.Type[] { typeof(float), typeof(float), typeof(float), typeof(float), typeof(Il2CppSystem.Action) })]
internal static class Patch_CameraFade_Fade
{
    private static void Prefix(ref float time, ref float delay)
    {
        if (FadeSuppressionWindow.IsActive) { time = 0f; delay = 0f; }
    }
}

// v2.7.30 重新加回 TimeOfDay.Accelerate Prefix,但只在 QuickCraft 开时改 gameTimeHours=0
//   根因:v2.7.29 只把 craft time=1 秒,但 Accelerate 的 gameTimeHours 原值 8h 仍推进游戏时钟
//   现在 QuickCraft 时 gameTimeHours=0,配合 craft time=1 秒(GetFinalCraftingTime Postfix)
//   → 单个制作 1 秒真实时间完成,游戏时钟不跳,不黑屏
//   batch 不受影响 —— Panel_Crafting.Update 按 craft time 推进 percent,不依赖 gameTimeHours
// 注意:QuickAction(采集/修理/拆解) 不吃 Accelerate,避免 v2.7.26 "关了 QuickCraft 还不消耗时间"的误伤
// v2.7.33 —— 撤销 TimeOfDay.Accelerate Prefix(Panel_Crafting 可能不走 Accelerate 推进时钟)
// 改用 TimeOfDay.SetTODLocked 直接冻结整个游戏时钟:
//   CraftingStart Postfix → SetTODLocked(true)
//   CraftingEnd Postfix → SetTODLocked(false)  (整个 batch 结束才解锁)
// 用户接受黑屏,doFadeToBlack 不再拦(保持游戏默认行为)

// ——— 一击必杀:任何命中动物的伤害都放大到 9999 ———
// 用户要的是"我打它一下它就死",不是"开关一开所有动物全死"
// 4 参虚方法 ApplyDamage(damage, bleedOutMinutes, DamageSource, collider) 指定签名避免歧义
[HarmonyPatch(typeof(BaseAi), "ApplyDamage",
    new System.Type[] { typeof(float), typeof(float), typeof(DamageSource), typeof(string) })]
internal static class Patch_BaseAi_ApplyDamage
{
    private static void Prefix(ref float damage)
    {
        if (CheatState.InstantKillAnimals && damage > 0f) damage = 9999f;
    }
}

// ——— 免费制作:强制返回 "能造",跳过材料检查 ———
[HarmonyPatch(typeof(Panel_Crafting), "CanCraftSelectedBlueprint")]
internal static class Patch_Craft_CanCraftSelected
{
    private static void Postfix(ref bool __result)
    {
        if (CheatState.FreeCraft) __result = true;
    }
}

[HarmonyPatch(typeof(Panel_Crafting), "CanCraftBlueprint")]
internal static class Patch_Craft_CanCraft
{
    private static void Postfix(ref bool __result)
    {
        if (CheatState.FreeCraft) __result = true;
    }
}

// ——— 快速制作 v2.7.29:简化为 time=1 策略,完全移除循环递归护栏 ———
// v2.7.27 的 static _busy 护栏有 bug:OnCraftingSuccess 递归触发 CraftingStart 时内层 return
// 跳过 batch 第 2+ 个 item。改成纯粹 "每个 craft time 归 1 秒" 让游戏自己跑 batch:
//   GetFinalCraftingTimeWithAllModifiers / GetAdjustedCraftingTime Postfix = 1
//   → 游戏 Update 读 craftTime=1 → 第 1 个 1 秒完成 → 自动跑第 2 个 1 秒 → ...
//   batch 10 个 = 10 秒,无黑屏,游戏时钟跳动极少
// 不再 patch CraftingStart Postfix —— 让游戏流程自洽推进
// v2.7.40 按用户思路重写:撤销 craft time=1 (那让进度卡 3%)
//   保留原 craft time (比如 10h),但加速 TOD 流速让真实秒走几小时游戏时间
//   craft 完成瞬间 TOD 拉回 → 用户看不到跳
// GetFinalCraftingTime / GetAdjustedCraftingTime 不再 patch,游戏看到正常 craft 时长

// v2.7.43/44 QuickCraft 完整复刻 CT 方案 —— 在 CraftingOperation.Update 开头设字段
[HarmonyPatch(typeof(CraftingOperation), "Update")]
internal static class Patch_CraftingOp_Update
{
    private static void Prefix(CraftingOperation __instance)
    {
        if (!CheatState.QuickCraft) return;
        try
        {
            __instance.m_RealTimeDuration = 0.2f;
            __instance.m_HoursToSpendCrafting = 100f;
            FadeSuppressionWindow.Arm(1f);  // v2.7.44 craft 期间吃 fade
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[QuickCraft] {ex.Message}"); }
    }
}

// v2.7.44 —— 修 "做完屏幕暗一点" 残留 fade
//   craft 完成后 CameraFade 可能留在 alpha > 0,强制 FadeIn(0,0) 瞬亮
[HarmonyPatch(typeof(Panel_Crafting), "CraftingEnd")]
internal static class Patch_Craft_End_ForceBright
{
    private static void Postfix()
    {
        if (!CheatState.QuickCraft) return;
        try
        {
            CameraFade.FadeIn(0f, 0f, null);  // 瞬 fade in,强制亮
        }
        catch { }
    }
}

// OnCraftingSuccess 每个 item 完成时调,继续 Arm fade 但不解锁(batch 里还有剩余)
[HarmonyPatch(typeof(Panel_Crafting), "OnCraftingSuccess")]
internal static class Patch_Craft_OnSuccess_ArmFade
{
    private static void Postfix()
    {
        if (CheatState.QuickCraft) FadeSuppressionWindow.Arm(3f);
    }
}

// ——— 隐身感知切断:CanSeeTarget / ScanForSmells ———
// v2.7.23 —— TrueInvisible 语义恢复为"全动物忽略玩家"(狼熊不追/兔鹿不逃)
//   Stealth 仍保持原语义(SetAiMode(Flee) 强制威胁动物逃跑,但兔鹿也会跟着逃,所以 Stealth 是"吓跑模式")
//   TrueInvisible 靠感知层切断 → AI 根本感知不到玩家 → 所有动物当玩家不存在
[HarmonyPatch(typeof(BaseAi), "CanSeeTarget")]
internal static class Patch_BaseAi_CanSeeTarget
{
    private static void Postfix(ref bool __result)
    {
        if (CheatState.Stealth || CheatState.TrueInvisible) __result = false;
    }
}

[HarmonyPatch(typeof(BaseAi), "ScanForSmells")]
internal static class Patch_BaseAi_ScanForSmells
{
    private static bool Prefix() => !(CheatState.Stealth || CheatState.TrueInvisible);
}

// v2.7.29 —— BaseAi 注册式:Start Postfix 加入 HashSet + OnDisable Prefix 移除
//   替代 TickAnimalsFull 每 N 秒 FindObjectsOfType<BaseAi> 的高开销(大地图 200+ AI)
//   Start Postfix 在新 AI 出生时 (1) 注册到 _knownAis (2) 若 TrueInvisible 开则一次性设字段
internal static class BaseAiRegistry
{
    public static readonly System.Collections.Generic.HashSet<BaseAi> Known = new();

    public static void ApplyInvisibleFields(BaseAi ai)
    {
        try
        {
            ai.m_DisableScanForTargets = true;
            ai.m_DetectionRange = 0f;
            ai.m_DetectionFOV = 0f;
            ai.m_HearFootstepsRange = 0f;
            ai.m_HearRifleRange = 0f;
            ai.m_HearCarAlarmRange = 0f;
            ai.m_SmellRange = 0f;
            ai.m_DetectionRangeWhileFeeding = 0f;
            ai.m_HearFootstepsRangeWhileFeeding = 0f;
            ai.m_HearFootstepsRangeWhileSleeping = 0f;
        }
        catch (System.Exception ex) { ModMain.Log?.Warning($"[TrueInvisible] field set failed: {ex.Message}"); }
    }
}

[HarmonyPatch(typeof(BaseAi), "Start")]
internal static class Patch_BaseAi_Start_Register
{
    private static void Postfix(BaseAi __instance)
    {
        if (__instance == null) return;
        BaseAiRegistry.Known.Add(__instance);
        if (CheatState.TrueInvisible) BaseAiRegistry.ApplyInvisibleFields(__instance);
    }
}

[HarmonyPatch(typeof(BaseAi), "OnDisable")]
internal static class Patch_BaseAi_OnDisable_Unregister
{
    private static void Prefix(BaseAi __instance)
    {
        if (__instance != null) BaseAiRegistry.Known.Remove(__instance);
    }
}

// ——— 忽略上锁(只保留 IsLocked,其余 patch 去掉 —— 2.7.1 启动卡死嫌疑)———
[HarmonyPatch(typeof(Lock), "IsLocked")]
internal static class Patch_Lock_IsLocked
{
    private static void Postfix(ref bool __result)
    {
        if (CheatState.IgnoreLock) __result = false;
    }
}
// Lock.RequiresToolToUnlock + PlayerHasRequiredToolToUnlock, Breath.GetBreathTimePercent,
// Encumber.IsEncumbered + GetEncumbranceSlowdownMultiplier 这 5 个 patch 去掉 ——
// 前者因启动卡死嫌疑,后者因为对应功能(IC/IS)已去除(交给 UniversalTweaks 等 mod)

// ——— 衣物不潮湿:拦源头 IncreaseWetnessPercent 和 MaybeGetWetOnGround + Update 兜底 ———
[HarmonyPatch(typeof(ClothingItem), "IncreaseWetnessPercent", new System.Type[] { typeof(float) })]
internal static class Patch_Clothing_IncreaseWet
{
    private static bool Prefix() => !CheatState.NoWetClothes;
}

[HarmonyPatch(typeof(ClothingItem), "MaybeGetWetOnGround", new System.Type[] { typeof(float) })]
internal static class Patch_Clothing_GetWetOnGround
{
    private static bool Prefix() => !CheatState.NoWetClothes;
}

// v2.7.18:删了 ClothingItem.Update Postfix —— 每帧每件 Harmony bridge 是主要 FPS 杀手
// 只靠 2 个 Prefix(IncreaseWetnessPercent + MaybeGetWetOnGround)+ TickClothingWetness(低频)
// 如果还漏,再加另一个 Prefix 拦源头,不要 Update Postfix

// ——— 冰面不破:冰面破裂触发 / 落水 直接跳过 ———
[HarmonyPatch(typeof(IceCrackingTrigger), "BreakIce")]
internal static class Patch_IceBreak_BreakIce
{
    private static bool Prefix() => !CheatState.ThinIceNoBreak;
}

[HarmonyPatch(typeof(IceCrackingTrigger), "FallInWater")]
internal static class Patch_IceBreak_FallInWater
{
    private static bool Prefix() => !CheatState.ThinIceNoBreak;
}

// ——— 生火 100% 成功 ———
// v2.7.31 修:CalculateFireStartSuccess 返回的是 0-100 的 percent(不是 0-1 概率)
//   之前设 1f 实际 = 1% 成功率 —— 用户报告 "开 toggle 只有 1% 概率"
[HarmonyPatch(typeof(FireManager), "CalculateFireStartSuccess")]
internal static class Patch_FireMgr_Success
{
    private static void Postfix(ref float __result)
    {
        if (CheatState.QuickFire) __result = 100f;
    }
}

// ——— 关闭瞄准景深(DOF)—— 拦截 EnableCameraWeaponPostEffects(true),强制传 false ———
// v2.7.29:参数名改 __0 避免 TLD 更新改参数名后 Harmony silently 失配
[HarmonyPatch(typeof(CameraEffects), "EnableCameraWeaponPostEffects")]
internal static class Patch_CamEffects_WeaponPost
{
    private static void Prefix(ref bool __0)
    {
        if (CheatState.NoAimDOF) __0 = false;
    }
}

// ——— 快速开容器 ——
// v2.7.13:原 Patch_Container_Enable Postfix 覆盖字段不够 —— 改 EnableAfterDelay Prefix 拦源头
[HarmonyPatch(typeof(Panel_Container), "EnableAfterDelay", new System.Type[] { typeof(float) })]
internal static class Patch_Container_EnableAfterDelay
{
    private static void Prefix(ref float delaySeconds)
    {
        if (CheatState.QuickOpenContainer) delaySeconds = 0f;
    }
}

// 兜底:Enable(bool,bool,Action) Postfix 仍强制 elapsed=999 让任何残留 delay 立刻完成
[HarmonyPatch(typeof(Panel_Container), "Enable", new System.Type[] { typeof(bool), typeof(bool), typeof(Il2CppSystem.Action) })]
internal static class Patch_Container_Enable
{
    private static void Postfix(Panel_Container __instance)
    {
        if (!CheatState.QuickOpenContainer) return;
        try
        {
            __instance.m_EnableDelaySeconds = 0f;
            __instance.m_EnableDelayElapsed = 999f;
        }
        catch { }
    }
}


// ——————————— Tick-based 补丁 ———————————
// OnUpdate 每帧/每 N 帧扫一次实例,直接改字段。
internal static class CheatsTick
{
    // ——— 武器:无限弹药 / 永不卡壳 / 快速射击 / 无后坐(字段层 fallback)———
    public static void TickGuns()
    {
        if (!CheatState.InfiniteAmmo && !CheatState.NoJam && !CheatState.FastFire && !CheatState.NoRecoil) return;
        try
        {
            if (CheatState.NoJam)
            {
                try { GunItem.m_ForceNoJam = true; } catch { }
            }

            var guns = UnityEngine.Object.FindObjectsOfType<GunItem>();
            if (guns == null) return;
            foreach (var g in guns)
            {
                if (g == null) continue;
                try
                {
                    if (CheatState.NoJam)
                    {
                        try { g.m_IsJammed = false; } catch { }
                    }
                    if (CheatState.InfiniteAmmo && g.m_ClipSize > 0)
                    {
                        if (g.m_RoundsInClip < g.m_ClipSize)
                            g.m_RoundsInClip = g.m_ClipSize;
                    }
                    if (CheatState.FastFire)
                    {
                        // 强制覆盖 —— 不要 if 守卫,游戏可能在 weapon 切换时重置为 prefab 值
                        // 注意:m_FiringRateSeconds = 0 会卡死 state machine,0.05 是安全最小(20/秒)
                        try { g.m_FiringRateSeconds     = 0.05f; } catch { }
                        try { g.m_FireDelayOnAim        = 0f;    } catch { }
                        try { g.m_FireDelayAfterReload  = 0f;    } catch { }
                    }
                    if (CheatState.NoRecoil)
                    {
                        try { g.m_PitchRecoilMin = 0f; } catch { }
                        try { g.m_PitchRecoilMax = 0f; } catch { }
                        try { g.m_YawRecoilMin = 0f; } catch { }
                        try { g.m_YawRecoilMax = 0f; } catch { }
                        // 其 sway 也顺带归零,aim 稳定
                        try { g.m_SwayValueZeroFatigue = 0f; } catch { }
                        try { g.m_SwayValueMaxFatigue = 0f; } catch { }
                    }
                    if (CheatState.NoAimSway)
                    {
                        try { g.m_SwayValueZeroFatigue = 0f; } catch { }
                        try { g.m_SwayValueMaxFatigue = 0f; } catch { }
                        try { g.m_SwayValue = 0f; } catch { }
                    }
                }
                catch { }
            }
        }
        catch { }
    }

    // ——— 摄像机:NoRecoil / NoAimSway / NoAimShake / NoBreathSway / NoAimStamina ———
    // v2.7.2 优化:(1) 所有 FieldInfo lazy 缓存到 static,不再每次 GetField;
    //             (2) 有任一 toggle ON 才跑 full path,全关且上次也关 → 早退 0 开销;
    //             (3) 从每帧改成 ModMain 里调用 —— 调用频率由 ModMain 决定(30 帧 = 0.5s)
    private static bool _lastAnyAimToggle = false;
    // Cached reflection members (lazy init on first call)
    private static FieldInfo _fi_currentWeapon, _fi_recoilSpring;
    private static FieldInfo[] _fi_camFloats;   // 对应 ShakeAmplitude / ShakeSpeed / BobAmplitude 等
    private static FieldInfo[] _fi_camSwayFloats; // camera sway 字段
    private static FieldInfo _fi_rsCur, _fi_rsTgt, _fi_rsVel;
    private static FieldInfo _fi_weapShake, _fi_weapShakeSpeed, _fi_weapCold, _fi_weapRandom, _fi_weapShakeInst;
    private static FieldInfo _fi_weapBob, _fi_weapBobRate;
    private static FieldInfo _fi_weapSwayLim, _fi_weapSwayMax, _fi_weapSwayStart, _fi_weapSwayCrouch, _fi_weapSwayMotion;
    private static FieldInfo _fi_weapRotLook, _fi_weapRotStrafe, _fi_weapRotFall, _fi_weapRotSlope;
    private static FieldInfo _fi_weapDisSway, _fi_weapDisShake, _fi_weapDisBreath, _fi_weapDisStam;
    private static bool _reflectionInited = false;
    // v2.7.29:按武器类型缓存 FieldInfo。之前用 Rifle 的 FieldInfo SetValue 到 Bow 实例 → ArgumentException
    private static System.Type _cachedWeaponType = null;

    private static void EnsureReflectionInited(object weapon)
    {
        if (_reflectionInited && weapon == null) return;
        // v2.7.29:武器类型切换(Rifle ↔ Bow ↔ Revolver),全部 weapon field 要重新绑
        if (weapon != null && weapon.GetType() != _cachedWeaponType)
        {
            _fi_weapDisSway = _fi_weapDisShake = _fi_weapDisBreath = _fi_weapDisStam = null;
            _fi_weapShake = _fi_weapShakeSpeed = _fi_weapCold = _fi_weapRandom = _fi_weapShakeInst = null;
            _fi_weapBob = _fi_weapBobRate = null;
            _fi_weapSwayLim = _fi_weapSwayMax = _fi_weapSwayStart = _fi_weapSwayCrouch = _fi_weapSwayMotion = null;
            _fi_weapRotLook = _fi_weapRotStrafe = _fi_weapRotFall = _fi_weapRotSlope = null;
            _cachedWeaponType = weapon.GetType();
        }
        var camT = typeof(vp_FPSCamera);
        _fi_currentWeapon = _fi_currentWeapon ?? camT.GetField("m_CurrentWeapon", BindingFlags.Instance | BindingFlags.Public);
        _fi_recoilSpring  = _fi_recoilSpring  ?? camT.GetField("m_RecoilSpring",  BindingFlags.Instance | BindingFlags.Public);
        _fi_camFloats ??= new[] {
            camT.GetField("ShakeAmplitude", BindingFlags.Instance | BindingFlags.Public),
            camT.GetField("ShakeSpeed",     BindingFlags.Instance | BindingFlags.Public),
            camT.GetField("BobAmplitude",   BindingFlags.Instance | BindingFlags.Public),
        };
        _fi_camSwayFloats ??= new[] {
            camT.GetField("m_MaxAmbientSwayAngleDegreesA",        BindingFlags.Instance | BindingFlags.Public),
            camT.GetField("m_MaxAmbientAimingSwayAngleDegreesA",  BindingFlags.Instance | BindingFlags.Public),
            camT.GetField("m_AmbientSwaySpeedA",                   BindingFlags.Instance | BindingFlags.Public),
            camT.GetField("m_AmbientAimingSwaySpeedA",             BindingFlags.Instance | BindingFlags.Public),
            camT.GetField("m_CurrentMaxAmbientSwayAngle",          BindingFlags.Instance | BindingFlags.Public),
            camT.GetField("m_CurrentAmbientSwaySpeed",             BindingFlags.Instance | BindingFlags.Public),
        };
        // RecoilSpring struct 字段
        if (_fi_recoilSpring != null)
        {
            var rsT = _fi_recoilSpring.FieldType;
            _fi_rsCur = _fi_rsCur ?? rsT.GetField("m_Current",  BindingFlags.Instance | BindingFlags.Public);
            _fi_rsTgt = _fi_rsTgt ?? rsT.GetField("m_Target",   BindingFlags.Instance | BindingFlags.Public);
            _fi_rsVel = _fi_rsVel ?? rsT.GetField("m_Velocity", BindingFlags.Instance | BindingFlags.Public);
        }
        // 武器字段(需要一个 weapon 实例才能拿 Type)
        if (weapon != null && _fi_weapDisSway == null)
        {
            var wt = weapon.GetType();
            _fi_weapDisSway   = wt.GetField("m_DisableAimSway",      BindingFlags.Instance | BindingFlags.Public);
            _fi_weapDisShake  = wt.GetField("m_DisableAimShake",     BindingFlags.Instance | BindingFlags.Public);
            _fi_weapDisBreath = wt.GetField("m_DisableAimBreathing", BindingFlags.Instance | BindingFlags.Public);
            _fi_weapDisStam   = wt.GetField("m_DisableAimStamina",   BindingFlags.Instance | BindingFlags.Public);
            _fi_weapShake      = wt.GetField("ShakeAmplitude",        BindingFlags.Instance | BindingFlags.Public);
            _fi_weapShakeSpeed = wt.GetField("ShakeSpeed",            BindingFlags.Instance | BindingFlags.Public);
            _fi_weapCold       = wt.GetField("m_ColdShakeAngle",      BindingFlags.Instance | BindingFlags.Public);
            _fi_weapRandom     = wt.GetField("m_RandomShakeAngle",    BindingFlags.Instance | BindingFlags.Public);
            _fi_weapShakeInst  = wt.GetField("m_Shake",               BindingFlags.Instance | BindingFlags.Public);
            _fi_weapBob        = wt.GetField("BobAmplitude",          BindingFlags.Instance | BindingFlags.Public);
            _fi_weapBobRate    = wt.GetField("BobRate",               BindingFlags.Instance | BindingFlags.Public);
            _fi_weapSwayLim    = wt.GetField("SwayLimits",            BindingFlags.Instance | BindingFlags.Public);
            _fi_weapSwayMax    = wt.GetField("SwayMaxFatigue",        BindingFlags.Instance | BindingFlags.Public);
            _fi_weapSwayStart  = wt.GetField("SwayStartFatigue",      BindingFlags.Instance | BindingFlags.Public);
            _fi_weapSwayCrouch = wt.GetField("SwayCrouchScalar",      BindingFlags.Instance | BindingFlags.Public);
            _fi_weapSwayMotion = wt.GetField("SwayMotionSpeed",       BindingFlags.Instance | BindingFlags.Public);
            // Rotation*Sway —— 移动/下蹲/跌倒/侧移时枪的摆动(最关键!)
            _fi_weapRotLook    = wt.GetField("RotationLookSway",      BindingFlags.Instance | BindingFlags.Public);
            _fi_weapRotStrafe  = wt.GetField("RotationStrafeSway",    BindingFlags.Instance | BindingFlags.Public);
            _fi_weapRotFall    = wt.GetField("RotationFallSway",      BindingFlags.Instance | BindingFlags.Public);
            _fi_weapRotSlope   = wt.GetField("RotationSlopeSway",     BindingFlags.Instance | BindingFlags.Public);
        }
        _reflectionInited = true;
    }

    private static void SetFloatIfNotNull(FieldInfo f, object inst, float v)
    {
        if (f == null || inst == null) return;
        try { f.SetValue(inst, v); } catch { }
    }
    private static void SetBoolIfNotNull(FieldInfo f, object inst, bool v)
    {
        if (f == null || inst == null) return;
        try { f.SetValue(inst, v); } catch { }
    }

    public static void TickCamera()
    {
        bool anyOn = CheatState.NoAimSway || CheatState.NoAimShake || CheatState.NoBreathSway
                  || CheatState.NoAimStamina || CheatState.NoRecoil;
        // 全关 AND 上次也全关 → 零开销早退
        if (!anyOn && !_lastAnyAimToggle) return;

        try
        {
            var cam = GameManager.GetVpFPSCamera();
            if (cam == null) return;

            object weapon = null;
            EnsureReflectionInited(null);
            if (_fi_currentWeapon != null) { try { weapon = _fi_currentWeapon.GetValue(cam); } catch { } }
            if (weapon != null) EnsureReflectionInited(weapon);

            // bool 开关双向同步(包括 toggle 关后恢复)
            try { vp_FPSCamera.m_DisableAmbientSway = CheatState.NoAimSway; } catch { }
            SetBoolIfNotNull(_fi_weapDisSway,   weapon, CheatState.NoAimSway);
            SetBoolIfNotNull(_fi_weapDisShake,  weapon, CheatState.NoAimShake);
            SetBoolIfNotNull(_fi_weapDisBreath, weapon, CheatState.NoBreathSway);
            SetBoolIfNotNull(_fi_weapDisStam,   weapon, CheatState.NoAimStamina);

            // 浮点字段:只在 toggle 开时归零(关掉后不可逆)
            if (!anyOn)
            {
                _lastAnyAimToggle = false;
                return;
            }

            if (CheatState.NoAimSway)
            {
                // m_Max* / m_Ambient* 6 个 camera sway 字段
                for (int i = 0; i < _fi_camSwayFloats.Length; i++)
                    SetFloatIfNotNull(_fi_camSwayFloats[i], cam, 0f);
                // Weapon sway:SwayLimits/Max/Start/Crouch/Motion + 4 个 Rotation*Sway
                SetFloatIfNotNull(_fi_weapSwayLim,    weapon, 0f);
                SetFloatIfNotNull(_fi_weapSwayMax,    weapon, 0f);
                SetFloatIfNotNull(_fi_weapSwayStart,  weapon, 0f);
                SetFloatIfNotNull(_fi_weapSwayCrouch, weapon, 0f);
                SetFloatIfNotNull(_fi_weapSwayMotion, weapon, 0f);
                SetFloatIfNotNull(_fi_weapRotLook,    weapon, 0f);
                SetFloatIfNotNull(_fi_weapRotStrafe,  weapon, 0f);
                SetFloatIfNotNull(_fi_weapRotFall,    weapon, 0f);
                SetFloatIfNotNull(_fi_weapRotSlope,   weapon, 0f);
            }
            if (CheatState.NoAimShake)
            {
                // Camera shake
                SetFloatIfNotNull(_fi_camFloats[0], cam, 0f); // ShakeAmplitude
                SetFloatIfNotNull(_fi_camFloats[1], cam, 0f); // ShakeSpeed
                // Weapon shake
                SetFloatIfNotNull(_fi_weapShake,      weapon, 0f);
                SetFloatIfNotNull(_fi_weapShakeSpeed, weapon, 0f);
                SetFloatIfNotNull(_fi_weapCold,       weapon, 0f);
                SetFloatIfNotNull(_fi_weapRandom,     weapon, 0f);
                SetFloatIfNotNull(_fi_weapShakeInst,  weapon, 0f);
            }
            if (CheatState.NoBreathSway)
            {
                SetFloatIfNotNull(_fi_camFloats[2], cam, 0f);    // Camera BobAmplitude
                SetFloatIfNotNull(_fi_weapBob,      weapon, 0f); // Weapon BobAmplitude
                SetFloatIfNotNull(_fi_weapBobRate,  weapon, 0f); // Weapon BobRate
            }

            if (CheatState.NoRecoil && _fi_recoilSpring != null)
            {
                try
                {
                    var rs = _fi_recoilSpring.GetValue(cam);
                    if (rs != null)
                    {
                        SetFloatIfNotNull(_fi_rsCur, rs, 0f);
                        SetFloatIfNotNull(_fi_rsTgt, rs, 0f);
                        SetFloatIfNotNull(_fi_rsVel, rs, 0f);
                        _fi_recoilSpring.SetValue(cam, rs); // struct 复制回去
                    }
                }
                catch { }
            }

            _lastAnyAimToggle = true;
        }
        catch { }
    }

    // (旧的 TrySetFieldFloat / TrySetFieldBool / SetStructFieldFloat helper 已被缓存版
    //  FieldInfo 替换,删除。)

    // ——— 动物不能动 / 隐身 ———
    // v2.7.9 重写 Stealth:之前 m_DisableScanForTargets 单靠它不够 ——
    // 游戏内部的检测路径还有别的入口。现在强力方案:
    //   1) 设 m_DisableScanForTargets = true(阻止新增探测)
    //   2) 对已经在 Attack/Stalking/Investigate/HoldGround 的 AI,立即 SetAiMode(Wander)
    //      —— 公开方法,可靠;等于把玩家从威胁列表踢出
    private static System.Reflection.FieldInfo _fi_baseAi_disableScan;
    // v2.7.13 TrueInvisible 强化:把 AI 的探测/听觉 range 都归零,堵所有感知通道
    private static System.Reflection.FieldInfo _fi_baseAi_detectRange, _fi_baseAi_detectFOV;
    private static System.Reflection.FieldInfo _fi_baseAi_hearFootsteps, _fi_baseAi_hearRifle;
    private static System.Reflection.FieldInfo _fi_baseAi_detectRangeFeeding;
    private static bool _lastTickedStealth = false;
    private static bool _lastTickedFreeze = false;
    private static bool _lastTickedInvis = false;

    // v2.7.24 TrueInvisible cheap:每 1s 设玩家 AiTarget.m_IsEnabled + ClearTarget 所有 AI
    //   关玩家 AiTarget 让世界查询 FindAiTargets 拿不到玩家;清 AI 现有 target 避免已 target 的残留
    public static void TickAnimalsCheap()
    {
        if (!CheatState.TrueInvisible && !_lastTickedInvis) return;
        try
        {
            var pm = GameManager.GetPlayerManagerComponent();
            if (pm != null && pm.m_AiTarget != null)
                pm.m_AiTarget.m_IsEnabled = !CheatState.TrueInvisible;
        }
        catch { }
        _lastTickedInvis = CheatState.TrueInvisible;
    }

    public static void TickAnimalsFull() => TickAnimals();

    public static void TickAnimals()
    {
        bool runStealth = CheatState.Stealth || _lastTickedStealth;
        bool runFreeze  = CheatState.FreezeAnimals || _lastTickedFreeze;
        bool runInvis   = CheatState.TrueInvisible || _lastTickedInvis;
        if (!runStealth && !runFreeze && !runInvis) return;

        try
        {
            // v2.7.29:改用注册表(HashSet)而不是 FindObjectsOfType<BaseAi>
            //   大地图 200+ AI 的 FindObjects 要 ~20-50ms,HashSet iterate 只几百纳秒
            //   HashSet 维护:BaseAi.Start Postfix 加入,OnDisable Prefix 移除
            var ais = BaseAiRegistry.Known;
            if (ais.Count == 0)
            {
                _lastTickedStealth = CheatState.Stealth;
                _lastTickedFreeze  = CheatState.FreezeAnimals;
                _lastTickedInvis   = CheatState.TrueInvisible;
                return;
            }

            // v2.7.29 兜底:Il2Cpp wrapper == null 判断不可靠,还要检查 Pointer
            System.Collections.Generic.List<BaseAi> stale = null;
            foreach (var ai in ais)
            {
                if (ai == null || ai.Pointer == System.IntPtr.Zero)
                {
                    (stale ??= new()).Add(ai);
                    continue;
                }
                try
                {
                    if (runFreeze)
                    {
                        try { ai.m_SpeedForPathfindingOverride = CheatState.FreezeAnimals; } catch { }
                        if (CheatState.FreezeAnimals)
                            try { ai.m_OverrideSpeed = 0f; } catch { }
                    }

                    // v2.7.24 TrueInvisible 核心修:用强类型属性直接访问(Il2CppInterop 下 m_X 是 property 不是 field,反射无效)
                    // 切断所有感知通道 —— AI 像瞎子聋子,闻不到看不见听不见,加清现有 target 踢出残留
                    if (runInvis)
                    {
                        try { ai.m_DisableScanForTargets = CheatState.TrueInvisible; } catch { }
                        if (CheatState.TrueInvisible)
                        {
                            try { ai.ClearTarget(); } catch { }
                            try { ai.m_DetectionRange = 0f; } catch { }
                            try { ai.m_DetectionFOV = 0f; } catch { }
                            try { ai.m_HearFootstepsRange = 0f; } catch { }
                            try { ai.m_HearRifleRange = 0f; } catch { }
                            try { ai.m_HearCarAlarmRange = 0f; } catch { }
                            try { ai.m_SmellRange = 0f; } catch { }
                            try { ai.m_DetectionRangeWhileFeeding = 0f; } catch { }
                            try { ai.m_HearFootstepsRangeWhileFeeding = 0f; } catch { }
                            try { ai.m_HearFootstepsRangeWhileSleeping = 0f; } catch { }
                        }
                    }

                    // Stealth 主力:看到动物就强制切 Flee(逃跑)
                    // (Stealth 和 TrueInvisible 互斥使用建议:TrueInvisible = 全忽略;Stealth = 全吓跑)
                    if (CheatState.Stealth)
                    {
                        try
                        {
                            var mode = ai.GetAiMode();
                            if (mode != AiMode.Flee
                                && mode != AiMode.Dead
                                && mode != AiMode.Sleep
                                && mode != AiMode.Struggle
                                && mode != AiMode.Stunned
                                && mode != AiMode.ScriptedSequence)
                            {
                                try { ai.ClearTarget(); } catch { }
                                ai.SetAiMode(AiMode.Flee);
                            }
                        }
                        catch { }
                    }
                }
                catch { }
            }

            // v2.7.29:清 stale AI(场景切换时可能遗留的 disposed wrapper)
            if (stale != null) foreach (var s in stale) ais.Remove(s);

            _lastTickedStealth = CheatState.Stealth;
            _lastTickedFreeze  = CheatState.FreezeAnimals;
            _lastTickedInvis   = CheatState.TrueInvisible;
        }
        catch { }
    }

    // ——— 忽略上锁:v2.7.8 修 —— 不能用 lk.IsLocked() 判(被 Patch_Lock_IsLocked 篡返回 false)
    // 改直接读 m_LockState 字段
    private static System.Reflection.MethodInfo _mi_lock_setLockState;
    private static System.Reflection.FieldInfo _fi_lock_state;
    public static void TickLocks()
    {
        if (!CheatState.IgnoreLock) return;
        try
        {
            var locks = UnityEngine.Object.FindObjectsOfType<Lock>();
            if (locks == null) return;

            if (_mi_lock_setLockState == null)
                _mi_lock_setLockState = typeof(Lock).GetMethod("SetLockState", BindingFlags.Instance | BindingFlags.Public);
            if (_fi_lock_state == null)
                _fi_lock_state = typeof(Lock).GetField("m_LockState", BindingFlags.Instance | BindingFlags.Public);
            if (_mi_lock_setLockState == null || _fi_lock_state == null) return;

            foreach (var lk in locks)
            {
                if (lk == null) continue;
                try
                {
                    // 直接读字段,绕过 Patch_Lock_IsLocked 的 Postfix 篡改
                    var state = _fi_lock_state.GetValue(lk);
                    if (state != null && (int)state == (int)LockState.Locked)
                        _mi_lock_setLockState.Invoke(lk, new object[] { LockState.Unlocked });
                }
                catch { }
            }
        }
        catch { }
    }

    // 火焰无限时长已移除 —— 用户说其他 mod(如 InfiniteFiresDLC)覆盖

    // ——— 爬绳加速 ———
    // 直接设固定值 5.0(原值 0.3~1.5 左右),幂等:多次 tick 不会叠加。
    // 注意:无法在 toggle 关掉后还原原值(原值没记录),要等场景重载。
    public static void TickClimbRope()
    {
        if (!CheatState.QuickClimb) return;
        try
        {
            var all = UnityEngine.Object.FindObjectsOfType<PlayerClimbRope>();
            if (all == null) return;
            foreach (var p in all)
            {
                if (p == null) continue;
                try
                {
                    var t = typeof(PlayerClimbRope);
                    foreach (var fn in new[] { "m_ClimbSpeed", "m_ClimbSpeedUpFullyRested", "m_ClimbSpeedUpExhausted",
                                               "m_ClimbSpeedDownFullyRested", "m_ClimbSpeedDownExhausted" })
                    {
                        var f = t.GetField(fn, BindingFlags.Instance | BindingFlags.Public);
                        if (f != null && f.FieldType == typeof(float))
                        {
                            float cur = (float)f.GetValue(p);
                            if (cur > 0f && cur < 5f) f.SetValue(p, 5f); // 幂等:设到 5 就不动了
                        }
                    }
                }
                catch { }
            }
        }
        catch { }
    }

    // ——— 无饥饿 / 无口渴 / 无疲劳 / 始终温暖 / GodMode 兜底 ———
    // (InfiniteStamina / InfiniteCarry 已去除,交给 UniversalTweaks 等 mod 处理)
    public static void TickStatus()
    {
        if (!CheatState.NoFatigue && !CheatState.NoHunger
            && !CheatState.NoThirst && !CheatState.AlwaysWarm && !CheatState.GodMode) return;

        try
        {
            if (CheatState.NoFatigue || CheatState.GodMode)
            {
                var fat = GameManager.GetFatigueComponent();
                if (fat != null) { try { fat.m_CurrentFatigue = 0f; } catch { } }
            }
            if (CheatState.NoHunger || CheatState.GodMode)
            {
                var h = GameManager.GetHungerComponent();
                if (h != null) { try { h.m_CurrentReserveCalories = h.m_MaxReserveCalories; } catch { } }
            }
            if (CheatState.NoThirst || CheatState.GodMode)
            {
                var t = GameManager.GetThirstComponent();
                if (t != null) { try { t.m_CurrentThirst = 0f; } catch { } }
            }
            if (CheatState.AlwaysWarm || CheatState.GodMode)
            {
                var f = GameManager.GetFreezingComponent();
                if (f != null) { try { f.m_CurrentFreezing = 0f; } catch { } }
            }
            if (CheatState.GodMode)
            {
                var c = GameManager.GetConditionComponent();
                if (c != null) { try { c.m_CurrentHP = c.m_MaxHP; } catch { } }
            }
        }
        catch { }
    }

    // ——— 节约时间:拆解(BreakDown 没有 StartXxx Prefix 路径,改 tick 设 m_SecondsToBreakDown=0)
    // Harvest / Repair 走 Patch_Harvest_StartHarvest / Patch_Repair_StartRepair 这些 Prefix 搞定
    private static FieldInfo _fi_breakdownSecs;
    public static void TickQuickActions()
    {
        if (!CheatState.QuickAction) return;
        try
        {
            if (_fi_breakdownSecs == null)
                _fi_breakdownSecs = typeof(Panel_BreakDown).GetField("m_SecondsToBreakDown",
                    BindingFlags.Instance | BindingFlags.Public);
            if (_fi_breakdownSecs == null) return;

            foreach (var p in UnityEngine.Object.FindObjectsOfType<Panel_BreakDown>())
                try { _fi_breakdownSecs.SetValue(p, 0f); } catch { }
        }
        catch { }
    }

    // ——— 衣物不潮湿 ——— v2.7.15 改进:直接 wrapper 字段访问 + tick 频率跟 90 帧
    public static void TickClothingWetness()
    {
        if (!CheatState.NoWetClothes) return;
        try
        {
            var clothes = UnityEngine.Object.FindObjectsOfType<ClothingItem>();
            if (clothes == null) return;
            foreach (var c in clothes)
            {
                if (c == null) continue;
                try { c.m_PercentWet = 0f; } catch { }
            }
        }
        catch { }
    }

    // TryLockFieldOnAll 已移除 —— 仅 TickFires 用,TickFires 本身已删
}

// ═══════════════════════════════════════════════════════════════════
//   v2.7.45 CT 复刻 —— 从 CT 脚本直接映射的 Harmony patches
// ═══════════════════════════════════════════════════════════════════

// —— 秒烤肉 v2.7.51 —— 仿 CT:只推 m_CookingElapsedHours,不碰 percent/state
//   CT 做法:mov [rcx+m_CookingElapsedHours],(float)10; jmp original;
//   之前 v2.7.49/50 直接写 state=Ready,拾取时游戏内部想换状态被 Postfix 抢回 → 抽搐卡死
//   新法:Prefix 拿 cookTimeMinutes 参数,把 elapsed 推到刚好过熟透阈值,让原方法自然转 Ready
//   仅在 Cooking 状态下干预;Ready/Ruined 状态完全不动,pickup / eat 流程正常
[HarmonyPatch(typeof(CookingPotItem), "UpdateCookingTimeAndState", new System.Type[] { typeof(float), typeof(float) })]
internal static class Patch_CookingPot_Update
{
    private static bool _logged;
    private static void Prefix(CookingPotItem __instance, float cookTimeMinutes, float readyTimeMinutes)
    {
        if (!CheatState.QuickCook) return;
        try
        {
            if (__instance.m_CookingState != CookingPotItem.CookingState.Cooking) return;
            if (cookTimeMinutes <= 0f) return;
            if (!_logged)
            {
                _logged = true;
                ModMain.Log.Msg($"[QuickCook] elapsed-push cookTimeMin={cookTimeMinutes}");
            }
            // elapsed 单位小时,cookTime 单位分钟;推到 (cookTime/60)*1.01 刚过熟透阈值
            //   原方法会自己算 percent = elapsed*60 / cookTime → 约 1.01 → 转 Ready
            //   下一帧 state != Cooking,patch 直接 return 不再干扰
            __instance.m_CookingElapsedHours = cookTimeMinutes / 60f * 1.01f;
        }
        catch (System.Exception e) { ModMain.Log.Error($"[QuickCook] {e.Message}"); }
    }
}

// —— 秒采集地上作物 v2.7.54 —— 回退 PerformHold 方案(采不了 bug)
//   v2.7.52 PerformHold Prefix __result=true 导致按 E 无响应 —— 猜 true 在 TLD 语义是"继续 hold"
//   回退到 v2.7.49 方案:字段压爆 + UpdateHoldInteraction deltaTime ×10000
//   代价是看得见 ~0.1s 读条,但能采集
[HarmonyPatch(typeof(HarvestableInteraction), "InitializeInteraction")]
internal static class Patch_HarvestableInteraction_Init
{
    private static void Postfix(HarvestableInteraction __instance)
    {
        if (!CheatState.QuickSearch) return;
        try { __instance.m_DefaultHoldTime = 0.001f; } catch { }
    }
}

[HarmonyPatch(typeof(HarvestableInteraction), "BeginHold")]
internal static class Patch_HarvestableInteraction_BeginHold
{
    private static void Postfix(HarvestableInteraction __instance)
    {
        if (!CheatState.QuickSearch) return;
        try
        {
            __instance.m_DefaultHoldTime = 0.001f;
            __instance.m_Timer = 99999f;
        }
        catch { }
    }
}

// 按子类类型过滤放大 deltaTime,Harvestable 子类一帧内跑满
[HarmonyPatch(typeof(Il2CppTLD.Interactions.TimedHoldInteraction), "UpdateHoldInteraction", new System.Type[] { typeof(float) })]
internal static class Patch_TimedHold_UpdateHoldInteraction_QuickSearch
{
    private static bool _logged;
    private static void Prefix(Il2CppTLD.Interactions.TimedHoldInteraction __instance, ref float deltaTime)
    {
        if (!CheatState.QuickSearch) return;
        try
        {
            var name = __instance.GetType().Name;
            if (name.Contains("Harvest") || name.Contains("PickUp"))
            {
                if (!_logged)
                {
                    _logged = true;
                    ModMain.Log.Msg($"[QuickSearch.UpdateHold] 加速 type={name}");
                }
                deltaTime *= 10000f;
            }
        }
        catch { }
    }
}

// —— 秒割肉 CT:Panel_BodyHarvest.Refresh 设 HarvestTimeSeconds=TotalHarvestTimeSeconds,Minutes=0 ——
[HarmonyPatch(typeof(Panel_BodyHarvest), "Refresh")]
internal static class Patch_Harvest_Refresh_Quick
{
    private static void Prefix(Panel_BodyHarvest __instance)
    {
        if (!CheatState.QuickHarvest) return;
        try
        {
            __instance.m_HarvestTimeSeconds = __instance.m_TotalHarvestTimeSeconds;
            __instance.m_HarvestTimeMinutes = 0f;
        }
        catch { }
    }
}

// 秒割肉 CT 还加:BodyHarvest.MaybeFreeze 设 m_PercentFrozen=0(冻肉也能割)
[HarmonyPatch(typeof(BodyHarvest), "MaybeFreeze")]
internal static class Patch_BodyHarvest_MaybeFreeze
{
    private static void Prefix(BodyHarvest __instance)
    {
        if (!CheatState.QuickHarvest) return;
        try { __instance.m_PercentFrozen = 0f; } catch { }
    }
}

// —— 秒打碎/回收 CT:Panel_BreakDown.UpdateDurationLabel 设 SecondsToBreakDown=0.2, BreakDown.TimeCostHours=0 ——
//   v2.7.61 加 snapshot+restore —— toggle off 时恢复 BreakDown.m_TimeCostHours,否则"UI 还是 0 min"
//           key 用 m_BreakDown.Pointer(每个物品的 BreakDown 组件独立)
[HarmonyPatch(typeof(Panel_BreakDown), "UpdateDurationLabel")]
internal static class Patch_BreakDown_UpdateDuration
{
    internal static readonly System.Collections.Generic.Dictionary<System.IntPtr, float> Snapshots
        = new System.Collections.Generic.Dictionary<System.IntPtr, float>();

    private static void Prefix(Panel_BreakDown __instance)
    {
        try
        {
            var bd = __instance.m_BreakDown;
            if (bd == null) return;
            var ptr = bd.Pointer;
            if (CheatState.QuickBreakDown)
            {
                if (!Snapshots.ContainsKey(ptr))
                    Snapshots[ptr] = bd.m_TimeCostHours;    // 首次 toggle on → 存原值
                __instance.m_SecondsToBreakDown = 0.2f;
                bd.m_TimeCostHours = 0f;
            }
            else if (Snapshots.TryGetValue(ptr, out var origH))
            {
                bd.m_TimeCostHours = origH;                 // toggle off → 恢复
                Snapshots.Remove(ptr);
                // m_SecondsToBreakDown 不用我们写,原方法会根据 TimeCostHours 重算
            }
        }
        catch { }
    }
}

// v2.7.61 删除 Patch_BreakDown_Update_ForceFinish —— 每帧强推 m_TimeSpentBreakingDown 但没 Fade.Arm,
//   拆完留黑屏。改由 Patch_BreakDown_OnBreakDown 响应 QuickBreakDown 即可(原路径已带 Fade.Arm)

// —— 解锁保险箱 CT:SafeCracking.Update 直接 jmp UnlockSafe ——
[HarmonyPatch(typeof(SafeCracking), "Update")]
internal static class Patch_SafeCracking_Update
{
    private static void Postfix(SafeCracking __instance)
    {
        if (!CheatState.UnlockSafes) return;
        try { __instance.UnlockSafe(); } catch { }
    }
}

// —— 解锁上锁门/柜子 CT:LockedInteraction.IsLocked 强 return false ——
[HarmonyPatch(typeof(LockedInteraction), "IsLocked")]
internal static class Patch_LockedInteraction_IsLocked_Unlock
{
    private static void Postfix(ref bool __result)
    {
        if (CheatState.UnlockSafes || CheatState.IgnoreLock) __result = false;
    }
}

// —— 防风油灯油不减 CT:KeroseneLampItem.ReduceFuel 首字节 db C3(return) ——
[HarmonyPatch(typeof(Il2CppTLD.Gear.KeroseneLampItem), "ReduceFuel")]
internal static class Patch_KeroseneLamp_ReduceFuel
{
    private static bool Prefix() => !CheatState.LampFuelNoDrain;
}

// —— 保温杯永不失温 CT:InsulatedFlask.CalculateHeatLoss NOP 关键字节 ——
[HarmonyPatch(typeof(Il2CppTLD.Gear.InsulatedFlask), "CalculateHeatLoss")]
internal static class Patch_Flask_CalcHeatLoss
{
    private static bool Prefix() => !CheatState.FlaskNoHeatLoss;
}

// —— 保温杯存放无限 CT:InsulatedFlask.UpdateVolume NOP ——
[HarmonyPatch(typeof(Il2CppTLD.Gear.InsulatedFlask), "UpdateVolume")]
internal static class Patch_Flask_UpdateVolume
{
    private static bool Prefix() => !CheatState.FlaskInfiniteVol;
}

// —— 保温瓶装任意 CT:IsItemCompatibleWithFlask 强 true ——
[HarmonyPatch(typeof(Il2CppTLD.Gear.InsulatedFlask), "IsItemCompatibleWithFlask", new System.Type[] { typeof(GearItem) })]
internal static class Patch_Flask_IsCompatible
{
    private static void Postfix(ref bool __result)
    {
        if (CheatState.FlaskAnyItem) __result = true;
    }
}

// —— 加工秒完成 CT:EvolveItem.Update 设 TimeToEvolveGameDays=0, TimeSpentEvolvingGameHours=1 ——
[HarmonyPatch(typeof(EvolveItem), "Update")]
internal static class Patch_EvolveItem_Update
{
    private static void Prefix(EvolveItem __instance)
    {
        if (!CheatState.QuickEvolve) return;
        try
        {
            __instance.m_TimeToEvolveGameDays = 0f;
            __instance.m_TimeSpentEvolvingGameHours = 1f;
        }
        catch { }
    }
}

// —— 篝火温度 300℃ v2.7.49 加 snapshot+restore —— toggle off 必须还原
//   v2.7.48 只写不还原 → 关 toggle 后 m_MaxTempIncrease 仍是 300
[HarmonyPatch(typeof(HeatSource), "Update")]
internal static class Patch_HeatSource_Update
{
    // 每实例原值快照:toggle 第一次 on 时记,toggle off 时恢复并删除
    internal static readonly System.Collections.Generic.Dictionary<System.IntPtr, float> Snapshots
        = new System.Collections.Generic.Dictionary<System.IntPtr, float>();

    private static void Prefix(HeatSource __instance)
    {
        try
        {
            var ptr = __instance.Pointer;
            if (CheatState.FireTemp300)
            {
                if (!Snapshots.ContainsKey(ptr))
                    Snapshots[ptr] = __instance.m_MaxTempIncrease;   // 首次开启 snapshot
                __instance.m_MaxTempIncrease = 300f;
            }
            else if (Snapshots.TryGetValue(ptr, out var orig))
            {
                __instance.m_MaxTempIncrease = orig;                 // 关闭 → 恢复
                Snapshots.Remove(ptr);
            }
        }
        catch { }
    }
}

// —— 篝火永不熄灭 v2.7.49 加 snapshot+restore ——
//   v2.7.48 写 IsPerpetual=true / MaxOnTODSeconds=INF / ElapsedOnTODSeconds=0 / BurnMinutesIfLit=99999
//   toggle off 时必须复原这些字段,不然火堆"永久烙印"
[HarmonyPatch(typeof(Fire), "Update")]
internal static class Patch_Fire_Update_NeverDie
{
    // (isPerpetual, maxOnTOD, elapsedOnTOD, burnMinutesIfLit)
    internal static readonly System.Collections.Generic.Dictionary<System.IntPtr, (bool, float, float, float)> Snapshots
        = new System.Collections.Generic.Dictionary<System.IntPtr, (bool, float, float, float)>();

    private static void Prefix(Fire __instance)
    {
        try
        {
            var ptr = __instance.Pointer;
            if (CheatState.FireNeverDie)
            {
                if (!Snapshots.ContainsKey(ptr))
                    Snapshots[ptr] = (__instance.m_IsPerpetual,
                                      __instance.m_MaxOnTODSeconds,
                                      __instance.m_ElapsedOnTODSeconds,
                                      __instance.m_BurnMinutesIfLit);
                __instance.m_IsPerpetual = true;
                __instance.m_MaxOnTODSeconds = float.PositiveInfinity;
                __instance.m_ElapsedOnTODSeconds = 0f;
                __instance.m_BurnMinutesIfLit = 99999f;
            }
            else if (Snapshots.TryGetValue(ptr, out var s))
            {
                __instance.m_IsPerpetual = s.Item1;
                __instance.m_MaxOnTODSeconds = s.Item2;
                __instance.m_ElapsedOnTODSeconds = s.Item3;
                __instance.m_BurnMinutesIfLit = s.Item4;
                Snapshots.Remove(ptr);
            }
        }
        catch { }
    }
}

// —— 清除死亡惩罚 CT:CheatDeathAffliction 设为 cured ——
[HarmonyPatch(typeof(CheatDeathAffliction), "Update")]
internal static class Patch_CheatDeathAfflict_Update
{
    private static void Prefix(CheatDeathAffliction __instance)
    {
        if (!CheatState.ClearDeathPenalty) return;
        try { if (__instance.HasAffliction) __instance.Cure(AfflictionOptions.None); } catch { }
    }
}

// ═══════════════════════════════════════════════════════════════════
//   v2.7.55 CT 复刻 —— 商人 Trader + 美洲狮 Cougar
// ═══════════════════════════════════════════════════════════════════

// —— 商人:交易清单上限 + 信任最大化 ——
//   CT: GetAvailableTradeExchanges Prefix,根据 toggle 改 m_MaxExchangesInTrade(原 3 → 64)
//       以及把 m_CurrentState.m_CurrentTrust/m_HighestTrust = m_MaxTrustLevel
[HarmonyPatch(typeof(Il2CppTLD.Trader.TraderManager), "GetAvailableTradeExchanges")]
internal static class Patch_TraderManager_GetAvailableTradeExchanges
{
    private static void Prefix(Il2CppTLD.Trader.TraderManager __instance)
    {
        try
        {
            // CT 做法:toggle 开写 64,toggle 关强制写 3(原游戏默认)—— 每次调用都刷新
            //   这样 toggle off 立刻生效,不会像篝火那样"开了就回不去"
            __instance.m_MaxExchangesInTrade = CheatState.TraderUnlimitedList ? 64 : 3;

            if (CheatState.TraderMaxTrust)
            {
                var st = __instance.m_CurrentState;
                if (st != null)
                {
                    int mx = __instance.m_MaxTrustLevel;
                    st.m_CurrentTrust = mx;
                    st.m_HighestTrust = mx;
                }
            }
        }
        catch { }
    }
}

// —— 商人:随时可联系(无线电)——
//   CT: IsTraderAvailable 直接 mov al,01; ret → __result = true
[HarmonyPatch(typeof(Il2CppTLD.Trader.TraderManager), "IsTraderAvailable")]
internal static class Patch_TraderManager_IsTraderAvailable
{
    private static void Postfix(ref bool __result)
    {
        if (CheatState.TraderAlwaysAvailable) __result = true;
    }
}

// —— 商人:交易秒完成 ——
//   CT: ExchangeItem.IsFullyExchanged Prefix,把 Exchanged* 三个字段 = 源字段,让 IsFullyExchanged 自动 return true
[HarmonyPatch(typeof(Il2CppTLD.Trader.ExchangeItem), "IsFullyExchanged")]
internal static class Patch_ExchangeItem_IsFullyExchanged
{
    private static void Prefix(Il2CppTLD.Trader.ExchangeItem __instance)
    {
        if (!CheatState.TraderInstantExchange) return;
        try
        {
            __instance.m_ExchangedAmount       = __instance.m_Amount;
            __instance.m_ExchangedWeight       = __instance.m_Weight;
            __instance.m_ExchangedLiquidVolume = __instance.m_LiquidVolume;
        }
        catch { }
    }
}

// —— 美洲狮:新档首次立即激活 ——
//   CT: UpdateWaitingForArrival Prefix,设 __instance.m_ActiveTerritory.m_CougarState = 2 (WaitingForTransition)
//   enum CougarState { Start=0, WaitingForArrival=1, WaitingForTransition=2, HasArrivedAfterTransition=3, PlayingIntroTimeline=4, HasArrived=5 }
//   激活后进出门/睡觉会触发到 HasArrivedAfterTransition 动画
[HarmonyPatch(typeof(Il2CppTLD.AI.CougarManager), "UpdateWaitingForArrival")]
internal static class Patch_CougarManager_UpdateWaitingForArrival
{
    private static void Prefix(Il2CppTLD.AI.CougarManager __instance)
    {
        if (!CheatState.CougarInstantActivate) return;
        try
        {
            var terr = __instance.m_ActiveTerritory;
            if (terr != null)
                terr.m_CougarState = Il2CppTLD.AI.CougarManager.CougarState.WaitingForTransition;
        }
        catch { }
    }
}

