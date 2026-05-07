using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Il2Cpp;
using Il2CppAK;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppTLD.IntBackedUnit;
using Il2CppTLD.Interactions;
using UnityEngine;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace TldHacks;

// ═══════════════════════════════════════════════════════════════
// CarcassMoving: 搬运猎物尸骸(鹿/狼)跨场景
// ═══════════════════════════════════════════════════════════════

[Il2CppInterop.Runtime.Attributes.Il2CppImplements(typeof(MonoBehaviour))]
internal class CarcassMovingBehaviour : MonoBehaviour
{
    static CarcassMovingBehaviour() => ClassInjector.RegisterTypeInIl2Cpp<CarcassMovingBehaviour>();
    public CarcassMovingBehaviour(IntPtr ptr) : base(ptr) { }

    void Update()
    {
        if (CarcassMovingState.SaveTrigger)
        {
            CarcassMovingState.SaveTrigger = false;
            try { GearManager.UpdateAll(); GameManager.TriggerSurvivalSaveAndDisplayHUDMessage(); } catch { }
        }
        if (GameManager.m_IsPaused || !CarcassMovingState.IsCarrying) return;
        if (!CheatState.World_CarcassMoving) { CarcassMovingState.Drop(); return; }
        CarcassMovingState.DisplayDropPopup();
        if (InputManager.GetAltFirePressed((MonoBehaviour)(object)this) ||
            CarcassMovingState.HasInjuryPreventing() ||
            GameManager.GetPlayerStruggleComponent().InStruggle())
            CarcassMovingState.Drop();
    }
}

internal static class CarcassMovingState
{
    internal static GameObject MoveBtnObj;
    internal static GameObject CarcassObj;
    internal static BodyHarvest Harvest;
    internal static string OriginalScene;
    internal static float Weight;
    internal static bool IsCarrying;
    internal static bool SaveTrigger;

    internal static bool IsMovable(BodyHarvest bh)
    {
        var n = ((UnityEngine.Object)bh).name;
        return (n.Contains("Doe") || n.Contains("Stag") || n.Contains("Deer") || n.Contains("Wolf")) && !n.Contains("Quarter");
    }

    internal static bool HasInjuryPreventing() =>
        GameManager.GetSprainedAnkleComponent().HasSprainedAnkle() ||
        GameManager.GetSprainedWristComponent().HasSprainedWrist() ||
        GameManager.GetBrokenRibComponent().HasBrokenRib();

    internal static void PickUp()
    {
        IsCarrying = true;
        ItemWeight mw = Harvest.m_MeatAvailableKG;
        ItemWeight gw = Harvest.GetGutsAvailableWeightKg();
        ItemWeight hw = Harvest.GetHideAvailableWeightKg();
        Weight = ((ItemWeight)mw).ToQuantity(1f) + ((ItemWeight)gw).ToQuantity(1f) + ((ItemWeight)hw).ToQuantity(1f);
        OriginalScene = GameManager.m_ActiveScene;
        if (CarcassObj.GetComponent<CarcassMovingBehaviour>() == null)
            CarcassObj.AddComponent<CarcassMovingBehaviour>();
        GameManager.GetPlayerManagerComponent().UnequipItemInHands();
        CarcassObj.transform.localScale = Vector3.zero;
        GameAudioManager.PlaySound("Play_RopeGetOn", InterfaceManager.GetSoundEmitter());
        GameAudioManager.PlaySound(EVENTS.PLAY_EXERTIONLOW, InterfaceManager.GetSoundEmitter());
    }

    internal static void Drop()
    {
        IsCarrying = false;
        MoveToPlayer();
        CarcassObj.transform.localScale = Vector3.one;
        if (GameManager.m_ActiveScene != OriginalScene)
            AddToSceneSaveData();
        GameAudioManager.PlaySound(EVENTS.PLAY_BODYFALLLARGE, InterfaceManager.GetSoundEmitter());
        foreach (var r in CarcassObj.GetComponentsInChildren<SkinnedMeshRenderer>())
            ((Renderer)r).enabled = true;
        Harvest = null;
        CarcassObj = null;
    }

    internal static void MoveToPlayer()
    {
        CarcassObj.transform.position = GameManager.GetPlayerTransform().position;
        CarcassObj.transform.rotation = GameManager.GetPlayerTransform().rotation * Quaternion.Euler(0f, 90f, 0f);
    }

    internal static void AddToSceneSaveData()
    {
        BodyHarvestManager.AddBodyHarvest(Harvest);
        SceneManager.MoveGameObjectToScene(((Component)CarcassObj.transform.root).gameObject, SceneManager.GetActiveScene());
    }

    internal static void DisplayDropPopup()
    {
        InterfaceManager.GetPanel<Panel_HUD>().m_EquipItemPopup.ShowGenericPopupWithDefaultActions(string.Empty, "放下猎物");
    }

    internal static void AddMoveButton(Panel_BodyHarvest panel)
    {
        MoveBtnObj = UnityEngine.Object.Instantiate(panel.m_Mouse_Button_Harvest, panel.m_Mouse_Button_Harvest.transform);
        MoveBtnObj.GetComponentInChildren<UILocalize>().key = "搬运猎物";
        panel.m_Mouse_Button_Harvest.transform.localPosition += new Vector3(-100f, 0f, 0f);
        MoveBtnObj.transform.localPosition = new Vector3(200f, 0f, 0f);
        var btn = MoveBtnObj.GetComponentInChildren<UIButton>();
        btn.onClick.Clear();
        EventDelegate.Callback cb = new System.Action(OnMoveClicked);
        btn.onClick.Add(new EventDelegate(cb));
    }

    internal static void RemoveMoveButton(Panel_BodyHarvest panel)
    {
        UnityEngine.Object.DestroyImmediate(MoveBtnObj);
        panel.m_Mouse_Button_Harvest.transform.localPosition += new Vector3(100f, 0f, 0f);
    }

    internal static void OnMoveClicked()
    {
        if (HasInjuryPreventing())
        {
            GameAudioManager.PlayGUIError();
            HUDMessage.AddMessage("受伤时无法搬运猎物", false, false);
            return;
        }
        PickUp();
        InterfaceManager.GetPanel<Panel_BodyHarvest>().OnBack();
    }
}

// --- CarcassMoving Patches ---

internal static class Patch_Carcass_CanEnable
{
    internal static void Postfix(BodyHarvest bodyHarvest, ref bool __result)
    {
        if (!CheatState.World_CarcassMoving) return;
        if (bodyHarvest.GetCondition() >= 0.5f) __result = true;
    }
}

internal static class Patch_Carcass_PanelEnable
{
    internal static void Postfix(Panel_BodyHarvest __instance, BodyHarvest bh, bool enable)
    {
        if (!CheatState.World_CarcassMoving || !enable || !__instance.CanEnable(bh)) return;
        try
        {
            if (CarcassMovingState.IsCarrying) CarcassMovingState.Drop();
            if (CarcassMovingState.IsMovable(bh) && !CarcassMovingState.IsCarrying)
            {
                CarcassMovingState.Harvest = bh;
                CarcassMovingState.CarcassObj = ((Component)bh).gameObject;
                if (CarcassMovingState.MoveBtnObj == null) CarcassMovingState.AddMoveButton(__instance);
            }
            else if (CarcassMovingState.MoveBtnObj != null)
                CarcassMovingState.RemoveMoveButton(__instance);
        }
        catch { }
    }
}

internal static class Patch_Carcass_SceneLoaded
{
    internal static void Postfix()
    {
        if (!CheatState.World_CarcassMoving || !CarcassMovingState.IsCarrying) return;
        try
        {
            if (CarcassMovingState.CarcassObj == null) { CarcassMovingState.IsCarrying = false; return; }
            if (CarcassMovingState.Harvest != null) ((Behaviour)CarcassMovingState.Harvest).enabled = true;
            CarcassMovingState.SaveTrigger = true;
        }
        catch { }
    }
}

internal static class Patch_Carcass_LoadScene
{
    internal static void Postfix()
    {
        if (!CheatState.World_CarcassMoving || !CarcassMovingState.IsCarrying) return;
        if (CarcassMovingState.CarcassObj == null) return;
        try
        {
            UnityEngine.Object.DontDestroyOnLoad(((Component)CarcassMovingState.CarcassObj.transform.root).gameObject);
            ((Behaviour)CarcassMovingState.Harvest).enabled = false;
        }
        catch { }
    }
}

internal static class Patch_Carcass_BeforeSave
{
    internal static void Prefix()
    {
        if (!CheatState.World_CarcassMoving || !CarcassMovingState.IsCarrying || CarcassMovingState.Harvest == null) return;
        try
        {
            ((Behaviour)CarcassMovingState.Harvest).enabled = true;
            CarcassMovingState.MoveToPlayer();
            CarcassMovingState.AddToSceneSaveData();
        }
        catch { }
    }
}

internal static class Patch_Carcass_NoSprint
{
    internal static void Postfix(ref bool __result)
    {
        if (CheatState.World_CarcassMoving && CarcassMovingState.IsCarrying) __result = false;
    }
}

internal static class Patch_Carcass_NoEquip
{
    internal static bool Prefix()
    {
        if (!CheatState.World_CarcassMoving || !CarcassMovingState.IsCarrying) return true;
        GameAudioManager.PlaySound(GameAudioManager.Instance.m_ErrorAudio, ((Component)GameAudioManager.Instance).gameObject);
        HUDMessage.AddMessage("搬运猎物时无法装备物品", false, false);
        return false;
    }
}

internal static class Patch_Carcass_Fatigue
{
    internal static void Postfix(ref float __result)
    {
        if (CheatState.World_CarcassMoving && CarcassMovingState.IsCarrying)
            __result *= 1f + CarcassMovingState.Weight * 0.05f;
    }
}

internal static class Patch_Carcass_Calorie
{
    internal static void Postfix(ref float __result)
    {
        if (CheatState.World_CarcassMoving && CarcassMovingState.IsCarrying)
            __result += CarcassMovingState.Weight * 15f;
    }
}

internal static class Patch_Carcass_Scent
{
    internal static void Postfix(ref float __result)
    {
        if (CheatState.World_CarcassMoving && CarcassMovingState.IsCarrying)
            __result += 33f;
    }
}

internal static class Patch_Carcass_RopeDrop
{
    internal static void Postfix()
    {
        if (CheatState.World_CarcassMoving && CarcassMovingState.IsCarrying)
            CarcassMovingState.Drop();
    }
}

// ═══════════════════════════════════════════════════════════════
// ElectricTorchLighting: 极光期间从电源点燃火把
// ═══════════════════════════════════════════════════════════════

internal static class ElectricTorchState
{
    internal static readonly string[] LightSources = { "socket", "outlet", "cableset", "electricdamage_temp" };
    internal static GameObject LookingAt;
}

internal static class Patch_ElectricTorch_SceneInit
{
    internal static void Postfix()
    {
        if (!CheatState.World_ElectricTorch) return;
        try { ElectricTorchHelper.MakeInteractible(); } catch { }
    }
}

internal static class Patch_ElectricTorch_Interact
{
    internal static void Postfix(PlayerManager __instance)
    {
        if (!CheatState.World_ElectricTorch) return;
        try
        {
            if (!GameManager.GetAuroraManager().AuroraIsActive()) return;
            if (!ElectricTorchHelper.PlayerNearSource(__instance)) return;
            if (!__instance.PlayerHoldingTorchThatCanBeLit()) return;
            var panel = InterfaceManager.GetPanel<Panel_TorchLight>();
            if (panel != null) panel.StartTorchIgnite(2f, string.Empty, true);
        }
        catch { }
    }
}

internal static class Patch_ElectricTorch_NoDamage
{
    internal static bool Prefix(DamageTrigger __instance)
    {
        if (!CheatState.World_ElectricTorch) return true;
        return (int)__instance.m_DamageSource != 7;
    }
}

internal static class Patch_ElectricTorch_NoDamageCont
{
    internal static bool Prefix(DamageTrigger __instance)
    {
        if (!CheatState.World_ElectricTorch) return true;
        return (int)__instance.m_DamageSource != 7;
    }
}

internal static class Patch_ElectricTorch_TriggerExit
{
    internal static bool Prefix(DamageTrigger __instance)
    {
        if (!CheatState.World_ElectricTorch) return true;
        return false;
    }
}

internal static class Patch_ElectricTorch_Crosshair
{
    internal static void Postfix(PlayerManager __instance, ref GameObject __result)
    {
        if (!CheatState.World_ElectricTorch) return;
        try
        {
            if ((__result == null || __result != ElectricTorchState.LookingAt) && ElectricTorchState.LookingAt != null)
            {
                var si = ElectricTorchState.LookingAt.GetComponent<SimpleInteraction>();
                if (si != null) { ((Behaviour)si).enabled = false; ElectricTorchState.LookingAt = null; }
            }
            if (__result != null && ElectricTorchState.LightSources.Any(((UnityEngine.Object)__result).name.ToLowerInvariant().Contains)
                && GameManager.GetAuroraManager().AuroraIsActive() && __instance.PlayerHoldingTorchThatCanBeLit())
            {
                var si2 = __result.GetComponent<SimpleInteraction>();
                if (si2 != null && __result != ElectricTorchState.LookingAt)
                {
                    ((Behaviour)si2).enabled = true;
                    ElectricTorchState.LookingAt = __result;
                }
            }
        }
        catch { }
    }
}

internal static class ElectricTorchHelper
{
    internal static bool PlayerNearSource(PlayerManager pm)
    {
        float range = pm.ComputeModifiedPickupRange(GameManager.GetGlobalParameters().m_MaxPickupRange);
        var obj = pm.GetInteractiveObjectUnderCrosshairs(range);
        return obj != null && ElectricTorchState.LightSources.Any(((UnityEngine.Object)obj).name.ToLowerInvariant().Contains);
    }

    internal static void MakeInteractible()
    {
        var scene = SceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();
        var found = new Dictionary<int, GameObject>();
        foreach (var root in roots)
        {
            found.Clear();
            ScanChildren(root, found);
            foreach (var kv in found)
            {
                kv.Value.layer = vp_Layer.InteractivePropNoCollideGear;
                var si = kv.Value.AddComponent<SimpleInteraction>();
                var ls = new LocalizedString();
                ls.m_LocalizationID = "GAMEPLAY_Light";
                ((BaseInteraction)si).m_DefaultHoverText = ls;
                ((Behaviour)si).enabled = false;
            }
        }
    }

    private static void ScanChildren(GameObject obj, Dictionary<int, GameObject> found)
    {
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            var child = obj.transform.GetChild(i).gameObject;
            if (ElectricTorchState.LightSources.Any(((UnityEngine.Object)child).name.ToLower().Contains)
                && !found.ContainsKey(child.GetInstanceID()))
                found.Add(child.GetInstanceID(), child);
            ScanChildren(child, found);
        }
    }
}
