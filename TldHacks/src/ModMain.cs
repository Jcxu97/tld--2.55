using System;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(TldHacks.ModMain), "TldHacks", "2.7.14", "user")]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: MelonAdditionalDependencies("ModSettings")]

namespace TldHacks;

public class ModMain : MelonMod
{
    public static MelonLogger.Instance Log;
    internal static TldHacksSettings Settings;

    private int _durabilityTick = 0;
    private int _killTick = 0;
    private int _posTick = 0;
    private int _extraTick = 0;
    private int _camTick = 0;
    private int _animalsTick = 0;

    public override void OnInitializeMelon()
    {
        Log = LoggerInstance;
        try
        {
            Settings = new TldHacksSettings();
            Settings.AddToModSettings("TldHacks");

            // 把 Settings 持久值同步到 CheatState
            SyncStateFromSettings();
            Log.Msg($"TldHacks v2.7.14 loaded — menu hotkey = {Settings.MenuHotkey}, items = {ItemDatabase.All.Count}");
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
    }

    public override void OnUpdate()
    {
        try
        {
            if (Settings == null) return;

            // Menu toggle
            if (Settings.MenuHotkey != KeyCode.None && Input.GetKeyDown(Settings.MenuHotkey))
                Menu.Toggle();

            // Fly toggle hotkey
            if (Settings.FlyHotkey != KeyCode.None && Input.GetKeyDown(Settings.FlyHotkey))
            {
                CheatState.CFly = !CheatState.CFly;
                ConsoleBridge.Run("fly");
            }

            // Time scale
            if (CheatState.SpeedMultiplier > 0f && Mathf.Abs(Time.timeScale - CheatState.SpeedMultiplier) > 0.01f)
                Time.timeScale = CheatState.SpeedMultiplier;

            // (InfiniteCarry 已去除,由其他 mod 覆盖)

            // 所有 tick 都降频:避免 FindObjectsOfType 高频扫场景导致掉帧
            // 原 30/60 帧太猛;大部分 cheat 状态刷 1-5 秒一次足够
            // Infinite durability: ~5 秒扫一次
            if (CheatState.InfiniteDurability && ++_durabilityTick >= 300)
            {
                _durabilityTick = 0;
                Cheats.TickInfiniteDurability();
            }

            // 一击必杀改成命中伤害放大(Patch_BaseAi_ApplyDamage),不再扫场景自动 kill
            // (Cheats.ScanAndKillAnimals 保留给一次性手动按钮 / uConsole kill_all_animals)

            // Guns / Animals / Fires ~1.5 秒。三个 tick 里部分功能要"持续保持",
            // 降太多会手感差(如开枪时弹药回填延迟)。~90 帧是折中。
            // 而且只有相应 toggle 开了才进入扫描(每个 tick 内部有 early return)
            // Stealth / TrueInvisible:60 帧(~1s)—— 新 AI 生成后最多 1s 内被压制
            if (++_animalsTick >= 60)
            {
                _animalsTick = 0;
                CheatsTick.TickAnimals();
            }

            if (++_extraTick >= 90)
            {
                _extraTick = 0;
                CheatsTick.TickGuns();
                CheatsTick.TickClothingWetness();
                CheatsTick.TickClimbRope();
                CheatsTick.TickStatus();
                CheatsTick.TickLocks();
                CheatsTick.TickQuickActions();
                // 窗口状态类 one-shot apply:每次循环都调,内部 toggle 关时 DisableWindEffect 会被原游戏自己覆盖 —— 可以接受
                ExtraOneShot.TickStopWind();
                ExtraOneShot.TickSprainRisk();
            }

            // 摄像机 / 武器 aim 相关:60 帧 ≈ 1 秒,进一步降 FPS 开销
            if (++_camTick >= 60)
            {
                _camTick = 0;
                CheatsTick.TickCamera();
            }

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
        Stacking.OnLateUpdate();
    }

    public override void OnGUI()
    {
        Menu.Draw();
    }

    public override void OnSceneWasInitialized(int buildIndex, string sceneName)
    {
        Teleport.OnSceneLoaded(sceneName);
    }
}
