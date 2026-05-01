using System;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(TldHacks.ModMain), "TldHacks", "2.7.79", "user")]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: MelonAdditionalDependencies("ModSettings")]

namespace TldHacks;

public class ModMain : MelonMod
{
    public static MelonLogger.Instance Log;
    internal static TldHacksSettings Settings;

    private int _posTick = 0;
    // v2.7.21 —— 单调递增帧计数器,用 modulo 把 7 个 tick 摊到不同帧,消除 90 帧 1 次的集中大 spike
    private int _frame = 0;

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
            Log.Msg($"TldHacks v2.7.83 loaded — menu hotkey = {Settings.MenuHotkey}, items = {ItemDatabase.All.Count}+{ItemDatabaseMod.All.Count} mod, transitions = {TransitionRecorder.Count}");
        }
        catch (Exception ex) { Log.Error($"[Init] {ex}"); }
    }

    private void SyncStateFromSettings()
    {
        CheatState.GodMode = Settings.GodMode;
        CheatState.NoFallDamage = Settings.NoFallDamage;
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
        CheatState.QuickCraft = Settings.QuickCraft;
        CheatState.InfiniteAmmo = Settings.InfiniteAmmo;
        CheatState.NoJam = Settings.NoJam;
        CheatState.NoRecoil = Settings.NoRecoil;
        CheatState.NoAimSway = Settings.NoAimSway;
        CheatState.NoAimShake = Settings.NoAimShake;
        CheatState.NoBreathSway = Settings.NoBreathSway;
        CheatState.NoAimStamina = Settings.NoAimStamina;
        CheatState.NoAimDOF = Settings.NoAimDOF;
        CheatState.SpeedMultiplier = Settings.SpeedMultiplier;
        CheatState.FastFire = Settings.FastFire;
        CheatState.StopWind = Settings.StopWind;
        CheatState.NoSprainRisk = Settings.NoSprainRisk;
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
        CheatState.FlaskNoHeatLoss = Settings.FlaskNoHeatLoss;
        CheatState.FlaskInfiniteVol = Settings.FlaskInfiniteVol;
        CheatState.FlaskAnyItem = Settings.FlaskAnyItem;
        CheatState.QuickEvolve = Settings.QuickEvolve;
        CheatState.InfiniteContainer = Settings.InfiniteContainer;
        CheatState.FireTemp300 = Settings.FireTemp300;
        CheatState.FireNeverDie = Settings.FireNeverDie;
        CheatState.CureFrostbite = Settings.CureFrostbite;
        CheatState.ClearDeathPenalty = Settings.ClearDeathPenalty;
        CheatState.QuickFishing = Settings.QuickFishing;
        // v2.7.64 商人 + 美洲狮
        CheatState.TraderUnlimitedList = Settings.TraderUnlimitedList;
        CheatState.TraderMaxTrust = Settings.TraderMaxTrust;
        CheatState.TraderInstantExchange = Settings.TraderInstantExchange;
        CheatState.TraderAlwaysAvailable = Settings.TraderAlwaysAvailable;
        CheatState.CougarInstantActivate = Settings.CougarInstantActivate;
        CheatState.BlockAutoPickupOwnDrops = Settings.BlockAutoPickupOwnDrops;
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

            // Fly toggle hotkey —— 实测 work,保留(之前 HANDOFF 说 release 无效,与实际不符)
            if (Settings.FlyHotkey != KeyCode.None && Input.GetKeyDown(Settings.FlyHotkey))
            {
                CheatState.CFly = !CheatState.CFly;
                ConsoleBridge.Run("fly");
            }

            // Time scale —— v2.7.21:默认 1.0 时不写回,避免覆盖第三方时间加速 mod
            // 只有用户在菜单主动改成 != 1.0 的倍率时才强制同步
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

            // v2.7.25 TickStatus 从 180 帧 → 60 帧(1s)—— 取代 5 个被删的每帧 Update Postfix
            // 饥/渴/累/寒 1s 内几乎不可能出现异常上升,TickStatus 60 帧重置一次够用
            if ((_frame % 60) == 10)   CheatsTick.TickStatus();

            // 其余 sub-tick 每 180 帧一次(3s),错开 phase
            if ((_frame % 180) == 15)  CheatsTick.TickGuns();
            if ((_frame % 180) == 45)  CheatsTick.TickClothingWetness();
            if ((_frame % 180) == 75)  CheatsTick.TickClimbRope();
            if ((_frame % 180) == 135) CheatsTick.TickLocks();
            if ((_frame % 180) == 165) CheatsTick.TickQuickActions();

            // One-shot 类:状态变化时重设即可,低频 OK
            if ((_frame % 180) == 90) { ExtraOneShot.TickStopWind(); ExtraOneShot.TickSprainRisk(); }

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
        }
        catch (Exception ex) { Log?.Error($"[OnUpdate] {ex}"); }
    }

    public override void OnLateUpdate()
    {
        if (Settings != null && (Settings.DiagPauseRuntime || Settings.DiagUnpatchAll)) return;
        Stacking.OnLateUpdate();
    }

    public override void OnGUI()
    {
        if (Settings != null && (Settings.DiagPauseRuntime || Settings.DiagUnpatchAll)) return;
        Menu.Draw();
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
    }
}
