using System;
using UnityEngine;

namespace TldHacks;

internal static partial class MenuTweaks
{
    private static bool _qolSleepExpanded = false;
    private static bool _qolSurveyExpanded = false;

    internal static float DrawQoLTab(Func<float, float, float, float, Rect> R, TldHacksSettings s)
    {
        float y1 = 6f, y2 = 6f;
        float x1 = 10f;
        float x2 = x1 + COL_W + 10f;
        bool changed = false;
        float v;
        bool b;

        // ══════ LEFT COLUMN ══════

        y1 = SectionCol(R, x1, y1, COL_W, I18n.T("TinyTweaks-NoSaveOnSprain / 扭伤不存档", "TinyTweaks-NoSaveOnSprain"));

        b = GUI.Toggle(R(x1, y1, COL_TOG_W, ROW_H), s.QoL_NoSaveOnSprain,
            I18n.T(" 扭伤不存档", " No Save On Sprain"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.QoL_NoSaveOnSprain) { s.QoL_NoSaveOnSprain = b; changed = true; }
        y1 += ROW_ADV;

        b = GUI.Toggle(R(x1, y1, COL_TOG_W, ROW_H), s.QoL_NoSaveOnSprainFalls,
            I18n.T(" 坠落扭伤也不存档", " No Save On Fall Sprain"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.QoL_NoSaveOnSprainFalls) { s.QoL_NoSaveOnSprainFalls = b; changed = true; }
        y1 += ROW_ADV;

        y1 += SECTION_END_ADV;
        y1 = SectionCol(R, x1, y1, COL_W, I18n.T("TinyTweaks-WakeUpCall / 唤醒&极光&睡眠", "TinyTweaks-WakeUpCall"));

        b = GUI.Toggle(R(x1, y1, COL_TOG_W, ROW_H), s.QoL_WakeUpCall,
            I18n.T(" 唤醒提示(日出)", " Wake Up Call"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.QoL_WakeUpCall) { s.QoL_WakeUpCall = b; changed = true; }
        y1 += ROW_ADV;

        b = GUI.Toggle(R(x1, y1, COL_TOG_W, ROW_H), s.QoL_AuroraSense,
            I18n.T(" 极光感知提示", " Aurora Sense"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.QoL_AuroraSense) { s.QoL_AuroraSense = b; changed = true; }
        y1 += ROW_ADV;

        b = GUI.Toggle(R(x1, y1, COL_TOG_W, ROW_H), s.QoL_ShowTimeSleep,
            I18n.T(" 睡眠显示时间", " Show Time in Sleep"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.QoL_ShowTimeSleep) { s.QoL_ShowTimeSleep = b; changed = true; }
        y1 += ROW_ADV;

        b = GUI.Toggle(R(x1, y1, COL_TOG_W, ROW_H), s.QoL_NoPitchBlack,
            I18n.T(" 禁止全黑", " No Pitch Black"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.QoL_NoPitchBlack) { s.QoL_NoPitchBlack = b; changed = true; }
        y1 += ROW_ADV;

        y1 += SECTION_END_ADV;
        y1 = SectionCol(R, x1, y1, COL_W, I18n.T("TinyTweaks-MapTextOutline / 地图文字描边", "TinyTweaks-MapTextOutline"));

        b = GUI.Toggle(R(x1, y1, COL_TOG_W, ROW_H), s.QoL_MapTextOutlineEnabled,
            I18n.T(" 地图文字描边", " Map Text Outline"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.QoL_MapTextOutlineEnabled) { s.QoL_MapTextOutlineEnabled = b; changed = true; }
        y1 += ROW_ADV;

        if (s.QoL_MapTextOutlineEnabled)
        {
            v = DrawSliderCol(R, x1, y1, I18n.T("描边粗细(0-3)", "Thickness"), (float)s.QoL_MapTextOutline, 0f, 3f, "F0");
            if ((int)v != s.QoL_MapTextOutline) { s.QoL_MapTextOutline = (int)v; changed = true; }
            y1 += ROW_ADV;

            GUI.Label(R(x1, y1, COL_W, ROW_H),
                I18n.T("  0=无 1=细 2=中 3=粗", "  0=None 1=Thin 2=Med 3=Thick"),
                MutedLabel ?? GUI.skin.label);
            y1 += ROW_ADV;
        }

        y1 += SECTION_END_ADV;
        y1 = SectionCol(R, x1, y1, COL_W, I18n.T("TinyTweaks-BuryCorpses / 埋葬尸体", "TinyTweaks-BuryCorpses"));

        b = GUI.Toggle(R(x1, y1, COL_TOG_W, ROW_H), s.QoL_BuryCorpses,
            I18n.T(" 埋葬尸体", " Bury Corpses"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.QoL_BuryCorpses) { s.QoL_BuryCorpses = b; changed = true; }
        y1 += ROW_ADV;

        y1 += SECTION_END_ADV;

        // ══════ RIGHT COLUMN ══════

        // --- Sleep Anywhere ---
        y2 = SectionCol(R, x2, y2, COL_W, I18n.T("SleepWithoutABed / 随地睡觉", "SleepWithoutABed / Sleep Anywhere"));

        b = GUI.Toggle(R(x2, y2, COL_TOG_W, ROW_H), s.QoL_SleepAnywhere,
            I18n.T(" 启用随地睡", " Enable"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.QoL_SleepAnywhere) { s.QoL_SleepAnywhere = b; changed = true; }
        y2 += ROW_ADV;

        if (GUI.Button(R(x2, y2, 200f, ROW_H), _qolSleepExpanded
            ? I18n.T("▲ 收起睡眠参数", "▲ Hide Sleep")
            : I18n.T("▼ 睡眠参数", "▼ Sleep Params")))
            _qolSleepExpanded = !_qolSleepExpanded;
        y2 += ROW_ADV;

        if (_qolSleepExpanded)
        {
            v = DrawSliderCol(R, x2, y2, I18n.T("疲劳恢复率", "Fatigue Recov"), s.QoL_SleepFatigueRecovery, 0.15f, 1f, "F2");
            if (v != s.QoL_SleepFatigueRecovery) { s.QoL_SleepFatigueRecovery = v; changed = true; }
            y2 += ROW_ADV;

            v = DrawSliderCol(R, x2, y2, I18n.T("状态恢复率", "Condition Recov"), s.QoL_SleepConditionRecovery, 0.05f, 1f, "F2");
            if (v != s.QoL_SleepConditionRecovery) { s.QoL_SleepConditionRecovery = v; changed = true; }
            y2 += ROW_ADV;

            v = DrawSliderCol(R, x2, y2, I18n.T("冻伤系数", "Freezing Scale"), s.QoL_SleepFreezingScale, 1f, 3f, "F2");
            if (v != s.QoL_SleepFreezingScale) { s.QoL_SleepFreezingScale = v; changed = true; }
            y2 += ROW_ADV;

            v = DrawSliderCol(R, x2, y2, I18n.T("冻伤HP损失", "Freeze HP Loss"), s.QoL_SleepFreezingHealthLoss, 1f, 2f, "F2");
            if (v != s.QoL_SleepFreezingHealthLoss) { s.QoL_SleepFreezingHealthLoss = v; changed = true; }
            y2 += ROW_ADV;

            v = DrawSliderCol(R, x2, y2, I18n.T("低温症HP损失", "Hypo HP Loss"), s.QoL_SleepHypothermicHealthLoss, 1f, 2f, "F2");
            if (v != s.QoL_SleepHypothermicHealthLoss) { s.QoL_SleepHypothermicHealthLoss = v; changed = true; }
            y2 += ROW_ADV;

            v = DrawSliderCol(R, x2, y2, I18n.T("消磨时间暴露", "PassTime Expos"), s.QoL_SleepPassTimeExposure, 0.25f, 2f, "F2");
            if (v != s.QoL_SleepPassTimeExposure) { s.QoL_SleepPassTimeExposure = v; changed = true; }
            y2 += ROW_ADV;

            b = GUI.Toggle(R(x2, y2, COL_TOG_W, ROW_H), s.QoL_SleepInterrupt,
                I18n.T(" 低血量中断睡眠", " Low HP Interrupt"), ToggleStyle ?? GUI.skin.toggle);
            if (b != s.QoL_SleepInterrupt) { s.QoL_SleepInterrupt = b; changed = true; }
            y2 += ROW_ADV;

            v = DrawSliderCol(R, x2, y2, I18n.T("中断阈值%", "Interrupt Thr"), s.QoL_SleepInterruptThreshold, 0.05f, 0.2f, "F2");
            if (v != s.QoL_SleepInterruptThreshold) { s.QoL_SleepInterruptThreshold = v; changed = true; }
            y2 += ROW_ADV;

            v = DrawSliderCol(R, x2, y2, I18n.T("中断冷却(秒)", "Cooldown(s)"), s.QoL_SleepInterruptCooldown, 1f, 60f, "F0");
            if (v != s.QoL_SleepInterruptCooldown) { s.QoL_SleepInterruptCooldown = v; changed = true; }
            y2 += ROW_ADV;

            b = GUI.Toggle(R(x2, y2, COL_TOG_W, ROW_H), s.QoL_SleepInterruptHudMsg,
                I18n.T(" 显示HUD提示", " Show HUD Msg"), ToggleStyle ?? GUI.skin.toggle);
            if (b != s.QoL_SleepInterruptHudMsg) { s.QoL_SleepInterruptHudMsg = b; changed = true; }
            y2 += ROW_ADV;

            b = GUI.Toggle(R(x2, y2, COL_TOG_W, ROW_H), s.QoL_SleepInterruptAllBeds,
                I18n.T(" 所有床铺也中断", " All Beds Too"), ToggleStyle ?? GUI.skin.toggle);
            if (b != s.QoL_SleepInterruptAllBeds) { s.QoL_SleepInterruptAllBeds = b; changed = true; }
            y2 += ROW_ADV;

            v = DrawSliderCol(R, x2, y2, I18n.T("暴露灵敏度", "Sensitivity"), s.QoL_SleepSensitivityScale, 0.01f, 1f, "F2");
            if (v != s.QoL_SleepSensitivityScale) { s.QoL_SleepSensitivityScale = v; changed = true; }
            y2 += ROW_ADV;

            v = DrawSliderCol(R, x2, y2, I18n.T("修正灵敏度", "Adj Sensitivity"), s.QoL_SleepAdjustedSensitivity, 0.01f, 2f, "F2");
            if (v != s.QoL_SleepAdjustedSensitivity) { s.QoL_SleepAdjustedSensitivity = v; changed = true; }
            y2 += ROW_ADV;
        }
        y2 += SECTION_END_ADV;

        // --- Auto Survey ---
        y2 = SectionCol(R, x2, y2, COL_W, I18n.T("AutoSurvey / 自动测绘", "AutoSurvey / Auto Mapping"));

        b = GUI.Toggle(R(x2, y2, COL_TOG_W, ROW_H), s.QoL_AutoSurvey,
            I18n.T(" 启用自动测绘", " Enable"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.QoL_AutoSurvey) { s.QoL_AutoSurvey = b; changed = true; }
        y2 += ROW_ADV;

        if (GUI.Button(R(x2, y2, 200f, ROW_H), _qolSurveyExpanded
            ? I18n.T("▲ 收起测绘参数", "▲ Hide Survey")
            : I18n.T("▼ 测绘参数", "▼ Survey Params")))
            _qolSurveyExpanded = !_qolSurveyExpanded;
        y2 += ROW_ADV;

        if (_qolSurveyExpanded)
        {
            v = DrawSliderCol(R, x2, y2, I18n.T("延迟(秒)", "Delay"), s.QoL_AutoSurveyDelay, 1f, 60f, "F0");
            if (v != s.QoL_AutoSurveyDelay) { s.QoL_AutoSurveyDelay = v; changed = true; }
            y2 += ROW_ADV;

            v = DrawSliderCol(R, x2, y2, I18n.T("范围倍率", "Range"), s.QoL_AutoSurveyRange, 0.1f, 5f, "F1");
            if (v != s.QoL_AutoSurveyRange) { s.QoL_AutoSurveyRange = v; changed = true; }
            y2 += ROW_ADV;

            b = GUI.Toggle(R(x2, y2, COL_TOG_W, ROW_H), s.QoL_AutoSurveyUnlock,
                I18n.T(" 全图解锁", " Unlock All"), ToggleStyle ?? GUI.skin.toggle);
            if (b != s.QoL_AutoSurveyUnlock) { s.QoL_AutoSurveyUnlock = b; changed = true; }
            y2 += ROW_ADV;
        }
        y2 += SECTION_END_ADV;

        // --- v3.0.4 PlaceFromInventory ---
        y2 = SectionCol(R, x2, y2, COL_W, I18n.T("PlaceFromInv / 微操放置 v1.1.3", "PlaceFromInv v1.1.3"));
        b = GUI.Toggle(R(x2, y2, COL_TOG_W, ROW_H), s.PlaceFromInv_Enabled,
            I18n.T(" 启用背包右键放置", " Enable Right-Click Place"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.PlaceFromInv_Enabled) { s.PlaceFromInv_Enabled = b; changed = true; }
        y2 += ROW_ADV;
        b = GUI.Toggle(R(x2, y2, COL_TOG_W, ROW_H), s.PlaceFromInv_AllowClose,
            I18n.T(" 允许贴近放置", " Allow Close Place"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.PlaceFromInv_AllowClose) { s.PlaceFromInv_AllowClose = b; changed = true; }
        y2 += ROW_ADV;
        b = GUI.Toggle(R(x2, y2, COL_TOG_W, ROW_H), s.PlaceFromInv_StackDrop,
            I18n.T(" Ctrl 整堆丢出", " Ctrl=Drop Stack"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.PlaceFromInv_StackDrop) { s.PlaceFromInv_StackDrop = b; changed = true; }
        y2 += ROW_ADV + SECTION_END_ADV;

        if (changed) s.Save();
        float yMax = Mathf.Max(y1, y2);
        return yMax + 8f;
    }
}
