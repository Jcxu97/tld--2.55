using ModSettings;
using UnityEngine;

namespace TldHacks;

// Persisted to Mods/TldHacks.json via JsonModSettings.
// KeyCode 字段会被 ModSettings 2.2.x 自动识别为可重绑按钮;bool → toggle;带 [Slider] 的 float → 滑块。
internal class TldHacksSettings : JsonModSettings
{
    // ——— Menu ———
    [Section("Menu")]
    [Name("Toggle menu hotkey")]
    [Description("按此键呼出/关闭 TldHacks 菜单。默认 Tab 会和游戏日志键冲突,建议改成 F1 / \\ 等。")]
    public KeyCode MenuHotkey = KeyCode.Tab;

    [Name("Fly toggle hotkey(飞行)")]
    [Description("按此键切换 uConsole fly 命令。需要 DeveloperConsole.dll 启用。")]
    public KeyCode FlyHotkey = KeyCode.F1;

    [Slider(0.6f, 2.0f, 14)]
    [Name("Menu UI Scale(菜单缩放)")]
    public float MenuScale = 1f;

    // ——— Stacking ———
    [Section("Stacking")]
    [Name("Enable UI stacking")]
    public bool StackingEnabled = true;

    // ——— Life ———
    [Section("Life / 生命")]
    [Name("God Mode(无敌模式)")]
    public bool GodMode = false;

    [Name("No Fall Damage(无坠落伤害)")]
    public bool NoFallDamage = false;

    // ——— Status ———
    [Section("Status / 状态")]
    [Name("Infinite Stamina(无限体力)")]
    public bool InfiniteStamina = false;

    [Name("Always Warm(始终温暖)")]
    public bool AlwaysWarm = false;

    [Name("No Hunger(无饥饿)")]
    public bool NoHunger = false;

    [Name("No Thirst(无口渴)")]
    public bool NoThirst = false;

    [Name("No Fatigue(无疲劳)")]
    public bool NoFatigue = false;

    // ——— Movement ———
    [Section("Movement / 移动")]
    [Slider(0.5f, 5f, 10)]
    [Name("Speed Multiplier(游戏速度)")]
    public float SpeedMultiplier = 1f;

    [Name("Infinite Carry(无限负重)")]
    public bool InfiniteCarry = false;

    // ——— Animals ———
    [Section("Animals / 动物")]
    [Name("Instant Kill(一击必杀)")]
    public bool InstantKillAnimals = false;

    [Name("Freeze Animals(动物不能动)")]
    public bool FreezeAnimals = false;

    [Name("Stealth(动物无法发现你)")]
    public bool Stealth = false;

    // ——— World ———
    [Section("World / 世界")]
    [Name("Thin Ice No Break(冰面不破裂)")]
    public bool ThinIceNoBreak = false;

    [Name("Ignore Locks(忽略上锁)")]
    public bool IgnoreLock = false;

    [Name("Quick Open Container(快速打开容器)")]
    public bool QuickOpenContainer = false;

    // ——— Items / Fire ———
    [Section("Items / 物品")]
    [Name("Infinite Durability(物品不损耗)")]
    public bool InfiniteDurability = false;

    [Name("No Wet Clothes(衣物不潮湿)")]
    public bool NoWetClothes = false;

    [Name("Infinite Fire Durations(火焰无限时长)")]
    [Description("火把/信号棒/营火/灯油/油灯/火把头,所有燃烧值锁满。")]
    public bool InfiniteFireDurations = false;

    // ——— Crafting ———
    [Section("Crafting / 制作")]
    [Name("Free Crafting(免费制作)")]
    public bool FreeCraft = false;

    [Name("Quick Craft(快速制作)")]
    public bool QuickCraft = false;

    // ——— Weapons ———
    [Section("Weapons / 武器")]
    [Name("Infinite Ammo(无限弹药)")]
    public bool InfiniteAmmo = false;

    [Name("No Jam(永不卡壳)")]
    public bool NoJam = false;

    [Name("No Recoil(无后坐力)")]
    public bool NoRecoil = false;

    // ——— Aiming ———
    [Section("Aiming / 瞄准")]
    [Name("No Aim Sway(关闭瞄准晃动)")]
    public bool NoAimSway = false;

    [Name("No Aim Shake(关闭瞄准抖动)")]
    public bool NoAimShake = false;

    [Name("No Breath Sway(关闭呼吸晃动)")]
    public bool NoBreathSway = false;

    [Name("No Aim Stamina(关闭瞄准体力消耗)")]
    public bool NoAimStamina = false;

    [Name("No Aim DOF(关闭瞄准景深)")]
    public bool NoAimDOF = false;

    [Section("Speed / 节约时间")]
    [Name("Fast Fire(快速射击)")]
    [Description("开火间隔 / 瞄准延迟 / 装填后延迟 = 0。每 30 帧扫所有 GunItem。")]
    public bool FastFire = false;

    // ——— Environment / 环境 ———
    [Section("Environment / 环境")]
    [Name("Stop Wind(停止刮风)")]
    public bool StopWind = false;

    [Name("No Sprain Risk(免扭伤风险)")]
    public bool NoSprainRisk = false;

    [Name("Immune Animal Damage(免疫狼/熊/美洲狮伤害)")]
    public bool ImmuneAnimalDamage = false;

    [Name("No Suffocating(不会窒息)")]
    public bool NoSuffocating = false;

    // ——— Shortcuts / 快速操作 ———
    [Section("Shortcuts / 快捷")]
    [Name("Quick Fire Start(生火 100% 成功)")]
    public bool QuickFire = false;

    [Name("Quick Rope Climb(爬绳速度 ×5)")]
    public bool QuickClimb = false;
}
