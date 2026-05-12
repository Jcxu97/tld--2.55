using Il2Cpp;
using Il2CppSystem.Collections.Generic;
using Il2CppTLD.Gear;
using Il2CppTLD.IntBackedUnit;
using UnityEngine;

namespace TldHacks;

// ═══════════════════════════════════════════════════════════════════
// ImprovedFlasks v1.2.3 by Fuar — 整合
// 快捷轮盘喝水 / 背包分类显示 / 温度指示优化
// ═══════════════════════════════════════════════════════════════════

internal static class FlaskUtils
{
    public static void ConsumeFromFlask(InsulatedFlask flaskItem)
    {
        var items = new List<GearItem>();
        flaskItem.GetAllItems(items);
        if (items.Count == 0)
        {
            GameAudioManager.PlayGUIError();
            HUDMessage.AddMessage("Flask is empty.", false, false);
            return;
        }
        var foodItem = items[0].m_FoodItem;
        if (foodItem == null) return;

        if (foodItem.m_IsDrink && GameManager.GetThirstComponent().m_CurrentThirst / GameManager.GetThirstComponent().m_MaxThirst < 0.01f)
        {
            HUDMessage.AddMessage(Localization.Get("GAMEPLAY_Youarenotthirsty"), false, true);
            GameAudioManager.PlayGUIError();
            return;
        }
        if (!foodItem.m_IsDrink && GameManager.GetHungerComponent().m_MaxReserveCalories - GameManager.GetHungerComponent().GetCalorieReserves() < 10f)
        {
            GameAudioManager.PlayGUIError();
            HUDMessage.AddMessage(Localization.Get("GAMEPLAY_Youaretoofulltoeat"), false, true);
            return;
        }

        if (flaskItem.TryRemoveItem(foodItem.m_GearItem))
        {
            var pm = GameManager.GetPlayerManagerComponent();
            pm.UseFoodInventoryItem(foodItem.m_GearItem);
            if (!pm.ShouldDestroyFoodAfterEating(pm.m_FoodItemEaten))
                flaskItem.TryAddItem(pm.m_FoodItemEaten);
        }
    }
}

// 1. GearItem.Awake Postfix — 设保温瓶 A~G 的类型/容量/热损耗
internal static class Patch_ImprovedFlasks_Awake
{
    internal static void Postfix(GearItem __instance)
    {
        try
        {
            string name = ((Object)__instance).name;
            if (!name.StartsWith("GEAR_InsulatedFlask_")) return;

            __instance.GearItemData.m_Type = (GearType)9;
            var flask = ((Component)__instance).GetComponent<InsulatedFlask>();
            if (flask == null) flask = ((Component)__instance).gameObject.AddComponent<InsulatedFlask>();

            char suffix = name.Length > 20 ? name[20] : '?';
            switch (suffix)
            {
                case 'A':
                    flask.m_Capacity = ItemLiquidVolume.Liter * 1f;
                    flask.m_FallDamagePerMeter = 2f;
                    flask.m_PercentHeatLossPerMinuteIndoors = 0.15f;
                    flask.m_PercentHeatLossPerMinuteOutdoors = 0.25f;
                    flask.m_RangeToPreventHeatLossWhenNextToFire = 2f;
                    break;
                case 'B':
                    flask.m_Capacity = ItemLiquidVolume.Liter * 1.5f;
                    flask.m_FallDamagePerMeter = 4f;
                    flask.m_PercentHeatLossPerMinuteIndoors = 0.12f;
                    flask.m_PercentHeatLossPerMinuteOutdoors = 0.22f;
                    flask.m_RangeToPreventHeatLossWhenNextToFire = 3f;
                    break;
                case 'C':
                    flask.m_Capacity = ItemLiquidVolume.Liter * 2f;
                    flask.m_FallDamagePerMeter = 6f;
                    flask.m_PercentHeatLossPerMinuteIndoors = 0.09f;
                    flask.m_PercentHeatLossPerMinuteOutdoors = 0.19f;
                    flask.m_RangeToPreventHeatLossWhenNextToFire = 4f;
                    break;
                case 'D':
                    flask.m_Capacity = ItemLiquidVolume.Liter * 2.5f;
                    flask.m_FallDamagePerMeter = 8f;
                    flask.m_PercentHeatLossPerMinuteIndoors = 0.06f;
                    flask.m_PercentHeatLossPerMinuteOutdoors = 0.16f;
                    flask.m_RangeToPreventHeatLossWhenNextToFire = 5f;
                    break;
                case 'E':
                    flask.m_Capacity = ItemLiquidVolume.Liter * 3f;
                    flask.m_FallDamagePerMeter = 10f;
                    flask.m_PercentHeatLossPerMinuteIndoors = 0.03f;
                    flask.m_PercentHeatLossPerMinuteOutdoors = 0.13f;
                    flask.m_RangeToPreventHeatLossWhenNextToFire = 6f;
                    break;
                case 'F':
                    flask.m_Capacity = ItemLiquidVolume.Liter * 3.5f;
                    flask.m_FallDamagePerMeter = 12f;
                    flask.m_PercentHeatLossPerMinuteIndoors = 0.01f;
                    flask.m_PercentHeatLossPerMinuteOutdoors = 0.1f;
                    flask.m_RangeToPreventHeatLossWhenNextToFire = 7f;
                    break;
                case 'G':
                    flask.m_Capacity = ItemLiquidVolume.Liter * 4f;
                    flask.m_FallDamagePerMeter = 14f;
                    flask.m_PercentHeatLossPerMinuteIndoors = 0f;
                    flask.m_PercentHeatLossPerMinuteOutdoors = 0.05f;
                    flask.m_RangeToPreventHeatLossWhenNextToFire = 8f;
                    break;
            }
        }
        catch { }
    }
}

// 2. ItemDescriptionPage.UpdateButtons Postfix — 按钮文字改饮用/倒入
internal static class Patch_ImprovedFlasks_Buttons
{
    internal static void Postfix(ItemDescriptionPage __instance, ref GearItem gi)
    {
        try
        {
            if (__instance == null || gi == null || gi.m_InsulatedFlask == null) return;
            __instance.m_Label_MouseButtonEquip.text = "饮用";
            __instance.m_MouseButtonExamine.SetActive(true);
            __instance.m_Label_MouseButtonExamine.text = "倒入";
            __instance.m_OnActionsDelegate = __instance.m_OnEquipDelegate;
            if (__instance.m_MouseButtonExamine != null)
            {
                var btn = __instance.m_MouseButtonExamine.GetComponent<UIButton>();
                if (btn != null) ((UIButtonColor)btn).isEnabled = true;
            }
        }
        catch { }
    }
}

// 3. ItemDescriptionPage.OnEquip Prefix — 背包里点保温瓶直接喝
internal static class Patch_ImprovedFlasks_OnEquip
{
    internal static bool Prefix(ItemDescriptionPage __instance)
    {
        try
        {
            if (__instance == null) return true;
            var inv = InterfaceManager.GetPanel<Panel_Inventory>();
            if (inv == null || !((UnityEngine.Behaviour)inv).isActiveAndEnabled) return true;
            var selected = inv.GetCurrentlySelectedItem();
            if (selected != null && selected.m_GearItem != null && selected.m_GearItem.m_InsulatedFlask != null)
            {
                FlaskUtils.ConsumeFromFlask(selected.m_GearItem.m_InsulatedFlask);
                return false;
            }
        }
        catch { }
        return true;
    }
}

// 4. Panel_ActionsRadial.GetDrinkItemsInInventory — 轮盘加入保温瓶
internal static class Patch_ImprovedFlasks_Radial
{
    internal static bool Prefix() => false;

    internal static void Postfix(Panel_ActionsRadial __instance, ref List<GearItem> __result)
    {
        try
        {
            __instance.m_TempGearItemListNormal.Clear();
            __instance.m_TempGearItemListFavorites.Clear();

            var potable = GameManager.GetInventoryComponent().GetPotableWaterSupply();
            if (potable != null && potable.m_WaterSupply.m_VolumeInLiters > ItemLiquidVolume.Zero)
                __instance.m_TempGearItemListFavorites.Add(potable);

            for (int i = 0; i < GameManager.GetInventoryComponent().m_Items.Count; i++)
            {
                GearItem gi = ((GameObject)GameManager.GetInventoryComponent().m_Items[i])?.GetComponent<GearItem>();
                if (gi == null) continue;

                if (gi.m_EnergyBoost == null && gi.m_FoodItem != null && gi.m_FoodItem.m_IsDrink && (!gi.IsWornOut() || gi.m_IsInSatchel))
                {
                    if (gi.m_IsInSatchel)
                        __instance.m_TempGearItemListFavorites.Add(gi);
                    else
                        __instance.m_TempGearItemListNormal.Add(gi);
                }
                else if (gi.m_InsulatedFlask != null && gi.m_InsulatedFlask.m_VolumeLitres > ItemLiquidVolume.Zero)
                {
                    __instance.m_TempGearItemListFavorites.Add(gi);
                }
            }

            var enumerator = __instance.m_TempGearItemListNormal.GetEnumerator();
            while (enumerator.MoveNext())
                __instance.m_TempGearItemListFavorites.Add(enumerator.Current);

            __result = __instance.m_TempGearItemListFavorites;
        }
        catch { }
    }
}

// 5. Panel_ActionsRadial.UseItem Prefix+Postfix — 轮盘使用保温瓶
internal static class Patch_ImprovedFlasks_UseItem
{
    internal static bool Prefix(ref GearItem gi)
    {
        try { return gi == null || gi.m_InsulatedFlask == null; }
        catch { return true; }
    }

    internal static void Postfix(ref GearItem gi)
    {
        try
        {
            if (gi == null || gi.m_InsulatedFlask == null) return;
            InterfaceManager.GetPanel<Panel_Inventory>().UpdateFilteredInventoryList();
            FlaskUtils.ConsumeFromFlask(gi.m_InsulatedFlask);
        }
        catch { }
    }
}

// 6. RadialMenuArm.SetRadialInfoGear Postfix — 轮盘显示瓶内饮品名
internal static class Patch_ImprovedFlasks_RadialName
{
    internal static void Postfix(RadialMenuArm __instance)
    {
        try
        {
            if (__instance.m_GearItem == null || __instance.m_GearItem.m_InsulatedFlask == null) return;
            var items = __instance.m_GearItem.m_InsulatedFlask.m_Items;
            if (items == null || items.Count == 0) return;
            __instance.m_NameWhenHoveredOver = items[0].m_GearItem.DisplayName.Replace("Cup of", "").Trim();
        }
        catch { }
    }
}

// 7. ItemDescriptionPage.UpdateInsulatedFlaskIndicators Postfix — 温度文字替代填充条
internal static class Patch_ImprovedFlasks_Indicators
{
    internal static void Postfix(ItemDescriptionPage __instance, InsulatedFlask insulatedFlask)
    {
        try
        {
            if (insulatedFlask == null) return;
            var sprite = __instance.m_FlaskHotFillSprite;
            if (sprite != null && ((Component)sprite).transform.parent != null)
                ((Component)((Component)sprite).transform.parent).gameObject.SetActive(false);

            var hotObj = __instance.m_FlaskHot;
            var label = hotObj != null ? hotObj.GetComponentInChildren<UILabel>() : null;
            if (label != null)
            {
                int value = (int)insulatedFlask.m_HeatPercent;
                label.text = $"{Localization.Get("GAMEPLAY_Hot")} ({value}%)";
            }
        }
        catch { }
    }
}

// 8. Panel_InsulatedFlask.RefreshTables Postfix — 标题栏温度+颜色
internal static class Patch_ImprovedFlasks_RefreshTables
{
    internal static void Postfix(Panel_InsulatedFlask __instance)
    {
        try
        {
            if (__instance?.m_InsulatedFlask == null) return;
            float heat = __instance.m_InsulatedFlask.m_HeatPercent;
            int pct = (int)heat;
            string color = heat > 0f ? "be7817" : "5b828f";
            string name = __instance.m_InsulatedFlask.GearItem.GetDisplayNameWithoutConditionForInventoryInterfaces();
            __instance.m_ContainerUI.m_ContainerTitle.text = $"{name} [{color}]({pct}%)[-]";
        }
        catch { }
    }
}
