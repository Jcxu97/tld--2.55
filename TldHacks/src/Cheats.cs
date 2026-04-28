using System;
using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TldHacks;

// 所有作弊相关的运行时状态。静态,方便 Harmony patch 里 access
// 状态写入由 Menu / Settings 触发,OnInitializeMelon 会从 Settings 同步过来
internal static class CheatState
{
    // Life / 生命
    public static bool GodMode;
    public static bool NoFallDamage;
    // Status / 状态(注:InfiniteStamina 去掉 —— UniversalTweaks / 其他 mod 已覆盖)
    public static bool AlwaysWarm;
    public static bool NoHunger;
    public static bool NoThirst;
    public static bool NoFatigue;
    // Movement / 移动(注:InfiniteCarry 去掉 —— 其他 mod 已覆盖)
    public static float SpeedMultiplier = 1f;
    // Animals / 动物
    public static bool InstantKillAnimals;
    public static bool FreezeAnimals;
    public static bool Stealth;
    // World
    public static bool ThinIceNoBreak;
    public static bool IgnoreLock;
    public static bool QuickOpenContainer;
    // Items / Fire
    public static bool InfiniteDurability;
    public static bool NoWetClothes;
    // InfiniteFireDurations 去除 —— 其他 mod 已覆盖
    // Crafting
    public static bool FreeCraft;
    public static bool QuickCraft;
    // Weapons
    public static bool InfiniteAmmo;
    public static bool NoJam;
    public static bool NoRecoil;
    // Aiming
    public static bool NoAimSway;
    public static bool NoAimShake;
    public static bool NoBreathSway;
    public static bool NoAimStamina;
    public static bool NoAimDOF;
    // Environment / body
    public static bool StopWind;
    public static bool NoSprainRisk;
    public static bool ImmuneAnimalDamage; // Wolf/Bear/Cougar 攻击不扣血
    public static bool NoSuffocating;      // 不会窒息
    // Skills / shortcuts
    public static bool QuickFire;          // 生火 100% 成功
    public static bool QuickClimb;         // 爬绳速度 ×5
    public static bool QuickAction;        // 采集 / 修理 / 拆解 自动时间加速
    // Display
    public static string PositionText = "";
    public static string LastActionLog = "";  // Menu 里显示最后一次按钮操作的结果

    // Console-command-based toggles(内存态,不持久化 —— 每次启动游戏要重新开)
    public static bool CInvulnerable;
    public static bool CInvisible;
    public static bool CNoJamConsole;
    public static bool CNoSprain;
    public static bool CFly;

    // Fast fire(tick 类,持久化)
    public static bool FastFire;
}

// 工具函数:刷物品/清 affliction/天气/时间/耐久恢复
internal static class Cheats
{
    public static void SpawnItem(string prefabName, int quantity)
    {
        try
        {
            var pm = GameManager.GetPlayerManagerComponent();
            if (pm == null) { ModMain.Log?.Warning("[Cheats.Spawn] PlayerManager not ready"); return; }
            for (int i = 0; i < quantity; i++) pm.AddItemCONSOLE(prefabName, 1, 100f);
            ModMain.Log?.Msg($"[Cheats.Spawn] +{quantity} {prefabName}");
        }
        catch (Exception ex) { ModMain.Log?.Error($"[Cheats.Spawn] {ex.Message}"); }
    }

    public static void ClearAllAfflictions()
    {
        int cleared = 0;
        var log = new System.Text.StringBuilder();
        // v2.7.11:放弃依赖 End() —— 许多 End 方法要求"满足治愈条件"才真正清除。
        // 直接用 wrapper 类型 setter 强置内部状态为 default,外加 End() 走一遍音效/UI 刷新。

        // —— Hypothermia ——
        try { var c = GameManager.GetHypothermiaComponent(); if (c != null) {
            bool had = c.HasHypothermia();
            try { c.m_Active = false; } catch { }
            try { c.m_ElapsedHours = 0f; } catch { }
            try { c.m_ElapsedWarmTime = 0f; } catch { }
            try { c.m_StartHasBeenCalled = false; } catch { }
            try { c.m_SuppressHypothermia = true; c.HypothermiaEnd(true); c.m_SuppressHypothermia = false; } catch { }
            if (had) { cleared++; log.Append("低温; "); }
        }} catch (Exception ex) { log.Append($"低温-err:{ex.Message}; "); }

        // —— Frostbite ——
        try { var c = GameManager.GetFrostbiteComponent(); if (c != null) {
            bool had = c.HasFrostbite();
            try { c.FrostbiteEnd(); } catch { }
            try { c.m_SuppressFrostbite = true; } catch { }
            if (had) { cleared++; log.Append("冻伤; "); }
        }} catch (Exception ex) { log.Append($"冻伤-err:{ex.Message}; "); }

        // —— CabinFever ——
        try { var c = GameManager.GetCabinFeverComponent(); if (c != null) {
            bool had = c.HasCabinFever();
            try { c.m_Active = false; } catch { }
            try { c.CabinFeverEnd(); c.ClearCabinFeverRisk(); } catch { }
            if (had) { cleared++; log.Append("幽闭症; "); }
        }} catch (Exception ex) { log.Append($"幽闭症-err:{ex.Message}; "); }

        // —— Dysentery ——
        try { var c = GameManager.GetDysenteryComponent(); if (c != null) {
            bool had = c.HasDysentery();
            try { c.m_Active = false; } catch { }
            try { c.DysenteryEnd(true); } catch { }
            if (had) { cleared++; log.Append("痢疾; "); }
        }} catch (Exception ex) { log.Append($"痢疾-err:{ex.Message}; "); }

        // —— FoodPoisoning ——
        try { var c = GameManager.GetFoodPoisoningComponent(); if (c != null) {
            bool had = c.HasFoodPoisoning();
            try { c.m_Active = false; } catch { }
            try { c.FoodPoisoningEnd(true); } catch { }
            if (had) { cleared++; log.Append("食物中毒; "); }
        }} catch (Exception ex) { log.Append($"食物中毒-err:{ex.Message}; "); }

        // —— SprainedWrist / Ankle ——
        try { var c = GameManager.GetSprainedWristComponent(); if (c != null) {
            bool had = c.HasSprainedWrist();
            try { c.SetForceNoSprainWrist(true); } catch { }
            // 没 Active 字段,直接 End(0)
            try { c.SprainedWristEnd(0, (AfflictionOptions)0); } catch { }
            try { c.SprainedWristEnd(1, (AfflictionOptions)0); } catch { } // 另一只手
            if (had) { cleared++; log.Append("扭腕; "); }
        }} catch (Exception ex) { log.Append($"扭腕-err:{ex.Message}; "); }

        try { var c = GameManager.GetSprainedAnkleComponent(); if (c != null) {
            bool had = c.HasSprainedAnkle();
            try { c.SprainedAnkleEnd(0, (AfflictionOptions)0); } catch { }
            try { c.SprainedAnkleEnd(1, (AfflictionOptions)0); } catch { }
            if (had) { cleared++; log.Append("扭踝; "); }
        }} catch (Exception ex) { log.Append($"扭踝-err:{ex.Message}; "); }

        // —— BloodLoss ——
        try { var c = GameManager.GetBloodLossComponent(); if (c != null) {
            bool had = c.HasBloodLoss();
            // 对每个身体部位都 End
            for (int i = 0; i < 6; i++) try { c.BloodLossEnd(i, (AfflictionOptions)0); } catch { }
            if (had) { cleared++; log.Append("出血; "); }
        }} catch (Exception ex) { log.Append($"出血-err:{ex.Message}; "); }

        // —— Infection ——
        try { var c = GameManager.GetInfectionComponent(); if (c != null) {
            bool had = c.HasInfection();
            for (int i = 0; i < 6; i++) try { c.InfectionEnd(i); } catch { }
            if (had) { cleared++; log.Append("感染; "); }
        }} catch (Exception ex) { log.Append($"感染-err:{ex.Message}; "); }

        // —— BrokenRib ——
        try { var c = GameManager.GetBrokenRibComponent(); if (c != null) {
            bool had = c.HasBrokenRib();
            for (int i = 0; i < 6; i++) try { c.BrokenRibEnd(i, true); } catch { }
            if (had) { cleared++; log.Append("骨折; "); }
        }} catch (Exception ex) { log.Append($"骨折-err:{ex.Message}; "); }

        string summary = cleared > 0 ? $"已清 {cleared} 项: {log}" : $"未检测到活跃负面; 尝试过: {log}";
        ModMain.Log?.Msg($"[Cheats] {summary}");
        CheatState.LastActionLog = summary;
    }

    // 真正切天气:WeatherTransition.ActivateWeatherSetImmediate(WeatherStage)
    // 这是 public 方法,不走 uConsole(release build 下 no-op)。
    // 从场景里找 WeatherTransition 实例 —— 反射调 GameManager 或 FindObjectOfType 兜底
    public static void SetWeatherStage(int stage)
    {
        try
        {
            WeatherTransition wt = null;

            // 尝试 GameManager.GetWeatherTransitionComponent()(如果存在)
            try
            {
                var m = typeof(GameManager).GetMethod("GetWeatherTransitionComponent", BindingFlags.Static | BindingFlags.Public);
                if (m != null) wt = m.Invoke(null, null) as WeatherTransition;
            }
            catch { }

            // 兜底:场景里找
            if (wt == null)
            {
                try { wt = UnityEngine.Object.FindObjectOfType<WeatherTransition>(); } catch { }
            }

            if (wt == null)
            {
                ModMain.Log?.Warning("[Weather] WeatherTransition not found in scene");
                CheatState.LastActionLog = "[Weather] 没找到 WeatherTransition";
                return;
            }

            // WeatherStage 是 enum,int 可以直接转
            WeatherStage stageEnum = (WeatherStage)stage;
            wt.ActivateWeatherSetImmediate(stageEnum);
            ModMain.Log?.Msg($"[Weather] ActivateWeatherSetImmediate({stageEnum})");
            CheatState.LastActionLog = $"[Weather] → {stageEnum}";
        }
        catch (Exception ex) { ModMain.Log?.Error($"[Weather] {ex.Message}"); }
    }

    public static void SetTimeOfDay(float hour)
    {
        try
        {
            var tod = GameManager.GetTimeOfDayComponent();
            if (tod == null) { ModMain.Log?.Error("[Cheats.Time] ToD null"); return; }
            float norm = Mathf.Clamp01(hour / 24f);
            var m1 = tod.GetType().GetMethod("SetNormalizedTime", BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(float) }, null);
            if (m1 != null) { m1.Invoke(tod, new object[] { norm }); }
            else
            {
                var m2 = tod.GetType().GetMethod("SetNormalizedTime", BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(float), typeof(bool) }, null);
                if (m2 == null) { ModMain.Log?.Error("[Cheats.Time] no SetNormalizedTime"); return; }
                m2.Invoke(tod, new object[] { norm, false });
            }
            ModMain.Log?.Msg($"[Cheats] Time → {hour:F1}h");
        }
        catch (Exception ex) { ModMain.Log?.Error($"[Cheats.Time] {ex.Message}"); }
    }

    // 每 60 帧扫一次 GearItem 刷耐久。代价大,只在 InfiniteDurability 开时跑。
    // TldItemSpawner 同款做法
    public static void TickInfiniteDurability()
    {
        try
        {
            var gears = UnityEngine.Object.FindObjectsOfType<GearItem>();
            if (gears == null) return;
            foreach (var g in gears) RestoreDurability(g);
        }
        catch { }
    }

    internal static void RestoreDurability(GearItem gear)
    {
        if (gear == null) return;
        try
        {
            // v2.7.10:衣服也一视同仁 —— m_CurrentHP 对 Clothing / Tool / Weapon / Food 都是 0-100
            if (gear.m_CurrentHP < 100f) gear.m_CurrentHP = 100f;
        }
        catch { }
    }

    // 修复玩家当前手持物品 —— 一键
    public static void RepairItemInHands()
    {
        try
        {
            var pm = GameManager.GetPlayerManagerComponent();
            if (pm == null) { CheatState.LastActionLog = "[修复手持] no PM"; return; }
            var item = pm.m_ItemInHands;
            if (item == null) { CheatState.LastActionLog = "[修复手持] 手上没东西"; return; }
            RestoreDurability(item);
            CheatState.LastActionLog = $"[修复手持] {item.name} → 100%";
            ModMain.Log?.Msg($"[Repair.Hands] {item.name}");
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[Repair.Hands] {ex.Message}"); }
    }

    internal static void RestoreClothingDurability(object clothing)
    {
        if (clothing == null) return;
        try
        {
            var type = clothing.GetType();
            // 先找 max 作为目标值,fallback 100
            float target = 100f;
            foreach (var maxName in new[] { "m_MaxHP", "m_MaxCondition", "m_MaxDurability", "MaxHP", "MaxCondition", "MaxDurability" })
            {
                try
                {
                    var f = type.GetField(maxName, BindingFlags.Instance | BindingFlags.Public);
                    if (f != null) { var v = f.GetValue(clothing); if (v != null) { target = Convert.ToSingle(v); break; } }
                }
                catch { }
            }
            foreach (var curName in new[] { "m_CurrentHP", "m_CurrentCondition", "m_CurrentDurability", "CurrentHP", "CurrentCondition", "CurrentDurability" })
            {
                try
                {
                    var f = type.GetField(curName, BindingFlags.Instance | BindingFlags.Public);
                    if (f != null) { f.SetValue(clothing, target); break; }
                }
                catch { }
            }
        }
        catch { }
    }

    // 扫场景所有 BaseAi 实例直接调 DebugKill / ApplyDamage —— 比 SendMessage 可靠
    public static void ScanAndKillAnimals()
    {
        try
        {
            var ais = UnityEngine.Object.FindObjectsOfType<BaseAi>();
            if (ais == null) return;
            int kills = 0;
            foreach (var ai in ais)
            {
                if (ai == null) continue;
                try
                {
                    var go = ((Component)ai).gameObject;
                    if (go == null || !go.activeInHierarchy) continue;
                    // 优先 DebugKill(一步到位),失败了 fallback 到 ApplyDamage 9999
                    bool killed = false;
                    try { ai.DebugKill(); killed = true; } catch { }
                    if (!killed)
                    {
                        try { ai.ApplyDamage(9999f, 0f, DamageSource.Player, ""); killed = true; } catch { }
                    }
                    if (!killed)
                    {
                        try { ai.EnterDead(); killed = true; } catch { }
                    }
                    if (killed) kills++;
                    if (kills > 30) break;
                }
                catch { }
            }
            if (kills > 0) ModMain.Log?.Msg($"[Cheats.Kill] killed {kills}");
            CheatState.LastActionLog = $"已击杀 {kills} 只";
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[Cheats.Kill] {ex.Message}"); }
    }

    // 全地图揭示 —— 反射调 RegionManager 的各种 "RevealAll" 方法
    public static void RevealFullMap()
    {
        try
        {
            object rm = null;
            try
            {
                var instField = typeof(GameManager).GetField("Instance", BindingFlags.Static | BindingFlags.Public);
                var inst = instField?.GetValue(null);
                if (inst != null)
                {
                    var m = inst.GetType().GetMethod("GetRegionManagerComponent", BindingFlags.Instance | BindingFlags.Public);
                    rm = m?.Invoke(inst, null);
                }
            }
            catch { }
            if (rm == null) { ModMain.Log?.Warning("[Map] no RegionManager"); return; }

            foreach (var name in new[] { "RevealAllRegions", "SetAllRegionsExplored", "UnlockAllRegions", "DiscoverAllRegions" })
            {
                try
                {
                    var m = rm.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.Public);
                    if (m != null) { m.Invoke(rm, null); ModMain.Log?.Msg($"[Map] called {name}"); return; }
                }
                catch { }
            }
            ModMain.Log?.Warning("[Map] no reveal method found on RegionManager");
        }
        catch (Exception ex) { ModMain.Log?.Error($"[Map] {ex.Message}"); }
    }

    // 玩家位置,每 10 帧更新。PositionText 给 Menu 显示用
    public static void UpdatePlayerPosition()
    {
        try
        {
            var pm = GameManager.GetPlayerManagerComponent();
            if (pm == null) return;

            Transform tr = null;
            // try 1: pm.GetControlledGameObject() 方法
            try
            {
                var mi = pm.GetType().GetMethod("GetControlledGameObject", BindingFlags.Instance | BindingFlags.Public);
                if (mi != null)
                {
                    var go = mi.Invoke(pm, null) as GameObject;
                    if (go != null) tr = go.transform;
                }
            }
            catch { }
            // try 2: pm.m_Player 字段 .transform
            if (tr == null)
            {
                try
                {
                    var fi = pm.GetType().GetField("m_Player", BindingFlags.Instance | BindingFlags.Public);
                    var v = fi?.GetValue(pm);
                    if (v != null)
                    {
                        var pp = v.GetType().GetProperty("transform");
                        tr = pp?.GetValue(v) as Transform;
                    }
                }
                catch { }
            }

            if (tr != null)
            {
                var p = tr.position;
                CheatState.PositionText = $"X:{p.x:F1}  Y:{p.y:F1}  Z:{p.z:F1}";
            }
        }
        catch { }
    }
}

// —————————————— Harmony patches ——————————————

[HarmonyPatch(typeof(Condition), "Update")]
internal static class Patch_Condition_Update
{
    private static void Postfix(Condition __instance)
    {
        if (!CheatState.GodMode) return;
        try { __instance.m_CurrentHP = __instance.m_MaxHP; } catch { }
    }
}

[HarmonyPatch(typeof(Fatigue), "Update")]
internal static class Patch_Fatigue_Update
{
    private static void Postfix(Fatigue __instance)
    {
        if (!(CheatState.GodMode || CheatState.NoFatigue)) return;
        try { __instance.m_CurrentFatigue = 0f; } catch { }
    }
}

[HarmonyPatch(typeof(Hunger), "Update")]
internal static class Patch_Hunger_Update
{
    private static void Postfix(Hunger __instance)
    {
        if (!(CheatState.GodMode || CheatState.NoHunger)) return;
        try { __instance.m_CurrentReserveCalories = __instance.m_MaxReserveCalories; } catch { }
    }
}

[HarmonyPatch(typeof(Hunger), "UpdateCalorieReserves")]
internal static class Patch_Hunger_UpdateCalorieReserves
{
    private static bool Prefix() => !(CheatState.GodMode || CheatState.NoHunger);
}

[HarmonyPatch(typeof(Thirst), "Update")]
internal static class Patch_Thirst_Update
{
    private static void Postfix(Thirst __instance)
    {
        if (!(CheatState.GodMode || CheatState.NoThirst)) return;
        try { __instance.m_CurrentThirst = 0f; } catch { }
    }
}

[HarmonyPatch(typeof(Freezing), "Update")]
internal static class Patch_Freezing_Update
{
    private static bool Prefix() => !(CheatState.GodMode || CheatState.AlwaysWarm);
    private static void Postfix(Freezing __instance)
    {
        if (!(CheatState.GodMode || CheatState.AlwaysWarm)) return;
        try { __instance.m_CurrentFreezing = 0f; } catch { }
    }
}

[HarmonyPatch(typeof(PlayerManager), "MaybeFlushPlayerDamage")]
internal static class Patch_PM_FlushDamage
{
    private static bool Prefix() => !CheatState.GodMode;
}

[HarmonyPatch(typeof(Hypothermia), "HypothermiaStart")]
internal static class Patch_Hypothermia_Start
{
    private static bool Prefix() => !CheatState.GodMode;
}

[HarmonyPatch(typeof(SprainedWrist), "SprainedWristStart")]
internal static class Patch_SprainedWrist_Start
{
    private static bool Prefix() => !(CheatState.GodMode || CheatState.NoSprainRisk);
}

[HarmonyPatch(typeof(SprainedAnkle), "SprainedAnkleStart")]
internal static class Patch_SprainedAnkle_Start
{
    private static bool Prefix() => !(CheatState.GodMode || CheatState.NoSprainRisk);
}

[HarmonyPatch(typeof(BloodLoss), "BloodLossStart")]
internal static class Patch_BloodLoss_Start
{
    private static bool Prefix() => !(CheatState.GodMode || CheatState.ImmuneAnimalDamage);
}

[HarmonyPatch(typeof(BloodLoss), "BloodLossStartOverrideArea")]
internal static class Patch_BloodLoss_StartOverride
{
    private static bool Prefix() => !(CheatState.GodMode || CheatState.ImmuneAnimalDamage);
}

[HarmonyPatch(typeof(Infection), "InfectionStart")]
internal static class Patch_Infection_Start
{
    private static bool Prefix() => !CheatState.GodMode;
}

[HarmonyPatch(typeof(GearItem), "ManualUpdate")]
internal static class Patch_GearItem_ManualUpdate
{
    private static void Postfix(GearItem __instance)
    {
        if (CheatState.InfiniteDurability) Cheats.RestoreDurability(__instance);
    }
}

// InfiniteDurability 兜底:拦衰减 3 个源头 Degrade / WearOut / DegradeOnUse
[HarmonyPatch(typeof(GearItem), "Degrade", new System.Type[] { typeof(float) })]
internal static class Patch_GearItem_Degrade
{
    private static bool Prefix() => !CheatState.InfiniteDurability;
}

[HarmonyPatch(typeof(GearItem), "WearOut")]
internal static class Patch_GearItem_WearOut
{
    private static bool Prefix() => !CheatState.InfiniteDurability;
}

[HarmonyPatch(typeof(GearItem), "DegradeOnUse")]
internal static class Patch_GearItem_DegradeOnUse
{
    private static bool Prefix() => !CheatState.InfiniteDurability;
}

[HarmonyPatch(typeof(GearItem), "Awake")]
internal static class Patch_GearItem_Awake
{
    private static void Postfix(GearItem __instance)
    {
        if (CheatState.InfiniteDurability) Cheats.RestoreDurability(__instance);
    }
}

// Patch_Inventory_MaybeAdd / Patch_Inventory_AddGear 已去除(InfiniteCarry 功能交给其他 mod)
