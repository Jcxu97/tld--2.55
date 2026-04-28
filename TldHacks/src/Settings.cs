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

    [Slider(0.6f, 3.0f, 24)]
    [Name("Menu UI Scale(菜单缩放,4K 可设 2-3)")]
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

    // ——— Animals ———
    [Section("Animals / 动物")]
    [Name("Instant Kill(一击必杀)")]
    public bool InstantKillAnimals = false;

    [Name("Freeze Animals(动物不能动)")]
    public bool FreezeAnimals = false;

    [Name("Stealth(动物自动逃跑)")]
    public bool Stealth = false;

    [Name("True Invisible(真·隐身,动物检测不到)")]
    [Description("动物像看不见你一样,正常闲逛/觅食/睡觉,不扑也不逃")]
    public bool TrueInvisible = false;

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

    // InfiniteFireDurations 去除 —— 其他 mod 已覆盖(InfiniteFiresDLC)

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

    [Name("Quick Action(采集/修理/拆解 时间自动加速)")]
    [Description("采集尸骸 / 修理工具 / 拆解物品 时游戏内置有加速按钮,这个 toggle 自动帮你按")]
    public bool QuickAction = false;

    // ——— CT 复刻 v2.7.45+ ———
    [Section("CT 复刻 / 参照 Cheat Engine 表")]
    [Name("秒烤肉(CookingPot 瞬完成)")]
    public bool QuickCook = false;

    [Name("秒搜索(容器/尸体搜刮 NOP TimedHold)")]
    public bool QuickSearch = false;

    [Name("秒割肉(Panel_BodyHarvest)")]
    public bool QuickHarvest = false;

    [Name("秒打碎(Panel_BreakDown)")]
    public bool QuickBreakDown = false;

    [Name("解锁保险箱/上锁门/柜子")]
    public bool UnlockSafes = false;

    [Name("防风油灯油量不减")]
    public bool LampFuelNoDrain = false;

    [Name("保温杯永不失温")]
    public bool FlaskNoHeatLoss = false;

    [Name("保温杯存放无限容量")]
    public bool FlaskInfiniteVol = false;

    [Name("保温瓶装所有茶(任意液体)")]
    public bool FlaskAnyItem = false;

    [Name("加工秒完成(风干/腌制)")]
    public bool QuickEvolve = false;

    [Name("容器无限容量")]
    public bool InfiniteContainer = false;

    [Name("篝火温度 300℃")]
    public bool FireTemp300 = false;

    [Name("篝火永不熄灭")]
    public bool FireNeverDie = false;

    [Name("治愈永久冻伤")]
    public bool CureFrostbite = false;

    [Name("清除死亡惩罚")]
    public bool ClearDeathPenalty = false;

    [Name("钓鱼 100% 成功")]
    public bool QuickFishing = false;
}
