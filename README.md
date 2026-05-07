# tld 自用改造 2.55

**The Long Dark 2.55** 个人定制层 —— 基于 117-mod 整合包之上的配置快照 + 自制 mod。

> ⚠️ **只含配置和我写的 mod,不含第三方 mod 本体。** 要用这个仓库,你得先装好你用的那个 117-mod 整合包,然后把这里的 `configs/Mods/*` 覆盖过去。第三方 mod 的版权属于各自作者。

## 包含什么

```
├── BunkerDefaults/         ← 我原创的 mod,可自由使用/修改
│   ├── BunkerDefaults.dll      编译好的,直接丢 Mods/ 就行
│   └── src/                    C# 源码 + 工程,dotnet build 即可
├── FoodStackable/          ← 我原创的 mod,已并入 TldHacks(这里保留历史版本)
│   ├── FoodStackable.dll       编译好的,直接丢 Mods/ 就行
│   └── src/                    C# 源码 + 工程,dotnet build 即可
├── TldHacks/               ← 我原创的综合修改器,合并了 FoodStackable 堆叠 + 完整 CT 作弊功能
│   ├── TldHacks.dll            编译好的,直接丢 Mods/ 就行(装 v2.7.0 就不用装 FoodStackable)
│   └── src/                    C# 源码(13 文件)+ HANDOFF.md 详细开发日志
├── ModSettingsQuickNav/    ← 我原创的 mod,给 ModSettings.dll 加"快速跳转"列表
│   ├── ModSettingsQuickNav.dll
│   └── src/
├── configs/                ← 所有 mod 的配置快照(203 个文件)
│   ├── Mods/                   Mods/ 下所有 *.json / *.txt(不含 .dll)
│   └── UserData/               MelonLoader 全局偏好
├── tools/
│   └── PopulateFastTravel.ps1  PS 脚本:手动批量注入 FastTravel 传送点
├── CHANGELOG.md            ← 对原 mod 做了哪些具体改动
└── LICENSE
```

## 不包含什么(为什么没带)

| 类型 | 原因 |
|---|---|
| 117 个整合包的 `.dll` / `.modcomponent` | 都是别人的代码,没作者许可不能分发 |
| MelonLoader 本体 | 从 [LavaGang/MelonLoader](https://github.com/LavaGang/MelonLoader) 官方下 |
| 游戏本体 | 买 |
| 存档 (`sandbox*` / `*.moddata`) | 私人数据 |

## BunkerDefaults — 自制 mod

**解决什么问题:** Pathoschild 的 FastTravel mod 要你**先走到**才能保存一个传送点。我这个 mod 在游戏每次启动时扫描 `Mods/ModData/*.moddata`,自动往 FastTravel 存档里**预填 9 个大地图地堡坐标**,新开存档也不用自己跑。

**热键(继承 FastTravel 的)— v1.0.3 按偏好重排**:

| 键 | 目的地 | 场景名 | 默认坐标 |
|---|---|---|---|
| `5` | 废弃机场应急舱 | AirfieldRegion | **空,走到存** |
| `6` | 神秘湖地堡 | LakeRegion | ✅ |
| `7` | 沿海公路加油站 | CoastalRegion | **空,走到存** |
| `8` | 山间小镇地堡 | MountainTownRegion | ✅ |
| `9` | 荒凉水湾地堡 | CanneryRegion | ✅ |
| `F2` | 荒芜据点 | WhalingStationRegion | **空,走到存** |
| `F3` | 黑岩地区地堡 | BlackrockRegion | ✅ |
| `F4` | 灰烬峡谷地堡 | AshCanyonRegion | ✅ |
| `F7` | 林狼雪岭地堡 | MountainPassRegion | **空,走到存**(社区坐标 Y=207 掉崖) |

5 个 `✅` 默认可用。4 个 `空` 首次用前走到对应场景,按 **`=` (Equals) + 对应数字键** 覆盖真实坐标。CheatEngine 社区表的 MountainPass / Marsh 坐标实测都会掉崖 / 地下,已从默认注入移除。Rural / RiverValley / Marsh 按用户偏好也从默认移除(需要时自己走去按保存键)。

**新开存档时的流程(重要):**
1. 建新沙盒,玩到触发一次存档(睡觉 / `ESC→Save` / 过场景)
2. BunkerDefaults 会在下次切场景时自动往 `<save>.moddata` 注入 9 个传送点
3. **`ESC → Main Menu → 重新载入存档`** —— 让 FastTravel 刷新内存缓存
4. 按 `5` 直接传神秘湖地堡

第 3 步的重载**每个新存档只需做一次**,之后永远好用。

## FoodStackable — 自制 mod

**解决什么问题:** 原版背包里每份食物(Jerky / CannedBeans / Soda 等)占一格,捡 6 份就是 6 格。视觉很乱。

**做法:** Hook `Panel_Inventory.RefreshTable`,把 `m_FilteredInventoryList` 里同类(同 prefab + 同开罐状态)的物品去重,只显示一个代表格 + 右下角 `x6` 数字角标(复用游戏内置 `m_StackLabel`)。

**不碰底层** —— `Inventory.m_Items` 里 6 份 GearItem 一份没动:
- 吃一份 → 剩 5 份,下次刷新显示 `x5`
- 每份各自按自己的 condition 腐坏,存档干净
- 丢弃/装箱只影响代表那一份

**Bonus:** 代表 = 最低 condition 那一份(游戏 filter 天然按 condition 升序排 + 我的 first-wins 策略)→ 点吃优先消化快坏的,符合生存最优解。

**跳过**:纯液体(LampFuel / JerrycanRusty / 水)和 Soda 双料,游戏原版已处理。

**更新记录:**
- `v0.4.0` (2026-04-27) 首版,UI 去重 + `x N` 角标
- `v0.4.1` (2026-04-27) 修:重量标签显示单份 → 改为 `单份 × N`(公英制自动识别)
- `v0.4.2` (2026-04-27) 跳过原生 `StackableItem.m_Units > 1`;黑名单 `MixedNuts` / `MashedPotatoes` / `WaterPurificationTablets` / 两种香烟;前缀 `GEAR_cc*` 跳过卡牌
- `v0.4.3` (2026-04-27) hook `Panel_Container.RefreshTables` → 容器视图也堆叠;dict key 由 `gi.Pointer` 改成 `dataItem.Pointer`,修"容器里单个物品错标 ×2"bug
- `v0.4.4` (2026-04-27) key 从 `m_GearItemData.name` 改成 `gi.name`(GameObject 名),修"多个不同 prefab 因共用 GearItemData asset 被误合并"(例:roastedAlmonds 吞并多种食物,DryMilkPacket 和 HotCocoaBox 合成一格)
- `v0.4.5` (2026-04-27) 和整合包 StackManager 的兼容性收紧:**所有**挂了 `StackableItem` 组件的物品(不管 `m_Units` 多少)都完全让 StackManager / 原版处理;gi.name 剥 `(Clone)` 后缀确保黑名单精确匹配
- `v0.4.6` (2026-04-27) hook `InventoryGridItem.OnClick` / `ToggleSelection` Postfix 恢复 `×N` 角标(点击物品后游戏会清 `m_StackLabel`,之前 bug 导致点一下角标就没了);`StackableItem` 兜底从 v0.4.5 的过严收回到 `m_Units>1` —— 挂组件但没真合过的物品(DryMilkPacket / MixedNuts)恢复 UI 堆叠;`MixedNuts` 从黑名单移除
- `v0.4.7` (2026-04-27) v0.4.6 的 OnClick/ToggleSelection 只盖到部分清 label 路径(用户报 bug 减轻但未彻底)。改成 hook `InventoryGridItem.Update()` 的 Postfix 每帧兜底 — **但在 Il2CppInterop 下 Harmony hook 不到 Unity 引擎隐式调用的 MonoBehaviour.Update,`CallerCount(0)` 确认无游戏代码调用,这个 hook 实际没生效**
- `v0.4.8` (2026-04-27) 彻底修 v0.4.7 未解的 bug。改用 `MelonMod.OnUpdate()`(Melon 框架自己每帧可靠调用)+ `StackState.SeenItems` dict 记录所有刷新过的 `InventoryGridItem`,每帧遍历 reapply label。Panel_Container 不暴露 grid item 数组不是问题,SeenItems 自动收集所有 panel 的 items
- `v0.4.9` (2026-04-27) 点击角标丢失 + 拖动后重量变单份。`OnUpdate` 跑在 Unity `MonoBehaviour.Update` 之前,被游戏之后的 Update 覆盖。改用 `OnLateUpdate`(所有 Update 之后跑);SeenItems 存 `(item, dataItem)` pair — reapply 用 dataItem.Pointer 查 Counts 更稳;同时恢复 weight label N 倍显示(不仅是 stack label)
- `v2.7.0` (2026-04-28) **并入 TldHacks**。cell 复用 bug 修法:`SeenItems` 改存 `(item, di, giPtr)`,每帧 verify `item.m_GearItem.Pointer == giPtr` —— 不匹配说明 cell 已被滚动/切分类重绑到别的 gear,skip 本次 reapply 等 `RefreshDataItem.Postfix` 重新登记。旧 FoodStackable.dll 保留作历史,新装直接用 `TldHacks.dll`

## TldHacks — 自制综合修改器

**做什么**:把 Cheat Engine `The Long Dark.CT` 表里大部分功能,用 MelonMod + Harmony patch 方式重写成内置修改器。按 Tab(可改)呼出三标签页菜单(主要 / uConsole / 物品&传送),~60 个 toggle 覆盖无敌、无限体力、各种免疫、武器增强、瞄准稳定、制作/生火快捷、15 个地图区域传送、358 条物品刷出、技能满级、解锁壮举/蓝图/地图。

**关键点**:
- **Il2CppInterop 下 GUILayout 全被 strip**,只能用 `GUI.Xxx(Rect, ...)` —— 菜单代码手写所有坐标,不是 GUILayout 风格
- **瞄准/武器用游戏内建 `m_DisableAim*` bool 字段**(vp_FPSWeapon / vp_FPSCamera.m_DisableAmbientSway)—— 比 patch getter 可靠,toggle 可双向同步
- **uConsole 大部分命令在 release build 里 no-op**,只 spawn 系稳;关键 cheat 自己 Harmony patch
- **堆叠按 cell-bind 时机挂 `RefreshDataItem.Postfix`**,不是点击症状层;`OnLateUpdate` 用 giPtr verify 防 cell-reuse 写 stale 数据

**完整设计 / 实现细节 / 踩坑记录** 见 `TldHacks/src/HANDOFF.md`(400+ 行)。

**版本**:v2.7.0 —— 合并 FoodStackable + 全 15 region 传送 + 所有原 TODO 后端实装。

## ModSettingsQuickNav — 自制 ModSettings 扩展

**解决什么问题**:装了 50+ mod 后,`ModSettings.dll` 的 mod 设置面板只能点箭头一个一个翻,找特定 mod 要戳几十下。

**做法**:Harmony patch `ModSettings.ModSettingsGUI.OnEnable/OnDisable`(都是 internal 类,靠 `AccessTools.TypeByName` + 反射 bypass)。当 ModSettings tab 激活时,屏幕右上角出现一个 IMGUI 切换按钮 + 按 `` ` ``(backquote)快捷键打开浮层:
- A-Z 字母跳段(13×2 栅格按钮)
- 清除筛选
- 滚动列表显示所有 mod,点一下直接跳
- 内部通过反射调 ModSettingsGUI 的 private `SelectMod(string)` 完成切换

主菜单 / 游戏内暂停菜单都能用。不碰 TldHacks,独立工作。

## 快速换机恢复

```powershell
# 1. 装好你的整合包到 <TLD>/Mods/
# 2. clone 这个仓库
git clone https://github.com/Jcxu97/tld-自用改造2.55.git

# 3. 覆盖配置
cp -r tld-自用改造2.55/configs/Mods/*       <TLD>/Mods/
cp -r tld-自用改造2.55/configs/UserData/*   <TLD>/UserData/
cp    tld-自用改造2.55/BunkerDefaults/BunkerDefaults.dll <TLD>/Mods/
cp    tld-自用改造2.55/TldHacks/TldHacks.dll             <TLD>/Mods/
cp    tld-自用改造2.55/ModSettingsQuickNav/ModSettingsQuickNav.dll <TLD>/Mods/
# 装了 TldHacks v2.7+ 就别装 FoodStackable,功能已内置
```

## 已知问题

- **整合包切场景偶发闪退 + 重启卡启动**:117-mod 整合包通病。切场景前按 `F5` 快存,闪退后反复重启/重启电脑即可。详见 `CHANGELOG.md`。
- **DarkerNights.dll (v1.3 by Xpazeman)**:对 TLD 2.55 已失效,加载会让启动死循环。**必须不装它**。该作者其他 mod(AmbientLights / PlacingAnywhere / HouseLights / GearDecayModifier)还能用。
- **StackManager 的 `AddStackableComponent` 只影响新生成的物品**:已经在你背包里的 soda/jerky 还是不能叠,要去新容器捡。

## License

- `BunkerDefaults/` / `FoodStackable/` / `TldHacks/` 下面我写的一切:MIT(见 `LICENSE`)
- `configs/` 下面的是我的设置快照 —— 各 mod 配置格式归原作者,这些只是"我填了什么值"。你随便拿去改。


---

# 推荐配套 MOD 合集


> 配套 TldHacks 使用的精选基础 mod 包。**不含**剧情区域/衣物食物内容包/风格皮肤,只保留**框架 + 核心玩法补强**。
> 内容包 (modcomponent) 类按需自加,不在本合集范围。

---

## 1. 必装框架 (8 项)

无这些 TldHacks 不工作或外部 mod 加载失败。

| dll | 作用 |
|---|---|
| MelonLoader | mod 框架 (外部安装,装游戏前先装) |
| ModSettings.dll | mod 配置面板入口,TldHacks 详细参数全靠它 |
| ModData.dll | TldHacks 序列化用 |
| ModComponent.dll | 加载 .modcomponent 扩展物品包 |
| ComplexLogger.dll | 多个 mod 共享日志库 |
| AudioManager.dll | 音频替换 API,多 mod 依赖 |
| AfflictionComponent.dll | 疾病/状态扩展 API |
| ExamineActionsAPI.dll | 物品 Examine 交互扩展 API |
| DeveloperConsole.dll | TldHacks ConsoleBridge 必需 (商人刷新等命令依赖) |

## 2. 核心本体

| dll | 作用 |
|---|---|
| **TldHacks.dll** | 本合集主体,整合 30+ 独立 mod 功能 |
| disable_integrated_mods.bat | 禁用脚本,把已被 TldHacks 整合的独立 mod 改名 .disabled |

## 3. 安全屋 + 搬运 (用户钦点)

| dll | 作用 |
|---|---|
| **SafehouseCustomizationPlus.dll** | 自定义安全屋,可改室内布局/物品摆放 |
| **Architect.dll + Architect.modcomponent** | **按 Y 搬炉子/家具**,自由建造 |

## 4. 地图 / 导航 (3 项)

| dll | 作用 |
|---|---|
| MapManager.dll | 玩家箭头/居中/调查范围/无宝丽来/全开/地堡热键 (TldHacks 不再整合) |
| MotionTrackerLite.dll | 运动追踪雷达,提示附近野兽 |
| ModSettingsQuickNav.dll | ModSettings 配置面板快速导航 |

## 5. 物品管理 (4 项)

| dll | 作用 |
|---|---|
| GearInfo.dll | 物品详情面板 (重量/HP/分类) |
| GearToolbox.dll | 装备工具箱,批量整理 |
| ItemPicker.dll | 拾取增强 + 自动堆叠提示 |
| InventoryReassignments.dll | 背包/快捷栏重排 |

## 6. 通用补强 (3 项)

| dll | 作用 |
|---|---|
| SaveManager.dll | 多存档/手动存档 |
| PlacingAnywhere.dll | 物品任意位置放置 (室外/路面) |
| BlueprintCleaner.dll | 配方清单清理 (TldHacks 已 unpatch 它的破损 Postfix) |

## 7. 兼容性修复 (2 项,可选)

| dll | 作用 |
|---|---|
| BricklayersDoorFix.dll | 修复部分 mod 引入的门交互 bug |
| RavineBridgeFix.dll | 修复某些桥梁碰撞 bug |

---

## 总计

- **必装**: 8 (框架) + 1 (TldHacks) = **9 个 dll**
- **强烈推荐**: 1 (安全屋) + 1 (Architect) + 3 (地图) + 4 (物品) + 3 (补强) = **12 个 dll** + 1 modcomponent
- **可选**: 2 (兼容性修复)
- **总共**: 23 个 dll + 1 modcomponent + 1 bat

体积小,启动快,稳定。

---

## 玩家自加 (本合集不内置)

按个人喜好可加,**与本合集兼容**:
- 难度调整: MiseryModePlus / Minor_Miseries / SeasonedInterloping / InterloperHudPro / StalkerAidsAndSupplements
- 内容扩展 (.modcomponent): FoodPackByTKG / ClothingExpanded / FirePack / CampingTools / IndoorsGreenery / ZC8787 系列等
- 风格皮肤: RetroFood / RetroTextures
- 区域 mod: LitharsRidge
- 杂项: AlcoholMod / WildFire / RestandReadMod / PineNeedleTea / EdiblePlants / Bountiful_Foraging / FortifiedLookouts / PrepperCache

---

## 不推荐(已被 TldHacks 整合,装了会冲突)

`disable_integrated_mods.bat` 会自动禁用以下 33 项:
SonicMode / Jump / GunZoom / VehicleFov / FastTravel / StretchArmstrong / TorchTweaker / KeroseneLampTweaks /
AutoToggleLights / GearDecayModifier / BowRepair / DisableAutoEquipCharcoal / RememberBreakDownItem /
DroppableUndroppables / CraftAnywhereRedux / MoreCookingSlots / TimeScaleHotkey / FullSwing / SilentWalker /
QoL / AutoSurvey / TinyTweaks-(MapTextOuline/NoSaveOnSprain/WakeUpCall/BuryHumanCorpses) /
SleepWithoutABed / SkipIntroRedux / PlaceFromInventory / CaffeinatedSodas / Sprainkle / RnStripped /
ExtraGraphicsSettings / UniversalTweaks / StackManager
+ 已知冲突 (GfxBoost/LightCull/DarkerNights)

---

## 安装步骤

1. 装 MelonLoader (TLD 2.55+)
2. 把本合集所有文件复制到 `<TLD>/Mods/`
3. 双击 `disable_integrated_mods.bat` (一次性,把整合冗余 mod 改名 .disabled;若已不在你的 Mods 目录会显示"未找到")
4. 启动游戏,Tab 键打开 TldHacks 菜单


---

# 整合包改动记录


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
| `TldHacks.dll` | **本仓库原创**(v2.7.74)—— 综合修改器。替代 FoodStackable(含其全部功能)+ 移植 Cheat Engine 表 ~90 个 cheat:无敌/免疫/无限体力/瞄准稳定/无后坐力/快速制作/生火 100%/爬绳 ×5/15 region 传送/**911 条物品刷出(358 vanilla + 553 自动扫出的 mod 物品,带 `[ModName]` 标签)**/技能满级/解锁壮举·蓝图·地图/冻结寒冷值(开启抓当前,关闭自然变化)等 |
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


---

# 视频介绍稿


---

## 开场

大家好，今天给大家介绍一下我们的 TldHacks 模组整合包。

我们把 29 个独立模组整合进了一个 DLL 里面。
所有功能都可以在游戏内的 IMGUI 菜单里面实时开关和调参。
不需要退出游戏，不需要改配置文件。

---

## 速度与动作类

**SonicMode** — 超级跑步模式。移动速度、攀爬速度、蹲伏速度全部可调。

**FullSwing** — 近战全力挥击。伤害倍率和击退力度都可以拉满。

**VehicleFov** — 调整车辆视角 FOV，开车/乘坐都能设。

---

## 画面与性能类

**GfxBoost** — 一键优化画面性能。阴影距离、LOD、树木渲染距离，11 个参数全部可调。

**ExtraGraphicsSettings** — 额外画面设置。分辨率缩放、各向异性过滤、纹理质量，12 个参数。

---

## 武器与瞄准

**GunZoom** — 枪械瞄准倍率调节。

**BowRepair** — 允许修弓。

---

## 睡眠与时间

**WakeUpCall** — 日出唤醒提示、极光感知、睡眠显示时间、禁止全黑屏。四个独立开关。

**SleepWithoutABed** — 随地睡觉。不需要床也能睡。13 个参数可调：疲劳恢复率、冻伤系数、低血量中断、冷却时间等等。

**TimeScaleHotkey** — 时间加速热键。按一下加速，再按一下恢复。

---

## 灯光与火把

**TorchTweaker** — 火把参数调节。亮度、燃烧时间、投掷距离全能改。

**KeroseneLampTweaks** — 煤油灯优化。油耗、亮度、照射范围，6 个参数。

**AutoToggleLights** — 进出室内自动开关灯。

---

## 物品与制作

**CraftAnywhereRedux** — 任意地点制作。不用找工作台。

**MoreCookingSlots** — 更多烹饪槽位。一次最多烤多少东西你来定。

**CaffeinatedSodas** — 汽水含咖啡因，喝了提升疲劳恢复。9 个参数控制效果强度。

**DroppableUndroppables** — 让不能丢弃的物品可以丢弃。

**DisableAutoEquipCharcoal** — 测绘完不自动装备炭笔。

**RememberBreakDownItem** — 记住上次拆解用的工具。

---

## 地图与测绘

**AutoSurvey** — 自动测绘。走到哪画到哪，范围和延迟可调。全图解锁也行。

**MapTextOutline** — 地图文字描边。0-3 级粗细可选。

---

## 隐身与声音

**SilentWalker** — 脚步静音。走路、跑步、金属/木头/水/通用 5 个音量通道独立控制。

---

## 物品衰减

**GearDecayModifier** — 装备衰减倍率修改器。37 个分类独立可调：食物、饮料、衣物、工具、武器，全覆盖。猎刀/手斧/钢锯屠宰衰减也能单独设。

---

## 扭伤与受伤

**Sprainkle** — 扭伤系统全面修改。16 个参数：扭伤概率、负重阈值、恢复时间、绷带效果等。

**NoSaveOnSprain** — 扭伤不触发自动存档。坠落扭伤也可以单独控制。

---

## 跳过与杂项

**SkipIntroRedux** — 跳过开头动画和警告界面。

**RnStripped** — 两个功能：尸体拖拽移动 + 手电筒永不耗电。

**BuryHumanCorpses** — 埋葬人类尸体。按住右键出进度条，埋完消失。数据持久化。

---

## 总结

29 个模组，一个 DLL。
菜单里分四个标签页：作弊功能、角色属性、生活质量、快捷导航。
所有参数实时可调，改完即生效。

装好之后记得跑一下我们的一键禁用脚本，把原来的独立 DLL 禁掉，避免重复加载冲突。

感谢观看，用得开心。