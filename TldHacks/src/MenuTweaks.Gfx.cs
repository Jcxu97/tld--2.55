using System;
using UnityEngine;

namespace TldHacks;

internal static partial class MenuTweaks
{
    internal static float DrawGfxTab(Func<float, float, float, float, Rect> R, TldHacksSettings s)
    {
        float c1 = 0f, c2 = COL_W + 20f;
        float subTogW = COL_W / 2f - 6f;
        float c1Sub = c1 + COL_W / 2f;
        float c2Sub = c2 + COL_W / 2f;
        float y1 = 6f, y2 = 6f;
        bool changed = false;
        float v;
        bool b;

        // ——— Column 1: Crosshair + Vehicle Camera ———

        y1 = SectionCol(R, c1, y1, COL_W, I18n.T("准星", "Crosshair"));

        bool ce = GUI.Toggle(R(c1, y1, COL_TOG_W, ROW_H), s.CrosshairEnabled,
            I18n.T(" 启用准星", " Enable Crosshair"), ToggleStyle ?? GUI.skin.toggle);
        if (ce != s.CrosshairEnabled) { s.CrosshairEnabled = ce; changed = true; }
        y1 += ROW_ADV;

        v = DrawSliderCol(R, c1, y1, I18n.T("准星透明度", "Alpha"), s.CrosshairAlpha, 0f, 1f, "F2");
        if (v != s.CrosshairAlpha) { s.CrosshairAlpha = v; changed = true; }
        y1 += ROW_ADV;
        GUI.Label(R(c1, y1, COL_W, ROW_H), I18n.T("   下方勾选用什么武器/工具时显示屏幕中心准星", "   show crosshair when wielding selected items"), MutedLabel ?? GUI.skin.label);
        y1 += ROW_ADV;

        bool csStone = GUI.Toggle(R(c1,    y1, subTogW, ROW_H), s.CrosshairStone, I18n.T(" 投掷石块", " Stone"), ToggleStyle ?? GUI.skin.toggle);
        bool csRifle = GUI.Toggle(R(c1Sub, y1, subTogW, ROW_H), s.CrosshairRifle, I18n.T(" 步枪瞄准", " Rifle"), ToggleStyle ?? GUI.skin.toggle);
        if (csStone != s.CrosshairStone) { s.CrosshairStone = csStone; changed = true; }
        if (csRifle != s.CrosshairRifle) { s.CrosshairRifle = csRifle; changed = true; }
        y1 += ROW_ADV;

        bool csBow = GUI.Toggle(R(c1, y1, subTogW, ROW_H), s.CrosshairBow, I18n.T(" 弓箭瞄准", " Bow"), ToggleStyle ?? GUI.skin.toggle);
        if (csBow != s.CrosshairBow) { s.CrosshairBow = csBow; changed = true; }
        y1 += ROW_ADV + SECTION_END_ADV;

        y1 = SectionCol(R, c1, y1, COL_W, I18n.T("车辆视角", "Vehicle Camera"));
        bool vFov  = GUI.Toggle(R(c1,    y1, subTogW, ROW_H), s.VehicleKeepFov,  I18n.T(" 车内保持FOV", " Keep FOV"), ToggleStyle ?? GUI.skin.toggle);
        bool vFree = GUI.Toggle(R(c1Sub, y1, subTogW, ROW_H), s.VehicleFreeLook, I18n.T(" 车内自由视角", " Free Look"), ToggleStyle ?? GUI.skin.toggle);
        if (vFov != s.VehicleKeepFov) { s.VehicleKeepFov = vFov; changed = true; }
        if (vFree != s.VehicleFreeLook) { s.VehicleFreeLook = vFree; changed = true; }
        y1 += ROW_ADV;
        v = DrawSliderCol(R, c1, y1, I18n.T("水平转头(°)", "Yaw"), s.VehicleFreeLookYaw, 30f, 180f, "F0");
        if (v != s.VehicleFreeLookYaw) { s.VehicleFreeLookYaw = v; changed = true; }
        y1 += ROW_ADV;
        v = DrawSliderCol(R, c1, y1, I18n.T("上下抬头(°)", "Pitch"), s.VehicleFreeLookPitch, 20f, 90f, "F0");
        if (v != s.VehicleFreeLookPitch) { s.VehicleFreeLookPitch = v; changed = true; }
        y1 += ROW_ADV + SECTION_END_ADV;

        // ——— Column 2: FOV + House Lights ———

        y2 = SectionCol(R, c2, y2, COL_W, I18n.T("视野 & FOV", "Field of View"));
        bool fovExt = GUI.Toggle(R(c2, y2, COL_TOG_W, ROW_H), s.TT_ExtendedFOV,
            I18n.T(" FOV滑块扩展(30-150)", " Extended FOV Slider (30-150)"), ToggleStyle ?? GUI.skin.toggle);
        if (fovExt != s.TT_ExtendedFOV) { s.TT_ExtendedFOV = fovExt; CheatState.TT_ExtendedFOV = fovExt; changed = true; }
        y2 += ROW_ADV;
        GUI.Label(R(c2, y2, COL_W, ROW_H), I18n.T("   游戏设置里FOV范围从54-90扩展到30-150", "   extends FOV range in game options"), MutedLabel ?? GUI.skin.label);
        y2 += ROW_ADV + SECTION_END_ADV;

        y2 = SectionCol(R, c2, y2, COL_W, I18n.T("室内灯光", "House Lights"));
        b = GUI.Toggle(R(c2, y2, COL_TOG_W, ROW_H), s.HL_Enabled,
            I18n.T(" 启用室内灯光(非极光也能开灯)", " Enable (lights on without aurora)"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.HL_Enabled) { s.HL_Enabled = b; changed = true; }
        y2 += ROW_ADV;
        b = GUI.Toggle(R(c2,    y2, subTogW, ROW_H), s.HL_EnableOutside, I18n.T(" 室外也启用", " Outdoors"), ToggleStyle ?? GUI.skin.toggle);
        bool hlW = GUI.Toggle(R(c2Sub, y2, subTogW, ROW_H), s.HL_WhiteLights,  I18n.T(" 白光模式", " White Light"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.HL_EnableOutside) { s.HL_EnableOutside = b; changed = true; }
        if (hlW != s.HL_WhiteLights) { s.HL_WhiteLights = hlW; changed = true; }
        y2 += ROW_ADV;
        b = GUI.Toggle(R(c2,    y2, subTogW, ROW_H), s.HL_NoFlicker,    I18n.T(" 禁止闪烁", " No Flicker"), ToggleStyle ?? GUI.skin.toggle);
        bool hlS = GUI.Toggle(R(c2Sub, y2, subTogW, ROW_H), s.HL_CastShadows, I18n.T(" 投射阴影", " Shadows"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.HL_NoFlicker) { s.HL_NoFlicker = b; changed = true; }
        if (hlS != s.HL_CastShadows) { s.HL_CastShadows = hlS; changed = true; }
        y2 += ROW_ADV;
        b = GUI.Toggle(R(c2, y2, subTogW, ROW_H), s.HL_LightAudio, I18n.T(" 灯光音效", " Audio"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.HL_LightAudio) { s.HL_LightAudio = b; changed = true; }
        y2 += ROW_ADV;
        v = DrawSliderCol(R, c2, y2, I18n.T("亮度", "Intensity"), s.HL_Intensity, 0f, 3f, "F1");
        if (v != s.HL_Intensity) { s.HL_Intensity = v; changed = true; }
        y2 += ROW_ADV;
        v = DrawSliderCol(R, c2, y2, I18n.T("范围倍率", "Range"), s.HL_RangeMultiplier, 0f, 5f, "F1");
        if (v != s.HL_RangeMultiplier) { s.HL_RangeMultiplier = v; changed = true; }
        y2 += ROW_ADV;
        v = DrawSliderCol(R, c2, y2, I18n.T("裁剪距离(m)", "Cull Dist"), s.HL_CullDistance, 10f, 75f, "F0");
        if (v != s.HL_CullDistance) { s.HL_CullDistance = v; changed = true; }
        y2 += ROW_ADV;
        v = DrawSliderCol(R, c2, y2, I18n.T("交互距离(m)", "Interact"), s.HL_InteractDistance, 1f, 3f, "F1");
        if (v != s.HL_InteractDistance) { s.HL_InteractDistance = v; changed = true; }
        y2 += ROW_ADV;
        GUI.Label(R(c2, y2, COL_W, ROW_H), I18n.T("  阴影=灯光投影(吃显卡); 裁剪=超距灯关闭(省性能,降到20可缓解卡顿)", "  shadows=dynamic(GPU heavy); cull=beyond this distance lights off(lower=faster)"), MutedLabel ?? GUI.skin.label);
        y2 += ROW_ADV + SECTION_END_ADV;

        if (changed) s.Save();
        return Mathf.Max(y1, y2) + 8f;
    }
}
