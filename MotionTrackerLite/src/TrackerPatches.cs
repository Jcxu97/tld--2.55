using System;
using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace MotionTrackerLite;

internal static class TrackerPatches
{
    private static HarmonyLib.Harmony _harmony;
    private static bool _patched;
    private static int _gearThrottle;

    public static void PatchAll()
    {
        if (_patched) return;
        _harmony ??= new HarmonyLib.Harmony("com.user.MotionTrackerLite");
        try
        {
            var self = typeof(TrackerPatches);
            var post = BindingFlags.Static | BindingFlags.NonPublic;

            _harmony.Patch(
                AccessTools.Method(typeof(BaseAi), "Start"),
                postfix: new HarmonyMethod(self.GetMethod(nameof(BaseAi_Start_Post), post)));

            _harmony.Patch(
                AccessTools.Method(typeof(BaseAi), "EnterDead"),
                prefix: new HarmonyMethod(self.GetMethod(nameof(BaseAi_EnterDead_Pre), post)));

            _harmony.Patch(
                AccessTools.Method(typeof(GearItem), "ManualUpdate"),
                postfix: new HarmonyMethod(self.GetMethod(nameof(GearItem_ManualUpdate_Post), post)));

            var harvestStart = AccessTools.Method(typeof(Harvestable), "Start");
            if (harvestStart != null)
                _harmony.Patch(harvestStart, postfix: new HarmonyMethod(self.GetMethod(nameof(Harvestable_Start_Post), post)));

            var containerStart = AccessTools.Method(typeof(Container), "Start");
            if (containerStart != null)
                _harmony.Patch(containerStart, postfix: new HarmonyMethod(self.GetMethod(nameof(Container_Start_Post), post)));

            var decalMethod = AccessTools.Method(typeof(DynamicDecalsManager), "TrySpawnDecalObject",
                new[] { typeof(DecalProjectorInstance) });
            if (decalMethod != null)
                _harmony.Patch(decalMethod, postfix: new HarmonyMethod(self.GetMethod(nameof(Decal_TrySpawn_Post), post)));

            var beachType = AccessTools.TypeByName("Il2Cpp.BeachcombingSpawner");
            if (beachType != null)
            {
                var beachStart = AccessTools.Method(beachType, "Start");
                if (beachStart != null)
                    _harmony.Patch(beachStart, postfix: new HarmonyMethod(self.GetMethod(nameof(Beachcombing_Start_Post), post)));
            }

            _patched = true;
            ModMain.Log?.Msg("[Patches] 7 patches applied");
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[PatchAll] {ex.Message}"); }
    }

    public static void UnpatchAll()
    {
        if (!_patched || _harmony == null) return;
        try
        {
            _harmony.UnpatchSelf();
            _patched = false;
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[UnpatchAll] {ex.Message}"); }
    }

    internal static void RegisterAnimal(BaseAi ai)
    {
        if (ai == null) return;
        var go = ((Component)ai).gameObject;
        if (go == null) return;
        int id = go.GetInstanceID();

        int subType = (int)ai.m_AiSubType;
        string name = ((UnityEngine.Object)go).name ?? "";

        AnimalKind kind;
        if (subType == 5) kind = AnimalKind.Moose;
        else if (subType == 2) kind = AnimalKind.Bear;
        else if (subType == 6) kind = AnimalKind.Cougar;
        else if (subType == 1 && name.ToLower().Contains("grey")) kind = AnimalKind.Timberwolf;
        else if (subType == 1) kind = AnimalKind.Wolf;
        else if (subType == 3 && name.Contains("_Doe")) kind = AnimalKind.Doe;
        else if (subType == 3) kind = AnimalKind.Stag;
        else if (subType == 4) kind = AnimalKind.Rabbit;
        else if ((int)ai.m_SnowImprintType == 8) kind = AnimalKind.Ptarmigan;
        else return;

        Tracker.Register(id, go.transform, EntityCategory.Animal, kind);
    }

    private static void BaseAi_Start_Post(BaseAi __instance)
    {
        try
        {
            if ((int)__instance.m_CurrentMode == 2) return;
            RegisterAnimal(__instance);
        }
        catch { }
    }

    private static void BaseAi_EnterDead_Pre(BaseAi __instance)
    {
        try
        {
            var go = ((Component)__instance).gameObject;
            if (go != null) Tracker.Unregister(go.GetInstanceID());
        }
        catch { }
    }

    private static void GearItem_ManualUpdate_Post(GearItem __instance)
    {
        try
        {
            if (++_gearThrottle % 30 != 0) return;

            if (__instance == null) return;
            var go = ((Component)__instance).gameObject;
            if (go == null) return;
            int id = go.GetInstanceID();

            bool inContainer = __instance.m_InsideContainer;
            bool inInventory = __instance.m_InPlayerInventory;

            if (inContainer || inInventory)
            {
                Tracker.Unregister(id);
                return;
            }

            if (Tracker.Entities.ContainsKey(id)) return;

            string name = ((UnityEngine.Object)go).name ?? "";
            AnimalKind gearKind;
            if (name.Contains("Arrow", StringComparison.OrdinalIgnoreCase))
                gearKind = AnimalKind.Wolf;
            else if (name.Contains("Coal", StringComparison.OrdinalIgnoreCase))
                gearKind = AnimalKind.Timberwolf;
            else if (IsRawFish(__instance))
                gearKind = AnimalKind.Bear;
            else
                return;

            Tracker.Register(id, go.transform, EntityCategory.Gear, gearKind);
        }
        catch { }
    }

    private static void Harvestable_Start_Post(Harvestable __instance)
    {
        try
        {
            var go = ((Component)__instance).gameObject;
            if (go == null) return;
            string name = ((UnityEngine.Object)go).name ?? "";
            if (!name.Contains("SaltDeposit", StringComparison.OrdinalIgnoreCase)) return;
            Tracker.Register(go.GetInstanceID(), go.transform, EntityCategory.Structure, AnimalKind.Timberwolf);
        }
        catch { }
    }

    private static void Container_Start_Post(Container __instance)
    {
        try
        {
            var go = ((Component)__instance).gameObject;
            if (go == null) return;
            string name = ((UnityEngine.Object)go).name ?? "";
            if (!name.Contains("InaccessibleGear", StringComparison.OrdinalIgnoreCase)) return;
            Tracker.Register(go.GetInstanceID(), go.transform, EntityCategory.Structure, AnimalKind.Wolf);
        }
        catch { }
    }

    private static void Decal_TrySpawn_Post(DynamicDecalsManager __instance, DecalProjectorInstance decalInstance)
    {
        try
        {
            if ((int)decalInstance.m_DecalProjectorType != 7) return;

            __instance.CalculateDecalTransform(decalInstance, null, out Vector3 position, out _, out _);

            var go = new GameObject("MTL_Decal");
            go.transform.position = position;
            Tracker.SprayGOs.Add(go);
            Tracker.Register(go.GetInstanceID(), go.transform, EntityCategory.Spraypaint);
        }
        catch { }
    }

    private static void Beachcombing_Start_Post(Component __instance)
    {
        try
        {
            var go = __instance.gameObject;
            if (go == null) return;
            Tracker.Register(go.GetInstanceID(), go.transform, EntityCategory.Structure, AnimalKind.Bear);
        }
        catch { }
    }

    private static bool IsRawFish(GearItem gi)
    {
        var food = ((Component)gi).GetComponent<FoodItem>();
        return food != null && food.m_IsFish && food.m_IsRawMeat;
    }
}
