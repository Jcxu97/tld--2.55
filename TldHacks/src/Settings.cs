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

    // ——— Crafting ———
    [Section("Crafting / 制作")]
    [Name("Free Crafting(免费制作)")]
    public bool FreeCraft = false;

    [Name("Quick Craft(快速制作)")]
    public bool QuickCraft = false;

    [Name("Free Repair(免费修理)")]
    public bool FreeRepair = false;

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

    [Name("Super Accuracy(已弃用,保留兼容)")]
    [Description("已弃用。保留此开关是为了旧 JSON 兼容:开启时等效于 稳定瞄准+无后坐力。")]
    public bool SuperAccuracy = false;


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
    [Name("秒烤肉(CookingPot 瞬完成,自带防烤焦)")]
    public bool QuickCook = false;

    [Name("防烤焦(只锁 Ready,不秒熟)")]
    public bool NoBurn = false;

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

    [Name("篝火燃烧上限(小时)")]
    [Description("原版上限12小时,提高后可无限加燃料")]
    [Slider(12f, 9999f, 100)]
    public float FireMaxBurnHours = 12f;

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

    [Name("自定义背包负重")]
    [Description("覆盖原版 30kg 背包上限为自定义值")]
    public bool TechBackpack = false;

    [Slider(10f, 10000f, 999)]
    [Name("背包负重上限(kg)")]
    [Description("替换原版 30kg 上限的目标值, 默认 30=原版")]
    public float TechBackpackKg = 30f;

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

    [Section("Weapon / 武器微调")]
    [Slider(0f, 1f, 10)]
    [Name("后坐力强度 (0=无 1=原版)")]
    public float RecoilScaleESP = 1f;

    // ——— Integrated: SonicMode (细分速度) ———
    [Section("Movement Detail / 细分速度")]
    [Name("启用细分速度(关=全部原版)")]
    [Description("总开关:关闭后下面所有速度/体力滑条不生效,等于原版速度")]
    public bool SpeedTweaksEnabled = true;

    [Slider(0.5f, 5f, 10)]
    [Name("蹲行速度倍率")]
    public float CrouchSpeed = 2.7f;

    [Slider(0.5f, 5f, 10)]
    [Name("步行速度倍率")]
    public float WalkSpeed = 1.45f;

    [Slider(0.5f, 5f, 10)]
    [Name("冲刺速度倍率")]
    public float SprintSpeed = 2.0f;

    [Slider(0.5f, 10f, 20)]
    [Name("体力恢复速度")]
    public float StaminaRecharge = 4.9f;

    [Slider(0.1f, 2f, 20)]
    [Name("体力消耗速度")]
    public float StaminaDrain = 0.44f;

    [Slider(0f, 2f, 20)]
    [Name("体力恢复延迟倍率")]
    [Description("停止冲刺后多久开始恢复体力的倍率,0=立即恢复,1=原版,2=双倍等待")]
    public float StaminaRecoveryDelay = 0.5f;

    // ——— Integrated: StretchArmstrong (交互距离) ———
    [Slider(1f, 5f, 8)]
    [Name("交互距离倍率")]
    [Description("拾取/交互距离倍数。1=原版,5=五倍远距离拾取")]
    public float PickupRange = 1f;

    // ——— Integrated: SilentWalker (脚步静音) ———
    [Name("脚步完全静音")]
    [Description("完全消除自身脚步声,动物也听不到。启用后忽略以下4项")]
    public bool SilentFootsteps = false;

    [Slider(0f, 100f, 101)]
    [Name("金属声音(%)")]
    [Description("背包金属物品发出的声音百分比,0=无声,100=原版")]
    public int InvWeightMetalVol = 100;

    [Slider(0f, 100f, 101)]
    [Name("木材声音(%)")]
    [Description("背包木材物品发出的声音百分比")]
    public int InvWeightWoodVol = 100;

    [Slider(0f, 100f, 101)]
    [Name("水资源声音(%)")]
    [Description("背包水资源发出的声音百分比")]
    public int InvWeightWaterVol = 100;

    [Slider(0f, 100f, 101)]
    [Name("其他声音(%)")]
    [Description("背包其他物品发出的声音百分比")]
    public int InvWeightGeneralVol = 100;

    // ——— Integrated: Jump (跳跃) ———
    [Name("启用跳跃功能")]
    [Description("总开关 — 关闭则跳跃热键无效")]
    public bool JumpEnabled = true;

    [Name("跳跃热键")]
    public KeyCode JumpKey = KeyCode.Space;

    [Slider(15f, 42f, 27)]
    [Name("跳跃高度")]
    [Description("基础跳跃力度,24=正常高度,42=超级跳")]
    public float JumpHeight = 24f;

    [Name("无限制跳跃(Jump King)")]
    [Description("开启后忽略负重/饥饿/口渴/疲劳/体力限制,直接跳")]
    public bool JumpKing = true;

    [Slider(10f, 50f, 40)]
    [Name("负重上限(kg)")]
    [Description("超过此重量无法跳跃(Jump King关闭时生效)")]
    public float JumpWeightLimit = 30f;

    [Slider(0f, 50f, 50)]
    [Name("卡路里消耗/次")]
    [Description("每次跳跃消耗的卡路里(Jump King关闭时生效)")]
    public float JumpCalorieCost = 14f;

    [Slider(0f, 20f, 20)]
    [Name("体力消耗/次")]
    [Description("每次跳跃消耗的体力百分比(Jump King关闭时生效)")]
    public float JumpStaminaCost = 7f;

    [Slider(0f, 10f, 100)]
    [Name("疲劳消耗/次")]
    [Description("每次跳跃增加的疲劳百分比(Jump King关闭时生效)")]
    public float JumpFatigueCost = 0.5f;

    // ——— Integrated: GunZoom (瞄准缩放) ———
    [Name("滚轮瞄准缩放")]
    [Description("瞄准时滚动鼠标滚轮可放大/缩小视野")]
    public bool GunZoomEnabled = true;

    // ——— Integrated: RunWithLantern ———
    [Name("拿油灯可跑步")]
    [Description("手持油灯时不再锁定移动速度,可正常跑步")]
    public bool RunWithLantern = true;

    // ——— Integrated: DisableAutoEquipCharcoal ———
    [Name("取炭不自动装备")]
    [Description("从篝火收集炭笔后不会自动装备到手上")]
    public bool NoAutoEquipCharcoal = true;

    // ——— Integrated: AutoToggleLights ———
    [Name("休息时自动熄灯")]
    [Description("睡觉或等待时自动关闭手持油灯/火把/手电")]
    public bool AutoExtinguishOnRest = true;

    // ——— Integrated: FullSwing (开门角度) ———
    [Slider(0.03f, 0.6f, 57)]
    [Name("开门角度")]
    [Description("门的打开弧度,0.03=原版,0.3=大开")]
    public float DoorSwingAngle = 0.29f;

    [Slider(0f, 1f, 10)]
    [Name("开门速度")]
    [Description("门的开启速度,1=原版,0=最快")]
    public float DoorSwingSpeed = 0.53f;

    // ——— Integrated: TimeScaleHotkey ———
    [Section("Time Scale / 时间加速")]
    [Name("使用说明")]
    [Description("按住热键1/2期间游戏加速到对应倍率,松开恢复正常速度")]
    public bool TimeScaleInfo = true;

    [Name("时间加速 热键1")]
    public KeyCode TimeScaleKey1 = KeyCode.Keypad1;

    [Slider(1f, 50f, 49)]
    [Name("时间加速1 (热键倍率)")]
    public float TimeScale1 = 20f;

    [Name("时间加速 热键2")]
    public KeyCode TimeScaleKey2 = KeyCode.Keypad2;

    [Slider(1f, 50f, 49)]
    [Name("时间加速2 (热键倍率)")]
    public float TimeScale2 = 5f;

    // ——— Integrated: GearDecayModifier ———
    [Section("Decay / 衰减倍率")]
    [Slider(0f, 2f, 20)]
    [Name("通用衰减倍率")]
    public float GeneralDecay = 1f;

    [Slider(0f, 2f, 20)]
    [Name("拾取前衰减")]
    public float DecayBeforePickup = 1f;

    [Slider(0f, 2f, 20)]
    [Name("使用时衰减")]
    public float OnUseDecay = 1f;

    [Slider(0f, 2f, 20)]
    [Name("食物衰减")]
    public float FoodDecay = 1f;

    [Slider(0f, 2f, 20)]
    [Name("生肉衰减")]
    public float RawMeatDecay = 1f;

    [Slider(0f, 2f, 20)]
    [Name("熟肉衰减")]
    public float CookedMeatDecay = 1f;

    [Slider(0f, 2f, 20)]
    [Name("饮品衰减")]
    public float DrinksDecay = 1f;

    [Slider(0f, 2f, 20)]
    [Name("衣物衰减")]
    public float ClothingDecayRate = 1f;

    [Slider(0f, 2f, 20)]
    [Name("枪械衰减")]
    public float GunDecay = 1f;

    [Slider(0f, 2f, 20)]
    [Name("弓箭衰减")]
    public float BowDecay = 1f;

    [Slider(0f, 2f, 20)]
    [Name("工具衰减")]
    public float ToolsDecay = 1f;

    [Slider(0f, 2f, 20)]
    [Name("睡袋衰减")]
    public float BedrollDecay = 1f;

    [Slider(0f, 2f, 20)]
    [Name("雪橇衰减")]
    public float TravoisDecay = 1f;

    [Slider(0f, 2f, 20)]
    [Name("箭矢衰减")]
    public float ArrowDecay = 1f;

    [Slider(0f, 2f, 20)]
    [Name("陷阱衰减")]
    public float SnareDecay = 1f;

    [Slider(0f, 2f, 20)]
    [Name("生火工具衰减")]
    public float FirestartingDecay = 1f;

    [Slider(0f, 2f, 20)]
    [Name("腌肉衰减")]
    public float CuredMeatDecay = 1f;

    [Slider(0f, 2f, 20)]
    [Name("生鱼衰减")]
    public float RawFishDecay = 1f;

    [Slider(0f, 2f, 20)]
    [Name("熟鱼衰减")]
    public float CookedFishDecay = 1f;

    [Slider(0f, 2f, 20)]
    [Name("罐头食品衰减")]
    public float CannedFoodDecay = 1f;

    [Slider(0f, 2f, 20)]
    [Name("咖啡/茶衰减")]
    public float CoffeeTeaDecay = 1f;

    [Slider(0f, 2f, 20)]
    [Name("包装食品衰减")]
    public float PackagedFoodDecay = 1f;

    [Slider(0f, 2f, 20)]
    [Name("其他食品衰减")]
    public float OtherFoodDecay = 1f;

    [Slider(0f, 2f, 20)]
    [Name("油脂衰减")]
    public float FatDecay = 1f;

    [Slider(0f, 2f, 20)]
    [Name("腌鱼衰减")]
    public float CuredFishDecay = 1f;

    [Slider(0f, 2f, 20)]
    [Name("食材衰减")]
    public float IngredientsDecay = 1f;

    [Slider(0f, 2f, 20)]
    [Name("兽皮/尸骸衰减")]
    public float HideDecay = 1f;

    [Slider(0f, 2f, 20)]
    [Name("急救品衰减")]
    public float FirstAidDecay = 1f;

    [Slider(0f, 2f, 20)]
    [Name("净水片衰减")]
    public float WaterPurifierDecay = 1f;

    [Slider(0f, 2f, 20)]
    [Name("锅具衰减")]
    public float CookingPotDecay = 1f;

    [Slider(0f, 2f, 20)]
    [Name("信号枪弹药衰减")]
    public float FlareGunAmmoDecay = 1f;

    [Slider(0f, 2f, 20)]
    [Name("磨刀石使用衰减")]
    public float WhetstoneDecay = 1f;

    [Slider(0f, 2f, 20)]
    [Name("开罐器使用衰减")]
    public float CanOpenerDecay = 1f;

    [Slider(0f, 2f, 20)]
    [Name("撬棍使用衰减")]
    public float PrybarDecay = 1f;

    [Slider(0f, 2f, 20)]
    [Name("尸骸采集工具衰减")]
    [Description("刀/斧/锯在采集动物尸体时的衰减倍率")]
    public float BodyHarvestDecay = 1f;

    // ——— Integrated: ClothingTweaker2 ———
    [Slider(0f, 1f, 10)]
    [Name("衣物每日衰减率")]
    public float ClothingDecayDaily = 1f;

    [Slider(0f, 1f, 10)]
    [Name("衣物室内衰减")]
    public float ClothingDecayIndoors = 1f;

    [Slider(0f, 1f, 10)]
    [Name("衣物室外衰减")]
    public float ClothingDecayOutdoors = 1f;

    // ——— Integrated: TorchTweaker + KeroseneLampTweaks ———
    [Section("Light Sources / 光源")]
    [Slider(1f, 2880f, 100)]
    [Name("火把燃烧时间(分钟)")]
    public float TorchBurnMinutes = 1440f;

    [Slider(0f, 1f, 10)]
    [Name("火把最低品质")]
    public float TorchMinCondition = 0.6f;

    [Slider(0f, 1f, 10)]
    [Name("火把最高品质")]
    public float TorchMaxCondition = 0.9f;

    // ——— Integrated: KeroseneLampTweaks v2.4.1 (DIY 定制) ———
    [Slider(0f, 3f, 30)]
    [Name("油灯耗油倍率 (旧/0=无限)")]
    [Description("v3.0.4 后已废弃,保留兼容老存档。新版用 放置/手持 分离倍率")]
    public float LampBurnMultiplier = 1f;

    [Slider(0f, 2f, 21)]
    [Name("油灯放置消耗率")]
    [Description("放置时燃油消耗倍率。1=原版(4小时),0=无限燃烧,2=两倍消耗")]
    public float LampPlacedBurnMult = 1f;

    [Slider(0f, 2f, 21)]
    [Name("油灯手持消耗率")]
    [Description("手持时燃油消耗倍率。1=原版,0=无限,2=两倍")]
    public float LampHeldBurnMult = 1f;

    [Slider(0f, 2f, 21)]
    [Name("油灯开启耗损 %")]
    [Description("开启时一次性扣 HP 百分比。0=无损,2=每次开启 -2%")]
    public float LampTurnOnDecay = 0f;

    [Slider(0f, 1f, 21)]
    [Name("油灯持续耗损 %/h")]
    [Description("点亮后每小时耐久损耗 %。0=无,1=每小时 -1%")]
    public float LampOverTimeDecay = 0f;

    [Slider(0f, 100f, 21)]
    [Name("油灯耗油惩罚阈值 %")]
    [Description("HP 低于此阈值,耗油加速。0=关闭惩罚,80=80%以下开始")]
    public int LampConditionThreshold = 0;

    [Slider(0f, 200f, 21)]
    [Name("油灯最大耗油惩罚 %")]
    [Description("HP 接近 0 时额外耗油。100=两倍消耗,200=三倍")]
    public int LampMaxPenalty = 100;

    [Name("油灯静音(放置时)")]
    [Description("油灯放置(非手持)时不发出燃烧/嘶嘶声")]
    public bool LampMute = false;

    [Slider(0.5f, 5f, 45)]
    [Name("油灯光照范围倍率")]
    [Description("1=原版, >1 增大照亮距离, 山洞/夜晚更亮")]
    public float LampRangeMultiplier = 1f;



    // ——— Integrated: TorchTweaker 额外 ———
    [Name("禁用左键灭火把")]
    [Description("防止鼠标左键误熄火把")]
    public bool DisableTorchLeftClick = true;

    [Name("禁用左键灭油灯")]
    [Description("防止鼠标左键误熄油灯")]
    public bool DisableLampLeftClick = true;

    // ——— Integrated: HouseLights (室内灯光开关) ———
    [Section("House Lights / 室内灯光")]
    [Name("启用室内灯光")]
    [Description("非极光时也能通过开关点亮室内灯")]
    public bool HL_Enabled = true;

    [Name("室外场景启用")]
    [Description("室外地图也启用灯光(实验性,可能影响性能)")]
    public bool HL_EnableOutside = false;

    [Name("白光")]
    [Description("灯光颜色去饱和,呈白色")]
    public bool HL_WhiteLights = false;

    [Name("禁止闪烁")]
    [Description("极光灯光不闪烁,保持常亮")]
    public bool HL_NoFlicker = true;

    [Name("投射阴影")]
    [Description("灯光投射阴影(可能影响性能)")]
    public bool HL_CastShadows = false;

    [Name("灯光音效")]
    [Description("灯开启时发出嗡嗡声")]
    public bool HL_LightAudio = false;

    [Slider(0f, 3f, 31)]
    [Name("亮度")]
    [Description("灯光强度,0=灭 3=最亮")]
    public float HL_Intensity = 2f;

    [Slider(0f, 5f, 51)]
    [Name("范围倍率")]
    [Description("1=默认照射距离, >1 增大范围")]
    public float HL_RangeMultiplier = 1.4f;

    [Slider(10f, 75f, 65)]
    [Name("裁剪距离(m)")]
    [Description("超出此距离的灯光关闭以节省性能")]
    public float HL_CullDistance = 50f;

    [Slider(1f, 3f, 21)]
    [Name("交互距离(m)")]
    [Description("可操作开关的最大距离")]
    public float HL_InteractDistance = 1f;

    // ——— Integrated Batch 2: QoL & Startup ———
    [Section("QoL / 生活品质")]
    [Name("打开日志暂停游戏")]
    [Description("打开Panel_Log(日志/地图)时游戏时间暂停")]
    public bool PauseInJournal = true;

    [Name("跳过开场动画")]
    [Description("跳过启动免责声明、开场动画、隐藏新闻轮播")]
    public bool SkipIntro = true;

    [Name("静音主菜单美洲狮")]
    [Description("销毁主菜单的美洲狮叫声音效")]
    public bool MuteCougarMenuSound = true;

    [Name("车内保持玩家FOV")]
    [Description("进入车辆时使用当前FOV而非车辆默认FOV")]
    public bool VehicleKeepFov = true;

    [Name("允许丢弃不可丢物品")]
    [Description("相机/对讲机/断线钳等原本不可丢的物品变为可丢弃")]
    public bool DroppableUndroppables = true;

    [Name("关键物品保留重量")]
    [Description("关闭则相机/断线钳等重量归零;开启则保留原重量")]
    public bool ImportantWeight = true;

    [Name("收藏品可丢弃")]
    [Description("笔记/钥匙/科技背包等变为可丢弃(注意:笔记将不可阅读)")]
    public bool AllNote = false;

    [Name("记忆拆解工具")]
    [Description("拆解面板自动选中上次使用的工具")]
    public bool RememberBreakdownTool = true;

    [Name("车内自由视角")]
    [Description("扩大车内视角旋转范围")]
    public bool VehicleFreeLook = true;

    [Slider(30f, 180f, 30)]
    [Name("车内最大水平角度")]
    public float VehicleFreeLookYaw = 120f;

    [Slider(20f, 90f, 14)]
    [Name("车内最大俯仰角度")]
    public float VehicleFreeLookPitch = 60f;


    // ——— Integrated: ExtraGraphicsSettings 准星 ———
    [Name("启用瞄准准星修改")]
    [Description("瞄准时显示额外准星(石头/步枪/弓)")]
    public bool CrosshairEnabled = false;

    [Slider(0f, 1f, 20)]
    [Name("准星透明度")]
    public float CrosshairAlpha = 1f;

    [Name("石块准星")]
    public bool CrosshairStone = false;

    [Name("步枪准星")]
    public bool CrosshairRifle = false;

    [Name("弓箭准星")]
    public bool CrosshairBow = false;

    // ═══════════════════════════════════════════════════════════════════════════
    // v2.8.0  Tab 6: QoL (移植自 QoL.dll + TinyTweaks + SleepWithoutABed + AutoSurvey)
    // ═══════════════════════════════════════════════════════════════════════════

    [Section("QoL: General")]
    [Name("QoL 总开关")]
    public bool QoL_Enabled = true;


    [Section("QoL: TinyTweaks / 小改动")]
    [Name("扭伤不自动存档")]
    public bool QoL_NoSaveOnSprain = false;
    [Name("坠落扭伤也不存档")]
    [Description("坠落引起的扭伤同样不触发自动存档")]
    public bool QoL_NoSaveOnSprainFalls = false;
    [Name("睡眠中可醒来")]
    [Description("暂停菜单里可以选择醒来")]
    public bool QoL_WakeUpCall = false;
    [Name("极光感应(睡眠时极光自动醒)")]
    public bool QoL_AuroraSense = true;
    [Name("睡眠时显示时间")]
    public bool QoL_ShowTimeSleep = false;
    [Name("睡眠不全黑")]
    public bool QoL_NoPitchBlack = false;
    [Choice("黑字白描边", "黑字棕描边", "白字黑描边", "浅字黑描边")]
    [Name("地图文字样式")]
    public int QoL_MapTextOutline = 0;
    [Name("启用地图文字样式修改")]
    public bool QoL_MapTextOutlineEnabled = false;
    [Name("埋葬尸体")]
    [Description("对人类/动物尸体按 Alt+Fire 埋葬")]
    public bool QoL_BuryCorpses = false;

    [Section("QoL: Sleep Anywhere / 随地睡")]
    [Name("无睡袋也能睡觉")]
    public bool QoL_SleepAnywhere = false;
    [Slider(0.15f, 1f, 17)]
    [Name("疲劳恢复惩罚")]
    public float QoL_SleepFatigueRecovery = 0.75f;
    [Slider(0.05f, 1f, 19)]
    [Name("状态恢复倍率")]
    public float QoL_SleepConditionRecovery = 0.50f;
    [Slider(1f, 3f, 20)]
    [Name("受冻加速倍率")]
    public float QoL_SleepFreezingScale = 1.75f;
    [Slider(1f, 2f, 20)]
    [Name("冻伤生命损失倍率")]
    public float QoL_SleepFreezingHealthLoss = 1.2f;
    [Slider(1f, 2f, 20)]
    [Name("低温症生命损失倍率")]
    public float QoL_SleepHypothermicHealthLoss = 1.4f;
    [Slider(0.25f, 2f, 35)]
    [Name("消磨时间暴露惩罚")]
    public float QoL_SleepPassTimeExposure = 0.75f;
    [Name("低血量中断睡眠")]
    public bool QoL_SleepInterrupt = true;
    [Slider(0.05f, 0.2f, 15)]
    [Name("中断血量阈值%")]
    public float QoL_SleepInterruptThreshold = 0.10f;
    [Slider(1f, 60f, 59)]
    [Name("中断冷却(秒)")]
    public float QoL_SleepInterruptCooldown = 15f;
    [Name("中断时显示HUD提示")]
    public bool QoL_SleepInterruptHudMsg = true;
    [Name("对所有床铺启用中断")]
    public bool QoL_SleepInterruptAllBeds = true;
    [Slider(0.01f, 1f, 99)]
    [Name("暴露灵敏度系数")]
    public float QoL_SleepSensitivityScale = 0.2f;
    [Slider(0.01f, 2f, 199)]
    [Name("修正灵敏度")]
    public float QoL_SleepAdjustedSensitivity = 0.75f;

    [Section("QoL: AutoSurvey / 自动绘图")]
    [Name("自动绘制地图")]
    public bool QoL_AutoSurvey = false;
    [Slider(1f, 120f, 23)]
    [Name("绘制间隔(秒)")]
    public float QoL_AutoSurveyDelay = 10f;
    [Slider(0f, 10f, 20)]
    [Name("绘制范围倍率")]
    public float QoL_AutoSurveyRange = 1f;
    [Name("解除天气限制")]
    public bool QoL_AutoSurveyUnlock = false;

    [Name("保温瓶增强")]
    [Description("快捷轮盘喝水/背包分类显示/温度指示优化")]
    public bool QoL_ImprovedFlasks = true;

    // ═══════════════════════════════════════════════════════════════════════════
    // v2.8.0  Tab 8: Crafting & Fire (CraftAnywhereRedux + MoreCookingSlots)
    // ═══════════════════════════════════════════════════════════════════════════

    [Section("Craft Anywhere / 随意制作")]
    [Name("启用随意制作")]
    [Description("修改每个蓝图的制作位置要求")]
    public bool Craft_Anywhere = false;
    [Choice("任意", "工作台", "锻造台", "弹药台", "火堆")]
    [Name("默认位置(新蓝图)")]
    public int Craft_DefaultLocation = 0;

    [Section("More Cooking Slots / 更多烹饪位")]
    [Name("启用额外烹饪位")]
    public bool Craft_MoreCookingSlots = false;
    [Name("壁炉(2→3)")]
    public bool Craft_MoreSlots_Fireplace = true;
    [Name("火桶(2→4)")]
    public bool Craft_MoreSlots_Barrel = true;
    [Name("烤架(2→4)")]
    public bool Craft_MoreSlots_Grill = true;

    // ═══════════════════════════════════════════════════════════════════════════
    // v2.8.0  Tab 9: World & Items (移植自多个 mod)
    // ═══════════════════════════════════════════════════════════════════════════

    [Section("Sprainkle / 扭伤系统")]
    [Name("启用扭伤参数调整")]
    [Description("启用后用下方参数替代简单的'无扭伤风险'开关")]
    public bool World_Sprainkle = false;
    [Choice("原版", "自定义", "增强(高难)")]
    [Name("预设")]
    public int World_SprainklePreset = 0;
    [Slider(10f, 60f, 10)]
    [Name("最小坡度(度)")]
    public float World_SprainkleSlopeMin = 30f;
    [Slider(0.5f, 5f, 9)]
    [Name("坡度递增系数")]
    public float World_SprainkleSlopeIncrease = 1.5f;
    [Slider(0f, 50f, 50)]
    [Name("移动基础概率%")]
    public float World_SprainkleBaseChanceMoving = 15f;
    [Slider(0f, 1f, 20)]
    [Name("负重加成概率")]
    public float World_SprainkleEncumberChance = 0.3f;
    [Slider(0f, 1f, 20)]
    [Name("疲劳加成概率")]
    public float World_SprainkleExhaustionChance = 0.3f;
    [Slider(0f, 10f, 20)]
    [Name("冲刺加成概率")]
    public float World_SprainkleSprintChance = 2f;
    [Slider(0f, 100f, 20)]
    [Name("蹲行减免%")]
    public float World_SprainkleCrouchChance = 75f;
    [Slider(0f, 10f, 20)]
    [Name("最短触发间隔(秒)")]
    public float World_SprainkleMinSecondsRisk = 1.5f;
    [Slider(0f, 100f, 20)]
    [Name("手腕移动触发%")]
    public float World_SprainkleWristMovementChance = 50f;
    [Slider(0f, 1f, 10)]
    [Name("冲刺UI阈值(显示)")]
    public float World_SprainkleSprintUIOn = 0.5f;
    [Slider(0f, 1f, 10)]
    [Name("冲刺UI阈值(隐藏)")]
    public float World_SprainkleSprintUIOff = 0.3f;
    [Name("脚踝扭伤启用")]
    public bool World_SprainkleAnkleEnabled = true;
    [Slider(1f, 168f, 33)]
    [Name("脚踝最短时长(h)")]
    public float World_SprainkleAnkleDurMin = 48f;
    [Slider(1f, 168f, 33)]
    [Name("脚踝最长时长(h)")]
    public float World_SprainkleAnkleDurMax = 72f;
    [Slider(0f, 24f, 24)]
    [Name("脚踝休息恢复(h)")]
    public float World_SprainkleAnkleRestHours = 4f;
    [Slider(0f, 100f, 20)]
    [Name("脚踝坠落概率%")]
    public float World_SprainkleAnkleFallChance = 35f;
    [Name("手腕扭伤启用")]
    public bool World_SprainkleWristEnabled = true;
    [Slider(1f, 168f, 33)]
    [Name("手腕最短时长(h)")]
    public float World_SprainkleWristDurMin = 48f;
    [Slider(1f, 168f, 33)]
    [Name("手腕最长时长(h)")]
    public float World_SprainkleWristDurMax = 72f;
    [Slider(0f, 24f, 24)]
    [Name("手腕休息恢复(h)")]
    public float World_SprainkleWristRestHours = 2f;
    [Slider(0f, 100f, 20)]
    [Name("手腕坠落概率%")]
    public float World_SprainkleWristFallChance = 35f;

    // ——— v3.0.4 Integrated: PlaceFromInventory v1.1.3 (微操放置) ———
    [Section("PlaceFromInv / 微操放置 v1.1.3")]
    [Name("启用背包右键放置")]
    [Description("背包/衣物面板右键物品 → 进入放置模式")]
    public bool PlaceFromInv_Enabled = true;
    [Name("允许物品贴近放置")]
    [Description("解除原版'离物体太近'放置限制")]
    public bool PlaceFromInv_AllowClose = true;
    [Name("Ctrl 整堆丢出")]
    [Description("Pick Units 面板按住 Ctrl 时按 stack 丢出而非单件")]
    public bool PlaceFromInv_StackDrop = true;

    // ——— v3.0.4 Integrated: UniversalTweaks v1.4.8 (chunk 3a 中型功能集) ———
    // 跳过已有重复:InfiniteEncumberWeight/InfiniteContainerWeight/PermanentCrosshair/RemoveMainMenuItems

    [Section("UT / 常用修改 v1.4.8")]
    [Name("启用哈气效果")]
    [Description("关闭则不显示寒冷天气下口鼻哈气特效")]
    public bool UT_BreathVisibility = true;
    [Name("胡须地衣绷带统一重量(0.03kg)")]
    [Description("把 OldMansBeardDressing 重量改为 0.03kg")]
    public bool UT_ConsistantDressingWeight = false;
    [Name("自定义模式徽章进度")]
    [Description("自定义难度也能解锁徽章进度")]
    public bool UT_FeatProgressInCustom = false;
    [Name("物品掉落随机旋转")]
    [Description("Drop 物品时朝向随机化")]
    public bool UT_RandomizedItemRotation = false;
    [Name("左轮操作改良")]
    [Description("瞄准状态下也可以走动并隐藏限制 UI")]
    public bool UT_RevolverImprovements = false;
    [Slider(0.1f, 5f, 50)]
    [Name("制噪器引信燃烧时间(分)")]
    public float UT_NoisemakerBurnLength = 0.7f;
    [Slider(1f, 30f, 30)]
    [Name("制噪器投掷力度")]
    public int UT_NoisemakerThrowForce = 9;
    [Slider(5f, 240f, 48)]
    [Name("滤罐持续时间(秒)")]
    public int UT_RespiratorCanisterDuration = 45;
    [Slider(0f, 200f, 21)]
    [Name("雪屋每日损耗 HP")]
    [Description("100=原版,0=不损耗")]
    public int UT_SnowShelterDecay = 100;
    [Name("马桶水可饮用")]
    [Description("马桶水设为 Potable(无需净化)")]
    public bool UT_ToiletWaterPotable = false;

    [Section("UT / 食物修改")]
    [Name("移除甜点头疼 Debuff")]
    [Description("CookedPiePeach/RoseHip/PorridgeFruit/PancakePeach 不再头疼")]
    public bool UT_RemoveHeadacheDebuff = false;
    [Slider(0f, 50f, 26)]
    [Name("炖汤减疲劳量")]
    [Description("CookedStewMeat/Vegetables 的 Effect 值")]
    public int UT_StewFatigueLoss = 15;

    [Section("UT / 岩石贮藏处")]
    [Name("允许室内放置")]
    public bool UT_RockCacheIndoors = false;
    [Slider(1f, 50f, 50)]
    [Name("每区域最大数量")]
    public int UT_RockCacheMaxPerRegion = 5;
    [Slider(1f, 100f, 100)]
    [Name("最小间距(米)")]
    public float UT_RockCacheMinDistance = 10f;

    [Section("UT / 喷漆")]
    [Slider(0f, 1f, 21)]
    [Name("贴图重叠率")]
    public float UT_DecalOverlap = 0.2f;
    [Name("启用高亮喷漆")]
    public bool UT_GlowingDecals = false;
    [Slider(0f, 10f, 21)]
    [Name("高亮倍数")]
    public float UT_GlowingDecalMult = 1f;

    [Section("UT / 雪橇")]
    [Slider(0f, 100f, 21)]
    [Name("暴风雪损耗 HP/h")]
    public int UT_TravoisDecayBlizzard = 3;
    [Slider(0f, 100f, 21)]
    [Name("常态损耗 HP/h(/1000)")]
    public int UT_TravoisDecayHourly = 10;
    [Slider(0f, 100f, 21)]
    [Name("移动损耗/件物品(/100)")]
    public int UT_TravoisDecayMovement = 5;
    [Slider(0f, 90f, 19)]
    [Name("最大坡度角")]
    public int UT_TravoisMaxSlope = 35;
    [Slider(0.1f, 5f, 50)]
    [Name("转向速度")]
    public float UT_TravoisTurnSpeed = 0.5f;
    [Name("解除移动区域限制")]
    public bool UT_TravoisOverrideMovement = false;
    [Name("解除互动限制")]
    public bool UT_TravoisOverrideInteraction = false;

    [Section("UT / 手电筒")]
    [Name("无限电量")]
    [Description("手电筒永不耗电")]
    public bool UT_FlashInfiniteBattery = false;

    [Section("StackManager / 堆叠管理 v1.0.6")]
    [Name("启用堆叠组件添加")]
    [Description("给 Coffee/茶/胡萝卜/土豆等指定物品添加可堆叠")]
    public bool Stack_AddComponent = true;
    [Name("使用最大状态合并")]
    [Description("堆叠时按更高 HP 计算(8 张鹿皮按最佳那张)")]
    public bool Stack_UseMaxHP = true;


    [Section("Bow Repair / 弓修复")]
    [Name("允许修复弓")]
    [Description("给弓类武器添加修理/磨坊选项")]
    public bool World_BowRepair = false;
    [Name("DLC弓启用")]
    [Description("是否允许修复DLC弓(猎弓/灌木弓)")]
    public bool World_BowRepairDLC = false;
    [Choice("手修", "磨坊", "都可")]
    [Name("生存弓模式")]
    public int World_BowRepairMode = 2;
    [Choice("低", "中", "高")]
    [Name("生存弓材料需求")]
    public int World_BowMaterialNeed = 1;
    [Choice("手修", "磨坊", "都可")]
    [Name("运动弓模式")]
    public int World_SportBowRepairMode = 1;
    [Choice("低", "中", "高")]
    [Name("运动弓材料需求")]
    public int World_SportBowMaterialNeed = 1;
    [Choice("手修", "磨坊", "都可")]
    [Name("木弓模式")]
    public int World_WoodBowRepairMode = 2;
    [Choice("低", "中", "高")]
    [Name("木弓材料需求")]
    public int World_WoodBowMaterialNeed = 0;
    [Choice("手修", "磨坊", "都可")]
    [Name("灌木弓模式")]
    public int World_BushBowRepairMode = 2;
    [Choice("低", "中", "高")]
    [Name("灌木弓材料需求")]
    public int World_BushBowMaterialNeed = 1;

    [Section("Caffeinated Sodas / 苏打提神")]
    [Name("苏打水减疲劳")]
    [Description("喝苏打水获得减疲劳效果")]
    public bool World_CaffeinatedSodas = false;
    [Name("橙味启用")]
    public bool World_SodaOrangeEnabled = true;
    [Slider(1f, 15f, 14)]
    [Name("橙味减疲劳%")]
    public float World_SodaOrangeInitial = 7f;
    [Choice("5分钟", "10分钟", "15分钟", "30分钟")]
    [Name("橙味持续")]
    public int World_SodaOrangeDuration = 1;
    [Name("Summit启用")]
    public bool World_SodaSummitEnabled = true;
    [Slider(1f, 15f, 14)]
    [Name("Summit减疲劳%")]
    public float World_SodaSummitInitial = 3f;
    [Choice("5分钟", "10分钟", "15分钟", "30分钟")]
    [Name("Summit持续")]
    public int World_SodaSummitDuration = 0;
    [Name("葡萄味启用")]
    public bool World_SodaGrapeEnabled = true;
    [Slider(1f, 15f, 14)]
    [Name("葡萄味减疲劳%")]
    public float World_SodaGrapeInitial = 5f;
    [Choice("5分钟", "10分钟", "15分钟", "30分钟")]
    [Name("葡萄味持续")]
    public int World_SodaGrapeDuration = 1;

    [Section("RnStripped / 尸骸搬运&极光点火")]
    [Name("搬运猎物尸骸")]
    [Description("可拾取鹿/狼尸体搬运到其他地方(含跨场景)")]
    public bool World_CarcassMoving = false;
    [Name("搬运所有猎物")]
    [Description("开启后熊/驼鹿/美洲狮也可搬运")]
    public bool World_CarcassMovingAll = false;
    [Name("极光点火把")]
    [Description("极光期间可从电源插座/电线点燃火把")]
    public bool World_ElectricTorch = false;

    // ——— TinyTweaks 整合 ———
    [Section("TinyTweaks / 微调合集")]
    [Name("体感温度上限(℃)")]
    [Description("0=不限制")]
    [Slider(-10, 50)]
    public int TT_CapFeelsHigh = 0;
    [Name("体感温度下限(℃)")]
    [Description("0=不限制")]
    [Slider(-50, 10)]
    public int TT_CapFeelsLow = 0;
    [Name("启用体感温度限制")]
    public bool TT_CapFeelsEnabled = false;

    [Name("无视坠落即死墙")]
    public bool TT_FallDeathGoat = false;
    [Name("坠落伤害/米")]
    [Description("原版=3")]
    [Slider(1, 12)]
    public int TT_FallDamageMult = 6;

    [Name("步枪落地竖立")]
    public bool TT_DroppedOrientation = true;
    [Name("FOV滑块扩展(30-150)")]
    public bool TT_ExtendedFOV = true;
    [Name("辐射轮暂停")]
    public bool TT_PauseOnRadial = false;

    [Name("交互加速-通用")]
    [Description("修理/制作/烹饪/净水/雪屋等")]
    [Slider(0.2f, 6f, 30)]
    public float TT_GlobalSpeedMult = 1f;
    [Name("交互加速-开容器")]
    [Description("开箱/采植物/进车/开门")]
    [Slider(0.2f, 6f, 30)]
    public float TT_InteractionSpeedMult = 1f;
    [Name("交互加速-进食")]
    [Slider(0.2f, 6f, 30)]
    public float TT_EatingSpeedMult = 1f;
    [Name("交互加速-拆解")]
    [Slider(0.2f, 6f, 30)]
    public float TT_BreakdownSpeedMult = 1f;
    [Name("交互加速-阅读")]
    [Slider(0.2f, 6f, 30)]
    public float TT_ReadingSpeedMult = 1f;

    [Name("植物重生(天)")]
    [Slider(1, 365)]
    public int TT_PlantRespawnDays = 45;
    [Name("启用植物重生")]
    public bool TT_RespawnPlants = false;

    [Name("显示商人信任度")]
    public bool TT_ShowTraderTrust = false;

    [Name("商人到来天数")]
    [Description("游戏内经过该天数后商人可用。原版约25-30天，这里可以提前")]
    [Slider(1f, 40f, 40)]
    public float TraderArrivalDays = 10f;

}
