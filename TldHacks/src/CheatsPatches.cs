using System;
using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime;
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

// ——— Condition.AddHealth 总 Prefix:无坠落 + 免疫动物 + 不会窒息 + 真隐身=绝对无敌 ———
[HarmonyPatch(typeof(Condition), "AddHealth", new System.Type[] { typeof(float), typeof(DamageSource) })]
internal static class Patch_Condition_AddHealth_Filter
{
    private static bool Prefix(float hp, DamageSource cause)
    {
        if (hp >= 0f) return true; // 只拦负值(伤害),治疗放行

        // v2.7.22 —— TrueInvisible 再次重写:任何源的负 HP 全挡
        //   v2.7.21 只拦 Wolf/Bear/Cougar 但狼咬触发 Mauled→BloodLoss 持续掉血绕过 filter
        //   → "隐身彻底无效"。现在开 TrueInvisible = 血永不降
        if (CheatState.TrueInvisible) return false;

        if (CheatState.NoFallDamage && cause == DamageSource.Falling) return false;
        if (CheatState.NoSuffocating && cause == DamageSource.Suffocating) return false;
        if (CheatState.ImmuneAnimalDamage)
        {
            if (cause == DamageSource.Wolf || cause == DamageSource.Bear || cause == DamageSource.Cougar)
                return false;
        }
        return true;
    }
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
        QuickHarvestRunner.Queue(__instance, QuickHarvestRunner.Action.Harvest);
    }
}

[HarmonyPatch(typeof(Panel_BodyHarvest), "StartQuarter", new System.Type[] { typeof(int), typeof(string) })]
internal static class Patch_Harvest_StartQuarter
{
    private static void Postfix(Panel_BodyHarvest __instance)
    {
        if (!CheatState.QuickAction) return;
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
        Instance = inst; PendingAction = a; Countdown = 2;
    }

    public static void Tick()
    {
        if (PendingAction == Action.None) return;
        if (Instance == null) { Reset(); return; }
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

// ——— 快速修理(工具 + 衣物通用)v2.7.21:双重保险
//   StartRepair Postfix 一次性把进度跳满 —— 对衣物这种可能不走 Update 循环的路径生效
//   Update Postfix 持续维持 —— 对工具有 progress bar 的情况每帧推进
[HarmonyPatch(typeof(Panel_Repair), "StartRepair", new System.Type[] { typeof(int), typeof(string) })]
internal static class Patch_Repair_StartRepair
{
    private static void Postfix(Panel_Repair __instance)
    {
        if (!CheatState.QuickAction) return;
        try
        {
            __instance.m_ElapsedProgressBarSeconds = 999f;
            __instance.m_RepairTimeSeconds = 0.01f;
            __instance.m_ProgressBarTimeSeconds = 0.01f;
            __instance.m_RepairWillSucceed = true;
            __instance.m_TimeAccelerated = true;
        }
        catch { }
    }
}

[HarmonyPatch(typeof(Panel_Repair), "Update")]
internal static class Patch_Repair_Update
{
    private static void Postfix(Panel_Repair __instance)
    {
        if (!CheatState.QuickAction) return;
        try
        {
            if (!__instance.m_RepairInProgress) return;
            if (__instance.m_RepairSucceeded || __instance.m_RepairFailed) return;
            __instance.m_ElapsedProgressBarSeconds = __instance.m_ProgressBarTimeSeconds + 1f;
            __instance.m_RepairTimeSeconds = 0.01f;
            __instance.m_RepairWillSucceed = true;
            __instance.m_TimeAccelerated = true;
        }
        catch { }
    }
}

// ——— 快速拆解 v2.7.18:同 Repair 思路 ———
[HarmonyPatch(typeof(Panel_BreakDown), "Update")]
internal static class Patch_BreakDown_Update
{
    private static void Postfix(Panel_BreakDown __instance)
    {
        if (!CheatState.QuickAction) return;
        try
        {
            if (!__instance.m_IsBreakingDown) return;
            __instance.m_TimeSpentBreakingDown = __instance.m_SecondsToBreakDown + 1f;
            __instance.m_TimeIsAccelerated = true;
        }
        catch { }
    }
}

// v2.7.19 撤掉所有 CameraFade patch + Panel_BodyHarvest.Update Patch:
//   前者递归风险,后者每帧强推字段让游戏状态机错乱 → 2.7.18 卡死根因
//   新方案靠 QuickHarvestRunner 延迟完成 = 直接跳过整个 fade + time 流程,根本不让黑屏出现

// v2.7.24 加回 CameraFade 4 个 Prefix —— 用户反馈采集/制作仍有黑屏
//   只设 time=0 delay=0 让 fade 瞬完,不调 FinishFade/UpdateCameraFade(那是 v2.7.18 卡死根因)
//   Action 仍会触发,游戏逻辑不断,只是视觉上不黑
[HarmonyPatch(typeof(CameraFade), "FadeOut", new System.Type[] { typeof(float), typeof(float), typeof(Il2CppSystem.Action) })]
internal static class Patch_CameraFade_FadeOut
{
    private static void Prefix(ref float time, ref float delay)
    {
        if (CheatState.QuickAction || CheatState.QuickCraft) { time = 0f; delay = 0f; }
    }
}

[HarmonyPatch(typeof(CameraFade), "FadeIn", new System.Type[] { typeof(float), typeof(float), typeof(Il2CppSystem.Action) })]
internal static class Patch_CameraFade_FadeIn
{
    private static void Prefix(ref float time, ref float delay)
    {
        if (CheatState.QuickAction || CheatState.QuickCraft) { time = 0f; delay = 0f; }
    }
}

[HarmonyPatch(typeof(CameraFade), "FadeTo", new System.Type[] { typeof(float), typeof(float), typeof(float), typeof(Il2CppSystem.Action) })]
internal static class Patch_CameraFade_FadeTo
{
    private static void Prefix(ref float time, ref float delay)
    {
        if (CheatState.QuickAction || CheatState.QuickCraft) { time = 0f; delay = 0f; }
    }
}

[HarmonyPatch(typeof(CameraFade), "Fade", new System.Type[] { typeof(float), typeof(float), typeof(float), typeof(float), typeof(Il2CppSystem.Action) })]
internal static class Patch_CameraFade_Fade
{
    private static void Prefix(ref float time, ref float delay)
    {
        if (CheatState.QuickAction || CheatState.QuickCraft) { time = 0f; delay = 0f; }
    }
}

// ——— 快速操作 v2.7.26 ———
// TimeOfDay.Accelerate(realTimeSec, gameTimeHours, doFadeToBlack):
//   QuickCraft: 只设 realTimeSeconds=0.01 + doFadeToBlack=false,gameTimeHours 保留
//     —— v2.7.22 设 gameTimeHours=0 导致批量 batch 剩余项时间不走,只完成 1 个 = 50%
//   QuickAction: 全 3 个都设(采集/修理/拆解走这条),时间不跳
[HarmonyPatch(typeof(TimeOfDay), "Accelerate",
    new System.Type[] { typeof(float), typeof(float), typeof(bool) })]
internal static class Patch_TimeOfDay_Accelerate
{
    private static void Prefix(ref float realTimeSeconds, ref float gameTimeHours, ref bool doFadeToBlack)
    {
        if (CheatState.QuickAction)
        {
            realTimeSeconds = 0.01f;
            gameTimeHours = 0f;
            doFadeToBlack = false;
        }
        else if (CheatState.QuickCraft)
        {
            realTimeSeconds = 0.01f;
            // gameTimeHours 保留 —— 批量制作剩余项靠时间推进启动
            doFadeToBlack = false;
        }
    }
}

[HarmonyPatch(typeof(TimeOfDay), "AccelerateTime",
    new System.Type[] { typeof(float), typeof(float) })]
internal static class Patch_TimeOfDay_AccelerateTime
{
    private static void Prefix(ref float realtimeDurationSeconds, ref float gameTimeHours)
    {
        if (CheatState.QuickAction)
        {
            realtimeDurationSeconds = 0.01f;
            gameTimeHours = 0f;
        }
        else if (CheatState.QuickCraft)
        {
            realtimeDurationSeconds = 0.01f;
        }
    }
}

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

// ——— 快速制作 v2.7.17 改:直接调 Panel_Crafting.OnCraftingSuccess 完成,跳过 time accel ———
// 原方案 Accelerate 1000x 30s = 8 小时游戏时长,19 小时 craft 只做一半 —— 不够彻底
[HarmonyPatch(typeof(Panel_Crafting), "CraftingStart")]
internal static class Patch_Craft_Start_Instant
{
    private static void Postfix(Panel_Crafting __instance)
    {
        if (!CheatState.QuickCraft) return;
        try
        {
            __instance.OnCraftingSuccess();
            ModMain.Log?.Msg("[QuickCraft] instant complete");
        }
        catch (System.Exception ex) { ModMain.Log?.Warning($"[QuickCraft] {ex.Message}"); }
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

// v2.7.26 —— BaseAi.Start Postfix:新 AI 出生时一次性设 TrueInvisible 字段
//   替代 TickAnimalsFull 每 5s 扫 FindObjectsOfType<BaseAi> + 写 10 字段/AI 的高开销
//   AI 一般稳定,新生出少,这条 patch 几乎零开销 + tick 降到 10s 兜底
[HarmonyPatch(typeof(BaseAi), "Start")]
internal static class Patch_BaseAi_Start_Invis
{
    private static void Postfix(BaseAi __instance)
    {
        if (!CheatState.TrueInvisible) return;
        try
        {
            __instance.m_DisableScanForTargets = true;
            __instance.m_DetectionRange = 0f;
            __instance.m_DetectionFOV = 0f;
            __instance.m_HearFootstepsRange = 0f;
            __instance.m_HearRifleRange = 0f;
            __instance.m_HearCarAlarmRange = 0f;
            __instance.m_SmellRange = 0f;
            __instance.m_DetectionRangeWhileFeeding = 0f;
            __instance.m_HearFootstepsRangeWhileFeeding = 0f;
            __instance.m_HearFootstepsRangeWhileSleeping = 0f;
        }
        catch { }
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
[HarmonyPatch(typeof(FireManager), "CalculateFireStartSuccess")]
internal static class Patch_FireMgr_Success
{
    private static void Postfix(ref float __result)
    {
        if (CheatState.QuickFire) __result = 1f;
    }
}

// ——— 关闭瞄准景深(DOF)—— 拦截 EnableCameraWeaponPostEffects(true),强制传 false ———
[HarmonyPatch(typeof(CameraEffects), "EnableCameraWeaponPostEffects")]
internal static class Patch_CamEffects_WeaponPost
{
    private static void Prefix(ref bool isEnabled)
    {
        if (CheatState.NoAimDOF) isEnabled = false;
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

    private static void EnsureReflectionInited(object weapon)
    {
        if (_reflectionInited && weapon == null) return;
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
            var ais = UnityEngine.Object.FindObjectsOfType<BaseAi>();
            if (ais == null)
            {
                _lastTickedStealth = CheatState.Stealth;
                _lastTickedFreeze  = CheatState.FreezeAnimals;
                _lastTickedInvis   = CheatState.TrueInvisible;
                return;
            }

            foreach (var ai in ais)
            {
                if (ai == null) continue;
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
