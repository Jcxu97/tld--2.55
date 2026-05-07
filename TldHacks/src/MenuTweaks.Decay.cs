using System;
using UnityEngine;

namespace TldHacks;

internal static partial class MenuTweaks
{
    internal static float DrawDecayTab(Func<float, float, float, float, Rect> R, TldHacksSettings s)
    {
        float y1 = 6f, y2 = 6f;
        float x1 = 10f;
        float x2 = x1 + COL_W + 10f;
        bool changed = false;
        float v;

        // ══════ LEFT COLUMN ══════

        y1 = SectionCol(R, x1, y1, COL_W, I18n.T("GearDecayModifier / 总开关", "GearDecayModifier / Master"));

        bool infDur = GUI.Toggle(R(x1, y1, COL_TOG_W, ROW_H), s.InfiniteDurability,
            I18n.T(" 无限耐久", " Infinite Durability"), ToggleStyle ?? GUI.skin.toggle);
        if (infDur != s.InfiniteDurability) { s.InfiniteDurability = infDur; changed = true; }
        y1 += ROW_ADV;

        GUI.Label(R(x1, y1, COL_W, ROW_H),
            I18n.T("关闭无限耐久时,下方倍率生效", "When off, multipliers apply"),
            MutedLabel ?? GUI.skin.label);
        y1 += ROW_ADV + SECTION_END_ADV;

        y1 = SectionCol(R, x1, y1, COL_W, I18n.T("GearDecayModifier / 通用衰减", "GearDecayModifier / General Decay"));

        GUI.Label(R(x1, y1, COL_W, ROW_H),
            I18n.T("  倍率: 0=不衰减 1=原版 2=双倍衰减", "  0=no decay 1=vanilla 2=2x decay"),
            MutedLabel ?? GUI.skin.label);
        y1 += ROW_ADV;

        v = DrawSliderCol(R, x1, y1, I18n.T("通用衰减", "General"), s.GeneralDecay, 0f, 2f, "F2");
        if (v != s.GeneralDecay) { s.GeneralDecay = v; changed = true; }
        y1 += ROW_ADV;

        v = DrawSliderCol(R, x1, y1, I18n.T("拾取前衰减", "Pre-Pickup"), s.DecayBeforePickup, 0f, 2f, "F2");
        if (v != s.DecayBeforePickup) { s.DecayBeforePickup = v; changed = true; }
        y1 += ROW_ADV;

        v = DrawSliderCol(R, x1, y1, I18n.T("使用时衰减", "On-Use"), s.OnUseDecay, 0f, 2f, "F2");
        if (v != s.OnUseDecay) { s.OnUseDecay = v; changed = true; }
        y1 += ROW_ADV + SECTION_END_ADV;

        y1 = SectionCol(R, x1, y1, COL_W, I18n.T("GearDecayModifier / 食物衰减", "GearDecayModifier / Food Decay"));

        v = DrawSliderCol(R, x1, y1, I18n.T("食物(总)", "Food Total"), s.FoodDecay, 0f, 2f, "F2");
        if (v != s.FoodDecay) { s.FoodDecay = v; changed = true; }
        y1 += ROW_ADV;

        v = DrawSliderCol(R, x1, y1, I18n.T("生肉", "Raw Meat"), s.RawMeatDecay, 0f, 2f, "F2");
        if (v != s.RawMeatDecay) { s.RawMeatDecay = v; changed = true; }
        y1 += ROW_ADV;

        v = DrawSliderCol(R, x1, y1, I18n.T("熟肉", "Cooked Meat"), s.CookedMeatDecay, 0f, 2f, "F2");
        if (v != s.CookedMeatDecay) { s.CookedMeatDecay = v; changed = true; }
        y1 += ROW_ADV;

        v = DrawSliderCol(R, x1, y1, I18n.T("饮品", "Drinks"), s.DrinksDecay, 0f, 2f, "F2");
        if (v != s.DrinksDecay) { s.DrinksDecay = v; changed = true; }
        y1 += ROW_ADV;

        if (GUI.Button(R(x1, y1, 200f, ROW_H), _foodDetailExpanded
            ? I18n.T("▲ 收起食物细分", "▲ Hide Food Detail")
            : I18n.T("▼ 展开食物细分 (10)", "▼ Food Detail (10)")))
            _foodDetailExpanded = !_foodDetailExpanded;
        y1 += ROW_ADV;

        if (_foodDetailExpanded)
        {
            v = DrawSliderCol(R, x1, y1, I18n.T("腌肉", "Cured Meat"), s.CuredMeatDecay, 0f, 2f, "F2");
            if (v != s.CuredMeatDecay) { s.CuredMeatDecay = v; changed = true; }
            y1 += ROW_ADV;

            v = DrawSliderCol(R, x1, y1, I18n.T("生鱼", "Raw Fish"), s.RawFishDecay, 0f, 2f, "F2");
            if (v != s.RawFishDecay) { s.RawFishDecay = v; changed = true; }
            y1 += ROW_ADV;

            v = DrawSliderCol(R, x1, y1, I18n.T("熟鱼", "Cooked Fish"), s.CookedFishDecay, 0f, 2f, "F2");
            if (v != s.CookedFishDecay) { s.CookedFishDecay = v; changed = true; }
            y1 += ROW_ADV;

            v = DrawSliderCol(R, x1, y1, I18n.T("罐头", "Canned"), s.CannedFoodDecay, 0f, 2f, "F2");
            if (v != s.CannedFoodDecay) { s.CannedFoodDecay = v; changed = true; }
            y1 += ROW_ADV;

            v = DrawSliderCol(R, x1, y1, I18n.T("咖啡/茶", "Coffee/Tea"), s.CoffeeTeaDecay, 0f, 2f, "F2");
            if (v != s.CoffeeTeaDecay) { s.CoffeeTeaDecay = v; changed = true; }
            y1 += ROW_ADV;

            v = DrawSliderCol(R, x1, y1, I18n.T("包装食品", "Packaged"), s.PackagedFoodDecay, 0f, 2f, "F2");
            if (v != s.PackagedFoodDecay) { s.PackagedFoodDecay = v; changed = true; }
            y1 += ROW_ADV;

            v = DrawSliderCol(R, x1, y1, I18n.T("油脂", "Fat"), s.FatDecay, 0f, 2f, "F2");
            if (v != s.FatDecay) { s.FatDecay = v; changed = true; }
            y1 += ROW_ADV;

            v = DrawSliderCol(R, x1, y1, I18n.T("腌鱼", "Cured Fish"), s.CuredFishDecay, 0f, 2f, "F2");
            if (v != s.CuredFishDecay) { s.CuredFishDecay = v; changed = true; }
            y1 += ROW_ADV;

            v = DrawSliderCol(R, x1, y1, I18n.T("食材", "Ingredients"), s.IngredientsDecay, 0f, 2f, "F2");
            if (v != s.IngredientsDecay) { s.IngredientsDecay = v; changed = true; }
            y1 += ROW_ADV;

            v = DrawSliderCol(R, x1, y1, I18n.T("其他食品", "Other Food"), s.OtherFoodDecay, 0f, 2f, "F2");
            if (v != s.OtherFoodDecay) { s.OtherFoodDecay = v; changed = true; }
            y1 += ROW_ADV;
        }
        y1 += SECTION_END_ADV;

        // ══════ RIGHT COLUMN ══════

        y2 = SectionCol(R, x2, y2, COL_W, I18n.T("GearDecayModifier / 装备衰减", "GearDecayModifier / Gear Decay"));

        v = DrawSliderCol(R, x2, y2, I18n.T("衣物", "Clothing"), s.ClothingDecayRate, 0f, 2f, "F2");
        if (v != s.ClothingDecayRate) { s.ClothingDecayRate = v; changed = true; }
        y2 += ROW_ADV;

        v = DrawSliderCol(R, x2, y2, I18n.T("枪械", "Gun"), s.GunDecay, 0f, 2f, "F2");
        if (v != s.GunDecay) { s.GunDecay = v; changed = true; }
        y2 += ROW_ADV;

        v = DrawSliderCol(R, x2, y2, I18n.T("弓箭", "Bow"), s.BowDecay, 0f, 2f, "F2");
        if (v != s.BowDecay) { s.BowDecay = v; changed = true; }
        y2 += ROW_ADV;

        v = DrawSliderCol(R, x2, y2, I18n.T("工具", "Tools"), s.ToolsDecay, 0f, 2f, "F2");
        if (v != s.ToolsDecay) { s.ToolsDecay = v; changed = true; }
        y2 += ROW_ADV;

        if (GUI.Button(R(x2, y2, 200f, ROW_H), _gearDetailExpanded
            ? I18n.T("▲ 收起装备细分", "▲ Hide Gear Detail")
            : I18n.T("▼ 展开装备细分 (13)", "▼ Gear Detail (13)")))
            _gearDetailExpanded = !_gearDetailExpanded;
        y2 += ROW_ADV;

        if (_gearDetailExpanded)
        {
            v = DrawSliderCol(R, x2, y2, I18n.T("睡袋", "Bedroll"), s.BedrollDecay, 0f, 2f, "F2");
            if (v != s.BedrollDecay) { s.BedrollDecay = v; changed = true; }
            y2 += ROW_ADV;

            v = DrawSliderCol(R, x2, y2, I18n.T("雪橇", "Travois"), s.TravoisDecay, 0f, 2f, "F2");
            if (v != s.TravoisDecay) { s.TravoisDecay = v; changed = true; }
            y2 += ROW_ADV;

            v = DrawSliderCol(R, x2, y2, I18n.T("箭矢", "Arrow"), s.ArrowDecay, 0f, 2f, "F2");
            if (v != s.ArrowDecay) { s.ArrowDecay = v; changed = true; }
            y2 += ROW_ADV;

            v = DrawSliderCol(R, x2, y2, I18n.T("陷阱", "Snare"), s.SnareDecay, 0f, 2f, "F2");
            if (v != s.SnareDecay) { s.SnareDecay = v; changed = true; }
            y2 += ROW_ADV;

            v = DrawSliderCol(R, x2, y2, I18n.T("生火工具", "Firestarting"), s.FirestartingDecay, 0f, 2f, "F2");
            if (v != s.FirestartingDecay) { s.FirestartingDecay = v; changed = true; }
            y2 += ROW_ADV;

            v = DrawSliderCol(R, x2, y2, I18n.T("兽皮/尸骸", "Hide/Gut"), s.HideDecay, 0f, 2f, "F2");
            if (v != s.HideDecay) { s.HideDecay = v; changed = true; }
            y2 += ROW_ADV;

            v = DrawSliderCol(R, x2, y2, I18n.T("急救品", "First Aid"), s.FirstAidDecay, 0f, 2f, "F2");
            if (v != s.FirstAidDecay) { s.FirstAidDecay = v; changed = true; }
            y2 += ROW_ADV;

            v = DrawSliderCol(R, x2, y2, I18n.T("净水片", "Water Purifier"), s.WaterPurifierDecay, 0f, 2f, "F2");
            if (v != s.WaterPurifierDecay) { s.WaterPurifierDecay = v; changed = true; }
            y2 += ROW_ADV;

            v = DrawSliderCol(R, x2, y2, I18n.T("锅具", "Cooking Pot"), s.CookingPotDecay, 0f, 2f, "F2");
            if (v != s.CookingPotDecay) { s.CookingPotDecay = v; changed = true; }
            y2 += ROW_ADV;

            v = DrawSliderCol(R, x2, y2, I18n.T("信号枪弹药", "Flare Ammo"), s.FlareGunAmmoDecay, 0f, 2f, "F2");
            if (v != s.FlareGunAmmoDecay) { s.FlareGunAmmoDecay = v; changed = true; }
            y2 += ROW_ADV;

            v = DrawSliderCol(R, x2, y2, I18n.T("磨刀石", "Whetstone"), s.WhetstoneDecay, 0f, 2f, "F2");
            if (v != s.WhetstoneDecay) { s.WhetstoneDecay = v; changed = true; }
            y2 += ROW_ADV;

            v = DrawSliderCol(R, x2, y2, I18n.T("开罐器", "Can Opener"), s.CanOpenerDecay, 0f, 2f, "F2");
            if (v != s.CanOpenerDecay) { s.CanOpenerDecay = v; changed = true; }
            y2 += ROW_ADV;

            v = DrawSliderCol(R, x2, y2, I18n.T("撬棍", "Prybar"), s.PrybarDecay, 0f, 2f, "F2");
            if (v != s.PrybarDecay) { s.PrybarDecay = v; changed = true; }
            y2 += ROW_ADV;
        }
        y2 += SECTION_END_ADV;

        y2 = SectionCol(R, x2, y2, COL_W, I18n.T("衣物衰减细分", "Clothing Decay"));

        v = DrawSliderCol(R, x2, y2, I18n.T("日常衰减", "Daily"), s.ClothingDecayDaily, 0f, 1f, "F2");
        if (v != s.ClothingDecayDaily) { s.ClothingDecayDaily = v; changed = true; }
        y2 += ROW_ADV;

        v = DrawSliderCol(R, x2, y2, I18n.T("室内衰减", "Indoor"), s.ClothingDecayIndoors, 0f, 1f, "F2");
        if (v != s.ClothingDecayIndoors) { s.ClothingDecayIndoors = v; changed = true; }
        y2 += ROW_ADV;

        v = DrawSliderCol(R, x2, y2, I18n.T("室外衰减", "Outdoor"), s.ClothingDecayOutdoors, 0f, 1f, "F2");
        if (v != s.ClothingDecayOutdoors) { s.ClothingDecayOutdoors = v; changed = true; }
        y2 += ROW_ADV + SECTION_END_ADV;

        if (changed) s.Save();
        float yMax = Mathf.Max(y1, y2);
        return yMax + 8f;
    }
}
