using System;
using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.Gear;

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
            "Prefix", null, () => CheatState.FireNeverDie,
            () => Patch_Fire_Update_NeverDie.Snapshots.Count == 0),
        Spec(typeof(HeatSource), "Update", typeof(Patch_HeatSource_Update),
            "Prefix", null, () => CheatState.FireTemp300,
            () => Patch_HeatSource_Update.Snapshots.Count == 0),

        Spec(typeof(Il2CppTLD.AI.CougarManager), "Update", typeof(Patch_CougarManager_Update_ForceActivate),
            "Prefix", null, () => CheatState.CougarInstantActivate),
        Spec(typeof(Panel_BreakDown), "Update", typeof(Patch_BreakDown_Update_Edge),
            null, "Postfix", () => CheatState.QuickAction || CheatState.QuickBreakDown),
        Spec(typeof(CheatDeathAffliction), "Update", typeof(Patch_CheatDeathAfflict_Update),
            "Prefix", null, () => CheatState.ClearDeathPenalty),

        Spec(typeof(GearItem), "Degrade", typeof(Patch_GearItem_Degrade),
            "Prefix", null, () => CheatState.InfiniteDurability, null, OneFloat),
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
            "Prefix", null, () => CheatState.LampFuelNoDrain),
        Spec(typeof(GunItem), "RemoveNextFromClip", typeof(Patch_Gun_RemoveNextFromClip),
            "Prefix", null, () => CheatState.InfiniteAmmo),

        Spec(typeof(Il2CppTLD.Gear.InsulatedFlask), "CalculateHeatLoss", typeof(Patch_Flask_CalcHeatLoss),
            "Prefix", null, () => CheatState.FlaskNoHeatLoss),
        Spec(typeof(Il2CppTLD.Gear.InsulatedFlask), "UpdateVolume", typeof(Patch_Flask_UpdateVolume),
            "Prefix", null, () => CheatState.FlaskInfiniteVol),
        Spec(typeof(Il2CppTLD.Gear.InsulatedFlask), "IsItemCompatibleWithFlask", typeof(Patch_Flask_IsCompatible),
            null, "Postfix", () => CheatState.FlaskAnyItem, null, OneGearItem),

        Spec(typeof(CraftingOperation), "Update", typeof(Patch_CraftingOp_Update),
            "Prefix", null, () => CheatState.QuickCraft),
        Spec(typeof(CookingPotItem), "UpdateCookingTimeAndState", typeof(Patch_CookingPot_Update),
            "Prefix", null, () => CheatState.QuickCook, null, TwoCookingFloats),
        Spec(typeof(Panel_Crafting), "CraftingEnd", typeof(Patch_Craft_End_ForceBright),
            null, "Postfix", () => CheatState.QuickCraft),
        Spec(typeof(Panel_Crafting), "OnCraftingSuccess", typeof(Patch_Craft_OnSuccess_ArmFade),
            null, "Postfix", () => CheatState.QuickCraft),
        Spec(typeof(SafeCracking), "Update", typeof(Patch_SafeCracking_Update),
            null, "Postfix", () => CheatState.UnlockSafes),
        Spec(typeof(HarvestableInteraction), "InitializeInteraction", typeof(Patch_HarvestableInteraction_Init),
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
            null, "Postfix", () => CheatState.QuickAction || CheatState.QuickBreakDown),
        Spec(typeof(Panel_BreakDown), "BreakDownFinished", typeof(Patch_BreakDown_Finished_Unfade),
            null, "Postfix", () => CheatState.QuickAction || CheatState.QuickBreakDown),
        Spec(typeof(Panel_BreakDown), "ExitInterface", typeof(Patch_BreakDown_ExitInterface_UnlockTOD),
            null, "Postfix", () => CheatState.QuickAction || CheatState.QuickBreakDown),
        Spec(typeof(Panel_BreakDown), "OnCancel", typeof(Patch_BreakDown_OnCancel_UnlockTOD),
            null, "Postfix", () => CheatState.QuickAction || CheatState.QuickBreakDown),
        Spec(typeof(Panel_BreakDown), "Enable", typeof(Patch_BreakDown_Enable_Cleanup),
            null, "Postfix", () => CheatState.QuickAction || CheatState.QuickBreakDown, null, PanelBreakDownEnableArgs),

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
            null, "Postfix", () => CheatState.QuickOpenContainer, null, ContainerEnableArgs),

        Spec(typeof(Il2CppTLD.Trader.TraderManager), "GetAvailableTradeExchanges",
            typeof(Patch_TraderManager_GetAvailableTradeExchanges),
            "Prefix", null, () => CheatState.TraderUnlimitedList || CheatState.TraderMaxTrust),
        Spec(typeof(Il2CppTLD.Trader.TraderManager), "IsTraderAvailable",
            typeof(Patch_TraderManager_IsTraderAvailable),
            null, "Postfix", () => CheatState.TraderAlwaysAvailable),
        Spec(typeof(Il2CppTLD.Trader.ExchangeItem), "IsFullyExchanged",
            typeof(Patch_ExchangeItem_IsFullyExchanged),
            "Prefix", null, () => CheatState.TraderInstantExchange),

        // v2.7.89 无后坐力:零化 GunItem 后坐参数(Prefix 归零 + Postfix 恢复)
        Spec(typeof(vp_FPSWeapon), "PlayFireAnimation", typeof(Patch_FPSWeapon_PlayFireAnimation),
            "Prefix", "Postfix", () => CheatState.NoRecoil || CheatState.SuperAccuracy || CheatStateESP.RecoilScale < 0.99f),
        Spec(typeof(vp_FPSCamera), "Update", typeof(Patch_FPSCamera_ClearRecoil),
            "Prefix", "Postfix", () => CheatState.NoRecoil || CheatState.SuperAccuracy || CheatStateESP.RecoilScale < 0.99f || CheatState.NoAimSway),
        Spec(typeof(vp_FPSCamera), "LateUpdate", typeof(Patch_FPSCamera_LateUpdate),
            "Prefix", "Postfix", () => CheatState.NoRecoil || CheatState.SuperAccuracy || CheatStateESP.RecoilScale < 0.99f || CheatState.NoAimSway),
        Spec(typeof(vp_FPSWeapon), "Update", typeof(Patch_FPSWeapon_SteadyAim),
            "Prefix", "Postfix", () => CheatState.NoAimSway || CheatState.SuperAccuracy),
        // vp_FPSWeapon.LateUpdate 在 IL2CPP 中不存在,已移除

        // v2.7.86 新增功能
        Spec(typeof(InputManager), "CanStartFireIndoors", typeof(Patch_FireAnywhere),
            "Prefix", null, () => CheatState.FireAnywhere),
        Spec(typeof(PlayerMovement), "AddSprintStamina", typeof(Patch_AddSprintStamina),
            null, "Postfix", () => CheatState.InfiniteStamina),
        Spec(typeof(PlayerManager), "ConsumeUnitFromInventory", typeof(Patch_ConsumeUnit),
            "Prefix", null, () => CheatState.FreeFireFuel),
        Spec(typeof(PlayerManager), "GetCarryCapacityKGBuff", typeof(Patch_TechBackpack),
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
    };

    public static void Reconcile()
    {
        try
        {
            for (int i = 0; i < Specs.Length; i++) Specs[i].Sync();
            // ItemPicker.OnUpdate 是第三方 mod 的每帧 hook,只有对应 toggle 开启时才挂。
            AutoPickupGuard.ReconcileItemPickerPatch();
        }
        catch (Exception ex) { ModMain.Log?.Error($"[DynPatch.Reconcile] {ex}"); }
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
                    _original = _paramTypes != null
                        ? AccessTools.Method(_target, _method, _paramTypes)
                        : AccessTools.Method(_target, _method);
                return _original;
            }
        }

        private void Patch()
        {
            try
            {
                var original = Original;
                if (original == null) { ModMain.Log?.Warning($"[DynPatch] method not found: {Key}"); return; }
                H.Patch(original, _prefix, _postfix);
                Applied = true;
                ModMain.Log?.Msg($"[DynPatch] ON  {Key}");
            }
            catch (Exception ex) { ModMain.Log?.Error($"[DynPatch] Patch {Key}: {ex.Message}"); }
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
