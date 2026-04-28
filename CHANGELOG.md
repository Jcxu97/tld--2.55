# 改动记录 vs 原整合包

基准:`【2026-4-21】V2.54MOD整合包 / 【V2.54整合包】/【第二步】/Mods`
当前:`D:\Steam\steamapps\common\TheLongDark\Mods`(TLD 2.55)

## 从整合包删除的 mod(6 个)

| DLL | 备注 |
|---|---|
| `DarkerNights.dll` | ⚠️ **必须删** —— 最后更新 2023-01-24,对应 TLD 2.06;在 2.55 启动时死循环卡住 mod 初始化。详见 `README.md` 已知问题段 |
| `CatchColdMod.dll` | 未装 |
| `OxygenLevels.dll` | 未装 |
| `PhotoOfALovedOne.dll` | 未装 |
| `ReducedLoot.dll` | 未装 |
| `TheLongMood.dll` | 未装 |

## 额外装的 mod(7 个)

| DLL | 来源 / 用途 |
|---|---|
| `BunkerDefaults.dll` | **本仓库原创** —— 给每个存档预填 9 个地堡 FastTravel 传送点 |
| `FoodStackable.dll` | **本仓库原创**(v0.4.9,已并入 TldHacks v2.7.0)—— 背包里同类食物视觉堆叠成一格 ×N,不改底层物品 |
| `TldHacks.dll` | **本仓库原创**(v2.7.0)—— 综合修改器。替代 FoodStackable(含其全部功能)+ 移植 Cheat Engine 表 ~60 个 cheat:无敌/免疫/无限体力/瞄准稳定/无后坐力/快速制作/生火 100%/爬绳 ×5/15 region 传送/358 条物品刷出/技能满级/解锁壮举·蓝图·地图 等 |
| `ModSettingsQuickNav.dll` | **本仓库原创**(v1.0.0)—— ModSettings 面板快速跳转:按 `` ` `` 打开 IMGUI 浮层,A-Z 字母跳段 + 滚动列表点一下直接切到对应 mod 设置。Harmony patch `ModSettingsGUI.OnEnable/OnDisable`(internal 类走 `AccessTools.TypeByName` 反射) |
| `AudioCore.dll` | 某个 mod 的依赖 |
| `ImprovedTrader.dll` | 优化商人交互 |
| `RecipeRequirements.dll` | 食谱需求调整 |

## 额外的 modcomponent

- `Rare-ExclusiveLoot.modcomponent`
- `ZC8787JerkyChipsblueprints.modcomponent`

## 主要配置改动

### `Mods/FastTravel.json`
- `OnlyBetweenDestinations`: `true` → **`false`**
  - 原设定要求当前位置也是已保存传送点才让你传
  - 改后可以从**任意位置**传到 9 个地堡
- `LogDebugInfo`: 保持 `false`(debug 太吵)

### `Mods/StackManager/config.json`
加了以下物品到 `STACK_MERGE` + `AddStackableComponent`:
- `GEAR_Soda` — 苏打水
- `GEAR_SodaEnergy` — 能量饮料
- `GEAR_BeefJerky` — 牛肉干
- `GEAR_CannedBeans` — 罐装豆子
- `GEAR_SprayPaintCan` — 喷漆罐

⚠️ 只对**新生成**的物品生效;已经在存档里的老物品还是不能叠。

### `Mods/ItemPickerCustomList.txt`
自动拾取列表从 ~130 行扩到 **627 行**(2026-04-27 三批累积:68 件衣服 + 25 件杂项 + 全量批扫 catalog.json 补齐 246 条),新增类别:
- **弹药/武器耗材**:`Bullet`、`GunpowderCan`、`ScrapLead`、`ArrowHead`、`Accelerant`、`RifleCleaningKit`、`SharpeningStone` 等
- **建材/工具**:`Prybar`、`SimpleTools`、`SewingKit`、`BeeHive`、`Flint`、`WireBundle`、`CarBattery`、`Battery9V`、`Fuse`、`ScrapPlastic`、`ElectronicParts`、`GlassShards`、`NutsNBolts`、`NutsNBoltsBox`、`TarpSheet`、`Charcoal`、`SprayPaintCan`、`FlareA`、`BlueFlare`、`TapeRoll`、`BottleHydrogenPeroxide` 等
- **食物/饮料**:`CuredMeat*`(熊/鸟/驼鹿/虎鲸/雷鸟/兔/鹿/狼)、`SaltedMeat*`(同)、`CuredFish*` / `SaltedFish*`(9 种鱼)、`BirdMeatRaw/Cooked`、`BirdEggRaw/Boiled`、`OrcaMeatRaw/Cooked`、`CannedChili/Stew/Spaghetti/Pears/Mangos/Pineapples/Beans`、`Cooked*`(饼/炖菜/煎蛋/披萨/三明治)、`BabyFood*`、`Tea*`、`CoffeeCupSugar` 等
- **耗材**:`PineNeedle` 系列、`PineNeedleDried`、`FilmBoxColour/BW/Sepia`、`MapleSyrup`、`GranolaBar`、`CondensedMilk`、`BariumCarbonate`、`LampFuel`、`PackMatches`、`WoolSocks`、`BasicBoots`、`FleeceSweater` 等
- **原版遗漏的罐装**:`Soda`、`SodaEnergy`、`KetchupChips`、`AuroraEnergyDrink`、`CheeseDoodles`、`SwedishMeatballs`、`icecreamCup` 等
- **衣服(2026-04-27 新增 68 件)**:外套/Parka(14)、马甲(3)、毛衣/卫衣(6)、衬衫(4)、裤子(8)、靴子/鞋(11)、袜子(2)、手套(9)、帽子(7)、围巾/头套(4)。包含 `BearSkinCoat` / `WolfSkinCape` / `Improvised*` 等自制装备变体。prefab 名全量来源:`tld_Data/StreamingAssets/aa/catalog.json`(848 条)。
- **杂项补漏(2026-04-27 累计)**:`SodaGrape`、`DogFood`、`Hacksaw`、`CanOpener`、`RecycledCan`、`HeavyBandage`、`MagnifyingLens`、`CandyBar`、`MashedPotatoes`、`EmergencyStim`、`HighQualityTools`、`HatchetImprovised`、`Hammer`、`Rope`、`RevolverAmmoBox`、`MixedNuts`、`PinnacleCanPeaches`、`InsulatedFlask_C`、`Hatchet`、`Jeans`、`Toque`、`LongUnderwear`、`Flour`、`CookingPot`、`Skillet`。

**全量批扫(2026-04-27 最后一次)**:从 `tld_Data/StreamingAssets/aa/catalog.json` 抽 848 个 GEAR_ prefab,去掉 `_Mat/_Dif` 材质后,差集补齐 246 条。明确排除(122 条)**会破坏游戏的剧情物**:
- 剧情钥匙(~20:`BIKey1/2`、`LakeCabinKey_*`、`BankManagerHouseKey`、`DepositBoxKey` 等)— 自动捡可能阻断剧情
- 剧情笔记(~70:`BackerNote*`、`VisorNote*`、`Blackrock*Note`、`DeadmanNote1-5` 等)— 挤背包
- 明信片(21:`PostCard_*`)— 收集品
- 尸体(3:`*Carcass`)— 要屠宰不是捡(`*Quarter` 肉块保留)
- 水壶系统(4:`WaterSupply*`、`WaterBottle*`)— 水不走物品拾取
- 占位/生物/剧情物(4:`NULL`、`Stalker`、`ElevatorCrank`、大背包 `BackPack_A*`)

完整表在 `configs/Mods/ItemPickerCustomList.txt`

### 其他自动生成的 mod 配置
- `Mods/AmbientLights.json` — 环境光默认值(mod 自动写的)
- `Mods/UniversalTweaks.json` — UniversalTweaks 调参(mod 自动写的)
- `Mods/MapManager.json` — 地图管理默认配置
- `Mods/ImprovedTrader.json` / `SilentWalker.json` / `PrepperCache.json` — mod 默认配置

### `Mods/ModData/*.moddata`(不在仓库里,每个存档各自的)
BunkerDefaults 会在运行时预填 9 个 FastTravel 地堡传送点。不包进仓库因为 moddata 是你自己的存档数据。

## 已知整合包级问题(不是我改出来的)

- **切场景偶发闪退 → 重启游戏卡在 `CraftAnywhere is online` 日志行**:117-mod 整合包通病。社区解法:
  - 切场景前按 `F5` 快存(SaveManager 提供)
  - 启动卡死就反复重启游戏,不行就重启电脑
  - 耐心等,有时 Unity 后台还在加载只是 MelonLoader 日志停了
- **CampingTools v2.2.1 报 MissingMethodException**:ExamineActionsAPI v2.0.7 接口改了签名,CampingTools 没跟上。不致命(CampingTools 自己挂,其他 mod 照跑),但 CampingTools 功能用不了。
