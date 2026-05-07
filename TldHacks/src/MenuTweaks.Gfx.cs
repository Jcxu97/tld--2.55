using System;
using UnityEngine;

namespace TldHacks;

internal static partial class MenuTweaks
{
    internal static float DrawGfxTab(Func<float, float, float, float, Rect> R, TldHacksSettings s)
    {
        float y = 6f;
        float x = 10f;
        bool changed = false;
        float v;
        bool b;

        y = SectionCol(R, x, y, COL_W, I18n.T("ExtraGraphicsSettings / 准星", "ExtraGraphicsSettings / Crosshair"));

        b = GUI.Toggle(R(x, y, COL_TOG_W, ROW_H), s.CrosshairEnabled,
            I18n.T(" 启用准星", " Enable"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.CrosshairEnabled) { s.CrosshairEnabled = b; changed = true; }
        y += ROW_ADV;

        v = DrawSliderCol(R, x, y, I18n.T("透明度", "Alpha"), s.CrosshairAlpha, 0f, 1f, "F2");
        if (v != s.CrosshairAlpha) { s.CrosshairAlpha = v; changed = true; }
        y += ROW_ADV;

        b = GUI.Toggle(R(x, y, COL_TOG_W, ROW_H), s.CrosshairStone,
            I18n.T(" 石块", " Stone"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.CrosshairStone) { s.CrosshairStone = b; changed = true; }
        y += ROW_ADV;

        b = GUI.Toggle(R(x, y, COL_TOG_W, ROW_H), s.CrosshairRifle,
            I18n.T(" 步枪", " Rifle"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.CrosshairRifle) { s.CrosshairRifle = b; changed = true; }
        y += ROW_ADV;

        b = GUI.Toggle(R(x, y, COL_TOG_W, ROW_H), s.CrosshairBow,
            I18n.T(" 弓箭", " Bow"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.CrosshairBow) { s.CrosshairBow = b; changed = true; }
        y += ROW_ADV;

        if (changed) s.Save();
        return y + 8f;
    }
}
