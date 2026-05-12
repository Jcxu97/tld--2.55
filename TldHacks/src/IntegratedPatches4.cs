using System;
using System.Collections.Generic;
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
        CarcassMovingState.DisplayDropHint();
        if ((Input.GetMouseButtonDown(1) && Time.time - CarcassMovingState.PickUpTime > 0.5f) ||
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
    internal static float PickUpTime;
    private static bool _harvestBtnShifted;
    private static HashSet<int> _enabledColliders;

    internal static bool IsMovable(BodyHarvest bh)
    {
        var n = ((UnityEngine.Object)bh).name;
        if (n.Contains("Quarter")) return false;
        if (n.Contains("Doe") || n.Contains("Stag") || n.Contains("Deer") || n.Contains("Wolf")) return true;
        if (CheatState.World_CarcassMovingAll && (n.Contains("Bear") || n.Contains("Moose") || n.Contains("Cougar"))) return true;
        return false;
    }

    internal static bool HasInjuryPreventing() =>
        GameManager.GetSprainedAnkleComponent().HasSprainedAnkle() ||
        GameManager.GetSprainedWristComponent().HasSprainedWrist() ||
        GameManager.GetBrokenRibComponent().HasBrokenRib();

    internal static void PickUp()
    {
        IsCarrying = true;
        PickUpTime = Time.time;
        ItemWeight mw = Harvest.m_MeatAvailableKG;
        ItemWeight gw = Harvest.GetGutsAvailableWeightKg();
        ItemWeight hw = Harvest.GetHideAvailableWeightKg();
        Weight = ((ItemWeight)mw).ToQuantity(1f) + ((ItemWeight)gw).ToQuantity(1f) + ((ItemWeight)hw).ToQuantity(1f);
        OriginalScene = GameManager.m_ActiveScene;
        if (CarcassObj.GetComponent<CarcassMovingBehaviour>() == null)
            CarcassObj.AddComponent<CarcassMovingBehaviour>();
        GameManager.GetPlayerManagerComponent().UnequipItemInHands();
        SetCarcassVisible(false);
        GameAudioManager.PlaySound("Play_RopeGetOn", InterfaceManager.GetSoundEmitter());
        GameAudioManager.PlaySound(EVENTS.PLAY_EXERTIONLOW, InterfaceManager.GetSoundEmitter());
    }

    internal static void Drop()
    {
        IsCarrying = false;
        DropInFrontOfPlayer();
        SetCarcassVisible(true);
        if (Harvest != null) ((Behaviour)Harvest).enabled = true;
        var activeScene = SceneManager.GetActiveScene();
        var root = ((Component)CarcassObj.transform.root).gameObject;
        if (root.scene != activeScene)
        {
            SceneManager.MoveGameObjectToScene(root, activeScene);
            if (Harvest != null) BodyHarvestManager.AddBodyHarvest(Harvest);
        }
        GameAudioManager.PlaySound(EVENTS.PLAY_BODYFALLLARGE, InterfaceManager.GetSoundEmitter());
        var cmb = CarcassObj.GetComponent<CarcassMovingBehaviour>();
        if (cmb != null) UnityEngine.Object.Destroy(cmb);
        Harvest = null;
        CarcassObj = null;
    }

    internal static void MoveToPlayer()
    {
        CarcassObj.transform.position = GameManager.GetPlayerTransform().position;
        CarcassObj.transform.rotation = GameManager.GetPlayerTransform().rotation * Quaternion.Euler(0f, 90f, 0f);
    }

    internal static void DropInFrontOfPlayer()
    {
        var pt = GameManager.GetPlayerTransform();
        var fwd = pt.forward;
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 0.01f) fwd = Vector3.forward;
        else fwd.Normalize();
        CarcassObj.transform.position = pt.position + fwd * 2f;
        CarcassObj.transform.rotation = pt.rotation * Quaternion.Euler(0f, 90f, 0f);
    }

    internal static void SetCarcassVisible(bool visible)
    {
        if (!visible)
        {
            _enabledColliders = new HashSet<int>();
            foreach (var c in CarcassObj.GetComponentsInChildren<Collider>(true))
            {
                if (c.enabled) _enabledColliders.Add(c.GetInstanceID());
                c.enabled = false;
            }
        }
        else
        {
            foreach (var c in CarcassObj.GetComponentsInChildren<Collider>(true))
            {
                c.enabled = _enabledColliders != null && _enabledColliders.Contains(c.GetInstanceID());
            }
            _enabledColliders = null;
        }
        foreach (var r in CarcassObj.GetComponentsInChildren<Renderer>(true))
            r.enabled = visible;
    }

    internal static void AddToSceneSaveData()
    {
        BodyHarvestManager.AddBodyHarvest(Harvest);
        SceneManager.MoveGameObjectToScene(((Component)CarcassObj.transform.root).gameObject, SceneManager.GetActiveScene());
    }

    private static float _lastHintTime;
    internal static void DisplayDropHint()
    {
        if (Time.time - _lastHintTime < 5f) return;
        _lastHintTime = Time.time;
        HUDMessage.AddMessage(I18n.T("右键放下猎物", "Right-click to drop carcass"), false, false);
    }

    internal static bool IsBtnAlive() =>
        MoveBtnObj != null && (bool)(UnityEngine.Object)MoveBtnObj;

    internal static void AddMoveButton(Panel_BodyHarvest panel)
    {
        var harvestBtn = panel.m_Mouse_Button_Harvest;
        if (harvestBtn == null) return;
        if (IsBtnAlive())
        {
            MoveBtnObj.SetActive(true);
        }
        else
        {
            MoveBtnObj = UnityEngine.Object.Instantiate(harvestBtn, harvestBtn.transform.parent);
            MoveBtnObj.transform.localScale = harvestBtn.transform.localScale;
            var loc = MoveBtnObj.GetComponentInChildren<UILocalize>();
            if (loc != null) ((Behaviour)loc).enabled = false;
            var lbl = MoveBtnObj.GetComponentInChildren<UILabel>();
            if (lbl != null) lbl.text = I18n.T("搬运猎物", "Carry");
            var btn = MoveBtnObj.GetComponentInChildren<UIButton>();
            if (btn != null) { btn.onClick.Clear(); EventDelegate.Callback cb = new System.Action(OnMoveClicked); btn.onClick.Add(new EventDelegate(cb)); }
        }
        MoveBtnObj.transform.localPosition = harvestBtn.transform.localPosition + new Vector3(100f, 0f, 0f);
        if (!_harvestBtnShifted)
        {
            harvestBtn.transform.localPosition += new Vector3(-100f, 0f, 0f);
            _harvestBtnShifted = true;
        }
    }

    internal static void RemoveMoveButton(Panel_BodyHarvest panel)
    {
        if (IsBtnAlive())
            MoveBtnObj.SetActive(false);
        if (_harvestBtnShifted)
        {
            var harvestBtn = panel.m_Mouse_Button_Harvest;
            if (harvestBtn != null)
                harvestBtn.transform.localPosition += new Vector3(100f, 0f, 0f);
            _harvestBtnShifted = false;
        }
    }

    internal static void OnMoveClicked()
    {
        if (HasInjuryPreventing())
        {
            GameAudioManager.PlayGUIError();
            HUDMessage.AddMessage(I18n.T("受伤时无法搬运猎物", "Cannot carry while injured"), false, false);
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
        if (bodyHarvest.GetCondition() > 0f) __result = true;
    }
}

internal static class Patch_Carcass_PanelEnable
{
    internal static void Postfix(Panel_BodyHarvest __instance, BodyHarvest bh, bool enable)
    {
        if (!CheatState.World_CarcassMoving) return;
        try
        {
            MelonLoader.MelonLogger.Msg($"[CarcassMove] PanelEnable enable={enable} bh={bh?.ToString() ?? "null"} isCarrying={CarcassMovingState.IsCarrying}");
            if (!enable)
            {
                CarcassMovingState.RemoveMoveButton(__instance);
                return;
            }
            bool canEn = __instance.CanEnable(bh);
            bool movable = bh != null && CarcassMovingState.IsMovable(bh);
            MelonLoader.MelonLogger.Msg($"[CarcassMove] canEnable={canEn} movable={movable} btnAlive={CarcassMovingState.IsBtnAlive()}");
            if (!canEn) return;
            if (CarcassMovingState.IsCarrying) CarcassMovingState.Drop();
            if (movable && !CarcassMovingState.IsCarrying)
            {
                CarcassMovingState.Harvest = bh;
                CarcassMovingState.CarcassObj = ((Component)bh).gameObject;
                CarcassMovingState.AddMoveButton(__instance);
                MelonLoader.MelonLogger.Msg($"[CarcassMove] AddMoveButton done, btnAlive={CarcassMovingState.IsBtnAlive()}");
            }
            else
                CarcassMovingState.RemoveMoveButton(__instance);
        }
        catch (Exception ex) { MelonLoader.MelonLogger.Msg($"[CarcassMove] PanelEnable exception: {ex}"); }
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
        HUDMessage.AddMessage(I18n.T("搬运猎物时无法装备物品", "Cannot equip while carrying"), false, false);
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
    private static bool _logged;

    // __0 = vanilla CalculateModifiedCalorieBurnRate 的第一个输入参数(基础消耗率,根据走/跑/睡等活动)
    internal static void Postfix(ref float __result, float __0)
    {
        if (CheatState.World_CarcassMoving && CarcassMovingState.IsCarrying)
            __result += CarcassMovingState.Weight * 15f;

        // TechBackpack: 消除超重卡路里惩罚,完全对齐 vanilla 不超重时的消耗
        // 当 __result 大于 __0 (base) 说明 vanilla 在加超重/疲劳修正,直接钳回 __0
        // 走路/冲刺/睡觉等活动差异通过 __0 本身体现,不会被破坏
        if (CheatState.TechBackpack && __0 > 0f)
        {
            if (!_logged)
            {
                _logged = true;
                ModMain.Log?.Msg($"[CalorieCap] TechBackpack active, base={__0:F1} result={__result:F1} → cap={__0:F1}");
            }
            if (__result > __0)
                __result = __0;
        }
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

    internal static bool NameMatchesSource(string name)
    {
        if (name == null) return false;
        for (int i = 0; i < LightSources.Length; i++)
            if (name.IndexOf(LightSources[i], System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        return false;
    }
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
            if (__result != null && ElectricTorchState.NameMatchesSource(((UnityEngine.Object)__result).name)
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
        return obj != null && ElectricTorchState.NameMatchesSource(((UnityEngine.Object)obj).name);
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
            if (ElectricTorchState.NameMatchesSource(((UnityEngine.Object)child).name)
                && !found.ContainsKey(child.GetInstanceID()))
                found.Add(child.GetInstanceID(), child);
            ScanChildren(child, found);
        }
    }
}
