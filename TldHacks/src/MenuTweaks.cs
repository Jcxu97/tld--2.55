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
    private static bool _footstepExpanded = true;
    private static bool _qolSleepExpanded = false;
    private static bool _worldSprainkleExpanded = false;
    private static bool _worldSodaExpanded = false;
    private static bool _worldBowExpanded = false;

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

    internal static float DrawSlider(Func<float, float, float, float, Rect> R, float x, float y,
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
        // 子列宽:同一 column 内并排放 2 个 toggle
        float subTogW = COL_W3 / 2f - 6f;
        float c2Sub = c2 + COL_W3 / 2f;
        float c3Sub = c3 + COL_W3 / 2f;
        float y1 = 6f, y2 = 6f, y3 = 6f;
        bool changed = false;
        float v;

        // ——— Column 2: 交互 & 感知 + 生活品质 + 车辆 ———

        y2 = SectionCol(R, c2, y2, COL_W3, I18n.T("交互 & 感知", "Interaction & Senses"));
        v = DrawSliderCol3(R, c2, y2, I18n.T("拾取距离", "Pickup Range"), s.PickupRange, 1f, 5f, "F1");
        if (v != s.PickupRange) { s.PickupRange = v; changed = true; }
        y2 += ROW_ADV;
        bool silent = GUI.Toggle(R(c2,    y2, subTogW, ROW_H), s.SilentFootsteps, I18n.T(" 脚步静音", " Silent"), ToggleStyle ?? GUI.skin.toggle);
        bool gunZoom = GUI.Toggle(R(c2Sub, y2, subTogW, ROW_H), s.GunZoomEnabled,  I18n.T(" 滚轮缩放", " Scroll Zoom"), ToggleStyle ?? GUI.skin.toggle);
        if (silent != s.SilentFootsteps) { s.SilentFootsteps = silent; CheatState.SilentFootsteps = silent; changed = true; }
        if (gunZoom != s.GunZoomEnabled) { s.GunZoomEnabled = gunZoom; changed = true; }
        y2 += ROW_ADV;
        if (GUI.Button(R(c2, y2, COL_W3, ROW_H), _footstepExpanded
            ? I18n.T("  [--] 收起材质音量", "  [--] Hide Volume")
            : I18n.T("  [+] 材质音量调节", "  [+] Volume Detail")))
            _footstepExpanded = !_footstepExpanded;
        y2 += ROW_ADV;
        if (_footstepExpanded)
        {
            v = DrawSliderCol3(R, c2, y2, I18n.T("金属声音%", "Metal Vol%"), (float)s.InvWeightMetalVol, 0f, 100f, "F0");
            if ((int)v != s.InvWeightMetalVol) { s.InvWeightMetalVol = (int)v; changed = true; }
            y2 += ROW_ADV;
            v = DrawSliderCol3(R, c2, y2, I18n.T("木材声音%", "Wood Vol%"), (float)s.InvWeightWoodVol, 0f, 100f, "F0");
            if ((int)v != s.InvWeightWoodVol) { s.InvWeightWoodVol = (int)v; changed = true; }
            y2 += ROW_ADV;
            v = DrawSliderCol3(R, c2, y2, I18n.T("水声音%", "Water Vol%"), (float)s.InvWeightWaterVol, 0f, 100f, "F0");
            if ((int)v != s.InvWeightWaterVol) { s.InvWeightWaterVol = (int)v; changed = true; }
            y2 += ROW_ADV;
            v = DrawSliderCol3(R, c2, y2, I18n.T("其他声音%", "General Vol%"), (float)s.InvWeightGeneralVol, 0f, 100f, "F0");
            if ((int)v != s.InvWeightGeneralVol) { s.InvWeightGeneralVol = (int)v; changed = true; }
            y2 += ROW_ADV;
        }
        bool rwl = GUI.Toggle(R(c2,    y2, subTogW, ROW_H), s.RunWithLantern,    I18n.T(" 油灯可跑", " Run w/ Lantern"), ToggleStyle ?? GUI.skin.toggle);
        bool noCharcoal = GUI.Toggle(R(c2Sub, y2, subTogW, ROW_H), s.NoAutoEquipCharcoal, I18n.T(" 取炭不戴", " No Auto Charcoal"), ToggleStyle ?? GUI.skin.toggle);
        if (rwl != s.RunWithLantern) { s.RunWithLantern = rwl; CheatState.RunWithLantern = rwl; changed = true; }
        if (noCharcoal != s.NoAutoEquipCharcoal) { s.NoAutoEquipCharcoal = noCharcoal; CheatState.NoAutoEquipCharcoal = noCharcoal; changed = true; }
        y2 += ROW_ADV;
        bool autoExt = GUI.Toggle(R(c2, y2, COL_TOG_W3, ROW_H), s.AutoExtinguishOnRest, I18n.T(" 休息熄灯", " Auto-Extinguish"), ToggleStyle ?? GUI.skin.toggle);
        if (autoExt != s.AutoExtinguishOnRest) { s.AutoExtinguishOnRest = autoExt; CheatState.AutoExtinguishOnRest = autoExt; changed = true; }
        y2 += ROW_ADV;
        GUI.Label(R(c2, y2, COL_W3, ROW_H), I18n.T("   取炭不戴=取出木炭不再握手上;休息熄灯=躺床自动熄灯", "   no charcoal/auto-extinguish on rest"), MutedLabel ?? GUI.skin.label);
        y2 += ROW_ADV;
        v = DrawSliderCol3(R, c2, y2, I18n.T("开门幅度", "Door Angle"), s.DoorSwingAngle, 0.03f, 0.6f, "F2");
        if (v != s.DoorSwingAngle) { s.DoorSwingAngle = v; changed = true; }
        y2 += ROW_ADV;
        v = DrawSliderCol3(R, c2, y2, I18n.T("开门速度", "Door Speed"), s.DoorSwingSpeed, 0f, 1f, "F2");
        if (v != s.DoorSwingSpeed) { s.DoorSwingSpeed = v; changed = true; }
        y2 += ROW_ADV + SECTION_END_ADV;

        y2 = SectionCol(R, c2, y2, COL_W3, I18n.T("生活品质", "Quality of Life"));
        bool pauseJ = GUI.Toggle(R(c2,    y2, subTogW, ROW_H), s.PauseInJournal, I18n.T(" 日志暂停", " Pause Journal"), ToggleStyle ?? GUI.skin.toggle);
        bool skipI  = GUI.Toggle(R(c2Sub, y2, subTogW, ROW_H), s.SkipIntro,      I18n.T(" 跳过片头", " Skip Intro"), ToggleStyle ?? GUI.skin.toggle);
        if (pauseJ != s.PauseInJournal) { s.PauseInJournal = pauseJ; changed = true; }
        if (skipI != s.SkipIntro) { s.SkipIntro = skipI; changed = true; }
        y2 += ROW_ADV;
        bool muteCougar = GUI.Toggle(R(c2,    y2, subTogW, ROW_H), s.MuteCougarMenuSound,  I18n.T(" 静音狮吼", " Mute Cougar"), ToggleStyle ?? GUI.skin.toggle);
        bool droppable  = GUI.Toggle(R(c2Sub, y2, subTogW, ROW_H), s.DroppableUndroppables, I18n.T(" 任意丢弃", " Drop Any"), ToggleStyle ?? GUI.skin.toggle);
        if (muteCougar != s.MuteCougarMenuSound) { s.MuteCougarMenuSound = muteCougar; changed = true; }
        if (droppable != s.DroppableUndroppables) { s.DroppableUndroppables = droppable; changed = true; }
        y2 += ROW_ADV;
        bool impW    = GUI.Toggle(R(c2,    y2, subTogW, ROW_H), s.ImportantWeight, I18n.T(" 关键重量", " Key Weight"), ToggleStyle ?? GUI.skin.toggle);
        bool allNote = GUI.Toggle(R(c2Sub, y2, subTogW, ROW_H), s.AllNote,         I18n.T(" 笔记可丢", " Drop Notes"), ToggleStyle ?? GUI.skin.toggle);
        if (impW != s.ImportantWeight) { s.ImportantWeight = impW; changed = true; }
        if (allNote != s.AllNote) { s.AllNote = allNote; changed = true; }
        y2 += ROW_ADV;
        bool remTool = GUI.Toggle(R(c2, y2, COL_TOG_W3, ROW_H), s.RememberBreakdownTool, I18n.T(" 记忆工具", " Remember Tool"), ToggleStyle ?? GUI.skin.toggle);
        if (remTool != s.RememberBreakdownTool) { s.RememberBreakdownTool = remTool; changed = true; }
        y2 += ROW_ADV;
        GUI.Label(R(c2, y2, COL_W3, ROW_H), I18n.T("   任意丢弃=钥匙/任务物品也能扔;关键重量=剧情物品按真实重量", "   drop quest items / restore real weight"), MutedLabel ?? GUI.skin.label);
        y2 += ROW_ADV;
        GUI.Label(R(c2, y2, COL_W3, ROW_H), I18n.T("   笔记可丢=笔记便签可丢;记忆工具=拆解默认选上次工具", "   drop notes / preselect last tool"), MutedLabel ?? GUI.skin.label);
        y2 += ROW_ADV + SECTION_END_ADV;



        // ——— Column 1: 速度体力 + 跳跃 ———

        y1 = SectionCol(R, c1, y1, COL_W3, I18n.T("移动 & 体力", "Movement & Stamina"));
        bool speedOn = GUI.Toggle(R(c1, y1, COL_TOG_W3, ROW_H), s.SpeedTweaksEnabled,
            I18n.T(" 启用速度调节", " Enable Speed Tweaks"), ToggleStyle ?? GUI.skin.toggle);
        if (speedOn != s.SpeedTweaksEnabled) { s.SpeedTweaksEnabled = speedOn; changed = true; }
        y1 += ROW_ADV;
        GUI.Label(R(c1, y1, COL_W3, ROW_H),
            I18n.T("   下方所有滑条:1.0=原版,数值越大越快/越省体力", "   1.0=vanilla on all sliders below"), MutedLabel ?? GUI.skin.label);
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
        v = DrawSliderCol3(R, c1, y1, I18n.T("恢复延迟(秒)", "Recovery Delay"), s.StaminaRecoveryDelay, 0f, 2f, "F2");
        if (v != s.StaminaRecoveryDelay) { s.StaminaRecoveryDelay = v; changed = true; }
        y1 += ROW_ADV;
        GUI.Label(R(c1, y1, COL_W3, ROW_H), I18n.T("   冲刺停下后多久开始回体力,0=立即回", "   sec before stamina starts regen"), MutedLabel ?? GUI.skin.label);
        y1 += ROW_ADV + SECTION_END_ADV;

        y1 = SectionCol(R, c1, y1, COL_W3, I18n.T("跳跃", "Jump"));
        bool je = GUI.Toggle(R(c1, y1, COL_TOG_W3, ROW_H), s.JumpEnabled,
            I18n.T(" 启用跳跃", " Enable Jump"), ToggleStyle ?? GUI.skin.toggle);
        if (je != s.JumpEnabled) { s.JumpEnabled = je; changed = true; }
        y1 += ROW_ADV;
        v = DrawSliderCol3(R, c1, y1, I18n.T("跳跃高度", "Height"), s.JumpHeight, 15f, 42f, "F0");
        if (v != s.JumpHeight) { s.JumpHeight = v; changed = true; }
        y1 += ROW_ADV;
        GUI.Label(R(c1, y1, COL_W3, ROW_H), I18n.T("   起跳速度(实际高度≈数值/30 米)", "   jump impulse; actual height≈val/30m"), MutedLabel ?? GUI.skin.label);
        y1 += ROW_ADV;
        bool jk = GUI.Toggle(R(c1, y1, COL_TOG_W3, ROW_H), s.JumpKing,
            I18n.T(" 无限制跳", " Jump King"), ToggleStyle ?? GUI.skin.toggle);
        if (jk != s.JumpKing) { s.JumpKing = jk; changed = true; }
        y1 += ROW_ADV;
        GUI.Label(R(c1, y1, COL_W3, ROW_H), I18n.T("   忽略下方负重上限,任何时候都能跳", "   bypass weight limit; jump anytime"), MutedLabel ?? GUI.skin.label);
        y1 += ROW_ADV;
        v = DrawSliderCol3(R, c1, y1, I18n.T("负重上限(kg)", "Weight Limit"), s.JumpWeightLimit, 10f, 50f, "F0");
        if (v != s.JumpWeightLimit) { s.JumpWeightLimit = v; changed = true; }
        y1 += ROW_ADV;
        GUI.Label(R(c1, y1, COL_W3, ROW_H), I18n.T("   背包重量超此值就跳不动(无限跳关掉时)", "   above this weight, jump disabled"), MutedLabel ?? GUI.skin.label);
        y1 += ROW_ADV;
        v = DrawSliderCol3(R, c1, y1, I18n.T("卡路里消耗", "Cal Cost"), s.JumpCalorieCost, 0f, 50f, "F0");
        if (v != s.JumpCalorieCost) { s.JumpCalorieCost = v; changed = true; }
        y1 += ROW_ADV;
        v = DrawSliderCol3(R, c1, y1, I18n.T("体力消耗", "Stam Cost"), s.JumpStaminaCost, 0f, 20f, "F0");
        if (v != s.JumpStaminaCost) { s.JumpStaminaCost = v; changed = true; }
        y1 += ROW_ADV;
        v = DrawSliderCol3(R, c1, y1, I18n.T("疲劳消耗(%)", "Fatigue Cost"), s.JumpFatigueCost, 0f, 10f, "F1");
        if (v != s.JumpFatigueCost) { s.JumpFatigueCost = v; changed = true; }
        y1 += ROW_ADV;
        GUI.Label(R(c1, y1, COL_W3, ROW_H), I18n.T("   每跳一次扣的卡/体/疲(0=完全免费)", "   per-jump cost (0=free jump)"), MutedLabel ?? GUI.skin.label);
        y1 += ROW_ADV;
        GUI.Label(R(c1, y1, COL_W3, ROW_H),
            I18n.T($"   热键=[{s.JumpKey}](ModSettings 改键)", $"   Key=[{s.JumpKey}] (rebind in ModSettings)"), MutedLabel ?? GUI.skin.label);
        y1 += ROW_ADV + SECTION_END_ADV;

        y1 = SectionCol(R, c1, y1, COL_W3, I18n.T("时间加速", "Time Scale"));
        v = DrawSliderCol3(R, c1, y1, I18n.T("时间加速1", "TimeScale1"), s.TimeScale1, 1f, 50f, "F0");
        if (v != s.TimeScale1) { s.TimeScale1 = v; changed = true; }
        y1 += ROW_ADV;
        v = DrawSliderCol3(R, c1, y1, I18n.T("时间加速2", "TimeScale2"), s.TimeScale2, 1f, 50f, "F0");
        if (v != s.TimeScale2) { s.TimeScale2 = v; changed = true; }
        y1 += ROW_ADV;
        GUI.Label(R(c1, y1, COL_W3, ROW_H),
            I18n.T($"   按住[{s.TimeScaleKey1}][{s.TimeScaleKey2}]加速游戏(ModSettings改键)",
                   $"   Hold [{s.TimeScaleKey1}][{s.TimeScaleKey2}] to fast-forward"),
            MutedLabel ?? GUI.skin.label);
        y1 += ROW_ADV + SECTION_END_ADV;

        // ——— Column 3: 睡眠 + 测绘 + 地图描边 + 埋尸 + QoL 开关 ———

        y3 = SectionCol(R, c3, y3, COL_W3, I18n.T("随地睡觉", "Sleep Anywhere"));
        bool sleepOn = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.QoL_SleepAnywhere,
            I18n.T(" 启用随地睡", " Enable"), ToggleStyle ?? GUI.skin.toggle);
        if (sleepOn != s.QoL_SleepAnywhere) { s.QoL_SleepAnywhere = sleepOn; changed = true; }
        y3 += ROW_ADV;
        GUI.Label(R(c3, y3, COL_W3, ROW_H), I18n.T("   不需要床/睡袋,任何地方都能睡觉", "   sleep without bed/bedroll anywhere"), MutedLabel ?? GUI.skin.label);
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
            GUI.Label(R(c3, y3, COL_W3, ROW_H), I18n.T("   1.0=原版恢复速度,2.0=两倍快回血回精力", "   1.0=vanilla regen rate; 2.0=2x"), MutedLabel ?? GUI.skin.label);
            y3 += ROW_ADV;
            v = DrawSliderCol3(R, c3, y3, I18n.T("冻伤系数", "Freeze Scale"), s.QoL_SleepFreezingScale, 0f, 5f, "F2");
            if (v != s.QoL_SleepFreezingScale) { s.QoL_SleepFreezingScale = v; changed = true; }
            y3 += ROW_ADV;
            GUI.Label(R(c3, y3, COL_W3, ROW_H), I18n.T("   睡眠中失温的倍率,0=完全免冻", "   freezing rate while asleep (0=immune)"), MutedLabel ?? GUI.skin.label);
            y3 += ROW_ADV;
            bool intF = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.QoL_SleepInterrupt,
                I18n.T(" 冷醒机制", " Freeze Interrupt"), ToggleStyle ?? GUI.skin.toggle);
            if (intF != s.QoL_SleepInterrupt) { s.QoL_SleepInterrupt = intF; changed = true; }
            y3 += ROW_ADV;
            GUI.Label(R(c3, y3, COL_W3, ROW_H), I18n.T("   状态降到下方阈值时强制醒来", "   wake up if condition drops below"), MutedLabel ?? GUI.skin.label);
            y3 += ROW_ADV;
            v = DrawSliderCol3(R, c3, y3, I18n.T("冷醒阈值", "Interrupt Thr"), s.QoL_SleepInterruptThreshold, 0f, 0.5f, "F2");
            if (v != s.QoL_SleepInterruptThreshold) { s.QoL_SleepInterruptThreshold = v; changed = true; }
            y3 += ROW_ADV;
            GUI.Label(R(c3, y3, COL_W3, ROW_H), I18n.T("   状态低于此(如0.2=20%)就被冻醒", "   wake threshold (0.2=20% condition)"), MutedLabel ?? GUI.skin.label);
            y3 += ROW_ADV;
        }
        y3 += SECTION_END_ADV;

        y3 = SectionCol(R, c3, y3, COL_W3, I18n.T("自动测绘", "Auto Survey"));
        bool surveyOn = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.QoL_AutoSurvey,
            I18n.T(" 启用自动测绘", " Enable"), ToggleStyle ?? GUI.skin.toggle);
        if (surveyOn != s.QoL_AutoSurvey) { s.QoL_AutoSurvey = surveyOn; changed = true; }
        y3 += ROW_ADV;
        GUI.Label(R(c3, y3, COL_W3, ROW_H), I18n.T("   定时自动绘制周围地图+解锁图标", "   auto-survey nearby map + unlock icons"), MutedLabel ?? GUI.skin.label);
        y3 += ROW_ADV;
        v = DrawSliderCol3(R, c3, y3, I18n.T("触发延迟(秒)", "Delay"), s.QoL_AutoSurveyDelay, 1f, 60f, "F0");
        if (v != s.QoL_AutoSurveyDelay) { s.QoL_AutoSurveyDelay = v; changed = true; }
        y3 += ROW_ADV;
        GUI.Label(R(c3, y3, COL_W3, ROW_H), I18n.T("   每隔多少秒触发一次", "   interval between surveys"), MutedLabel ?? GUI.skin.label);
        y3 += ROW_ADV;
        v = DrawSliderCol3(R, c3, y3, I18n.T("范围倍率", "Range"), s.QoL_AutoSurveyRange, 0.1f, 10f, "F1");
        if (v != s.QoL_AutoSurveyRange) { s.QoL_AutoSurveyRange = v; changed = true; }
        y3 += ROW_ADV;
        GUI.Label(R(c3, y3, COL_W3, ROW_H), I18n.T("   1.0=20m, 数值越大覆盖越广", "   1.0=20m radius; higher=wider"), MutedLabel ?? GUI.skin.label);
        y3 += ROW_ADV;
        bool unl = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.QoL_AutoSurveyUnlock,
            I18n.T(" 无视天气限制", " Ignore Weather"), ToggleStyle ?? GUI.skin.toggle);
        if (unl != s.QoL_AutoSurveyUnlock) { s.QoL_AutoSurveyUnlock = unl; changed = true; }
        y3 += ROW_ADV;
        GUI.Label(R(c3, y3, COL_W3, ROW_H), I18n.T("   允许在任何天气下自动测绘", "   allow survey in any weather"), MutedLabel ?? GUI.skin.label);
        y3 += ROW_ADV;
        y3 += SECTION_END_ADV;

        // ImprovedFlasks toggle
        bool flasks = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.QoL_ImprovedFlasks,
            I18n.T(" 保温瓶增强", " Improved Flasks"), ToggleStyle ?? GUI.skin.toggle);
        if (flasks != s.QoL_ImprovedFlasks) { s.QoL_ImprovedFlasks = flasks; CheatState.QoL_ImprovedFlasks = flasks; changed = true; }
        y3 += ROW_ADV;
        GUI.Label(R(c3, y3, COL_W3, ROW_H), I18n.T("   快捷轮盘喝水/背包分类/温度优化", "   radial drink/sort/temp display"), MutedLabel ?? GUI.skin.label);
        y3 += ROW_ADV;
        y3 += SECTION_END_ADV;

        y2 = SectionCol(R, c2, y2, COL_W3, I18n.T("其他便利功能", "Misc QoL"));
        bool mapOn  = GUI.Toggle(R(c2,    y2, subTogW, ROW_H), s.QoL_MapTextOutlineEnabled, I18n.T(" 地图描边", " Map Outline"), ToggleStyle ?? GUI.skin.toggle);
        bool noSave = GUI.Toggle(R(c2Sub, y2, subTogW, ROW_H), s.QoL_NoSaveOnSprain,        I18n.T(" 扭伤不存", " No Save Sprain"), ToggleStyle ?? GUI.skin.toggle);
        if (mapOn != s.QoL_MapTextOutlineEnabled) { s.QoL_MapTextOutlineEnabled = mapOn; changed = true; }
        if (noSave != s.QoL_NoSaveOnSprain) { s.QoL_NoSaveOnSprain = noSave; changed = true; }
        y2 += ROW_ADV;
        bool noFallSpr = GUI.Toggle(R(c2,    y2, subTogW, ROW_H), s.QoL_NoSaveOnSprainFalls, I18n.T(" 含坠扭伤", " Include Falls"), ToggleStyle ?? GUI.skin.toggle);
        bool bury      = GUI.Toggle(R(c2Sub, y2, subTogW, ROW_H), s.QoL_BuryCorpses,         I18n.T(" 埋葬尸体", " Bury Corpses"), ToggleStyle ?? GUI.skin.toggle);
        if (noFallSpr != s.QoL_NoSaveOnSprainFalls) { s.QoL_NoSaveOnSprainFalls = noFallSpr; changed = true; }
        if (bury != s.QoL_BuryCorpses) { s.QoL_BuryCorpses = bury; changed = true; }
        y2 += ROW_ADV;
        bool wakeUp = GUI.Toggle(R(c2,    y2, subTogW, ROW_H), s.QoL_WakeUpCall,  I18n.T(" 醒来报时", " Wake Up Call"), ToggleStyle ?? GUI.skin.toggle);
        bool aurora = GUI.Toggle(R(c2Sub, y2, subTogW, ROW_H), s.QoL_AuroraSense, I18n.T(" 极光提示", " Aurora Sense"), ToggleStyle ?? GUI.skin.toggle);
        if (wakeUp != s.QoL_WakeUpCall) { s.QoL_WakeUpCall = wakeUp; changed = true; }
        if (aurora != s.QoL_AuroraSense) { s.QoL_AuroraSense = aurora; changed = true; }
        y2 += ROW_ADV;
        bool showT = GUI.Toggle(R(c2,    y2, subTogW, ROW_H), s.QoL_ShowTimeSleep, I18n.T(" 睡眠时间", " Sleep Time"), ToggleStyle ?? GUI.skin.toggle);
        bool noPB  = GUI.Toggle(R(c2Sub, y2, subTogW, ROW_H), s.QoL_NoPitchBlack,  I18n.T(" 不要纯黑", " No Pitch Black"), ToggleStyle ?? GUI.skin.toggle);
        if (showT != s.QoL_ShowTimeSleep) { s.QoL_ShowTimeSleep = showT; changed = true; }
        if (noPB != s.QoL_NoPitchBlack) { s.QoL_NoPitchBlack = noPB; changed = true; }
        y2 += ROW_ADV;
        GUI.Label(R(c2, y2, COL_W3, ROW_H), I18n.T("   扭伤不存=扭伤不强存档(可 SL);含坠扭伤=连摔扭伤一起", "   skip auto-save on sprain / falls"), MutedLabel ?? GUI.skin.label);
        y2 += ROW_ADV;
        GUI.Label(R(c2, y2, COL_W3, ROW_H), I18n.T("   睡眠时间=睡眠界面显示当前钟点;不要纯黑=保留环境光", "   show clock in sleep panel / keep ambient"), MutedLabel ?? GUI.skin.label);
        y2 += ROW_ADV + SECTION_END_ADV;

        y3 = SectionCol(R, c3, y3, COL_W3, I18n.T("高级微调", "Advanced Tweaks"));
        bool pauseRad = GUI.Toggle(R(c3,    y3, subTogW, ROW_H), s.TT_PauseOnRadial, I18n.T(" 开轮盘减速", " Radial Slowmo"), ToggleStyle ?? GUI.skin.toggle);
        bool dropOri  = GUI.Toggle(R(c3Sub, y3, subTogW, ROW_H), s.TT_DroppedOrientation, I18n.T(" 步枪落地竖放", " Rifle Upright"), ToggleStyle ?? GUI.skin.toggle);
        if (pauseRad != s.TT_PauseOnRadial) { s.TT_PauseOnRadial = pauseRad; CheatState.TT_PauseOnRadial = pauseRad; changed = true; }
        if (dropOri != s.TT_DroppedOrientation) { s.TT_DroppedOrientation = dropOri; CheatState.TT_DroppedOrientation = dropOri; changed = true; }
        y3 += ROW_ADV;
        GUI.Label(R(c3, y3, COL_W3, ROW_H), I18n.T("   开轮盘减速=打开辐射轮时游戏慢动作;步枪丢地后竖立不倒", "   radial wheel slows time; rifles land upright"), MutedLabel ?? GUI.skin.label);
        y3 += ROW_ADV;

        bool deathGoat = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.TT_FallDeathGoat, I18n.T(" 无视坠落即死墙(穿越后不会立即死亡)", " Ignore Fall Death Triggers"), ToggleStyle ?? GUI.skin.toggle);
        if (deathGoat != s.TT_FallDeathGoat) { s.TT_FallDeathGoat = deathGoat; CheatState.TT_FallDeathGoat = deathGoat; changed = true; }
        y3 += ROW_ADV;
        v = DrawSliderCol3(R, c3, y3, I18n.T("坠落伤害/米", "Fall Dmg/m"), (float)s.TT_FallDamageMult, 1f, 12f, "F0");
        if ((int)v != s.TT_FallDamageMult) { s.TT_FallDamageMult = (int)v; changed = true; }
        y3 += ROW_ADV;
        GUI.Label(R(c3, y3, COL_W3, ROW_H), I18n.T("   原版=3; 越大摔得越疼", "   vanilla=3; higher=more damage per meter"), MutedLabel ?? GUI.skin.label);
        y3 += ROW_ADV;

        bool capFeels = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.TT_CapFeelsEnabled, I18n.T(" 限制室外体感温度(裁剪极端值)", " Cap Outdoor Feels-Like Temp"), ToggleStyle ?? GUI.skin.toggle);
        if (capFeels != s.TT_CapFeelsEnabled) { s.TT_CapFeelsEnabled = capFeels; CheatState.TT_CapFeelsEnabled = capFeels; changed = true; }
        y3 += ROW_ADV;
        v = DrawSliderCol3(R, c3, y3, I18n.T("体感上限(℃)", "High Cap(℃)"), (float)s.TT_CapFeelsHigh, -10f, 50f, "F0");
        if ((int)v != s.TT_CapFeelsHigh) { s.TT_CapFeelsHigh = (int)v; changed = true; }
        y3 += ROW_ADV;
        v = DrawSliderCol3(R, c3, y3, I18n.T("体感下限(℃)", "Low Cap(℃)"), (float)s.TT_CapFeelsLow, -50f, 10f, "F0");
        if ((int)v != s.TT_CapFeelsLow) { s.TT_CapFeelsLow = (int)v; changed = true; }
        y3 += ROW_ADV;
        GUI.Label(R(c3, y3, COL_W3, ROW_H), I18n.T("   0=不限制; 室内/篝火旁不生效", "   0=no cap; indoor/campfire bypassed"), MutedLabel ?? GUI.skin.label);
        y3 += ROW_ADV;

        bool respawn = GUI.Toggle(R(c3, y3, COL_TOG_W3, ROW_H), s.TT_RespawnPlants, I18n.T(" 采过的植物定时重生", " Respawn Harvested Plants"), ToggleStyle ?? GUI.skin.toggle);
        if (respawn != s.TT_RespawnPlants) { s.TT_RespawnPlants = respawn; CheatState.TT_RespawnPlants = respawn; changed = true; }
        y3 += ROW_ADV;
        v = DrawSliderCol3(R, c3, y3, I18n.T("重生天数", "Respawn Days"), (float)s.TT_PlantRespawnDays, 1f, 365f, "F0");
        if ((int)v != s.TT_PlantRespawnDays) { s.TT_PlantRespawnDays = (int)v; changed = true; }
        y3 += ROW_ADV;
        GUI.Label(R(c3, y3, COL_W3, ROW_H), I18n.T("   采集后经过该天数植物自动复活", "   plant regrows after N days"), MutedLabel ?? GUI.skin.label);
        y3 += ROW_ADV + SECTION_END_ADV;

        if (changed) s.Save();
        return Mathf.Max(y1, Mathf.Max(y2, y3));
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Tab 3: 装备 & 世界 — 3 columns: General Decay | Gear Decay | World Items
    // ═══════════════════════════════════════════════════════════════════════
    internal static float DrawGearAndWorldTab(Func<float, float, float, float, Rect> R, TldHacksSettings s)
    {
        float c1 = 0f, c2 = COL_W3 + 15f, c3 = 2f * (COL_W3 + 15f);
        float subTogW = COL_W3 / 2f - 6f;
        float c3Sub = c3 + COL_W3 / 2f;
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

        y2 = SectionCol(R, c2, y2, COL_W3, I18n.T("弓修理", "Bow Repair"));
        b = GUI.Toggle(R(c2, y2, COL_TOG_W3, ROW_H), s.World_BowRepair,
            I18n.T(" 启用弓修复", " Enable Bow Repair"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.World_BowRepair) { s.World_BowRepair = b; changed = true; }
        y2 += ROW_ADV;
        b = GUI.Toggle(R(c2, y2, COL_TOG_W3, ROW_H), s.World_BowRepairDLC,
            I18n.T(" DLC弓启用", " DLC Bows"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.World_BowRepairDLC) { s.World_BowRepairDLC = b; changed = true; }
        y2 += ROW_ADV;
        if (GUI.Button(R(c2, y2, 160f, ROW_H), _worldBowExpanded
            ? I18n.T("▲ 收起", "▲ Hide")
            : I18n.T("▼ 弓参数", "▼ Bow Params")))
            _worldBowExpanded = !_worldBowExpanded;
        y2 += ROW_ADV;
        if (_worldBowExpanded)
        {
            GUI.Label(R(c2, y2, COL_W3, ROW_H),
                I18n.T("  模式: 0=手工 1=磨刀石 2=两者", "  Mode: 0=Hand 1=Whet 2=Both"),
                MutedLabel ?? GUI.skin.label);
            y2 += ROW_ADV;
            v = DrawSliderCol3(R, c2, y2, I18n.T("生存弓模式", "Surv Mode"), (float)s.World_BowRepairMode, 0f, 2f, "F0");
            if ((int)v != s.World_BowRepairMode) { s.World_BowRepairMode = (int)v; changed = true; }
            y2 += ROW_ADV;
            v = DrawSliderCol3(R, c2, y2, I18n.T("生存弓材料", "Surv Mat"), (float)s.World_BowMaterialNeed, 0f, 2f, "F0");
            if ((int)v != s.World_BowMaterialNeed) { s.World_BowMaterialNeed = (int)v; changed = true; }
            y2 += ROW_ADV;
            v = DrawSliderCol3(R, c2, y2, I18n.T("运动弓模式", "Sport Mode"), (float)s.World_SportBowRepairMode, 0f, 2f, "F0");
            if ((int)v != s.World_SportBowRepairMode) { s.World_SportBowRepairMode = (int)v; changed = true; }
            y2 += ROW_ADV;
            v = DrawSliderCol3(R, c2, y2, I18n.T("运动弓材料", "Sport Mat"), (float)s.World_SportBowMaterialNeed, 0f, 2f, "F0");
            if ((int)v != s.World_SportBowMaterialNeed) { s.World_SportBowMaterialNeed = (int)v; changed = true; }
            y2 += ROW_ADV;
        }
        y2 += SECTION_END_ADV;

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

        // ═══ UT 整合 — 总开关集中区(详细参数见 ModSettings) ═══
        y3 = SectionCol(R, c3, y3, COL_W3, I18n.T("杂项开关", "Misc Toggles"));
        b = GUI.Toggle(R(c3,    y3, subTogW, ROW_H), s.UT_BreathVisibility,    I18n.T(" 哈气特效", " Breath"), ToggleStyle ?? GUI.skin.toggle);
        bool bb = GUI.Toggle(R(c3Sub, y3, subTogW, ROW_H), s.UT_FeatProgressInCustom, I18n.T(" 难度徽章", " Feat Custom"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.UT_BreathVisibility) { s.UT_BreathVisibility = b; changed = true; }
        if (bb != s.UT_FeatProgressInCustom) { s.UT_FeatProgressInCustom = bb; changed = true; }
        y3 += ROW_ADV;
        b  = GUI.Toggle(R(c3,    y3, subTogW, ROW_H), s.UT_RandomizedItemRotation, I18n.T(" 随机朝向", " Random Rot"), ToggleStyle ?? GUI.skin.toggle);
        bb = GUI.Toggle(R(c3Sub, y3, subTogW, ROW_H), s.UT_RevolverImprovements,   I18n.T(" 左轮可走", " Revolver Walk"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.UT_RandomizedItemRotation) { s.UT_RandomizedItemRotation = b; changed = true; }
        if (bb != s.UT_RevolverImprovements) { s.UT_RevolverImprovements = bb; changed = true; }
        y3 += ROW_ADV;
        b  = GUI.Toggle(R(c3,    y3, subTogW, ROW_H), s.UT_RemoveHeadacheDebuff, I18n.T(" 免甜点疼", " No Pie Ache"), ToggleStyle ?? GUI.skin.toggle);
        bb = GUI.Toggle(R(c3Sub, y3, subTogW, ROW_H), s.UT_RockCacheIndoors,     I18n.T(" 室内贮石", " Indoor Rocks"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.UT_RemoveHeadacheDebuff) { s.UT_RemoveHeadacheDebuff = b; changed = true; }
        if (bb != s.UT_RockCacheIndoors) { s.UT_RockCacheIndoors = bb; changed = true; }
        y3 += ROW_ADV;
        b  = GUI.Toggle(R(c3,    y3, subTogW, ROW_H), s.UT_GlowingDecals,             I18n.T(" 高亮喷漆", " Glow Decals"), ToggleStyle ?? GUI.skin.toggle);
        bb = GUI.Toggle(R(c3Sub, y3, subTogW, ROW_H), s.UT_TravoisOverrideMovement,   I18n.T(" 雪橇可动", " Travois Move"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.UT_GlowingDecals) { s.UT_GlowingDecals = b; changed = true; }
        if (bb != s.UT_TravoisOverrideMovement) { s.UT_TravoisOverrideMovement = bb; changed = true; }
        y3 += ROW_ADV;
        b  = GUI.Toggle(R(c3,    y3, subTogW, ROW_H), s.UT_TravoisOverrideInteraction, I18n.T(" 雪橇可拿", " Travois Int"), ToggleStyle ?? GUI.skin.toggle);
        bb = GUI.Toggle(R(c3Sub, y3, subTogW, ROW_H), s.UT_ToiletWaterPotable,         I18n.T(" 马桶可饮", " Toilet Potable"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.UT_TravoisOverrideInteraction) { s.UT_TravoisOverrideInteraction = b; changed = true; }
        if (bb != s.UT_ToiletWaterPotable) { s.UT_ToiletWaterPotable = bb; changed = true; }
        y3 += ROW_ADV;
        b  = GUI.Toggle(R(c3,    y3, subTogW, ROW_H), s.UT_ConsistantDressingWeight, I18n.T(" 绷带统重", " Bandage Weight"), ToggleStyle ?? GUI.skin.toggle);
        bb = GUI.Toggle(R(c3Sub, y3, subTogW, ROW_H), s.World_CarcassMoving,        I18n.T(" 搬运猎物", " Carry Carcass"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.UT_ConsistantDressingWeight) { s.UT_ConsistantDressingWeight = b; changed = true; }
        if (bb != s.World_CarcassMoving) { s.World_CarcassMoving = bb; CheatState.World_CarcassMoving = bb; changed = true; }
        y3 += ROW_ADV;
        b  = GUI.Toggle(R(c3,    y3, subTogW, ROW_H), s.World_ElectricTorch, I18n.T(" 极光点火", " Aurora Torch"), ToggleStyle ?? GUI.skin.toggle);
        bb = GUI.Toggle(R(c3Sub, y3, subTogW, ROW_H), s.World_CarcassMovingAll, I18n.T(" 搬运全部", " Carry All"), ToggleStyle ?? GUI.skin.toggle);
        if (bb != s.World_CarcassMovingAll) { s.World_CarcassMovingAll = bb; CheatState.World_CarcassMovingAll = bb; changed = true; }
        if (b != s.World_ElectricTorch) { s.World_ElectricTorch = b; CheatState.World_ElectricTorch = b; changed = true; }
        y3 += ROW_ADV;
        GUI.Label(R(c3, y3, COL_W3, ROW_H), I18n.T("   哈气特效=呼出白气可见; 随机朝向=地上物品随机旋转角度", "   visible breath; items land at random angles"), MutedLabel ?? GUI.skin.label);
        y3 += ROW_ADV;
        GUI.Label(R(c3, y3, COL_W3, ROW_H), I18n.T("   免甜点疼=吃蛋糕不头疼; 高亮喷漆=喷漆标记发光易辨", "   no pie headache; glowing spray decals"), MutedLabel ?? GUI.skin.label);
        y3 += ROW_ADV;
        GUI.Label(R(c3, y3, COL_W3, ROW_H), I18n.T("   左轮可走=举左轮时可移动; 马桶可饮=马桶水箱当饮用水", "   walk with revolver drawn; toilet water drinkable"), MutedLabel ?? GUI.skin.label);
        y3 += ROW_ADV;
        GUI.Label(R(c3, y3, COL_W3, ROW_H), I18n.T("   雪橇可动=拖橇时仍可冲刺; 雪橇可拿=拖橇时也能拾物", "   sprint while pulling travois; loot while pulling"), MutedLabel ?? GUI.skin.label);
        y3 += ROW_ADV;
        GUI.Label(R(c3, y3, COL_W3, ROW_H), I18n.T("   难度徽章=自定义难度也算成就; 室内贮石=室内可放岩石贮藏", "   feat progress in custom; indoor rock caches"), MutedLabel ?? GUI.skin.label);
        y3 += ROW_ADV;
        GUI.Label(R(c3, y3, COL_W3, ROW_H), I18n.T("   搬运猎物=拾取鹿/狼尸体搬走; 极光点火=极光从电源点火把", "   carry deer/wolf carcass; aurora electric torch"), MutedLabel ?? GUI.skin.label);
        y3 += ROW_ADV;
        GUI.Label(R(c3, y3, COL_W3, ROW_H), I18n.T("   绷带统重=所有绷带/敷料重量统一(不因材料不同而变)", "   consistent bandage weight regardless of material"), MutedLabel ?? GUI.skin.label);
        y3 += ROW_ADV + SECTION_END_ADV;

        // StackManager v1.0.6
        y1 = SectionCol(R, c1, y1, COL_W3, I18n.T("物品堆叠", "Item Stacking"));
        b = GUI.Toggle(R(c1, y1, COL_TOG_W3, ROW_H), s.Stack_AddComponent,
            I18n.T(" 启用堆叠组件", " Add Stackable"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.Stack_AddComponent) { s.Stack_AddComponent = b; changed = true; }
        y1 += ROW_ADV;
        b = GUI.Toggle(R(c1, y1, COL_TOG_W3, ROW_H), s.Stack_UseMaxHP,
            I18n.T(" 按最大状态合并", " Use Max HP"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.Stack_UseMaxHP) { s.Stack_UseMaxHP = b; changed = true; }
        y1 += ROW_ADV;
        GUI.Label(R(c1, y1, COL_W3, ROW_H),
            I18n.T("  详细参数(slider/容器): ModSettings", "  Details: ModSettings panel"),
            MutedLabel ?? GUI.skin.label);
        y1 += ROW_ADV + SECTION_END_ADV;

        if (changed) s.Save();
        return Mathf.Max(y1, Mathf.Max(y2, y3));
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Tab 4: 光火 & 制作 — 3 columns: Torch+Anti-Misclick+Cooking | Flashlight+Lamp | Craft+Speed
    // ═══════════════════════════════════════════════════════════════════════
    internal static float DrawLightAndCraftTab(Func<float, float, float, float, Rect> R, TldHacksSettings s)
    {
        float c1 = 0f, c2 = COL_W3 + 15f, c3 = 2f * (COL_W3 + 15f);
        float subTogW = COL_W3 / 2f - 6f;
        float c1Sub = c1 + COL_W3 / 2f;
        float c2Sub = c2 + COL_W3 / 2f;
        float y1 = 6f, y2 = 6f, y3 = 6f;
        bool changed = false;
        float v;
        bool b;

        // ——— Column 1: Torch + Anti-Misclick + Cooking Slots ———

        y1 = SectionCol(R, c1, y1, COL_W3, I18n.T("火把", "Torch"));
        v = DrawSliderCol3(R, c1, y1, I18n.T("燃烧时间(分)", "Burn Time"), s.TorchBurnMinutes, 1f, 2880f, "F0");
        if (v != s.TorchBurnMinutes) { s.TorchBurnMinutes = v; changed = true; }
        y1 += ROW_ADV;
        v = DrawSliderCol3(R, c1, y1, I18n.T("最小状态", "Min Cond"), s.TorchMinCondition, 0f, 1f, "F2");
        if (v != s.TorchMinCondition) { s.TorchMinCondition = v; changed = true; }
        y1 += ROW_ADV;
        v = DrawSliderCol3(R, c1, y1, I18n.T("最大状态", "Max Cond"), s.TorchMaxCondition, 0f, 1f, "F2");
        if (v != s.TorchMaxCondition) { s.TorchMaxCondition = v; changed = true; }
        y1 += ROW_ADV;
        GUI.Label(R(c1, y1, COL_W3, ROW_H), I18n.T("   状态 0-1,新刷火把随机取最小~最大间值", "   torch spawn condition range 0-1"), MutedLabel ?? GUI.skin.label);
        y1 += ROW_ADV + SECTION_END_ADV;

        y1 = SectionCol(R, c1, y1, COL_W3, I18n.T("防左键误灭", "Anti-Misclick"));
        bool dtlc = GUI.Toggle(R(c1,    y1, subTogW, ROW_H), s.DisableTorchLeftClick, I18n.T(" 锁火把键", " Lock Torch"), ToggleStyle ?? GUI.skin.toggle);
        bool dllc = GUI.Toggle(R(c1Sub, y1, subTogW, ROW_H), s.DisableLampLeftClick,  I18n.T(" 锁油灯键", " Lock Lamp"), ToggleStyle ?? GUI.skin.toggle);
        if (dtlc != s.DisableTorchLeftClick) { s.DisableTorchLeftClick = dtlc; CheatState.DisableTorchLeftClick = dtlc; changed = true; }
        if (dllc != s.DisableLampLeftClick) { s.DisableLampLeftClick = dllc; CheatState.DisableLampLeftClick = dllc; changed = true; }
        y1 += ROW_ADV;
        GUI.Label(R(c1, y1, COL_W3, ROW_H), I18n.T("   开启后左键不会再误把手上的火把/油灯熄灭", "   left-click won't extinguish torch/lamp"), MutedLabel ?? GUI.skin.label);
        y1 += ROW_ADV + SECTION_END_ADV;

        y1 = SectionCol(R, c1, y1, COL_W3, I18n.T("烹饪位扩展", "Cooking Slots"));
        b = GUI.Toggle(R(c1, y1, COL_TOG_W3, ROW_H), s.Craft_MoreCookingSlots,
            I18n.T(" 额外烹饪位", " More Slots"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.Craft_MoreCookingSlots) { s.Craft_MoreCookingSlots = b; changed = true; }
        y1 += ROW_ADV;
        b = GUI.Toggle(R(c1, y1, COL_TOG_W3, ROW_H), s.Craft_MoreSlots_Fireplace,
            I18n.T(" 壁炉额外位", " Fireplace"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.Craft_MoreSlots_Fireplace) { s.Craft_MoreSlots_Fireplace = b; changed = true; }
        y1 += ROW_ADV;
        b = GUI.Toggle(R(c1, y1, COL_TOG_W3, ROW_H), s.Craft_MoreSlots_Barrel,
            I18n.T(" 油桶额外位", " Barrel"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.Craft_MoreSlots_Barrel) { s.Craft_MoreSlots_Barrel = b; changed = true; }
        y1 += ROW_ADV;
        b = GUI.Toggle(R(c1, y1, COL_TOG_W3, ROW_H), s.Craft_MoreSlots_Grill,
            I18n.T(" 烤架额外位", " Grill"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.Craft_MoreSlots_Grill) { s.Craft_MoreSlots_Grill = b; changed = true; }
        y1 += ROW_ADV + SECTION_END_ADV;

        // ——— Column 2: Flashlight + Lamp ———

        y2 = SectionCol(R, c2, y2, COL_W3, I18n.T("手电筒", "Flashlight"));
        bool flashInf = GUI.Toggle(R(c2, y2, COL_TOG_W3, ROW_H), s.UT_FlashInfiniteBattery, I18n.T(" 无限电量", " Inf. Battery"), ToggleStyle ?? GUI.skin.toggle);
        if (flashInf != s.UT_FlashInfiniteBattery) { s.UT_FlashInfiniteBattery = flashInf; changed = true; }
        y2 += ROW_ADV + SECTION_END_ADV;

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
        GUI.Label(R(c2, y2, COL_W3, ROW_H), I18n.T("   放置/手持消耗: 1.0=原版油耗;0=不耗油", "   1.0=vanilla fuel rate; 0=no drain"), MutedLabel ?? GUI.skin.label);
        y2 += ROW_ADV;
        GUI.Label(R(c2, y2, COL_W3, ROW_H), I18n.T("   开启耗损=点亮瞬间扣的状态;持续耗损/h=每小时扣", "   turn-on / hourly condition decay"), MutedLabel ?? GUI.skin.label);
        y2 += ROW_ADV;
        GUI.Label(R(c2, y2, COL_W3, ROW_H), I18n.T("   惩罚阈值=低于此 HP 油耗加成;最大惩罚=加成上限%", "   penalty threshold/cap on low HP"), MutedLabel ?? GUI.skin.label);
        y2 += ROW_ADV + SECTION_END_ADV;

        // ——— Column 3: Craft Anywhere + Interaction Speed ———

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

        y3 = SectionCol(R, c3, y3, COL_W3, I18n.T("交互速度", "Interaction Speed"));
        GUI.Label(R(c3, y3, COL_W3, ROW_H), I18n.T("  1.0=原速, 越大越快(不影响游戏内时间)", "  1.0=normal, higher=faster"), MutedLabel ?? GUI.skin.label);
        y3 += ROW_ADV;
        v = DrawSliderCol3(R, c3, y3, I18n.T("通用动作", "Actions"), s.TT_GlobalSpeedMult, 0.2f, 6f, "F1");
        if (v != s.TT_GlobalSpeedMult) { s.TT_GlobalSpeedMult = v; changed = true; }
        y3 += ROW_ADV;
        v = DrawSliderCol3(R, c3, y3, I18n.T("开容器", "Containers"), s.TT_InteractionSpeedMult, 0.2f, 6f, "F1");
        if (v != s.TT_InteractionSpeedMult) { s.TT_InteractionSpeedMult = v; changed = true; }
        y3 += ROW_ADV;
        v = DrawSliderCol3(R, c3, y3, I18n.T("进食/喝水", "Eating"), s.TT_EatingSpeedMult, 0.2f, 6f, "F1");
        if (v != s.TT_EatingSpeedMult) { s.TT_EatingSpeedMult = v; changed = true; }
        y3 += ROW_ADV;
        v = DrawSliderCol3(R, c3, y3, I18n.T("拆解物品", "Breakdown"), s.TT_BreakdownSpeedMult, 0.2f, 6f, "F1");
        if (v != s.TT_BreakdownSpeedMult) { s.TT_BreakdownSpeedMult = v; changed = true; }
        y3 += ROW_ADV;
        v = DrawSliderCol3(R, c3, y3, I18n.T("阅读研究", "Reading"), s.TT_ReadingSpeedMult, 0.2f, 6f, "F1");
        if (v != s.TT_ReadingSpeedMult) { s.TT_ReadingSpeedMult = v; changed = true; }
        y3 += ROW_ADV;
        GUI.Label(R(c3, y3, COL_W3, ROW_H), I18n.T("  修理/制作/烹饪/净水/雪屋/采药/凿冰等", "  repair/craft/cook/purify/shelter/harvest"), MutedLabel ?? GUI.skin.label);
        y3 += ROW_ADV + SECTION_END_ADV;

        if (changed) s.Save();
        return Mathf.Max(y1, Mathf.Max(y2, y3));
    }
}
