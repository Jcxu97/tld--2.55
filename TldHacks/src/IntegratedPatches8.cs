using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using Il2Cpp;
using Il2CppTLD.Interactions;
using MelonLoader;
using UnityEngine;

namespace TldHacks;

// ═══════════════════════════════════════════════════════════════
// TT-CapFeelsLikeTemp: 体感温度上下限
// ═══════════════════════════════════════════════════════════════

internal static class Patch_CapFeelsLikeTemp
{
    private static bool IsNearCampFire()
    {
        return GameManager.GetFireManagerComponent().GetDistanceToClosestFire(GameManager.GetPlayerTransform().position)
            < GameManager.GetBodyHarvestManagerComponent().m_RadiusToThawFromFire;
    }

    internal static void Postfix(ref float __result)
    {
        var s = ModMain.Settings;
        if ((GameManager.GetWeatherComponent() && GameManager.GetWeatherComponent().IsIndoorEnvironment()) || IsNearCampFire()) return;
        if (s.TT_CapFeelsHigh != 0 && __result > s.TT_CapFeelsHigh) __result = s.TT_CapFeelsHigh;
        if (s.TT_CapFeelsLow != 0 && __result < s.TT_CapFeelsLow) __result = s.TT_CapFeelsLow;
    }
}

// ═══════════════════════════════════════════════════════════════
// TT-DeathTriggerGoat: 坠落死亡触发器无效化 + 坠落伤害倍率
// ═══════════════════════════════════════════════════════════════

internal static class Patch_FallDeathTrigger
{
    internal static void Postfix(Collider c)
    {
        if (c != null && c.gameObject.CompareTag("Player"))
        {
            GameManager.GetFallDamageComponent().m_DieOnNextFall = false;
        }
    }
}

internal static class Patch_FallDamageMultiplier
{
    internal static void Postfix()
    {
        GameManager.GetFallDamageComponent().m_DamagePerMeter = ModMain.Settings.TT_FallDamageMult;
    }
}

// ═══════════════════════════════════════════════════════════════
// TT-DroppedObjectOrientation: 步枪落地竖立
// ═══════════════════════════════════════════════════════════════

internal static class Patch_DroppedObjectOrientation
{
    internal static void Postfix()
    {
        try
        {
            string[] rifles = { "GEAR_Rifle", "GEAR_Rifle_Barbs", "GEAR_Rifle_Curators", "GEAR_Rifle_Vaughns", "GEAR_Rifle_Trader" };
            foreach (var name in rifles)
            {
                var gi = GearItem.LoadGearItemPrefab(name);
                if (gi == null) continue;
                var posDummy = gi.gameObject.transform.FindChild("DropDummy");
                if (posDummy != null) posDummy.eulerAngles = new Vector3(0f, 0f, 270f);
            }
            var matches = GearItem.LoadGearItemPrefab("GEAR_WoodMatches");
            if (matches != null)
            {
                var lod0 = matches.gameObject.transform.FindChild("OBJ_WoodMatches_LOD0");
                var lod1 = matches.gameObject.transform.FindChild("OBJ_WoodMatches_LOD1");
                if (lod0 != null)
                {
                    matches.GetComponent<Inspect>().m_Angles = new Vector3(-50f, 20f, 0f);
                    lod0.localEulerAngles = new Vector3(0f, 180f, 0f);
                    lod0.localPosition = Vector3.zero;
                }
                if (lod1 != null)
                {
                    lod1.localEulerAngles = new Vector3(0f, 180f, 0f);
                    lod1.localPosition = Vector3.zero;
                }
            }
        }
        catch { }
    }
}

// ═══════════════════════════════════════════════════════════════
// TT-ExtendedFOVSlider: FOV 滑块范围扩展 30-150
// ═══════════════════════════════════════════════════════════════

internal static class Patch_ExtendedFOV_Apply
{
    internal static void Postfix(Panel_OptionsMenu __instance)
    {
        __instance.m_FieldOfViewMax = 150f;
        __instance.m_FieldOfViewMin = 30f;
        __instance.m_FieldOfViewSlider.m_Slider.numberOfSteps = 121;
    }
}

internal static class Patch_ExtendedFOV_Display
{
    internal static void Postfix(Panel_OptionsMenu __instance)
    {
        __instance.m_FieldOfViewMax = 150f;
        __instance.m_FieldOfViewMin = 30f;
        __instance.m_FieldOfViewSlider.m_Slider.numberOfSteps = 121;
    }
}

// ═══════════════════════════════════════════════════════════════
// TT-PauseOnRadial: 开辐射轮时暂停
// ═══════════════════════════════════════════════════════════════

internal static class PauseOnRadialState
{
    public static bool IsSlowed;
    public static object Coroutine;

    public static IEnumerator LerpTimescale(float value, float speed)
    {
        float current = Time.timeScale;
        float lerp = 0f;
        while (!Mathf.Approximately(value, Time.timeScale))
        {
            Time.timeScale = GameManager.m_GlobalTimeScale = Mathf.Lerp(current, value, lerp += Time.unscaledDeltaTime * speed);
            yield return new WaitForEndOfFrame();
        }
    }
}

internal static class Patch_PauseOnRadial_Enable
{
    internal static void Prefix(bool enable)
    {
        if (!CheatState.TT_PauseOnRadial) return;
        if (enable && !PauseOnRadialState.IsSlowed)
        {
            if (PauseOnRadialState.Coroutine != null) MelonCoroutines.Stop(PauseOnRadialState.Coroutine);
            PauseOnRadialState.Coroutine = MelonCoroutines.Start(PauseOnRadialState.LerpTimescale(0.1f, 1f));
            PauseOnRadialState.IsSlowed = true;
        }
        if (!enable && PauseOnRadialState.IsSlowed)
        {
            if (PauseOnRadialState.Coroutine != null) MelonCoroutines.Stop(PauseOnRadialState.Coroutine);
            PauseOnRadialState.Coroutine = MelonCoroutines.Start(PauseOnRadialState.LerpTimescale(1f, 3f));
            PauseOnRadialState.IsSlowed = false;
        }
    }
}

internal static class Patch_PauseOnRadial_FoolProof
{
    internal static void Postfix()
    {
        if (!CheatState.TT_PauseOnRadial) return;
        if (!PauseOnRadialState.IsSlowed) return;
        if (!InterfaceManager.GetPanel<Panel_ActionsRadial>().IsEnabled())
        {
            MelonCoroutines.Start(PauseOnRadialState.LerpTimescale(1f, 3f));
            PauseOnRadialState.IsSlowed = false;
        }
    }
}

// ═══════════════════════════════════════════════════════════════
// TT-SpeedyInteractions: 交互加速
// ═══════════════════════════════════════════════════════════════

internal static class Patch_Speedy_Eating
{
    internal static void Prefix(ref GearItem gi)
    {
        float m = ModMain.Settings.TT_EatingSpeedMult;
        if (m != 1f) { gi.m_FoodItem.m_TimeToEatSeconds /= m; gi.m_FoodItem.m_TimeToOpenAndEatSeconds /= m; }
    }
    internal static void Postfix(ref GearItem gi)
    {
        float m = ModMain.Settings.TT_EatingSpeedMult;
        if (m != 1f) { gi.m_FoodItem.m_TimeToEatSeconds *= m; gi.m_FoodItem.m_TimeToOpenAndEatSeconds *= m; }
    }
}

internal static class Patch_Speedy_Smash
{
    [ThreadStatic] static int _saved;
    internal static void Prefix(ref GearItem gi)
    {
        float m = ModMain.Settings.TT_EatingSpeedMult;
        _saved = gi.m_SmashableItem.m_TimeToSmash;
        if (m != 1f) gi.m_SmashableItem.m_TimeToSmash = Mathf.FloorToInt(_saved / m);
    }
    internal static void Postfix(ref GearItem gi)
    {
        gi.m_SmashableItem.m_TimeToSmash = _saved;
    }
}

internal static class Patch_Speedy_Drink
{
    internal static void Prefix(ref WaterSupply ws)
    {
        float m = ModMain.Settings.TT_EatingSpeedMult;
        if (m != 1f) ws.m_TimeToDrinkSeconds /= m;
    }
    internal static void Postfix(ref WaterSupply ws)
    {
        float m = ModMain.Settings.TT_EatingSpeedMult;
        if (m != 1f) ws.m_TimeToDrinkSeconds *= m;
    }
}

internal static class Patch_Speedy_Refuel
{
    internal static void Prefix(Panel_Inventory_Examine __instance)
    {
        float m = ModMain.Settings.TT_GlobalSpeedMult;
        if (m != 1f) { var kli = __instance.m_GearItem.GetComponent<Il2CppTLD.Gear.KeroseneLampItem>(); if (kli) kli.m_RefuelTimeSeconds /= m; }
    }
    internal static void Postfix(Panel_Inventory_Examine __instance)
    {
        float m = ModMain.Settings.TT_GlobalSpeedMult;
        if (m != 1f) { var kli = __instance.m_GearItem.GetComponent<Il2CppTLD.Gear.KeroseneLampItem>(); if (kli) kli.m_RefuelTimeSeconds *= m; }
    }
}

internal static class Patch_Speedy_Purify
{
    internal static void Prefix(ref GearItem gi)
    {
        float m = ModMain.Settings.TT_GlobalSpeedMult;
        if (m != 1f) gi.m_PurifyWater.m_ProgressBarDurationSeconds /= m;
    }
    internal static void Postfix(ref GearItem gi)
    {
        float m = ModMain.Settings.TT_GlobalSpeedMult;
        if (m != 1f) gi.m_PurifyWater.m_ProgressBarDurationSeconds *= m;
    }
}

internal static class Patch_Speedy_Examine
{
    internal static void Prefix(Panel_Inventory_Examine __instance)
    {
        if (__instance.IsResearchItem())
        {
            float m = ModMain.Settings.TT_ReadingSpeedMult;
            if (m != 1f) __instance.m_ProgressBarTimeSeconds /= m;
        }
        else
        {
            float m = ModMain.Settings.TT_GlobalSpeedMult;
            if (m != 1f) __instance.m_ProgressBarTimeSeconds /= m;
        }
    }
}

internal static class Patch_Speedy_RockCacheBuild
{
    internal static void Prefix(RockCache __instance)
    {
        float m = ModMain.Settings.TT_GlobalSpeedMult;
        if (m != 1f) __instance.m_BuildRealSecondsElapsed = Mathf.CeilToInt(2f / m);
    }
}

internal static class Patch_Speedy_RockCacheDismantle
{
    internal static void Prefix(RockCache __instance)
    {
        float m = ModMain.Settings.TT_GlobalSpeedMult;
        if (m != 1f) __instance.m_DismantleRealSecondsElapsed = Mathf.CeilToInt(2f / m);
    }
}

internal static class Patch_Speedy_Breakdown
{
    internal static void Prefix(Panel_BreakDown __instance)
    {
        float m = ModMain.Settings.TT_BreakdownSpeedMult;
        if (m != 1f) __instance.m_SecondsToBreakDown = 3f / m;
    }
}

internal static class Patch_Speedy_Crafting
{
    internal static void Prefix(Panel_Crafting __instance)
    {
        float m = ModMain.Settings.TT_GlobalSpeedMult;
        if (m != 1f) __instance.m_CraftingDisplayTimeSeconds = 5f / m;
    }
}

internal static class Patch_Speedy_Cook1
{
    internal static void Prefix(Panel_Cooking __instance)
    {
        float m = ModMain.Settings.TT_GlobalSpeedMult;
        if (m != 1f) __instance.m_RecipePreparationDisplayTimeSeconds = 5f / m;
    }
}

internal static class Patch_Speedy_Cook2
{
    internal static void Prefix(Panel_Cooking __instance)
    {
        float m = ModMain.Settings.TT_GlobalSpeedMult;
        if (m != 1f) __instance.m_RecipePreparationDisplayTimeSeconds = 5f / m;
    }
}

internal static class Patch_Speedy_Milling
{
    internal static void Prefix(Panel_Milling __instance)
    {
        float m = ModMain.Settings.TT_GlobalSpeedMult;
        if (m != 1f) __instance.m_RepairRealTimeSeconds = 5f / m;
    }
}

internal static class Patch_Speedy_SnowShelterBuild
{
    internal static void Prefix(Panel_SnowShelterBuild __instance)
    {
        float m = ModMain.Settings.TT_GlobalSpeedMult;
        if (m != 1f) __instance.m_RealtimeSecondsToBuild = 3f / m;
    }
}

internal static class Patch_Speedy_SnowShelterInteract
{
    internal static void Prefix(Panel_SnowShelterInteract __instance)
    {
        float m = ModMain.Settings.TT_GlobalSpeedMult;
        if (m != 1f) __instance.m_RealtimeSecondsToRepairOrDismantle = 3f / m;
    }
}

internal static class Patch_Speedy_PickWater
{
    internal static void Prefix(Panel_PickWater __instance)
    {
        float m = ModMain.Settings.TT_GlobalSpeedMult;
        if (m != 1f) __instance.m_ProgressBarDurationSecondsBase = 2f / m;
    }
}

internal static class Patch_Speedy_IceFishing
{
    internal static void Prefix(Panel_IceFishingHoleClear __instance)
    {
        float m = ModMain.Settings.TT_GlobalSpeedMult;
        if (m != 1f) __instance.m_ProgressBarSeconds = 10f / m;
    }
}

internal static class Patch_Speedy_Affliction1
{
    internal static void Postfix(ref float __result)
    {
        float m = ModMain.Settings.TT_GlobalSpeedMult;
        if (m != 1f) __result /= m;
    }
}

internal static class Patch_Speedy_Affliction2
{
    internal static void Postfix(ref float __result)
    {
        float m = ModMain.Settings.TT_GlobalSpeedMult;
        if (m != 1f) __result /= m;
    }
}

internal static class Patch_Speedy_HoldInteraction
{
    public static Dictionary<TimedHoldInteraction, (float baseValue, float lastValue, float lastModifier)> Data = new();

    internal static void Prefix(TimedHoldInteraction __instance)
    {
        if (CheatState.QuickSearch) { __instance.HoldTime = 0.01f; return; }
        float m = ModMain.Settings.TT_InteractionSpeedMult;
        if (m == 1f)
        {
            if (Data.TryGetValue(__instance, out var restore))
            {
                __instance.HoldTime = restore.baseValue;
                Data.Remove(__instance);
            }
            return;
        }
        float currentValue = __instance.HoldTime;
        if (!Data.ContainsKey(__instance))
        {
            __instance.HoldTime = currentValue / m;
            Data.Add(__instance, (currentValue, __instance.HoldTime, m));
            return;
        }
        var entry = Data[__instance];
        if (Math.Abs(currentValue - entry.lastValue) > 0.0001f)
        {
            __instance.HoldTime /= m;
            entry.baseValue = currentValue;
        }
        else if (entry.lastModifier != m)
        {
            __instance.HoldTime = entry.baseValue / m;
        }
        entry.lastValue = __instance.HoldTime;
        entry.lastModifier = m;
        Data[__instance] = entry;
    }
}

internal static class Patch_Speedy_ResetDict
{
    internal static void Postfix()
    {
        Patch_Speedy_HoldInteraction.Data.Clear();
    }
}

// ═══════════════════════════════════════════════════════════════
// TT-RespawnablePlants: 植物重生
// ═══════════════════════════════════════════════════════════════

internal class RegrowSaveData
{
    public Dictionary<string, Dictionary<string, float>> dictionarySaveProxy { get; set; }
}

internal static class RespawnablePlantsState
{
    public static Dictionary<string, Dictionary<string, float>> HarvestedPlants = new();
    public static Dictionary<string, float> RetroactivePending = new();
    private static string _lastScene = "";
    public static object Routine;

    public static readonly ModData.ModDataManager DataManager = new("TldHacks");

    public static IEnumerator CheckRespawn()
    {
        while (true)
        {
            for (float t = 0f; t < 30f; t += Time.deltaTime)
            {
                yield return new WaitForEndOfFrame();
            }
            if (string.IsNullOrEmpty(GameManager.m_ActiveScene)) continue;
            if (HarvestedPlants == null || HarvestedPlants.Count < 1) continue;
            string scene = GameManager.m_ActiveScene;
            if (!HarvestedPlants.ContainsKey(scene)) continue;
            var toRemove = new List<string>();
            foreach (var entry in HarvestedPlants[scene])
            {
                float hours = ModMain.Settings.TT_PlantRespawnDays * 24f;
                if (entry.Value + hours < GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused())
                {
                    var h = HarvestableManager.FindHarvestableByGuid(entry.Key);
                    if (h != null) { h.gameObject.SetActive(true); h.m_Harvested = false; }
                    toRemove.Add(entry.Key);
                }
            }
            foreach (var k in toRemove) HarvestedPlants[scene].Remove(k);
        }
    }

    public static void AddRetroactive()
    {
        if (RetroactivePending.Count == 0) return;
        string scene = GameManager.m_ActiveScene;
        if (!HarvestedPlants.ContainsKey(scene)) HarvestedPlants[scene] = new();
        foreach (var entry in RetroactivePending)
            if (!HarvestedPlants[scene].ContainsKey(entry.Key))
                HarvestedPlants[scene][entry.Key] = entry.Value;
        RetroactivePending.Clear();
    }

    public static void OnSceneLoaded() => RetroactivePending.Clear();
    public static void UpdateLastScene(string scene) { if (_lastScene != scene) { _lastScene = scene; RetroactivePending.Clear(); } }
}

internal static class Patch_RespawnPlants_Awake
{
    internal static void Prefix(ref Harvestable __instance)
    {
        if (__instance == null || !__instance.RegisterAsPlantsHaversted) return;
        __instance.m_DestroyObjectOnHarvest = false;
    }
}

internal static class Patch_RespawnPlants_Harvest
{
    internal static void Postfix(ref Harvestable __instance)
    {
        if (__instance.m_Harvested && __instance.RegisterAsPlantsHaversted)
        {
            string scene = GameManager.m_ActiveScene;
            if (!RespawnablePlantsState.HarvestedPlants.ContainsKey(scene))
                RespawnablePlantsState.HarvestedPlants[scene] = new();
            string guid = ObjectGuid.GetGuidFromGameObject(__instance.gameObject);
            RespawnablePlantsState.HarvestedPlants[scene][guid] = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused();
        }
    }
}

internal static class Patch_RespawnPlants_Deserialize
{
    [ThreadStatic] static bool _hadData;
    internal static void Prefix(ref Harvestable __instance, string text)
    {
        _hadData = !string.IsNullOrEmpty(text);
    }
    internal static void Postfix(ref Harvestable __instance)
    {
        RespawnablePlantsState.UpdateLastScene(GameManager.m_ActiveScene);
        if (__instance.m_Harvested && __instance.RegisterAsPlantsHaversted && _hadData)
        {
            string guid = ObjectGuid.GetGuidFromGameObject(__instance.gameObject);
            RespawnablePlantsState.RetroactivePending[guid] = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused();
        }
    }
}

internal static class Patch_RespawnPlants_Save
{
    internal static void Postfix()
    {
        try
        {
            var data = new RegrowSaveData { dictionarySaveProxy = RespawnablePlantsState.HarvestedPlants };
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { IncludeFields = true });
            RespawnablePlantsState.DataManager.Save(json, "regrowPlants");
        }
        catch { }
    }
}

internal static class Patch_RespawnPlants_Load
{
    internal static void Postfix()
    {
        try
        {
            string json = RespawnablePlantsState.DataManager.Load("regrowPlants");
            if (!string.IsNullOrEmpty(json))
            {
                var data = JsonSerializer.Deserialize<RegrowSaveData>(json, new JsonSerializerOptions { IncludeFields = true });
                if (data?.dictionarySaveProxy != null)
                    RespawnablePlantsState.HarvestedPlants = data.dictionarySaveProxy;
            }
            if (RespawnablePlantsState.Routine != null) MelonCoroutines.Stop(RespawnablePlantsState.Routine);
            RespawnablePlantsState.Routine = MelonCoroutines.Start(RespawnablePlantsState.CheckRespawn());
            RespawnablePlantsState.AddRetroactive();
        }
        catch { }
    }
}

// ═══════════════════════════════════════════════════════════════
// ShowTraderTrust: 商人信任度 HUD 显示
// ═══════════════════════════════════════════════════════════════

internal static class ShowTraderTrustHelper
{
    private static Il2CppTLD.Trader.TraderRadio _radio;
    private static string _text;
    private static bool _visible;
    private static bool _searchFailed;
    private static int _lastTrust = -1, _lastMax = -1;

    public static void Tick()
    {
        if (!CheatState.TT_ShowTraderTrust) { _visible = false; return; }
        try
        {
            if (!GameManager.GetWeatherComponent().IsIndoorEnvironment()) { _visible = false; return; }
            if (_radio == null && !_searchFailed)
            {
                _radio = UnityEngine.Object.FindObjectOfType<Il2CppTLD.Trader.TraderRadio>();
                if (_radio == null) { _searchFailed = true; _visible = false; return; }
            }
            if (_radio == null) { _visible = false; return; }

            int state = (int)_radio.m_CurrentState;
            if (state >= 1 && state <= 5)
            {
                var tm = GameManager.GetTraderManager();
                if (tm == null) { _visible = false; return; }
                int cur = tm.CurrentTrust, max = tm.MaxTrust;
                if (cur != _lastTrust || max != _lastMax)
                {
                    _lastTrust = cur; _lastMax = max;
                    _text = I18n.T($"信任度: {cur} / {max}", $"Trust: {cur} / {max}");
                }
                _visible = true;
            }
            else
            {
                _visible = false;
            }
        }
        catch { _visible = false; }
    }

    private static GUIStyle _style;

    public static void DrawGUI()
    {
        if (!_visible || string.IsNullOrEmpty(_text)) return;
        if (_style == null)
        {
            _style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                alignment = TextAnchor.LowerCenter,
                normal = { textColor = Color.white }
            };
        }
        GUI.Label(new Rect(Screen.width / 2f - 150f, Screen.height - 60f, 300f, 40f), _text, _style);
    }

    public static void OnSceneChange() { _radio = null; _visible = false; _searchFailed = false; }
}
