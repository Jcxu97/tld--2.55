using System;
using UnityEngine;

namespace TldHacks;

internal static partial class MenuTweaks
{
    private static bool _worldSprainkleExpanded = false;
    private static bool _worldSodaExpanded = false;
    private static bool _worldBowExpanded = false;

    internal static float DrawWorldItemsTab(Func<float, float, float, float, Rect> R, TldHacksSettings s)
    {
        float y1 = 6f, y2 = 6f;
        float x1 = 10f;
        float x2 = x1 + COL_W + 10f;
        bool changed = false;
        float v;
        bool b;

        // ══════ LEFT COLUMN ══════

        // --- Sprainkle ---
        y1 = SectionCol(R, x1, y1, COL_W, I18n.T("Sprainkle / 扭伤系统", "Sprainkle / Sprain System"));

        b = GUI.Toggle(R(x1, y1, COL_TOG_W, ROW_H), s.World_Sprainkle,
            I18n.T(" 启用扭伤调整", " Enable Sprain Tweaks"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.World_Sprainkle) { s.World_Sprainkle = b; changed = true; }
        y1 += ROW_ADV;

        v = DrawSliderCol(R, x1, y1, I18n.T("预设(0原版/1自定/2增强)", "Preset"), (float)s.World_SprainklePreset, 0f, 2f, "F0");
        if ((int)v != s.World_SprainklePreset) { s.World_SprainklePreset = (int)v; changed = true; }
        y1 += ROW_ADV;

        GUI.Label(R(x1, y1, COL_W, ROW_H),
            I18n.T("  0=原版规则 1=自定义 2=增强难度", "  0=Vanilla 1=Custom 2=Harder"),
            MutedLabel ?? GUI.skin.label);
        y1 += ROW_ADV;

        if (GUI.Button(R(x1, y1, 200f, ROW_H), _worldSprainkleExpanded
            ? I18n.T("▲ 收起扭伤参数", "▲ Hide Sprain Params")
            : I18n.T("▼ 自定义扭伤参数", "▼ Custom Sprain Params")))
            _worldSprainkleExpanded = !_worldSprainkleExpanded;
        y1 += ROW_ADV;

        if (_worldSprainkleExpanded)
        {
            v = DrawSliderCol(R, x1, y1, I18n.T("最小坡度(°)", "Min Slope"), s.World_SprainkleSlopeMin, 10f, 60f, "F0");
            if (v != s.World_SprainkleSlopeMin) { s.World_SprainkleSlopeMin = v; changed = true; }
            y1 += ROW_ADV;

            v = DrawSliderCol(R, x1, y1, I18n.T("坡度递增系数", "Slope Increase"), s.World_SprainkleSlopeIncrease, 0.5f, 5f, "F1");
            if (v != s.World_SprainkleSlopeIncrease) { s.World_SprainkleSlopeIncrease = v; changed = true; }
            y1 += ROW_ADV;

            v = DrawSliderCol(R, x1, y1, I18n.T("移动基础概率%", "Base Chance%"), s.World_SprainkleBaseChanceMoving, 0f, 50f, "F1");
            if (v != s.World_SprainkleBaseChanceMoving) { s.World_SprainkleBaseChanceMoving = v; changed = true; }
            y1 += ROW_ADV;

            v = DrawSliderCol(R, x1, y1, I18n.T("负重加成", "Encumber+"), s.World_SprainkleEncumberChance, 0f, 1f, "F2");
            if (v != s.World_SprainkleEncumberChance) { s.World_SprainkleEncumberChance = v; changed = true; }
            y1 += ROW_ADV;

            v = DrawSliderCol(R, x1, y1, I18n.T("疲劳加成", "Exhaust+"), s.World_SprainkleExhaustionChance, 0f, 1f, "F2");
            if (v != s.World_SprainkleExhaustionChance) { s.World_SprainkleExhaustionChance = v; changed = true; }
            y1 += ROW_ADV;

            v = DrawSliderCol(R, x1, y1, I18n.T("冲刺加成", "Sprint+"), s.World_SprainkleSprintChance, 0f, 10f, "F1");
            if (v != s.World_SprainkleSprintChance) { s.World_SprainkleSprintChance = v; changed = true; }
            y1 += ROW_ADV;

            v = DrawSliderCol(R, x1, y1, I18n.T("蹲行减免%", "Crouch-%"), s.World_SprainkleCrouchChance, 0f, 100f, "F0");
            if (v != s.World_SprainkleCrouchChance) { s.World_SprainkleCrouchChance = v; changed = true; }
            y1 += ROW_ADV;

            v = DrawSliderCol(R, x1, y1, I18n.T("最短间隔(s)", "Min Interval"), s.World_SprainkleMinSecondsRisk, 0f, 10f, "F1");
            if (v != s.World_SprainkleMinSecondsRisk) { s.World_SprainkleMinSecondsRisk = v; changed = true; }
            y1 += ROW_ADV;

            v = DrawSliderCol(R, x1, y1, I18n.T("手腕移动触发%", "Wrist Move%"), s.World_SprainkleWristMovementChance, 0f, 100f, "F0");
            if (v != s.World_SprainkleWristMovementChance) { s.World_SprainkleWristMovementChance = v; changed = true; }
            y1 += ROW_ADV;

            // ankle
            b = GUI.Toggle(R(x1, y1, COL_TOG_W, ROW_H), s.World_SprainkleAnkleEnabled,
                I18n.T(" 脚踝扭伤", " Ankle Sprain"), ToggleStyle ?? GUI.skin.toggle);
            if (b != s.World_SprainkleAnkleEnabled) { s.World_SprainkleAnkleEnabled = b; changed = true; }
            y1 += ROW_ADV;

            v = DrawSliderCol(R, x1, y1, I18n.T("脚踝最短(h)", "Ankle Min"), s.World_SprainkleAnkleDurMin, 1f, 168f, "F0");
            if (v != s.World_SprainkleAnkleDurMin) { s.World_SprainkleAnkleDurMin = v; changed = true; }
            y1 += ROW_ADV;

            v = DrawSliderCol(R, x1, y1, I18n.T("脚踝最长(h)", "Ankle Max"), s.World_SprainkleAnkleDurMax, 1f, 168f, "F0");
            if (v != s.World_SprainkleAnkleDurMax) { s.World_SprainkleAnkleDurMax = v; changed = true; }
            y1 += ROW_ADV;

            v = DrawSliderCol(R, x1, y1, I18n.T("脚踝休息恢复(h)", "Ankle Rest"), s.World_SprainkleAnkleRestHours, 0f, 24f, "F1");
            if (v != s.World_SprainkleAnkleRestHours) { s.World_SprainkleAnkleRestHours = v; changed = true; }
            y1 += ROW_ADV;

            v = DrawSliderCol(R, x1, y1, I18n.T("脚踝坠落概率%", "Ankle Fall%"), s.World_SprainkleAnkleFallChance, 0f, 100f, "F0");
            if (v != s.World_SprainkleAnkleFallChance) { s.World_SprainkleAnkleFallChance = v; changed = true; }
            y1 += ROW_ADV;

            // wrist
            b = GUI.Toggle(R(x1, y1, COL_TOG_W, ROW_H), s.World_SprainkleWristEnabled,
                I18n.T(" 手腕扭伤", " Wrist Sprain"), ToggleStyle ?? GUI.skin.toggle);
            if (b != s.World_SprainkleWristEnabled) { s.World_SprainkleWristEnabled = b; changed = true; }
            y1 += ROW_ADV;

            v = DrawSliderCol(R, x1, y1, I18n.T("手腕最短(h)", "Wrist Min"), s.World_SprainkleWristDurMin, 1f, 168f, "F0");
            if (v != s.World_SprainkleWristDurMin) { s.World_SprainkleWristDurMin = v; changed = true; }
            y1 += ROW_ADV;

            v = DrawSliderCol(R, x1, y1, I18n.T("手腕最长(h)", "Wrist Max"), s.World_SprainkleWristDurMax, 1f, 168f, "F0");
            if (v != s.World_SprainkleWristDurMax) { s.World_SprainkleWristDurMax = v; changed = true; }
            y1 += ROW_ADV;

            v = DrawSliderCol(R, x1, y1, I18n.T("手腕休息恢复(h)", "Wrist Rest"), s.World_SprainkleWristRestHours, 0f, 24f, "F1");
            if (v != s.World_SprainkleWristRestHours) { s.World_SprainkleWristRestHours = v; changed = true; }
            y1 += ROW_ADV;

            v = DrawSliderCol(R, x1, y1, I18n.T("手腕坠落概率%", "Wrist Fall%"), s.World_SprainkleWristFallChance, 0f, 100f, "F0");
            if (v != s.World_SprainkleWristFallChance) { s.World_SprainkleWristFallChance = v; changed = true; }
            y1 += ROW_ADV;
        }
        y1 += SECTION_END_ADV;

        // --- Bow Repair ---
        y1 = SectionCol(R, x1, y1, COL_W, I18n.T("BowRepair / 弓修复", "BowRepair / Bow Repair"));

        b = GUI.Toggle(R(x1, y1, COL_TOG_W, ROW_H), s.World_BowRepair,
            I18n.T(" 启用弓修复", " Enable Bow Repair"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.World_BowRepair) { s.World_BowRepair = b; changed = true; }
        y1 += ROW_ADV;

        b = GUI.Toggle(R(x1, y1, COL_TOG_W, ROW_H), s.World_BowRepairDLC,
            I18n.T(" DLC弓启用", " DLC Bows"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.World_BowRepairDLC) { s.World_BowRepairDLC = b; changed = true; }
        y1 += ROW_ADV;

        if (GUI.Button(R(x1, y1, 200f, ROW_H), _worldBowExpanded
            ? I18n.T("▲ 收起弓参数", "▲ Hide Bow Params")
            : I18n.T("▼ 弓参数", "▼ Bow Params")))
            _worldBowExpanded = !_worldBowExpanded;
        y1 += ROW_ADV;

        if (_worldBowExpanded)
        {
            GUI.Label(R(x1, y1, COL_W, ROW_H),
                I18n.T("  模式: 0=手工 1=磨刀石 2=两者皆可", "  Mode: 0=Hand 1=Whetstone 2=Both"),
                MutedLabel ?? GUI.skin.label);
            y1 += ROW_ADV;

            GUI.Label(R(x1, y1, COL_W, ROW_H),
                I18n.T("  材料: 0=少量 1=中等 2=大量", "  Mat: 0=Low 1=Med 2=High"),
                MutedLabel ?? GUI.skin.label);
            y1 += ROW_ADV;

            v = DrawSliderCol(R, x1, y1, I18n.T("生存弓模式(0手/1磨/2都)", "Surv Mode"), (float)s.World_BowRepairMode, 0f, 2f, "F0");
            if ((int)v != s.World_BowRepairMode) { s.World_BowRepairMode = (int)v; changed = true; }
            y1 += ROW_ADV;

            v = DrawSliderCol(R, x1, y1, I18n.T("生存弓材料(0低/1中/2高)", "Surv Mat"), (float)s.World_BowMaterialNeed, 0f, 2f, "F0");
            if ((int)v != s.World_BowMaterialNeed) { s.World_BowMaterialNeed = (int)v; changed = true; }
            y1 += ROW_ADV;

            v = DrawSliderCol(R, x1, y1, I18n.T("运动弓模式", "Sport Mode"), (float)s.World_SportBowRepairMode, 0f, 2f, "F0");
            if ((int)v != s.World_SportBowRepairMode) { s.World_SportBowRepairMode = (int)v; changed = true; }
            y1 += ROW_ADV;

            v = DrawSliderCol(R, x1, y1, I18n.T("运动弓材料", "Sport Mat"), (float)s.World_SportBowMaterialNeed, 0f, 2f, "F0");
            if ((int)v != s.World_SportBowMaterialNeed) { s.World_SportBowMaterialNeed = (int)v; changed = true; }
            y1 += ROW_ADV;

            v = DrawSliderCol(R, x1, y1, I18n.T("木弓模式", "Wood Mode"), (float)s.World_WoodBowRepairMode, 0f, 2f, "F0");
            if ((int)v != s.World_WoodBowRepairMode) { s.World_WoodBowRepairMode = (int)v; changed = true; }
            y1 += ROW_ADV;

            v = DrawSliderCol(R, x1, y1, I18n.T("木弓材料", "Wood Mat"), (float)s.World_WoodBowMaterialNeed, 0f, 2f, "F0");
            if ((int)v != s.World_WoodBowMaterialNeed) { s.World_WoodBowMaterialNeed = (int)v; changed = true; }
            y1 += ROW_ADV;

            v = DrawSliderCol(R, x1, y1, I18n.T("灌木弓模式", "Bush Mode"), (float)s.World_BushBowRepairMode, 0f, 2f, "F0");
            if ((int)v != s.World_BushBowRepairMode) { s.World_BushBowRepairMode = (int)v; changed = true; }
            y1 += ROW_ADV;

            v = DrawSliderCol(R, x1, y1, I18n.T("灌木弓材料", "Bush Mat"), (float)s.World_BushBowMaterialNeed, 0f, 2f, "F0");
            if ((int)v != s.World_BushBowMaterialNeed) { s.World_BushBowMaterialNeed = (int)v; changed = true; }
            y1 += ROW_ADV;
        }
        y1 += SECTION_END_ADV;

        // ══════ RIGHT COLUMN ══════

        // --- Caffeinated Sodas ---
        y2 = SectionCol(R, x2, y2, COL_W, I18n.T("CaffeinatedSodas / 含咖啡因汽水", "CaffeinatedSodas / Sodas"));

        b = GUI.Toggle(R(x2, y2, COL_TOG_W, ROW_H), s.World_CaffeinatedSodas,
            I18n.T(" 启用苏打减疲劳", " Enable Soda Fatigue"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.World_CaffeinatedSodas) { s.World_CaffeinatedSodas = b; changed = true; }
        y2 += ROW_ADV;

        if (GUI.Button(R(x2, y2, 200f, ROW_H), _worldSodaExpanded
            ? I18n.T("▲ 收起汽水参数", "▲ Hide Soda Params")
            : I18n.T("▼ 汽水参数", "▼ Soda Params")))
            _worldSodaExpanded = !_worldSodaExpanded;
        y2 += ROW_ADV;

        if (_worldSodaExpanded)
        {
            // Orange
            b = GUI.Toggle(R(x2, y2, COL_TOG_W, ROW_H), s.World_SodaOrangeEnabled,
                I18n.T(" 橙味", " Orange"), ToggleStyle ?? GUI.skin.toggle);
            if (b != s.World_SodaOrangeEnabled) { s.World_SodaOrangeEnabled = b; changed = true; }
            y2 += ROW_ADV;

            v = DrawSliderCol(R, x2, y2, I18n.T("橙味减疲劳%", "Orange %"), s.World_SodaOrangeInitial, 1f, 15f, "F0");
            if (v != s.World_SodaOrangeInitial) { s.World_SodaOrangeInitial = v; changed = true; }
            y2 += ROW_ADV;

            v = DrawSliderCol(R, x2, y2, I18n.T("橙味持续(0-3)", "Orange Dur"), (float)s.World_SodaOrangeDuration, 0f, 3f, "F0");
            if ((int)v != s.World_SodaOrangeDuration) { s.World_SodaOrangeDuration = (int)v; changed = true; }
            y2 += ROW_ADV;

            // Summit
            b = GUI.Toggle(R(x2, y2, COL_TOG_W, ROW_H), s.World_SodaSummitEnabled,
                I18n.T(" Summit", " Summit"), ToggleStyle ?? GUI.skin.toggle);
            if (b != s.World_SodaSummitEnabled) { s.World_SodaSummitEnabled = b; changed = true; }
            y2 += ROW_ADV;

            v = DrawSliderCol(R, x2, y2, I18n.T("Summit减疲劳%", "Summit %"), s.World_SodaSummitInitial, 1f, 15f, "F0");
            if (v != s.World_SodaSummitInitial) { s.World_SodaSummitInitial = v; changed = true; }
            y2 += ROW_ADV;

            v = DrawSliderCol(R, x2, y2, I18n.T("Summit持续(0-3)", "Summit Dur"), (float)s.World_SodaSummitDuration, 0f, 3f, "F0");
            if ((int)v != s.World_SodaSummitDuration) { s.World_SodaSummitDuration = (int)v; changed = true; }
            y2 += ROW_ADV;

            // Grape
            b = GUI.Toggle(R(x2, y2, COL_TOG_W, ROW_H), s.World_SodaGrapeEnabled,
                I18n.T(" 葡萄味", " Grape"), ToggleStyle ?? GUI.skin.toggle);
            if (b != s.World_SodaGrapeEnabled) { s.World_SodaGrapeEnabled = b; changed = true; }
            y2 += ROW_ADV;

            v = DrawSliderCol(R, x2, y2, I18n.T("葡萄减疲劳%", "Grape %"), s.World_SodaGrapeInitial, 1f, 15f, "F0");
            if (v != s.World_SodaGrapeInitial) { s.World_SodaGrapeInitial = v; changed = true; }
            y2 += ROW_ADV;

            v = DrawSliderCol(R, x2, y2, I18n.T("葡萄持续(0-3)", "Grape Dur"), (float)s.World_SodaGrapeDuration, 0f, 3f, "F0");
            if ((int)v != s.World_SodaGrapeDuration) { s.World_SodaGrapeDuration = (int)v; changed = true; }
            y2 += ROW_ADV;

            GUI.Label(R(x2, y2, COL_W, ROW_H),
                I18n.T("  持续: 0=5分 1=10分 2=15分 3=30分", "  Dur: 0=5m 1=10m 2=15m 3=30m"),
                MutedLabel ?? GUI.skin.label);
            y2 += ROW_ADV;
        }
        y2 += SECTION_END_ADV;


        // --- Carcass Moving ---
        y2 = SectionCol(R, x2, y2, COL_W, I18n.T("CarcassMoving / 搬运猎物", "CarcassMoving / Carry Carcass"));

        b = GUI.Toggle(R(x2, y2, COL_TOG_W, ROW_H), s.World_CarcassMoving,
            I18n.T(" 启用搬运猎物", " Enable Carcass Moving"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.World_CarcassMoving) { s.World_CarcassMoving = b; changed = true; }
        y2 += ROW_ADV;

        GUI.Label(R(x2, y2, COL_W, ROW_H),
            I18n.T("  搬运鹿/狼尸骸跨场景(仅限一次切图)", "  Carry deer/wolf carcass across scenes (1 transition)"),
            MutedLabel ?? GUI.skin.label);
        y2 += ROW_ADV;
        y2 += SECTION_END_ADV;

        // --- Electric Torch ---
        y2 = SectionCol(R, x2, y2, COL_W, I18n.T("ElectricTorch / 极光引火", "ElectricTorch / Aurora Ignite"));

        b = GUI.Toggle(R(x2, y2, COL_TOG_W, ROW_H), s.World_ElectricTorch,
            I18n.T(" 启用极光电点火把", " Enable Electric Torch Light"), ToggleStyle ?? GUI.skin.toggle);
        if (b != s.World_ElectricTorch) { s.World_ElectricTorch = b; changed = true; }
        y2 += ROW_ADV;

        GUI.Label(R(x2, y2, COL_W, ROW_H),
            I18n.T("  极光期间用电源点燃火把,免触电伤害", "  Light torches from electric sources during aurora"),
            MutedLabel ?? GUI.skin.label);
        y2 += ROW_ADV;
        y2 += SECTION_END_ADV;

        if (changed) s.Save();
        float yMax = Mathf.Max(y1, y2);
        return yMax + 8f;
    }

}
