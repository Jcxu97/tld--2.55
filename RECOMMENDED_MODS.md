# TldHacks 基础修改 MOD 合集 v3.0.4

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
