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
