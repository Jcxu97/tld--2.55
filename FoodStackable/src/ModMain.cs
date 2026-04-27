using System.Collections.Generic;
using HarmonyLib;
using MelonLoader;
using Il2Cpp;
using Il2CppTLD.IntBackedUnit;

[assembly: MelonInfo(typeof(FoodStackable.ModMain), "FoodStackable", "0.4.1", "user")]
[assembly: MelonGame("Hinterland", "TheLongDark")]

namespace FoodStackable;

public class ModMain : MelonMod
{
    public static MelonLogger.Instance Log;

    public override void OnInitializeMelon()
    {
        Log = LoggerInstance;
        Log.Msg("FoodStackable v0.4.1 loaded — UI-only stacking + N× weight label");
    }
}

internal static class StackState
{
    // Key = representative GearItem.Pointer, Value = count in that group
    public static Dictionary<System.IntPtr, int> Counts = new();
}

[HarmonyPatch(typeof(Panel_Inventory), nameof(Panel_Inventory.RefreshTable))]
internal static class Patch_RefreshTable
{
    private static void Prefix(Panel_Inventory __instance)
    {
        try
        {
            var list = __instance.m_FilteredInventoryList;
            if (list == null || list.Count <= 1) { StackState.Counts.Clear(); return; }

            StackState.Counts.Clear();
            var firstOfGroup = new Dictionary<string, GearItem>();
            var keep = new Il2CppSystem.Collections.Generic.List<InventoryGridDataItem>();

            for (int i = 0; i < list.Count; i++)
            {
                var di = list[i];
                var gi = di?.m_GearItem;

                // Decoration or null gear: keep as-is, no grouping
                if (gi == null) { keep.Add(di); continue; }

                // Skip liquids/Soda — vanilla already handles liquid stacking
                if (gi.GetComponent<LiquidItem>() != null) { keep.Add(di); continue; }

                string key = MakeKey(gi);
                if (firstOfGroup.TryGetValue(key, out var rep))
                {
                    StackState.Counts[rep.Pointer] = StackState.Counts[rep.Pointer] + 1;
                }
                else
                {
                    firstOfGroup[key] = gi;
                    StackState.Counts[gi.Pointer] = 1;
                    keep.Add(di);
                }
            }

            if (keep.Count == list.Count) return;  // nothing to dedupe

            list.Clear();
            for (int i = 0; i < keep.Count; i++) list.Add(keep[i]);
        }
        catch (System.Exception ex)
        {
            ModMain.Log.Error($"[RefreshTable.Prefix] {ex}");
        }
    }

    private static string MakeKey(GearItem gi)
    {
        string prefab = gi.m_GearItemData != null ? gi.m_GearItemData.name : gi.name;
        var food = gi.GetComponent<FoodItem>();
        string opened = food != null ? (food.m_Opened ? "O" : "C") : "_";
        return prefab + "|" + opened;
    }
}

[HarmonyPatch(typeof(InventoryGridItem), nameof(InventoryGridItem.RefreshDataItem))]
internal static class Patch_RefreshDataItem
{
    private static void Postfix(InventoryGridItem __instance, InventoryGridDataItem dataItem, int index)
    {
        try
        {
            var gi = dataItem?.m_GearItem;
            if (gi == null) return;
            var label = __instance.m_StackLabel;
            if (label == null) return;

            if (StackState.Counts.TryGetValue(gi.Pointer, out int count) && count > 1)
            {
                label.text = "x" + count;
                if (label.gameObject != null) label.gameObject.SetActive(true);

                var weightLabel = __instance.m_WeightLabel;
                if (weightLabel != null)
                {
                    bool imperial = weightLabel.text != null && weightLabel.text.Contains("lb");
                    ItemWeight total = dataItem.GetItemWeight(false) * count;
                    weightLabel.text = imperial ? total.ToStringImperial(2u) : total.ToStringMetric(2u);
                }
            }
        }
        catch (System.Exception ex)
        {
            ModMain.Log.Error($"[RefreshDataItem.Postfix] {ex}");
        }
    }
}
