using System;
using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.Gear;
using Il2CppTLD.Interactions;
using Il2CppTLD.ModularElectrolizer;
using Il2CppTLD.News;
using UnityEngine;

namespace TldHacks;

// 按 toggle 动态 Harmony.Patch / Unpatch 每帧高频方法,消除 IL2Cpp bridge 开销。
// 无状态 patch:立即挂/卸。
// 有状态 patch(Fire/HeatSource snapshot restore):toggle off 保留 patch 直到 cleanup 完成再卸。
internal static class DynamicPatch
{
    private const string HARMONY_ID = "TldHacks.Dynamic";
    private static HarmonyLib.Harmony _h;
    private static readonly Type[] OneFloat = { typeof(float) };
    private static readonly Type[] OneBool = { typeof(bool) };
    private static readonly Type[] OneGearItem = { typeof(GearItem) };
    private static readonly Type[] HealthDamageArgs = { typeof(float), typeof(DamageSource) };
    private static readonly Type[] HealthDamageBoolArgs = { typeof(float), typeof(DamageSource), typeof(bool) };
    private static readonly Type[] OneIntString = { typeof(int), typeof(string) };
    private static readonly Type[] TwoCookingFloats = { typeof(float), typeof(float) };
    private static readonly Type[] PanelBreakDownEnableArgs = { typeof(bool) };
    private static readonly Type[] ContainerEnableArgs = { typeof(bool), typeof(bool), typeof(Il2CppSystem.Action) };
    private static readonly Type[] FloatFatigueFlags = { typeof(float), typeof(FatigueFlags) };
    private static readonly Type[] AnimStateArgs = { typeof(UnityEngine.Animator), typeof(UnityEngine.AnimatorStateInfo), typeof(int) };
    private static readonly Type[] OneString = { typeof(string) };
    private static readonly Type[] TwoBool = { typeof(bool), typeof(bool) };
    private static readonly Type[] GameObjectFloat = { typeof(UnityEngine.GameObject), typeof(float) };
    private static readonly Type[] RTPCArgs = { typeof(uint), typeof(float), typeof(UnityEngine.GameObject) };
    private static readonly Type[] FootStepArgs2 = { typeof(UnityEngine.Vector3), typeof(string) };
    private static readonly Type[] FootStepArgs3 = { typeof(UnityEngine.Vector3), typeof(string), typeof(FootStepSounds.State) };
    private static readonly Type[] GearItemBool = { typeof(GearItem), typeof(bool) };
    private static readonly Type[] HLRegisterLightArgs = { typeof(AuroraLightingSimple) };
    private static readonly Type[] HLUpdateHUDArgs = { typeof(Panel_HUD) };
    private static readonly Type[] HLTooDarkArgs = { typeof(ActionsToBlock) };
    private static readonly Type[] BodyHarvestEnableArgs = ResolvePanelBodyHarvestEnableArgs();
    private static Type[] ResolvePanelBodyHarvestEnableArgs()
    {
        var cfs = typeof(Panel_BodyHarvest).Assembly.GetType("Il2Cpp.ComingFromScreenCategory");
        if (cfs == null) return null;
        return new Type[] { typeof(bool), typeof(BodyHarvest), typeof(bool), cfs };
    }
    private static readonly Type[] BeginSleepArgs = ResolveBeginSleepArgs();
    private static Type[] ResolveBeginSleepArgs()
    {
        var asm = typeof(Rest).Assembly;
        var pt = asm.GetType("Il2Cpp.PassTimeOptions")
              ?? asm.GetType("PassTimeOptions")
              ?? asm.GetType("Il2Cpp.TLD.PassTimeOptions");
        if (pt != null)
            return new Type[] { typeof(Bed), typeof(int), typeof(int), typeof(float), pt, typeof(Il2CppSystem.Action) };

        // Fallback: find BeginSleeping(Bed, int, int, float, ?, Action) by signature shape
        var methods = typeof(Rest).GetMethods(System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        foreach (var m in methods)
        {
            if (m.Name != "BeginSleeping") continue;
            var ps = m.GetParameters();
            if (ps.Length == 6
                && ps[0].ParameterType == typeof(Bed)
                && ps[1].ParameterType == typeof(int)
                && ps[2].ParameterType == typeof(int)
                && ps[3].ParameterType == typeof(float)
                && ps[5].ParameterType == typeof(Il2CppSystem.Action))
            {
                return new Type[] { ps[0].ParameterType, ps[1].ParameterType, ps[2].ParameterType,
                    ps[3].ParameterType, ps[4].ParameterType, ps[5].ParameterType };
            }
        }
        return null;
    }

    private static HarmonyLib.Harmony H { get { if (_h == null) _h = new HarmonyLib.Harmony(HARMONY_ID); return _h; } }

    private static readonly PatchSpec[] Specs =
    {
        Spec(typeof(Condition), "AddHealth", typeof(Patch_Condition_AddHealth_2),
            "Prefix", null, () => CheatState.GodMode || CheatState.TrueInvisible || CheatState.NoFallDamage || CheatState.NoSuffocating || CheatState.ImmuneAnimalDamage || CheatState.NoFrostbiteRisk,
            null, HealthDamageArgs),
        Spec(typeof(Condition), "AddHealth", typeof(Patch_Condition_AddHealth_3),
            "Prefix", null, () => CheatState.GodMode || CheatState.TrueInvisible || CheatState.NoFallDamage || CheatState.NoSuffocating || CheatState.ImmuneAnimalDamage || CheatState.NoFrostbiteRisk,
            null, HealthDamageBoolArgs),
        Spec(typeof(Condition), "AddHealthWithNoHudNotification", typeof(Patch_Condition_AddHealthNoHud),
            "Prefix", null, () => CheatState.GodMode || CheatState.TrueInvisible || CheatState.NoFallDamage || CheatState.NoSuffocating || CheatState.ImmuneAnimalDamage || CheatState.NoFrostbiteRisk,
            null, HealthDamageArgs),

        Spec(typeof(EvolveItem), "Update", typeof(Patch_EvolveItem_Update),
            "Prefix", null, () => CheatState.QuickEvolve),

        Spec(typeof(BaseAi), "CanSeeTarget", typeof(Patch_BaseAi_CanSeeTarget),
            "Prefix", null, () => CheatState.Stealth || CheatState.TrueInvisible),
        Spec(typeof(BaseAi), "ScanForSmells", typeof(Patch_BaseAi_ScanForSmells),
            "Prefix", null, () => CheatState.Stealth || CheatState.TrueInvisible),

        Spec(typeof(Fire), "Update", typeof(Patch_Fire_Update_NeverDie),
            "Prefix", null, () => CheatState.FireNeverDie),
        Spec(typeof(FireManager), "PlayerCalculateFireStartTime", typeof(Patch_FireManager_CalcStartTime_Uncap),
            "Prefix", null, () => CheatState.FireMaxBurnHours > 12f),
        Spec(typeof(HeatSource), "Update", typeof(Patch_HeatSource_Update),
            "Prefix", null, () => CheatState.FireTemp300,
            () => Patch_HeatSource_Update.Snapshots.Count == 0),

        Spec(typeof(Il2CppTLD.AI.CougarManager), "Update", typeof(Patch_CougarManager_Update_ForceActivate),
            "Prefix", null, () => CheatState.CougarInstantActivate),
        Spec(typeof(Panel_BreakDown), "Update", typeof(Patch_BreakDown_Update_Edge),
            null, "Postfix", () => CheatState.QuickBreakDown),
        Spec(typeof(CheatDeathAffliction), "Update", typeof(Patch_CheatDeathAfflict_Update),
            "Prefix", null, () => CheatState.ClearDeathPenalty),

        Spec(typeof(GearItem), "WearOut", typeof(Patch_GearItem_WearOut),
            "Prefix", null, () => CheatState.InfiniteDurability),
        Spec(typeof(GearItem), "DegradeOnUse", typeof(Patch_GearItem_DegradeOnUse),
            "Prefix", null, () => CheatState.InfiniteDurability),

        Spec(typeof(ClothingItem), "IncreaseWetnessPercent", typeof(Patch_Clothing_IncreaseWet),
            "Prefix", null, () => CheatState.NoWetClothes, null, OneFloat),
        Spec(typeof(ClothingItem), "MaybeGetWetOnGround", typeof(Patch_Clothing_GetWetOnGround),
            "Prefix", null, () => CheatState.NoWetClothes, null, OneFloat),

        Spec(typeof(Lock), "IsLocked", typeof(Patch_Lock_IsLocked),
            null, "Postfix", () => CheatState.IgnoreLock),
        Spec(typeof(LockedInteraction), "IsLocked", typeof(Patch_LockedInteraction_IsLocked_Unlock),
            null, "Postfix", () => CheatState.UnlockSafes || CheatState.IgnoreLock),
        Spec(typeof(BodyHarvest), "MaybeFreeze", typeof(Patch_BodyHarvest_MaybeFreeze),
            "Prefix", null, () => CheatState.QuickHarvest),
        Spec(typeof(Il2CppTLD.Gear.KeroseneLampItem), "ReduceFuel", typeof(Patch_KeroseneLamp_ReduceFuel),
            "Prefix", null, () => CheatState.LampFuelNoDrain
                || (ModMain.Settings != null && (Mathf.Abs(ModMain.Settings.LampPlacedBurnMult - 1f) >= 0.01f
                                              || Mathf.Abs(ModMain.Settings.LampHeldBurnMult - 1f) >= 0.01f
                                              || ModMain.Settings.LampConditionThreshold > 0)), null, OneFloat),
        Spec(typeof(Il2CppTLD.Gear.KeroseneLampItem), "OnIgniteComplete", typeof(Patch_KeroseneLamp_OnIgnite),
            null, "Postfix", () => ModMain.Settings != null && ModMain.Settings.LampTurnOnDecay >= 0.001f),
        Spec(typeof(Il2CppTLD.Gear.KeroseneLampItem), "Update", typeof(Patch_KeroseneLamp_LightRange),
            null, "Postfix", () => ModMain.Settings != null && (Mathf.Abs(ModMain.Settings.LampRangeMultiplier - 1f) >= 0.01f
                                                             || ModMain.Settings.LampOverTimeDecay >= 0.001f
                                                             || ModMain.Settings.LampMute || CheatState.LampMute)),
        Spec(typeof(GunItem), "RemoveNextFromClip", typeof(Patch_Gun_RemoveNextFromClip),
            "Prefix", null, () => CheatState.InfiniteAmmo),

        Spec(typeof(Il2CppTLD.Gear.InsulatedFlask), "CalculateHeatLoss", typeof(Patch_Flask_CalcHeatLoss),
            "Prefix", null, () => CheatState.FlaskNoHeatLoss),
        Spec(typeof(Il2CppTLD.Gear.InsulatedFlask), "UpdateVolume", typeof(Patch_Flask_UpdateVolume),
            "Prefix", null, () => CheatState.FlaskInfiniteVol),
        Spec(typeof(Il2CppTLD.Gear.InsulatedFlask), "IsItemCompatibleWithFlask", typeof(Patch_Flask_IsCompatible),
            null, "Postfix", () => CheatState.FlaskAnyItem, null, OneGearItem),
        Spec(typeof(Panel_InsulatedFlask), "IsCompatibleDrink", typeof(Patch_Flask_IsCompatible),
            null, "Postfix", () => CheatState.FlaskAnyItem, null, OneGearItem),
        Spec(typeof(Il2CppTLD.Gear.InsulatedFlaskLiquidTypeConstraint), "IsAllowed", typeof(Patch_Flask_IsCompatible),
            null, "Postfix", () => CheatState.FlaskAnyItem, null, OneGearItem),

        Spec(typeof(CraftingOperation), "Update", typeof(Patch_CraftingOp_Update),
            "Prefix", null, () => CheatState.QuickCraft),
        Spec(typeof(CookingPotItem), "UpdateCookingTimeAndState", typeof(Patch_CookingPot_Update),
            "Prefix", null, () => CheatState.QuickCook || CheatState.NoBurn, null, TwoCookingFloats),
        Spec(typeof(Panel_Crafting), "CraftingEnd", typeof(Patch_Craft_End_ForceBright),
            null, "Postfix", () => CheatState.QuickCraft),
        Spec(typeof(Panel_Crafting), "OnCraftingSuccess", typeof(Patch_Craft_OnSuccess_ArmFade),
            null, "Postfix", () => CheatState.QuickCraft),

        // v2.8.1: 快速修理 — 从 [HarmonyPatch] 属性迁移到 DynamicPatch(IL2CPP 兼容)
        Spec(typeof(Panel_Repair), "StartRepair", typeof(Patch_Repair_StartRepair),
            null, "Postfix", () => CheatState.QuickAction, null, OneIntString),
        // FreeRepair: 无条件修理 — 消耗 skip + 工具不损耗
        Spec(typeof(Panel_Repair), "ConsumeMaterialsUsedForRepair", typeof(Patch_Repair_SkipConsume),
            "Prefix", null, () => CheatState.FreeRepair),
        Spec(typeof(Panel_Repair), "DegradeToolUsedForRepair", typeof(Patch_Repair_SkipToolDegrade),
            "Prefix", null, () => CheatState.FreeRepair),
        Spec(typeof(SafeCracking), "Update", typeof(Patch_SafeCracking_Update),
            null, "Postfix", () => CheatState.UnlockSafes),
        Spec(typeof(Il2CppTLD.Interactions.TimedHoldInteraction), "InitializeInteraction", typeof(Patch_HarvestableInteraction_Init),
            null, "Postfix", () => CheatState.QuickSearch),
        // v2.7.80 BeginHold 承担 snapshot restore 责任 —— cleanup 未完不卸
        Spec(typeof(HarvestableInteraction), "BeginHold", typeof(Patch_HarvestableInteraction_BeginHold),
            null, "Postfix", () => CheatState.QuickSearch,
            () => HarvestableSnaps.Snapshots.Count == 0),
        Spec(typeof(Il2CppTLD.Interactions.TimedHoldInteraction), "UpdateHoldInteraction",
            typeof(Patch_TimedHold_UpdateHoldInteraction_QuickSearch),
            "Prefix", null, () => CheatState.QuickSearch || CheatState.QuickAction, null, OneFloat),
        Spec(typeof(Panel_BodyHarvest), "Refresh", typeof(Patch_Harvest_Refresh_Quick),
            "Prefix", null, () => CheatState.QuickHarvest,
            () => Patch_Harvest_Refresh_Quick.Snapshots.Count == 0),
        Spec(typeof(Panel_BreakDown), "UpdateDurationLabel", typeof(Patch_BreakDown_UpdateDuration),
            "Prefix", null, () => CheatState.QuickBreakDown,
            () => Patch_BreakDown_UpdateDuration.Snapshots.Count == 0),
        Spec(typeof(Panel_BodyHarvest), "StartHarvest", typeof(Patch_Harvest_Start),
            null, "Postfix", () => CheatState.QuickAction, null, OneIntString),
        Spec(typeof(Panel_BodyHarvest), "StartQuarter", typeof(Patch_Harvest_StartQuarter),
            null, "Postfix", () => CheatState.QuickAction, null, OneIntString),
        Spec(typeof(Panel_BreakDown), "OnBreakDown", typeof(Patch_BreakDown_OnBreakDown),
            null, "Postfix", () => CheatState.QuickBreakDown),
        Spec(typeof(Panel_BreakDown), "BreakDownFinished", typeof(Patch_BreakDown_Finished_Unfade),
            null, "Postfix", () => CheatState.QuickBreakDown),
        Spec(typeof(Panel_BreakDown), "ExitInterface", typeof(Patch_BreakDown_ExitInterface_UnlockTOD),
            null, "Postfix", () => CheatState.QuickBreakDown),
        Spec(typeof(Panel_BreakDown), "OnCancel", typeof(Patch_BreakDown_OnCancel_UnlockTOD),
            null, "Postfix", () => CheatState.QuickBreakDown),
        Spec(typeof(Panel_BreakDown), "Enable", typeof(Patch_BreakDown_Enable_Cleanup),
            null, "Postfix", () => CheatState.QuickBreakDown, null, PanelBreakDownEnableArgs),

        // v2.7.78:把一批原本静态常驻的中/高频 patch 动态化,逻辑不变。
        Spec(typeof(IceCrackingTrigger), "BreakIce", typeof(Patch_IceBreak_BreakIce),
            "Prefix", null, () => CheatState.ThinIceNoBreak),
        Spec(typeof(IceCrackingTrigger), "FallInWater", typeof(Patch_IceBreak_FallInWater),
            "Prefix", null, () => CheatState.ThinIceNoBreak),
        Spec(typeof(FireManager), "CalculateFireStartSuccess", typeof(Patch_FireMgr_Success),
            null, "Postfix", () => CheatState.QuickFire),
        Spec(typeof(Panel_Container), "EnableAfterDelay", typeof(Patch_Container_EnableAfterDelay),
            "Prefix", null, () => CheatState.QuickOpenContainer, null, OneFloat),
        Spec(typeof(Panel_Container), "Enable", typeof(Patch_Container_Enable),
            null, "Postfix", () => CheatState.QuickOpenContainer || CheatState.InfiniteContainer, null, ContainerEnableArgs),

        Spec(typeof(Il2CppTLD.Trader.TraderManager), "GetAvailableTradeExchanges",
            typeof(Patch_TraderManager_GetAvailableTradeExchanges),
            "Prefix", null, () => CheatState.TraderUnlimitedList || CheatState.TraderMaxTrust),
        Spec(typeof(Il2CppTLD.Trader.TraderManager), "IsTraderAvailable",
            typeof(Patch_TraderManager_IsTraderAvailable),
            null, "Postfix", () => true),
        Spec(typeof(Il2CppTLD.Trader.TraderRadio), "CanContactTrader",
            typeof(Patch_TraderRadio_CanContactTrader),
            null, "Postfix", () => CheatState.TraderAlwaysAvailable),
        Spec(typeof(Il2CppTLD.Trader.ExchangeItem), "IsFullyExchanged",
            typeof(Patch_ExchangeItem_IsFullyExchanged),
            "Prefix", "Postfix", () => CheatState.TraderInstantExchange),

        // v2.7.89 无后坐力:零化 GunItem 后坐参数(Prefix 归零 + Postfix 恢复)
        Spec(typeof(vp_FPSWeapon), "PlayFireAnimation", typeof(Patch_FPSWeapon_PlayFireAnimation),
            "Prefix", "Postfix", () => CheatState.NoRecoil || CheatState.SuperAccuracy || CheatStateESP.RecoilScale < 0.99f),
        Spec(typeof(vp_FPSCamera), "Update", typeof(Patch_FPSCamera_ClearRecoil),
            "Prefix", "Postfix", () => CheatState.NoRecoil || CheatState.SuperAccuracy || CheatStateESP.RecoilScale < 0.99f || CheatState.NoAimSway),
        Spec(typeof(vp_FPSCamera), "LateUpdate", typeof(Patch_FPSCamera_LateUpdate),
            "Prefix", "Postfix", () => CheatState.NoRecoil || CheatState.SuperAccuracy || CheatStateESP.RecoilScale < 0.99f || CheatState.NoAimSway || (ModMain.Settings != null && ModMain.Settings.GunZoomEnabled)),
        Spec(typeof(vp_FPSWeapon), "Update", typeof(Patch_FPSWeapon_SteadyAim),
            "Prefix", "Postfix", () => CheatState.NoAimSway || CheatState.SuperAccuracy),
        // vp_FPSWeapon.LateUpdate 在 IL2CPP 中不存在,已移除

        // v2.7.86 新增功能
        Spec(typeof(InputManager), "CanStartFireIndoors", typeof(Patch_FireAnywhere),
            "Prefix", null, () => CheatState.FireAnywhere),
        Spec(typeof(PlayerMovement), "AddSprintStamina", typeof(Patch_AddSprintStamina),
            null, "Postfix", () => CheatState.InfiniteStamina),
        Spec(typeof(PlayerManager), "ConsumeUnitFromInventory", typeof(Patch_ConsumeUnit),
            "Prefix", null, () => CheatState.FreeFireFuel || CheatState.FreeRepair),
        Spec(typeof(Encumber), "Update", typeof(Patch_TechBackpack),
            null, "Postfix", () => CheatState.TechBackpack),
        Spec(typeof(TorchItem), "Update", typeof(Patch_TorchFullValue),
            "Prefix", null, () => CheatState.TorchFullValue),

        // CT 复刻:无冻伤/饱饱/温度/疲劳
        Spec(typeof(Frostbite), "DealFrostbiteDamageToLocation", typeof(Patch_Frostbite_DealDamage),
            "Prefix", null, () => CheatState.NoFrostbiteRisk),
        Spec(typeof(Frostbite), "DealFrostbiteDamageToRegion", typeof(Patch_Frostbite_DealDamage),
            "Prefix", null, () => CheatState.NoFrostbiteRisk),
        Spec(typeof(Frostbite), "FrostbiteStart", typeof(Patch_Frostbite_DealDamage),
            "Prefix", null, () => CheatState.NoFrostbiteRisk),
        Spec(typeof(WellFed), "Update", typeof(Patch_WellFed_Update),
            "Prefix", null, () => CheatState.WellFedBuff),
        Spec(typeof(PlayerManager), "FreezingBuffActive", typeof(Patch_PlayerManager_FreezingBuffActive),
            null, "Postfix", () => CheatState.FreezingBuff),
        Spec(typeof(PlayerManager), "FatigueBuffActive", typeof(Patch_PlayerManager_FatigueBuffActive),
            null, "Postfix", () => CheatState.FatigueBuff),

        // 整合 SonicMode:速度倍率 + 体力恢复(仅 SpeedTweaks 开启时挂载)
        Spec(typeof(vp_FPSController), "GetSlopeMultiplier", typeof(Patch_SpeedMultiplier),
            null, "Postfix", () => ModMain.Settings != null && ModMain.Settings.SpeedTweaksEnabled),
        Spec(typeof(PlayerMovement), "Update", typeof(Patch_StaminaTweaks),
            null, "Postfix", () => ModMain.Settings != null && ModMain.Settings.SpeedTweaksEnabled),

        // 整合 TorchTweaker:火把 condition(两个面板都 patch,覆盖所有交互入口)
        Spec(typeof(Panel_FeedFire), "Enable", typeof(Patch_TorchCondition),
            null, "Postfix", () => ModMain.Settings != null && (ModMain.Settings.TorchMinCondition != 0.6f || ModMain.Settings.TorchMaxCondition != 0.9f)),
        Spec(typeof(Panel_ActionPicker), "Enable", typeof(Patch_TorchCondition_ActionPicker),
            null, "Postfix", () => ModMain.Settings != null && (ModMain.Settings.TorchMinCondition != 0.6f || ModMain.Settings.TorchMaxCondition != 0.9f)),

        // 整合 GearDecayModifier:物品衰减倍率(仅在有衰减修改/无限耐久/快速制作时挂载)
        Spec(typeof(GearItem), "Degrade", typeof(Patch_GearDegrade),
            "Prefix", null, () => CheatState.InfiniteDurability || CheatState.QuickCraft || DecayState.HasNonDefaultMultiplier(), null, OneFloat),

        // 整合 PauseInJournal: 开日志暂停
        Spec(typeof(Panel_Log), "Enable", typeof(Patch_PanelLog_Pause),
            null, "Postfix", () => CheatState.PauseInJournal),

        // 整合 SkipIntroRedux: 跳过开场
        Spec(typeof(Panel_Boot), "Update", typeof(Patch_PanelBoot_SkipIntro),
            null, "Postfix", () => CheatState.SkipIntro),
        Spec(typeof(Panel_MainMenu), "Enable", typeof(Patch_PanelMainMenu_SkipIntro),
            "Prefix", null, () => CheatState.SkipIntro),
        Spec(typeof(NewsCarousel), "Awake", typeof(Patch_NewsCarousel_Hide),
            null, "Postfix", () => CheatState.SkipIntro),

        // 整合 VehicleFov + FreeLook: 车内FOV/视角
        Spec(typeof(PlayerInVehicle), "EnterVehicle", typeof(Patch_VehicleFov_Enter),
            null, "Postfix", () => CheatState.VehicleKeepFov || CheatState.VehicleFreeLook),
        Spec(typeof(PlayerInVehicle), "EnterVehicle", typeof(Patch_Vehicle_FreeLook),
            null, "Postfix", () => CheatState.VehicleFreeLook),

        // 整合 DroppableUndroppables: 允许丢弃不可丢物品
        Spec(typeof(GearItem), "Awake", typeof(Patch_GearItem_Awake_Droppable),
            null, "Postfix", () => CheatState.DroppableUndroppables),

        // 整合 RememberBreakDownItem: 记忆拆解工具
        Spec(typeof(Panel_BreakDown), "Enable", typeof(Patch_BreakDown_RememberTool_Enable),
            null, "Postfix", () => CheatState.RememberBreakdownTool, null, new[] { typeof(bool) }),
        Spec(typeof(Panel_BreakDown), "OnBreakDown", typeof(Patch_BreakDown_RememberTool_OnBreakDown),
            "Prefix", null, () => CheatState.RememberBreakdownTool),

        // 整合 ExtraGraphicsSettings: 准星
        Spec(typeof(HUDManager), "UpdateCrosshair", typeof(Patch_Crosshair_Show),
            null, "Postfix", () => ModMain.Settings != null && ModMain.Settings.CrosshairEnabled),

        // 整合 StretchArmstrong:交互距离倍率(1.0 = 原版,不挂载)
        Spec(typeof(PlayerManager), "ComputeModifiedPickupRange", typeof(Patch_PickupRange),
            null, "Postfix", () => ModMain.Settings != null && Mathf.Abs(ModMain.Settings.PickupRange - 1f) >= 0.01f),

        // 整合 SilentWalker:脚步静音 — PlayFootStepSound 有两个重载必须分别 patch
        Spec(typeof(FootStepSounds), "PlayFootStepSound", typeof(Patch_SilentFootsteps),
            "Prefix", null, () => CheatState.SilentFootsteps, null, FootStepArgs2),
        Spec(typeof(FootStepSounds), "PlayFootStepSound", typeof(Patch_SilentFootsteps_3p),
            "Prefix", null, () => CheatState.SilentFootsteps, null, FootStepArgs3),
        // 整合 SilentWalker:背包物品声音音量(仅调过音量时挂载)
        Spec(typeof(GameAudioManager), "SetRTPCValue", typeof(Patch_SilentWalker_RTPC),
            "Prefix", null, () => ModMain.Settings != null && (ModMain.Settings.InvWeightMetalVol != 100 || ModMain.Settings.InvWeightWoodVol != 100 || ModMain.Settings.InvWeightWaterVol != 100 || ModMain.Settings.InvWeightGeneralVol != 100), null, RTPCArgs),

        // 整合 RunWithLantern:拿油灯可跑步
        Spec(typeof(GameManager), "IsMovementLockedBecauseOfLantern", typeof(Patch_RunWithLantern),
            "Prefix", null, () => CheatState.RunWithLantern),
        Spec(typeof(CameraOverride), "OnStateUpdate", typeof(Patch_CameraOverride_OnStateUpdate),
            "Prefix", null, () => CheatState.RunWithLantern, null, AnimStateArgs),
        Spec(typeof(CameraOverride), "OnStateEnter", typeof(Patch_CameraOverride_OnStateEnter),
            "Prefix", null, () => CheatState.RunWithLantern, null, AnimStateArgs),

        // 整合 FullSwing:开门角度/速度(仅非默认值时挂载)
        Spec(typeof(ObjectAnim), "Play", typeof(Patch_ObjectAnim_Play),
            "Prefix", null, () => ModMain.Settings != null && (Mathf.Abs(ModMain.Settings.DoorSwingAngle - 0.29f) >= 0.01f || Mathf.Abs(ModMain.Settings.DoorSwingSpeed - 0.53f) >= 0.01f), null, OneString),

        // 整合 DisableAutoEquipCharcoal:取炭不装备
        Spec(typeof(Panel_FireStart), "OnCharcoalHarvest", typeof(Patch_CharcoalHarvest_NoEquip),
            null, "Postfix", () => CheatState.NoAutoEquipCharcoal),

        // 整合 AutoToggleLights:休息自动熄灯
        Spec(typeof(PassTime), "Begin", typeof(Patch_AutoExtinguish_PassTime),
            null, "Postfix", () => CheatState.AutoExtinguishOnRest),
        Spec(typeof(Panel_Rest), "OnRest", typeof(Patch_AutoExtinguish_OnRest),
            null, "Postfix", () => CheatState.AutoExtinguishOnRest),

        // 整合 TorchTweaker: 防误灭
        Spec(typeof(TorchItem), "ExtinguishDelayed", typeof(Patch_TorchExtinguish_Block),
            "Prefix", null, () => CheatState.DisableTorchLeftClick),
        Spec(typeof(KeroseneLampItem), "Toggle", typeof(Patch_LampToggle_Block),
            "Prefix", null, () => CheatState.DisableLampLeftClick),

        // 整合 Sprainkle: 扭伤参数实时覆写
        Spec(typeof(Sprains), "Update", typeof(Patch_Sprains_Update),
            null, "Postfix", () => CheatState.World_Sprainkle),
        Spec(typeof(SprainedAnkle), "SprainedAnkleStart", typeof(Patch_SprainedAnkle_Sprainkle),
            null, "Postfix", () => CheatState.World_Sprainkle),
        Spec(typeof(SprainedWrist), "SprainedWristStart", typeof(Patch_SprainedWrist_Sprainkle),
            null, "Postfix", () => CheatState.World_Sprainkle),

        // 整合 MoreCookingSlots: 增加烹饪槽位
        Spec(typeof(WoodStove), "Awake", typeof(Patch_WoodStove_Awake_CookingSlots),
            null, "Postfix", () => CheatState.Craft_MoreCookingSlots),


        // 整合 CraftAnywhere: 制作位置覆写(hook Panel_Crafting.ItemPassesFilter, 非 BlueprintData.Awake)
        Spec(typeof(Panel_Crafting), "ItemPassesFilter", typeof(Patch_CraftAnywhere),
            "Prefix", "Postfix", () => CheatState.Craft_Anywhere, null, new[] { typeof(BlueprintData) }),

        // 整合 CaffeinatedSodas: 苏打水加减疲劳 buff
        Spec(typeof(GearItem), "Awake", typeof(Patch_GearItem_Awake_CaffeinatedSodas),
            null, "Postfix", () => CheatState.World_CaffeinatedSodas),

        // 整合 BowRepair: 弓添加 Repairable/Millable 组件
        Spec(typeof(GearItem), "Awake", typeof(Patch_GearItem_Awake_BowRepair),
            null, "Postfix", () => CheatState.World_BowRepair),

        // 整合 NoSaveOnSprain + v2.8.1 NoFallDamage 阻止扭伤
        // Prefix 尝试 skip + Postfix Cure() 双保险(IL2CPP 可能忽略 Prefix return false)
        Spec(typeof(SprainedAnkle), "SprainedAnkleStart", typeof(Patch_NoSaveOnSprain_Ankle),
            "Prefix", "Postfix", () => CheatState.QoL_NoSaveOnSprain || CheatState.NoFallDamage),
        Spec(typeof(SprainedWrist), "SprainedWristStart", typeof(Patch_NoSaveOnSprain_Wrist),
            "Prefix", "Postfix", () => CheatState.QoL_NoSaveOnSprain || CheatState.NoFallDamage),

        // 整合 MapTextOutline: 地图文字描边
        Spec(typeof(Panel_Map), "CreateObjectPools", typeof(Patch_MapTextOutline),
            null, "Postfix", () => CheatState.QoL_MapTextOutline),

        // 整合 WakeUpCall: 睡眠唤醒+极光+时间+微光
        Spec(typeof(Rest), "BeginSleeping", typeof(Patch_WakeUpCall_BeginSleeping),
            "Prefix", null, () => CheatState.QoL_WakeUpCall || CheatState.QoL_AuroraSense, null, BeginSleepArgs),
        Spec(typeof(AuroraManager), "UpdateAuroraValue", typeof(Patch_WakeUpCall_AuroraWake),
            null, "Postfix", () => CheatState.QoL_AuroraSense),
        Spec(typeof(InterfaceManager), "SetTimeWidgetActive", typeof(Patch_WakeUpCall_TimeWidget),
            "Prefix", null, () => CheatState.QoL_ShowTimeSleep, null, OneBool),
        Spec(typeof(Panel_Rest), "Enable", typeof(Patch_WakeUpCall_RestEnable),
            null, "Postfix", () => CheatState.QoL_NoPitchBlack, null, TwoBool),

        // 整合 RnStripped-CarcassMoving: 搬运猎物
        Spec(typeof(Panel_BodyHarvest), "CanEnable", typeof(Patch_Carcass_CanEnable),
            null, "Postfix", () => CheatState.World_CarcassMoving, null, new[] { typeof(BodyHarvest) }),
        Spec(typeof(Panel_BodyHarvest), "Enable", typeof(Patch_Carcass_PanelEnable),
            null, "Postfix", () => CheatState.World_CarcassMoving, null, BodyHarvestEnableArgs),
        Spec(typeof(GameManager), "SetAudioModeForLoadedScene", typeof(Patch_Carcass_SceneLoaded),
            null, "Postfix", () => CheatState.World_CarcassMoving),
        Spec(typeof(LoadScene), "Activate", typeof(Patch_Carcass_LoadScene),
            null, "Postfix", () => CheatState.World_CarcassMoving, null, OneBool),
        Spec(typeof(GameManager), "TriggerSurvivalSaveAndDisplayHUDMessage", typeof(Patch_Carcass_BeforeSave),
            "Prefix", null, () => CheatState.World_CarcassMoving),
        Spec(typeof(PlayerManager), "PlayerCanSprint", typeof(Patch_Carcass_NoSprint),
            null, "Postfix", () => CheatState.World_CarcassMoving),
        Spec(typeof(PlayerManager), "EquipItem", typeof(Patch_Carcass_NoEquip),
            "Prefix", null, () => CheatState.World_CarcassMoving, null, GearItemBool),
        Spec(typeof(Fatigue), "CalculateFatigueIncrease", typeof(Patch_Carcass_Fatigue),
            null, "Postfix", () => CheatState.World_CarcassMoving, null, OneFloat),
        Spec(typeof(PlayerManager), "CalculateModifiedCalorieBurnRate", typeof(Patch_Carcass_Calorie),
            null, "Postfix", () => CheatState.World_CarcassMoving, null, OneFloat),
        Spec(typeof(Inventory), "GetExtraScentIntensity", typeof(Patch_Carcass_Scent),
            null, "Postfix", () => CheatState.World_CarcassMoving),
        Spec(typeof(RopeClimbPoint), "OnRopeTransition", typeof(Patch_Carcass_RopeDrop),
            null, "Postfix", () => CheatState.World_CarcassMoving, null, OneBool),

        // 整合 RnStripped-ElectricTorchLighting: 极光电点火把
        Spec(typeof(MissionServicesManager), "RegisterAnyMissionObjects", typeof(Patch_ElectricTorch_SceneInit),
            null, "Postfix", () => CheatState.World_ElectricTorch),
        Spec(typeof(PlayerManager), "InteractiveObjectsProcessInteraction", typeof(Patch_ElectricTorch_Interact),
            null, "Postfix", () => CheatState.World_ElectricTorch),
        Spec(typeof(DamageTrigger), "ApplyOneTimeDamage", typeof(Patch_ElectricTorch_NoDamage),
            "Prefix", null, () => CheatState.World_ElectricTorch, null, GameObjectFloat),
        Spec(typeof(DamageTrigger), "ApplyContinuousDamage", typeof(Patch_ElectricTorch_NoDamageCont),
            "Prefix", null, () => CheatState.World_ElectricTorch, null, GameObjectFloat),
        Spec(typeof(DamageTrigger), "OnTriggerExit", typeof(Patch_ElectricTorch_TriggerExit),
            "Prefix", null, () => CheatState.World_ElectricTorch),
        Spec(typeof(PlayerManager), "GetInteractiveObjectUnderCrosshairs", typeof(Patch_ElectricTorch_Crosshair),
            null, "Postfix", () => CheatState.World_ElectricTorch, null, OneFloat),

        // 整合 TinyTweaks-BuryHumanCorpses: Alt交互埋葬尸体
        Spec(typeof(InputManager), "ExecuteAltFire", typeof(Patch_BuryCorpses_AltFire),
            "Prefix", null, () => CheatState.QoL_BuryCorpses),
        Spec(typeof(Panel_HUD), "SetHoverText", typeof(Patch_BuryCorpses_HoverText),
            "Prefix", null, () => CheatState.QoL_BuryCorpses),
        Spec(typeof(Panel_GenericProgressBar), "ProgressBarEnded", typeof(Patch_BuryCorpses_ProgressBarEnded),
            "Prefix", null, () => CheatState.QoL_BuryCorpses),
        Spec(typeof(SaveGameSystem), "SaveSceneData", typeof(Patch_BuryCorpses_SaveScene),
            "Prefix", null, () => CheatState.QoL_BuryCorpses),
        Spec(typeof(SaveGameSystem), "LoadSceneData", typeof(Patch_BuryCorpses_LoadScene),
            null, "Postfix", () => CheatState.QoL_BuryCorpses),

        // 整合 SleepWithoutABed: 无床睡觉
        Spec(typeof(Panel_Rest), "Enable", typeof(Patch_SleepAnywhere_RestEnable),
            "Prefix", null, () => CheatState.QoL_SleepAnywhere, null, TwoBool),
        Spec(typeof(Panel_ActionsRadial), "DoPassTime", typeof(Patch_SleepAnywhere_RadialPassTime),
            null, "Postfix", () => CheatState.QoL_SleepAnywhere),
        Spec(typeof(Panel_Rest), "UpdateButtonLegend", typeof(Patch_SleepAnywhere_ButtonLegend),
            null, "Postfix", () => CheatState.QoL_SleepAnywhere),
        Spec(typeof(Panel_Rest), "OnRest", typeof(Patch_SleepAnywhere_OnRest),
            null, "Postfix", () => CheatState.QoL_SleepAnywhere),
        Spec(typeof(Rest), "UpdateWhenSleeping", typeof(Patch_SleepAnywhere_ConditionRecovery),
            null, "Postfix", () => CheatState.QoL_SleepAnywhere),
        Spec(typeof(Rest), "UpdateWhenSleeping", typeof(Patch_SleepAnywhere_SleepInterruption),
            null, "Postfix", () => CheatState.QoL_SleepAnywhere),
        Spec(typeof(Rest), "EndSleeping", typeof(Patch_SleepAnywhere_EndSleeping),
            null, "Postfix", () => CheatState.QoL_SleepAnywhere, null, OneBool),
        Spec(typeof(Panel_Rest), "OnPassTime", typeof(Patch_SleepAnywhere_PassTimeExposure),
            null, "Postfix", () => CheatState.QoL_SleepAnywhere),
        Spec(typeof(PlayerManager), "PlayerIsSleeping", typeof(Patch_SleepAnywhere_RestPanelClose),
            null, "Postfix", () => CheatState.QoL_SleepAnywhere),

        // v2.8.1 Bug 5+6: 睡眠黑屏安全网 — EndSleeping 时强制恢复 HUD
        Spec(typeof(Rest), "EndSleeping", typeof(Patch_WakeUpCall_EndSleeping_Safety),
            null, "Postfix", () => CheatState.QoL_WakeUpCall || CheatState.QoL_AuroraSense, null, OneBool),

        // v2.8.1 Bug 2: 无限容器容量
        Spec(typeof(Container), "Awake", typeof(Patch_Container_Awake_InfiniteCapacity),
            null, "Postfix", () => CheatState.InfiniteContainer),

        // v2.8.1 Bug 3: 无坠落伤害(TLD 2.55 存在的方法)
        Spec(typeof(FallDamage), "MaybeSprainAnkle", typeof(Patch_FallDamage_MaybeApply),
            "Prefix", null, () => CheatState.NoFallDamage),
        Spec(typeof(FallDamage), "MaybeSprainWrist", typeof(Patch_FallDamage_MaybeApply),
            "Prefix", null, () => CheatState.NoFallDamage),
        Spec(typeof(FallDamage), "ApplyClothingDamage", typeof(Patch_FallDamage_ApplyAll),
            "Prefix", "Postfix", () => CheatState.NoFallDamage),

        // v2.8.1 Bug 9: 免费制作材料数量 — 方法在 TLD 2.55 中已移除,由 FreeCraft 其他 patches 覆盖

        // ——— TinyTweaks 整合 ———
        Spec(typeof(Freezing), "CalculateBodyTemperature", typeof(Patch_CapFeelsLikeTemp),
            null, "Postfix", () => CheatState.TT_CapFeelsEnabled),

        Spec(typeof(FallDeathTrigger), "OnTriggerEnter", typeof(Patch_FallDeathTrigger),
            null, "Postfix", () => CheatState.TT_FallDeathGoat),
        Spec(typeof(GameManager), "Awake", typeof(Patch_FallDamageMultiplier),
            null, "Postfix", () => CheatState.TT_FallDeathGoat),

        Spec(typeof(GameManager), "Start", typeof(Patch_DroppedObjectOrientation),
            null, "Postfix", () => CheatState.TT_DroppedOrientation),

        Spec(typeof(Panel_OptionsMenu), "ApplyGraphicsModeAndResolution", typeof(Patch_ExtendedFOV_Apply),
            null, "Postfix", () => CheatState.TT_ExtendedFOV),
        Spec(typeof(Panel_OptionsMenu), "OnDisplayTab", typeof(Patch_ExtendedFOV_Display),
            null, "Postfix", () => CheatState.TT_ExtendedFOV),

        Spec(typeof(Panel_ActionsRadial), "Enable", typeof(Patch_PauseOnRadial_Enable),
            "Prefix", null, () => CheatState.TT_PauseOnRadial, null, new[] { typeof(bool), typeof(bool) }),
        Spec(typeof(GameManager), "Update", typeof(Patch_PauseOnRadial_FoolProof),
            null, "Postfix", () => CheatState.TT_PauseOnRadial),

        // SpeedyInteractions — 只在任一倍率 != 1 时才挂载(避免 20 个无用 hook 拖 FPS)
        Spec(typeof(PlayerManager), "UseFoodInventoryItem", typeof(Patch_Speedy_Eating),
            "Prefix", "Postfix", () => ModMain.Settings.TT_EatingSpeedMult != 1f),
        Spec(typeof(PlayerManager), "UseSmashableItem", typeof(Patch_Speedy_Smash),
            "Prefix", "Postfix", () => ModMain.Settings.TT_EatingSpeedMult != 1f),
        Spec(typeof(PlayerManager), "DrinkFromWaterSupply", typeof(Patch_Speedy_Drink),
            "Prefix", "Postfix", () => ModMain.Settings.TT_EatingSpeedMult != 1f),
        Spec(typeof(Panel_Inventory_Examine), "OnRefuel", typeof(Patch_Speedy_Refuel),
            "Prefix", "Postfix", () => ModMain.Settings.TT_GlobalSpeedMult != 1f),
        Spec(typeof(PlayerManager), "UseWaterPurificationItem", typeof(Patch_Speedy_Purify),
            "Prefix", "Postfix", () => ModMain.Settings.TT_GlobalSpeedMult != 1f),
        Spec(typeof(Panel_Inventory_Examine), "AccelerateTimeOfDay", typeof(Patch_Speedy_Examine),
            "Prefix", null, () => ModMain.Settings.TT_GlobalSpeedMult != 1f || ModMain.Settings.TT_ReadingSpeedMult != 1f),
        Spec(typeof(RockCache), "OnBuild", typeof(Patch_Speedy_RockCacheBuild),
            "Prefix", null, () => ModMain.Settings.TT_GlobalSpeedMult != 1f),
        Spec(typeof(RockCache), "OnDismantle", typeof(Patch_Speedy_RockCacheDismantle),
            "Prefix", null, () => ModMain.Settings.TT_GlobalSpeedMult != 1f),
        Spec(typeof(Panel_BreakDown), "OnBreakDown", typeof(Patch_Speedy_Breakdown),
            "Prefix", null, () => ModMain.Settings.TT_BreakdownSpeedMult != 1f),
        Spec(typeof(Panel_Crafting), "CraftingStart", typeof(Patch_Speedy_Crafting),
            "Prefix", null, () => ModMain.Settings.TT_GlobalSpeedMult != 1f),
        Spec(typeof(Panel_Cooking), "OnCook", typeof(Patch_Speedy_Cook1),
            "Prefix", null, () => ModMain.Settings.TT_GlobalSpeedMult != 1f),
        Spec(typeof(Panel_Cooking), "OnCookRecipe", typeof(Patch_Speedy_Cook2),
            "Prefix", null, () => ModMain.Settings.TT_GlobalSpeedMult != 1f),
        Spec(typeof(Panel_Milling), "BeginRepair", typeof(Patch_Speedy_Milling),
            "Prefix", null, () => ModMain.Settings.TT_GlobalSpeedMult != 1f),
        Spec(typeof(Panel_SnowShelterBuild), "OnBuild", typeof(Patch_Speedy_SnowShelterBuild),
            "Prefix", null, () => ModMain.Settings.TT_GlobalSpeedMult != 1f),
        Spec(typeof(Panel_SnowShelterInteract), "OnInteractionCommon", typeof(Patch_Speedy_SnowShelterInteract),
            "Prefix", null, () => ModMain.Settings.TT_GlobalSpeedMult != 1f),
        Spec(typeof(Panel_PickWater), "TakeWater", typeof(Patch_Speedy_PickWater),
            "Prefix", null, () => ModMain.Settings.TT_GlobalSpeedMult != 1f),
        Spec(typeof(Panel_IceFishingHoleClear), "UseTool", typeof(Patch_Speedy_IceFishing),
            "Prefix", null, () => ModMain.Settings.TT_GlobalSpeedMult != 1f),
        // IL2CPP 原始名含多余 "d"(DurationdUsed),非 typo
        Spec(typeof(AfflictionDefinition), "GetStandardDurationdUsed", typeof(Patch_Speedy_Affliction1),
            null, "Postfix", () => ModMain.Settings.TT_GlobalSpeedMult != 1f),
        Spec(typeof(AfflictionDefinition), "GetAlternateDurationUsed", typeof(Patch_Speedy_Affliction2),
            null, "Postfix", () => ModMain.Settings.TT_GlobalSpeedMult != 1f),
        Spec(typeof(TimedHoldInteraction), "BeginHold", typeof(Patch_Speedy_HoldInteraction),
            "Prefix", null, () => ModMain.Settings != null && (ModMain.Settings.TT_InteractionSpeedMult != 1f || CheatState.QuickSearch || Patch_Speedy_HoldInteraction.Data.Count > 0)),
        Spec(typeof(GameManager), "ResetLists", typeof(Patch_Speedy_ResetDict),
            null, "Postfix", () => ModMain.Settings.TT_InteractionSpeedMult != 1f || CheatState.QuickSearch),

        // RespawnablePlants
        Spec(typeof(Harvestable), "Awake", typeof(Patch_RespawnPlants_Awake),
            "Prefix", null, () => CheatState.TT_RespawnPlants),
        Spec(typeof(Harvestable), "Harvest", typeof(Patch_RespawnPlants_Harvest),
            null, "Postfix", () => CheatState.TT_RespawnPlants),
        Spec(typeof(Harvestable), "Deserialize", typeof(Patch_RespawnPlants_Deserialize),
            "Prefix", "Postfix", () => CheatState.TT_RespawnPlants),
        Spec(typeof(SaveGameSystem), "SaveSceneData", typeof(Patch_RespawnPlants_Save),
            null, "Postfix", () => CheatState.TT_RespawnPlants),
        Spec(typeof(SaveGameSystem), "LoadSceneData", typeof(Patch_RespawnPlants_Load),
            null, "Postfix", () => CheatState.TT_RespawnPlants),

        // HouseLights
        Spec(typeof(GameManager), "InstantiatePlayerObject", typeof(Patch_HL_GameManager_InstantiatePlayer),
            "Prefix", null, () => CheatState.HL_Enabled),
        Spec(typeof(AuroraModularElectrolizer), "Initialize", typeof(Patch_HL_Electrolizer_Initialize),
            null, "Postfix", () => CheatState.HL_Enabled),
        Spec(typeof(AuroraManager), "RegisterAuroraLightSimple", typeof(Patch_HL_AuroraManager_RegisterLightSimple),
            null, "Postfix", () => CheatState.HL_Enabled, null, HLRegisterLightArgs),
        Spec(typeof(AuroraManager), "UpdateForceAurora", typeof(Patch_HL_AuroraManager_UpdateForceAurora),
            null, "Postfix", () => CheatState.HL_Enabled),
        Spec(typeof(PlayerManager), "UpdateHUDText", typeof(Patch_HL_PlayerManager_UpdateHUDText),
            null, "Postfix", () => CheatState.HL_Enabled, null, HLUpdateHUDArgs),
        Spec(typeof(PlayerManager), "InteractiveObjectsProcessInteraction", typeof(Patch_HL_PlayerManager_ProcessInteraction),
            null, "Postfix", () => CheatState.HL_Enabled),
        Spec(typeof(Weather), "IsTooDarkForAction", typeof(Patch_HL_Weather_IsTooDarkForAction),
            null, "Postfix", () => CheatState.HL_Enabled, null, HLTooDarkArgs),

        // FlashFlicker 已改为 [HarmonyPatch] 属性直接注册(IL2Cpp 下 DynamicPatch Prefix return false 不可靠)

    };

    public static void Reconcile()
    {
        for (int i = 0; i < Specs.Length; i++)
        {
            try { Specs[i].Sync(); }
            catch (Exception ex)
            {
                if (!Specs[i]._failed)
                {
                    Specs[i]._failed = true;
                    ModMain.Log?.Error($"[DynPatch] {Specs[i].Key} permanently disabled: {ex.GetType().Name}: {ex.Message}");
                }
            }
        }
        try { AutoPickupGuard.ReconcileItemPickerPatch(); }
        catch (Exception ex) { ModMain.Log?.Error($"[DynPatch.AutoPickup] {ex.Message}"); }
    }

    public static string ActiveSummary()
    {
        try
        {
            int count = 0;
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < Specs.Length; i++)
            {
                var spec = Specs[i];
                if (!spec.Applied) continue;
                if (count > 0) sb.Append(',');
                sb.Append(spec.Key);
                count++;
            }
            return count == 0 ? "none" : sb.ToString();
        }
        catch { return "err"; }
    }

    private static PatchSpec Spec(Type target, string method, Type patchHolder,
        string prefixName, string postfixName, Func<bool> wanted, Func<bool> cleanupDone = null, Type[] paramTypes = null)
    {
        return new PatchSpec(target, method, patchHolder, prefixName, postfixName, wanted, cleanupDone, paramTypes);
    }

    private sealed class PatchSpec
    {
        public readonly string Key;
        public bool Applied;
        internal bool _failed;

        private readonly Type _target;
        private readonly string _method;
        private readonly Type[] _paramTypes;
        private readonly Func<bool> _wanted;
        private readonly Func<bool> _cleanupDone;
        private readonly HarmonyMethod _prefix;
        private readonly HarmonyMethod _postfix;
        private MethodBase _original;

        public PatchSpec(Type target, string method, Type patchHolder,
            string prefixName, string postfixName, Func<bool> wanted, Func<bool> cleanupDone, Type[] paramTypes)
        {
            _target = target;
            _method = method;
            _paramTypes = paramTypes;
            _wanted = wanted;
            _cleanupDone = cleanupDone;
            Key = target.FullName + "." + method + (paramTypes != null ? "(" + paramTypes.Length + ")" : "");
            _prefix = prefixName != null ? new HarmonyMethod(AccessTools.Method(patchHolder, prefixName)) : null;
            _postfix = postfixName != null ? new HarmonyMethod(AccessTools.Method(patchHolder, postfixName)) : null;
        }

        public void Sync()
        {
            if (_failed) return;
            bool wanted = _wanted();
            if (wanted)
            {
                if (!Applied) Patch();
                return;
            }

            if (Applied && (_cleanupDone == null || _cleanupDone())) Unpatch();
        }

        private MethodBase Original
        {
            get
            {
                if (_original == null)
                {
                    if (_paramTypes != null && System.Array.Exists(_paramTypes, t => t == null))
                        return null;
                    try
                    {
                        _original = _paramTypes != null
                            ? AccessTools.Method(_target, _method, _paramTypes)
                            : AccessTools.Method(_target, _method);
                    }
                    catch (System.Reflection.AmbiguousMatchException)
                    {
                        // IL2CPP may generate multiple overloads with the same arity.
                        // Fall back to exact parameter-type matching.
                        _original = FindMethodByParamTypes(_target, _method, _paramTypes);
                        if (_original == null)
                            ModMain.Log?.Warning($"[DynPatch] AmbiguousMatch fallback failed: {Key}");
                    }
                }
                return _original;
            }
        }

        private static System.Reflection.MethodInfo FindMethodByParamTypes(Type type, string name, Type[] paramTypes)
        {
            if (paramTypes == null) return null;
            var methods = type.GetMethods(System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.Static
                | System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.NonPublic);
            foreach (var m in methods)
            {
                if (m.Name != name) continue;
                var ps = m.GetParameters();
                if (ps.Length != paramTypes.Length) continue;
                bool match = true;
                for (int i = 0; i < ps.Length; i++)
                {
                    if (ps[i].ParameterType != paramTypes[i]) { match = false; break; }
                }
                if (match) return m;
            }
            return null;
        }

        private void Patch()
        {
            try
            {
                var original = Original;
                if (original == null) { _failed = true; ModMain.Log?.Warning($"[DynPatch] method not found (disabled): {Key}"); return; }
                H.Patch(original, _prefix, _postfix);
                Applied = true;
                ModMain.Log?.Msg($"[DynPatch] ON  {Key}");
            }
            catch (Exception ex) { _failed = true; ModMain.Log?.Error($"[DynPatch] Patch {Key} (disabled): {ex.Message}"); }
        }

        private void Unpatch()
        {
            try
            {
                var original = Original;
                if (original == null) return;
                H.Unpatch(original, HarmonyPatchType.All, HARMONY_ID);
                Applied = false;
                ModMain.Log?.Msg($"[DynPatch] OFF {Key}");
            }
            catch (Exception ex) { ModMain.Log?.Error($"[DynPatch] Unpatch {Key}: {ex.Message}"); }
        }
    }
}
