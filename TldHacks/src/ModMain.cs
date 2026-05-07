using System;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(TldHacks.ModMain), "TldHacks", "3.0.4", "user")]
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
            Settings = new TldHacksSettings();
            Settings.AddToModSettings("TldHacks");

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
                DynamicPatch.Reconcile();
            }

            // v2.7.64 加载 scene transition 历史记录
            TransitionRecorder.Init();
            Log.Msg($"TldHacks v2.7.96 loaded — menu hotkey = {Settings.MenuHotkey}, items = {ItemDatabase.All.Count}+{ItemDatabaseMod.All.Count} mod, transitions = {TransitionRecorder.Count}");
        }
        catch (Exception ex) { Log.Error($"[Init] {ex}"); }
    }

    private void SyncStateFromSettings()
    {
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
        // v2.7.96 整合 Batch 2
        CheatState.PauseInJournal = Settings.PauseInJournal;
        CheatState.SkipIntro = Settings.SkipIntro;
        CheatState.MuteCougarMenuSound = Settings.MuteCougarMenuSound;
        CheatState.VehicleKeepFov = Settings.VehicleKeepFov;
        CheatState.DroppableUndroppables = Settings.DroppableUndroppables;
        CheatState.RememberBreakdownTool = Settings.RememberBreakdownTool;
        CheatState.VehicleFreeLook = Settings.VehicleFreeLook;
        // v2.7.92 ESP/AutoAim UI 已删 → 强制 false,防 JSON 残留值激活每帧重扫
        CheatStateESP.ESP = false;
        CheatStateESP.AutoAim = false;
        CheatStateESP.MagicBullet = Settings.MagicBullet;
        CheatStateESP.AutoAimFOV = Settings.AutoAimFOV;
        CheatStateESP.AutoAimSpeed = Settings.AutoAimSpeed;
        CheatStateESP.AimPart = Settings.AimPart;
        CheatStateESP.RecoilScale = Settings.RecoilScaleESP;
        CheatStateESP.FireRateScale = Settings.FireRateScale;
        CheatStateESP.ReloadScale = Settings.ReloadScale;
        CheatStateESP.ESPRange = Settings.ESPRange;
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
        CheatState.World_ElectricTorch = Settings.World_ElectricTorch;
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
                Menu.Toggle();

            // Fly toggle hotkey
            if (Settings.FlyHotkey != KeyCode.None && Input.GetKeyDown(Settings.FlyHotkey))
            {
                CheatState.CFly = !CheatState.CFly;
                ConsoleBridge.Run("fly");
            }
            // AutoAim toggle hotkey
            if (Settings.AutoAimHotkey != KeyCode.None && Input.GetKeyDown(Settings.AutoAimHotkey))
                CheatStateESP.AutoAim = !CheatStateESP.AutoAim;

            // Time scale —— 按住加速,松开恢复
            // v2.8.1: 不再用 DetermineIfOverlayIsActive (读条面板也算 overlay 导致加速失效)
            //   只在暂停菜单/uConsole 时屏蔽,读条/制作/搜刮等进度条期间允许加速
            {
                bool held1 = Settings.TimeScaleKey1 != KeyCode.None && Input.GetKey(Settings.TimeScaleKey1);
                bool held2 = Settings.TimeScaleKey2 != KeyCode.None && Input.GetKey(Settings.TimeScaleKey2);
                bool blocked = uConsole.m_On;
                try { blocked = blocked || InterfaceManager.GetPanel<Panel_PauseMenu>().isActiveAndEnabled; } catch { }
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
            }

            // v2.7.75 稳定节奏:动态 patch 只在固定低频点 reconcile。
            if ((_frame % 30) == 25) DynamicPatch.Reconcile();

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

            // v2.7.94 双击地图图标传送
            MapClickTeleport.Tick();

            // v2.7.94 TweaksRuntime: 实时应用滑块设置(移动/图形/光源/衰减)
            TweaksRuntime.Tick();


            // v2.7.95 跳跃(整合 Jump mod) — v3.0.3 加总开关
            if (Settings.JumpEnabled) JumpHelper.Tick();

            // v2.7.95 GunZoom 滚轮输入
            GunZoomState.UpdateScroll();


            // v2.7.92 性能:ESP/AutoAim UI 已删,只保留 MagicBullet 预览(降频 0.25s)
            if (CheatStateESP.MagicBullet) MagicBulletSystem.UpdatePreview();

            WakeUpCallHelper.OnUpdateTick();
            AutoSurveyHelper.OnUpdateTick();
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
        ESPOverlay.OnGUI();
    }

    public override void OnSceneWasInitialized(int buildIndex, string sceneName)
    {
        Teleport.OnSceneLoaded(sceneName);
        // v2.7.64 学习 scene transition 字段到 JSON,解决 DLC Tale 跨区物品丢失
        TransitionRecorder.OnSceneInitialized(sceneName);
        // v2.7.29 关键修:跨场景时清 BaseAi HashSet,避免 stale wrapper 残留 → AccessViolation
        try { BaseAiRegistry.Known.Clear(); } catch { }
        // v2.7.64 清 Fire/HeatSource snapshot dicts,防 long session 累积旧场景的 stale IntPtr
        try { Patch_Fire_Update_NeverDie.Snapshots.Clear(); } catch { }
        try { Patch_HeatSource_Update.Snapshots.Clear(); } catch { }
        // v2.7.80 快速采集/割肉 snapshot 闭环 —— 实例随场景 GC,字段污染自然消失
        try { HarvestableSnaps.Snapshots.Clear(); } catch { }
        try { Patch_Harvest_Refresh_Quick.Snapshots.Clear(); } catch { }
        // v2.7.64 清 AutoPickupGuard 的 DroppedAt dict —— 跨 scene 后地上 gear 会重新 spawn,旧 Pointer 失效
        try { AutoPickupGuard.DroppedAt.Clear(); } catch { }
        // v2.7.64 清 BreakDown snapshot —— 跨 scene 后 BreakDown 组件重新 spawn,stale ptr
        try { Patch_BreakDown_UpdateDuration.Snapshots.Clear(); } catch { }
        // v2.7.84 清 vp_FPSCamera 缓存 + 重置 aim 诊断 —— 跨场景相机实例重建
        try { CheatsTick.InvalidateCameraCache(); } catch { }
        // v2.7.94 MotionTrackerLite 默认启用雷达
        try { MotionTrackerLiteHelper.EnsureVisible(); } catch { }
        // v2.7.94 地图传送缓存清理
        try { MapClickTeleport.OnSceneChange(); } catch { }
        // v2.7.96 CougarSoundBegone
        try { CougarSoundKiller.OnSceneLoad(sceneName); } catch { }
        // v2.7.96 BreakDownToolMemory 跨场景清理
        try { BreakDownToolMemory.LastToolIndex.Clear(); } catch { }
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
                _visField = t.GetField("Visible", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        }
        if (_visField != null) _visField.SetValue(null, true);
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
            { Il2Cpp.HUDMessage.AddMessage("你受伤了，无法跳跃", true, true); return; }

            float weight = Il2Cpp.GameManager.GetInventoryComponent().GetTotalWeightKG().ToQuantity(1f);
            float weightCap = s.JumpWeightLimit + s.JumpWeightLimit * 0.334f;
            if (weight >= weightCap)
            { Il2Cpp.HUDMessage.AddMessage("携带物品过重，无法跳跃", true, true); return; }

            if ((int)Il2Cpp.GameManager.GetHungerComponent().GetHungerLevel() == 4)
            { Il2Cpp.HUDMessage.AddMessage("饥饿状态无法跳跃", true, true); return; }

            if ((int)Il2Cpp.GameManager.GetThirstComponent().GetThirstLevel() == 4)
            { Il2Cpp.HUDMessage.AddMessage("脱水时无法跳跃", true, true); return; }

            if ((int)Il2Cpp.GameManager.GetFatigueComponent().GetFatigueLevel() == 4)
            { Il2Cpp.HUDMessage.AddMessage("疲惫时无法跳跃", true, true); return; }

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
