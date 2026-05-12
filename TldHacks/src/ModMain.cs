using System;
using System.Reflection;
using System.Runtime;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(TldHacks.ModMain), "TldHacks", "6.5.0", "user")]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: MelonAdditionalDependencies("ModSettings")]
[assembly: MelonOptionalDependencies("ModData")]

namespace TldHacks;

public class ModMain : MelonMod
{
    public static MelonLogger.Instance Log;
    internal static TldHacksSettings Settings;

    private int _posTick = 0;
    private int _frame = 0;
    private static bool _tsActive1, _tsActive2;

    public override void OnInitializeMelon()
    {
        Log = LoggerInstance;
        try
        {
            RegisterSettings();

            // 把 Settings 持久值同步到 CheatState
            SyncStateFromSettings();

            // v2.7.79 DiagUnpatchAll:启动时卸掉 MelonLoader 自动挂的所有 [HarmonyPatch] attribute,
            //   跳过 DynamicPatch.Reconcile() → TldHacks 全程 0 patch 挂载
            if (Settings.DiagUnpatchAll)
            {
                try { HarmonyInstance.UnpatchSelf(); }
                catch (Exception ex) { Log.Warning($"[DiagUnpatch] UnpatchSelf: {ex.Message}"); }
                // DynamicPatch 自己的 instance 也清一下,防上次残留(正常启动不会残留,保险起见)
                try { new HarmonyLib.Harmony("TldHacks.Dynamic").UnpatchSelf(); } catch { }
                Log.Msg("[DIAG] DiagUnpatchAll = ON → TldHacks 所有 Harmony patch 已卸载,Reconcile / 所有 tick 已停。重启游戏仍会重新应用此开关。");
            }
            else
            {
                // 写崩溃哨兵:如果 patch 导致加载存档崩溃,下次启动自动重置
                try { System.IO.File.WriteAllText(SentinelPath, DateTime.Now.ToString("o")); } catch { }
                DynamicPatch.Reconcile();
            }

            // v2.7.64 加载 scene transition 历史记录
            TransitionRecorder.Init();
            try { GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency; } catch { }
            Log.Msg($"TldHacks v6.6 loaded — menu hotkey = {Settings.MenuHotkey}, items = {ItemDatabase.All.Count}+{ItemDatabaseMod.All.Count} mod, transitions = {TransitionRecorder.Count}");
        }
        catch (Exception ex) { Log.Error($"[Init] {ex}"); }
    }

    // v3.0.6 跨版本配置兼容: 老 JSON + 新字段时不再整个 Load 失败,
    //   逐字段反射救援 — 兼容字段保留用户值,不兼容字段静默用默认。
    //   仅在 OnInitializeMelon 启动时跑一次,运行时零开销。
    //   全程 try-catch 兜底,任何异常退化为标准 AddToModSettings 路径。
    private static readonly string SentinelPath = System.IO.Path.Combine("Mods", "TldHacks.loading");

    internal static void ClearSentinel()
    {
        try { if (System.IO.File.Exists(SentinelPath)) System.IO.File.Delete(SentinelPath); } catch { }
    }

    private static void RegisterSettings()
    {
        Settings = new TldHacksSettings();
        var path = System.IO.Path.Combine("Mods", "TldHacks.json");

        // 崩溃哨兵:上次启动挂了 patch 后没成功进场景
        // → 照常加载 JSON(保留 slider/hotkey/菜单位置), 但把所有 bool toggle 关掉
        bool crashRecovery = System.IO.File.Exists(SentinelPath);
        if (crashRecovery)
        {
            Log?.Warning("[Settings] 检测到上次加载崩溃 → 安全模式: 加载配置但禁用所有开关");
            ClearSentinel();
        }

        // 全新安装 → 直接走标准路径
        if (!System.IO.File.Exists(path))
        {
            try { Settings.AddToModSettings("TldHacks"); }
            catch (Exception ex) { Log?.Error($"[Settings] AddToModSettings(new): {ex.Message}"); }
            return;
        }

        // 已有 JSON → 反射救援
        bool needRewrite = false;
        try
        {
            string raw = System.IO.File.ReadAllText(path);
            var jobj = Newtonsoft.Json.Linq.JObject.Parse(raw);

            int ok = 0, dropped = 0, orphans = 0, clamped = 0;
            var fields = typeof(TldHacksSettings).GetFields(
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var validNames = new System.Collections.Generic.HashSet<string>();
            foreach (var f in fields)
            {
                if (f.IsStatic || f.IsInitOnly) continue;
                validNames.Add(f.Name);
                var token = jobj[f.Name];
                if (token == null) continue;
                try
                {
                    object val = token.ToObject(f.FieldType);
                    if (val != null)
                    {
                        // Slider 范围 clamp:超界值钳到边界,防止运行时异常
                        var sa = f.GetCustomAttribute<ModSettings.SliderAttribute>();
                        if (sa != null)
                        {
                            if (f.FieldType == typeof(float))
                            {
                                float fv = (float)val;
                                float fc = Mathf.Clamp(fv, sa.From, sa.To);
                                if (fc != fv) { val = fc; clamped++; }
                            }
                            else if (f.FieldType == typeof(int))
                            {
                                int iv = (int)val;
                                int ic = Mathf.Clamp(iv, (int)sa.From, (int)sa.To);
                                if (ic != iv) { val = ic; clamped++; }
                            }
                        }
                        f.SetValue(Settings, val);
                    }
                    ok++;
                }
                catch { dropped++; }
            }

            // 孤儿字段:JSON 有但当前 Settings 类没有 → 触发 rewrite 把 JSON 净化掉
            // (老版本删除/重命名的字段残留在 JSON 里,会让 ModSettings 库二次 Load 行为异常)
            foreach (var prop in jobj.Properties())
            {
                if (!validNames.Contains(prop.Name)) orphans++;
            }

            if (dropped > 0 || orphans > 0 || clamped > 0)
            {
                // 不兼容字段 / 孤儿字段 / 越界值 → 备份老文件 + 写回净化版本(让库 Load 不失败)
                string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                string backup = path + ".broken-" + stamp;
                try { System.IO.File.Copy(path, backup, true); } catch { }
                needRewrite = true;
                Log?.Warning($"[Settings] 救援老配置: 保留 {ok} 字段, 丢弃 {dropped} 不兼容, 删除 {orphans} 孤儿字段, 钳制 {clamped} 越界值。备份: {System.IO.Path.GetFileName(backup)}");
            }
            else
            {
                Log?.Msg($"[Settings] 配置全部兼容 ({ok} 字段)");
            }
        }
        catch (Exception ex)
        {
            // 救援本身炸了(JSON 语法错/磁盘错) → 备份+清掉,走标准默认路径
            Log?.Warning($"[Settings] SafeLoad 失败 ({ex.Message}),备份原文件并使用默认值");
            try
            {
                string backup = path + ".corrupt-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
                System.IO.File.Move(path, backup);
            }
            catch { try { System.IO.File.Delete(path); } catch { } }
            Settings = new TldHacksSettings();
        }

        // 崩溃恢复:保留 slider/hotkey/菜单位置,但把所有 bool 开关关回默认
        if (crashRecovery)
        {
            var defaults = new TldHacksSettings();
            var fields = typeof(TldHacksSettings).GetFields(BindingFlags.Public | BindingFlags.Instance);
            int resetCount = 0;
            foreach (var f in fields)
            {
                if (f.IsStatic || f.IsInitOnly) continue;
                if (f.FieldType != typeof(bool)) continue;
                object defVal = f.GetValue(defaults);
                object curVal = f.GetValue(Settings);
                if (!Equals(curVal, defVal)) { f.SetValue(Settings, defVal); resetCount++; }
            }
            needRewrite = true;
            Log?.Warning($"[Settings] 安全模式: 重置 {resetCount} 个 bool 开关为默认值, 保留所有数值/热键设置");
        }

        // 净化文件:把 hydrate 后的 settings 序列化回去,确保库 Load 不会撞旧字段
        if (needRewrite)
        {
            try
            {
                string clean = Newtonsoft.Json.JsonConvert.SerializeObject(Settings, Newtonsoft.Json.Formatting.Indented);
                System.IO.File.WriteAllText(path, clean);
            }
            catch (Exception ex) { Log?.Warning($"[Settings] 写净化 JSON 失败: {ex.Message}"); }
        }

        // 注册到 ModSettings UI(库会再 Load 一遍,但此时文件结构已 100% 兼容)
        try { Settings.AddToModSettings("TldHacks"); }
        catch (Exception ex)
        {
            // 极小概率仍失败 → 备份+全默认+二次重试
            Log?.Warning($"[Settings] AddToModSettings 失败 ({ex.Message}),备份并重置");
            try
            {
                string backup = path + ".unloadable-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
                System.IO.File.Move(path, backup);
            }
            catch { }
            Settings = new TldHacksSettings();
            try { Settings.AddToModSettings("TldHacks"); }
            catch (Exception ex2) { Log?.Error($"[Settings] 二次注册仍失败: {ex2.Message}"); }
        }
    }

    private void SyncStateFromSettings()
    {
        // 诊断开关开启时等价于"一键禁用所有功能"
        if (Settings.DiagPauseRuntime || Settings.DiagUnpatchAll)
        {
            Cheats.DisableAllCheats();
            return;
        }

        CheatState.GodMode = Settings.GodMode;
        // v3.0.4r4: GodMode 隐含 NoFallDamage + NoSprainRisk(用户语义:无敌应该全免)
        CheatState.NoFallDamage = Settings.NoFallDamage || Settings.GodMode;
        CheatState.AlwaysWarm = Settings.AlwaysWarm;
        CheatState.NoHunger = Settings.NoHunger;
        CheatState.NoThirst = Settings.NoThirst;
        CheatState.NoFatigue = Settings.NoFatigue;
        CheatState.InfiniteDurability = Settings.InfiniteDurability;
        CheatState.InstantKillAnimals = Settings.InstantKillAnimals;
        CheatState.FreezeAnimals = Settings.FreezeAnimals;
        CheatState.Stealth = Settings.Stealth;
        CheatState.TrueInvisible = Settings.TrueInvisible;
        CheatState.ThinIceNoBreak = Settings.ThinIceNoBreak;
        CheatState.IgnoreLock = Settings.IgnoreLock;
        CheatState.QuickOpenContainer = Settings.QuickOpenContainer;
        CheatState.NoWetClothes = Settings.NoWetClothes;
        CheatState.FreeCraft = Settings.FreeCraft;
        CheatState.FreeRepair = Settings.FreeRepair;
        CheatState.QuickCraft = Settings.QuickCraft;
        CheatState.InfiniteAmmo = Settings.InfiniteAmmo;
        CheatState.NoJam = Settings.NoJam;
        CheatState.NoRecoil = Settings.NoRecoil;
        CheatState.NoAimSway = Settings.NoAimSway;
        CheatState.NoAimStamina = Settings.NoAimStamina;
        CheatState.SuperAccuracy = Settings.SuperAccuracy;
        CheatState.SpeedMultiplier = Settings.SpeedMultiplier;
        CheatState.NoSprainRisk = Settings.NoSprainRisk || Settings.GodMode;  // v3.0.4r4 GodMode 隐含
        CheatState.ImmuneAnimalDamage = Settings.ImmuneAnimalDamage;
        CheatState.NoSuffocating = Settings.NoSuffocating;
        CheatState.QuickFire = Settings.QuickFire;
        CheatState.QuickClimb = Settings.QuickClimb;
        CheatState.QuickAction = Settings.QuickAction;
        // CT 复刻 v2.7.45+
        CheatState.QuickCook = Settings.QuickCook;
        CheatState.NoBurn = Settings.NoBurn;
        CheatState.QuickSearch = Settings.QuickSearch;
        CheatState.QuickHarvest = Settings.QuickHarvest;
        CheatState.QuickBreakDown = Settings.QuickBreakDown;
        CheatState.UnlockSafes = Settings.UnlockSafes;
        CheatState.LampFuelNoDrain = Settings.LampFuelNoDrain;
        CheatState.LampMute = Settings.LampMute;
        CheatState.FlaskNoHeatLoss = Settings.FlaskNoHeatLoss;
        CheatState.FlaskInfiniteVol = Settings.FlaskInfiniteVol;
        CheatState.FlaskAnyItem = Settings.FlaskAnyItem;
        CheatState.QuickEvolve = Settings.QuickEvolve;
        CheatState.InfiniteContainer = Settings.InfiniteContainer;
        CheatState.FireTemp300 = Settings.FireTemp300;
        CheatState.FireNeverDie = Settings.FireNeverDie;
        CheatState.FireMaxBurnHours = Settings.FireMaxBurnHours;
        CheatState.NoFrostbiteRisk = Settings.NoFrostbiteRisk;
        CheatState.WellFedBuff = Settings.WellFedBuff;
        CheatState.FreezingBuff = Settings.FreezingBuff;
        CheatState.FatigueBuff = Settings.FatigueBuff;
        CheatState.CureFrostbite = Settings.CureFrostbite;
        CheatState.ClearDeathPenalty = Settings.ClearDeathPenalty;
        CheatState.QuickFishing = Settings.QuickFishing;
        // v2.7.86 新增功能
        CheatState.FireAnywhere = Settings.FireAnywhere;
        CheatState.FreeFireFuel = Settings.FreeFireFuel;
        CheatState.TechBackpack = Settings.TechBackpack;
        CheatState.TechBackpackKg = Settings.TechBackpackKg;
        CheatState.TorchFullValue = Settings.TorchFullValue;
        CheatState.InfiniteStamina = Settings.InfiniteStamina;
        CheatState.MapClickTP = Settings.MapClickTP;
        // v2.7.64 商人 + 美洲狮
        CheatState.TraderUnlimitedList = Settings.TraderUnlimitedList;
        CheatState.TraderMaxTrust = Settings.TraderMaxTrust;
        CheatState.TraderInstantExchange = Settings.TraderInstantExchange;
        CheatState.TraderAlwaysAvailable = Settings.TraderAlwaysAvailable;
        CheatState.CougarInstantActivate = Settings.CougarInstantActivate;
        CheatState.BlockAutoPickupOwnDrops = Settings.BlockAutoPickupOwnDrops;
        CheatState.SilentFootsteps = Settings.SilentFootsteps;
        CheatState.RunWithLantern = Settings.RunWithLantern;
        CheatState.NoAutoEquipCharcoal = Settings.NoAutoEquipCharcoal;
        CheatState.AutoExtinguishOnRest = Settings.AutoExtinguishOnRest;
        CheatState.DisableTorchLeftClick = Settings.DisableTorchLeftClick;
        CheatState.DisableLampLeftClick = Settings.DisableLampLeftClick;
        // HouseLights 整合
        CheatState.HL_Enabled = Settings.HL_Enabled;
        CheatState.HL_EnableOutside = Settings.HL_EnableOutside;
        CheatState.HL_WhiteLights = Settings.HL_WhiteLights;
        CheatState.HL_NoFlicker = Settings.HL_NoFlicker;
        CheatState.HL_CastShadows = Settings.HL_CastShadows;
        CheatState.HL_LightAudio = Settings.HL_LightAudio;
        CheatState.HL_Intensity = Settings.HL_Intensity;
        CheatState.HL_RangeMultiplier = Settings.HL_RangeMultiplier;
        CheatState.HL_CullDistance = Settings.HL_CullDistance;
        CheatState.HL_InteractDistance = Settings.HL_InteractDistance;
        // v2.7.96 整合 Batch 2
        CheatState.PauseInJournal = Settings.PauseInJournal;
        CheatState.SkipIntro = Settings.SkipIntro;
        CheatState.MuteCougarMenuSound = Settings.MuteCougarMenuSound;
        CheatState.VehicleKeepFov = Settings.VehicleKeepFov;
        CheatState.DroppableUndroppables = Settings.DroppableUndroppables;
        CheatState.RememberBreakdownTool = Settings.RememberBreakdownTool;
        CheatState.VehicleFreeLook = Settings.VehicleFreeLook;
        CheatStateESP.RecoilScale = Settings.RecoilScaleESP;
        // v2.8.0 批量整合 — Tab 6: Universal Tweaks
        // v2.8.0 批量整合 — Tab 7: QoL
        CheatState.QoL_Enabled = Settings.QoL_Enabled;
        CheatState.QoL_NoSaveOnSprain = Settings.QoL_NoSaveOnSprain;
        CheatState.QoL_NoSaveOnSprainFalls = Settings.QoL_NoSaveOnSprainFalls;
        CheatState.QoL_WakeUpCall = Settings.QoL_WakeUpCall;
        CheatState.QoL_AuroraSense = Settings.QoL_AuroraSense;
        CheatState.QoL_ShowTimeSleep = Settings.QoL_ShowTimeSleep;
        CheatState.QoL_NoPitchBlack = Settings.QoL_NoPitchBlack;
        CheatState.QoL_MapTextOutline = Settings.QoL_MapTextOutlineEnabled;
        CheatState.QoL_BuryCorpses = Settings.QoL_BuryCorpses;
        CheatState.QoL_SleepAnywhere = Settings.QoL_SleepAnywhere;
        SleepAnywhereState.FatigueRecoveryPenalty = Settings.QoL_SleepFatigueRecovery;
        SleepAnywhereState.ConditionGainRate = Settings.QoL_SleepConditionRecovery;
        SleepAnywhereState.FreezingScale = Settings.QoL_SleepFreezingScale;
        SleepAnywhereState.FreezingHealthLoss = Settings.QoL_SleepFreezingHealthLoss;
        SleepAnywhereState.HypothermicHealthLoss = Settings.QoL_SleepHypothermicHealthLoss;
        SleepAnywhereState.PassTimeExposurePenalty = Settings.QoL_SleepPassTimeExposure;
        SleepAnywhereState.LowHealthInterrupt = Settings.QoL_SleepInterrupt;
        SleepAnywhereState.SleepInterruptionThreshold = Settings.QoL_SleepInterruptThreshold;
        SleepAnywhereState.InterruptionCooldown = Settings.QoL_SleepInterruptCooldown;
        SleepAnywhereState.HudMessage = Settings.QoL_SleepInterruptHudMsg;
        SleepAnywhereState.ApplyInterruptToBeds = Settings.QoL_SleepInterruptAllBeds;
        SleepAnywhereState.SensitivityScale = Settings.QoL_SleepSensitivityScale;
        SleepAnywhereState.AdjustedSensitivity = Settings.QoL_SleepAdjustedSensitivity;
        CheatState.QoL_AutoSurvey = Settings.QoL_AutoSurvey;

        // v2.8.0 批量整合 — Tab 8: Crafting & Fire
        CheatState.Craft_Anywhere = Settings.Craft_Anywhere;
        CheatState.Craft_MoreCookingSlots = Settings.Craft_MoreCookingSlots;
        // v2.8.0 批量整合 — Tab 9: World & Items
        CheatState.World_Sprainkle = Settings.World_Sprainkle;
        CheatState.World_SprainklePreset = Settings.World_SprainklePreset;
        CheatState.World_SprainkleSlopeMin = Settings.World_SprainkleSlopeMin;
        CheatState.World_SprainkleSlopeIncrease = Settings.World_SprainkleSlopeIncrease;
        CheatState.World_SprainkleBaseChanceMoving = Settings.World_SprainkleBaseChanceMoving;
        CheatState.World_SprainkleEncumberChance = Settings.World_SprainkleEncumberChance;
        CheatState.World_SprainkleExhaustionChance = Settings.World_SprainkleExhaustionChance;
        CheatState.World_SprainkleSprintChance = Settings.World_SprainkleSprintChance;
        CheatState.World_SprainkleCrouchChance = Settings.World_SprainkleCrouchChance;
        CheatState.World_SprainkleMinSecondsRisk = Settings.World_SprainkleMinSecondsRisk;
        CheatState.World_SprainkleWristMovementChance = Settings.World_SprainkleWristMovementChance;
        CheatState.World_SprainkleSprintUIOn = Settings.World_SprainkleSprintUIOn;
        CheatState.World_SprainkleSprintUIOff = Settings.World_SprainkleSprintUIOff;
        CheatState.World_SprainkleAnkleEnabled = Settings.World_SprainkleAnkleEnabled;
        CheatState.World_SprainkleAnkleDurMin = Settings.World_SprainkleAnkleDurMin;
        CheatState.World_SprainkleAnkleDurMax = Settings.World_SprainkleAnkleDurMax;
        CheatState.World_SprainkleAnkleRestHours = Settings.World_SprainkleAnkleRestHours;
        CheatState.World_SprainkleAnkleFallChance = Settings.World_SprainkleAnkleFallChance;
        CheatState.World_SprainkleWristEnabled = Settings.World_SprainkleWristEnabled;
        CheatState.World_SprainkleWristDurMin = Settings.World_SprainkleWristDurMin;
        CheatState.World_SprainkleWristDurMax = Settings.World_SprainkleWristDurMax;
        CheatState.World_SprainkleWristRestHours = Settings.World_SprainkleWristRestHours;
        CheatState.World_SprainkleWristFallChance = Settings.World_SprainkleWristFallChance;
        CheatState.World_BowRepair = Settings.World_BowRepair;
        CheatState.World_BowRepairDLC = Settings.World_BowRepairDLC;
        CheatState.World_CaffeinatedSodas = Settings.World_CaffeinatedSodas;
        CheatState.World_SodaOrangeEnabled = Settings.World_SodaOrangeEnabled;
        CheatState.World_SodaSummitEnabled = Settings.World_SodaSummitEnabled;
        CheatState.World_SodaGrapeEnabled = Settings.World_SodaGrapeEnabled;
        CheatState.World_CarcassMoving = Settings.World_CarcassMoving;
        CheatState.World_CarcassMovingAll = Settings.World_CarcassMovingAll;
        CheatState.World_ElectricTorch = Settings.World_ElectricTorch;

        // TinyTweaks
        CheatState.TT_CapFeelsEnabled = Settings.TT_CapFeelsEnabled;
        CheatState.TT_FallDeathGoat = Settings.TT_FallDeathGoat;
        CheatState.TT_DroppedOrientation = Settings.TT_DroppedOrientation;
        CheatState.TT_ExtendedFOV = Settings.TT_ExtendedFOV;
        CheatState.TT_PauseOnRadial = Settings.TT_PauseOnRadial;
        CheatState.TT_RespawnPlants = Settings.TT_RespawnPlants;
        CheatState.TT_ShowTraderTrust = Settings.TT_ShowTraderTrust;
    }

    public override void OnLateInitializeMelon()
    {
        // BlueprintCleaner 的 ScrollBehaviourItem.OnClick Postfix 调用了 TLD 2.55 中不存在的
        // GameManager.IsBootSceneActive()，导致 MissingMethodException 刷屏。
        // 在所有 mod 加载完后移除该 broken postfix。
        try
        {
            var target = HarmonyLib.AccessTools.Method(typeof(Il2CppTLD.UI.Scroll.ScrollBehaviourItem), "OnClick");
            if (target != null)
            {
                var patchInfo = HarmonyLib.Harmony.GetPatchInfo(target);
                if (patchInfo != null)
                {
                    foreach (var p in patchInfo.Postfixes)
                    {
                        if (p.PatchMethod?.DeclaringType?.Assembly?.GetName()?.Name == "BlueprintCleaner")
                        {
                            HarmonyInstance.Unpatch(target, p.PatchMethod);
                            Log.Msg("[Compat] Unpatched BlueprintCleaner broken Postfix on ScrollBehaviourItem.OnClick");
                            break;
                        }
                    }
                }
            }
        }
        catch (Exception ex) { Log.Warning($"[Compat] BlueprintCleaner unpatch failed: {ex.Message}"); }

    }

    public override void OnUpdate()
    {
        try
        {
            if (Settings == null) return;
            if (Settings.DiagPauseRuntime) return;  // v2.7.75 诊断开关
            if (Settings.DiagUnpatchAll) return;    // v2.7.79 UnpatchAll 模式下也停掉 tick / Reconcile

            // Menu toggle
            if (Settings.MenuHotkey != KeyCode.None && Input.GetKeyDown(Settings.MenuHotkey))
            {
                Menu.Toggle();
                if (Menu.Open) try { System.GC.Collect(0, System.GCCollectionMode.Optimized, false); } catch { }
            }

            // Fly toggle hotkey
            if (Settings.FlyHotkey != KeyCode.None && Input.GetKeyDown(Settings.FlyHotkey))
            {
                CheatState.CFly = !CheatState.CFly;
                ConsoleBridge.Run("fly");
            }

            // Time scale —— 按住加速,松开恢复
            // v2.8.1: 不再用 DetermineIfOverlayIsActive (读条面板也算 overlay 导致加速失效)
            //   只在暂停菜单/uConsole 时屏蔽,读条/制作/搜刮等进度条期间允许加速
            if (Settings.TimeScaleKey1 != KeyCode.None || Settings.TimeScaleKey2 != KeyCode.None || _tsActive1 || _tsActive2 || Time.timeScale != 1f)
            {
                bool held1 = Settings.TimeScaleKey1 != KeyCode.None && Input.GetKey(Settings.TimeScaleKey1);
                bool held2 = Settings.TimeScaleKey2 != KeyCode.None && Input.GetKey(Settings.TimeScaleKey2);
                bool blocked = uConsole.m_On;
                if (!blocked)
                {
                    var pause = InterfaceManager.GetPanel<Panel_PauseMenu>();
                    if (pause != null) try { blocked = pause.isActiveAndEnabled; } catch { }
                }
                if (!blocked && held1)
                {
                    _tsActive1 = true; _tsActive2 = false;
                    Time.timeScale = GameManager.m_GlobalTimeScale = Settings.TimeScale1;
                }
                else if (!blocked && held2)
                {
                    _tsActive2 = true; _tsActive1 = false;
                    Time.timeScale = GameManager.m_GlobalTimeScale = Settings.TimeScale2;
                }
                else if (_tsActive1 || _tsActive2)
                {
                    _tsActive1 = _tsActive2 = false;
                    Time.timeScale = GameManager.m_GlobalTimeScale = 1f;
                }
            }

            // Speed multiplier (菜单全局倍速滑块)
            if (CheatState.SpeedMultiplier > 0f
                && Mathf.Abs(CheatState.SpeedMultiplier - 1f) > 0.01f
                && Mathf.Abs(Time.timeScale - CheatState.SpeedMultiplier) > 0.01f)
                Time.timeScale = CheatState.SpeedMultiplier;

            // 快速采集延迟完成 (QuickCraft v2.7.42 改走 IncrementProgress Prefix,不需要 Tick)
            QuickHarvestRunner.Tick();

            // v2.7.80 同步频率 5s → 0.5s(300→30 帧)—— 修 "ModSettings Disable All 后 5 秒内还生效" 根因
            //   SyncStateFromSettings 只是 ~50 个 bool copy,成本可忽略
            if (_frame > 0 && (_frame % 30) == 15)
            {
                SyncStateFromSettings();
                Patch_GearDegrade.InvalidateCache();
            }

            // v2.7.75 稳定节奏:动态 patch 只在固定低频点 reconcile。
            if ((_frame % 300) == 25) DynamicPatch.Reconcile();
            // v6.0.7 FlaskAnyItem 原生 NOP patch
            if ((_frame % 300) == 30) FlaskNativePatch.Sync();

            // (InfiniteCarry 已去除,由其他 mod 覆盖)

            // v2.7.21 FPS 修:所有 tick 按 frame modulo 摊到不同帧
            // 之前 _extraTick==90 时 7 个 sub-tick 同帧触发 = 单帧 ~50ms 巨 spike = 跳帧
            // 现在每种 tick 有自己的 phase + period,最多 1 个 heavy tick/帧
            _frame++;

            // Infinite durability: 每 300 帧(~5s)phase 0
            if (CheatState.InfiniteDurability && (_frame % 300) == 0)
                Cheats.TickInfiniteDurability();

            // v2.7.26 FPS 优化:
            //   TrueInvisible 字段设置从 tick 扫 FindObjects 改成 BaseAi.Start Postfix(新 AI 出生时自动设)
            //   TickAnimalsFull 降到 600 帧(10s)仅做兜底,Stealth 的 SetAiMode(Flee) 也仍走这里
            //   TickAnimalsCheap(1s,只设玩家 m_AiTarget.m_IsEnabled 1 字段)保留
            if ((_frame % 60) == 5)
                CheatsTick.TickAnimalsCheap();
            if ((_frame % 600) == 30)
                CheatsTick.TickAnimalsFull();

            // v2.7.84 TickStatus 从 60 帧搬到 OnLateUpdate 每帧 —— 消除 HUD 值"掉到中间→跳回满"闪烁
            //   Unity 顺序:MonoBehaviour.Update(游戏的 Fatigue.Update 在这耗体力) → LateUpdate(我们这里 clamp)→ Render
            //   每帧 clamp 5 个字段,<10μs/帧,FPS 影响可忽略;GodMode 的 HP 同理
            // 注:此处不再调 TickStatus,见 OnLateUpdate()

            // 其余 sub-tick 每 180 帧一次(3s),错开 phase
            if ((_frame % 180) == 15)  CheatsTick.TickGuns();
            if ((_frame % 180) == 45)  CheatsTick.TickClothingWetness();
            if ((_frame % 180) == 75)  CheatsTick.TickClimbRope();
            if ((_frame % 180) == 135) CheatsTick.TickLocks();
            if ((_frame % 180) == 165) CheatsTick.TickQuickActions();

            // One-shot 类:状态变化时重设即可,低频 OK
            if ((_frame % 180) == 90) { ExtraOneShot.TickSprainRisk(); }

            // 天气锁定:每 600 帧(~10s)重新 apply,防止游戏计时器自动切换
            if (CheatState.WeatherLocked && (_frame % 60) == 30)
                try { Cheats.SetWeatherStageInternal(CheatState.WeatherLockedStage); } catch { }

            // v2.8.2 NoFallDamage reactive tick — 检测+清除扭伤/衣物破损
            if (CheatState.NoFallDamage && (_frame % 30) == 7) CheatsTick.TickNoFallDamage();

            // 摄像机 / 武器 aim:v2.7.83 从 120 帧降到 30 帧(0.5s),更灵敏
            if ((_frame % 30) == 20) CheatsTick.TickCamera();

            // 玩家位置:菜单打开时 ~1 秒刷一次
            if (Menu.Open && ++_posTick >= 60)
            {
                _posTick = 0;
                Cheats.UpdatePlayerPosition();
            }

            // 跨场景传送 pending tick
            Teleport.TickPendingTeleport();
            TeleportFallGuard.Tick();

            // v2.7.94 MotionTrackerLite 持续强制启用(5s)
            if ((_frame % 300) == 50) MotionTrackerLiteHelper.EnsureVisible();
            // 每 300 帧在暂停/菜单安全时机做 Gen0 优化回收
            if ((_frame % 300) == 50)
            {
                try
                {
                    if (GameManager.m_IsPaused || InterfaceManager.IsOverlayActiveImmediate())
                        System.GC.Collect(0, System.GCCollectionMode.Optimized, false);
                }
                catch { }
            }

            // v2.7.94 双击地图图标传送
            MapClickTeleport.Tick();

            // v2.7.94 TweaksRuntime: 实时应用滑块设置(移动/图形/光源/衰减)
            TweaksRuntime.Tick();


            // v2.7.95 跳跃(整合 Jump mod) — v3.0.3 加总开关
            if (Settings.JumpEnabled) JumpHelper.Tick();

            // v2.7.95 GunZoom 滚轮输入
            if (Settings.GunZoomEnabled) GunZoomState.UpdateScroll();



            WakeUpCallHelper.OnUpdateTick();
            AutoSurveyHelper.OnUpdateTick();

            if (!NLBFontFixHelper.Exhausted)
            {
                int nlbPeriod = NLBFontFixHelper.Fixed ? 3600 : 300;
                if ((_frame % nlbPeriod) == 200)
                    try { NLBFontFixHelper.PatchOnce(); } catch { }
            }

            // 每 60 秒主动做一次 Gen0 收集，避免累积到大暂停
            if ((_frame % 3600) == 1800)
                try { System.GC.Collect(0, System.GCCollectionMode.Optimized, false); } catch { }

            // ShowTraderTrust 仅在用户开启时才 tick
            if (CheatState.TT_ShowTraderTrust && (_frame % 60) == 5)
                try { ShowTraderTrustHelper.Tick(); } catch { }

        }
        catch (Exception ex) { Log?.Error($"[OnUpdate] {ex}"); }
    }

    public override void OnLateUpdate()
    {
        if (Settings != null && (Settings.DiagPauseRuntime || Settings.DiagUnpatchAll)) return;
        Stacking.OnLateUpdate();
        // v2.7.84 HUD 状态 clamp 每帧跑 —— LateUpdate 在 Update 的 drain 后、render 前,UI 不看到"掉到中间值"闪烁
        CheatsTick.TickStatus();
    }

    public override void OnGUI()
    {
        if (Settings != null && (Settings.DiagPauseRuntime || Settings.DiagUnpatchAll)) return;
        Menu.Draw();
        ShowTraderTrustHelper.DrawGUI();
    }

    public override void OnSceneWasInitialized(int buildIndex, string sceneName)
    {
        // 成功进入游戏场景 → 清除崩溃哨兵(MainMenu 不算,要真正加载存档才证明 patch 没崩)
        if (sceneName != "MainMenu" && sceneName != "Boot")
            ClearSentinel();
        Teleport.OnSceneLoaded(sceneName);
        // v2.7.64 学习 scene transition 字段到 JSON,解决 DLC Tale 跨区物品丢失
        TransitionRecorder.OnSceneInitialized(sceneName);
        // v2.7.29 关键修:跨场景时清 BaseAi HashSet,避免 stale wrapper 残留 → AccessViolation
        try { BaseAiRegistry.Known.Clear(); } catch { }
        // v2.7.64 清 HeatSource snapshot dict,防 long session 累积旧场景的 stale IntPtr
        try { Patch_HeatSource_Update.Snapshots.Clear(); } catch { }
        // v2.7.80 快速采集/割肉 snapshot 闭环 —— 实例随场景 GC,字段污染自然消失
        try { HarvestableSnaps.Snapshots.Clear(); } catch { }
        try { Patch_Harvest_Refresh_Quick.Snapshots.Clear(); } catch { }
        // v2.7.64 清 AutoPickupGuard 的 DroppedAt dict —— 跨 scene 后地上 gear 会重新 spawn,旧 Pointer 失效
        try { AutoPickupGuard.DroppedAt.Clear(); } catch { }
        try { Patch_GearDegrade.InvalidateCache(); } catch { }
        try { RespawnablePlantsState.OnSceneLoaded(); } catch { }
        try { ShowTraderTrustHelper.OnSceneChange(); } catch { }
        // v2.7.64 清 BreakDown snapshot —— 跨 scene 后 BreakDown 组件重新 spawn,stale ptr
        try { Patch_BreakDown_UpdateDuration.Snapshots.Clear(); } catch { }
        // v2.7.84 清 vp_FPSCamera 缓存 + 重置 aim 诊断 —— 跨场景相机实例重建
        try { CheatsTick.InvalidateCameraCache(); } catch { }
        // v2.7.94 MotionTrackerLite 默认启用雷达
        try { MotionTrackerLiteHelper.EnsureVisible(); } catch { }
        try { NLBFontFixHelper.OnSceneChange(); } catch { }
        // v2.7.94 地图传送缓存清理
        try { MapClickTeleport.OnSceneChange(); } catch { }
        // v2.7.96 CougarSoundBegone
        try { CougarSoundKiller.OnSceneLoad(sceneName); } catch { }
        // v2.7.96 BreakDownToolMemory 跨场景清理
        try { BreakDownToolMemory.LastToolIndex.Clear(); } catch { }
        // 场景切换是安全的 GC 时机 — 此时有 loading screen 遮挡，做完整回收
        try { System.GC.Collect(2, System.GCCollectionMode.Forced, true); } catch { }
    }
}

internal static class MotionTrackerLiteHelper
{
    private static System.Reflection.FieldInfo _visField;
    private static bool _resolved;

    public static void EnsureVisible()
    {
        if (!_resolved)
        {
            _resolved = true;
            var t = System.Type.GetType("MotionTrackerLite.Tracker, MotionTrackerLite")
                 ?? HarmonyLib.AccessTools.TypeByName("MotionTrackerLite.Tracker")
                 ?? HarmonyLib.AccessTools.TypeByName("MotionTrackerLite.ModMain");
            if (t != null)
                _visField = t.GetField("Enabled", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        }
        if (_visField != null) _visField.SetValue(null, true);
    }
}

// v4.0r5 NorthernLightsBroadcast 字体修复
// IL2CPP 模式下 NLB 的 assetbundle prefab 里 TMP_Text.fontAsset 反序列化后为 null → 所有文字空白
// 修法:场景加载后主动搜索所有 font==null 的 TextMeshProUGUI 并赋值(TLD vanilla 用 NGUI 不用 TMP,
// 所以 font==null 的 TMP 只可能来自第三方 mod bundle)
internal static class NLBFontFixHelper
{
    private static bool _fixed;
    public static bool Fixed => _fixed;
    private static int _scanAttempts;
    public static bool Exhausted { get; private set; }
    private static Il2CppTMPro.TMP_FontAsset _cachedFont;
    private static UnityEngine.Material _cachedMat;

    public static void OnSceneChange() { _scanAttempts = 0; Exhausted = false; }

    public static void PatchOnce()
    {
        if (Exhausted) return;
        try
        {
            var allTmp = UnityEngine.Resources.FindObjectsOfTypeAll<Il2CppTMPro.TextMeshProUGUI>();
            if (allTmp == null || allTmp.Count == 0) { if (++_scanAttempts > 10) Exhausted = true; return; }

            if (_cachedFont == null)
            {
                var all = UnityEngine.Resources.FindObjectsOfTypeAll<Il2CppTMPro.TMP_FontAsset>();
                if (all != null && all.Count > 0)
                {
                    foreach (var f in all)
                    {
                        if (f == null) continue;
                        try
                        {
                            if (f.name != null && f.name.Contains("LiberationSans") && f.material != null && f.atlasTexture != null)
                            { _cachedFont = f; break; }
                        }
                        catch { }
                    }
                    if (_cachedFont == null)
                    {
                        foreach (var f in all)
                        {
                            if (f == null) continue;
                            try
                            {
                                if (f.name != null && !f.name.Contains("Arcade") && f.material != null && f.atlasTexture != null)
                                { _cachedFont = f; break; }
                            }
                            catch { }
                        }
                    }
                    if (_cachedFont == null)
                    {
                        foreach (var f in all)
                        {
                            if (f == null) continue;
                            try { if (f.material != null) { _cachedFont = f; break; } } catch { }
                        }
                    }
                }
            }
            if (_cachedFont == null) return;
            if (_cachedMat == null) try { _cachedMat = _cachedFont.material; } catch { }

            int count = 0;
            foreach (var tmp in allTmp)
            {
                if (tmp == null) continue;
                bool broken = false;
                try { broken = (tmp.font == null); } catch { broken = true; }
                if (!broken)
                {
                    try { var n = tmp.font.name; if (n == null) broken = true; } catch { broken = true; }
                }
                if (!broken)
                {
                    try { var m = tmp.font.material; if (m == null) broken = true; } catch { broken = true; }
                }
                if (!broken)
                {
                    try { var a = tmp.font.atlasTexture; if (a == null) broken = true; } catch { broken = true; }
                }
                if (broken)
                {
                    tmp.font = _cachedFont;
                    if (_cachedMat != null)
                        try { tmp.fontSharedMaterial = _cachedMat; } catch { }
                    try { tmp.ForceMeshUpdate(true, true); } catch { }
                    count++;
                }
            }
            if (count > 0)
            {
                _fixed = true;
                ModMain.Log?.Msg($"[NLB] Fixed {count} TMP_Text with broken font -> {_cachedFont.name}");
            }
        }
        catch { }
    }
}

// v2.7.95 整合 Jump mod — 完整版(支持负重/卡路里/体力/疲劳限制 + JumpKing 无限制模式)
internal static class JumpHelper
{
    public static void Tick()
    {
        try
        {
            var s = ModMain.Settings;
            if (s == null || s.JumpKey == UnityEngine.KeyCode.None) return;
            if (!Il2Cpp.InputManager.GetKeyDown(Il2Cpp.InputManager.m_CurrentContext, s.JumpKey)) return;
            var cam = Il2Cpp.GameManager.GetVpFPSCamera();
            if (cam == null || !cam.Controller.isGrounded) return;
            if (Il2Cpp.uConsole.m_On || Il2Cpp.InterfaceManager.IsOverlayActiveCached()) return;
            if (Il2Cpp.GameManager.GetPlayerManagerComponent().PlayerIsCrouched()) return;

            var player = Il2Cpp.GameManager.GetVpFPSPlayer();
            float jumpForce = s.JumpHeight / 100f;

            if (s.JumpKing || Il2Cpp.GameManager.GetPlayerManagerComponent().m_God)
            {
                player.Controller.MotorJumpForce = jumpForce;
                player.Controller.Jump();
                return;
            }

            // 限制检查
            if (Il2Cpp.GameManager.GetSprainedAnkleComponent().HasSprainedAnkle())
            { Il2Cpp.HUDMessage.AddMessage(I18n.T("你受伤了，无法跳跃", "Sprained — can't jump"), true, true); return; }

            float weight = Il2Cpp.GameManager.GetInventoryComponent().GetTotalWeightKG().ToQuantity(1f);
            float weightCap = s.JumpWeightLimit + s.JumpWeightLimit * 0.334f;
            if (weight >= weightCap)
            { Il2Cpp.HUDMessage.AddMessage(I18n.T("携带物品过重，无法跳跃", "Too heavy — can't jump"), true, true); return; }

            if ((int)Il2Cpp.GameManager.GetHungerComponent().GetHungerLevel() == 4)
            { Il2Cpp.HUDMessage.AddMessage(I18n.T("饥饿状态无法跳跃", "Starving — can't jump"), true, true); return; }

            if ((int)Il2Cpp.GameManager.GetThirstComponent().GetThirstLevel() == 4)
            { Il2Cpp.HUDMessage.AddMessage(I18n.T("脱水时无法跳跃", "Dehydrated — can't jump"), true, true); return; }

            if ((int)Il2Cpp.GameManager.GetFatigueComponent().GetFatigueLevel() == 4)
            { Il2Cpp.HUDMessage.AddMessage(I18n.T("疲惫时无法跳跃", "Exhausted — can't jump"), true, true); return; }

            float stamina = Il2Cpp.GameManager.GetPlayerMovementComponent().m_SprintStamina;
            if (stamina < s.JumpStaminaCost * 0.5f)
            { return; }

            // 负重影响跳跃高度
            float weightRatio = weight / s.JumpWeightLimit * 100f;
            float heightMult = 100f / (1f + UnityEngine.Mathf.Pow(weightRatio / 115f, 10f)) / 100f;
            player.Controller.MotorJumpForce = jumpForce * heightMult;

            // 消耗
            if (s.JumpCalorieCost > 0)
            {
                float calCost = (UnityEngine.Mathf.Round(heightMult * 100f) >= 100f)
                    ? s.JumpCalorieCost * 0.5f : s.JumpCalorieCost;
                Il2Cpp.GameManager.GetHungerComponent().RemoveReserveCalories(calCost);
            }

            if (s.JumpStaminaCost > 0)
                Il2Cpp.GameManager.GetPlayerMovementComponent().AddSprintStamina(-s.JumpStaminaCost);

            if (s.JumpFatigueCost > 0)
                Il2Cpp.GameManager.GetFatigueComponent().AddFatigue(s.JumpFatigueCost);

            player.Controller.Jump();
        }
        catch { }
    }
}
