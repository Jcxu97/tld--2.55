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
    // Status / 状态
    public static bool InfiniteStamina;
    public static bool AlwaysWarm;
    public static bool NoHunger;
    public static bool NoThirst;
    public static bool NoFatigue;
    // Movement / 移动
    public static bool InfiniteCarry;
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
    public static bool InfiniteFireDurations;
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
        int n = 0;
        // 已知签名的(硬编码)
        try { GameManager.GetHypothermiaComponent()?.HypothermiaEnd(true); n++; } catch { }
        try { GameManager.GetFrostbiteComponent()?.FrostbiteEnd(); n++; } catch { }
        try { GameManager.GetCabinFeverComponent()?.CabinFeverEnd(); n++; } catch { }
        try { GameManager.GetDysenteryComponent()?.DysenteryEnd(true); n++; } catch { }
        try { GameManager.GetFoodPoisoningComponent()?.FoodPoisoningEnd(true); n++; } catch { }
        try { GameManager.GetBrokenRibComponent()?.BrokenRibEnd(0, true); n++; } catch { }
        try { GameManager.GetSprainedWristComponent()?.SprainedWristEnd(0, (AfflictionOptions)0); n++; } catch { }
        try { GameManager.GetSprainedAnkleComponent()?.SprainedAnkleEnd(0, (AfflictionOptions)0); n++; } catch { }
        try { GameManager.GetBloodLossComponent()?.BloodLossEnd(0, (AfflictionOptions)0); n++; } catch { }
        try { GameManager.GetInfectionComponent()?.InfectionEnd(0); n++; } catch { }

        // 反射扫扩展项 —— TLD 其它 affliction 组件,签名未知,用 "无参 End" 兜底
        foreach (var name in new[]
        {
            "Headache", "Scurvy", "Parasites", "Burns", "Concussion",
            "InfectionRisk", "ChemicalPoisoning", "ElectricalShock",
            "BearAttack", "WolfBite", "IntestinalParasites", "AnimalAttack",
            "Inflammation", "RashA", "RashB"
        })
        {
            try
            {
                var getter = typeof(GameManager).GetMethod($"Get{name}Component", BindingFlags.Static | BindingFlags.Public);
                var comp = getter?.Invoke(null, null);
                if (comp == null) continue;
                var endM = comp.GetType().GetMethod($"{name}End", BindingFlags.Instance | BindingFlags.Public)
                        ?? comp.GetType().GetMethod("End", BindingFlags.Instance | BindingFlags.Public)
                        ?? comp.GetType().GetMethod("Cure", BindingFlags.Instance | BindingFlags.Public);
                if (endM == null) continue;
                var ps = endM.GetParameters();
                var args = new object[ps.Length];
                for (int i = 0; i < ps.Length; i++)
                    args[i] = ps[i].ParameterType.IsValueType ? Activator.CreateInstance(ps[i].ParameterType) : null;
                endM.Invoke(comp, args);
                n++;
            }
            catch { }
        }

        ModMain.Log?.Msg($"[Cheats] Cleared {n} affliction component(s)");
        CheatState.LastActionLog = $"已清 {n} 项负面";
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
            // Clothing 的 HP 单位不是 0-100,设 100 会破坏 —— skip 让 ClothingItem 自己管
            if (gear.GetComponent<ClothingItem>() != null) return;

            // 其余 GearItem 的 m_CurrentHP 范围是 0-100,直接设 100 = 满
            if (gear.m_CurrentHP < 100f) gear.m_CurrentHP = 100f;
        }
        catch { }
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

    // 扫场景所有 BaseAi 实例(而不是所有 GameObject)。
    // 动物都是 BaseAi 派生,实例数几十个,远少于 GameObject 几千个。
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
                    TryKillGameObject(go);
                    kills++;
                    if (kills > 20) break;
                }
                catch { }
            }
            if (kills > 0) ModMain.Log?.Msg($"[Cheats.Kill] killed {kills}");
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[Cheats.Kill] {ex.Message}"); }
    }

    private static void TryKillGameObject(GameObject go)
    {
        if (go == null) return;
        try
        {
            // 1 参版本 + 无参版本,都试。SendMessageOptions.DontRequireReceiver = 1
            go.SendMessage("TakeDamage", 9999f, (SendMessageOptions)1);
            go.SendMessage("ApplyDamage", 9999f, (SendMessageOptions)1);
            go.SendMessage("Kill", (SendMessageOptions)1);
            go.SendMessage("ForceKill", (SendMessageOptions)1);
        }
        catch { }
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
        // 冲刺消耗的就是 Fatigue,所以 InfiniteStamina 也走这条路径
        if (!(CheatState.GodMode || CheatState.NoFatigue || CheatState.InfiniteStamina)) return;
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

[HarmonyPatch(typeof(Breath), "Update")]
internal static class Patch_Breath_Update
{
    private static void Postfix(Breath __instance)
    {
        if (!CheatState.InfiniteStamina) return;
        try { __instance.m_BreathTime = 1f; } catch { }
        try { __instance.m_BreathTimePercent = 1f; } catch { }  // 真正体力条源
    }
}

[HarmonyPatch(typeof(PlayerManager), "MaybeFlushPlayerDamage")]
internal static class Patch_PM_FlushDamage
{
    private static bool Prefix() => !CheatState.GodMode;
}

[HarmonyPatch(typeof(PlayerManager), "PlayerCanSprint")]
internal static class Patch_PM_CanSprint
{
    private static void Postfix(ref bool __result)
    {
        if (CheatState.InfiniteStamina) __result = true;
    }
}

[HarmonyPatch(typeof(PlayerManager), "PlayerCantSprintBecauseOfInjury")]
internal static class Patch_PM_CantSprintInjury
{
    private static void Postfix(ref bool __result)
    {
        if (CheatState.InfiniteStamina) __result = false;
    }
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
    private static bool Prefix() => !CheatState.GodMode;
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

[HarmonyPatch(typeof(GearItem), "Awake")]
internal static class Patch_GearItem_Awake
{
    private static void Postfix(GearItem __instance)
    {
        if (CheatState.InfiniteDurability) Cheats.RestoreDurability(__instance);
    }
}

[HarmonyPatch(typeof(Inventory), "MaybeAdd")]
internal static class Patch_Inventory_MaybeAdd
{
    private static void Postfix(Inventory __instance)
    {
        if (!CheatState.InfiniteCarry) return;
        try { __instance.m_ForceOverrideWeight = true; } catch { }
    }
}

[HarmonyPatch(typeof(Inventory), "AddGear")]
internal static class Patch_Inventory_AddGear
{
    private static void Postfix(Inventory __instance)
    {
        if (!CheatState.InfiniteCarry) return;
        try { __instance.m_ForceOverrideWeight = true; } catch { }
    }
}
