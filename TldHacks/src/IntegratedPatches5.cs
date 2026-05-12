using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.Placement;
using UnityEngine;

namespace TldHacks;

// ═══════════════════════════════════════════════════════════════════
//   v3.0.4 IntegratedPatches5
//   PlaceFromInventory v1.1.3 — 背包/衣物右键放置 + Ctrl 整堆丢
//   v3.0.4r1: MapManager 整合已移除(改回 MapManager.dll 独立 mod)
// ═══════════════════════════════════════════════════════════════════

internal static class PlaceFromInvUI
{
    private static bool _backToInv;
    private static bool _backToCloth;

    public static Panel_Inventory InvPanel => InterfaceManager.GetPanel<Panel_Inventory>();
    public static Panel_Clothing ClothPanel => InterfaceManager.GetPanel<Panel_Clothing>();

    public static void HideCurrentPanel()
    {
        _backToInv = InvPanel != null && ((Panel_Base)InvPanel).IsEnabled();
        if (_backToInv) InvPanel.Enable(false, true);
        _backToCloth = ClothPanel != null && ((Panel_Base)ClothPanel).IsEnabled();
        if (_backToCloth) ((Panel_Base)ClothPanel).Enable(false);
    }

    public static void RestorePreviousPanel()
    {
        if (_backToInv) InvPanel.Enable(true, true);
        if (_backToCloth) ((Panel_Base)ClothPanel).Enable(true);
        _backToInv = _backToCloth = false;
    }

    public static void StartPlaceObject(GameObject go, PlaceMeshFlags flags = (PlaceMeshFlags)0)
    {
        try
        {
            HideCurrentPanel();
            var pm = GameManager.GetPlayerManagerComponent();
            if (pm != null) pm.StartPlaceMesh(go, flags, (PlaceMeshRules)1);
        }
        catch { }
    }
}

[HarmonyPatch(typeof(InventoryGridItem), "OnClick")]
internal static class Patch_PlaceFromInv_GridClick
{
    static int _lastFrameCancelled;
    public static void NotifyCancelled() => _lastFrameCancelled = Time.frameCount;
    static bool ShouldSkipClick => Time.frameCount - _lastFrameCancelled < 10;

    static void Postfix(InventoryGridItem __instance)
    {
        try
        {
            if (ModMain.Settings == null || !ModMain.Settings.PlaceFromInv_Enabled) return;
            if (ShouldSkipClick) return;
            if (!Input.GetMouseButtonUp(1)) return;
            if (((Panel_Base)InterfaceManager.GetPanel<Panel_Container>()).IsEnabled()) return;
            var inv = PlaceFromInvUI.InvPanel;
            if (inv == null) return;
            if (!inv.m_ItemDescriptionPage.CanDrop(__instance.m_GearItem)) return;
            if (__instance.m_GearItem.m_WaterSupply != null) return;
            PlaceFromInvUI.StartPlaceObject(((UnityEngine.Component)__instance.m_GearItem).gameObject, (PlaceMeshFlags)2);
        }
        catch { }
    }
}

[HarmonyPatch(typeof(ClothingSlot), "DoClickAction")]
internal static class Patch_PlaceFromInv_ClothClick
{
    internal static GearItem _clothItemToPlace;

    static void Postfix(ClothingSlot __instance)
    {
        try
        {
            if (ModMain.Settings == null || !ModMain.Settings.PlaceFromInv_Enabled) return;
            if (!Input.GetMouseButtonUp(1)) return;
            if (__instance.m_GearItem == null) return;
            var cp = PlaceFromInvUI.ClothPanel;
            if (cp == null) return;
            if (!cp.m_ItemDescriptionPage.CanDrop(__instance.m_GearItem)) return;
            _clothItemToPlace = __instance.m_GearItem;
            cp.OnDropItem();
        }
        catch { }
    }
}

[HarmonyPatch(typeof(Panel_Clothing), "OnDropItem")]
internal static class Patch_PlaceFromInv_ClothDrop
{
    static void Postfix()
    {
        try
        {
            if (ModMain.Settings == null || !ModMain.Settings.PlaceFromInv_Enabled) return;
            if (Patch_PlaceFromInv_ClothClick._clothItemToPlace == null) return;
            PlaceFromInvUI.StartPlaceObject(((UnityEngine.Component)Patch_PlaceFromInv_ClothClick._clothItemToPlace).gameObject, (PlaceMeshFlags)0);
            Patch_PlaceFromInv_ClothClick._clothItemToPlace = null;
        }
        catch { }
    }
}

[HarmonyPatch(typeof(PlayerManager), "ExitMeshPlacement")]
internal static class Patch_PlaceFromInv_ExitMesh
{
    static void Postfix(PlayerManager __instance)
    {
        try
        {
            if (ModMain.Settings == null || !ModMain.Settings.PlaceFromInv_Enabled) return;
            if (!__instance.m_SkipCancel) Patch_PlaceFromInv_GridClick.NotifyCancelled();
            PlaceFromInvUI.RestorePreviousPanel();
        }
        catch { }
    }
}

// DoPositionCheck 每帧被游戏调用 — 迁到 DynamicPatch,功能关时零 trampoline
internal static class Patch_PlaceFromInv_PositionCheck
{
    public static void Postfix(ref MeshLocationCategory __result)
    {
        try
        {
            if ((int)__result == 11) __result = (MeshLocationCategory)0;
        }
        catch { }
    }
}
