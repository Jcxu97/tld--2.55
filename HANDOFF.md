# TLD 2.55 整合包 — 交接文档

**最后更新**:2026-05-01
**仓库**:https://github.com/Jcxu97/tld--2.55
**本地工作副本**:`D:\TLD-Mods\tld--2.55\`
**游戏安装**:`D:\Steam\steamapps\common\TheLongDark`

---

## 0. 本次会话(2026-04-30 晚 → 2026-05-01 凌晨)最终状态

TldHacks 升 **v2.7.74**。主要变更:

### 新功能
- **Spawner 加 mod 物品**:自动扫 `Mods/*.modcomponent`(40 个)读 `auto-mapped/*.json` 和 `localizations/Localization.json`,按 `ModGenericComponent / ModFoodComponent / ModClothingComponent / ModToolComponent / ModFirstAidComponent / ModCollectibleComponent / ModBedComponent / ModLiquidComponent` 优先级取 InvCat + WeightKG,走 Simplified Chinese 本地化拿中文名。新文件 `TldHacks/src/ItemDatabaseMod.cs`(553 条,358 vanilla 基础上,去重 2 条),每条 Name 加 `[ModName简写]` 标签。生成脚本 `C:\Users\82077\AppData\Local\Temp\tldmod\gen_moddb.py`。**Spawner UI 顶部显示 911 条**。
- **冻结寒冷值 toggle**(玩家列温饱 section):开启那一 tick 抓 `Freezing.m_CurrentFreezing` 到 `CheatState._frozenColdSnapshot`,之后每 tick 写回;toggle off 那一 tick 清 snapshot 为 `NaN`,游戏自然接管。代码 `CheatState.FreezeColdValue` + `CheatsTick.TickStatus` 里的分支。

### Bug fix
- **秒打碎变暗(用户反馈 CE 也有)**:Patch 合并 Fade5 + Fade(同签名冗余)为一个 5-param Prefix 归 0 start/target/time/delay;BreakDownFinished Postfix 增强为走 `BreakDownCleanup.Run`(PassTime.End + `m_TimeAccelerated=false` + Panel `m_TimeIsAccelerated=false` + CameraFade.FinishFade)。因 `BreakDownFinished` 被 IL2Cpp **内联**(log 0 次命中 Postfix),加 **Update 边沿检测**(`IsBreakingDown` true→false)+ **Enable(false) 兜底**(关 panel 时)走同一 cleanup。**FadeSuppressionWindow 从 3s → 5s**。log 证据:Fade5 `target=0.5` 就是那一层半透明暗,已成功拦截。用户反馈"概率性还暗",未最终定论。
- **Stealth 关掉后动物还在逃**:TickAnimals 在 `CheatState.Stealth=false && _lastTickedStealth=true` 边沿的那一 tick 加 else 分支,遍历 AI 把 `AiMode.Flee` 的 `ClearTarget()` + `SetAiMode(AiMode.Wander)`,让 AI 下一帧按 fear/感知重新判断(狼/熊会 Attack,兔/鹿继续 Flee)。
- **锁温度 uConsole UI 删除**:`unlock_temperature` 游戏内损坏,锁完不可逆。替代用新的"冻结寒冷值"toggle 走 `Freezing.m_CurrentFreezing` 路径,toggle off 干净。

### 其他
- 版本号 Menu.cs:`v2.7.72` → `v2.7.74`,ModMain.cs 日志字串:`v2.7.64` → `v2.7.74`(加 mod items 计数)
- **DLL**:204800 bytes(原 136192,+50% 来自 553 mod 物品)
- 本次会话**没再动** BunkerDefaults / FoodStackable / codex Cursor Ink UI 主题/尺寸

### 桌面打包(不进仓库)
- `C:\Users\82077\Desktop\TldHacks-一键修改器整合包.zip` (866 MB) —— 纯净游戏版:MelonLoader 0.7.3 + version.dll + ModSettings + DeveloperConsole + TLDev(964MB Unity 资源 bundle) + TldHacks + 使用说明.txt
- `C:\Users\82077\Desktop\TldHacks-单独安装包.zip` (101 KB) —— 老玩家版:只 TldHacks.dll + ModSettings.dll + 使用说明.txt

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

### TldHacks `v2.7.74`(2026-05-01 已 commit)— 详见上方第 0 节

以下是 v2.7.72/73 时段的历史说明(codex Cursor Ink UI 合并 + 5 处修复,2026-04-30):

**codex 在本仓库之外的 `D:\TLD-Mods\TldHacks\` 开发了 v2.7.66~v2.7.72 "Cursor Ink UI"(深色主题 + 自绘 GUIStyle),然后合并回本仓库。** 本会话在此基础上又做了 5 处修复,工作树相对 HEAD 有改动未 commit。

**1. 列重分组(我早先做过的,codex 合并时保留了)**
- 锁&容器:列 2 → 列 1
- 制作:列 3 → 列 1
- 瞄准:列 3 → 列 1
- 商人 uConsole:列 2 → 列 3

新分布:
- 列 1 · 玩家:生存 / 温饱 / 移动速度 / 技能 / 制作 / 锁&容器 / 瞄准
- 列 2 · 世界:动物 / 环境&篝火 / 世界时钟 / 一次性操作 / 商人&美洲狮
- 列 3 · 物品:快速操作 / 物品&装备 / 武器射击 / 一键获取武器 / 商人 uConsole

**2. x1.7 缩放 label 垂直居中(Menu.cs:294)**
codex 的 `_mutedLabelStyle` 没设 `alignment`,默认 `UpperLeft` → x1.7 label 比 `-/+` 按钮里的字符偏上偏左。**不加 `TextAnchor.MiddleCenter`**(需引 `UnityEngine.TextRenderingModule`,动 csproj),改用坐标 `R(W-158, 14, 50, 18)`(原 `R(W-158, 8, 50, 24)`),让 label 的 `UpperLeft` 文字视觉对齐按钮 `MiddleCenter` 文字。

**3. Spawner 翻页按钮消失(Menu.cs:710)**
codex 把 `ContentH_Spawner = H-108`(line 19),viewport = `H-128`(line 308),但 Spawner 内部仍写死 `availH = (H - 80f) - y - (ROW_ADV + 8f)`。680 vs 652 差 28px → 物品多塞 1 行 → 分页按钮被裁出 scroll content 外看不见。改 `H - 80f → ContentH_Spawner`。

**4. UI 底色切场景/开 uConsole 后变透明(Menu.cs:101-107 + InitStyles 开头)**
codex 的 `Tex()` 创建 `Texture2D` 没标 `hideFlags` → Unity 场景切换时把它当游离资源 GC 掉 → `GUIStyle.background` 指向 dead texture → 渲染透明。
- `Tex()` 加 `tex.hideFlags = HideFlags.HideAndDontSave`
- `InitStyles()` 开头加兜底:`if (_stylesReady && _bgTex == null) _stylesReady = false;`(Unity 的 `== null` 对 destroyed Object 返回 true,触发重建)

**5. 秒打碎完成后持续暗(CheatsPatches.cs v2.7.65 修复不够)**
v2.7.65 的 `BreakDownFinished` Postfix 只 `FadeIn(0,0,null)` + `SetTODLocked(false)` + `FadeSuppressionWindow.Arm(3s)`,用户反馈"又暗了"。本次加两条防线:
- **Postfix 强化**:`m_TargetAlpha=0` / `m_StartAlpha=0` / `m_FadeTimer=0` / `m_FadeDuration=0` + `FinishFade(true)` → 跳过任何进行中的 fade 状态机。
- **补 `Patch_CameraFade_Fade5`** (CheatsPatches.cs 的 `Patch_CameraFade_Fade5`):之前 codex 只 patch 了 `FadeIn/FadeOut/FadeTo` 3 个 3-4 参数重载,`Fade(startAlpha, targetAlpha, time, delay, action)` 5 参数版本是漏网之鱼。打碎完成后的暗很可能走这条。

**部署状态**:
- 2026-05-01 已随 v2.7.74 commit(见上方第 0 节)
- `Mods/TldHacks.dll` 最终大小 204800 bytes(+50% 因 553 mod 物品)

**⚠ 关键规则 — codex UI 别动**:
- 窗口 `W=1280 H=760` / 三列 `COL_W=405` / 按钮常量 `TOG_W=182 / TOG2_OFF=198 / TOG_WIDE=380` / 节拍 `SEC_H=30 / ROW_ADV=30 / SECTION_END_ADV=14`
- Cursor Ink 配色 7 个 Color 常量
- Tex() / InitStyles / 各 GUIStyle 配置
- **全部别动,用户明确说过两次**。只允许:section 重分组、功能 bugfix、texture 生命周期修、单 toggle 增删。
- memory 有 `feedback_tldhacks_codex_ui.md` 详细清单。

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
| TldHacks | v2.7.74 | `Mods/TldHacks.dll` | 204800 | 2026-05-01 00:41 |
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
5. **v2.7.74 未最终验证**:
   - 秒打碎变暗 —— 用户反馈"概率性还暗"。log 证据 Fade5 `target=0.5` 已拦截,但 BreakDownFinished Postfix 命中 0 次(内联);现改 Update 边沿 + Enable(false) 兜底 + 5s FadeSuppressionWindow,用户未再复测。若仍概率暗,log 看 `[QuickBD.Cleanup:<tag>]` 哪个 tag fire,再加诊断
   - Stealth 关掉动物还在逃 —— 已加 on→off 边沿清(Flee→Wander + ClearTarget),用户未复测
   - 冻结寒冷值 toggle —— 新功能未复测,开启抓 snapshot / 关闭清 snapshot 逻辑
   - uConsole 锁温度 UI 已删(unlock_temperature 游戏内损坏,锁完不可逆)
   - Spawner 911 条 + `[ModName]` 标签 —— 用户上次测完 Spawner 正常出,未专门翻 mod 分类验证
6. **TldHacks UI 设计保持 codex Cursor Ink 主题** —— 用户明确说过两次,只允许 section 重分组 / 功能 bugfix / texture 生命周期修 / 单 toggle 增删。不要碰宽度/间距常量 / GUIStyle / Cursor* 配色。详见 memory `feedback_tldhacks_codex_ui.md`。
7. **打包分发**(不入仓库):桌面 `TldHacks-一键修改器整合包.zip`(866 MB,含 MelonLoader + 控制台 + TldHacks)和 `TldHacks-单独安装包.zip`(101 KB,只 TldHacks.dll + ModSettings.dll)。重新打包改路径在 `使用说明.txt`。

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
