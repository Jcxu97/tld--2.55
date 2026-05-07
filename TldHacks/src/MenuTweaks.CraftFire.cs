using System;
using UnityEngine;

namespace TldHacks;

internal static partial class MenuTweaks
{
    internal static float DrawCraftFireTab(Func<float, float, float, float, Rect> R, TldHacksSettings s)
    {
        float y1 = 6f;
        float x1 = 10f;
        bool changed = false;
        float v;
        bool b;

        y1 = SectionCol(R, x1, y1, COL_W, I18n.T("CraftAnywhereRedux / 随意制作", "CraftAnywhereRedux / Craft Anywhere"));

        b = GUI.Toggle(R(x1, y1, COL_TOG_W, ROW_H), s.Craft_Anywhere,
            I18n.T(" 随地制作", " Craft Anywhere"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.Craft_Anywhere) { s.Craft_Anywhere = b; changed = true; }
        y1 += ROW_ADV;

        v = DrawSliderCol(R, x1, y1, I18n.T("默认位置(0-4)", "Default Loc"), (float)s.Craft_DefaultLocation, 0f, 4f, "F0");
        if ((int)v != s.Craft_DefaultLocation) { s.Craft_DefaultLocation = (int)v; changed = true; }
        y1 += ROW_ADV;

        GUI.Label(R(x1, y1, COL_W, ROW_H),
            I18n.T("  0=任意 1=工作台 2=锻造 3=弹药台 4=火堆", "  0=Any 1=Bench 2=Forge 3=Ammo 4=Fire"),
            MutedLabel ?? GUI.skin.label);
        y1 += ROW_ADV + SECTION_END_ADV;

        y1 = SectionCol(R, x1, y1, COL_W, I18n.T("MoreCookingSlots / 更多烹饪位", "MoreCookingSlots / Cooking Slots"));

        b = GUI.Toggle(R(x1, y1, COL_TOG_W, ROW_H), s.Craft_MoreCookingSlots,
            I18n.T(" 额外烹饪位", " More Cooking Slots"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.Craft_MoreCookingSlots) { s.Craft_MoreCookingSlots = b; changed = true; }
        y1 += ROW_ADV;

        b = GUI.Toggle(R(x1, y1, COL_TOG_W, ROW_H), s.Craft_MoreSlots_Fireplace,
            I18n.T(" 壁炉额外位", " Fireplace Slots"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.Craft_MoreSlots_Fireplace) { s.Craft_MoreSlots_Fireplace = b; changed = true; }
        y1 += ROW_ADV;

        b = GUI.Toggle(R(x1, y1, COL_TOG_W, ROW_H), s.Craft_MoreSlots_Barrel,
            I18n.T(" 油桶额外位", " Barrel Slots"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.Craft_MoreSlots_Barrel) { s.Craft_MoreSlots_Barrel = b; changed = true; }
        y1 += ROW_ADV;

        b = GUI.Toggle(R(x1, y1, COL_TOG_W, ROW_H), s.Craft_MoreSlots_Grill,
            I18n.T(" 烤架额外位", " Grill Slots"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.Craft_MoreSlots_Grill) { s.Craft_MoreSlots_Grill = b; changed = true; }
        y1 += ROW_ADV + SECTION_END_ADV;

        if (changed) s.Save();
        return y1 + 8f;
    }
}
