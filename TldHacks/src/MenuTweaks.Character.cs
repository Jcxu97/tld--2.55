using System;
using UnityEngine;

namespace TldHacks;

internal static partial class MenuTweaks
{
    internal static float DrawCharacterTab(Func<float, float, float, float, Rect> R, TldHacksSettings s)
    {
        float y1 = 6f, y2 = 6f;
        float x1 = 10f;
        float x2 = x1 + COL_W + 10f;
        bool changed = false;
        float v;

        // ══════ LEFT COLUMN ══════

        y1 = SectionCol(R, x1, y1, COL_W, I18n.T("速度 & 体力 (SonicMode)", "Speed & Stamina"));

        bool speedOn = GUI.Toggle(R(x1, y1, COL_TOG_W, ROW_H), s.SpeedTweaksEnabled,
            I18n.T(" 启用速度调节", " Enable Speed Tweaks"), ToggleStyle ?? GUI.skin.toggle);
        if (speedOn != s.SpeedTweaksEnabled) { s.SpeedTweaksEnabled = speedOn; changed = true; }
        y1 += ROW_ADV;

        GUI.Label(R(x1, y1, COL_W, ROW_H),
            I18n.T("  1.0=原版速度", "  1.0 = vanilla speed"),
            MutedLabel ?? GUI.skin.label);
        y1 += ROW_ADV;

        v = DrawSliderCol(R, x1, y1, I18n.T("蹲行", "Crouch"), s.CrouchSpeed, 0.5f, 5f, "F2");
        if (v != s.CrouchSpeed) { s.CrouchSpeed = v; changed = true; }
        y1 += ROW_ADV;

        v = DrawSliderCol(R, x1, y1, I18n.T("步行", "Walk"), s.WalkSpeed, 0.5f, 5f, "F2");
        if (v != s.WalkSpeed) { s.WalkSpeed = v; changed = true; }
        y1 += ROW_ADV;

        v = DrawSliderCol(R, x1, y1, I18n.T("冲刺", "Sprint"), s.SprintSpeed, 0.5f, 5f, "F2");
        if (v != s.SprintSpeed) { s.SprintSpeed = v; changed = true; }
        y1 += ROW_ADV;

        v = DrawSliderCol(R, x1, y1, I18n.T("体力恢复", "Stamina Regen"), s.StaminaRecharge, 0.5f, 10f, "F1");
        if (v != s.StaminaRecharge) { s.StaminaRecharge = v; changed = true; }
        y1 += ROW_ADV;

        v = DrawSliderCol(R, x1, y1, I18n.T("体力消耗", "Stamina Drain"), s.StaminaDrain, 0.1f, 2f, "F2");
        if (v != s.StaminaDrain) { s.StaminaDrain = v; changed = true; }
        y1 += ROW_ADV;

        GUI.Label(R(x1, y1, COL_W, ROW_H),
            I18n.T("  1.0=原版消耗, 越小越省体力", "  1.0=vanilla, lower=less drain"),
            MutedLabel ?? GUI.skin.label);
        y1 += ROW_ADV;

        v = DrawSliderCol(R, x1, y1, I18n.T("恢复延迟", "Recovery Delay"), s.StaminaRecoveryDelay, 0f, 2f, "F2");
        if (v != s.StaminaRecoveryDelay) { s.StaminaRecoveryDelay = v; changed = true; }
        y1 += ROW_ADV + SECTION_END_ADV;

        // --- Jump ---
        y1 = SectionCol(R, x1, y1, COL_W, I18n.T("跳跃 (Jump)", "Jump"));

        bool je = GUI.Toggle(R(x1, y1, COL_TOG_W, ROW_H), s.JumpEnabled,
            I18n.T(" 启用跳跃功能", " Enable Jump"), ToggleStyle ?? GUI.skin.toggle);
        if (je != s.JumpEnabled) { s.JumpEnabled = je; changed = true; }
        y1 += ROW_ADV;

        v = DrawSliderCol(R, x1, y1, I18n.T("跳跃高度", "Height"), s.JumpHeight, 15f, 42f, "F0");
        if (v != s.JumpHeight) { s.JumpHeight = v; changed = true; }
        y1 += ROW_ADV;

        bool jk = GUI.Toggle(R(x1, y1, COL_TOG_W, ROW_H), s.JumpKing,
            I18n.T(" 无限制跳(无视负重)", " Jump King"), ToggleStyle ?? GUI.skin.toggle);
        if (jk != s.JumpKing) { s.JumpKing = jk; changed = true; }
        y1 += ROW_ADV;

        v = DrawSliderCol(R, x1, y1, I18n.T("负重上限(kg)", "Weight Limit"), s.JumpWeightLimit, 10f, 50f, "F0");
        if (v != s.JumpWeightLimit) { s.JumpWeightLimit = v; changed = true; }
        y1 += ROW_ADV;

        v = DrawSliderCol(R, x1, y1, I18n.T("卡路里/次", "Cal Cost"), s.JumpCalorieCost, 0f, 50f, "F0");
        if (v != s.JumpCalorieCost) { s.JumpCalorieCost = v; changed = true; }
        y1 += ROW_ADV;

        v = DrawSliderCol(R, x1, y1, I18n.T("体力/次", "Stam Cost"), s.JumpStaminaCost, 0f, 20f, "F0");
        if (v != s.JumpStaminaCost) { s.JumpStaminaCost = v; changed = true; }
        y1 += ROW_ADV;

        v = DrawSliderCol(R, x1, y1, I18n.T("疲劳/次", "Fatigue Cost"), s.JumpFatigueCost, 0f, 10f, "F1");
        if (v != s.JumpFatigueCost) { s.JumpFatigueCost = v; changed = true; }
        y1 += ROW_ADV;

        GUI.Label(R(x1, y1, COL_W, ROW_H),
            I18n.T($"  热键=[{s.JumpKey}] (ModSettings改绑)", $"  Key=[{s.JumpKey}] (ModSettings)"),
            MutedLabel ?? GUI.skin.label);
        y1 += ROW_ADV + SECTION_END_ADV;

        // --- FullSwing ---
        y1 = SectionCol(R, x1, y1, COL_W, I18n.T("开门 (FullSwing)", "Door Swing"));

        v = DrawSliderCol(R, x1, y1, I18n.T("开门角度", "Angle"), s.DoorSwingAngle, 0.03f, 0.6f, "F2");
        if (v != s.DoorSwingAngle) { s.DoorSwingAngle = v; changed = true; }
        y1 += ROW_ADV;

        v = DrawSliderCol(R, x1, y1, I18n.T("开门速度", "Speed"), s.DoorSwingSpeed, 0f, 1f, "F2");
        if (v != s.DoorSwingSpeed) { s.DoorSwingSpeed = v; changed = true; }
        y1 += ROW_ADV + SECTION_END_ADV;

        // --- TimeScale ---
        y1 = SectionCol(R, x1, y1, COL_W, I18n.T("时间加速 (TimeScale)", "Time Scale"));

        v = DrawSliderCol(R, x1, y1, I18n.T("热键1 倍速", "Key1 Speed"), s.TimeScale1, 1f, 50f, "F0");
        if (v != s.TimeScale1) { s.TimeScale1 = v; changed = true; }
        y1 += ROW_ADV;

        v = DrawSliderCol(R, x1, y1, I18n.T("热键2 倍速", "Key2 Speed"), s.TimeScale2, 1f, 50f, "F0");
        if (v != s.TimeScale2) { s.TimeScale2 = v; changed = true; }
        y1 += ROW_ADV;

        GUI.Label(R(x1, y1, COL_W, ROW_H),
            I18n.T($"  [{s.TimeScaleKey1}] [{s.TimeScaleKey2}] (ModSettings)",
                   $"  [{s.TimeScaleKey1}] [{s.TimeScaleKey2}] (ModSettings)"),
            MutedLabel ?? GUI.skin.label);
        y1 += ROW_ADV + SECTION_END_ADV;

        // ══════ RIGHT COLUMN ══════

        y2 = SectionCol(R, x2, y2, COL_W, I18n.T("交互 & 感知", "Interaction & Senses"));

        v = DrawSliderCol(R, x2, y2, I18n.T("交互距离倍率", "Pickup Range"), s.PickupRange, 1f, 5f, "F1");
        if (v != s.PickupRange) { s.PickupRange = v; changed = true; }
        y2 += ROW_ADV;

        bool silent = GUI.Toggle(R(x2, y2, COL_TOG_W, ROW_H), s.SilentFootsteps,
            I18n.T(" 脚步静音", " Silent Footsteps"), ToggleStyle ?? GUI.skin.toggle);
        if (silent != s.SilentFootsteps) { s.SilentFootsteps = silent; CheatState.SilentFootsteps = silent; changed = true; }
        y2 += ROW_ADV;

        if (!s.SilentFootsteps)
        {
            v = DrawSliderCol(R, x2, y2, I18n.T("金属声音%", "Metal Vol%"), (float)s.InvWeightMetalVol, 0f, 100f, "F0");
            if ((int)v != s.InvWeightMetalVol) { s.InvWeightMetalVol = (int)v; changed = true; }
            y2 += ROW_ADV;

            v = DrawSliderCol(R, x2, y2, I18n.T("木材声音%", "Wood Vol%"), (float)s.InvWeightWoodVol, 0f, 100f, "F0");
            if ((int)v != s.InvWeightWoodVol) { s.InvWeightWoodVol = (int)v; changed = true; }
            y2 += ROW_ADV;

            v = DrawSliderCol(R, x2, y2, I18n.T("水声音%", "Water Vol%"), (float)s.InvWeightWaterVol, 0f, 100f, "F0");
            if ((int)v != s.InvWeightWaterVol) { s.InvWeightWaterVol = (int)v; changed = true; }
            y2 += ROW_ADV;

            v = DrawSliderCol(R, x2, y2, I18n.T("其他声音%", "General Vol%"), (float)s.InvWeightGeneralVol, 0f, 100f, "F0");
            if ((int)v != s.InvWeightGeneralVol) { s.InvWeightGeneralVol = (int)v; changed = true; }
            y2 += ROW_ADV;
        }

        bool gunZoom = GUI.Toggle(R(x2, y2, COL_TOG_W, ROW_H), s.GunZoomEnabled,
            I18n.T(" 瞄准滚轮缩放", " Scroll Wheel Zoom"), ToggleStyle ?? GUI.skin.toggle);
        if (gunZoom != s.GunZoomEnabled) { s.GunZoomEnabled = gunZoom; changed = true; }
        y2 += ROW_ADV;

        bool rwl = GUI.Toggle(R(x2, y2, COL_TOG_W, ROW_H), s.RunWithLantern,
            I18n.T(" 拿油灯可跑步", " Run With Lantern"), ToggleStyle ?? GUI.skin.toggle);
        if (rwl != s.RunWithLantern) { s.RunWithLantern = rwl; CheatState.RunWithLantern = rwl; changed = true; }
        y2 += ROW_ADV;

        bool noCharcoal = GUI.Toggle(R(x2, y2, COL_TOG_W, ROW_H), s.NoAutoEquipCharcoal,
            I18n.T(" 取炭不装备", " No Auto-Equip Charcoal"), ToggleStyle ?? GUI.skin.toggle);
        if (noCharcoal != s.NoAutoEquipCharcoal) { s.NoAutoEquipCharcoal = noCharcoal; CheatState.NoAutoEquipCharcoal = noCharcoal; changed = true; }
        y2 += ROW_ADV;

        bool autoExt = GUI.Toggle(R(x2, y2, COL_TOG_W, ROW_H), s.AutoExtinguishOnRest,
            I18n.T(" 休息自动熄灯", " Auto-Extinguish on Rest"), ToggleStyle ?? GUI.skin.toggle);
        if (autoExt != s.AutoExtinguishOnRest) { s.AutoExtinguishOnRest = autoExt; CheatState.AutoExtinguishOnRest = autoExt; changed = true; }
        y2 += ROW_ADV + SECTION_END_ADV;

        // --- QoL ---
        y2 = SectionCol(R, x2, y2, COL_W, I18n.T("生活品质 (QoL)", "Quality of Life"));

        bool pauseJ = GUI.Toggle(R(x2, y2, COL_TOG_W, ROW_H), s.PauseInJournal,
            I18n.T(" 开日志暂停游戏", " Pause in Journal"), ToggleStyle ?? GUI.skin.toggle);
        if (pauseJ != s.PauseInJournal) { s.PauseInJournal = pauseJ; changed = true; }
        y2 += ROW_ADV;

        bool skipI = GUI.Toggle(R(x2, y2, COL_TOG_W, ROW_H), s.SkipIntro,
            I18n.T(" 跳过开场动画", " Skip Intro"), ToggleStyle ?? GUI.skin.toggle);
        if (skipI != s.SkipIntro) { s.SkipIntro = skipI; changed = true; }
        y2 += ROW_ADV;

        bool muteCougar = GUI.Toggle(R(x2, y2, COL_TOG_W, ROW_H), s.MuteCougarMenuSound,
            I18n.T(" 静音主菜单美洲狮", " Mute Cougar Menu"), ToggleStyle ?? GUI.skin.toggle);
        if (muteCougar != s.MuteCougarMenuSound) { s.MuteCougarMenuSound = muteCougar; changed = true; }
        y2 += ROW_ADV;

        bool droppable = GUI.Toggle(R(x2, y2, COL_TOG_W, ROW_H), s.DroppableUndroppables,
            I18n.T(" 允许丢弃不可丢物品", " Droppable Undroppables"), ToggleStyle ?? GUI.skin.toggle);
        if (droppable != s.DroppableUndroppables) { s.DroppableUndroppables = droppable; changed = true; }
        y2 += ROW_ADV;

        bool impW = GUI.Toggle(R(x2, y2, COL_TOG_W, ROW_H), s.ImportantWeight,
            I18n.T(" 关键物品保留重量", " Keep Key Item Weight"), ToggleStyle ?? GUI.skin.toggle);
        if (impW != s.ImportantWeight) { s.ImportantWeight = impW; changed = true; }
        y2 += ROW_ADV;

        bool allNote = GUI.Toggle(R(x2, y2, COL_TOG_W, ROW_H), s.AllNote,
            I18n.T(" 收藏品可丢弃", " Droppable Collectibles"), ToggleStyle ?? GUI.skin.toggle);
        if (allNote != s.AllNote) { s.AllNote = allNote; changed = true; }
        y2 += ROW_ADV;

        bool remTool = GUI.Toggle(R(x2, y2, COL_TOG_W, ROW_H), s.RememberBreakdownTool,
            I18n.T(" 记忆拆解工具", " Remember Breakdown Tool"), ToggleStyle ?? GUI.skin.toggle);
        if (remTool != s.RememberBreakdownTool) { s.RememberBreakdownTool = remTool; changed = true; }
        y2 += ROW_ADV + SECTION_END_ADV;

        // --- Vehicle ---
        y2 = SectionCol(R, x2, y2, COL_W, I18n.T("车辆 (VehicleFov)", "Vehicle"));

        bool vFov = GUI.Toggle(R(x2, y2, COL_TOG_W, ROW_H), s.VehicleKeepFov,
            I18n.T(" 车内保持玩家FOV", " Keep FOV"), ToggleStyle ?? GUI.skin.toggle);
        if (vFov != s.VehicleKeepFov) { s.VehicleKeepFov = vFov; changed = true; }
        y2 += ROW_ADV;

        bool vFree = GUI.Toggle(R(x2, y2, COL_TOG_W, ROW_H), s.VehicleFreeLook,
            I18n.T(" 车内自由视角", " Free Look"), ToggleStyle ?? GUI.skin.toggle);
        if (vFree != s.VehicleFreeLook) { s.VehicleFreeLook = vFree; changed = true; }
        y2 += ROW_ADV;

        v = DrawSliderCol(R, x2, y2, I18n.T("水平角度", "Yaw"), s.VehicleFreeLookYaw, 30f, 180f, "F0");
        if (v != s.VehicleFreeLookYaw) { s.VehicleFreeLookYaw = v; changed = true; }
        y2 += ROW_ADV;

        v = DrawSliderCol(R, x2, y2, I18n.T("俯仰角度", "Pitch"), s.VehicleFreeLookPitch, 20f, 90f, "F0");
        if (v != s.VehicleFreeLookPitch) { s.VehicleFreeLookPitch = v; changed = true; }
        y2 += ROW_ADV + SECTION_END_ADV;

        if (changed) s.Save();
        float yMax = Mathf.Max(y1, y2);
        return yMax + 8f;
    }
}
