using System;
using UnityEngine;

namespace TldHacks;

internal static partial class MenuTweaks
{
    internal static float DrawLightTab(Func<float, float, float, float, Rect> R, TldHacksSettings s)
    {
        float y1 = 6f, y2 = 6f;
        float x1 = 10f;
        float x2 = x1 + COL_W + 10f;
        bool changed = false;
        float v;

        // ══════ LEFT COLUMN ══════

        y1 = SectionCol(R, x1, y1, COL_W, I18n.T("TorchTweaker / 火把调整", "TorchTweaker / Torch"));

        v = DrawSliderCol(R, x1, y1, I18n.T("燃烧时间(分)", "Burn Time"), s.TorchBurnMinutes, 1f, 2880f, "F0");
        if (v != s.TorchBurnMinutes) { s.TorchBurnMinutes = v; changed = true; }
        y1 += ROW_ADV;

        v = DrawSliderCol(R, x1, y1, I18n.T("最小状态", "Min Condition"), s.TorchMinCondition, 0f, 1f, "F2");
        if (v != s.TorchMinCondition) { s.TorchMinCondition = v; changed = true; }
        y1 += ROW_ADV;

        v = DrawSliderCol(R, x1, y1, I18n.T("最大状态", "Max Condition"), s.TorchMaxCondition, 0f, 1f, "F2");
        if (v != s.TorchMaxCondition) { s.TorchMaxCondition = v; changed = true; }
        y1 += ROW_ADV + SECTION_END_ADV;

        y1 = SectionCol(R, x1, y1, COL_W, I18n.T("KeroseneLampTweaks / 防误灭", "KeroseneLampTweaks / Anti-Misclick"));

        bool dtlc = GUI.Toggle(R(x1, y1, COL_TOG_W, ROW_H), s.DisableTorchLeftClick,
            I18n.T(" 禁用左键灭火把", " No Left-Click Torch"), ToggleStyle ?? GUI.skin.toggle);
        if (dtlc != s.DisableTorchLeftClick) { s.DisableTorchLeftClick = dtlc; CheatState.DisableTorchLeftClick = dtlc; changed = true; }
        y1 += ROW_ADV;

        bool dllc = GUI.Toggle(R(x1, y1, COL_TOG_W, ROW_H), s.DisableLampLeftClick,
            I18n.T(" 禁用左键灭油灯", " No Left-Click Lamp"), ToggleStyle ?? GUI.skin.toggle);
        if (dllc != s.DisableLampLeftClick) { s.DisableLampLeftClick = dllc; CheatState.DisableLampLeftClick = dllc; changed = true; }
        y1 += ROW_ADV + SECTION_END_ADV;

        // ══════ RIGHT COLUMN ══════

        y2 = SectionCol(R, x2, y2, COL_W, I18n.T("KeroseneLamp DIY v2.4.1", "KeroseneLamp DIY v2.4.1"));

        v = DrawSliderCol(R, x2, y2, I18n.T("放置消耗率", "Placed Burn"), s.LampPlacedBurnMult, 0f, 2f, "F2");
        if (v != s.LampPlacedBurnMult) { s.LampPlacedBurnMult = v; changed = true; }
        y2 += ROW_ADV;

        v = DrawSliderCol(R, x2, y2, I18n.T("手持消耗率", "Held Burn"), s.LampHeldBurnMult, 0f, 2f, "F2");
        if (v != s.LampHeldBurnMult) { s.LampHeldBurnMult = v; changed = true; }
        y2 += ROW_ADV;

        GUI.Label(R(x2, y2, COL_W, ROW_H),
            I18n.T("  1=原版(4h) 0=无限 2=两倍消耗", "  1=vanilla(4h) 0=infinite 2=2x"),
            MutedLabel ?? GUI.skin.label);
        y2 += ROW_ADV;

        v = DrawSliderCol(R, x2, y2, I18n.T("开启耗损 %", "TurnOn Decay"), s.LampTurnOnDecay, 0f, 2f, "F2");
        if (v != s.LampTurnOnDecay) { s.LampTurnOnDecay = v; changed = true; }
        y2 += ROW_ADV;

        v = DrawSliderCol(R, x2, y2, I18n.T("持续耗损 %/h", "OverTime Decay"), s.LampOverTimeDecay, 0f, 1f, "F2");
        if (v != s.LampOverTimeDecay) { s.LampOverTimeDecay = v; changed = true; }
        y2 += ROW_ADV;

        v = DrawSliderCol(R, x2, y2, I18n.T("惩罚阈值 HP%", "Penalty Thresh"), (float)s.LampConditionThreshold, 0f, 100f, "F0");
        if ((int)v != s.LampConditionThreshold) { s.LampConditionThreshold = (int)v; changed = true; }
        y2 += ROW_ADV;

        v = DrawSliderCol(R, x2, y2, I18n.T("最大惩罚 %", "Max Penalty"), (float)s.LampMaxPenalty, 0f, 200f, "F0");
        if ((int)v != s.LampMaxPenalty) { s.LampMaxPenalty = (int)v; changed = true; }
        y2 += ROW_ADV;

        v = DrawSliderCol(R, x2, y2, I18n.T("光照范围倍率", "Light Range"), s.LampRangeMultiplier, 0.5f, 5f, "F2");
        if (v != s.LampRangeMultiplier) { s.LampRangeMultiplier = v; changed = true; }
        y2 += ROW_ADV;

        bool lmute = GUI.Toggle(R(x2, y2, COL_TOG_W, ROW_H), s.LampMute,
            I18n.T(" 油灯放置静音", " Lamp Mute (placed)"), ToggleStyle ?? GUI.skin.toggle);
        if (lmute != s.LampMute) { s.LampMute = lmute; CheatState.LampMute = lmute; changed = true; }
        y2 += ROW_ADV;

        y2 += SECTION_END_ADV;

        if (changed) s.Save();
        float yMax = Mathf.Max(y1, y2);
        return yMax + 8f;
    }
}
