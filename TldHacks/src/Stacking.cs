using System.Collections.Generic;
using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.IntBackedUnit;

namespace TldHacks;

internal static class Stacking
{
    // OnLateUpdate 在所有 MonoBehaviour.Update 之后跑。
    // 核心改动(v2.7.0,per GPT 诊断):
    // Unity UI 的 InventoryGridItem 会被复用(滚动/切分类时同一 cell GameObject 被重新绑到新 dataItem)。
    // 如果盲目用 SeenItems 里缓存的 (item, di) reapply,会把旧 count 写到新 gi 上 —— 表现为"部分堆叠视觉失效"。
    // 修正:每次 reapply 前 VERIFY item 当前 m_GearItem 还是我们当初 hook 时缓存的 gi。
    // 如果 cell 已经被复用到别的 gear,跳过本次,等 RefreshDataItem Postfix 重新登记。
    public static void OnLateUpdate()
    {
        try
        {
            if (StackState.Counts.Count == 0) return;
            if (StackState.SeenItems.Count == 0) return;

            // 不能在遍历中修改 dict。先收集待删的 key,遍历结束统一清理
            System.Collections.Generic.List<System.IntPtr> stale = null;

            foreach (var kv in StackState.SeenItems)
            {
                var (item, cachedDi, cachedGiPtr) = kv.Value;
                if (item == null) { (stale ??= new()).Add(kv.Key); continue; }

                // 关键 verify:当前绑定的 GearItem 是否还是我们缓存的那个?
                // 复用后 item.m_GearItem 会变,此时跳过,不写 stale label
                var curGi = item.m_GearItem;
                if (curGi == null || curGi.Pointer != cachedGiPtr)
                {
                    // cell 已被复用 / 绑到空。等 RefreshDataItem Postfix 重新登记。
                    // 不从 SeenItems 删除 —— Postfix 会 overwrite 这个 key
                    continue;
                }

                LabelFix.Reapply(item, cachedDi);
            }

            if (stale != null)
                foreach (var k in stale) StackState.SeenItems.Remove(k);
        }
        catch (System.Exception ex)
        {
            ModMain.Log?.Error($"[OnLateUpdate] {ex}");
        }
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

    // OnUpdate tick 用来知道要扫哪个 panel 的 m_TableItems
    public static Panel_Inventory LastInventoryPanel;
    public static Panel_Container LastContainerPanel;

    // 所有最近刷新过的 (InventoryGridItem, 它当时绑的 InventoryGridDataItem, 当时的 gi.Pointer)
    // OnLateUpdate 逐帧 verify+reapply。giPtr 是 cell-reuse 侦测的锚 ——
    // 如果 item.m_GearItem.Pointer 已经变了,就说明 cell 被复用到别的 gear 上,不能用缓存 di reapply。
    public static Dictionary<System.IntPtr, (InventoryGridItem item, InventoryGridDataItem di, System.IntPtr giPtr)> SeenItems = new();

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
        if (ModMain.Settings != null && !ModMain.Settings.StackingEnabled) return;

        // key → (dataItem.Pointer, gi.Pointer) 代表
        var firstOfGroup = new Dictionary<string, (System.IntPtr di, System.IntPtr gi)>();
        var keep = new Il2CppSystem.Collections.Generic.List<InventoryGridDataItem>();

        for (int i = 0; i < list.Count; i++)
        {
            var di = list[i];
            var gi = di?.m_GearItem;

            if (gi == null) { keep.Add(di); continue; }

            // Skip liquids/Soda — vanilla handles
            if (gi.GetComponent<LiquidItem>() != null) { keep.Add(di); continue; }

            // 已真合并的 stackable(m_Units>1)由原版/StackManager 显示 × N,我们让路。
            // 只挂组件但 m_Units=1 的(如整合包给 DryMilkPacket / MixedNuts 加的),
            // 实际没被合,我们仍做 UI 堆叠
            var stackable = gi.GetComponent<StackableItem>();
            if (stackable != null && stackable.m_Units > 1) { keep.Add(di); continue; }

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
            StackState.SeenItems.Clear();
            StackState.LastInventoryPanel = __instance;
            StackState.LastContainerPanel = null;
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
            StackState.SeenItems.Clear();
            StackState.LastContainerPanel = __instance;
            StackState.LastInventoryPanel = null;
            Dedupe.Process(__instance.m_FilteredInventoryList);  // 玩家背包栏
            Dedupe.Process(__instance.m_FilteredContainerList);  // 容器栏
        }
        catch (System.Exception ex)
        {
            ModMain.Log.Error($"[Container.RefreshTables.Prefix] {ex}");
        }
    }
}

internal static class LabelFix
{
    public static void Reapply(InventoryGridItem item, InventoryGridDataItem di)
    {
        try
        {
            if (item == null || di == null) return;
            if (!StackState.Counts.TryGetValue(di.Pointer, out int count)) return;
            if (count <= 1) return;

            // stack label —— 只在需要变化时写 text(避免每帧字符串分配)
            var label = item.m_StackLabel;
            if (label != null)
            {
                string want = "x" + count;
                if (label.text != want) label.text = want;
                if (label.gameObject != null && !label.gameObject.activeSelf)
                    label.gameObject.SetActive(true);
            }

            // weight label —— 只在 text 不是已经是预期值时重算(避免每帧算 ItemWeight 乘法)
            var weightLabel = item.m_WeightLabel;
            if (weightLabel != null)
            {
                string cur = weightLabel.text;
                // 粗判:如果 label 已经有 "(N)" 或 stack 后缀暗示已处理,skip
                // 简化:用 cached compare,只在游戏改了 weight label 时才 recompute
                bool imperial = cur != null && cur.Contains("lb");
                ItemWeight total = di.GetItemWeight(false) * count;
                string want = imperial ? total.ToStringImperial(2u) : total.ToStringMetric(2u);
                if (cur != want) weightLabel.text = want;
            }
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

            // 记录 (item, 此次绑的 dataItem, 此次的 gi.Pointer) 到 SeenItems。
            // OnLateUpdate 每帧 verify(靠 giPtr 对比当前绑定)+ reapply;复用后 gi 变,verify 会 skip。
            if (__instance != null)
            {
                var gi = dataItem.m_GearItem;
                System.IntPtr giPtr = gi != null ? gi.Pointer : System.IntPtr.Zero;
                StackState.SeenItems[__instance.Pointer] = (__instance, dataItem, giPtr);
            }

            LabelFix.Reapply(__instance, dataItem);
        }
        catch (System.Exception ex)
        {
            ModMain.Log.Error($"[RefreshDataItem.Postfix] {ex}");
        }
    }
}

// Refresh(GearItem, int) 是另一个 cell-bind 路径(跳过 dataItem 直接绑 gi)——
// 一些 panel (slot / equipment) 走这条。我们从 CountsByGi 拿 count。
[HarmonyPatch(typeof(InventoryGridItem), nameof(InventoryGridItem.Refresh))]
internal static class Patch_InventoryGridItem_Refresh
{
    private static void Postfix(InventoryGridItem __instance, GearItem gi, int index)
    {
        try
        {
            if (__instance == null || gi == null) return;
            if (!StackState.CountsByGi.TryGetValue(gi.Pointer, out int count) || count <= 1) return;

            var label = __instance.m_StackLabel;
            if (label != null)
            {
                string want = "x" + count;
                if (label.text != want) label.text = want;
                if (label.gameObject != null && !label.gameObject.activeSelf)
                    label.gameObject.SetActive(true);
            }
            // weight label 这条路没 dataItem,改用 GearItem 自身的 GetItemWeightKG() * count
            var weightLabel = __instance.m_WeightLabel;
            if (weightLabel != null)
            {
                try
                {
                    ItemWeight total = gi.GetItemWeightKG() * count;
                    bool imperial = weightLabel.text != null && weightLabel.text.Contains("lb");
                    string want = imperial ? total.ToStringImperial(2u) : total.ToStringMetric(2u);
                    if (weightLabel.text != want) weightLabel.text = want;
                }
                catch { }
            }
        }
        catch (System.Exception ex)
        {
            ModMain.Log.Error($"[InventoryGridItem.Refresh.Postfix] {ex}");
        }
    }
}
