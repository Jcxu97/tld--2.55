using System;
using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppTLD.Gear;
using Il2CppTLD.IntBackedUnit;
using UnityEngine;

namespace TldHacks;

// —— 武器相关的方法级 patch(比 tick 改字段更可靠)——

// 无限弹药:每次 RemoveNextFromClip 被调用,若 InfiniteAmmo 则 skip
// v2.7.75 DynamicPatch
internal static class Patch_Gun_RemoveNextFromClip
{
    internal static bool Prefix() => !CheatState.InfiniteAmmo;
}

// v2.7.75 删 5 个 GunItem getter "belt-n-suspenders" patch —— TickCamera 的
// m_DisableAimSway / m_DisableAimShake / m_DisableAimBreathing / m_DisableAimStamina /
// m_RecoilSpring 重置是主力,这些 getter patch 是 v2.7.0 留的冗余,删掉减少启动 bridge 数

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

        // v2.7.84:GodMode 拦所有伤害源(含 Freezing/Hypothermia/FrostBite/Starving/Dehydrated/Exhausted)
        //   之前 GodMode 只在 TickStatus 里每帧刷满 HP,但冻伤每帧又扣 → HUD 闪烁
        //   现在直接在 AddHealth Prefix 拦截,HP 永远不掉,TickStatus 只做兜底
        if (CheatState.GodMode) return true;
        if (CheatState.TrueInvisible) return true;  // 真隐身 = 绝对无敌

        // v2.7.85:各状态 toggle 独立拦截对应伤害源 —— 修复"GodMode OFF + AlwaysWarm ON 时 HP 条闪烁"
        //   之前只在 GodMode 时拦全部,Now 每个 toggle 拦自己的源,减少 TickStatus "先掉再补"的 HUD 闪烁
        if (CheatState.AlwaysWarm && (cause == DamageSource.Freezing || cause == DamageSource.Hypothermia || cause == DamageSource.FrostBite))
            return true;
        if (CheatState.NoFrostbiteRisk && cause == DamageSource.FrostBite)
            return true;
        if (CheatState.NoHunger && cause == DamageSource.Starving) return true;
        if (CheatState.NoThirst && cause == DamageSource.Dehydrated) return true;
        if (CheatState.NoFatigue && cause == DamageSource.Exhausted) return true;

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

internal static class Patch_Condition_AddHealth_2
{
    internal static bool Prefix(float hp, DamageSource cause)
        => !DamageFilter.ShouldBlock(hp, cause);
}

internal static class Patch_Condition_AddHealth_3
{
    internal static bool Prefix(float hp, DamageSource cause)
        => !DamageFilter.ShouldBlock(hp, cause);
}

internal static class Patch_Condition_AddHealthNoHud
{
    internal static bool Prefix(float hp, DamageSource damageSource)
        => !DamageFilter.ShouldBlock(hp, damageSource);
}

// ——— 快速采集 v2.7.19:彻底换方案 —— 延迟调 HarvestSuccessful/QuarterSuccessful 跳过整个时间流逝 + fade
// 之前 Patch_Harvest_Accelerate 把 minutes=0 → panel 等时间但时间不走 = 卡死
// 之前 Patch_Harvest_Update 每帧强推字段 + CameraFade FinishFade = 黑屏 + 递归 = 点取消游戏都卡死
// 现在:Postfix 只记录"2 帧后完成",由 ModMain.OnUpdate 调 QuickHarvestRunner.Tick 完成
internal static class Patch_Harvest_Start
{
    internal static void Postfix(Panel_BodyHarvest __instance)
    {
        if (!CheatState.QuickAction) return;
        // v3.0.4r2 fix: 当 QuickHarvest 已开启,m_HarvestTimeSeconds=Total 让 vanilla 自动 HarvestSuccessful,
        // Runner 不应再 Queue 第二次,否则 vanilla + Runner 双重触发 → 尸体多扣 m_HarvestUnits 个 + stack 合并产出错乱
        if (CheatState.QuickHarvest) return;
        FadeSuppressionWindow.Arm();  // v2.7.29:采集过程内吃 fade
        QuickHarvestRunner.Queue(__instance, QuickHarvestRunner.Action.Harvest);
    }
}

internal static class Patch_Harvest_StartQuarter
{
    internal static void Postfix(Panel_BodyHarvest __instance)
    {
        if (!CheatState.QuickAction) return;
        // v3.0.4r2 fix: 见 Patch_Harvest_Start 同样防双重触发
        if (CheatState.QuickHarvest) return;
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
// v2.8.1: 移除 [HarmonyPatch] 属性(IL2CPP 下静默失败),改用 DynamicPatch spec
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
internal static class Patch_BreakDown_OnBreakDown
{
    internal static void Postfix(Panel_BreakDown __instance)
    {
        if (!CheatState.QuickAction && !CheatState.QuickBreakDown) return;
        FadeSuppressionWindow.Arm(5.0f);
        try
        {
            // v2.7.65 关键修:冻结 TOD 避免游戏推进时钟触发 UniStorm 昼夜/天气 ScreenTint(变灰源头)
            //   和 QuickCraft 用的 m_HoursToSpendCrafting=100 但 TOD 不真实推进是同逻辑
            try { var tod = GameManager.GetTimeOfDayComponent(); if (tod != null) tod.SetTODLocked(true); } catch { }
            __instance.m_TimeSpentBreakingDown = __instance.m_SecondsToBreakDown + 1f;
            // 不设 m_TimeIsAccelerated=true —— 它也会触发 ScreenTint
        }
        catch (System.Exception ex) { ModMain.Log?.Warning($"[QuickBreakDown] {ex.Message}"); }
    }
}

// v2.7.65 打碎完成解锁 TOD + 强制亮屏 —— 必须解锁,不然整个游戏时钟会卡住
// v2.7.73 修"持续暗":之前 FadeIn(0,0) 没拦住 alpha 往 target 走,加 FinishFade(true) 跳过进行中 fade
// v2.7.74 定位到真源头 —— CT 也会出现,说明不是 CameraFade,是 PassTime.m_TimeAccelerated 触发的 ScreenTint;
//   原 BreakDownFinished 可能没 call PassTime.End(),或 End 被我们 force 到的字段抄近路绕过;
//   显式 End() + reset Panel 实例 m_TimeIsAccelerated + PassTime.m_TimeAccelerated
internal static class Patch_BreakDown_Finished_Unfade
{
    internal static void Postfix(Panel_BreakDown __instance)
    {
        if (!CheatState.QuickAction && !CheatState.QuickBreakDown) return;
        try
        {
            try { var tod = GameManager.GetTimeOfDayComponent(); if (tod != null) tod.SetTODLocked(false); } catch { }
            FadeSuppressionWindow.Arm(3.0f);
            CameraFade.FadeIn(0f, 0f, null);
            try
            {
                CameraFade.m_TargetAlpha = 0f;
                CameraFade.m_StartAlpha = 0f;
                CameraFade.m_FadeTimer = 0f;
                CameraFade.m_FadeDuration = 0f;
                CameraFade.FinishFade(true);
            }
            catch { }
            // v2.7.74 关键:强制终结 PassTime — "阴天感" overlay 来自 m_TimeAccelerated
            try
            {
                if (__instance != null) __instance.m_TimeIsAccelerated = false;
                var pt = GameManager.GetPassTime();
                if (pt != null)
                {
                    bool wasActive = false;
                    try { wasActive = pt.IsPassingTime(); } catch { }
                    try { if (wasActive) pt.End(); } catch { }
                    try { pt.m_TimeAccelerated = false; } catch { }
                    ModMain.Log?.Msg($"[QuickBD.Finished] PassTime.End (wasActive={wasActive}) + m_TimeAccelerated=false");
                }
                else ModMain.Log?.Msg("[QuickBD.Finished] PassTime null");
            }
            catch (System.Exception ex) { ModMain.Log?.Warning($"[QuickBD.PassTime] {ex.Message}"); }
        }
        catch { }
    }
}

// 防御性:如果玩家中途取消或退出界面,也要解锁 TOD 不然时钟永远冻
internal static class Patch_BreakDown_ExitInterface_UnlockTOD
{
    internal static void Postfix(Panel_BreakDown __instance)
    {
        BreakDownCleanup.Run(__instance, "ExitInterface");
    }
}

internal static class Patch_BreakDown_OnCancel_UnlockTOD
{
    internal static void Postfix(Panel_BreakDown __instance)
    {
        BreakDownCleanup.Run(__instance, "OnCancel");
    }
}

// v2.7.74 Update 边沿检测:BreakDownFinished 被内联没 fire,用 IsBreakingDown true→false 替代
// v2.7.75 DynamicPatch: 去 [HarmonyPatch] attribute,只在 QuickAction || QuickBreakDown 时挂
internal static class Patch_BreakDown_Update_Edge
{
    private static bool _wasBreaking;
    internal static void Postfix(Panel_BreakDown __instance)
    {
        if (__instance == null) return;
        bool now = false;
        try { now = __instance.m_IsBreakingDown; } catch { }
        if (_wasBreaking && !now) BreakDownCleanup.Run(__instance, "UpdateEdge");
        _wasBreaking = now;
    }
}

// Panel 关闭时也 cleanup —— 兜底覆盖所有退出 path
internal static class Patch_BreakDown_Enable_Cleanup
{
    internal static void Postfix(Panel_BreakDown __instance, bool enable)
    {
        if (!enable) BreakDownCleanup.Run(__instance, "Enable(false)");
    }
}

internal static class BreakDownCleanup
{
    public static void Run(Panel_BreakDown inst, string tag)
    {
        if (!CheatState.QuickAction && !CheatState.QuickBreakDown) return;
        try { FadeSuppressionWindow.Arm(5.0f); } catch { }
        try { CameraFade.FadeIn(0f, 0f, null); } catch { }
        try
        {
            CameraFade.m_TargetAlpha = 0f;
            CameraFade.m_StartAlpha = 0f;
            CameraFade.m_FadeTimer = 0f;
            CameraFade.m_FadeDuration = 0f;
            CameraFade.FinishFade(true);
        }
        catch { }
        try { if (inst != null) inst.m_TimeIsAccelerated = false; } catch { }
        bool ptWasActive = false;
        try
        {
            var pt = GameManager.GetPassTime();
            if (pt != null)
            {
                try { ptWasActive = pt.IsPassingTime(); } catch { }
                try { if (ptWasActive) pt.End(); } catch { }
                try { pt.m_TimeAccelerated = false; } catch { }
            }
        }
        catch { }
        try { var tod = GameManager.GetTimeOfDayComponent(); if (tod != null) tod.SetTODLocked(false); } catch { }
        ModMain.Log?.Msg($"[QuickBD.Cleanup:{tag}] ptWasActive={ptWasActive}");
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
    // v2.7.64 从 1.5s 拉长到 3s —— 打碎完成时 TimeIsAccelerated 可能触发 ScreenTint,窗口太短漏掉
    public static void Arm(float seconds = 3.0f)
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
        if (FadeSuppressionWindow.IsActive)
        {
            ModMain.Log?.Msg($"[FadeSuppress] FadeOut time={time} delay={delay}");
            time = 0f; delay = 0f;
        }
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

// v2.7.73 合并 Fade(startAlpha, targetAlpha, time, delay, action) 5 参数
//   窗口内把 start/target 也归 0,避免 Fade(1,1,...) 把黑屏锁死
[HarmonyPatch(typeof(CameraFade), "Fade", new System.Type[] { typeof(float), typeof(float), typeof(float), typeof(float), typeof(Il2CppSystem.Action) })]
internal static class Patch_CameraFade_Fade
{
    private static void Prefix(ref float startAlpha, ref float targetAlpha, ref float time, ref float delay)
    {
        if (FadeSuppressionWindow.IsActive)
        {
            ModMain.Log?.Msg($"[FadeSuppress] Fade5 s={startAlpha} t={targetAlpha} time={time} delay={delay}");
            startAlpha = 0f; targetAlpha = 0f; time = 0f; delay = 0f;
        }
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

// ——— 免费制作扩展:BlueprintData.HasRequiredMaterials 强制返回 true ———
[HarmonyPatch(typeof(Il2CppTLD.Gear.BlueprintData), "HasRequiredMaterials")]
internal static class Patch_BlueprintData_HasRequiredMaterials
{
    private static void Postfix(ref bool __result)
    {
        if (CheatState.FreeCraft) __result = true;
    }
}

// ——— 免费制作扩展:RecipeBook.IsRecipeUnlocked 强制返回 true(解锁 DLC/mod 食谱) ———
[HarmonyPatch(typeof(Il2CppTLD.Cooking.RecipeBook), "IsRecipeUnlocked")]
internal static class Patch_RecipeBook_IsRecipeUnlocked
{
    private static void Postfix(ref bool __result)
    {
        if (CheatState.FreeCraft) __result = true;
    }
}

// ——— 免费制作扩展:Panel_Crafting 内部材料检查(mod 物品路径) ———
[HarmonyPatch(typeof(Panel_Milling), "HasEnoughMaterials")]
internal static class Patch_Crafting_HasEnoughMaterials
{
    private static void Postfix(ref bool __result)
    {
        if (CheatState.FreeCraft) __result = true;
    }
}

[HarmonyPatch(typeof(Panel_Crafting), "CanCraftBlueprint", new[] { typeof(Il2CppTLD.Gear.BlueprintData) })]
internal static class Patch_Crafting_CanCraftBlueprint
{
    private static void Postfix(ref bool __result)
    {
        if (CheatState.FreeCraft) __result = true;
    }
}

[HarmonyPatch(typeof(Panel_Crafting), "CanCraftSelectedBlueprint")]
internal static class Patch_Crafting_CanCraftSelected
{
    private static void Postfix(ref bool __result)
    {
        if (CheatState.FreeCraft) __result = true;
    }
}

[HarmonyPatch(typeof(Panel_Cooking), "PlayerHasEnoughPotableWaterForCookingItem")]
internal static class Patch_Crafting_HasWater
{
    private static void Postfix(ref bool __result)
    {
        if (CheatState.FreeCraft) __result = true;
    }
}

[HarmonyPatch(typeof(CraftingOperation), "ConsumeMaterialsUsedForCrafting")]
internal static class Patch_Crafting_SkipConsume
{
    private static bool Prefix() => !CheatState.FreeCraft;
}

// ——— 免费修理:静态 Postfix 强制修理按钮可用 ———
[HarmonyPatch(typeof(Panel_Inventory_Examine), "CanRepair")]
internal static class Patch_Repair_CanRepair
{
    private static void Postfix(ref bool __result)
    {
        if (CheatState.FreeRepair) __result = true;
    }
}

// v2.8.1: HasMaterialsForRepair/RepairHasRequiredTool/RepairHasRequiredMaterial
// 已在 TLD 2.55 中改名或移除,删除失败的 attribute patches(CanRepair + SkipConsume 已足够)

[HarmonyPatch(typeof(Panel_Repair), "ToolCanRepairSelectedItem")]
internal static class Patch_Repair_ToolCanRepair
{
    private static void Postfix(ref bool __result)
    {
        if (CheatState.FreeRepair) __result = true;
    }
}

// ——— 免费修理:跳过消耗材料和工具损耗 ———
internal static class Patch_Repair_SkipConsume
{
    internal static bool Prefix() => !CheatState.FreeRepair;
}

internal static class Patch_Repair_SkipToolDegrade
{
    internal static bool Prefix() => !CheatState.FreeRepair;
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
// v2.7.75 DynamicPatch
internal static class Patch_CraftingOp_Update
{
    internal static void Prefix(CraftingOperation __instance)
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
internal static class Patch_Craft_End_ForceBright
{
    internal static void Postfix()
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
internal static class Patch_Craft_OnSuccess_ArmFade
{
    internal static void Postfix()
    {
        if (CheatState.QuickCraft) FadeSuppressionWindow.Arm(3f);
    }
}

// ——— 隐身感知切断:CanSeeTarget / ScanForSmells ———
// v2.7.23 —— TrueInvisible 语义恢复为"全动物忽略玩家"(狼熊不追/兔鹿不逃)
//   Stealth 仍保持原语义(SetAiMode(Flee) 强制威胁动物逃跑,但兔鹿也会跟着逃,所以 Stealth 是"吓跑模式")
//   TrueInvisible 靠感知层切断 → AI 根本感知不到玩家 → 所有动物当玩家不存在
// v2.7.75 DynamicPatch: 去掉 [HarmonyPatch] attribute,由 DynamicPatch.Reconcile 按 toggle 挂卸
internal static class Patch_BaseAi_CanSeeTarget
{
    internal static bool Prefix(ref bool __result)
    {
        if (CheatState.Stealth || CheatState.TrueInvisible) { __result = false; return false; }
        return true;
    }
}

internal static class Patch_BaseAi_ScanForSmells
{
    internal static bool Prefix() => !(CheatState.Stealth || CheatState.TrueInvisible);
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
// v2.7.75 DynamicPatch: 去 [HarmonyPatch] attribute —— TLD 里锁查询高频(AI/物理/视线)
internal static class Patch_Lock_IsLocked
{
    internal static void Postfix(Lock __instance, ref bool __result)
    {
        if (CheatState.IgnoreLock)
        {
            __result = false;
            // Actually unlock so the lock icon disappears (same as prying open)
            try { __instance.m_LockState = LockState.Unlocked; } catch { }
        }
    }
}
// Lock.RequiresToolToUnlock + PlayerHasRequiredToolToUnlock, Breath.GetBreathTimePercent,
// Encumber.IsEncumbered + GetEncumbranceSlowdownMultiplier 这 5 个 patch 去掉 ——
// 前者因启动卡死嫌疑,后者因为对应功能(IC/IS)已去除(交给 UniversalTweaks 等 mod)

// v2.7.75 DynamicPatch: 去 [HarmonyPatch] attribute —— 下雨时每件衣服每帧调,bridge 开销大
internal static class Patch_Clothing_IncreaseWet
{
    internal static bool Prefix() => !CheatState.NoWetClothes;
}

internal static class Patch_Clothing_GetWetOnGround
{
    internal static bool Prefix() => !CheatState.NoWetClothes;
}

// v2.7.18:删了 ClothingItem.Update Postfix —— 每帧每件 Harmony bridge 是主要 FPS 杀手
// 只靠 2 个 Prefix(IncreaseWetnessPercent + MaybeGetWetOnGround)+ TickClothingWetness(低频)
// 如果还漏,再加另一个 Prefix 拦源头,不要 Update Postfix

// ——— 冰面不破:冰面破裂触发 / 落水 直接跳过 ———
internal static class Patch_IceBreak_BreakIce
{
    internal static bool Prefix() => !CheatState.ThinIceNoBreak;
}

internal static class Patch_IceBreak_FallInWater
{
    internal static bool Prefix() => !CheatState.ThinIceNoBreak;
}

// ——— 生火 100% 成功 ———
// v2.7.31 修:CalculateFireStartSuccess 返回的是 0-100 的 percent(不是 0-1 概率)
//   之前设 1f 实际 = 1% 成功率 —— 用户报告 "开 toggle 只有 1% 概率"
internal static class Patch_FireMgr_Success
{
    internal static void Postfix(ref float __result)
    {
        if (CheatState.QuickFire) __result = 100f;
    }
}

// ——— 快速开容器 ——
// v2.7.13:原 Patch_Container_Enable Postfix 覆盖字段不够 —— 改 EnableAfterDelay Prefix 拦源头
internal static class Patch_Container_EnableAfterDelay
{
    internal static void Prefix(ref float delaySeconds)
    {
        if (CheatState.QuickOpenContainer) delaySeconds = 0f;
    }
}

// 兜底:Enable(bool,bool,Action) Postfix 仍强制 elapsed=999 让任何残留 delay 立刻完成
internal static class Patch_Container_Enable
{
    internal static void Postfix(Panel_Container __instance)
    {
        try
        {
            if (CheatState.QuickOpenContainer)
            {
                __instance.m_EnableDelaySeconds = 0f;
                __instance.m_EnableDelayElapsed = 999f;
            }
            if (CheatState.InfiniteContainer && __instance.m_Container != null)
                __instance.m_Container.m_Capacity = Il2CppTLD.IntBackedUnit.ItemWeight.FromKilograms(10000f);
        }
        catch { }
    }
}

// ═══════════════════════════════════════════════════════════════════
//   v2.7.86 无后坐力根治 —— Hook PlayFireAnimation
// ═══════════════════════════════════════════════════════════════════
// CT 实现:hook vp_FPSWeapon.PlayFireAnimation → 设 [rdi+120..12c] = 0
//   这 4 个偏移 = vp_FPSWeapon 的 recoil 动画参数(继承自 vp_Component 的 float 字段)
//   之前只改 GunItem 字段(m_PitchRecoilMin/Max)无效——后坐力在 PlayFireAnimation 内施加
//   现在 Harmony Postfix 在开枪动画触发后立刻归零这些参数,彻底消除后坐力视觉

// —— 无后坐力 + 超级精准:Hook vp_FPSWeapon.PlayFireAnimation ——
// v2.7.89 无后坐力:零化 GunItem 的后坐力参数,让 PlayFireAnimation 正常播放动画
// 原理:PlayFireAnimation 读 GunItem.m_PitchRecoilMin/Max + m_YawRecoilMin/Max 计算后坐力。
// 值为 0 → 不施加力 → 无后坐。Postfix 恢复原值避免持久修改。
internal static class Patch_FPSWeapon_PlayFireAnimation
{
    private static GunItem _gun;
    private static float _pitchMin, _pitchMax, _yawMin, _yawMax;

    internal static bool Prefix(vp_FPSWeapon __instance)
    {
        bool fullSuppress = CheatState.NoRecoil || CheatState.SuperAccuracy;
        bool partialSuppress = CheatStateESP.RecoilScale < 0.99f;
        if (!fullSuppress && !partialSuppress) return true;
        Patch_FPSCamera_LateUpdate.LastFireTime = Time.time;
        _gun = null;
        try
        {
            var pm = GameManager.GetPlayerManagerComponent();
            var gi = pm?.m_ItemInHands;
            var gun = gi?.GetComponent<GunItem>();
            if (gun != null)
            {
                _gun = gun;
                _pitchMin = gun.m_PitchRecoilMin;
                _pitchMax = gun.m_PitchRecoilMax;
                _yawMin = gun.m_YawRecoilMin;
                _yawMax = gun.m_YawRecoilMax;
                float scale = fullSuppress ? 0f : CheatStateESP.RecoilScale;
                gun.m_PitchRecoilMin = _pitchMin * scale;
                gun.m_PitchRecoilMax = _pitchMax * scale;
                gun.m_YawRecoilMin = _yawMin * scale;
                gun.m_YawRecoilMax = _yawMax * scale;
            }
        }
        catch { }
        return true;
    }

    internal static void Postfix(vp_FPSWeapon __instance)
    {
        if (_gun == null) return;
        try
        {
            _gun.m_PitchRecoilMin = _pitchMin;
            _gun.m_PitchRecoilMax = _pitchMax;
            _gun.m_YawRecoilMin = _yawMin;
            _gun.m_YawRecoilMax = _yawMax;
        }
        catch { }
        _gun = null;
        // v2.7.90 魔法子弹:开枪后直接对目标施加伤害
        try { MagicBulletSystem.OnFired(); } catch { }
    }
}

// v2.7.89 Camera.Update: Prefix 仍尝试归零弹簧参数(阻止弹簧物理运动)
// 如果归零无效(IL2Cpp struct 限制),LateUpdate Postfix 的 transform 补偿兜底
internal static class Patch_FPSCamera_ClearRecoil
{
    private static bool _logged;

    private static bool IsHoldingGun()
    {
        try
        {
            var pm = GameManager.GetPlayerManagerComponent();
            if (pm == null || pm.m_ItemInHands == null) return false;
            return pm.m_ItemInHands.GetComponent<GunItem>() != null;
        }
        catch { return false; }
    }

    internal static void Prefix(vp_FPSCamera __instance)
    {
        bool recoilOn = CheatState.NoRecoil || CheatState.SuperAccuracy || CheatStateESP.RecoilScale < 0.99f;
        bool swayOn = CheatState.NoAimSway;
        if (!recoilOn && !swayOn) return;
        if (!IsHoldingGun()) return;

        if (recoilOn)
        {
            try
            {
                var spring = __instance.m_RecoilSpring;
                spring.m_Current = UnityEngine.Vector2.zero;
                spring.m_Target = UnityEngine.Vector2.zero;
                spring.m_Velocity = UnityEngine.Vector2.zero;
                __instance.m_RecoilSpring = spring;
            }
            catch { }
            try { __instance.RecoilPitchStiffness = 0f; } catch { }
            try { __instance.RecoilYawStiffness = 0f; } catch { }
            try { __instance.RecoilPitchDamping = 1f; } catch { }
            try { __instance.RecoilYawDamping = 1f; } catch { }
        }

        if (swayOn)
        {
            try { __instance.m_MaxAmbientSwayAngleDegreesA = 0f; } catch { }
            try { __instance.m_MaxAmbientAimingSwayAngleDegreesA = 0f; } catch { }
            try { __instance.m_AmbientSwaySpeedA = 0f; } catch { }
            try { __instance.m_AmbientAimingSwaySpeedA = 0f; } catch { }
            try { __instance.m_CurrentMaxAmbientSwayAngle = 0f; } catch { }
            try { __instance.m_CurrentAmbientSwaySpeed = 0f; } catch { }
            try { __instance.ShakeAmplitude = UnityEngine.Vector3.zero; } catch { }
            try { __instance.ShakeSpeed = 0f; } catch { }
            try { __instance.BobAmplitude = UnityEngine.Vector4.zero; } catch { }
        }
    }

    internal static void Postfix(vp_FPSCamera __instance)
    {
        if (!_logged && (CheatState.NoRecoil || CheatState.NoAimSway))
        {
            _logged = true;
            try { ModMain.Log?.Msg($"[Aim] Camera Update Prefix active (recoil={CheatState.NoRecoil} sway={CheatState.NoAimSway})"); } catch { }
        }
    }
}

// v2.7.90 无后坐力核心:只在开枪后短时间窗口内覆盖 transform,防止全局鼠标反转
// 方案:Prefix 记录 transform + m_Pitch/m_Yaw,Postfix 仅保留鼠标 delta、剥离弹簧贡献
internal static class Patch_FPSCamera_LateUpdate
{
    internal static float LastFireTime;
    private const float RecoilWindow = 0.7f;
    private static Quaternion _preRot;
    private static float _prePitch, _preYaw;

    private static bool ShouldActivate()
    {
        return CheatState.NoRecoil || CheatState.SuperAccuracy || CheatStateESP.RecoilScale < 0.99f;
    }

    internal static void Prefix(vp_FPSCamera __instance)
    {
        if (!ShouldActivate()) return;
        if (Time.time - LastFireTime > RecoilWindow) return;
        _preRot = __instance.transform.rotation;
        _prePitch = __instance.m_Pitch;
        _preYaw = __instance.m_Yaw;
    }

    internal static void Postfix(vp_FPSCamera __instance)
    {
        if (ShouldActivate() && Time.time - LastFireTime <= RecoilWindow)
        {
            try
            {
                float dPitch = __instance.m_Pitch - _prePitch;
                float dYaw = __instance.m_Yaw - _preYaw;
                var noRecoilRot = _preRot * Quaternion.Euler(dPitch, dYaw, 0f);
                float suppress = 1f;
                if (CheatState.NoRecoil || CheatState.SuperAccuracy)
                    suppress = 1f;
                else
                    suppress = 1f - CheatStateESP.RecoilScale;
                __instance.transform.rotation = Quaternion.Slerp(
                    __instance.transform.rotation, noRecoilRot, suppress);
            }
            catch { }
        }
        // 稳定瞄准:LateUpdate 结束后再清一次 sway,防游戏在 LateUpdate 里重写(仅持枪时)
        if (CheatState.NoAimSway)
        {
            try
            {
                var pm = GameManager.GetPlayerManagerComponent();
                if (pm != null && pm.m_ItemInHands != null && pm.m_ItemInHands.GetComponent<GunItem>() != null)
                {
                    __instance.ShakeAmplitude = UnityEngine.Vector3.zero;
                    __instance.ShakeSpeed = 0f;
                    __instance.BobAmplitude = UnityEngine.Vector4.zero;
                    __instance.m_CurrentMaxAmbientSwayAngle = 0f;
                    __instance.m_CurrentAmbientSwaySpeed = 0f;
                }
            }
            catch { }
        }
        // GunZoom:瞄准时滚轮缩放 FOV
        GunZoomState.ApplyZoom(__instance);
    }
}

// —— 瞄准滚轮缩放:整合 GunZoom ——
internal static class GunZoomState
{
    public static float RifleMult = 1f;
    public static float RevolverMult = 1f;
    private static float _lerpMult = 1f;
    private static float _originalMainFOV = -1f;
    private static float _originalWeaponFOV = -1f;
    private static float _baseSensitivity = -1f;
    private static bool _wasZooming;
    private const float ZoomIncrement = 1.2f;
    private const float ZoomSpeed = 15f;

    public static void UpdateScroll()
    {
        var s = ModMain.Settings;
        if (s == null || !s.GunZoomEnabled) return;
        var cam = GameManager.m_vpFPSCamera;
        if (cam == null || !cam.IsZoomed) { _lerpMult = 1f; return; }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll == 0f) return;

        var pm = GameManager.GetPlayerManagerComponent();
        if (pm == null || pm.m_ItemInHands == null) return;
        string name = ((UnityEngine.Object)pm.m_ItemInHands).name.ToLower();
        bool rifle = name.Contains("rifle");
        bool revolver = name.Contains("revolver");
        if (!rifle && !revolver) return;

        if (rifle)
        {
            if (scroll > 0f) RifleMult /= ZoomIncrement; else RifleMult *= ZoomIncrement;
            RifleMult = Mathf.Clamp(RifleMult, 0.05f, 1f);
        }
        else
        {
            if (scroll > 0f) RevolverMult /= ZoomIncrement; else RevolverMult *= ZoomIncrement;
            RevolverMult = Mathf.Clamp(RevolverMult, 0.33f, 1f);
        }
    }

    public static void ApplyZoom(vp_FPSCamera cam)
    {
        try
        {
            var s = ModMain.Settings;
            if (s == null || !s.GunZoomEnabled) return;
            if (!cam.IsZoomed)
            {
                if (_wasZooming) RestoreFOV(cam);
                _lerpMult = 1f;
                return;
            }

            var pm = GameManager.GetPlayerManagerComponent();
            if (pm == null || pm.m_ItemInHands == null) { if (_wasZooming) RestoreFOV(cam); return; }
            string name = ((UnityEngine.Object)pm.m_ItemInHands).name.ToLower();
            bool rifle = name.Contains("rifle");
            bool revolver = name.Contains("revolver");
            if (!rifle && !revolver) { if (_wasZooming) RestoreFOV(cam); return; }

            if (_baseSensitivity <= 0f)
            {
                _baseSensitivity = cam.MouseSensitivity;
                var mainCam = cam.GetComponent<Camera>();
                if (mainCam != null) _originalMainFOV = mainCam.fieldOfView;
                if (cam.m_WeaponCamera != null) _originalWeaponFOV = cam.m_WeaponCamera.fieldOfView;
            }

            float target = rifle ? RifleMult : RevolverMult;
            _lerpMult = Mathf.Lerp(_lerpMult, target, Time.deltaTime * ZoomSpeed);

            var mc = cam.GetComponent<Camera>();
            if (mc != null) mc.fieldOfView = _originalMainFOV * _lerpMult;
            if (cam.m_WeaponCamera != null) cam.m_WeaponCamera.fieldOfView = _originalWeaponFOV * _lerpMult;
            cam.MouseSensitivity = _baseSensitivity * Mathf.Clamp(_lerpMult + 0.15f, 0f, 1f);
            _wasZooming = true;
        }
        catch { }
    }

    private static void RestoreFOV(vp_FPSCamera cam)
    {
        try
        {
            if (_baseSensitivity > 0f) cam.MouseSensitivity = _baseSensitivity;
            var mc = cam.GetComponent<Camera>();
            if (mc != null && _originalMainFOV > 0f) mc.fieldOfView = _originalMainFOV;
            if (cam.m_WeaponCamera != null && _originalWeaponFOV > 0f) cam.m_WeaponCamera.fieldOfView = _originalWeaponFOV;
        }
        catch { }
        _wasZooming = false;
    }
}

// —— 稳定瞄准:锁定武器位置/旋转 + 归零 bob/shake/sway 参数 ——
internal static class Patch_FPSWeapon_SteadyAim
{
    [ThreadStatic] private static Vector3 _savedPos;
    [ThreadStatic] private static Quaternion _savedRot;
    [ThreadStatic] private static bool _hasSaved;

    private static bool IsHoldingGun()
    {
        try
        {
            var pm = GameManager.GetPlayerManagerComponent();
            if (pm == null || pm.m_ItemInHands == null) return false;
            return pm.m_ItemInHands.GetComponent<GunItem>() != null;
        }
        catch { return false; }
    }

    internal static void Prefix(vp_FPSWeapon __instance)
    {
        if (!CheatState.NoAimSway && !CheatState.SuperAccuracy) { _hasSaved = false; return; }
        if (!IsHoldingGun()) { _hasSaved = false; return; }
        try
        {
            var t = __instance.transform;
            if (t != null) { _savedPos = t.localPosition; _savedRot = t.localRotation; _hasSaved = true; }
            else _hasSaved = false;
        }
        catch { _hasSaved = false; }
    }

    internal static void Postfix(vp_FPSWeapon __instance)
    {
        if (!CheatState.NoAimSway && !CheatState.SuperAccuracy) return;
        if (!_hasSaved) return;
        try { __instance.transform.localPosition = _savedPos; } catch { }
        try { __instance.transform.localRotation = _savedRot; } catch { }
        try { __instance.BobAmplitude = Vector4.zero; } catch { }
        try { __instance.ShakeAmplitude = Vector3.zero; } catch { }
        try { __instance.ShakeSpeed = 0f; } catch { }
        try { __instance.SwayMaxFatigue = 0f; } catch { }
        try { __instance.SwayStartFatigue = 999f; } catch { }
    }
}


// ═══════════════════════════════════════════════════════════════════
//   v2.7.86 新增功能 —— 随意生火 / 生火材料不减 / 科技背包 / 火把满值 / 无条件冲刺 / 无限体力
// ═══════════════════════════════════════════════════════════════════


// —— 无限体力:PlayerMovement.AddSprintStamina 设 stamina = 100 ——
// CT 实现:patch AddSprintStamina → mov [rbx+80],(float)100
// Harmony 做法:Postfix 设 stamina 字段为满值
internal static class Patch_AddSprintStamina
{
    internal static void Postfix(PlayerMovement __instance)
    {
        if (!CheatState.InfiniteStamina) return;
        try { __instance.m_SprintStamina = 100f; } catch { }
    }
}

// —— 速度倍率:vp_FPSController.GetSlopeMultiplier Postfix ——
// 复制自 SonicMode:根据移动状态(蹲/走/冲刺)乘以对应系数
internal static class Patch_SpeedMultiplier
{
    internal static void Postfix(ref float __result)
    {
        try
        {
            var s = ModMain.Settings;
            if (s == null || !s.SpeedTweaksEnabled) return;
            var pm = GameManager.GetPlayerManagerComponent();
            if (pm == null) return;
            if (pm.PlayerIsSprinting())
                __result *= s.SprintSpeed;
            else if (pm.PlayerIsCrouched())
                __result *= s.CrouchSpeed;
            else
                __result *= s.WalkSpeed;
        }
        catch { }
    }
}

// —— 体力恢复:PlayerMovement.Update Postfix ——
// 复制自 SonicMode:每帧设置恢复速率和消耗速率
internal static class Patch_StaminaTweaks
{
    private static float _initRecovery;
    private static float _initSeconds;
    private static bool _inited;

    internal static void Postfix(PlayerMovement __instance)
    {
        try
        {
            var s = ModMain.Settings;
            if (s == null) return;
            if (!_inited)
            {
                _initRecovery = __instance.m_SprintStaminaRecoverPerHour;
                _initSeconds = __instance.m_SecondsNotSprintingBeforeRecovery;
                _inited = true;
            }
            if (!s.SpeedTweaksEnabled) return;
            __instance.m_SprintStaminaRecoverPerHour = _initRecovery * s.StaminaRecharge;
            __instance.m_SecondsNotSprintingBeforeRecovery = _initSeconds * s.StaminaRecoveryDelay;
        }
        catch { }
    }
}

// —— 火把燃烧时间:Panel_FeedFire.Enable Postfix ——
// 复制自 TorchTweaker:设置 min/max condition
internal static class Patch_TorchCondition
{
    internal static void Postfix(Panel_FeedFire __instance)
    {
        try
        {
            var s = ModMain.Settings;
            if (s == null) return;
            __instance.m_MinNormalizedTorchCondition = s.TorchMinCondition;
            __instance.m_MaxNormalizedTorchCondition = s.TorchMaxCondition;
        }
        catch { }
    }
}

// —— 火把 condition:Panel_ActionPicker.Enable Postfix ——
// TorchTweaker 原版同时 patch 这两个面板,保证从地上/靠近火堆等交互也生效
internal static class Patch_TorchCondition_ActionPicker
{
    internal static void Postfix()
    {
        try
        {
            var s = ModMain.Settings;
            if (s == null) return;
            var panel = InterfaceManager.GetPanel<Panel_FeedFire>();
            if (panel == null) return;
            panel.m_MinNormalizedTorchCondition = s.TorchMinCondition;
            panel.m_MaxNormalizedTorchCondition = s.TorchMaxCondition;
        }
        catch { }
    }
}


// —— 交互距离:PlayerManager.ComputeModifiedPickupRange Postfix ——
// 复制自 StretchArmstrong:乘以滑块倍率
internal static class Patch_PickupRange
{
    internal static void Postfix(ref float __result)
    {
        try
        {
            var s = ModMain.Settings;
            if (s != null) __result *= s.PickupRange;
        }
        catch { }
    }
}

// —— 脚步静音:FootStepSounds.PlayFootStepSound Prefix ——
// 复制自 SilentWalker:toggle on 时完全不播放脚步声
internal static class Patch_SilentFootsteps
{
    internal static bool Prefix() => !CheatState.SilentFootsteps;
}

// 复制自 SilentWalker:按 RTPC ID 缩放背包物品发声音量
internal static class Patch_SilentWalker_RTPC
{
    internal static void Prefix(uint rtpcID, ref float rtpcValue)
    {
        var s = ModMain.Settings;
        if (s == null) return;
        int pct;
        switch (rtpcID)
        {
            case 135115684u: pct = s.InvWeightMetalVol; break;
            case 330491720u: pct = s.InvWeightWoodVol; break;
            case 1721946080u: pct = s.InvWeightWaterVol; break;
            case 2064316281u: pct = s.InvWeightGeneralVol; break;
            default: return;
        }
        rtpcValue *= pct / 100f;
    }
}

// —— 衰减倍率:GearItem.Degrade Prefix ——
// 复制自 GearDecayModifier:根据物品类型乘以对应衰减系数
internal static class Patch_GearDegrade
{
    internal static bool Prefix(GearItem __instance, ref float hp)
    {
        try
        {
            // v2.8.1: 坠落衣物保护 — IL2CPP 可能忽略 return false,改用 hp=0 让 Degrade 无效化
            if (FallClothingGuard.Active) { hp = 0f; return true; }
            if (CheatState.InfiniteDurability) { hp = 0f; return true; }
            if (CheatState.QuickCraft) { hp = 0f; return true; }
            if (__instance == null) return true;

            float mult = DecayState.GeneralDecay;
            string name = __instance.gameObject.name;

            // Check specific item types by name first
            if (name.Contains("Bedroll")) mult = DecayState.BedrollDecay;
            else if (name.Contains("Travois")) mult = DecayState.TravoisDecay;
            else if (name.Contains("Arrow")) mult = DecayState.ArrowDecay;
            else if (name.Contains("Snare")) mult = DecayState.SnareDecay;
            else if (name.Contains("FlareGunAmmo")) mult = DecayState.FlareGunAmmoDecay;
            else if (name.Contains("CookingPot") || name.Contains("RecycledCan")) mult = DecayState.CookingPotDecay;
            else if (name.Contains("Hide") || name.Contains("Pelt") || name.Contains("Gut") || name.Contains("Leather"))
                mult = DecayState.HideDecay;
            else if (name.Contains("FirstAid") || name.Contains("Bandage") || name.Contains("Antibiotic") || name.Contains("Painkiller"))
                mult = DecayState.FirstAidDecay;
            else if (name.Contains("WaterPurifier")) mult = DecayState.WaterPurifierDecay;
            else if (__instance.m_DegradeOnUse != null)
            {
                if (__instance.m_GunItem != null) mult = DecayState.GunDecay;
                else if (__instance.m_BowItem != null) mult = DecayState.BowDecay;
                else if (name.Contains("Whetstone") || name.Contains("SharpeningStone"))
                    mult = DecayState.WhetstoneDecay;
                else if (name.Contains("CanOpener")) mult = DecayState.CanOpenerDecay;
                else if (name.Contains("Prybar")) mult = DecayState.PrybarDecay;
                else if (name.Contains("FireStriker") || name.Contains("Matches") || name.Contains("Flare"))
                    mult = DecayState.FirestartingDecay;
                else if (__instance.m_ToolsItem != null)
                {
                    if (name.Contains("Hatchet") || name.Contains("Knife") || name.Contains("Hacksaw"))
                        mult = DecayState.BodyHarvestDecay;
                    else
                        mult = DecayState.ToolsDecay;
                }
                else mult = DecayState.OnUseDecay;
            }
            else if (__instance.m_FoodItem != null)
            {
                if (name.Contains("Coffee") || name.Contains("Tea") || name.Contains("HerbalTea"))
                    mult = DecayState.CoffeeTeaDecay;
                else if (__instance.m_FoodItem.m_IsDrink) mult = DecayState.DrinksDecay;
                else if (name.Contains("Canned") || name.Contains("PinnacleCanPeaches") || name.Contains("TomatoSoup") || name.Contains("DogFood"))
                    mult = DecayState.CannedFoodDecay;
                else if (name.Contains("PackagedFood") || name.Contains("GranolaBar") || name.Contains("EnergyBar") || name.Contains("CattailStalk"))
                    mult = DecayState.PackagedFoodDecay;
                else if (name.Contains("CookingFat") || name.Contains("BearFat"))
                    mult = DecayState.FatDecay;
                else if (name.Contains("CuredFish") || name.Contains("Jerky") && name.Contains("Fish"))
                    mult = DecayState.CuredFishDecay;
                else if (__instance.m_FoodItem.m_IsRawMeat && name.Contains("Fish"))
                    mult = DecayState.RawFishDecay;
                else if (__instance.m_FoodItem.m_IsMeat && name.Contains("Fish"))
                    mult = DecayState.CookedFishDecay;
                else if (name.Contains("Cured") || name.Contains("Jerky"))
                    mult = DecayState.CuredMeatDecay;
                else if (name.Contains("Flour") || name.Contains("Yeast") || name.Contains("Salt") || name.Contains("Sugar"))
                    mult = DecayState.IngredientsDecay;
                else if (__instance.m_FoodItem.m_IsRawMeat) mult = DecayState.RawMeatDecay;
                else if (__instance.m_FoodItem.m_IsMeat) mult = DecayState.CookedMeatDecay;
                else mult = DecayState.OtherFoodDecay;
            }
            else if (__instance.m_ClothingItem != null)
            {
                mult = DecayState.ClothingDecayRate;
            }

            if (!__instance.m_BeenInspected && !__instance.m_BeenInPlayerInventory)
                mult *= DecayState.DecayBeforePickup;

            hp *= mult;
        }
        catch { }
        return true;
    }
}

// —— 随意生火:绕过室内生火限制 ——
// CT 实现:patch InputManager.CanStartFireIndoors → NOP 2 bytes
// Harmony 做法:Prefix 强制 return true
internal static class Patch_FireAnywhere
{
    internal static bool Prefix(ref bool __result)
    {
        if (CheatState.FireAnywhere)
        {
            __result = true;
            return false; // skip original
        }
        return true;
    }
}

// —— 生火/修理材料不减:PlayerManager.ConsumeUnitFromInventory(void) skip ——
internal static class Patch_ConsumeUnit
{
    internal static bool Prefix()
    {
        if (CheatState.FreeFireFuel || CheatState.FreeRepair) return false;
        return true;
    }
}

// —— 自定义背包负重:Encumber.Update Postfix 直接覆盖原版 30kg 上限 ——
// v3.0.4 fix: 移除 dirty-check —— vanilla Encumber.Update 每帧重置字段,
//   dirty-check 第一次写后被 vanilla 覆盖 → 永远 return → 看似无效。
//   照抄 UniversalTweaks 写法: 每帧 Postfix 写 7 字段(开销可忽略,Update 1 次/帧)
internal static class Patch_TechBackpack
{
    internal static void Postfix(Encumber __instance)
    {
        try
        {
            if (__instance == null || !CheatState.TechBackpack) return;
            float kg = CheatState.TechBackpackKg;
            __instance.m_MaxCarryCapacity = ItemWeight.FromKilograms(kg);
            __instance.m_MaxCarryCapacityWhenExhausted = ItemWeight.FromKilograms(kg * 0.5f);
            __instance.m_NoSprintCarryCapacity = ItemWeight.FromKilograms(kg + 10f);
            __instance.m_NoWalkCarryCapacity = ItemWeight.FromKilograms(kg + 30f);
            __instance.m_EncumberLowThreshold = ItemWeight.FromKilograms(kg + 1f);
            __instance.m_EncumberMedThreshold = ItemWeight.FromKilograms(kg + 10f);
            __instance.m_EncumberHighThreshold = ItemWeight.FromKilograms(kg + 30f);
        }
        catch { }
    }
}

// —— 关闭自定义负重时手动还原原版值(由 Menu UI 调) ——
internal static class EncumberVanilla
{
    internal static void Reset()
    {
        try
        {
            var e = GameManager.GetEncumberComponent();
            if (e == null) return;
            e.m_MaxCarryCapacity = ItemWeight.FromKilograms(30f);
            e.m_MaxCarryCapacityWhenExhausted = ItemWeight.FromKilograms(15f);
            e.m_NoSprintCarryCapacity = ItemWeight.FromKilograms(40f);
            e.m_NoWalkCarryCapacity = ItemWeight.FromKilograms(60f);
            e.m_EncumberLowThreshold = ItemWeight.FromKilograms(31f);
            e.m_EncumberMedThreshold = ItemWeight.FromKilograms(40f);
            e.m_EncumberHighThreshold = ItemWeight.FromKilograms(60f);
        }
        catch { }
    }
}

// —— 火把满值:TorchItem.Update 重置燃烧时间 + HP ——
internal static class Patch_TorchFullValue
{
    internal static void Prefix(TorchItem __instance)
    {
        if (!CheatState.TorchFullValue) return;
        if (!__instance.IsBurning()) return;
        __instance.m_ElapsedBurnMinutes = 0f;
        var gi = __instance.m_GearItem;
        if (gi != null) gi.m_CurrentHP = 100f;
    }
}

// ——————————— Tick-based 补丁 ———————————
// OnUpdate 每帧/每 N 帧扫一次实例,直接改字段。
internal static class CheatsTick
{
    // ——— 武器:无限弹药 / 永不卡壳 / 无后坐(字段层 fallback)———
    // v2.7.92 性能:InfiniteAmmo/NoJam 需扫全场景;其余只需手持武器,避免 FindObjectsOfType
    public static void TickGuns()
    {
        if (!CheatState.InfiniteAmmo && !CheatState.NoJam && !CheatState.NoRecoil && !CheatState.SuperAccuracy && !CheatState.NoAimSway) return;
        try
        {
            if (CheatState.NoJam)
            {
                try { GunItem.m_ForceNoJam = true; } catch { }
            }

            // 只有 InfiniteAmmo/NoJam 需要遍历所有枪(补满全部弹夹/清卡壳)
            if (CheatState.InfiniteAmmo || CheatState.NoJam)
            {
                var guns = UnityEngine.Object.FindObjectsOfType<GunItem>();
                if (guns != null)
                {
                    foreach (var g in guns)
                    {
                        if (g == null) continue;
                        try
                        {
                            if (CheatState.NoJam) try { g.m_IsJammed = false; } catch { }
                            if (CheatState.InfiniteAmmo && g.m_ClipSize > 0 && g.m_RoundsInClip < g.m_ClipSize)
                                g.m_RoundsInClip = g.m_ClipSize;
                        }
                        catch { }
                    }
                }
            }

            if (CheatState.NoRecoil || CheatState.SuperAccuracy || CheatState.NoAimSway)
            {
                var pm = GameManager.GetPlayerManagerComponent();
                var gi = pm?.m_ItemInHands;
                var g = gi != null ? gi.GetComponent<GunItem>() : null;
                if (g != null)
                {
                    if (CheatState.NoRecoil || CheatState.SuperAccuracy)
                    {
                        try { g.m_PitchRecoilMin = 0f; } catch { }
                        try { g.m_PitchRecoilMax = 0f; } catch { }
                        try { g.m_YawRecoilMin = 0f; } catch { }
                        try { g.m_YawRecoilMax = 0f; } catch { }
                        try { g.m_SwayValueZeroFatigue = 0f; } catch { }
                        try { g.m_SwayValueMaxFatigue = 0f; } catch { }
                    }
                    if (CheatState.SuperAccuracy)
                    {
                        try { g.m_SwayValue = 0f; } catch { }
                    }
                    if (CheatState.NoAimSway)
                    {
                        try { g.m_SwayValueZeroFatigue = 0f; } catch { }
                        try { g.m_SwayValueMaxFatigue = 0f; } catch { }
                        try { g.m_SwayValue = 0f; } catch { }
                    }
                }
            }
        }
        catch { }
    }

    // ——— 摄像机:NoRecoil / NoAimSway / NoAimShake / NoBreathSway / NoAimStamina ———
    // v2.7.2 优化:(1) 所有 FieldInfo lazy 缓存到 static,不再每次 GetField;
    //             (2) 有任一 toggle ON 才跑 full path,全关且上次也关 → 早退 0 开销;
    //             (3) 从每帧改成 ModMain 里调用 —— 调用频率由 ModMain 决定(30 帧 = 0.5s)
    // v2.7.83 修:加诊断日志 + 多重 fallback 武器查找 + 降频到 30 帧
    private static bool _lastAnyAimToggle = false;
    private static bool _aimDiagDone = false; // 只打一次诊断日志
    // v2.7.84:vp_FPSCamera 缓存 + 兜底查找节流
    private static vp_FPSCamera _cachedCam = null;
    private static int _lastCamScanFrame = -999;
    private static int _camNullLogCount = 0;
    // v2.7.84 重写:只保留 FindWeapon 需要的 + RecoilSpring struct 操作需要的反射
    private static FieldInfo _fi_currentWeapon;   // FindWeapon 路径 1 用
    private static FieldInfo _fi_recoilSpring;    // TickCamera NoRecoil struct 操作用
    private static FieldInfo _fi_rsCur, _fi_rsTgt, _fi_rsVel;
    private static bool _reflectionInited = false;

    // v2.7.84:跨场景时清相机缓存 + 重置诊断开关 —— 从 ModMain.OnSceneWasInitialized 调
    public static void InvalidateCameraCache()
    {
        _cachedCam = null;
        _aimDiagDone = false;
        _camNullLogCount = 0;
        _lastCamScanFrame = -999;
    }

    // v2.7.83:多路径查找武器实例(反射 → PlayerManager → GetComponentInChildren)
    private static object FindWeapon(vp_FPSCamera cam)
    {
        // 路径 1:反射 m_CurrentWeapon 字段
        if (_fi_currentWeapon != null)
        {
            try
            {
                var w = _fi_currentWeapon.GetValue(cam);
                if (w != null) return w;
            }
            catch { }
        }
        // 路径 2:尝试其他字段名(m_Weapon / m_ActiveWeapon / currentWeapon)
        try
        {
            var camT = cam.GetType();
            foreach (var name in new[] { "m_Weapon", "m_ActiveWeapon", "currentWeapon", "Weapon" })
            {
                var fi = camT.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (fi != null)
                {
                    var w = fi.GetValue(cam);
                    if (w != null)
                    {
                        ModMain.Log?.Msg($"[Aim] Found weapon via fallback field '{name}'");
                        return w;
                    }
                }
            }
        }
        catch { }
        // 路径 3:PlayerManager.GetWeaponItem() 等公开方法
        try
        {
            var pm = GameManager.GetPlayerManagerComponent();
            if (pm != null)
            {
                var pmT = pm.GetType();
                foreach (var mname in new[] { "GetWeaponItem", "GetCurrentWeapon", "GetActiveWeapon" })
                {
                    var mi = pmT.GetMethod(mname, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (mi != null)
                    {
                        var w = mi.Invoke(pm, null);
                        if (w != null)
                        {
                            ModMain.Log?.Msg($"[Aim] Found weapon via PlayerManager.{mname}()");
                            return w;
                        }
                    }
                }
            }
        }
        catch { }
        // 路径 4:从 camera 的 GameObject 子层级找 vp_FPSWeapon 组件
        try
        {
            var camGo = cam.gameObject;
            if (camGo != null)
            {
                var weapons = camGo.GetComponentsInChildren<vp_FPSWeapon>(true);
                if (weapons != null && weapons.Count > 0)
                {
                    ModMain.Log?.Msg($"[Aim] Found weapon via GetComponentsInChildren<vp_FPSWeapon> ({weapons.Count})");
                    return weapons[0];
                }
                // 也试 MonoBehaviour 基类搜索
                var allMono = camGo.GetComponentsInChildren<MonoBehaviour>(true);
                if (allMono != null)
                {
                    foreach (var mb in allMono)
                    {
                        if (mb == null) continue;
                        var n = mb.GetType().Name;
                        if (n.Contains("Weapon") || n.Contains("Gun") || n.Contains("Rifle"))
                        {
                            ModMain.Log?.Msg($"[Aim] Found weapon-like component '{n}' on child");
                            return mb;
                        }
                    }
                }
            }
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[Aim] path4 children: {ex.Message}"); }
        return null;
    }

    // v2.7.84 简化:只初始化 RecoilSpring struct 操作需要的 FieldInfo。
    //   武器 bool 开关走 SetDisable* 静态方法(反编译确认存在);
    //   武器/camera float 字段直接通过实例赋值,不需要反射。
    private static void EnsureReflectionInited()
    {
        if (_reflectionInited) return;
        var camT = typeof(vp_FPSCamera);
        _fi_currentWeapon = _fi_currentWeapon ?? camT.GetField("m_CurrentWeapon", BindingFlags.Instance | BindingFlags.Public);
        _fi_recoilSpring  = _fi_recoilSpring  ?? camT.GetField("m_RecoilSpring",  BindingFlags.Instance | BindingFlags.Public);
        if (_fi_recoilSpring != null)
        {
            var rsT = _fi_recoilSpring.FieldType;
            _fi_rsCur = _fi_rsCur ?? rsT.GetField("m_Current",  BindingFlags.Instance | BindingFlags.Public);
            _fi_rsTgt = _fi_rsTgt ?? rsT.GetField("m_Target",   BindingFlags.Instance | BindingFlags.Public);
            _fi_rsVel = _fi_rsVel ?? rsT.GetField("m_Velocity", BindingFlags.Instance | BindingFlags.Public);
        }
        _reflectionInited = true;
    }

    // v2.7.84 重写:反编译确认 vp_FPSWeapon 的 Disable* 是 static bool + Set* 静态方法,
    //   不再需要实例反射。camera float 字段直接写实例属性。只剩 m_RecoilSpring(struct) 需要 FieldInfo。
    public static void TickCamera()
    {
        bool anyOn = CheatState.NoAimSway || CheatState.NoAimStamina || CheatState.NoRecoil || CheatState.SuperAccuracy;
        // 全关 AND 上次也全关 → 零开销早退
        if (!anyOn && !_lastAnyAimToggle) return;

        try
        {
            // v2.7.84:先 cache → GameManager getter → scene-wide scan 兜底
            var cam = _cachedCam;
            if (cam == null)
            {
                cam = GameManager.GetVpFPSCamera();
                if (cam == null && Time.frameCount - _lastCamScanFrame > 30)
                {
                    _lastCamScanFrame = Time.frameCount;
                    try { cam = UnityEngine.Object.FindObjectOfType<vp_FPSCamera>(); } catch { }
                    if (cam != null)
                        ModMain.Log?.Msg("[AimDiag] vp_FPSCamera via FindObjectOfType (GetVpFPSCamera=null)");
                }
                if (cam != null) _cachedCam = cam;
            }
            if (cam == null)
            {
                if ((++_camNullLogCount % 60) == 1)
                    ModMain.Log?.Warning($"[AimDiag] cam=null (try #{_camNullLogCount}), 未进游戏场景?");
                return;
            }

            object weapon = null;
            EnsureReflectionInited();
            weapon = FindWeapon(cam);

            // v2.7.83:首次有 toggle ON 时打诊断日志
            if (!_aimDiagDone && anyOn)
            {
                _aimDiagDone = true;
                // 相机继承链字段 dump
                try
                {
                    var t = cam.GetType(); int depth = 0;
                    while (t != null && depth < 5)
                    {
                        var fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                        var sb = new System.Text.StringBuilder($"[AimDiag] Cam depth={depth} type={t.FullName} fields({fields.Length}):");
                        foreach (var f in fields) sb.Append($" {f.Name}({f.FieldType.Name})");
                        ModMain.Log?.Msg(sb.ToString());
                        t = t.BaseType; depth++;
                    }
                }
                catch (Exception ex) { ModMain.Log?.Warning($"[AimDiag] cam dump: {ex.Message}"); }

                if (weapon != null)
                {
                    try
                    {
                        var wt = weapon.GetType(); int wd = 0;
                        while (wt != null && wd < 5)
                        {
                            var wfields = wt.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                            var wsb = new System.Text.StringBuilder($"[AimDiag] Weap depth={wd} type={wt.FullName} fields({wfields.Length}):");
                            foreach (var wf in wfields) wsb.Append($" {wf.Name}({wf.FieldType.Name})");
                            ModMain.Log?.Msg(wsb.ToString());
                            wt = wt.BaseType; wd++;
                        }
                    }
                    catch (Exception ex) { ModMain.Log?.Warning($"[AimDiag] weapon dump: {ex.Message}"); }
                }
                else ModMain.Log?.Warning("[AimDiag] weapon = NULL — FindWeapon() all 4 paths failed. No weapon equipped?");

                // dump PlayerManager 的 weapon 相关方法
                try
                {
                    var pm = GameManager.GetPlayerManagerComponent();
                    if (pm != null)
                    {
                        var pmT = pm.GetType();
                        var pmMethods = pmT.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        var pmSb = new System.Text.StringBuilder($"[AimDiag] PlayerManager type={pmT.FullName} methods({pmMethods.Length}):");
                        foreach (var m in pmMethods)
                        {
                            if (m.Name.Contains("Weapon") || m.Name.Contains("Gun") || m.Name.Contains("Rifle")
                                || m.Name.Contains("Equip") || m.Name.Contains("Active") || m.Name.Contains("Current"))
                                pmSb.Append($" {m.Name}({m.GetParameters().Length}p)");
                        }
                        ModMain.Log?.Msg(pmSb.ToString());
                    }
                    else ModMain.Log?.Warning("[AimDiag] PlayerManager = null");
                }
                catch (Exception ex) { ModMain.Log?.Warning($"[AimDiag] PM dump: {ex.Message}"); }

                // v2.7.84:静态 bool 字段诊断 —— 确认 Set* 方法可用
                var diags = new System.Text.StringBuilder("[AimDiag] Static Disable fields: ");
                try { diags.Append($"Sway={vp_FPSWeapon.m_DisableAimSway} "); } catch (Exception ex) { diags.Append($"Sway=ERR({ex.Message}) "); }
                try { diags.Append($"Shake={vp_FPSWeapon.m_DisableAimShake} "); } catch (Exception ex) { diags.Append($"Shake=ERR({ex.Message}) "); }
                try { diags.Append($"Breath={vp_FPSWeapon.m_DisableAimBreathing} "); } catch (Exception ex) { diags.Append($"Breath=ERR({ex.Message}) "); }
                try { diags.Append($"Stamina={vp_FPSWeapon.m_DisableAimStamina} "); } catch (Exception ex) { diags.Append($"Stamina=ERR({ex.Message}) "); }
                try { diags.Append($"DOF={vp_FPSWeapon.m_DisableDepthOfField} "); } catch (Exception ex) { diags.Append($"DOF=ERR({ex.Message}) "); }
                try { diags.Append($"CamAmbSway={vp_FPSCamera.m_DisableAmbientSway} "); } catch (Exception ex) { diags.Append($"CamAmbSway=ERR({ex.Message}) "); }
                diags.Append($"recoilSpring={(_fi_recoilSpring != null ? "OK" : "null")} ");
                diags.Append($"weapon={weapon?.GetType().FullName ?? "null"}");
                ModMain.Log?.Msg(diags.ToString());
            }

            // ——— 静态 bool 开关:每帧同步(toggle ON=true / OFF=false) ———
            // 反编译确认:vp_FPSWeapon.m_DisableAimSway 等是 static bool,Set* 是 static 方法
            // 空 weapon 不影响 —— static 字段全局生效,不依赖实例
            // v2.7.84:NoAimSway 合并了 sway+shake+breath 三个 toggle
            try { vp_FPSWeapon.m_DisableAimSway      = CheatState.NoAimSway;    } catch (Exception ex) { ModMain.Log?.Warning($"[Aim] SetAimSway: {ex.Message}"); }
            try { vp_FPSWeapon.m_DisableAimShake     = CheatState.NoAimSway;    } catch (Exception ex) { ModMain.Log?.Warning($"[Aim] SetAimShake: {ex.Message}"); }
            try { vp_FPSWeapon.m_DisableAimBreathing  = CheatState.NoAimSway;    } catch (Exception ex) { ModMain.Log?.Warning($"[Aim] SetAimBreath: {ex.Message}"); }
            try { vp_FPSWeapon.m_DisableAimStamina   = CheatState.NoAimStamina; } catch (Exception ex) { ModMain.Log?.Warning($"[Aim] SetAimStamina: {ex.Message}"); }
            try { vp_FPSCamera.m_DisableAmbientSway   = CheatState.NoAimSway;    } catch (Exception ex) { ModMain.Log?.Warning($"[Aim] m_DisableAmbientSway: {ex.Message}"); }

            // v2.7.92 性能:camera sway + recoil spring 清零已由 DynamicPatch 每帧 Prefix 覆盖,
            //   此处只保留 weapon 实例反射(DynamicPatch 的 SteadyAim 也做,但这里是兜底)
            if (!anyOn)
            {
                _lastAnyAimToggle = false;
                return;
            }

            if (CheatState.NoAimSway && weapon != null)
            {
                try
                {
                    var wBob = weapon.GetType().GetField("BobAmplitude", BindingFlags.Instance | BindingFlags.Public);
                    if (wBob != null) wBob.SetValue(weapon, UnityEngine.Vector4.zero);
                } catch { }
                try
                {
                    var wShake = weapon.GetType().GetField("ShakeAmplitude", BindingFlags.Instance | BindingFlags.Public);
                    if (wShake != null) wShake.SetValue(weapon, UnityEngine.Vector3.zero);
                } catch { }
                try
                {
                    var wShakeSpd = weapon.GetType().GetField("ShakeSpeed", BindingFlags.Instance | BindingFlags.Public);
                    if (wShakeSpd != null) wShakeSpd.SetValue(weapon, 0f);
                } catch { }
            }

            _lastAnyAimToggle = true;
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[TickCamera] {ex.Message}"); }
    }

    // ——— 动物不能动 / 隐身 ———
    // v2.7.9 重写 Stealth:之前 m_DisableScanForTargets 单靠它不够 ——
    // 游戏内部的检测路径还有别的入口。现在强力方案:
    //   1) 设 m_DisableScanForTargets = true(阻止新增探测)
    //   2) 对已经在 Attack/Stalking/Investigate/HoldGround 的 AI,立即 SetAiMode(Wander)
    //      —— 公开方法,可靠;等于把玩家从威胁列表踢出
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
                    // v2.7.74 Stealth 关闭边沿:把之前被强推到 Flee 的动物拉回 Wander,让 AI 重新按 fear/感知判断
                    else if (_lastTickedStealth)
                    {
                        try
                        {
                            if (ai.GetAiMode() == AiMode.Flee)
                            {
                                try { ai.ClearTarget(); } catch { }
                                ai.SetAiMode(AiMode.Wander);
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
    // v2.7.62 仿 CT:设"合理低值"而非"max/0",让游戏的吃喝睡 UI 认为还有空间,交互可用
    //   - Fatigue: 10(CT 原值)—— 玩家仍能触发睡觉(0 时游戏可能不让睡)
    //   - Hunger: 2450 cal(CT 原值,≈ 1 day starting reserve)—— 玩家仍能吃东西
    //   - Thirst: 0 —— UI 显示不渴即可,喝水仍能触发(游戏不 check "thirst > 0 才能喝")
    //   - Freezing: 0 —— 同上
    //   - GodMode 走 max HP 路径(玩家不会死,与吃喝睡 UI 无关)
    private static bool _frostbiteWasOn;
    public static void TickStatus()
    {
        // NoFrostbiteRisk: 只用 m_SuppressFrostbite 标志,不清列表(清列表会破坏游戏状态导致关不掉)
        if (CheatState.NoFrostbiteRisk)
        {
            _frostbiteWasOn = true;
            try
            {
                var fb = GameManager.GetFrostbiteComponent();
                if (fb != null) fb.m_SuppressFrostbite = true;
            }
            catch { }
        }
        else if (_frostbiteWasOn)
        {
            _frostbiteWasOn = false;
            try
            {
                var fb = GameManager.GetFrostbiteComponent();
                if (fb != null) fb.m_SuppressFrostbite = false;
            }
            catch { }
        }

        if (!CheatState.NoFatigue && !CheatState.NoHunger
            && !CheatState.NoThirst && !CheatState.AlwaysWarm
            && !CheatState.GodMode && !CheatState.FatigueBuff) return;

        try
        {
            if (CheatState.NoFatigue || CheatState.GodMode)
            {
                var fat = GameManager.GetFatigueComponent();
                if (fat != null) { try { fat.m_CurrentFatigue = 10f; } catch { } }
            }
            else if (CheatState.FatigueBuff)
            {
                // 保持原生 buff 的剩余时间不归零,防止游戏倒计时关掉 buff
                try
                {
                    var pm = GameManager.GetPlayerManagerComponent();
                    if (pm != null) pm.m_FatigueBuffHoursRemaining = 100f;
                }
                catch { }
            }
            if (CheatState.NoHunger || CheatState.GodMode)
            {
                var h = GameManager.GetHungerComponent();
                if (h != null) { try { h.m_CurrentReserveCalories = 2450f; } catch { } }
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
        // v2.7.82 兜底:toggle OFF 后如果 Snapshots 还有残留(UpdateDurationLabel 没被调),
        //   在 tick 里主动 restore m_TimeCostHours,避免"关了秒打碎还是秒"
        if (!CheatState.QuickBreakDown && Patch_BreakDown_UpdateDuration.Snapshots.Count > 0)
        {
            try
            {
                foreach (var p in UnityEngine.Object.FindObjectsOfType<Panel_BreakDown>())
                {
                    try
                    {
                        var bd = p.m_BreakDown;
                        if (bd == null) continue;
                        var ptr = bd.Pointer;
                        if (Patch_BreakDown_UpdateDuration.Snapshots.TryGetValue(ptr, out var snap))
                        {
                            bd.m_TimeCostHours = snap.timeCost;
                            _fi_breakdownSecs.SetValue(p, snap.seconds);
                            Patch_BreakDown_UpdateDuration.Snapshots.Remove(ptr);
                            ModMain.Log?.Msg($"[QuickBD] tick restore TimeCost={snap.timeCost:F2} Secs={snap.seconds:F1}");
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

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

    // v2.8.2 NoFallDamage tick — 反应式:既然 IL2CPP prefix return false 不可靠,
    // 每几帧检查+清除坠落产生的扭伤/衣物破损
    private static bool _fallGuardWristSet;
    private static float[] _clothingHpSnapshot;
    private static int _clothingSnapshotFrame;

    public static void TickNoFallDamage()
    {
        if (!CheatState.NoFallDamage)
        {
            if (_fallGuardWristSet)
            {
                try { GameManager.GetSprainedWristComponent()?.SetForceNoSprainWrist(false); } catch { }
                _fallGuardWristSet = false;
            }
            return;
        }
        try
        {
            // 1. 强制关闭手腕扭伤风险
            if (!_fallGuardWristSet)
            {
                try { GameManager.GetSprainedWristComponent()?.SetForceNoSprainWrist(true); } catch { }
                _fallGuardWristSet = true;
            }

            // 2. 检测并清除已有的坠落扭伤
            try
            {
                var ankle = GameManager.GetSprainedAnkleComponent();
                if (ankle != null && ankle.HasSprainedAnkle())
                {
                    ankle.SprainedAnkleEnd(0, (AfflictionOptions)0);
                    ankle.SprainedAnkleEnd(1, (AfflictionOptions)0);
                }
            }
            catch { }
            try
            {
                var wrist = GameManager.GetSprainedWristComponent();
                if (wrist != null && wrist.HasSprainedWrist())
                {
                    wrist.SprainedWristEnd(0, (AfflictionOptions)0);
                    wrist.SprainedWristEnd(1, (AfflictionOptions)0);
                }
            }
            catch { }

            // 3. 衣物 HP 快照/恢复 — 检测突然降低并还原(用背包列表,轻量)
            try
            {
                var inv = GameManager.m_Inventory;
                var items = inv?.m_Items;
                if (items != null)
                {
                    int count = items.Count;
                    if (_clothingHpSnapshot == null || _clothingHpSnapshot.Length < count)
                        _clothingHpSnapshot = new float[count + 8];

                    int frame = UnityEngine.Time.frameCount;
                    if (frame - _clothingSnapshotFrame <= 30)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            try
                            {
                                var obj = items[i];
                                if (obj == null) continue;
                                var gi = obj.m_GearItem;
                                if (gi == null || gi.m_ClothingItem == null) continue;
                                float cur = gi.m_CurrentHP;
                                float prev = _clothingHpSnapshot[i];
                                if (prev > 0f && cur < prev - 1f)
                                    gi.m_CurrentHP = prev;
                            }
                            catch { }
                        }
                    }

                    // 更新快照
                    for (int i = 0; i < count; i++)
                    {
                        try
                        {
                            var obj = items[i];
                            var gi = obj?.m_GearItem;
                            _clothingHpSnapshot[i] = (gi != null && gi.m_ClothingItem != null) ? gi.m_CurrentHP : 0f;
                        }
                        catch { _clothingHpSnapshot[i] = 0f; }
                    }
                    _clothingSnapshotFrame = frame;
                }
            }
            catch { }
        }
        catch { }
    }
}

// ═══════════════════════════════════════════════════════════════════
//   v2.7.45 CT 复刻 —— 从 CT 脚本直接映射的 Harmony patches
// ═══════════════════════════════════════════════════════════════════

// —— 秒烤肉 v2.7.51 —— 仿 CT:只推 m_CookingElapsedHours,不碰 percent/state
//   CT 做法:mov [rcx+m_CookingElapsedHours],(float)10; jmp original;
//   之前 v2.7.49/50 直接写 state=Ready,拾取时游戏内部想换状态被 Postfix 抢回 → 抽搐卡死
//   新法:Prefix 拿 cookTimeMinutes 参数,把 elapsed 推到刚好过熟透阈值,让原方法自然转 Ready
//   仅在 Cooking 状态下干预;Ready/Ruined 状态完全不动,pickup / eat 流程正常
// v2.7.75 DynamicPatch
internal static class Patch_CookingPot_Update
{
    private static bool _logged;
    internal static void Prefix(CookingPotItem __instance, float cookTimeMinutes, float readyTimeMinutes)
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
// v2.7.80 snapshot+restore 闭环 —— 修"toggle off 后秒采效果永久残留"
//   Init/BeginHold 共用 HarvestableSnaps.Snapshots;toggle off 时 BeginHold 再触发一次 restore 原值
//   场景切换 OnSceneWasInitialized 清 dict(实例随场景销毁,字段污染自然消失)
internal static class HarvestableSnaps
{
    public static readonly System.Collections.Generic.Dictionary<System.IntPtr, (float holdTime, float timer)> Snapshots = new();
}

internal static class Patch_HarvestableInteraction_Init
{
    internal static void Postfix(Il2CppTLD.Interactions.TimedHoldInteraction __instance)
    {
        try
        {
            if (!CheatState.QuickSearch) return;
            if (__instance is not HarvestableInteraction hi) return;
            var ptr = hi.Pointer;
            if (!HarvestableSnaps.Snapshots.ContainsKey(ptr))
                HarvestableSnaps.Snapshots[ptr] = (hi.m_DefaultHoldTime, hi.m_Timer);
            hi.m_DefaultHoldTime = 0.001f;
        }
        catch { }
    }
}

internal static class Patch_HarvestableInteraction_BeginHold
{
    internal static void Postfix(HarvestableInteraction __instance)
    {
        try
        {
            var ptr = __instance.Pointer;
            if (CheatState.QuickSearch)
            {
                if (!HarvestableSnaps.Snapshots.ContainsKey(ptr))
                    HarvestableSnaps.Snapshots[ptr] = (__instance.m_DefaultHoldTime, __instance.m_Timer);
                __instance.m_DefaultHoldTime = 0.001f;
                __instance.m_Timer = 99999f;
            }
            else if (HarvestableSnaps.Snapshots.TryGetValue(ptr, out var s))
            {
                __instance.m_DefaultHoldTime = s.holdTime;
                __instance.m_Timer = s.timer;
                HarvestableSnaps.Snapshots.Remove(ptr);
            }
        }
        catch { }
    }
}

internal static class Patch_TimedHold_UpdateHoldInteraction_QuickSearch
{
    internal static void Prefix(Il2CppTLD.Interactions.TimedHoldInteraction __instance, ref float deltaTime)
    {
        if (!CheatState.QuickSearch && !CheatState.QuickAction) return;
        try
        {
            var name = __instance.GetType().Name;
            if (!name.Contains("Harvest") && !name.Contains("PickUp")) return;

            __instance.m_DefaultHoldTime = 0.001f;
            __instance.HoldTime = 0.001f;
            deltaTime *= 10000f;
        }
        catch { }
    }
}

// —— 秒割肉 CT:Panel_BodyHarvest.Refresh 设 HarvestTimeSeconds=TotalHarvestTimeSeconds,Minutes=0 ——
// v2.7.80 snapshot+restore 闭环 —— toggle off 后下次打开割肉面板 Refresh 时恢复原值
internal static class Patch_Harvest_Refresh_Quick
{
    internal static readonly System.Collections.Generic.Dictionary<System.IntPtr, (float sec, float min)> Snapshots
        = new System.Collections.Generic.Dictionary<System.IntPtr, (float, float)>();

    internal static void Prefix(Panel_BodyHarvest __instance)
    {
        try
        {
            var ptr = __instance.Pointer;
            if (CheatState.QuickHarvest)
            {
                if (!Snapshots.ContainsKey(ptr))
                    Snapshots[ptr] = (__instance.m_HarvestTimeSeconds, __instance.m_HarvestTimeMinutes);
                __instance.m_HarvestTimeSeconds = __instance.m_TotalHarvestTimeSeconds;
                __instance.m_HarvestTimeMinutes = 0f;
            }
            else if (Snapshots.TryGetValue(ptr, out var s))
            {
                __instance.m_HarvestTimeSeconds = s.sec;
                __instance.m_HarvestTimeMinutes = s.min;
                Snapshots.Remove(ptr);
            }
        }
        catch { }
    }
}

// 秒割肉 CT 还加:BodyHarvest.MaybeFreeze 设 m_PercentFrozen=0(冻肉也能割)
// v2.7.75 DynamicPatch
internal static class Patch_BodyHarvest_MaybeFreeze
{
    internal static void Prefix(BodyHarvest __instance)
    {
        if (!CheatState.QuickHarvest) return;
        try { __instance.m_PercentFrozen = 0f; } catch { }
    }
}

// —— 秒打碎/回收 CT:Panel_BreakDown.UpdateDurationLabel 设 SecondsToBreakDown=0.2, BreakDown.TimeCostHours=0 ——
//   v2.7.61 加 snapshot+restore —— toggle off 时恢复 BreakDown.m_TimeCostHours,否则"UI 还是 0 min"
//           key 用 m_BreakDown.Pointer(每个物品的 BreakDown 组件独立)
// v2.7.75 DynamicPatch(有 snapshot,需 SyncWithCleanup)
// v2.7.82 修:同时存 m_SecondsToBreakDown,toggle OFF 一并恢复(之前只恢复 TimeCostHours 导致读条仍加速)
internal static class Patch_BreakDown_UpdateDuration
{
    internal static readonly System.Collections.Generic.Dictionary<System.IntPtr, (float timeCost, float seconds)> Snapshots
        = new System.Collections.Generic.Dictionary<System.IntPtr, (float timeCost, float seconds)>();

    internal static void Prefix(Panel_BreakDown __instance)
    {
        try
        {
            var bd = __instance.m_BreakDown;
            if (bd == null) return;
            var ptr = bd.Pointer;
            if (CheatState.QuickBreakDown)
            {
                if (!Snapshots.ContainsKey(ptr))
                    Snapshots[ptr] = (bd.m_TimeCostHours, __instance.m_SecondsToBreakDown);
                __instance.m_SecondsToBreakDown = 0.2f;
                bd.m_TimeCostHours = 0f;
            }
            else if (Snapshots.TryGetValue(ptr, out var snap))
            {
                bd.m_TimeCostHours = snap.timeCost;
                __instance.m_SecondsToBreakDown = snap.seconds;
                Snapshots.Remove(ptr);
            }
        }
        catch { }
    }
}

// v2.7.61 删除 Patch_BreakDown_Update_ForceFinish —— 每帧强推 m_TimeSpentBreakingDown 但没 Fade.Arm,
//   拆完留黑屏。改由 Patch_BreakDown_OnBreakDown 响应 QuickBreakDown 即可(原路径已带 Fade.Arm)

// —— 解锁保险箱 CT:SafeCracking.Update 直接 jmp UnlockSafe ——
// v2.7.75 DynamicPatch
internal static class Patch_SafeCracking_Update
{
    internal static void Postfix(SafeCracking __instance)
    {
        if (!CheatState.UnlockSafes) return;
        try { __instance.UnlockSafe(); } catch { }
    }
}

// —— 解锁上锁门/柜子 CT:LockedInteraction.IsLocked 强 return false ——
// v2.7.75 DynamicPatch
internal static class Patch_LockedInteraction_IsLocked_Unlock
{
    internal static void Postfix(LockedInteraction __instance, ref bool __result)
    {
        if (CheatState.UnlockSafes || CheatState.IgnoreLock)
        {
            __result = false;
            // Actually unlock the underlying Lock component so the lock icon disappears
            try
            {
                var lk = __instance.Lock;
                if (lk != null) lk.m_LockState = LockState.Unlocked;
            }
            catch { }
        }
    }
}

// —— 油灯 Update Postfix: 光照范围 + 持续 HP 损耗 + 静音 ——
// v3.0.4 整合 KeroseneLampTweaks v2.4.1 三个 Update 行为:lamp_range/overTimeDecay/muteLamps
internal static class Patch_KeroseneLamp_LightRange
{
    // v3.0.4r4 性能优化: 缓存 lamp instance ID → (Light[], 各自原始 range[])
    // 旧版每帧 GetComponentsInChildren<Light>(true) 扫整个 transform 树 + 分配数组,大开销
    // 新版第一次扫一次 light 子组件,记录原始 range,之后帧直接索引数组
    private struct LampCache
    {
        public UnityEngine.Light[] Lights;
        public float[] BaseRanges;
    }
    private static readonly System.Collections.Generic.Dictionary<int, LampCache> s_Cache = new();

    internal static void Postfix(KeroseneLampItem __instance)
    {
        try
        {
            var s = ModMain.Settings;
            if (s == null || __instance == null) return;

            // 1. 持续 HP 损耗(只在点亮时)
            if (s.LampOverTimeDecay > 0.001f && __instance.IsOn())
            {
                var gi = __instance.m_GearItem;
                if (gi != null)
                {
                    var tod = GameManager.GetTimeOfDayComponent();
                    float scale = (tod != null && tod.m_DayLengthScale > 0.001f) ? tod.m_DayLengthScale : 1f;
                    gi.m_CurrentHP = Mathf.Max(0f, gi.m_CurrentHP - s.LampOverTimeDecay * (Time.deltaTime / 300f) * (1f / scale));
                }
            }

            // 2. 静音(放置且非手持时)
            if (s.LampMute || CheatState.LampMute)
            {
                var gi2 = __instance.m_GearItem;
                if (gi2 != null && !gi2.m_InPlayerInventory)
                {
                    try { __instance.StopLoopingAudio(); } catch { }
                }
            }

            // 3. 光照范围倍率(性能关键路径) — 缓存 lamp 子 Light 数组
            float mult = s.LampRangeMultiplier;
            if (Mathf.Abs(mult - 1f) >= 0.005f)
            {
                int lampId = ((UnityEngine.Object)__instance).GetInstanceID();
                LampCache cache;
                if (!s_Cache.TryGetValue(lampId, out cache))
                {
                    var lts = __instance.GetComponentsInChildren<UnityEngine.Light>(true);
                    if (lts == null) return;
                    int n = lts.Length;
                    cache.Lights = new UnityEngine.Light[n];
                    cache.BaseRanges = new float[n];
                    for (int i = 0; i < n; i++)
                    {
                        var lt = lts[i];
                        cache.Lights[i] = lt;
                        cache.BaseRanges[i] = lt != null ? lt.range : 0f;
                    }
                    s_Cache[lampId] = cache;
                }
                // 用缓存数组(无 GC 分配,无 transform 树扫描)
                var lightsArr = cache.Lights;
                var baseArr = cache.BaseRanges;
                int len = lightsArr.Length;
                for (int i = 0; i < len; i++)
                {
                    var lt = lightsArr[i];
                    if (lt == null) continue;
                    float target = baseArr[i] * mult;
                    if (Mathf.Abs(lt.range - target) > 0.01f) lt.range = target;
                }
            }
        }
        catch { }
    }
}

// —— 油灯燃油消耗 v3.0.4 重写: KeroseneLampTweaks v2.4.1 完整版 ——
// 拆分手持/放置消耗倍率 + HP 阈值惩罚机制
// v3.0.4 替换原 LampBurnMultiplier 单一倍率
internal static class Patch_KeroseneLamp_ReduceFuel
{
    internal static bool Prefix(KeroseneLampItem __instance, ref float hoursBurned)
    {
        if (CheatState.LampFuelNoDrain) return false;
        var s = ModMain.Settings;
        if (s == null) return true;
        var gi = __instance != null ? __instance.m_GearItem : null;

        // 手持 vs 放置 倍率
        bool inInv = gi != null && gi.m_InPlayerInventory;
        float mult = inInv ? s.LampHeldBurnMult : s.LampPlacedBurnMult;

        // HP 阈值惩罚: HP 低于阈值时额外耗油
        float penalty = 1f;
        if (gi != null && s.LampConditionThreshold > 0 && gi.m_CurrentHP < s.LampConditionThreshold)
        {
            penalty += (1f - gi.m_CurrentHP / (float)s.LampConditionThreshold) * (s.LampMaxPenalty / 100f);
        }

        float effective = mult * penalty;
        if (effective < 0.001f) return false;  // 0 = 无限燃烧
        hoursBurned *= effective;
        return true;
    }
}

// —— v3.0.4 油灯开启时一次性 HP 损耗 (KeroseneLampTweaks turnOnDecay) ——
internal static class Patch_KeroseneLamp_OnIgnite
{
    internal static void Postfix(KeroseneLampItem __instance)
    {
        try
        {
            var s = ModMain.Settings;
            if (s == null || s.LampTurnOnDecay < 0.001f) return;
            var gi = __instance != null ? __instance.m_GearItem : null;
            if (gi != null) gi.m_CurrentHP -= s.LampTurnOnDecay;
        }
        catch { }
    }
}

// —— 保温杯永不失温 CT:InsulatedFlask.CalculateHeatLoss NOP 关键字节 ——
// v2.7.75 DynamicPatch
internal static class Patch_Flask_CalcHeatLoss
{
    internal static bool Prefix() => !CheatState.FlaskNoHeatLoss;
}

// —— 保温杯存放无限 CT:InsulatedFlask.UpdateVolume NOP ——
// v2.7.75 DynamicPatch
internal static class Patch_Flask_UpdateVolume
{
    internal static bool Prefix() => !CheatState.FlaskInfiniteVol;
}

// —— 保温瓶装任意 CT:IsItemCompatibleWithFlask 强 true ——
// v2.7.75 DynamicPatch
internal static class Patch_Flask_IsCompatible
{
    internal static void Postfix(ref bool __result)
    {
        if (CheatState.FlaskAnyItem) __result = true;
    }
}

// —— 加工秒完成 CT:EvolveItem.Update 设 TimeToEvolveGameDays=0, TimeSpentEvolvingGameHours=1 ——
// v2.7.75 DynamicPatch: 去掉 [HarmonyPatch] attribute
internal static class Patch_EvolveItem_Update
{
    internal static void Prefix(EvolveItem __instance)
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

// —— v3.0.4r3 修: stack 物品 evolve 数量丢失(用户报"10 内脏只产 1 干内脏") ——
// 根因: vanilla EvolveItem.DoEvolution 把 stack 整体替换为 1 个 evolved,原 N 个鲜内脏全消耗
// 方案 A(失败): Postfix 改 evolved 物品 m_Units → __instance 状态已变,设置无效
// 方案 C: Prefix 在 vanilla evolve 前,先用 InventoryComponent 直接 spawn N-1 个 evolved 物品到背包
//   (vanilla 自己 evolve 1 个 → 总产出 N 个;原 stack 消耗 N 个匹配)
[HarmonyPatch(typeof(EvolveItem), "DoEvolution")]
internal static class Patch_EvolveItem_DoEvolution_StackFix
{
    static void Prefix(EvolveItem __instance)
    {
        try
        {
            if (__instance == null) return;
            var gi = __instance.m_GearItem;
            if (gi == null || gi.m_StackableItem == null) return;
            int extraUnits = gi.m_StackableItem.m_Units - 1;
            if (extraUnits <= 0) return;

            var prefab = __instance.m_GearItemToBecome;
            if (prefab == null) return;

            // 用 vanilla InstantiateGearItem + AddGearItem 给背包加 N-1 个 evolved 副本
            var pm = GameManager.GetPlayerManagerComponent();
            var inv = GameManager.GetInventoryComponent();
            if (pm == null || inv == null) return;

            for (int i = 0; i < extraUnits; i++)
            {
                try
                {
                    // GearItem.Instantiate 的标准方式 — 用 GearItemPrefabUtility 或直接 Object.Instantiate
                    var go = UnityEngine.Object.Instantiate(((UnityEngine.Component)prefab).gameObject);
                    if (go == null) continue;
                    var newGi = go.GetComponent<GearItem>();
                    if (newGi == null) { UnityEngine.Object.Destroy(go); continue; }
                    pm.AddItemToPlayerInventory(newGi, false, true);
                }
                catch { }
            }
        }
        catch { }
    }
}

// —— 篝火温度 300℃ v2.7.49 加 snapshot+restore —— toggle off 必须还原
//   v2.7.48 只写不还原 → 关 toggle 后 m_MaxTempIncrease 仍是 300
// v2.7.75 DynamicPatch: 去掉 [HarmonyPatch] attribute
internal static class Patch_HeatSource_Update
{
    // 每实例原值快照:toggle 第一次 on 时记,toggle off 时恢复并删除
    internal static readonly System.Collections.Generic.Dictionary<System.IntPtr, float> Snapshots
        = new System.Collections.Generic.Dictionary<System.IntPtr, float>();

    internal static void Prefix(HeatSource __instance)
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
// v2.7.75 DynamicPatch: 去掉 [HarmonyPatch] attribute
internal static class Patch_Fire_Update_NeverDie
{
    // (isPerpetual, maxOnTOD, elapsedOnTOD, burnMinutesIfLit)
    internal static readonly System.Collections.Generic.Dictionary<System.IntPtr, (bool, float, float, float)> Snapshots
        = new System.Collections.Generic.Dictionary<System.IntPtr, (bool, float, float, float)>();

    internal static void Prefix(Fire __instance)
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
// v2.7.75 DynamicPatch: 去 [HarmonyPatch] attribute,只在 ClearDeathPenalty 时挂
internal static class Patch_CheatDeathAfflict_Update
{
    internal static void Prefix(CheatDeathAffliction __instance)
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
internal static class Patch_TraderManager_GetAvailableTradeExchanges
{
    internal static void Prefix(Il2CppTLD.Trader.TraderManager __instance)
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
internal static class Patch_TraderManager_IsTraderAvailable
{
    internal static void Postfix(ref bool __result)
    {
        if (CheatState.TraderAlwaysAvailable) __result = true;
    }
}

// —— 商人:交易秒完成 ——
//   CT: ExchangeItem.IsFullyExchanged Prefix,把 Exchanged* 三个字段 = 源字段,让 IsFullyExchanged 自动 return true
internal static class Patch_ExchangeItem_IsFullyExchanged
{
    internal static void Prefix(Il2CppTLD.Trader.ExchangeItem __instance)
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
// v2.7.63 双路并行:
//   A. Patch UpdateWaitingForArrival Prefix(CT 原路径)—— 仅 state=WaitingForArrival 时调
//   B. 加 tick 扫 m_ActiveTerritory,state < 2 时强推 2 —— 绕开 CT 路径只在特定 state 才触发的限制
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

// v2.7.75 DynamicPatch: 去 [HarmonyPatch] attribute —— 美洲狮地图每帧 bridge 是 23:40 卡的嫌疑
internal static class Patch_CougarManager_Update_ForceActivate
{
    private static bool _logged;
    internal static void Prefix(Il2CppTLD.AI.CougarManager __instance)
    {
        if (!CheatState.CougarInstantActivate) return;
        try
        {
            var terr = __instance.m_ActiveTerritory;
            if (terr == null) return;
            // 只在 state < WaitingForTransition(2)时推;>=2 不动,让进出门/睡觉自然触发动画
            if ((int)terr.m_CougarState < 2)
            {
                if (!_logged)
                {
                    _logged = true;
                    ModMain.Log?.Msg($"[Cougar] 强推 state {terr.m_CougarState} → WaitingForTransition (2)");
                }
                terr.m_CougarState = Il2CppTLD.AI.CougarManager.CougarState.WaitingForTransition;
            }
        }
        catch { }
    }
}

// ═══════════════════════════════════════════════════════════════════
//   CT 复刻 —— 无冻伤风险 / 饱饱 buff / 温度加成 / 疲劳加成 / 坏血病查看
// ═══════════════════════════════════════════════════════════════════

// —— 无冻伤风险:Frostbite.DealFrostbiteDamageToLocation → skip ——
// CT 做法:hook +100 处,设 [rcx+rbx*4+20]=(float)0(伤害归零)
// Harmony:Prefix return false = skip entire method
internal static class Patch_Frostbite_DealDamage
{
    internal static bool Prefix() => !CheatState.NoFrostbiteRisk;
}

// —— 饱饱 buff:WellFed.Update Prefix 强制 m_Active=true ——
// CT 做法:NOP WellFed.Update 前 2 字节(跳过退出条件),条件:饥饿不为0
// Harmony:Prefix 设 m_Active = true,原方法继续正常跑
internal static class Patch_WellFed_Update
{
    internal static void Prefix(WellFed __instance)
    {
        if (!CheatState.WellFedBuff) return;
        try { __instance.m_Active = true; } catch { }
    }
}

// —— 温度加成:FreezingBuff getter → __result = true ——
// CT 做法:hook PlayerManager.FreezingBuffActive,设 [rbx+50]=(float)1
// Harmony:Postfix 强制 __result = true
internal static class Patch_PlayerManager_FreezingBuffActive
{
    internal static void Postfix(ref bool __result)
    {
        if (CheatState.FreezingBuff) __result = true;
    }
}

// —— 疲劳加成:PlayerManager.FatigueBuffActive → true ——
// CT 做法:hook StatusBar.IsBuffActive,设 [rax+3C]=(float)1
// 游戏原生: PlayerManager.FatigueBuffActive() 返回 true 时,Fatigue 系统自动降低积累速率
// 与 FreezingBuffActive 完全对称的 Postfix 模式
internal static class Patch_PlayerManager_FatigueBuffActive
{
    internal static void Postfix(ref bool __result)
    {
        if (CheatState.FatigueBuff) __result = true;
    }
}

// —— 坏血病查看:直接读 ScurvyManager 属性(Menu 打开时调用,零 patch 开销)——
internal static class ScurvyViewer
{
    public static float VitaminC;
    public static float Threshold;
    public static string Status = "";

    public static void Poll()
    {
        try
        {
            var mgr = GameManager.GetScurvyComponent();
            if (mgr == null) { Status = "—"; return; }
            VitaminC = mgr.GetVitaminCNormalized();
            Threshold = 1f;
            int state = (int)mgr.m_ScurvyState;
            Status = state >= 2 ? (I18n.IsEnglish ? "SCURVY" : "坏血病!")
                   : state == 1 ? (I18n.IsEnglish ? "Risk" : "风险")
                   :              (I18n.IsEnglish ? "OK" : "正常");
        }
        catch { Status = "err"; }
    }
}

// ═══════════════════════════════════════════════════════════════════
//   整合 RunWithLantern:拿油灯时可以跑步
// ═══════════════════════════════════════════════════════════════════

internal static class Patch_RunWithLantern
{
    internal static bool Prefix() => false;
}

internal static class Patch_CameraOverride_OnStateUpdate
{
    internal static bool Prefix(AnimatorStateInfo stateInfo)
    {
        if (!CheatState.RunWithLantern) return true;
        string[] skip = { "Lantern_Prepare", "Lantern_Idle", "Lantern_Extinguish" };
        for (int i = 0; i < skip.Length; i++)
            if (stateInfo.IsName(skip[i])) return false;
        return true;
    }
}

internal static class Patch_CameraOverride_OnStateEnter
{
    internal static bool Prefix(AnimatorStateInfo stateInfo)
    {
        if (!CheatState.RunWithLantern) return true;
        string[] skip = { "Lantern_Prepare", "Lantern_Idle", "Lantern_Extinguish" };
        for (int i = 0; i < skip.Length; i++)
            if (stateInfo.IsName(skip[i])) return false;
        return true;
    }
}

// ═══════════════════════════════════════════════════════════════════
//   整合 FullSwing:门开角度和开门速度可调
// ═══════════════════════════════════════════════════════════════════

internal static class Patch_ObjectAnim_Play
{
    internal static void Prefix(ObjectAnim __instance, ref string name)
    {
        try
        {
            var s = ModMain.Settings;
            if (s == null) return;
            if ((UnityEngine.Object)__instance.m_Target == null || __instance.m_Events == null) return;
            if (name != "open" && name != "close") return;
            if (name == "open" && ((UnityEngine.Object)((Component)__instance).gameObject).name != "Door") return;

            iTweenEvent openEvt = null, closeEvt = null;
            var events = (Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppArrayBase<iTweenEvent>)(object)__instance.m_Events;
            foreach (var ev in events)
            {
                if (ev.tweenName == "open") openEvt = ev;
                if (ev.tweenName == "close") closeEvt = ev;
            }

            if (name == "open" && openEvt != null && closeEvt != null)
            {
                float angle = s.DoorSwingAngle;
                float speed = s.DoorSwingSpeed;
                ((Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppArrayBase<Vector3>)(object)openEvt.vector3s)[0] = new Vector3(angle, 0f, 0f);
                ((Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppArrayBase<Vector3>)(object)closeEvt.vector3s)[0] = new Vector3(0f - angle, 0f, 0f);
                ((Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppArrayBase<float>)(object)openEvt.floats)[0] = speed;
                ((Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppArrayBase<float>)(object)closeEvt.floats)[0] = speed;
                openEvt.DeserializeValues();
                closeEvt.DeserializeValues();
            }
        }
        catch (Exception ex)
        {
            MelonLoader.MelonLogger.Warning($"[TldHacks] FullSwing patch error: {ex.Message}");
        }
    }
}

// ═══════════════════════════════════════════════════════════════════
//   整合 DisableAutoEquipCharcoal:取炭不自动装备
// ═══════════════════════════════════════════════════════════════════

internal static class Patch_CharcoalHarvest_NoEquip
{
    internal static void Postfix()
    {
        if (!CheatState.NoAutoEquipCharcoal) return;
        try
        {
            var pm = GameManager.GetPlayerManagerComponent();
            if (pm == null || pm.m_ItemInHands == null) return;
            var name = ((UnityEngine.Object)pm.m_ItemInHands).name;
            if (name != null && name.Contains("Charcoal"))
                pm.UnequipItemInHands();
        }
        catch { }
    }
}

// ═══════════════════════════════════════════════════════════════════
//   整合 AutoToggleLights:休息/等待时自动熄灯
// ═══════════════════════════════════════════════════════════════════

internal static class Patch_AutoExtinguish_PassTime
{
    internal static void Postfix()
    {
        if (CheatState.AutoExtinguishOnRest) AutoExtinguishHelper.ExtinguishHeld();
    }
}

internal static class Patch_AutoExtinguish_OnRest
{
    internal static void Postfix()
    {
        if (CheatState.AutoExtinguishOnRest) AutoExtinguishHelper.ExtinguishHeld();
    }
}

internal static class AutoExtinguishHelper
{
    public static void ExtinguishHeld()
    {
        try
        {
            var pm = GameManager.GetPlayerManagerComponent();
            if (pm == null || pm.m_ItemInHands == null) return;
            var lamp = pm.m_ItemInHands.GetComponent<KeroseneLampItem>();
            if (lamp != null && lamp.IsOn()) { lamp.TurnOff(true); return; }
            var torch = pm.m_ItemInHands.GetComponent<TorchItem>();
            if (torch != null && torch.IsBurning()) { torch.Extinguish((TorchState)4); return; }
            var flash = pm.m_ItemInHands.GetComponent<FlashlightItem>();
            if (flash != null && flash.IsOn()) flash.TurnOff();
        }
        catch { }
    }
}

// —— 防止左键误灭火把 ——
internal static class Patch_TorchExtinguish_Block
{
    internal static bool Prefix() => !CheatState.DisableTorchLeftClick;
}

// —— 防止左键误灭油灯 ——
internal static class Patch_LampToggle_Block
{
    internal static bool Prefix(KeroseneLampItem __instance)
    {
        if (CheatState.DisableLampLeftClick && __instance.IsOn()) return false;
        return true;
    }
}

// ═══════════════════════════════════════════════════════════════════
//   整合 CaffeinatedSodas:苏打水加 FatigueBuff
// ═══════════════════════════════════════════════════════════════════

internal static class Patch_GearItem_Awake_CaffeinatedSodas
{
    private static float DurationHours(int choice) => choice switch
    {
        0 => 0.085f,
        1 => 0.167f,
        2 => 0.25f,
        3 => 0.5f,
        _ => 0.167f,
    };

    internal static void Postfix(GearItem __instance)
    {
        try
        {
            if (!CheatState.World_CaffeinatedSodas) return;
            var s = ModMain.Settings;
            if (s == null) return;

            string objName = ((UnityEngine.Object)__instance).name;

            if (objName.Contains("GEAR_SodaOrange") && s.World_SodaOrangeEnabled)
            {
                var buff = ((Component)__instance).gameObject.AddComponent<FatigueBuff>();
                __instance.m_FatigueBuff = buff;
                buff.m_InitialPercentDecrease = s.World_SodaOrangeInitial;
                buff.m_RateOfIncreaseScale = 1f;
                buff.m_DurationHours = DurationHours(s.World_SodaOrangeDuration);
            }
            else if (objName.Contains("GEAR_SodaGrape") && s.World_SodaGrapeEnabled)
            {
                var buff = ((Component)__instance).gameObject.AddComponent<FatigueBuff>();
                __instance.m_FatigueBuff = buff;
                buff.m_InitialPercentDecrease = s.World_SodaGrapeInitial;
                buff.m_RateOfIncreaseScale = 1f;
                buff.m_DurationHours = DurationHours(s.World_SodaGrapeDuration);
            }
            else if (objName.Contains("GEAR_Soda") && !objName.Contains("GEAR_SodaOrange")
                     && !objName.Contains("GEAR_SodaGrape") && s.World_SodaSummitEnabled)
            {
                var buff = ((Component)__instance).gameObject.AddComponent<FatigueBuff>();
                __instance.m_FatigueBuff = buff;
                buff.m_InitialPercentDecrease = s.World_SodaSummitInitial;
                buff.m_RateOfIncreaseScale = 1f;
                buff.m_DurationHours = DurationHours(s.World_SodaSummitDuration);
            }
        }
        catch (Exception ex)
        {
            MelonLoader.MelonLogger.Warning($"[TldHacks] CaffeinatedSodas patch error: {ex.Message}");
        }
    }
}

// ═══════════════════════════════════════════════════════════════════
//   v2.8.1 容器无限容量 — 照抄 UniversalTweaks 方案
// ═══════════════════════════════════════════════════════════════════

internal static class Patch_Container_Awake_InfiniteCapacity
{
    internal static void Postfix(Container __instance)
    {
        if (!CheatState.InfiniteContainer) return;
        try { __instance.m_Capacity = Il2CppTLD.IntBackedUnit.ItemWeight.FromKilograms(10000f); } catch { }
    }
}

// ═══════════════════════════════════════════════════════════════════
//   v2.8.1 坠落伤害 — patch FallDamage 入口方法彻底屏蔽
// ═══════════════════════════════════════════════════════════════════

internal static class Patch_FallDamage_MaybeApply
{
    internal static bool Prefix() => !CheatState.NoFallDamage && !TeleportFallGuard.Active;
}

internal static class Patch_FallDamage_ApplyAll
{
    // v3.0.4r3 修: 用户报衣物撕裂仍发生
    // 根因: IL2CPP 下 Prefix return false 不可靠,vanilla 仍跑 ApplyClothingDamage(fallHeight) → HP 减损 → 撕裂
    // 修复: ref 改 fallHeight=0,让 vanilla 自己算出 damage(0)=0,完全不依赖 return false
    internal static void Prefix(ref float fallHeight)
    {
        if (CheatState.NoFallDamage || TeleportFallGuard.Active)
        {
            fallHeight = 0f;            // 关键: vanilla 算 damage = f(height) → 0
            FallClothingGuard.Arm();    // 双保险: GearDegrade 也 hp=0
        }
    }
    internal static void Postfix()
    {
        FallClothingGuard.Clear();
    }
}

// v3.0.4r3: 主入口 ApplyFallDamage(height, damageOverride) 也清零,
// 防止内部 ApplyTravoisDamage / ApplyInsulatedFlaskDamage 等子方法用到非零 height
[HarmonyPatch(typeof(FallDamage), "ApplyFallDamage", new System.Type[] { typeof(float), typeof(float) })]
internal static class Patch_FallDamage_ApplyFallDamage_ZeroHeight
{
    static void Prefix(ref float height, ref float damageOverride)
    {
        if (CheatState.NoFallDamage || TeleportFallGuard.Active)
        {
            height = 0f;
            damageOverride = 0f;
        }
    }
}

internal static class FallClothingGuard
{
    public static bool Active;
    public static void Arm() => Active = true;
    public static void Clear() => Active = false;
}

internal static class TeleportFallGuard
{
    public static bool Active;
    private static int _frames;
    public static void Arm() { Active = true; _frames = 180; }
    public static void Tick() { if (Active && --_frames <= 0) Active = false; }
}

// ═══════════════════════════════════════════════════════════════════
//   v2.8.1 免费制作扩展 — NumSetsOfMaterialsAvailable 返回 ≥1
// ═══════════════════════════════════════════════════════════════════

internal static class Patch_Blueprint_NumSets
{
    internal static void Postfix(ref int __result)
    {
        if (CheatState.FreeCraft && __result < 1) __result = 1;
    }
}

