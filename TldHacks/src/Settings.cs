using ModSettings;
using UnityEngine;

namespace TldHacks;

// Persisted to Mods/TldHacks.json via JsonModSettings.
// KeyCode 字段会被 ModSettings 2.2.x 自动识别为可重绑按钮;bool → toggle;带 [Slider] 的 float → 滑块。
internal class TldHacksSettings : JsonModSettings
{
    // v2.7.75 诊断应急开关 —— 开了 OnUpdate/OnLateUpdate/OnGUI 全 skip,保留 Harmony patch
    [Section("Diagnostic / 诊断")]
    [Name("Emergency: Pause Runtime(暂停所有 tick/update)")]
    [Description("诊断用。开了 ModMain 不再跑任何 per-frame tick / Stacking OnLateUpdate / Menu UI。Harmony patch 仍挂着(静态挂载不受此开关影响)。若开了仍卡 → 卡在 patch 本身")]
    public bool DiagPauseRuntime = false;

    // v2.7.79 终极诊断:启动时把所有 Harmony patch 全卸载(静态 + DynamicPatch)
    [Name("Emergency: Unpatch All Harmony(卸载所有 patch)")]
    [Description("启动时立即 UnpatchAll + skip Reconcile,TldHacks 的所有 [HarmonyPatch] 零挂载。需重启游戏生效。开了仍卡 = 100% 不是 TldHacks;开了不卡 = 是 patch 总数问题,下一步全迁 DynamicPatch")]
    public bool DiagUnpatchAll = false;

    // ——— Menu ———
    [Section("Menu")]
    [Name("Language / 语言")]
    [Description("Auto = 按系统语言(非中文系统 → English);Chinese = 强制中文;English = 强制英文。改完需按菜单热键重开菜单生效。")]
    [Choice("Auto", "中文 / Chinese", "English")]
    public int LanguageMode = 0;

    [Name("Toggle menu hotkey")]
    [Description("按此键呼出/关闭 TldHacks 菜单。默认 Tab 会和游戏日志键冲突,建议改成 F1 / \\ 等。")]
    public KeyCode MenuHotkey = KeyCode.Tab;

    [Name("Fly toggle hotkey(飞行)")]
    [Description("按此键切换 uConsole fly 命令。需要 DeveloperConsole.dll 启用。")]
    public KeyCode FlyHotkey = KeyCode.F1;

    [Slider(0.6f, 3.0f, 24)]
    [Name("Menu UI Scale(菜单缩放,4K 可设 2-3)")]
    public float MenuScale = 1f;

    [Slider(600f, 2400f, 36)]
    [Name("Menu Width(菜单宽度,拖拽右边缘调整)")]
    public float MenuWidth = 1280f;

    [Slider(400f, 1200f, 32)]
    [Name("Menu Height(菜单高度,拖拽下边缘调整)")]
    public float MenuHeight = 760f;

    // v2.7.83:窗口位置持久化(拖动自动保存,重启保留)
    [Name("Menu X Position(窗口 X,拖动自动存)")]
    public float MenuX = 30f;

    [Name("Menu Y Position(窗口 Y,拖动自动存)")]
    public float MenuY = 30f;

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
    [Name("Steady Aim(稳定瞄准,关闭晃动+抖动+呼吸)")]
    [Description("合并瞄准晃动/抖动/呼吸晃动。开枪准星不动、武器不晃、瞄准画面不抖")]
    public bool NoAimSway = false;

    [Name("No Aim Stamina(瞄准不耗体力)")]
    public bool NoAimStamina = false;

    [Name("Super Accuracy(已由魔法子弹替代,保留兼容)")]
    [Description("已弃用 —— 请使用 ESP 区的「魔法子弹」。保留此开关是为了旧 JSON 兼容:开启时等效于 稳定瞄准+无后坐力。")]
    public bool SuperAccuracy = false;

    [Section("Speed / 节约时间")]
    [Name("Fast Fire(快速射击)")]
    [Description("开火间隔 / 瞄准延迟 / 装填后延迟 = 0。每 30 帧扫所有 GunItem。")]
    public bool FastFire = false;

    // ——— Environment / 环境 ———
    [Section("Environment / 环境")]
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

    [Name("无冻伤风险")]
    [Description("完全阻止冻伤伤害(CT: DealFrostbiteDamageToLocation=0)")]
    public bool NoFrostbiteRisk = false;

    [Name("饱饱 buff(Well Fed 常驻)")]
    [Description("强制 Well Fed 状态激活。CT: WellFed.Update NOP")]
    public bool WellFedBuff = false;

    [Name("温度加成 buff")]
    [Description("强制温度 buff 激活(CT: FreezingBuffActive=true)")]
    public bool FreezingBuff = false;

    [Name("疲劳加成 buff")]
    [Description("强制疲劳 buff 激活(CT: StatusBar fatigue buff=1)")]
    public bool FatigueBuff = false;

    [Name("治愈永久冻伤")]
    public bool CureFrostbite = false;

    [Name("清除死亡惩罚")]
    public bool ClearDeathPenalty = false;

    [Name("钓鱼 100% 成功")]
    public bool QuickFishing = false;

    // ——— v2.7.86 新增功能 ———
    [Name("随意生火(含室内)")]
    [Description("任何地方都能生火,包括室内")]
    public bool FireAnywhere = false;

    [Name("生火材料不减")]
    [Description("添加柴火时不消耗物品")]
    public bool FreeFireFuel = false;

    [Name("科技背包")]
    [Description("解锁科技背包的额外负重")]
    public bool TechBackpack = false;

    [Name("火把满值")]
    [Description("从篝火取出的火把燃烧值为最大")]
    public bool TorchFullValue = false;

    [Name("无条件冲刺(含拉雪橇)")]
    [Description("冲刺不消耗体力,拉雪橇也能无限冲刺")]
    public bool FreeSprint = false;

    [Name("无限体力")]
    [Description("体力条永远满,不影响疲劳值")]
    public bool InfiniteStamina = false;

    [Name("地图双击传送")]
    [Description("双击游戏地图图标传送到该位置(同区内,按M打开地图)")]
    public bool MapClickTP = true;

    // ——— v2.7.55 商人 + 美洲狮 ———
    [Section("商人 Trader")]
    [Name("交易清单不限制(上限 → 64)")]
    public bool TraderUnlimitedList = false;

    [Name("信任值最大化")]
    public bool TraderMaxTrust = false;

    [Name("商人交易秒完成")]
    public bool TraderInstantExchange = false;

    [Name("随时可联系商人(无线电)")]
    public bool TraderAlwaysAvailable = false;

    [Section("美洲狮")]
    [Name("新档首次立即激活美洲狮")]
    public bool CougarInstantActivate = false;

    [Section("与 ItemPicker mod 交互")]
    [Name("W 键自动拾取时跳过自己丢的物品")]
    [Description("按 W 触发 ItemPicker 自动拾取时,忽略本会话内玩家 drop 的 GearItem,避免刚丢就又捡回来")]
    public bool BlockAutoPickupOwnDrops = false;

    [Section("ESP & AutoAim / 透视自瞄")]
    [Name("ESP 透视")]
    public bool ESP = false;
    [Name("自动瞄准 (按住右键激活)")]
    public bool AutoAim = false;
    [Name("魔法子弹 (开枪自动命中目标)")]
    public bool MagicBullet = false;
    [Name("自瞄 FOV (搜索角度)")]
    public float AutoAimFOV = 30f;
    [Name("自瞄速度")]
    public float AutoAimSpeed = 15f;
    [Name("锁定部位 (0=躯干 1=头 2=腿)")]
    public int AimPart = 0;
    [Name("后坐力强度 (0=无 1=原版)")]
    public float RecoilScaleESP = 1f;
    [Name("射速倍率")]
    public float FireRateScale = 1f;
    [Name("换弹速度倍率")]
    public float ReloadScale = 1f;
    [Name("透视距离 (米)")]
    public float ESPRange = 300f;
    [Name("自瞄开关热键")]
    public KeyCode AutoAimHotkey = KeyCode.None;
}
