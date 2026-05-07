using System;
using UnityEngine;

namespace TldHacks;

internal static partial class MenuTweaks
{
    // Styles set by Menu.cs during InitStyles
    internal static GUIStyle SectionBox;
    internal static GUIStyle AccentBar;
    internal static GUIStyle LabelStyle;
    internal static GUIStyle MutedLabel;
    internal static GUIStyle SectionTitle;
    internal static GUIStyle ToggleStyle;

    // Collapse states for detail sections
    private static bool _foodDetailExpanded = false;
    private static bool _gearDetailExpanded = false;

    // Layout — dynamically tracks Menu window width
    private static float BOX_W => Menu.ContentW;
    private const float SEC_H = 30f;
    private const float SEC_ADV = 38f;
    private const float ROW_H = 26f;
    private const float ROW_ADV = 30f;
    private const float SECTION_END_ADV = 14f;
    private const float LBL_W = 180f;
    private static float SLD_W => Menu.ContentW - LBL_W - VAL_W - 30f;
    private const float VAL_W = 60f;
    private static float TOG_W => Menu.ContentW;

    // ——— Helpers ———

    private static float Section(Func<float, float, float, float, Rect> R, float x, float y, string title)
    {
        GUI.Box(R(x, y, BOX_W, SEC_H), "", SectionBox ?? GUI.skin.box);
        GUI.Box(R(x, y, 3f, SEC_H), "", AccentBar ?? GUI.skin.button);
        GUI.Label(R(x + 10f, y + 4f, BOX_W - 20f, SEC_H - 8f), title, SectionTitle ?? GUI.skin.label);
        return y + SEC_ADV;
    }

    private static float DrawSlider(Func<float, float, float, float, Rect> R, float x, float y,
        string label, float value, float min, float max, string format = "F2")
    {
        GUI.Label(R(x, y, LBL_W, ROW_H), label, LabelStyle ?? GUI.skin.label);
        float newVal = GUI.HorizontalSlider(R(x + LBL_W, y + 8f, SLD_W, 12f), value, min, max);
        GUI.Label(R(x + LBL_W + SLD_W + 8f, y, VAL_W, ROW_H), newVal.ToString(format), MutedLabel ?? GUI.skin.label);
        return newVal;
    }

    // 2-column helpers
    internal static float COL_W => (Menu.ContentW - 20f) / 2f;
    internal static float COL_SLD_W => COL_W - LBL_W - VAL_W - 20f;
    internal static float COL_TOG_W => COL_W - 10f;

    // 3-column helpers
    internal static float COL_W3 => (Menu.ContentW - 30f) / 3f;
    internal static float COL_SLD_W3 => COL_W3 - LBL_W - VAL_W - 20f;
    internal static float COL_TOG_W3 => COL_W3 - 10f;

    internal static float SectionCol(Func<float, float, float, float, Rect> R, float x, float y, float w, string title)
    {
        GUI.Box(R(x, y, w, SEC_H), "", SectionBox ?? GUI.skin.box);
        GUI.Box(R(x, y, 3f, SEC_H), "", AccentBar ?? GUI.skin.button);
        GUI.Label(R(x + 10f, y + 4f, w - 20f, SEC_H - 8f), title, SectionTitle ?? GUI.skin.label);
        return y + SEC_ADV;
    }

    internal static float DrawSliderCol(Func<float, float, float, float, Rect> R, float x, float y,
        string label, float value, float min, float max, string format = "F2")
    {
        GUI.Label(R(x, y, LBL_W, ROW_H), label, LabelStyle ?? GUI.skin.label);
        float newVal = GUI.HorizontalSlider(R(x + LBL_W, y + 8f, COL_SLD_W, 12f), value, min, max);
        GUI.Label(R(x + LBL_W + COL_SLD_W + 8f, y, VAL_W, ROW_H), newVal.ToString(format), MutedLabel ?? GUI.skin.label);
        return newVal;
    }

    internal static float DrawSliderCol3(Func<float, float, float, float, Rect> R, float x, float y,
        string label, float value, float min, float max, string format = "F2")
    {
        GUI.Label(R(x, y, LBL_W, ROW_H), label, LabelStyle ?? GUI.skin.label);
        float newVal = GUI.HorizontalSlider(R(x + LBL_W, y + 8f, COL_SLD_W3, 12f), value, min, max);
        GUI.Label(R(x + LBL_W + COL_SLD_W3 + 8f, y, VAL_W, ROW_H), newVal.ToString(format), MutedLabel ?? GUI.skin.label);
        return newVal;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Tab 2: 人物 & 生存 — 3 columns: Character | QoL base | QoL advanced
    // ═══════════════════════════════════════════════════════════════════════
    internal static float DrawCharacterAndQoLTab(Func<float, float, float, float, Rect> R, TldHacksSettings s)
    {
        float c1 = 0f, c2 = COL_W3 + 15f, c3 = 2f * (COL_W3 + 15f);
        float y1 = 6f, y2 = 6f, y3 = 6f;
        bool changed = false;
        float v;

        // ——— Column 2: 交互 & 感知 + 生活品质 + 车辆 ———

        y2 = SectionCol(R, c2, y2, COL_W3, I18n.T("交互 & 感知", "Interaction & Senses"));
        v = DrawSliderCol3(R, c2, y2, I18n.T("交互距离倍率", "Pickup Range"), s.PickupRange, 1f, 5f, "F1");
        if (v != s.PickupRange) { s.PickupRange = v; changed = true; }
        y2 += ROW_ADV;
        bool silent = GUI.Toggle(R(c2, y2, COL_TOG_W3, ROW_H), s.SilentFootsteps,
            I18n.T(" 脚步静音", " Silent Footsteps"), ToggleStyle ?? GUI.skin.toggle);
        if (silent != s.SilentFootsteps) { s.SilentFootsteps = silent; CheatState.SilentFootsteps = silent; changed = true; }
        y2 += ROW_ADV;
        bool gunZoom = GUI.Toggle(R(c2, y2, COL_TOG_W3, ROW_H), s.GunZoomEnabled,
            I18n.T(" 瞄准滚轮缩放", " Scroll Zoom"), ToggleStyle ?? GUI.skin.toggle);
        if (gunZoom != s.GunZoomEnabled) { s.GunZoomEnabled = gunZoom; changed = true; }
        y2 += ROW_ADV;
        bool rwl = GUI.Toggle(R(c2, y2, COL_TOG_W3, ROW_H), s.RunWithLantern,
            I18n.T(" 拿油灯可跑步", " Run w/ Lantern"), ToggleStyle ?? GUI.skin.toggle);
        if (rwl != s.RunWithLantern) { s.RunWithLantern = rwl; CheatState.RunWithLantern = rwl; changed = true; }
        y2 += ROW_ADV;
        bool noCharcoal = GUI.Toggle(R(c2, y2, COL_TOG_W3, ROW_H), s.NoAutoEquipCharcoal,
            I18n.T(" 取炭不装备", " No Auto-Equip Charcoal"), ToggleStyle ?? GUI.skin.toggle);
        if (noCharcoal != s.NoAutoEquipCharcoal) { s.NoAutoEquipCharcoal = noCharcoal; CheatState.NoAutoEquipCharcoal = noCharcoal; changed = true; }
        y2 += ROW_ADV;
        bool autoExt = GUI.Toggle(R(c2, y2, COL_TOG_W3, ROW_H), s.AutoExtinguishOnRest,
            I18n.T(" 休息自动熄灯", " Auto-Extinguish"), ToggleStyle ?? GUI.skin.toggle);
        if (autoExt != s.AutoExtinguishOnRest) { s.AutoExtinguishOnRest = autoExt; CheatState.AutoExtinguishOnRest = autoExt; changed = true; }
        y2 += ROW_ADV + SECTION_END_ADV;

        y2 = SectionCol(R, c2, y2, COL_W3, I18n.T("生活品质", "Quality of Life"));
        bool pauseJ = GUI.Toggle(R(c2, y2, COL_TOG_W3, ROW_H), s.PauseInJournal,
            I18n.T(" 开日志暂停游戏", " Pause in Journal"), ToggleStyle ?? GUI.skin.toggle);
        if (pauseJ != s.PauseInJournal) { s.PauseInJournal = pauseJ; changed = true; }
        y2 += ROW_ADV;
        bool skipI = GUI.Toggle(R(c2, y2, COL_TOG_W3, ROW_H), s.SkipIntro,
            I18n.T(" 跳过开场动画", " Skip Intro"), ToggleStyle ?? GUI.skin.toggle);
        if (skipI != s.SkipIntro) { s.SkipIntro = skipI; changed = true; }
        y2 += ROW_ADV;
        bool muteCougar = GUI.Toggle(R(c2, y2, COL_TOG_W3, ROW_H), s.MuteCougarMenuSound,
            I18n.T(" 静音主菜单美洲狮", " Mute Cougar Menu"), ToggleStyle ?? GUI.skin.toggle);
        if (muteCougar != s.MuteCougarMenuSound) { s.MuteCougarMenuSound = muteCougar; changed = true; }
        y2 += ROW_ADV;
        bool droppable = GUI.Toggle(R(c2, y2, COL_TOG_W3, ROW_H), s.DroppableUndroppables,
            I18n.T(" 允许丢弃不可丢物品", " Droppable Undroppables"), ToggleStyle ?? GUI.skin.toggle);
        if (droppable != s.DroppableUndroppables) { s.DroppableUndroppables = droppable; changed = true; }
        y2 += ROW_ADV;
        bool impW = GUI.Toggle(R(c2, y2, COL_TOG_W3, ROW_H), s.ImportantWeight,
            I18n.T(" 关键物品保留重量", " Keep Key Weight"), ToggleStyle ?? GUI.skin.toggle);
        if (impW != s.ImportantWeight) { s.ImportantWeight = impW; changed = true; }
        y2 += ROW_ADV;
        bool allNote = GUI.Toggle(R(c2, y2, COL_TOG_W3, ROW_H), s.AllNote,
            I18n.T(" 收藏品可丢弃", " Droppable Collectibles"), ToggleStyle ?? GUI.skin.toggle);
        if (allNote != s.AllNote) { s.AllNote = allNote; changed = true; }
        y2 += ROW_ADV;
        bool remTool = GUI.Toggle(R(c2, y2, COL_TOG_W3, ROW_H), s.RememberBreakdownTool,
            I18n.T(" 记忆拆解工具", " Remember Tool"), ToggleStyle ?? GUI.skin.toggle);
        if (remTool != s.RememberBreakdownTool) { s.RememberBreakdownTool = remTool; changed = true; }
        y2 += ROW_ADV + SECTION_END_ADV;

        y2 = SectionCol(R, c2, y2, COL_W3, I18n.T("车辆视角", "Vehicle"));
        bool vFov = GUI.Toggle(R(c2, y2, COL_TOG_W3, ROW_H), s.VehicleKeepFov,
            I18n.T(" 车内保持玩家FOV", " Keep FOV"), ToggleStyle ?? GUI.skin.toggle);
        if (vFov != s.VehicleKeepFov) { s.VehicleKeepFov = vFov; changed = true; }
        y2 += ROW_ADV;
        bool vFree = GUI.Toggle(R(c2, y2, COL_TOG_W3, ROW_H), s.VehicleFreeLook,
            I18n.T(" 车内自由视角", " Free Look"), ToggleStyle ?? GUI.skin.toggle);
        if (vFree != s.VehicleFreeLook) { s.VehicleFreeLook = vFree; changed = true; }
        y2 += ROW_ADV;
        v = DrawSliderCol3(R, c2, y2, I18n.T("水平角度", "Yaw"), s.VehicleFreeLookYaw, 30f, 180f, "F0");
        if (v != s.VehicleFreeLookYaw) { s.VehicleFreeLookYaw = v; changed = true; }
        y2 += ROW_ADV;
        v = DrawSliderCol3(R, c2, y2, I18n.T("俯仰角度", "Pitch"), s.VehicleFreeLookPitch, 20f, 90f, "F0");
        if (v != s.VehicleFreeLookPitch) { s.VehicleFreeLookPitch = v; changed = true; }
        y2 += ROW_ADV + SECTION_END_ADV;

        // ——— Column 1: 速度体力 + 跳跃 + 开门 + 时间加速 ———

        y1 = SectionCol(R, c1, y1, COL_W3, I18n.T("移动 & 体力", "Movement & Stamina"));
        bool speedOn = GUI.Toggle(R(c1, y1, COL_TOG_W3, ROW_H), s.SpeedTweaksEnabled,
            I18n.T(" 启用速度调节", " Enable Speed Tweaks"), ToggleStyle ?? GUI.skin.toggle);
        if (speedOn != s.SpeedTweaksEnabled) { s.SpeedTweaksEnabled = speedOn; changed = true; }
        y1 += ROW_ADV;
        GUI.Label(R(c1, y1, COL_W3, ROW_H),
            I18n.T("  1.0=原版速度", "  1.0 = vanilla speed"), MutedLabel ?? GUI.skin.label);
        y1 += ROW_ADV;
        v = DrawSliderCol3(R, c1, y1, I18n.T("蹲行", "Crouch"), s.CrouchSpeed, 0.5f, 5f, "F2");
        if (v != s.CrouchSpeed) { s.CrouchSpeed = v; changed = true; }
        y1 += ROW_ADV;
        v = DrawSliderCol3(R, c1, y1, I18n.T("步行", "Walk"), s.WalkSpeed, 0.5f, 5f, "F2");
        if (v != s.WalkSpeed) { s.WalkSpeed = v; changed = true; }
        y1 += ROW_ADV;
        v = DrawSliderCol3(R, c1, y1, I18n.T("冲刺", "Sprint"), s.SprintSpeed, 0.5f, 5f, "F2");
        if (v != s.SprintSpeed) { s.SprintSpeed = v; changed = true; }
        y1 += ROW_ADV;
        v = DrawSliderCol3(R, c1, y1, I18n.T("体力恢复", "Stamina Regen"), s.StaminaRecharge, 0.5f, 10f, "F1");
        if (v != s.StaminaRecharge) { s.StaminaRecharge = v; changed = true; }
        y1 += ROW_ADV;
        v = DrawSliderCol3(R, c1, y1, I18n.T("体力消耗", "Stamina Drain"), s.StaminaDrain, 0.1f, 2f, "F2");
        if (v != s.StaminaDrain) { s.StaminaDrain = v; changed = true; }
        y1 += ROW_ADV;
        v = DrawSliderCol3(R, c1, y1, I18n.T("恢复延迟", "Recovery Delay"), s.StaminaRecoveryDelay, 0f, 2f, "F2");
        if (v != s.StaminaRecoveryDelay) { s.StaminaRecoveryDelay = v; changed = true; }
        y1 += ROW_ADV + SECTION_END_ADV;

        y1 = SectionCol(R, c1, y1, COL_W3, I18n.T("跳跃", "Jump"));
        bool je = GUI.Toggle(R(c1, y1, COL_TOG_W3, ROW_H), s.JumpEnabled,
            I18n.T(" 启用跳跃", " Enable Jump"), ToggleStyle ?? GUI.skin.toggle);
        if (je != s.JumpEnabled) { s.JumpEnabled = je; changed = true; }
        y1 += ROW_ADV;
        v = DrawSliderCol3(R, c1, y1, I18n.T("跳跃高度", "Height"), s.JumpHeight, 15f, 42f, "F0");
        if (v != s.JumpHeight) { s.JumpHeight = v; changed = true; }
        y1 += ROW_ADV;
        bool jk = GUI.Toggle(R(c1, y1, COL_TOG_W3, ROW_H), s.JumpKing,
            I18n.T(" 无限制跳", " Jump King"), ToggleStyle ?? GUI.skin.toggle);
        if (jk != s.JumpKing) { s.JumpKing = jk; changed = true; }
        y1 += ROW_ADV;
        v = DrawSliderCol3(R, c1, y1, I18n.T("负重上限(kg)", "Weight Limit"), s.JumpWeightLimit, 10f, 50f, "F0");
        if (v != s.JumpWeightLimit) { s.JumpWeightLimit = v; changed = true; }
        y1 += ROW_ADV;
        v = DrawSliderCol3(R, c1, y1, I18n.T("卡路里/次", "Cal Cost"), s.JumpCalorieCost, 0f, 50f, "F0");
        if (v != s.JumpCalorieCost) { s.JumpCalorieCost = v; changed = true; }
        y1 += ROW_ADV;
        v = DrawSliderCol3(R, c1, y1, I18n.T("体力/次", "Stam Cost"), s.JumpStaminaCost, 0f, 20f, "F0");
        if (v != s.JumpStaminaCost) { s.JumpStaminaCost = v; changed = true; }
        y1 += ROW_ADV;
        v = DrawSliderCol3(R, c1, y1, I18n.T("疲劳/次", "Fatigue Cost"), s.JumpFatigueCost, 0f, 10f, "F1");
        if (v != s.JumpFatigueCost) { s.JumpFatigueCost = v; changed = true; }
        y1 += ROW_ADV;
        GUI.Label(R(c1, y1, COL_W3, ROW_H),
            I18n.T($"  热键=[{s.JumpKey}]", $"  Key=[{s.JumpKey}]"), MutedLabel ?? GUI.skin.label);
        y1 += ROW_ADV + SECTION_END_ADV;

        y1 = SectionCol(R, c1, y1, COL_W3, I18n.T("开门角度", "Door Swing"));
        v = DrawSliderCol3(R, c1, y1, I18n.T("开门角度", "Angle"), s.DoorSwingAngle, 0.03f, 0.6f, "F2");
        if (v != s.DoorSwingAngle) { s.DoorSwingAngle = v; changed = true; }
        y1 += ROW_ADV;
        v = DrawSliderCol3(R, c1, y1, I18n.T("开门速度", "Speed"), s.DoorSwingSpeed, 0f, 1f, "F2");
        if (v != s.DoorSwingSpeed) { s.DoorSwingSpeed = v; changed = true; }
        y1 += ROW_ADV + SECTION_END_ADV;

        y1 = SectionCol(R, c1, y1, COL_W3, I18n.T("时间加速", "Time Scale"));
        v = DrawSliderCol3(R, c1, y1, I18n.T("热键1 倍速", "Key1 Speed"), s.TimeScale1, 1f, 50f, "F0");
        if (v != s.TimeScale1) { s.TimeScale1 = v; changed = true; }
        y1 += ROW_ADV;
        v = DrawSliderCol3(R, c1, y1, I18n.T("热键2 倍速", "Key2 Speed"), s.TimeScale2, 1f, 50f, "F0");
        if (v != s.TimeScale2) { s.TimeScale2 = v; changed = true; }
        y1 += ROW_ADV;
        GUI.Label(R(c1, y1, COL_W3, ROW_H),
            I18n.T($"  [{s.TimeScaleKey1}] [{s.TimeScaleKey2}] (ModSettings)",
                   $"  [{s.TimeScaleKey1}] [{s.TimeScaleKey2}] (ModSettings)"),
            MutedLabel ?? GUI.skin.label);
        y1 += ROW_ADV + SECTION_END_ADV;

        // ——— Column 3: 睡眠 + 测绘 + 地图描边 + 埋尸 + QoL 开关 ———

        y3 = SectionCol(R, c3, y3, COL_W3, I18n.T("随地睡觉", "Sleep Anywhere"));
        bool sleepOn = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.QoL_SleepAnywhere,
            I18n.T(" 启用随地睡", " Enable"), ToggleStyle ?? GUI.skin.toggle);
        if (sleepOn != s.QoL_SleepAnywhere) { s.QoL_SleepAnywhere = sleepOn; changed = true; }
        y3 += ROW_ADV;
        if (GUI.Button(R(c3, y3, 160f, ROW_H), _qolSleepExpanded
            ? I18n.T("▲ 收起", "▲ Hide")
            : I18n.T("▼ 睡眠参数", "▼ Sleep Params")))
            _qolSleepExpanded = !_qolSleepExpanded;
        y3 += ROW_ADV;
        if (_qolSleepExpanded)
        {
            v = DrawSliderCol3(R, c3, y3, I18n.T("疲劳恢复率", "Fatigue Recov"), s.QoL_SleepFatigueRecovery, 0f, 2f, "F2");
            if (v != s.QoL_SleepFatigueRecovery) { s.QoL_SleepFatigueRecovery = v; changed = true; }
            y3 += ROW_ADV;
            v = DrawSliderCol3(R, c3, y3, I18n.T("状态恢复率", "Cond Recov"), s.QoL_SleepConditionRecovery, 0f, 2f, "F2");
            if (v != s.QoL_SleepConditionRecovery) { s.QoL_SleepConditionRecovery = v; changed = true; }
            y3 += ROW_ADV;
            v = DrawSliderCol3(R, c3, y3, I18n.T("冻伤系数", "Freeze Scale"), s.QoL_SleepFreezingScale, 0f, 5f, "F2");
            if (v != s.QoL_SleepFreezingScale) { s.QoL_SleepFreezingScale = v; changed = true; }
            y3 += ROW_ADV;
            bool intF = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.QoL_SleepInterrupt,
                I18n.T(" 冻醒机制", " Freeze Interrupt"), ToggleStyle ?? GUI.skin.toggle);
            if (intF != s.QoL_SleepInterrupt) { s.QoL_SleepInterrupt = intF; changed = true; }
            y3 += ROW_ADV;
            v = DrawSliderCol3(R, c3, y3, I18n.T("冻醒阈值", "Interrupt Thr"), s.QoL_SleepInterruptThreshold, 0f, 0.5f, "F2");
            if (v != s.QoL_SleepInterruptThreshold) { s.QoL_SleepInterruptThreshold = v; changed = true; }
            y3 += ROW_ADV;
        }
        y3 += SECTION_END_ADV;

        y3 = SectionCol(R, c3, y3, COL_W3, I18n.T("自动测绘", "Auto Survey"));
        bool surveyOn = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.QoL_AutoSurvey,
            I18n.T(" 启用自动测绘", " Enable"), ToggleStyle ?? GUI.skin.toggle);
        if (surveyOn != s.QoL_AutoSurvey) { s.QoL_AutoSurvey = surveyOn; changed = true; }
        y3 += ROW_ADV;
        if (GUI.Button(R(c3, y3, 160f, ROW_H), _qolSurveyExpanded
            ? I18n.T("▲ 收起", "▲ Hide")
            : I18n.T("▼ 测绘参数", "▼ Survey Params")))
            _qolSurveyExpanded = !_qolSurveyExpanded;
        y3 += ROW_ADV;
        if (_qolSurveyExpanded)
        {
            v = DrawSliderCol3(R, c3, y3, I18n.T("延迟(秒)", "Delay"), s.QoL_AutoSurveyDelay, 1f, 60f, "F0");
            if (v != s.QoL_AutoSurveyDelay) { s.QoL_AutoSurveyDelay = v; changed = true; }
            y3 += ROW_ADV;
            v = DrawSliderCol3(R, c3, y3, I18n.T("范围倍率", "Range"), s.QoL_AutoSurveyRange, 0.1f, 5f, "F1");
            if (v != s.QoL_AutoSurveyRange) { s.QoL_AutoSurveyRange = v; changed = true; }
            y3 += ROW_ADV;
            bool unl = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.QoL_AutoSurveyUnlock,
                I18n.T(" 全图解锁", " Unlock All"), ToggleStyle ?? GUI.skin.toggle);
            if (unl != s.QoL_AutoSurveyUnlock) { s.QoL_AutoSurveyUnlock = unl; changed = true; }
            y3 += ROW_ADV;
        }
        y3 += SECTION_END_ADV;

        y3 = SectionCol(R, c3, y3, COL_W3, I18n.T("杂项 (描边·扭伤不存·埋尸)", "Misc"));
        bool mapOn = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.QoL_MapTextOutlineEnabled,
            I18n.T(" 地图文字描边", " Map Text Outline"), ToggleStyle ?? GUI.skin.toggle);
        if (mapOn != s.QoL_MapTextOutlineEnabled) { s.QoL_MapTextOutlineEnabled = mapOn; changed = true; }
        y3 += ROW_ADV;
        bool noSave = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.QoL_NoSaveOnSprain,
            I18n.T(" 扭伤不存档", " No Save On Sprain"), ToggleStyle ?? GUI.skin.toggle);
        if (noSave != s.QoL_NoSaveOnSprain) { s.QoL_NoSaveOnSprain = noSave; changed = true; }
        y3 += ROW_ADV;
        bool noFallSpr = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.QoL_NoSaveOnSprainFalls,
            I18n.T("   含坠落扭伤", "   Include Falls"), ToggleStyle ?? GUI.skin.toggle);
        if (noFallSpr != s.QoL_NoSaveOnSprainFalls) { s.QoL_NoSaveOnSprainFalls = noFallSpr; changed = true; }
        y3 += ROW_ADV;
        bool bury = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.QoL_BuryCorpses,
            I18n.T(" 埋葬尸体", " Bury Corpses"), ToggleStyle ?? GUI.skin.toggle);
        if (bury != s.QoL_BuryCorpses) { s.QoL_BuryCorpses = bury; changed = true; }
        y3 += ROW_ADV;
        bool wakeUp = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.QoL_WakeUpCall,
            I18n.T(" 唤醒提示(日出)", " Wake Up Call"), ToggleStyle ?? GUI.skin.toggle);
        if (wakeUp != s.QoL_WakeUpCall) { s.QoL_WakeUpCall = wakeUp; changed = true; }
        y3 += ROW_ADV;
        bool aurora = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.QoL_AuroraSense,
            I18n.T(" 极光感知提示", " Aurora Sense"), ToggleStyle ?? GUI.skin.toggle);
        if (aurora != s.QoL_AuroraSense) { s.QoL_AuroraSense = aurora; changed = true; }
        y3 += ROW_ADV;
        bool showT = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.QoL_ShowTimeSleep,
            I18n.T(" 睡眠显示时间", " Show Time Sleep"), ToggleStyle ?? GUI.skin.toggle);
        if (showT != s.QoL_ShowTimeSleep) { s.QoL_ShowTimeSleep = showT; changed = true; }
        y3 += ROW_ADV;
        bool noPB = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.QoL_NoPitchBlack,
            I18n.T(" 禁止全黑", " No Pitch Black"), ToggleStyle ?? GUI.skin.toggle);
        if (noPB != s.QoL_NoPitchBlack) { s.QoL_NoPitchBlack = noPB; changed = true; }
        y3 += ROW_ADV;
        y3 += SECTION_END_ADV;


        if (changed) s.Save();
        return Mathf.Max(y1, Mathf.Max(y2, y3));
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Tab 3: 装备 & 世界 — 3 columns: General Decay | Gear Decay | World Items
    // ═══════════════════════════════════════════════════════════════════════
    internal static float DrawGearAndWorldTab(Func<float, float, float, float, Rect> R, TldHacksSettings s)
    {
        float c1 = 0f, c2 = COL_W3 + 15f, c3 = 2f * (COL_W3 + 15f);
        float y1 = 6f, y2 = 6f, y3 = 6f;
        bool changed = false;
        float v;
        bool b;

        // ——— Column 1: General Decay + Food Decay ———

        y1 = SectionCol(R, c1, y1, COL_W3, I18n.T("衰减·通用", "Decay·General"));
        bool infDur = GUI.Toggle(R(c1, y1, COL_TOG_W3, ROW_H), s.InfiniteDurability,
            I18n.T(" 无限耐久", " Infinite Durability"), ToggleStyle ?? GUI.skin.toggle);
        if (infDur != s.InfiniteDurability) { s.InfiniteDurability = infDur; changed = true; }
        y1 += ROW_ADV;
        GUI.Label(R(c1, y1, COL_W3, ROW_H),
            I18n.T("  0=不衰减 1=原版 2=双倍", "  0=off 1=vanilla 2=2x"),
            MutedLabel ?? GUI.skin.label);
        y1 += ROW_ADV;
        v = DrawSliderCol3(R, c1, y1, I18n.T("通用衰减", "General"), s.GeneralDecay, 0f, 2f, "F2");
        if (v != s.GeneralDecay) { s.GeneralDecay = v; changed = true; }
        y1 += ROW_ADV;
        v = DrawSliderCol3(R, c1, y1, I18n.T("拾取前衰减", "Pre-Pickup"), s.DecayBeforePickup, 0f, 2f, "F2");
        if (v != s.DecayBeforePickup) { s.DecayBeforePickup = v; changed = true; }
        y1 += ROW_ADV;
        v = DrawSliderCol3(R, c1, y1, I18n.T("使用时衰减", "On-Use"), s.OnUseDecay, 0f, 2f, "F2");
        if (v != s.OnUseDecay) { s.OnUseDecay = v; changed = true; }
        y1 += ROW_ADV + SECTION_END_ADV;

        y1 = SectionCol(R, c1, y1, COL_W3, I18n.T("衰减·食物", "Decay·Food"));
        v = DrawSliderCol3(R, c1, y1, I18n.T("食物(总)", "Food Total"), s.FoodDecay, 0f, 2f, "F2");
        if (v != s.FoodDecay) { s.FoodDecay = v; changed = true; }
        y1 += ROW_ADV;
        v = DrawSliderCol3(R, c1, y1, I18n.T("生肉", "Raw Meat"), s.RawMeatDecay, 0f, 2f, "F2");
        if (v != s.RawMeatDecay) { s.RawMeatDecay = v; changed = true; }
        y1 += ROW_ADV;
        v = DrawSliderCol3(R, c1, y1, I18n.T("熟肉", "Cooked Meat"), s.CookedMeatDecay, 0f, 2f, "F2");
        if (v != s.CookedMeatDecay) { s.CookedMeatDecay = v; changed = true; }
        y1 += ROW_ADV;
        v = DrawSliderCol3(R, c1, y1, I18n.T("饮品", "Drinks"), s.DrinksDecay, 0f, 2f, "F2");
        if (v != s.DrinksDecay) { s.DrinksDecay = v; changed = true; }
        y1 += ROW_ADV;
        if (GUI.Button(R(c1, y1, 160f, ROW_H), _foodDetailExpanded
            ? I18n.T("▲ 收起", "▲ Hide")
            : I18n.T("▼ 食物细分", "▼ Food Detail")))
            _foodDetailExpanded = !_foodDetailExpanded;
        y1 += ROW_ADV;
        if (_foodDetailExpanded)
        {
            v = DrawSliderCol3(R, c1, y1, I18n.T("腌肉", "Cured Meat"), s.CuredMeatDecay, 0f, 2f, "F2");
            if (v != s.CuredMeatDecay) { s.CuredMeatDecay = v; changed = true; }
            y1 += ROW_ADV;
            v = DrawSliderCol3(R, c1, y1, I18n.T("生鱼", "Raw Fish"), s.RawFishDecay, 0f, 2f, "F2");
            if (v != s.RawFishDecay) { s.RawFishDecay = v; changed = true; }
            y1 += ROW_ADV;
            v = DrawSliderCol3(R, c1, y1, I18n.T("熟鱼", "Cooked Fish"), s.CookedFishDecay, 0f, 2f, "F2");
            if (v != s.CookedFishDecay) { s.CookedFishDecay = v; changed = true; }
            y1 += ROW_ADV;
            v = DrawSliderCol3(R, c1, y1, I18n.T("罐头", "Canned"), s.CannedFoodDecay, 0f, 2f, "F2");
            if (v != s.CannedFoodDecay) { s.CannedFoodDecay = v; changed = true; }
            y1 += ROW_ADV;
            v = DrawSliderCol3(R, c1, y1, I18n.T("咖啡/茶", "Coffee/Tea"), s.CoffeeTeaDecay, 0f, 2f, "F2");
            if (v != s.CoffeeTeaDecay) { s.CoffeeTeaDecay = v; changed = true; }
            y1 += ROW_ADV;
            v = DrawSliderCol3(R, c1, y1, I18n.T("包装食品", "Packaged"), s.PackagedFoodDecay, 0f, 2f, "F2");
            if (v != s.PackagedFoodDecay) { s.PackagedFoodDecay = v; changed = true; }
            y1 += ROW_ADV;
            v = DrawSliderCol3(R, c1, y1, I18n.T("油脂", "Fat"), s.FatDecay, 0f, 2f, "F2");
            if (v != s.FatDecay) { s.FatDecay = v; changed = true; }
            y1 += ROW_ADV;
            v = DrawSliderCol3(R, c1, y1, I18n.T("腌鱼", "Cured Fish"), s.CuredFishDecay, 0f, 2f, "F2");
            if (v != s.CuredFishDecay) { s.CuredFishDecay = v; changed = true; }
            y1 += ROW_ADV;
            v = DrawSliderCol3(R, c1, y1, I18n.T("食材", "Ingredients"), s.IngredientsDecay, 0f, 2f, "F2");
            if (v != s.IngredientsDecay) { s.IngredientsDecay = v; changed = true; }
            y1 += ROW_ADV;
            v = DrawSliderCol3(R, c1, y1, I18n.T("其他食品", "Other Food"), s.OtherFoodDecay, 0f, 2f, "F2");
            if (v != s.OtherFoodDecay) { s.OtherFoodDecay = v; changed = true; }
            y1 += ROW_ADV;
        }
        y1 += SECTION_END_ADV;

        // ——— Column 2: Gear Decay + Clothing Decay ———

        y2 = SectionCol(R, c2, y2, COL_W3, I18n.T("衰减·装备", "Decay·Gear"));
        v = DrawSliderCol3(R, c2, y2, I18n.T("衣物", "Clothing"), s.ClothingDecayRate, 0f, 2f, "F2");
        if (v != s.ClothingDecayRate) { s.ClothingDecayRate = v; changed = true; }
        y2 += ROW_ADV;
        v = DrawSliderCol3(R, c2, y2, I18n.T("枪械", "Gun"), s.GunDecay, 0f, 2f, "F2");
        if (v != s.GunDecay) { s.GunDecay = v; changed = true; }
        y2 += ROW_ADV;
        v = DrawSliderCol3(R, c2, y2, I18n.T("弓箭", "Bow"), s.BowDecay, 0f, 2f, "F2");
        if (v != s.BowDecay) { s.BowDecay = v; changed = true; }
        y2 += ROW_ADV;
        v = DrawSliderCol3(R, c2, y2, I18n.T("工具", "Tools"), s.ToolsDecay, 0f, 2f, "F2");
        if (v != s.ToolsDecay) { s.ToolsDecay = v; changed = true; }
        y2 += ROW_ADV;
        if (GUI.Button(R(c2, y2, 160f, ROW_H), _gearDetailExpanded
            ? I18n.T("▲ 收起", "▲ Hide")
            : I18n.T("▼ 装备细分", "▼ Gear Detail")))
            _gearDetailExpanded = !_gearDetailExpanded;
        y2 += ROW_ADV;
        if (_gearDetailExpanded)
        {
            v = DrawSliderCol3(R, c2, y2, I18n.T("睡袋", "Bedroll"), s.BedrollDecay, 0f, 2f, "F2");
            if (v != s.BedrollDecay) { s.BedrollDecay = v; changed = true; }
            y2 += ROW_ADV;
            v = DrawSliderCol3(R, c2, y2, I18n.T("雪橇", "Travois"), s.TravoisDecay, 0f, 2f, "F2");
            if (v != s.TravoisDecay) { s.TravoisDecay = v; changed = true; }
            y2 += ROW_ADV;
            v = DrawSliderCol3(R, c2, y2, I18n.T("箭矢", "Arrow"), s.ArrowDecay, 0f, 2f, "F2");
            if (v != s.ArrowDecay) { s.ArrowDecay = v; changed = true; }
            y2 += ROW_ADV;
            v = DrawSliderCol3(R, c2, y2, I18n.T("陷阱", "Snare"), s.SnareDecay, 0f, 2f, "F2");
            if (v != s.SnareDecay) { s.SnareDecay = v; changed = true; }
            y2 += ROW_ADV;
            v = DrawSliderCol3(R, c2, y2, I18n.T("生火工具", "Firestart"), s.FirestartingDecay, 0f, 2f, "F2");
            if (v != s.FirestartingDecay) { s.FirestartingDecay = v; changed = true; }
            y2 += ROW_ADV;
            v = DrawSliderCol3(R, c2, y2, I18n.T("兽皮/尸骸", "Hide/Gut"), s.HideDecay, 0f, 2f, "F2");
            if (v != s.HideDecay) { s.HideDecay = v; changed = true; }
            y2 += ROW_ADV;
            v = DrawSliderCol3(R, c2, y2, I18n.T("急救品", "First Aid"), s.FirstAidDecay, 0f, 2f, "F2");
            if (v != s.FirstAidDecay) { s.FirstAidDecay = v; changed = true; }
            y2 += ROW_ADV;
            v = DrawSliderCol3(R, c2, y2, I18n.T("净水片", "Purifier"), s.WaterPurifierDecay, 0f, 2f, "F2");
            if (v != s.WaterPurifierDecay) { s.WaterPurifierDecay = v; changed = true; }
            y2 += ROW_ADV;
            v = DrawSliderCol3(R, c2, y2, I18n.T("锅具", "Cook Pot"), s.CookingPotDecay, 0f, 2f, "F2");
            if (v != s.CookingPotDecay) { s.CookingPotDecay = v; changed = true; }
            y2 += ROW_ADV;
            v = DrawSliderCol3(R, c2, y2, I18n.T("信号枪弹药", "Flare Ammo"), s.FlareGunAmmoDecay, 0f, 2f, "F2");
            if (v != s.FlareGunAmmoDecay) { s.FlareGunAmmoDecay = v; changed = true; }
            y2 += ROW_ADV;
            v = DrawSliderCol3(R, c2, y2, I18n.T("磨刀石", "Whetstone"), s.WhetstoneDecay, 0f, 2f, "F2");
            if (v != s.WhetstoneDecay) { s.WhetstoneDecay = v; changed = true; }
            y2 += ROW_ADV;
            v = DrawSliderCol3(R, c2, y2, I18n.T("开罐器", "Can Opener"), s.CanOpenerDecay, 0f, 2f, "F2");
            if (v != s.CanOpenerDecay) { s.CanOpenerDecay = v; changed = true; }
            y2 += ROW_ADV;
            v = DrawSliderCol3(R, c2, y2, I18n.T("撬棍", "Prybar"), s.PrybarDecay, 0f, 2f, "F2");
            if (v != s.PrybarDecay) { s.PrybarDecay = v; changed = true; }
            y2 += ROW_ADV;
        }
        y2 += SECTION_END_ADV;

        y2 = SectionCol(R, c2, y2, COL_W3, I18n.T("衰减·衣物", "Decay·Clothing"));
        v = DrawSliderCol3(R, c2, y2, I18n.T("日常衰减", "Daily"), s.ClothingDecayDaily, 0f, 1f, "F2");
        if (v != s.ClothingDecayDaily) { s.ClothingDecayDaily = v; changed = true; }
        y2 += ROW_ADV;
        v = DrawSliderCol3(R, c2, y2, I18n.T("室内衰减", "Indoor"), s.ClothingDecayIndoors, 0f, 1f, "F2");
        if (v != s.ClothingDecayIndoors) { s.ClothingDecayIndoors = v; changed = true; }
        y2 += ROW_ADV;
        v = DrawSliderCol3(R, c2, y2, I18n.T("室外衰减", "Outdoor"), s.ClothingDecayOutdoors, 0f, 1f, "F2");
        if (v != s.ClothingDecayOutdoors) { s.ClothingDecayOutdoors = v; changed = true; }
        y2 += ROW_ADV + SECTION_END_ADV;

        // ——— Column 3: World Items (Sprainkle + BowRepair + Sodas + Rarities + Misc) ———

        y3 = SectionCol(R, c3, y3, COL_W3, I18n.T("扭伤系统", "Sprain System"));
        b = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.World_Sprainkle,
            I18n.T(" 启用扭伤调整", " Enable Sprain"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.World_Sprainkle) { s.World_Sprainkle = b; changed = true; }
        y3 += ROW_ADV;
        v = DrawSliderCol3(R, c3, y3, I18n.T("预设(0/1/2)", "Preset"), (float)s.World_SprainklePreset, 0f, 2f, "F0");
        if ((int)v != s.World_SprainklePreset) { s.World_SprainklePreset = (int)v; changed = true; }
        y3 += ROW_ADV;
        if (GUI.Button(R(c3, y3, 160f, ROW_H), _worldSprainkleExpanded
            ? I18n.T("▲ 收起", "▲ Hide")
            : I18n.T("▼ 扭伤参数", "▼ Sprain Params")))
            _worldSprainkleExpanded = !_worldSprainkleExpanded;
        y3 += ROW_ADV;
        if (_worldSprainkleExpanded)
        {
            v = DrawSliderCol3(R, c3, y3, I18n.T("最小坡度(°)", "Min Slope"), s.World_SprainkleSlopeMin, 10f, 60f, "F0");
            if (v != s.World_SprainkleSlopeMin) { s.World_SprainkleSlopeMin = v; changed = true; }
            y3 += ROW_ADV;
            v = DrawSliderCol3(R, c3, y3, I18n.T("移动概率%", "Move%"), s.World_SprainkleBaseChanceMoving, 0f, 50f, "F1");
            if (v != s.World_SprainkleBaseChanceMoving) { s.World_SprainkleBaseChanceMoving = v; changed = true; }
            y3 += ROW_ADV;
            v = DrawSliderCol3(R, c3, y3, I18n.T("负重加成", "Encumber+"), s.World_SprainkleEncumberChance, 0f, 1f, "F2");
            if (v != s.World_SprainkleEncumberChance) { s.World_SprainkleEncumberChance = v; changed = true; }
            y3 += ROW_ADV;
            v = DrawSliderCol3(R, c3, y3, I18n.T("疲劳加成", "Exhaust+"), s.World_SprainkleExhaustionChance, 0f, 1f, "F2");
            if (v != s.World_SprainkleExhaustionChance) { s.World_SprainkleExhaustionChance = v; changed = true; }
            y3 += ROW_ADV;
            v = DrawSliderCol3(R, c3, y3, I18n.T("冲刺加成", "Sprint+"), s.World_SprainkleSprintChance, 0f, 10f, "F1");
            if (v != s.World_SprainkleSprintChance) { s.World_SprainkleSprintChance = v; changed = true; }
            y3 += ROW_ADV;
            b = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.World_SprainkleAnkleEnabled,
                I18n.T(" 脚踝扭伤", " Ankle Sprain"), ToggleStyle ?? GUI.skin.toggle);
            if (b != s.World_SprainkleAnkleEnabled) { s.World_SprainkleAnkleEnabled = b; changed = true; }
            y3 += ROW_ADV;
            b = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.World_SprainkleWristEnabled,
                I18n.T(" 手腕扭伤", " Wrist Sprain"), ToggleStyle ?? GUI.skin.toggle);
            if (b != s.World_SprainkleWristEnabled) { s.World_SprainkleWristEnabled = b; changed = true; }
            y3 += ROW_ADV;
        }
        y3 += SECTION_END_ADV;

        y3 = SectionCol(R, c3, y3, COL_W3, I18n.T("弓修理", "Bow Repair"));
        b = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.World_BowRepair,
            I18n.T(" 启用弓修复", " Enable Bow Repair"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.World_BowRepair) { s.World_BowRepair = b; changed = true; }
        y3 += ROW_ADV;
        b = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.World_BowRepairDLC,
            I18n.T(" DLC弓启用", " DLC Bows"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.World_BowRepairDLC) { s.World_BowRepairDLC = b; changed = true; }
        y3 += ROW_ADV;
        if (GUI.Button(R(c3, y3, 160f, ROW_H), _worldBowExpanded
            ? I18n.T("▲ 收起", "▲ Hide")
            : I18n.T("▼ 弓参数", "▼ Bow Params")))
            _worldBowExpanded = !_worldBowExpanded;
        y3 += ROW_ADV;
        if (_worldBowExpanded)
        {
            GUI.Label(R(c3, y3, COL_W3, ROW_H),
                I18n.T("  模式: 0=手工 1=磨刀石 2=两者", "  Mode: 0=Hand 1=Whet 2=Both"),
                MutedLabel ?? GUI.skin.label);
            y3 += ROW_ADV;
            v = DrawSliderCol3(R, c3, y3, I18n.T("生存弓模式", "Surv Mode"), (float)s.World_BowRepairMode, 0f, 2f, "F0");
            if ((int)v != s.World_BowRepairMode) { s.World_BowRepairMode = (int)v; changed = true; }
            y3 += ROW_ADV;
            v = DrawSliderCol3(R, c3, y3, I18n.T("生存弓材料", "Surv Mat"), (float)s.World_BowMaterialNeed, 0f, 2f, "F0");
            if ((int)v != s.World_BowMaterialNeed) { s.World_BowMaterialNeed = (int)v; changed = true; }
            y3 += ROW_ADV;
            v = DrawSliderCol3(R, c3, y3, I18n.T("运动弓模式", "Sport Mode"), (float)s.World_SportBowRepairMode, 0f, 2f, "F0");
            if ((int)v != s.World_SportBowRepairMode) { s.World_SportBowRepairMode = (int)v; changed = true; }
            y3 += ROW_ADV;
            v = DrawSliderCol3(R, c3, y3, I18n.T("运动弓材料", "Sport Mat"), (float)s.World_SportBowMaterialNeed, 0f, 2f, "F0");
            if ((int)v != s.World_SportBowMaterialNeed) { s.World_SportBowMaterialNeed = (int)v; changed = true; }
            y3 += ROW_ADV;
        }
        y3 += SECTION_END_ADV;

        y3 = SectionCol(R, c3, y3, COL_W3, I18n.T("含咖啡因汽水", "Sodas"));
        b = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.World_CaffeinatedSodas,
            I18n.T(" 启用苏打减疲劳", " Enable Soda Fatigue"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.World_CaffeinatedSodas) { s.World_CaffeinatedSodas = b; changed = true; }
        y3 += ROW_ADV;
        if (GUI.Button(R(c3, y3, 160f, ROW_H), _worldSodaExpanded
            ? I18n.T("▲ 收起", "▲ Hide")
            : I18n.T("▼ 汽水参数", "▼ Soda Params")))
            _worldSodaExpanded = !_worldSodaExpanded;
        y3 += ROW_ADV;
        if (_worldSodaExpanded)
        {
            b = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.World_SodaOrangeEnabled,
                I18n.T(" 橙味", " Orange"), ToggleStyle ?? GUI.skin.toggle);
            if (b != s.World_SodaOrangeEnabled) { s.World_SodaOrangeEnabled = b; changed = true; }
            y3 += ROW_ADV;
            v = DrawSliderCol3(R, c3, y3, I18n.T("橙味减疲劳%", "Orange %"), s.World_SodaOrangeInitial, 1f, 15f, "F0");
            if (v != s.World_SodaOrangeInitial) { s.World_SodaOrangeInitial = v; changed = true; }
            y3 += ROW_ADV;
            b = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.World_SodaSummitEnabled,
                I18n.T(" Summit", " Summit"), ToggleStyle ?? GUI.skin.toggle);
            if (b != s.World_SodaSummitEnabled) { s.World_SodaSummitEnabled = b; changed = true; }
            y3 += ROW_ADV;
            v = DrawSliderCol3(R, c3, y3, I18n.T("Summit减疲劳%", "Summit %"), s.World_SodaSummitInitial, 1f, 15f, "F0");
            if (v != s.World_SodaSummitInitial) { s.World_SodaSummitInitial = v; changed = true; }
            y3 += ROW_ADV;
            b = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.World_SodaGrapeEnabled,
                I18n.T(" 葡萄味", " Grape"), ToggleStyle ?? GUI.skin.toggle);
            if (b != s.World_SodaGrapeEnabled) { s.World_SodaGrapeEnabled = b; changed = true; }
            y3 += ROW_ADV;
            v = DrawSliderCol3(R, c3, y3, I18n.T("葡萄减疲劳%", "Grape %"), s.World_SodaGrapeInitial, 1f, 15f, "F0");
            if (v != s.World_SodaGrapeInitial) { s.World_SodaGrapeInitial = v; changed = true; }
            y3 += ROW_ADV;
            GUI.Label(R(c3, y3, COL_W3, ROW_H),
                I18n.T("  持续: 0=5分 1=10分 2=15分 3=30分", "  0=5m 1=10m 2=15m 3=30m"),
                MutedLabel ?? GUI.skin.label);
            y3 += ROW_ADV;
        }
        y3 += SECTION_END_ADV;

        // ═══ v3.0.4 UT/Stack 整合 — 总开关集中区(详细参数见 ModSettings) ═══
        y3 = SectionCol(R, c3, y3, COL_W3, I18n.T("杂项开关", "Misc Toggles"));
        b = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.UT_BreathVisibility,
            I18n.T(" 哈气特效", " Breath"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.UT_BreathVisibility) { s.UT_BreathVisibility = b; changed = true; }
        y3 += ROW_ADV;
        b = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.UT_FeatProgressInCustom,
            I18n.T(" 自定义难度徽章", " Feat in Custom"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.UT_FeatProgressInCustom) { s.UT_FeatProgressInCustom = b; changed = true; }
        y3 += ROW_ADV;
        b = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.UT_RandomizedItemRotation,
            I18n.T(" 掉落随机朝向", " Random Drop Rot"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.UT_RandomizedItemRotation) { s.UT_RandomizedItemRotation = b; changed = true; }
        y3 += ROW_ADV;
        b = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.UT_RevolverImprovements,
            I18n.T(" 左轮瞄准时可走", " Revolver Walk"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.UT_RevolverImprovements) { s.UT_RevolverImprovements = b; changed = true; }
        y3 += ROW_ADV;
        b = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.UT_RemoveHeadacheDebuff,
            I18n.T(" 去甜点头疼", " No Pie Headache"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.UT_RemoveHeadacheDebuff) { s.UT_RemoveHeadacheDebuff = b; changed = true; }
        y3 += ROW_ADV;
        b = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.UT_RockCacheIndoors,
            I18n.T(" 室内放岩石贮藏", " Indoor Rock Cache"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.UT_RockCacheIndoors) { s.UT_RockCacheIndoors = b; changed = true; }
        y3 += ROW_ADV;
        b = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.UT_GlowingDecals,
            I18n.T(" 高亮喷漆", " Glow Decals"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.UT_GlowingDecals) { s.UT_GlowingDecals = b; changed = true; }
        y3 += ROW_ADV;
        b = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.UT_TravoisOverrideMovement,
            I18n.T(" 雪橇移动无限制", " Travois NoMoveLim"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.UT_TravoisOverrideMovement) { s.UT_TravoisOverrideMovement = b; changed = true; }
        y3 += ROW_ADV;
        b = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.UT_TravoisOverrideInteraction,
            I18n.T(" 雪橇互动无限制", " Travois NoIntLim"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.UT_TravoisOverrideInteraction) { s.UT_TravoisOverrideInteraction = b; changed = true; }
        y3 += ROW_ADV;
        b = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.UT_ToiletWaterPotable,
            I18n.T(" 马桶水可饮", " Toilet Potable"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.UT_ToiletWaterPotable) { s.UT_ToiletWaterPotable = b; changed = true; }
        y3 += ROW_ADV;
        b = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.UT_ConsistantDressingWeight,
            I18n.T(" 绷带统一重量", " Dressing Weight"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.UT_ConsistantDressingWeight) { s.UT_ConsistantDressingWeight = b; changed = true; }
        y3 += ROW_ADV + SECTION_END_ADV;

        // StackManager v1.0.6
        y3 = SectionCol(R, c3, y3, COL_W3, I18n.T("物品堆叠", "Item Stacking"));
        b = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.Stack_AddComponent,
            I18n.T(" 启用堆叠组件", " Add Stackable"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.Stack_AddComponent) { s.Stack_AddComponent = b; changed = true; }
        y3 += ROW_ADV;
        b = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.Stack_UseMaxHP,
            I18n.T(" 按最大状态合并", " Use Max HP"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.Stack_UseMaxHP) { s.Stack_UseMaxHP = b; changed = true; }
        y3 += ROW_ADV;
        GUI.Label(R(c3, y3, COL_W3, ROW_H),
            I18n.T("  详细参数(slider/容器): ModSettings", "  Details: ModSettings panel"),
            MutedLabel ?? GUI.skin.label);
        y3 += ROW_ADV + SECTION_END_ADV;

        if (changed) s.Save();
        return Mathf.Max(y1, Mathf.Max(y2, y3));
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Tab 4: 光火 & 制作 — 3 columns: Torch | Lamp | Craft
    // ═══════════════════════════════════════════════════════════════════════
    internal static float DrawLightAndCraftTab(Func<float, float, float, float, Rect> R, TldHacksSettings s)
    {
        float c1 = 0f, c2 = COL_W3 + 15f, c3 = 2f * (COL_W3 + 15f);
        float y1 = 6f, y2 = 6f, y3 = 6f;
        bool changed = false;
        float v;
        bool b;

        // ——— Column 1: Torch + Anti-Misclick ———

        y1 = SectionCol(R, c1, y1, COL_W3, I18n.T("火把", "Torch"));
        v = DrawSliderCol3(R, c1, y1, I18n.T("燃烧时间(分)", "Burn Time"), s.TorchBurnMinutes, 1f, 2880f, "F0");
        if (v != s.TorchBurnMinutes) { s.TorchBurnMinutes = v; changed = true; }
        y1 += ROW_ADV;
        v = DrawSliderCol3(R, c1, y1, I18n.T("最小状态", "Min Cond"), s.TorchMinCondition, 0f, 1f, "F2");
        if (v != s.TorchMinCondition) { s.TorchMinCondition = v; changed = true; }
        y1 += ROW_ADV;
        v = DrawSliderCol3(R, c1, y1, I18n.T("最大状态", "Max Cond"), s.TorchMaxCondition, 0f, 1f, "F2");
        if (v != s.TorchMaxCondition) { s.TorchMaxCondition = v; changed = true; }
        y1 += ROW_ADV + SECTION_END_ADV;

        y1 = SectionCol(R, c1, y1, COL_W3, I18n.T("防左键误灭", "Anti-Misclick"));
        bool dtlc = GUI.Toggle(R(c1, y1, COL_TOG_W3, ROW_H), s.DisableTorchLeftClick,
            I18n.T(" 禁用左键灭火把", " No Left-Click Torch"), ToggleStyle ?? GUI.skin.toggle);
        if (dtlc != s.DisableTorchLeftClick) { s.DisableTorchLeftClick = dtlc; CheatState.DisableTorchLeftClick = dtlc; changed = true; }
        y1 += ROW_ADV;
        bool dllc = GUI.Toggle(R(c1, y1, COL_TOG_W3, ROW_H), s.DisableLampLeftClick,
            I18n.T(" 禁用左键灭油灯", " No Left-Click Lamp"), ToggleStyle ?? GUI.skin.toggle);
        if (dllc != s.DisableLampLeftClick) { s.DisableLampLeftClick = dllc; CheatState.DisableLampLeftClick = dllc; changed = true; }
        y1 += ROW_ADV + SECTION_END_ADV;

        // ——— Column 2: Lamp settings ———

        y2 = SectionCol(R, c2, y2, COL_W3, I18n.T("煤油灯", "Kerosene Lamp"));
        v = DrawSliderCol3(R, c2, y2, I18n.T("放置消耗", "Placed"), s.LampPlacedBurnMult, 0f, 2f, "F2");
        if (v != s.LampPlacedBurnMult) { s.LampPlacedBurnMult = v; changed = true; }
        y2 += ROW_ADV;
        v = DrawSliderCol3(R, c2, y2, I18n.T("手持消耗", "Held"), s.LampHeldBurnMult, 0f, 2f, "F2");
        if (v != s.LampHeldBurnMult) { s.LampHeldBurnMult = v; changed = true; }
        y2 += ROW_ADV;
        v = DrawSliderCol3(R, c2, y2, I18n.T("开启耗损%", "TurnOn"), s.LampTurnOnDecay, 0f, 2f, "F2");
        if (v != s.LampTurnOnDecay) { s.LampTurnOnDecay = v; changed = true; }
        y2 += ROW_ADV;
        v = DrawSliderCol3(R, c2, y2, I18n.T("持续耗损%/h", "OverTime"), s.LampOverTimeDecay, 0f, 1f, "F2");
        if (v != s.LampOverTimeDecay) { s.LampOverTimeDecay = v; changed = true; }
        y2 += ROW_ADV;
        v = DrawSliderCol3(R, c2, y2, I18n.T("惩罚阈值HP%", "Penalty"), (float)s.LampConditionThreshold, 0f, 100f, "F0");
        if ((int)v != s.LampConditionThreshold) { s.LampConditionThreshold = (int)v; changed = true; }
        y2 += ROW_ADV;
        v = DrawSliderCol3(R, c2, y2, I18n.T("最大惩罚%", "MaxPenalty"), (float)s.LampMaxPenalty, 0f, 200f, "F0");
        if ((int)v != s.LampMaxPenalty) { s.LampMaxPenalty = (int)v; changed = true; }
        y2 += ROW_ADV;
        v = DrawSliderCol3(R, c2, y2, I18n.T("光照范围", "Light Range"), s.LampRangeMultiplier, 0.5f, 5f, "F2");
        if (v != s.LampRangeMultiplier) { s.LampRangeMultiplier = v; changed = true; }
        y2 += ROW_ADV;
        bool lmute = GUI.Toggle(R(c2, y2, COL_TOG_W3, ROW_H), s.LampMute,
            I18n.T(" 放置静音", " Mute Placed"), ToggleStyle ?? GUI.skin.toggle);
        if (lmute != s.LampMute) { s.LampMute = lmute; CheatState.LampMute = lmute; changed = true; }
        y2 += ROW_ADV;
        y2 += SECTION_END_ADV;

        // ——— Column 3: Craft + Cooking ———

        y3 = SectionCol(R, c3, y3, COL_W3, I18n.T("随地制作", "Craft Anywhere"));
        b = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.Craft_Anywhere,
            I18n.T(" 随地制作", " Craft Anywhere"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.Craft_Anywhere) { s.Craft_Anywhere = b; changed = true; }
        y3 += ROW_ADV;
        v = DrawSliderCol3(R, c3, y3, I18n.T("默认位置(0-4)", "Default Loc"), (float)s.Craft_DefaultLocation, 0f, 4f, "F0");
        if ((int)v != s.Craft_DefaultLocation) { s.Craft_DefaultLocation = (int)v; changed = true; }
        y3 += ROW_ADV;
        GUI.Label(R(c3, y3, COL_W3, ROW_H),
            I18n.T("  0=任意 1=工作台 2=锻造 3=弹药台 4=火堆", "  0=Any 1=Bench 2=Forge 3=Ammo 4=Fire"),
            MutedLabel ?? GUI.skin.label);
        y3 += ROW_ADV + SECTION_END_ADV;

        y3 = SectionCol(R, c3, y3, COL_W3, I18n.T("烹饪位扩展", "Cooking Slots"));
        b = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.Craft_MoreCookingSlots,
            I18n.T(" 额外烹饪位", " More Slots"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.Craft_MoreCookingSlots) { s.Craft_MoreCookingSlots = b; changed = true; }
        y3 += ROW_ADV;
        b = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.Craft_MoreSlots_Fireplace,
            I18n.T(" 壁炉额外位", " Fireplace"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.Craft_MoreSlots_Fireplace) { s.Craft_MoreSlots_Fireplace = b; changed = true; }
        y3 += ROW_ADV;
        b = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.Craft_MoreSlots_Barrel,
            I18n.T(" 油桶额外位", " Barrel"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.Craft_MoreSlots_Barrel) { s.Craft_MoreSlots_Barrel = b; changed = true; }
        y3 += ROW_ADV;
        b = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.Craft_MoreSlots_Grill,
            I18n.T(" 烤架额外位", " Grill"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.Craft_MoreSlots_Grill) { s.Craft_MoreSlots_Grill = b; changed = true; }
        y3 += ROW_ADV + SECTION_END_ADV;

        if (changed) s.Save();
        return Mathf.Max(y1, Mathf.Max(y2, y3));
    }
}
