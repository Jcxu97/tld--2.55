using System.Collections.Generic;
using HarmonyLib;
using MelonLoader;
using Il2Cpp;
using Il2CppTLD.IntBackedUnit;

[assembly: MelonInfo(typeof(FoodStackable.ModMain), "FoodStackable", "0.4.7", "user")]
[assembly: MelonGame("Hinterland", "TheLongDark")]

namespace FoodStackable;

public class ModMain : MelonMod
{
    public static MelonLogger.Instance Log;

    public override void OnInitializeMelon()
    {
        Log = LoggerInstance;
        Log.Msg("FoodStackable v0.4.7 loaded — per-frame label reapply covers all redraw paths");
    }
}

internal static class StackState
{
    // Key = representative InventoryGridDataItem.Pointer, Value = count in that group
    // 用 dataItem 指针而不是 GearItem,避免跨 panel 冲突(容器/保温瓶共用 InventoryGridItem)
    public static Dictionary<System.IntPtr, int> Counts = new();

    // 镜像 map:gi.Pointer → count。click/toggle 回调没有 dataItem 参数时用这个
    // 一次只一个 panel active 的前提下,gi.Pointer 跨 panel 不碰撞
    public static Dictionary<System.IntPtr, int> CountsByGi = new();

    // 这些 prefab 明确排除 —— 让游戏 / StackManager 原版处理,避免冲突
    public static readonly HashSet<string> ExcludePrefabs = new HashSet<string>
    {
        "GEAR_MashedPotatoes",
        "GEAR_WaterPurificationTablets",
        "GEAR_CigarettePackMarlboro",
        "GEAR_CigarettePackOld",
    };

    // 前缀匹配:凡是 prefab 名以这些开头的都跳过
    public static readonly string[] ExcludePrefixes =
    {
        "GEAR_cc",  // 卡牌收集 mod 所有 card
    };
}

internal static class Dedupe
{
    public static void Process(Il2CppSystem.Collections.Generic.List<InventoryGridDataItem> list)
    {
        if (list == null || list.Count <= 1) return;

        // key → (dataItem.Pointer, gi.Pointer) 代表
        var firstOfGroup = new Dictionary<string, (System.IntPtr di, System.IntPtr gi)>();
        var keep = new Il2CppSystem.Collections.Generic.List<InventoryGridDataItem>();

        for (int i = 0; i < list.Count; i++)
        {
            var di = list[i];
            var gi = di?.m_GearItem;

            // Decoration or null gear: keep as-is, no grouping
            if (gi == null) { keep.Add(di); continue; }

            // Skip liquids/Soda — vanilla handles
            if (gi.GetComponent<LiquidItem>() != null) { keep.Add(di); continue; }

            // 已真合并的 stackable(m_Units>1)由原版/StackManager 显示 × N,我们让路。
            // 只挂组件但 m_Units=1 的(如整合包给 DryMilkPacket / MixedNuts 加的),
            // 实际没被合,我们仍做 UI 堆叠
            var stackable = gi.GetComponent<StackableItem>();
            if (stackable != null && stackable.m_Units > 1) { keep.Add(di); continue; }

            // Skip hardcoded blacklist.
            // 必须用 GameObject.name(每个 prefab 独立),不能用 m_GearItemData.name —
            // 整合包里多个 mod 加的物品可能共用同一个 GearItemData asset,name 相同,
            // 会导致不同 prefab 被误合并(曾见 roastedAlmonds 吞并多种食物)
            string prefab = gi.name ?? "";
            // Unity 实例化 prefab 后 GameObject.name 可能带 "(Clone)" 后缀
            int cloneIdx = prefab.IndexOf("(Clone)");
            if (cloneIdx >= 0) prefab = prefab.Substring(0, cloneIdx).TrimEnd();
            if (StackState.ExcludePrefabs.Contains(prefab)) { keep.Add(di); continue; }
            bool prefixHit = false;
            for (int p = 0; p < StackState.ExcludePrefixes.Length; p++)
            {
                if (prefab.StartsWith(StackState.ExcludePrefixes[p])) { prefixHit = true; break; }
            }
            if (prefixHit) { keep.Add(di); continue; }

            string key = MakeKey(gi, prefab);
            if (firstOfGroup.TryGetValue(key, out var rep))
            {
                StackState.Counts[rep.di] = StackState.Counts[rep.di] + 1;
                StackState.CountsByGi[rep.gi] = StackState.CountsByGi[rep.gi] + 1;
            }
            else
            {
                firstOfGroup[key] = (di.Pointer, gi.Pointer);
                StackState.Counts[di.Pointer] = 1;
                StackState.CountsByGi[gi.Pointer] = 1;
                keep.Add(di);
            }
        }

        if (keep.Count == list.Count) return;  // nothing to dedupe

        list.Clear();
        for (int i = 0; i < keep.Count; i++) list.Add(keep[i]);
    }

    private static string MakeKey(GearItem gi, string prefab)
    {
        var food = gi.GetComponent<FoodItem>();
        string opened = food != null ? (food.m_Opened ? "O" : "C") : "_";
        return prefab + "|" + opened;
    }
}

[HarmonyPatch(typeof(Panel_Inventory), nameof(Panel_Inventory.RefreshTable))]
internal static class Patch_Inventory_RefreshTable
{
    private static void Prefix(Panel_Inventory __instance)
    {
        try
        {
            StackState.Counts.Clear();
            StackState.CountsByGi.Clear();
            Dedupe.Process(__instance.m_FilteredInventoryList);
        }
        catch (System.Exception ex)
        {
            ModMain.Log.Error($"[Inventory.RefreshTable.Prefix] {ex}");
        }
    }
}

[HarmonyPatch(typeof(Panel_Container), nameof(Panel_Container.RefreshTables))]
internal static class Patch_Container_RefreshTables
{
    private static void Prefix(Panel_Container __instance)
    {
        try
        {
            StackState.Counts.Clear();
            StackState.CountsByGi.Clear();
            Dedupe.Process(__instance.m_FilteredInventoryList);  // 玩家背包栏
            Dedupe.Process(__instance.m_FilteredContainerList);  // 容器栏
        }
        catch (System.Exception ex)
        {
            ModMain.Log.Error($"[Container.RefreshTables.Prefix] {ex}");
        }
    }
}

// 每帧兜底:游戏有多条路径会清 m_StackLabel(OnClick/OnHover/UpdateConditionDisplay
// 等等,Postfix 某些还跑在帧后期其它写操作之前),最稳 cover 是 hook Update()。
// 只对 Counts 里有记录的 gi 重设 label 文本,非堆叠物品不碰,性能可忽略
[HarmonyPatch(typeof(InventoryGridItem), nameof(InventoryGridItem.Update))]
internal static class Patch_GridItem_Update
{
    private static void Postfix(InventoryGridItem __instance) => LabelFix.Reapply(__instance);
}

internal static class LabelFix
{
    public static void Reapply(InventoryGridItem item)
    {
        try
        {
            var gi = item?.m_GearItem;
            if (gi == null) return;
            if (!StackState.CountsByGi.TryGetValue(gi.Pointer, out int count)) return;
            if (count <= 1) return;
            var label = item.m_StackLabel;
            if (label == null) return;
            label.text = "x" + count;
            if (label.gameObject != null) label.gameObject.SetActive(true);
        }
        catch (System.Exception ex)
        {
            ModMain.Log.Error($"[LabelFix.Reapply] {ex}");
        }
    }
}

[HarmonyPatch(typeof(InventoryGridItem), nameof(InventoryGridItem.RefreshDataItem))]
internal static class Patch_RefreshDataItem
{
    private static void Postfix(InventoryGridItem __instance, InventoryGridDataItem dataItem, int index)
    {
        try
        {
            if (dataItem == null) return;
            var label = __instance.m_StackLabel;
            if (label == null) return;

            if (StackState.Counts.TryGetValue(dataItem.Pointer, out int count) && count > 1)
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
