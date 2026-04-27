# TLD 2.55 整合包 — 交接文档

**最后更新**:2026-04-27
**仓库**:https://github.com/Jcxu97/tld--2.55
**本地工作副本**:`D:\TLD-Mods\tld--2.55\`
**游戏安装**:`D:\Steam\steamapps\common\TheLongDark`

---

## 1. 两个自制 mod 的当前状态

### FoodStackable `v0.4.9`

**做什么**:背包里同类物品视觉上合并成一格 `×N`,不动底层 GearItem(吃一份剩 N-1,存档干净)。

**关键文件**:
- 源码 `FoodStackable/src/ModMain.cs`
- 产物 `FoodStackable/FoodStackable.dll` → `Mods/FoodStackable.dll`
- 当前部署:9728 字节 @ 20:55

**核心架构**:
- `Panel_Inventory.RefreshTable` 的 Prefix → `Dedupe.Process` 把 `m_FilteredInventoryList` 去重,代表项记 `Counts[di.Pointer]` + `CountsByGi[gi.Pointer]`
- `Panel_Container.RefreshTables` 的 Prefix → 同时处理背包栏 + 容器栏
- `InventoryGridItem.RefreshDataItem` 的 Postfix → 首次写 stack label + N 倍 weight,并登记 `SeenItems[item.Pointer] = (item, di)`
- `MelonMod.OnLateUpdate` → 每帧遍历 SeenItems 重写 label(Unity Update 之后,保证覆盖游戏的清 label)

**排除规则**(不参与我的 UI 堆叠):
- `LiquidItem`(纯液体)
- `StackableItem.m_Units > 1`(已被 StackManager/原版合并的)
- `ExcludePrefabs` 硬编码黑名单:`MashedPotatoes` / `WaterPurificationTablets` / `CigarettePackMarlboro` / `CigarettePackOld`
- `ExcludePrefixes` 前缀黑名单:`GEAR_cc`(卡牌收集 mod)

**分组 key**:`gi.name + "|" + m_Opened`(gi.name 是 GameObject 名,每 prefab 独立,避免 GearItemData asset 共享导致的误合并)

**Bonus 性质 — 不要破坏**:代表项 = 最低 condition 那份(游戏 filter 升序 + first-wins),点吃先消化快坏的。重构要保留。

**已知未完全解决**:
- ⚠ 用户报 **点击物品后 ×N 和 weight 消失**,v0.4.7 hook InventoryGridItem.Update 无效(Il2CppInterop 下 Harmony 钩不到 Unity 引擎隐式调用的 MonoBehaviour 生命周期),v0.4.8 改 MelonMod.OnUpdate 仍未彻底解决,v0.4.9 再换 OnLateUpdate + SeenItems 存 (item, di) pair。**v0.4.9 还没机会测点击 bug**

### BunkerDefaults `v1.0.3`

**做什么**:游戏启动时扫 `Mods/ModData/*.moddata`,向新存档 FastTravel 的 Destinations 里预填 5 个地堡坐标。

**关键文件**:
- 源码 `BunkerDefaults/src/BunkerDefaults.cs`
- 产物 `BunkerDefaults/BunkerDefaults.dll` → `Mods/BunkerDefaults.dll`

**v1.0.3 FastTravel 9-slot 映射**(用户偏好):

| 按键 | Dest# | 位置 | Scene | 默认坐标 |
|---|---|---|---|---|
| `5` | 0 | 废弃机场应急舱 | AirfieldRegion | **空,走到按 `=+5` 存** |
| `6` | 1 | 神秘湖地堡 | LakeRegion | ✅ `1029.06, 91.99, -52.52` |
| `7` | 2 | 沿海公路加油站 | CoastalRegion | **空,走到按 `=+7` 存** |
| `8` | 3 | 山间小镇地堡 | MountainTownRegion | ✅ `1828.20, 444.39, 1771.27` |
| `9` | 4 | 荒凉水湾地堡 | CanneryRegion | ✅ `328.37, 344.50, 833.16` |
| `F2` | 5 | 荒芜据点 | WhalingStationRegion | **空,走到按 `=+F2` 存** |
| `F3` | 6 | 黑岩地区地堡 | BlackrockRegion | ✅ `705.04, 373.98, 816.38` |
| `F4` | 7 | 灰烬峡谷地堡 | AshCanyonRegion | ✅ `-42.12, 172.95, -796.68` |
| `F7` | 8 | 林狼雪岭地堡 | MountainPassRegion | **空,走到按 `=+F7` 存**(社区 Y=207 坏) |

**坏坐标教训**(CheatEngine 社区表有 bug):
- `MarshRegion` `Y=-83.38`:海拔在水下/地下,传过去一直下落
- `MountainPassRegion` `Y=207.32`:X/Z 在山体外空中,传过去掉崖

从默认注入里移除,也从用户 layout 里移除 Marsh(改为 Airfield/Coastal/WhalingStation 占 slot 0/2/5,MountainPass 挪 slot 8 留空)。

**FastTravel 相关配置**:`Mods/FastTravel.json`
- `SaveModifierKey = Equals` —— 按 `= + 数字键` 保存当前位置
- `DeleteModifierKey = Minus`
- `CanEditDestinations = true` —— 可编辑
- `OnlyBetweenDestinations = false` —— 任意位置都能传

---

## 2. ItemPicker 自动拾取配置

**Mod**:`Mods/ItemPicker.dll`(Digitalzombie 作,非自制)

**拾取表**:`Mods/ItemPickerCustomList.txt` = **958 行**

按 `W` 键 → 25m 范围 SphereCast → 所有 GearItem.name 精确匹配 `customItems` 的都捡。

**扩 list 流程**:
1. 拿 prefab 名来源:`tld_Data/StreamingAssets/aa/catalog.json`(848 个原版 `GEAR_*`)+ `Mods/*.modcomponent` 解压(mod 加的物品)
2. 过滤:排除 `_Mat`/`_Dif`(材质)、剧情钥匙 `*Key*`、剧情笔记 `*Note*`、明信片 `PostCard*`、尸体 `*Carcass`、水壶系统 `WaterSupply/WaterBottle`、占位 `NULL/Stalker/ElevatorCrank`、背包 prefab `BackPack_A*`
3. `echo "GEAR_X" >> ItemPickerCustomList.txt`
4. **必须重启游戏** list 才生效(ItemPicker 只在 `OnInitializeMelon` 读一次)
5. 同步到仓库 `configs/Mods/ItemPickerCustomList.txt`

**ItemPicker 行为提醒**:`StreamReader.ReadLine()` 读 list,**空行/注释会被当成 item**(空字符串 `.Contains("")` 匹配所有物品 → 全捡)。追加时严禁加注释行、空行。

---

## 3. StackManager 的兼容策略

**Mod**:`Mods/StackManager.dll`(第三方)

**处理的 18 个物品**(在 `Mods/StackManager/config.json` 的 `STACK_MERGE`):
```
BirchSaplingDried, BearHideDried, BottleAntibiotics, BottlePainKillers,
Carrot, CoffeeTin, GreenTeaPackage, GutDried, LeatherDried,
LeatherHideDried, MapleSaplingDried, MooseHideDried, PackMatches,
Potato, RabbitPeltDried, StumpRemover, WolfPeltDried, WoodMatches
```

这些 prefab 会被 **真实合并**(改 `StackableItem.m_Units`,销毁 GearItem)。FoodStackable 在 `m_Units > 1` 时跳过,完全让路给 StackManager。

`AddStackableComponent`(给物品加 StackableItem 组件):`Potato`, `StumpRemover`

---

## 4. 版本/部署一览

| 组件 | 版本 | 文件 | 大小 | 部署时间 |
|---|---|---|---|---|
| FoodStackable | v0.4.9 | `Mods/FoodStackable.dll` | 10240 | 20:55 |
| BunkerDefaults | v1.0.3 | `Mods/BunkerDefaults.dll` | 10240+ | 21:0X |
| ItemPickerCustomList | - | `Mods/ItemPickerCustomList.txt` | 958 行 | - |

**编译流程**:`cd D:/TLD-Mods/<Mod>/ && dotnet build -c Release`,AfterBuild 自动 cp 到 `Mods/`。**游戏运行时 cp 被锁**,要先退游戏。

---

## 5. 待办 / 下次接着的点

1. **FoodStackable 点击丢 ×N 是否彻底修好** —— 用户上次测的是 v0.4.8,v0.4.9 加了 OnLateUpdate + SeenItems(item, di) pair + Reapply 同时写 stack label + weight。还没验证。
2. **新 FastTravel 布局里 4 个空 slot 需要用户走到存**:
   - `5` AirfieldRegion
   - `7` CoastalRegion
   - `F2` WhalingStationRegion
   - `F7` MountainPassRegion
3. **Marsh 之前手动保存的坐标丢了**(随 slot 重排没保留,`.bak_before_rearrange` 有备份)。用户如果还要 Marsh 需要重新分一个 slot。
4. **storymodeCollectibles / Lich.s.Decorations / Collector.Cards 等 mod 未扫入 CustomList**,用户说食物/工具/饮料/衣服就好。后续若要收集品类自动捡再处理。

---

## 6. 有用的路径 / 命令速查

**sandbox 存档**:`$USERPROFILE/AppData/Local/Hinterland/TheLongDark/Survival/sandbox3`

**存档 moddata**(ZIP):`Mods/ModData/sandbox3.moddata`
- 手动编辑(重排 FastTravel 等):用 Python `zipfile + json`
- memory `reference_tld_moddata.md` 有细节

**il2cpp 反编译**:
- `~/.dotnet/tools/ilspycmd "<dll>" -t "Il2Cpp.<ClassName>"` 单类
- TLD 类都在 `Il2Cpp.*` 命名空间下
- DLL 目录 `MelonLoader/Il2CppAssemblies/`

**游戏日志**:`MelonLoader/Latest.log`
- FoodStackable 错误前缀:`[Inventory.RefreshTable.Prefix]` / `[Container.RefreshTables.Prefix]` / `[RefreshDataItem.Postfix]` / `[OnLateUpdate]` / `[LabelFix.Reapply]`
- BunkerDefaults 前缀:`[BunkerDefaults] ...`

**Git**(工作副本 `D:/TLD-Mods/tld--2.55/`):
```bash
git pull
# 改动:复制 dll + 源码 + configs
# 更新 CHANGELOG / README / HANDOFF
git add -u && git commit -m "..." && git push
```
