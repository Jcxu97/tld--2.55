# TldHacks v5.5 — 交接文档

> 2026-05-10 当前活跃版本。v4.0r7 / v4.0r6 / v4.0r5 / v4.0r4 / v4.0r3 / v4.0r2 / v4.0 段保留在下方。

---

## 2026-05-10 修复批次

| 修复 | 文件 | 说明 |
|------|------|------|
| Panel_Container.Enable AmbiguousMatch | DynamicPatch.cs:212 | 加 `ContainerEnableArgs` 消歧，QuickOpen/InfiniteContainer 恢复生效 |
| AimDiag 主菜单日志刷屏 | CheatsPatches.cs:1587 | `GameManager.IsMainMenuActive()` 早退 |
| NLB SemVer 警告 | NLB AssemblyInfo + csproj | "2.0.6.1" → "2.1.0" |
| 保温瓶装任意液体不生效 | DynamicPatch.cs:152-155 | 新增 patch `Panel_InsulatedFlask.IsCompatibleDrink` + `InsulatedFlaskLiquidTypeConstraint.IsAllowed` |
| ItemDB 缺失物品 | ItemDatabaseMod.cs:565+ | 加入 7×保温瓶(A~G) + 4×背包(含高科技背包) |
| 配置同步 | TldHacks.json | 从游戏目录同步当前运行配置到仓库 |

当前已启用模组：**76 个**（Mods/ 共 80 DLL，4 个 NAudio 库除外）

---

## HarmonyCache Plugin v1.1（2026-05-10 新增）

**独立 MelonLoader Plugin，通过缓存 Harmony 反射扫描加速游戏启动。**

### 问题

100+ mod 环境下启动耗时约 2 分钟。根因：Harmony 的 `AccessTools.GetTypesFromAssembly` 无缓存，每个 mod 执行 `PatchAll()` 时都全量扫描所有 assembly。坏 assembly（如 CampingTools 引用了不存在的 ExamineActionsAPI 方法签名）每次扫描都抛 4 个 `MissingMethodException`，日志显示同一 assembly 被扫了 8+ 轮。

### 方案

写一个 MelonLoader **Plugin**（比 Mod 先加载），用 Harmony patch Harmony 自身的 3 个热点方法：

| 方法 | 缓存策略 | 效果 |
|------|----------|------|
| `AccessTools.GetTypesFromAssembly` | `ConcurrentDictionary<Assembly, Type[]>` | 每个 assembly 只扫 1 次，消除重复异常 |
| `AccessTools.TypeByName` | `ConcurrentDictionary<string, Type>`（含负缓存） | 不存在的类型（如 `Il2Cpp.BeachcombingSpawner`）只查 1 次 |
| `AccessTools.AllAssemblies` | 快照 + assembly 计数变化时刷新 | 避免每次 `AppDomain.GetAssemblies()` + LINQ 过滤 |

### 文件

| 路径 | 说明 |
|------|------|
| `HarmonyCache/src/Plugin.cs` | 全部逻辑，~120 行 |
| `HarmonyCache/src/HarmonyCache.csproj` | 编译配置，输出到 `Plugins/` |

### 编译 & 部署

```
dotnet build "D:/Github/tld--2.55/HarmonyCache/src/HarmonyCache.csproj" -c Release
```

输出自动 copy 到 `D:\Steam\steamapps\common\TheLongDark\Plugins\HarmonyCache.dll`。

### 预估效果

启动反射扫描从 O(N×M) 降到 O(M)（N=mod 数, M=assembly 数），预计节省 10~30 秒。

### 上游 PR 方向

原始仓库：`https://github.com/BepInEx/HarmonyX`（`Harmony/Tools/AccessTools.cs`）。如果本地验证效果好，可向 HarmonyX 提 PR 在 `GetTypesFromAssembly` 内部加 static cache。

---

## Mod 清理（2026-05-10）

### 新增禁用的 mod（移入 `Mods/_disabled/`）

| DLL | 原因 |
|-----|------|
| `EdiblePlants.dll` | TldHacks 已整合 RespawnablePlants（IntegratedPatches8.cs） |
| `TinyTweaks-RunWithLantern.dll` | TldHacks 已整合（Cheats.RunWithLantern + DynPatch） |
| `WildFire.dll` + `.json` + `.modcomponent` | 用户设置中已禁用，纯白占启动时间 |

### 清理残留

- 删除 `Mods/StackManager/` 目录（dll 早已 disabled，config 目录残留导致设置面板仍显示）

### disable_integrated_mods.bat 更新

1. `EdiblePlants.dll` 加入禁用列表
2. `TinyTweaks-RunWithLantern.dll` 加入禁用列表
3. `:check` 函数逻辑改为 `move /y` 到 `_disabled\` 子目录（原来是原地 rename `.disabled` 后缀）
4. 检测已禁用改为查 `_disabled\%~1` 是否存在

---

## v5.5 一句话

**正式发布版本。40+ mod 整合完成；版本号统一 5.5；中英双语压缩包重新打包；功能文档完善；AutoSurvey 图标发现问题已知但暂搁置**。

## v5.5 改动详情

### 1. 版本号统一 → 5.5.0

| 文件 | 改动 |
|------|------|
| `TldHacks.csproj:11` | `<Version>5.5.0</Version>` |
| `ModMain.cs:6` | `MelonInfo("5.5.0")` |
| `ModMain.cs:52` | 启动 log `v5.5 loaded` |
| `Menu.cs:420-421` | UI 标题 `TldHacks v5.5`(中英双语) |

### 2. 发行包重新打包

| 包 | 位置 | 说明 |
|---|---|---|
| 完整版 (~85MB) | `C:\Users\82077\Desktop\TLD\TldHacks-v5.5-完整版_解压到游戏根目录即可！按键在设置-Modseting里改！.zip` | 含所有依赖 mod + TldHacks.dll |
| 最小安装包 (~64MB) | `C:\Users\82077\Desktop\TLD\TldHacks-v5.5-最小安装包_只带修改mod！解压到游戏根目录.zip` | 仅 TldHacks.dll + 必须依赖 |
| 英文版 (~64MB) | `C:\Users\82077\Desktop\TLD\TLD HACK 5.5.zip` | 英文玩家用，README 全英文 |

旧版 v5.0 / v4.04 ZIP 已删除。

### 3. 文档完善

- `TldHacks v5.5 功能介绍.txt` — 18 节完整功能文档（中文），覆盖生存/武器/移动/动物/制作/火源/物品/世界/商人/QoL/扭伤/TinyTweaks/雪橇/UniversalTweaks/时间加速/物品生成器/性能/界面
- `使用说明！Mustread！.txt` — 中英双语安装说明 + disable_integrated_mods.bat 脚本用途说明
- 明确标注：**不含** ESP/透视、自瞄、换弹倍率功能

### 4. AutoSurvey 图标扫描问题（已知，暂搁置）

- **症状**: 用户反映部分图标扫不到
- **诊断结论**: 代码运行正确，在 20m 范围内（`QoL_AutoSurveyRange * 20f`）只能发现 0-2 个 MapDetail（Coastal Highway 共 547 个）
- **尝试过的修复**:
  1. 添加 `ShowOnMap(true)` + 调整调用顺序 → 无效
  2. `DoNearbyDetailsCheck` 参数 `useOverridePosition=true` + `RefreshIconVisibility()` → 无效
  3. 手动 `Unlock(true)` + `Surveyed()` + `ShowOnMap(true)` + `m_RequiresInteraction=false` → 对发现的 detail 有效，但范围内数量太少
  4. 建议增大范围到 150f（对标原版 AutoSurvey mod 的 `drawingRange * 150f`）→ 用户拒绝："不是范围的问题"
- **用户决定**: "算了，你先就这样吧" — 暂不改动
- **潜在方向**: 可能某些图标类型（如 VistaLocation）需要特殊条件才能解锁，或用户期望的"扫到"范围远大于 20m 但不想改 slider 默认值。下次回来可以测试将默认范围提高或添加"全图扫描"一次性按钮

### 5. 整合状态

40+ 独立 mod 全部整合完成，列表见 `TldHacks v5.5 功能介绍.txt` 末尾。`disable_integrated_mods.bat` 可一键禁用冲突的独立版本。

### 待验证

1. **AutoSurvey** — 如有群友反馈范围不够，可提高 `QoL_AutoSurveyRange` 默认值或改乘数 `20f → 150f`
2. **整合包兼容** — 新 ZIP 发给群友实测是否有遗漏依赖

---

## v4.0r7 一句话

**TinyTweaks 7 模块 + ShowTraderTrust + ImprovedTrader 整合；FPS 优化（室外无开销）；UI 大清理（7 死文件删除 + 布局重排）；搬运/搜刮/开容器/商人功能修复**。

## v4.0r7 改动详情

### 1. TinyTweaks 7 模块整合

全部从 GitHub 源码照抄，新文件 `IntegratedPatches8.cs`。

| 模块 | 功能 | 控制方式 |
|------|------|----------|
| CapFeelsLikeTemp | 室外体感温度上下限裁剪 | toggle + 2 slider (℃) |
| DeathTriggerGoat | 无视坠落即死墙 + 坠落伤害倍率 | toggle + slider (1-12) |
| DroppedObjectOrientation | 步枪落地竖立/火柴朝向修正 | toggle |
| ExtendedFOVSlider | FOV 范围扩展 30-150 | toggle |
| PauseOnRadial | 开辐射轮时时间减速 | toggle |
| SpeedyInteractions | 5 维交互速度乘数 | 5 slider (0.2x-6x) |
| RespawnablePlants | 采过的植物定时重生 | toggle + slider (1-365天) |

### 2. ShowTraderTrust 整合

- 来源: Pathoschild/TheLongDarkMods
- 功能: 和商人通话时屏幕底部显示 `信任度: X / MaxTrust`
- 实现: OnUpdate tick（仅室内 + 60帧间隔），IMGUI overlay
- 无 Harmony patch，纯轮询 TraderRadio.m_CurrentState

### 3. ImprovedTrader 整合

- 商人到来天数 slider (1-40天)，照抄原 mod: `hoursPlayed >= days * 24` 时 IsTraderAvailable = true
- DynamicPatch 始终挂载（`() => true`）
- UI: 主页商人 section 新增 slider

### 4. 搬运猎物修复

- condition 阈值: `>= 0.5f` → `> 0f`（割完肉也能搬）
- 按钮文字: 禁用 UILocalize，直接设 UILabel（中文"搬运猎物"/英文"Carry"）
- HUD 提示全双语

### 5. 秒搜刮 / 秒开容器修复

- **秒搜刮**: 旧 `UpdateHoldInteraction(float)` Prefix 在 IL2CPP 中 bind 不上 → 改用 `TimedHoldInteraction.BeginHold` Prefix 直接 `HoldTime = 0.01f`
- **秒开容器**: 从 `delay = 0`（瞬间）改 `delay /= 50f`（50x 速度），参考交互速度 slider 思路

### 6. 商人功能修复

- **信任+100**: 不再走 uConsole 命令 → 直接 `m_CurrentState.m_CurrentTrust += 100`
- **信任满值**: 已在 `GetAvailableTradeExchanges` Prefix 生效（交易时自动写满）

### 7. FPS 性能优化

| 问题 | 修复 |
|------|------|
| ShowTraderTrust 室外每 10 帧 FindObjectOfType 搜全场景 | 加室内检查 + `_searchFailed` 缓存 + 频率降到 60 帧 |
| SpeedyInteractions 20 个 patch `()=>true` 永驻 | 改条件挂载：mult=1 时自动卸载 |
| ShowTraderTrust 默认 true 导致室外也 tick | 默认改 false |

### 8. UI 布局重排

- **#1**: "开关门" 2 slider 合并进"交互 & 感知"（省 1 个 section header）
- **#2**: "时间加速" 2 slider 合并到"其他便利功能"末尾（省 1 个 section header）
- **#3**: "车辆视角" + "室内灯光" 移到 Tab5，Tab5 改名"视觉 & 显示"
- Tab4 Col3 新增"交互速度"section（5 slider）
- 杂项开关下方说明文字扩充（逐条解释每个 toggle）
- Section 标题去掉所有技术 mod 名（统一纯功能描述）

### 9. 死代码清理

删除 7 个从未被调用的 partial class 文件（~1200 行）：
- `MenuTweaks.Character.cs` / `MenuTweaks.CraftFire.cs` / `MenuTweaks.Light.cs`
- `MenuTweaks.QoL.cs` / `MenuTweaks.WorldItems.cs` / `MenuTweaks.Decay.cs` / `MenuTweaks.Combined.cs`

字段迁移到 `MenuTweaks.cs`（`_footstepExpanded` / `_qolSleepExpanded` / `_worldSprainkleExpanded` 等）

### 10. Bug 修复

| Bug | 修复 |
|------|------|
| `Patch_Speedy_Smash._saved` 静态字段竞态 | 加 `[ThreadStatic]` |
| `Patch_RespawnPlants_Deserialize._hadData` 静态字段 | 加 `[ThreadStatic]` |
| `CheckRespawn` 无场景检查死循环 | 加空场景 guard |
| `OnSceneLoaded/OnSceneChange` 未调用 | 加入 OnSceneWasInitialized |
| `GetTraderManager()` 空指针 | 加 null check |
| `ref Collider` 不必要 | 去掉 ref + 加 null check |
| SpeedyInteractions enabler=null → NRE | 改条件 lambda |
| IntegratedPatches9 (HouseLights) IL2CPP 编译错误 | 修 op_Implicit / SceneManager 歧义 / ColorHSV 转换 |
| 脚步 slider 在死代码文件 | 移到正确的 DrawCharacterAndQoLTab |

### 文件改动

| 文件 | 改动 |
|------|------|
| `IntegratedPatches8.cs` | 新建：TinyTweaks 7 模块 + ShowTraderTrust 全部 patch |
| `IntegratedPatches9.cs` | IL2CPP 编译修复（op_Implicit/ColorHSV/SceneManager） |
| `DynamicPatch.cs` | 新增 ~30 个 Spec（TinyTweaks + SpeedyInteractions 条件挂载） |
| `Settings.cs` | 新增 TT_* 字段 + TraderArrivalDays |
| `Cheats.cs` | 新增 TT_* CheatState 字段 |
| `ModMain.cs` | Sync + ShowTraderTrust tick（室内/60帧）+ OnSceneWasInitialized 调用 |
| `MenuTweaks.cs` | Tab2/3/4 UI 重排 + 交互速度 slider + 杂项说明 + 字段迁移 |
| `MenuTweaks.Gfx.cs` | 改名 Tab5 + 新增车辆视角/室内灯光/FOV 扩展 |
| `Menu.cs` | 商人 section: ShowTraderTrust toggle + TraderArrivalDays slider + 信任+100 直写 |
| `CheatsPatches.cs` | 秒开容器 50x + 秒搜刮 BeginHold + 商人 IsTraderAvailable 天数逻辑 |
| `TldHacks.csproj` | +AssetBundleModule 引用 |
| 删除 7 文件 | MenuTweaks.Character/CraftFire/Light/QoL/WorldItems/Decay/Combined.cs |

### 待验证

1. **室外 FPS** — 应回到接近 190（ShowTraderTrust 室外不 tick + SpeedyInteractions 默认不挂）
2. **脚步 slider** — Tab2 中列 脚步静音下方应有 [+] 展开按钮 + 4 slider
3. **商人到来** — slider 调到 1 天 → 新档第一天商人就可用
4. **信任+100** — 点按钮后 ShowTraderTrust overlay 应显示增加
5. **秒搜刮** — 开 toggle 后搜容器/植物应几乎瞬间完成
6. **交互速度** — Tab4 Col3 拖 slider > 1.0 → 开容器/吃东西/拆解等明显加速
7. **TinyTweaks toggles** — Tab2 Col3"高级微调"section 所有 toggle/slider 可调

---

### 11. 全 UI 双语化 + 遗漏汉化 + BAT 更新

Menu.cs / MenuTweaks.cs / MenuTweaks.Gfx.cs 所有 UI 字符串已全部使用 `I18n.T("中文", "English")` 且英文部分非空。

游戏内硬编码英文汉化（4 处）：`IntegratedPatches3.cs` 埋葬/搜索/临终提示 + `IntegratedPatches9.cs` 开关灯。

`disable_integrated_mods.bat` 新增 4 条：`TinyTweaks-RunWithLantern` / `ImprovedTrader` / `HouseLights` / `ShowTraderTrust`。

---

## v4.0r6 一句话

**HouseLights 整合 + SafeLoad 三重加固(孤儿检测/slider clamp/崩溃哨兵) + 天气锁定 + ImprovedTrader 冲突处理**。

## v4.0r6 改动详情

### 1. HouseLights mod 整合

- **原 mod**: DemonBunnyBon/tld-house-lights（室内灯光开关，非极光亮灯）
- **新文件**: `IntegratedPatches9.cs` — 全部逻辑 + 7 个 Harmony patch 类
- **数据类**: `HLElectrolizerConfig` / `HLElectroLightConfig`（从 MelonMod 继承改为 sealed class）
- **AssetBundle**: `HouseLights.hlbundle` 嵌入为 EmbeddedResource（csproj 新增 ItemGroup）
- **DynamicPatch**: 7 个 Spec，enabler = `CheatState.HL_Enabled`，关闭时零开销
- **UI**: Tab 4 Column 2 油灯 section 下方新增"室内灯光"section（6 toggle + 4 slider）
- **Settings**: 10 个新字段（HL_Enabled/HL_EnableOutside/HL_WhiteLights/HL_NoFlicker/HL_CastShadows/HL_LightAudio/HL_Intensity/HL_RangeMultiplier/HL_CullDistance/HL_InteractDistance）
- **原版 bug 修复**: electroLightSources 循环里 audio 索引误用 electroSources[k] → 统一用 src
- **冲突处理**: 原版 `HouseLights.dll` 已 rename `.disabled`，否则双 patch 冲突

### 2. SafeLoad 三重加固

#### 2a. 孤儿字段检测
- JSON 里有但 Settings 类没有的字段 → orphans 计数 → 触发 needRewrite
- 防止老版本删/改字段后 ModSettings 库二次 Load 行为异常

#### 2b. Slider 范围 clamp
- SafeLoad 时读 `[SliderAttribute]` 的 From/To → 超界值 Mathf.Clamp → clamped 计数
- 防止极端值导致运行时异常

#### 2c. 崩溃哨兵（crash sentinel）
- **写哨兵**: DynamicPatch.Reconcile() 前写 `Mods/TldHacks.loading`
- **清哨兵**: `OnSceneWasInitialized()` 时删除（证明 patch 没导致加载崩溃）
- **检测恢复**: 下次启动发现哨兵存在 → 安全模式：照常加载 JSON（保留 slider/hotkey/菜单位置），但所有 bool toggle 恢复默认值 → 写回 JSON
- **效果**: 群友遇到"进不去存档"只需重启一次，自动恢复，不需要手动删 JSON

### 3. 天气锁定

- **问题**: `SetWeatherStage` 设置天气后 10-20 分钟自动切到下一阶段
- **方案**: 设天气时自动锁定 `CheatState.WeatherLocked = true`，OnUpdate tick 每 600 帧静默重新 apply
- **新增**: `Cheats.SetWeatherStageInternal()` — 静默版本不输出日志不改 lock 状态
- **UI**: 天气按钮行下方新增"锁定当前天气"toggle，取消勾选解锁让天气自然变化
- **新字段**: `CheatState.WeatherLocked` / `CheatState.WeatherLockedStage`

### 4. ImprovedTrader 冲突

- **问题**: `ImprovedTrader.dll` 与 TldHacks 的 TraderManager patch 冲突 → 商人常驻/即时交易失效
- **处理**: ImprovedTrader.dll 已 rename `.disabled`
- **根因**: 两个 mod 同时 Postfix `IsTraderAvailable`，Harmony 执行顺序不确定
- **已完成**: v4.0r7 整合了 ImprovedTrader 的"送货天数"slider

### 5. 其他

- `MenuTweaks.cs`: `DrawSlider` private → internal（修 Menu.cs 访问报错）
- `DynamicPatch.cs`: 新增 `using Il2CppTLD.ModularElectrolizer` + 3 个类型数组 (HLRegisterLightArgs/HLUpdateHUDArgs/HLTooDarkArgs)
- `TldHacks.csproj`: 加 `UnityEngine.AssetBundleModule.dll` 引用 + `HouseLights.hlbundle` EmbeddedResource
- 版本号: MelonInfo 4.0.5，log 输出 v4.0r5

### 文件改动

| 文件 | 改动 |
|---|---|
| `IntegratedPatches9.cs` | **新文件** — HouseLights 全逻辑 + 7 patch 类（~510 行） |
| `Settings.cs` | 新增 10 个 HL_* 字段 + Section "House Lights / 室内灯光" |
| `Cheats.cs` | 新增 HL_* / WeatherLocked / WeatherLockedStage 字段 + SetWeatherStageInternal() |
| `ModMain.cs` | SyncState 加 HL_*；SafeLoad 加孤儿检测+slider clamp+崩溃哨兵；OnUpdate 加天气 tick |
| `DynamicPatch.cs` | 加 using + 3 类型数组 + 7 HouseLights Spec |
| `MenuTweaks.cs` | DrawSlider private→internal；Tab 4 Column 2 加"室内灯光"section |
| `MenuTweaks.CraftFire.cs` | 清理误加的 HouseLights UI（该函数是死代码，从未被调用） |
| `Menu.cs` | 天气区域加"锁定当前天气"toggle |
| `TldHacks.csproj` | 加 AssetBundleModule 引用 + hlbundle EmbeddedResource |
| `HouseLights.hlbundle` | **新文件** — 开关模型 AssetBundle |

### 待验证

1. **HouseLights** — 进室内场景 → 找到墙上开关 → 点击交互 → 灯应亮起；Tab 4 中列"室内灯光"section 可见
2. **天气锁定** — 设极光 → 等 20+ 分钟 → 天气应保持不变；取消勾选"锁定"→ 天气恢复自然变化
3. **崩溃哨兵** — 模拟：手动创建 `Mods/TldHacks.loading` → 启动游戏 → 日志应显示安全模式 + 重置 N 个 bool
4. **SafeLoad 孤儿** — 手动在 JSON 加一个假字段如 `"FakeOld": true` → 启动 → 日志应显示"删除 1 孤儿字段"
5. **商人交易** — 禁用 ImprovedTrader 后，开"商人常驻"→ 无线电可交互

---

## v4.0r5 一句话

**脚步静音 patch 复活 + NLB 电视机字体修复 + 极光提示睡觉黑屏修复**。三个独立 bug，一次 build。

## v4.0r5 改动详情

### Bug 1：脚步静音 toggle 失效

- **症状**：开 SilentFootsteps toggle 仍然听到脚步声
- **根因**：`FootStepSounds.PlayFootStepSound` 有两个重载 `(Vector3,string)` + `(Vector3,string,State)`。DynamicPatch 注册时 `paramTypes=null` → Harmony 内部 `AmbiguousMatchException` → `_failed=true` → patch 永不挂载
- **修复**：分两个 Spec 各 patch 一个重载，加 `Patch_SilentFootsteps_3p` 类

### Bug 2：NorthernLightsBroadcast（电视机 mod）所有文字空白

- **症状**：mod UI 界面出现了，但所有 TextMeshPro 组件文字为空
- **根因**：IL2CPP 模式下 NLB assetbundle prefab 的 TMP_Text.fontAsset 反序列化后引用断 → null → 不渲染。mod 自带字体 ArcadePlayingNow 加载成功（Player.log 可见），但 prefab 不引用它
- **修复**：`NLBFontFixHelper` 在 `OnSceneWasInitialized` 时反射找 `TVUI` 类 → Harmony patch `TVUI.Awake` Postfix → 遍历所有 `GetComponentsInChildren<TextMeshProUGUI>(true)`，null font 的赋 `TMP_Settings.defaultFontAsset` 或内存中找到的 ArcadePlayingNow
- **新依赖**：`TldHacks.csproj` 加 `Unity.TextMeshPro.dll` 引用

### Bug 3：极光提示（AuroraSense）开了后睡觉黑屏

- **症状**：开 AuroraSense toggle → 睡觉 → 极光来了唤醒 → 永久黑屏（鼠标能动、交互文字能出、不会保存）
- **根因**：WakeUpCall 整合时把一个 mod 拆成多个独立 toggle。`AuroraSense` 可以独立于 `WakeUpCall` 开启，但触发唤醒后恢复画面需要的三个安全网（BeginSleeping Prefix 设 PostWakeUp 回调、OnUpdateTick 兜底检测、EndSleeping_Safety Postfix）全绑在 `QoL_WakeUpCall` 的 enabler/guard 上。只开 AuroraSense 时安全网全不跑 → CameraFade overlay 永久不恢复
- **修复**：4 处条件改成 `QoL_WakeUpCall || QoL_AuroraSense`：
  1. `DynamicPatch.cs` BeginSleeping Spec enabler
  2. `DynamicPatch.cs` EndSleeping_Safety Spec enabler
  3. `IntegratedPatches3.cs` WakeUpCallHelper.OnUpdateTick guard
  4. `IntegratedPatches3.cs` Patch_WakeUpCall_EndSleeping_Safety.Postfix guard

### 4 个材质声音 slider（非 bug）

- InvWeightMetalVol / WoodVol / WaterVol / GeneralVol slider 改后"无效果"是 Wwise RTPC 的正常语义：值在声音事件触发时才读取。调完 slider 需要走动触发新脚步才能听到差异。非代码问题，用户操作指引。

### 文件改动

| 文件 | 改动 |
|---|---|
| `DynamicPatch.cs` | 脚步 Spec 拆双重载 + FootStepArgs2/3 类型数组；BeginSleeping/EndSleeping_Safety enabler 加 AuroraSense |
| `CheatsPatches.cs` | 新增 `Patch_SilentFootsteps_3p` 类（3 参重载 Prefix） |
| `ModMain.cs` | 新增 `NLBFontFixHelper` 类 + OnSceneWasInitialized 调用 |
| `IntegratedPatches3.cs` | OnUpdateTick + EndSleeping_Safety guard 加 AuroraSense |
| `TldHacks.csproj` | 加 `Unity.TextMeshPro.dll` 引用；版本 4.0.5 |
| `Menu.cs` / `ModMain.cs` | 版本号 r4→r5 |

### 待验证

1. **脚步静音** — 开 toggle → 走几步应完全无声
2. **电视机文字** — 打开 NLB 面板应能看到目录名 / 按钮 / "已停止" 状态文字
3. **极光提示不黑屏** — 开 AuroraSense（可以不开 WakeUpCall）→ 睡觉 → 等极光 → 唤醒后画面应正常恢复
4. **RTPC slider** — 改材质音量 slider → 走动 → 听脚步音量变化

---

## v4.0r4 一句话

**修 UT 手电筒 InfiniteBattery 在非极光夜失效**(极光夜正常)—— 元凶是 v4.0r3 的 `Patch_UT_FlashUpdate` 用 Prefix 抢在 vanilla 之前设 `charge=1f`,vanilla 内部某分支(疑非极光夜专属)读到"满电+某状态"组合触发不该触发的副作用。改成纯 Postfix 完全照搬 UT 反编译写法。

## v4.0r4 改动详情

### Bug:无限电量在非极光夜失效,极光夜正常

- **症状**(用户实测):
  - 开 UT_FlashInfiniteBattery toggle → 非极光夜手电不亮(电量没真维持在满)
  - 极光夜正常工作
  - **手动充满电后非极光夜光照也正常** —— 说明"charge=1 时一切正常",修复方向就是"自动维持 charge=1"
- **根因**:v4.0r3 的写法
  ```csharp
  static void Prefix(FlashlightItem __instance) {
      if (s.UT_FlashInfiniteBattery) __instance.m_CurrentBatteryCharge = 1f;
  }
  ```
  Prefix 抢在 vanilla Update body 之前设 1f → vanilla 内部某分支(具体哪个分支未深查,UT 反编译无对应代码,推测是非极光夜的扣电/state 同步逻辑)读到"满电"状态触发副作用 → state 重置 / Light disable
- **修复**:改成纯 Postfix,完全照搬 UT 反编译第 867-899 行写法
  ```csharp
  static void Postfix(FlashlightItem __instance) {
      // ... anyActive early return ...
      if (s.UT_FlashInfiniteBattery) __instance.m_CurrentBatteryCharge = 1f;
      // 自定义 duration / recharge
  }
  ```
  vanilla Update body 先按原扣电规则跑(非极光时 `charge -= dt/dur`)→ Postfix 末尾覆盖 `charge=1f` → vanilla 内部 `if (charge<=0) state=Off` 永不触发(已是 1)。完全等价 UT 已验证多年的写法。

### 关键设计点

- **不要抢在 vanilla 之前改字段** —— vanilla IL2CPP 内部分支不可见,Prefix 改字段可能引发"vanilla 看到不该看到的状态"。Postfix 模式 vanilla 完整跑完原行为再覆盖,最稳。
- 用户实测"手动充电后正常"是关键诊断信号 —— 说明 EnableLights / FirstPersonFlashlight / IsLit 其他链路都对了,只是 charge 没维持。

### AfflictionComponent NullRef 海量报错

用户 Player.log 里大量 `AfflictionComponent.IsBuffActive / IsDebuffActive` NullRef —— **不是 TldHacks 的 patch**,是 AfflictionComponent mod 自己的 bug,在它的 IsBuffActive/IsDebuffActive 内部 NRE,数千条重复。与 TldHacks 无关,日志噪音可忽略;真要修需要去那个 mod 仓库提 issue。

### 版本号统一 → v4.0r4

| 文件 | 改动 |
|---|---|
| `Menu.cs:420-421` | UI 标题 `TldHacks v4.0` → `v4.0r4`(中英双语) |
| `ModMain.cs:6` | `MelonInfo("4.0")` → `"4.0.4"` |
| `ModMain.cs:49` | 启动 log `v4.0 loaded` → `v4.0r4 loaded` |
| `TldHacks.csproj:11` | `<Version>4.0</Version>` → `4.0.4` |

### 文件改动

| 文件 | 改动 |
|---|---|
| `IntegratedPatches7.cs:114-156` | `Patch_UT_FlashUpdate` 删 Prefix,改纯 Postfix(完全照搬 UT 第 867-899)。anyActive early return 保留,自定义 duration/recharge 保留 |
| `Menu.cs` / `ModMain.cs` / `TldHacks.csproj` | 版本号 r4 |

## 待做 / 已知问题

### Shotgun mod 2.55 兼容性（群友需求，搁置）

- **来源**: https://github.com/monsieurmeh/MehToolBox/releases/tag/shotgun
- **结论**: 不可快速修——闭源 + 20+ Harmony patch + 6 前置 + 自定义 GunType=4。优先等作者更新；如果作者弃坑，需全套前置 + Player.log 才能尝试 binary patch
- **可能的 2.55 断裂点**: `GunItem`/`vp_FPSWeapon`/`PlayerAnimation` 签名变化、Addressables catalog 变化、ModComponent 不兼容

---

# TldHacks v4.0r3 — 交接文档

## v4.0r3 一句话

**修蓝图制作左侧分类筛选失效** —— 元凶是 v3.0.6 偷懒的 `Patch_CraftAnywhere` Postfix 把 `__result` 一刀切成 `true`,顺带把分类筛选也覆盖了。

## v4.0r3 改动详情

### Bug:蓝图分类按钮(生存/生活/制造)点了无反应

- **根因**: `Panel_Crafting.ItemPassesFilter(BlueprintData bpi)` 内部同时做两件事 —— 位置/工作台检查 + 左侧分类筛选 —— 共用一个返回值。`Patch_CraftAnywhere` v3.0.6 实现:
  ```csharp
  static void Postfix(BlueprintData bpi, ref bool __result) {
      if (!CheatState.Craft_Anywhere || bpi == null) return;
      __result = true;  // ← 一刀切,分类筛选也被覆盖
  }
  ```
  → 不管点哪个分类按钮,所有蓝图都通过 filter 全显示

- **修复**: 改回 CraftAnywhereRedux 原版思路 —— Prefix 临时把 `m_RequiredCraftingLocation` 字段设 0(AnyWhere),vanilla 内部位置检查通过 + 分类筛选独立工作;Postfix 还原原值,**不污染存档**:
  ```csharp
  internal static class Patch_CraftAnywhere {
      private static FieldInfo _locField;     // 反射 cache,免依赖 CraftingLocation enum 的 namespace
      [ThreadStatic] static object _saved;
      [ThreadStatic] static bool _hasSaved;

      static void Prefix(BlueprintData bpi) {
          _hasSaved = false;
          if (!CheatState.Craft_Anywhere || bpi == null) return;
          if (!_resolved) { _locField = AccessTools.Field(typeof(BlueprintData), "m_RequiredCraftingLocation"); _resolved = true; }
          if (_locField == null) return;
          try {
              _saved = _locField.GetValue(bpi);
              _hasSaved = true;
              _locField.SetValue(bpi, Enum.ToObject(_locField.FieldType, 0));
          } catch { _hasSaved = false; }
      }

      static void Postfix(BlueprintData bpi) {
          if (!_hasSaved || bpi == null || _locField == null) return;
          try { _locField.SetValue(bpi, _saved); } catch { }
          _hasSaved = false;
      }
  }
  ```

- **DynamicPatch 注册槽**: `null, "Postfix"` → `"Prefix", "Postfix"` (`DynamicPatch.cs:357`)

### 关键设计点

- 反射定位字段而非编译时引用 `CraftingLocation` enum,避免 namespace 不确定问题(IL2Cpp 反编译命名约定混乱)
- ThreadStatic + 单值 cache 而非字典 —— Panel_Crafting.ItemPassesFilter 是 UI 线程串行调用,不会嵌套不会并发
- Prefix 抛异常时 `_hasSaved = false`,Postfix 不会乱还原

### 文件改动

| 文件 | 改动 |
|---|---|
| `IntegratedPatches3.cs:454-490` | `Patch_CraftAnywhere` 从 Postfix 改 Prefix+Postfix 配对 + 反射 |
| `DynamicPatch.cs:357` | 注册槽 `null, "Postfix"` → `"Prefix", "Postfix"` |

### 待验证

1. 蓝图分类按钮(生存/生活/制造/...)点击应正常筛选
2. "随地制作" toggle 开着时仍能在任意位置造工作台限制蓝图
3. toggle 关掉后位置限制恢复

---

# TldHacks v4.0r2 — 交接文档

> 2026-05-09 当前活跃版本。v4.0 主段保留在下方,v3.0.5 及更早段落待归档至 `HANDOFF_archive.md`。

## v4.0r2 一句话

**MotionTrackerLite 小地图终于真正自动开**(helper 反射字段名一直错,从未生效);物品生成器**按钮/Label 垂直对齐**;补齐 v4.0 主段未记录的几大块 — **物品生成器双语切换** / **失效物品 IsValid 自动隐藏** / **mod 物品 (mod) 后缀** / **9 类分类重排** / **补 100 条 vanilla 漏物品** / **强心针** / **SafeLoad 跨版本配置兼容**。

---

## v4.0r2 改动详情

### 1. MotionTrackerLite 字段名修复(从未生效的 helper)

- **根因**: `MotionTrackerLiteHelper.EnsureVisible()` 反射查的字段名是 `"Visible"`,但 MotionTrackerLite/Tracker.cs 里 `Visible` 是 entity 实例字段(`public bool Visible;` 第 20 行,挂在 entity 数据上),用 `BindingFlags.Static` 找不到 → `_visField = null` → SetValue 跳过 → **EnsureVisible 每次都什么都没做**,每 5 秒 tick + 场景加载时的"强制启用"全部 silent fail
- 真正控制雷达开关的是 `Tracker.Enabled` (static, 第 25 行),默认 false,Tracker.cs:209 `if (!Enabled || !IsInGame) return;`
- **修复**: `ModMain.cs:543` `t.GetField("Visible", ... Static)` → `t.GetField("Enabled", ... Static)`
- 室内自动隐藏的逻辑 mod 自己已经做了(`TrackerConfig.OnlyOutdoors = true` 默认,Tracker.cs:209/260 双重检查),不用 TldHacks 介入
- **效果**: 进游戏(室外)雷达自动出现,室内自动隐藏,不需要再按 U

### 2. 物品生成器按钮 / Label 垂直对齐

- **根因**: GUI.Label 默认 `TextAnchor.UpperLeft`(文字贴顶),GUI.Button 默认 `MiddleCenter`(垂直居中),同行 ROW_H 高度下视觉差 5-7 像素
- **修复**: `Menu.cs` 加专用 style `_rowLabelStyle`,`alignment = MiddleLeft`,只用于物品生成器行 `GUI.Label`(不动其他 Label 默认行为)
- 字段声明 + InitStyles 创建 + ConfigureLabel 后单独设 alignment + 渲染处带 style 参数,4 处微改

### 3. 物品生成器双语切换(中英文跟 mod UI 语言)

- mod UI 切英文时物品名也切英文,游戏 Localization 没有"按语言取"public API → 两边都拿到再 mod UI 自己切
- `ItemEntry` 加 `NameZh / NameEn`;`Menu.cs:ResolveDisplayNames` 改用 `gi.GetDisplayNameWithoutConditionForInventoryInterfaces()` 拿当前游戏语言真名,按是否含中文字符(0x4E00-0x9FFF)分流到 `NameZh / NameEn`
- 缺英文时 PrefabName 拆驼峰兜底(`GEAR_BasicWoolHat` → `Basic Wool Hat`),Menu 渲染按 `I18n.IsEnglish` 切

### 4. 失效物品 IsValid 自动隐藏

- 群友反映 spawner 里 BowB / RifleB / 双管 等点了显示 +x1 但实际没生成
- **根因**: 那些 GEAR_ prefab 当前游戏版本不存在(可能是 mod 作者从社区/旧版本抄来的)
- **修复**: `ItemEntry.IsValid` 字段;`ResolveDisplayNames` 时 `LoadGearItemPrefab` 返回 null 标 false;`RebuildFilter` 跳过无效条目
- 一次性覆盖所有失效 prefab,不需要逐条删数据库;以后游戏更新加回来会自动重新出现

### 5. mod 物品 (mod) 后缀

- `ItemEntry.IsMod` 字段;`Resolve` 时给 `ItemDatabaseMod.All` 的 NameZh/En 都追加 ` (mod)`;原版 ItemDatabase 的物品不加
- 视觉上一眼分辨整合 mod 物品 vs 原版物品

### 6. 物品生成器 9 类分类重排

- 原 9 类(食物/工具/武器/弹药/医疗/衣物/材料/DLC武器/其他)→ 新 9 类(食物/饮料/工具/武器/医疗/衣物/狩猎/材料/装饰),tab 数不变
- **武器** = 原武器+弹药+DLC武器三合一(13+24+11 = 48 → 47 实际,失效隐藏后)
- **饮料** = 茶/咖啡杯/汽水/水(从食物拆 18 条)
- **狩猎** = 兽皮/兽骨/肉块/尸体/羽毛/肠线/鞣制皮革(从材料拆 31 条)
- **装饰** = 书/笔记/眼镜笔记/占位(从其他拆 44 条)
- 边界判断:净水片 → 工具(净化功能);桦树皮(晒干)→ 材料(泡茶原料);睡袋/熊皮睡袋 → 工具(装备性质)
- 用 Python 脚本批量改 ~130 条 ItemEntry 的 Category 字段,代码不动

### 7. 补 100 条 vanilla 漏物品

- 数据来源: `tld_Data/StreamingAssets/aa/catalog.json`,正则提取 `GEAR_*` 后过滤 `_A/_Mat/_Dif/_complete/_Story` 等贴图变体后,得 545 主 prefab,与 ItemDatabase 现有比对
- 排除剧情 89 条(VisorNote × 32 / BackerNote × 12 / PostCard × 21 / BunkerClue × 4 / Key × 13 / 其他 14)
- 补 100 条高价值 vanilla:**冰爪** / **防弹背心** / **林业背包** / **量产箭三件套** / **44 马格南** / **生存刀** / **航空餐(鸡/素)** / **腌菜三件套(蛋/洋葱/黄瓜)** / **干苹果干芒果** / **暖宝宝** / **桦树苗(生)** / **学习书 7 本**(弓术/烹饪/剥皮/生火/修补/枪械/冰钓) / 等
- IsValid 兜底:猜错的 prefab 名自动隐藏

### 8. 加强心针

- `GEAR_EmergencyStim`(catalog.json 验证存在,游戏 GameManager 类有引用)→ 医疗类
- 真名走 Resolve 拿,中文应该是"强心针"

### 9. SafeLoad 跨版本配置兼容(群友痛点根治)

- **痛点**: 群友拿老 JSON + 新 DLL → ModSettings 库整个 Load 失败 → settings 残废 → 功能集体失效。原解法"删 JSON 重生成"丢失用户原配置
- **方案**: `ModMain.RegisterSettings()` 替换 `Settings = new ...; AddToModSettings()`:
  1. 反射遍历 `TldHacksSettings` public 实例字段,逐字段 try `JObject[name].ToObject(fieldType)` — 兼容字段保留用户值,不兼容字段静默 fallback 默认
  2. 任何字段被丢弃 → 备份原文件 `TldHacks.json.broken-{yyyyMMdd-HHmmss}` + 用 `JsonConvert.SerializeObject` 把 hydrate 后的 settings 写回(净化文件,让库 Load 不再撞旧字段)
  3. 三层 catch 兜底:救援自爆 / 库 Load 仍失败 / 序列化失败 → 各自走"备份+全默认重置"路径
- **性能**: 仅 `OnInitializeMelon` 跑一次(启动多 50-200ms);运行时 0 开销
- **依赖**: `TldHacks.csproj` 加 `Newtonsoft.Json` reference (HintPath: `$(TldMlNet6)\Newtonsoft.Json.dll`),运行时由 ModSettings 库自带,不增整合包体积

### 文件改动 (v4.0r2 增量)

| 文件 | 改动 |
|---|---|
| `ModMain.cs` | `MotionTrackerLiteHelper.EnsureVisible` 字段名 `"Visible"`→`"Enabled"`;新增 `RegisterSettings()` 方法替换 `new TldHacksSettings(); AddToModSettings()`(SafeLoad 主体) |
| `Menu.cs` | 加 `_rowLabelStyle` (MiddleLeft) / `HasChineseChar` / `PrefabToEn`;`ResolveDisplayNames` 改用 `GetDisplayNameWithoutConditionForInventoryInterfaces` + 中英分流;`RebuildFilter` 跳过 IsValid=false;物品行 GUI.Label 带 _rowLabelStyle |
| `ItemDatabase.cs` | `ItemEntry` 加 `IsValid / IsMod / NameZh / NameEn` 字段;Categories/CategoriesEn 改 9 类;~130 条 ItemEntry Category 重分配;尾部加 100 条新条目;医疗加强心针 |
| `TldHacks.csproj` | 加 `Newtonsoft.Json` reference |

### 待验证(下次进游戏)

1. **小地图自动开** — 进室外存档应立即出现,室内自动消失;按 U 应仍可手动 toggle(未验证强制 keep 行为是否覆盖手动关闭)
2. **物品生成器对齐** — 按钮文字与右侧物品名垂直居中
3. **英文切换** — mod 设置切 English,物品生成器名字应跟着切英文(游戏中文模式下英文为 PrefabName 拆驼峰兜底,游戏英文模式下英文为真名)
4. **SafeLoad 实战** — 群友更新 DLL 后看 Player.log 应有 `[Settings] 配置全部兼容 (N 字段)` 或 `救援老配置: 保留 X 字段, 丢弃 Y` 的日志;无报错;群友老配置(hotkey/slider)应保留

---

# TldHacks v4.0 — 交接文档

> 2026-05-09 当前活跃版本。v3.0.5 及更早段落保留在 `HANDOFF_archive.md`(待归档)。

## v4.0 一句话

UI 大重排 + 文案统一字数 + 加 muted 注释帮记忆 + 主页操作类聚到第 3 列 + 删一键武器(Tab1 spawner 已有) + 商人合并 + 新增**手电筒 IMGUI section** + **燃烧上限/无烤焦/手电筒不亮/煤油灯名**四个老 bug 修。

---

## v4.0 改动详情

### 1. UI 重排 — 按"语义分组"原则归列

#### 主页(Tab 0)
- **删** "一键获取武器"section(弓/步枪/左轮/斧头/猎刀/箭+步枪弹+左轮弹) — Tab 1 物品生成器已覆盖,留着冗余
- **合** "商人 & 美洲狮" + "商人 uConsole 命令" 合并成一个 section(toggle + 6 个控制台按钮)
- **挪** "制作 & 修理"+"锁 & 容器" 从 column 1 → column 3(快速操作下方),按"操作"语义聚类
  - **新 column 1**:生存 / 温饱 / 增益 Buff / 全局速度 / 技能(纯角色状态)
  - **新 column 2**:动物 / 环境 & 篝火 / 世界时钟 / 商人 & 美洲狮(世界 + NPC)
  - **新 column 3**:快速操作 → 制作 & 修理 → 锁 & 容器 → 物品 & 装备 → 武器 & 瞄准 → 一次性操作(操作 + 装备)
- **燃烧上限滑条**:12-9999h → **12-1000h**(50天上限,够长够实用)

#### Tab 2 人物 & 生存
- **挪** "开关门 (FullSwing)" 从 column 1 → column 2 末尾(车辆视角后)
- **挪** "时间加速" 从 column 1 → column 3 末尾(其他便利功能后)
- **新 column 1**:移动 & 体力 / 跳跃(只这两个,留呼吸空间)
- **新 column 2**:交互 & 感知 / 生活品质 / 车辆视角 / 开关门
- **新 column 3**:随地睡觉 / 自动测绘 / 其他便利功能 / 时间加速
- **column 2/3 单列 → 双列布局**:每行放 2 toggle,竖向高度减半
- **每 section 末尾 1~2 行 muted 灰字注释**解释不直观的开关(取炭不戴/任意丢弃/扭伤不存/不要纯黑 等)
- **column 1 保留详细 muted**(slider 多,需要 1.0=原版 提示)

#### Tab 3 装备 & 世界
- **杂项开关 11 toggle 单列 → 双列**(11 行 → 6 行 + 2 行 muted 注释解释雪橇/难度徽章/室内贮石)

#### Tab 4 光火 & 制作
- **新增手电筒 section**(column 1 末尾):
  - 5 toggle 双列:无限电量 / 跨场保留 / 远光极光 / 极光闪烁 / 随机起电
  - 3 slider:近光持续(h) / 远光持续(h) / 充电时间(h)
  - 2 行 muted 注释解释含义,矿工灯参数走 ModSettings
- **防左键误灭** 单列 → 双列(锁火把键 / 锁油灯键)+ 1 行注释
- **火把/煤油灯 section** 末尾各加 1~3 行 muted 注释(状态范围/油耗倍率/惩罚阈值)

#### Tab 5 图形
- **准星 section** 武器选择改双列(投掷石块 / 步枪瞄准 / 弓箭瞄准)+ 用法注释

### 2. 文案字数对齐

#### 主页生存 section(全 4 字)
- 无敌模式 / 无坠落伤
- 免扭伤险 / 免动物伤
- 不会窒息 / 免死亡罚

#### 主页快速操作(全 3 字)
- 秒烹饪 / 防烤焦
- 秒搜刮 / 秒割肉
- 秒拆解 / 秒风干
- 秒爬绳 / 秒采集

(注:"秒采修" 改 "秒采集" — 原 patch 实测只秒采,不修)

### 3. 新增独立"防烤焦"toggle

- 放在"快速操作"section,秒烹饪右侧
- **秒烹饪自带防烤焦**(Cooking → 推 elapsed,Ready → clamp)
- **独立防烤焦** toggle 仅 clamp Ready elapsed,不秒熟 — 给想正常等熟的玩家用
- 共享 `Patch_CookingPot_Update` Prefix,触发条件 `QuickCook || NoBurn`

### 4. 关键 BUG 修

| BUG | 根因 | 修复 |
|---|---|---|
| **燃烧上限失效** | 原 `Patch_Fire_AddFuel_Uncap` 直接覆写 fire 实例 `m_BurnMinutesIfLit`,vanilla AddFuel 内部 cap 检查没动,导致"4000h加满后改9999h不能加" | 删旧 patch,改 `Patch_FireManager_CalcStartTime_Uncap` **Prefix** on `FireManager.PlayerCalculateFireStartTime`,把 `m_MaxDurationHoursOfFire` 改大,vanilla 自然按新 cap 算累加。等价 CT 的 `MonoMethodInit` 时机(老版用 Postfix 时游戏已按旧 12h cap 算完无效) |
| **手电筒有电不发光** | InfiniteBattery 只写 `m_CurrentBatteryCharge=1f`,vanilla 内部把 `m_State` 重置为 0(Off)时 Light/FX 子组件不 enable | `IntegratedPatches7.cs` `Patch_UT_FlashUpdate.Postfix` 加:InfiniteBattery 开启时 `if ((int)m_State == 0) m_State = LowBeam(1)`,确保始终至少在低光档 |
| **煤油灯刷不出** | `ItemDatabase.cs:196` 写 `GEAR_KeroseneLamp`,真名是 `GEAR_KeroseneLampB`(catalog.json 验证),原写法 prefab 不存在 | 改 `GEAR_KeroseneLampB` |
| **秒烹饪不防烤焦** | 旧版 QuickCook 只在 Cooking 阶段推 elapsed,Ready 后 vanilla 继续累加 elapsed,够久会进 Ruined | Prefix 加 Ready 状态 clamp:`elapsed > cookTime/60*1.01` 时 clamp 回 1.01,永远停在熟透阈值 |

### 5. 全局版本号统一 → v4.0

- `ModMain.cs:6` `MelonInfo("3.0.4")` → `"4.0"`
- `ModMain.cs:49` 启动 log `"v3.0.5 loaded"` → `"v4.0 loaded"`
- `TldHacks.csproj:11` `<Version>3.0.5</Version>` → `<Version>4.0</Version>`
- `Menu.cs:416-417` 窗口标题双语 `"TldHacks v3.0.5"` → `"TldHacks v4.0"`
- 历史 `// v2.7.x / v3.0.x` 注释保留(代码考古用)

---

## 文件改动清单

| 文件 | 行数变化 | 内容 |
|---|---|---|
| `Menu.cs` | +50 / -50 | 主页删一键武器,商人合并 uConsole,锁&容器/制作&修理 移到 col 3,字数对齐,版本号 4.0 |
| `MenuTweaks.cs` | +120 / -180 | Tab 2 双列 + 重排,Tab 3 杂项双列,muted 注释加回 |
| `MenuTweaks.Gfx.cs` | +5 / -7 | 准星双列 + 注释 |
| `CheatsPatches.cs` | -25 / +18 | 删 Patch_Fire_AddFuel_Uncap,加 Patch_FireManager_CalcStartTime_Uncap (Prefix);Patch_CookingPot_Update Prefix 加 Ready clamp |
| `IntegratedPatches7.cs` | +10 | Patch_UT_FlashUpdate Postfix 加 m_State 强制 LowBeam |
| `ItemDatabase.cs` | -1 / +1 | GEAR_KeroseneLamp → GEAR_KeroseneLampB |
| `Cheats.cs` / `Settings.cs` / `ModMain.cs` | 各 +3 | 加 NoBurn 独立 toggle 字段 + sync |
| `default_TldHacks.json` / `TldHacks.default.json` | 各 +1 | NoBurn 默认 false |
| `DynamicPatch.cs` | ±1 | Fire AddFuel patch 替换为 FireManager.PlayerCalculateFireStartTime |
| `ModMain.cs` / `TldHacks.csproj` | ±1 | 版本号 4.0 |

---

## 待验证(下次进游戏 / 群友测)

1. **燃烧上限累加** — 4000h 加满 → 拉到 9999h 加柴。验 vanilla 是否真按 m_MaxDurationHoursOfFire 累加。
2. **防烤焦独立 toggle** — 关 QuickCook,只开 NoBurn,等肉熟透后停留几小时,验是否真不变 Ruined。

---

## 不要碰的(用户明确说过)

- Menu.cs 的 codex 风格按钮宽度/间距/栅格 — 只许 section 重分组,不许调控件尺寸
- HANDOFF_archive.md / 历史 v2.7.x v3.0.x 注释 — 代码考古用,不删

---

## 编译 & 部署

```
dotnet build "D:/Github/tld--2.55/TldHacks/src/TldHacks.csproj" -c Release -nologo -v minimal
```

输出 `bin/Release/TldHacks.dll` 自动 copy 到 `D:/Steam/steamapps/common/TheLongDark/Mods/TldHacks.dll`。游戏在跑会锁住目标 DLL(MSB3027),关游戏重 build 即可。

当前版本编译:0 警告 0 错误。
