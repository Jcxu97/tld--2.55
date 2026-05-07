# TldHacks v3.0.4 — 交接文档

> 2026-05-08 发布版本。完整改动历史见 `HANDOFF_archive.md`。

## 一句话

整合 4 个独立 mod (UT/StackManager/PlaceFromInv/KeroseneLampTweaks DIY) + 5 个 BUG 修 + 4 项性能优化 + UI 整理 + 版本号 3.0.4 + 桌面发布包。

---

## 已替代独立 mod (4 个)

| Mod | 说明 |
|---|---|
| KeroseneLampTweaks v2.4.1 | 7 参数(放置/手持/开启/持续/阈值/惩罚/静音);颜色 6 项跳过 |
| PlaceFromInventory v1.1.3 | 6 patch + 3 toggle(背包右键放置/允许贴近/Ctrl 整堆) |
| StackManager v1.0.6 | GearItem.Awake patch + 默认列表(Potato/StumpRemover);防腐 HP |
| UniversalTweaks v1.4.8 | 35 项(常用 10/食物 2/岩石 3/喷漆 3/雪橇 6/手电筒 11) |

**保留独立**: MapManager v1.1.7 (用户决定),整合代码已删,功能由独立 dll 提供。

---

## 关键 BUG 修复

| BUG | 根因 | 修复 |
|---|---|---|
| **GodMode 不免坠落/扭伤** | toggle 独立 | `ModMain.Sync` + `Menu.Toggle` 写入时 `Settings.X \|\| Settings.GodMode` |
| **背包负重无效** | dirty-check 让第二帧失效(vanilla Encumber.Update 重置字段) | 移除 dirty-check,每帧 Postfix 写 7 字段(照抄 UT) |
| **背包 toggle 点不开** | TextField + Slider 双控件 IMGUI 冲突 | 删 TextField,纯 Slider |
| **衣物坠落仍撕裂** | IL2CPP `Prefix return false` 不可靠 | 改 `ref fallHeight = 0`,让 vanilla 算 damage = f(0) = 0;ApplyFallDamage 入口也清零 |
| **割肉数量翻倍** | QuickHarvest + QuickAction 双重触发(vanilla 自动 + Runner Tick) | `Patch_Harvest_Start/StartQuarter`: `if (CheatState.QuickHarvest) return;` |
| **stack-evolve 数量丢失** (10 鲜→1 干) | vanilla DoEvolution 整 stack 替换为 1 个 evolved | Prefix 中 `Object.Instantiate(prefab.gameObject)` + `AddItemToPlayerInventory` 补 N-1 个副本 |
| **商人刷新清单按钮无效** | `RunCommandSilent` 要求 m_On=true,旧版立即设回 false | 执行期间临时 `m_On=true`,结束恢复 |
| **GearDecay 细分不生效** | `HasNonDefaultMultiplier` 只检查 10/38 字段 | 扩展全 38 字段 |
| **Decay 默认值误导** | GeneralDecay=0.15 等"伪原版" | 全部默认 = 1.0 真原版 |

---

## 性能优化 (r4)

1. **`Patch_KeroseneLamp_LightRange` Light 缓存** — `lampInstanceID → (Light[], baseRange[])`,第一次扫一次,后续帧直接索引数组(消除每帧 `GetComponentsInChildren<Light>(true)` + GC 分配)
2. **删除 `Patch_UT_FlashFlicker`** — 空 patch(永远 return true),每个 LightRandomIntensity 每帧少 1 次 Harmony bridge
3. **`Patch_UT_FlashUpdate` early return** — 默认值时直接 return,避免每帧 3 setter + name 比较
4. **`Patch_Harvest_Start` 防双重** — 见上方 BUG 修复

---

## UI 整理

22 个 section 标题清理(去 mod 名/版本号/多余括号):
- 速度 & 体力 (SonicMode) → 移动 & 体力
- UT 整合 v1.4.8 → 杂项开关
- KeroseneLamp DIY v2.4.1 → 煤油灯
- StackMgr v1.0.6 → 物品堆叠
- 等

---

## 版本号

- `MelonInfo "TldHacks"` 2.8.0 → **3.0.4**
- `Menu 标题` v2.7.99 → **v3.0.4**
- `csproj <Version>` 1.0.0 → **3.0.4**

---

## 文件清单

新增/重写文件:
- `IntegratedPatches5.cs` — PlaceFromInventory (148 行,精简版,旧 MapManager 部分已删)
- `IntegratedPatches6.cs` — UT 中型 14 patches (常用/食物/岩石/喷漆/雪橇)
- `IntegratedPatches7.cs` — UT 手电筒 5 patches + StackManager + 默认列表
- `RECOMMENDED_MODS.md` — 推荐配套 mod 清单

修改文件:
- `Settings.cs` — 加 ~80 个 UT/Stack/Lamp/PlaceFromInv 字段
- `Cheats.cs` — `LampMute` 字段 + Default 改 30
- `CheatsPatches.cs` — Patch_TechBackpack 重写 + KeroseneLamp 全功能 + EvolveItem stack fix + FallDamage ref fallHeight
- `ConsoleBridge.cs` — Run() m_On=true 期间执行
- `DynamicPatch.cs` — KeroseneLamp.Update + Encumber.Update + OnIgniteComplete 新挂载
- `MenuTweaks.cs` — Combined view 加 UT/Stack/Lamp DIY section + 22 标题清理
- `Menu.cs` — 背包 UI 简化为纯 Slider + 标题升 v3.0.4
- `ModMain.cs` — GodMode 蕴含 NoFallDamage+NoSprainRisk
- `TweaksRuntime.cs` — DecayState 38 字段 + 默认值改 1.0

---

## 已知遗留 todo

| # | 事项 | 影响 |
|---|---|---|
| 1 | UT_TravoisOverrideMovement | toggle 在但无效(CarryDisplayError 是 IL2CPP 嵌套 enum,`[HarmonyPatch]` 静态注册解析不到);下次用 Harmony.Patch 反射 |
| 2 | StackManager 自动合并 | TryAddToExistingStackable / AddToExistingStackable 未做(StackingUtilities.Do 状态机);影响"拾取/丢入容器同种物品自动合并" |
| 3 | UT 容器 40 项细分 | 用户同意跳过(InfiniteContainer 等价) |
| 4 | UT MRE 棕色纹理 | TextureSwapper 复杂,鸡肋 |
| 5 | UT RemoveMainMenuItems | 与 SkipIntro 略不同(删 Wintermute/新闻菜单项 vs 隐藏轮播),用户感知低 |
| 6 | UT FlashExtended 完整 | 只整合"跨场景电量保持",光源切换跳过 |

## 用户报告未修 BUG

- **极光天气下睡觉黑屏** — v3.0.2 加了 PostWakeUp 安全网仍发生;**临时方案**: 关 QoL_AuroraSense;**需用户**: 黑屏发生时机+解除方式
- **动物脂肪做油显示错误** — **需用户**: 错误显示什么(图标乱?文字乱?数量错?哪个配方?)

---

## 下次任务 (Nexus 国际站发布前)

### 1. Settings.cs 全字段双语化
- 现状: `[Name("中文")]` / `[Description("中文")]` 大量只有中文(~150 字段),英文用户在 ModSettings 配置面板看不懂
- 已双语的: 老版基础作弊字段 (GodMode/NoFall/Hunger 等用 "English(中文)" 风格)
- 待补: v3.0.4 新加的 UT/Stack/Lamp DIY ~50 项 + 老版未双语的衰减/扭伤/汽水/等 ~100 项
- 总数: ~150 个 [Name] + ~80 个 [Description] + ~20 个 [Section]
- 原则: 改成 `"English (中文)"` 形式,英文在前
- **不影响功能**(只改字符串显示),Settings.cs 1156 行,工作量约 1 小时
- 文件: `D:\Github\tld--2.55\TldHacks\src\Settings.cs`

### 2. Nexus Mods 英文介绍
- 平台: nexusmods.com (国际站)
- 风格: 详细 README,markdown,带截图
- 内容要点:
  - 一句话介绍 (Replaces N independent mods)
  - 整合的 mod 清单 + 各 mod 链接
  - 已知 BUG / 不影响功能的限制
  - 安装步骤 (装 MelonLoader + 解压根目录)
  - 截图 (Menu UI / Spawner / Quick Teleport)
  - Credits (整合源 mod 作者 / Hinterland)
  - License / 反馈渠道
- 必须先做完任务 1 (双语化) 才能发,否则英文用户体验差

---

## 编译/部署

```
cd D:\Github\tld--2.55\TldHacks\src
dotnet build -c Release
```
- AfterTargets="Build" 自动 copy 到 `D:\Steam\...\Mods\TldHacks.dll`
- 游戏运行时 copy 失败(MSB3026 文件锁定),需先退游戏
- v3.0.4 r4 dll size: 404480 bytes

---

## 发布包

桌面 **`TldHacks-v3.0.4-完整版.zip`** (82 MB):
```
zip 根
├── version.dll              ← MelonLoader 注入器
├── MelonLoader/             ← 框架 + IL2CPP 程序集
├── Mods/                    ← 29 项(前置框架 + TldHacks 合集 + bat)
└── 安装说明.txt
```

新手用法: 解压 → 整体拖到游戏根目录覆盖 → 进 Mods/ 双击 disable_integrated_mods.bat → 启游戏 Tab 打开菜单。

---

## 用户机器特殊问题

- `.bat` 文件关联损坏 (`HKEY_CLASSES_ROOT\batfile\shell\open\command` 默认值缺 `cmd.exe /c`),双击 .bat 不工作
  - **临时**: PowerShell 帮跑禁用列表(已做)
  - **永久** (管理员): `reg add "HKEY_CLASSES_ROOT\batfile\shell\open\command" /ve /d "\"%%SystemRoot%%\\System32\\cmd.exe\" /c \"%%1\" %%*" /f`

---

## 历史版本要点 (详见 `HANDOFF_archive.md`)

- **v3.0.3** 油灯光照范围 / Character 跳跃总开关 / 自定义背包负重重构
- **v3.0.2** 山洞黑暗根因 (GfxBoost+LightCull 删除) / 扭伤 tick 兜底 / 衣物 HP 快照恢复
- **v3.0.1** FPS 7 个 DynamicPatch 条件化 / 免费修理修方法名 / 免费烹饪扩展 / 物品名解析


---

# 历史版本归档


> 当前活跃版本见 `HANDOFF.md` (v3.0.4)
> 完整历史在 git log:
> ```
> cd D:\Github\tld--2.55
> git log --oneline HANDOFF.md
> git show <commit>:HANDOFF.md
> ```

## v3.0.3 — 2026-05-07 油灯光照 / 跳跃开关 / 背包重构

- **LampRangeMultiplier** [0.5, 5] slider — 山洞黑暗补强
  - `Patch_KeroseneLamp_LightRange` Postfix `KeroseneLampItem.Update`
  - DynamicPatch 条件挂载 (mult≈1 不挂)
- **JumpEnabled toggle** — Character tab Jump 总开关
  - `ModMain.cs`: `if (Settings.JumpEnabled) JumpHelper.Tick();`
- **背包重构** — `Encumber.Update` Postfix 直接覆盖 7 字段(放弃 buff 路径)
  - `m_MaxCarryCapacity / m_MaxCarryCapacityWhenExhausted / m_NoSprintCarryCapacity / m_NoWalkCarryCapacity / m_EncumberLow/Med/HighThreshold`
- **新增传送点**: 神秘湖·营地办公室 LakeRegion (1015.87, 25.91, 450.86)
- **disable_integrated_mods.bat 全面更新** — 38 项,分组结构化注释,可双击运行

## v3.0.2 — 2026-05-07 山洞黑暗 / 扭伤 / 衣物

### 山洞黑暗根因
**罪魁**: GfxBoost.dll + LightCull.dll (第三方 mod 不是 TldHacks)
- GfxBoost strip billboard shadow 破坏动态灯光渲染
- LightCull 距离禁用灯光转场残留
- DiagUnpatchAll + DiagPauseRuntime + 禁用 TldHacks 三种模式都复现 → 确认非 TldHacks
- 已加进 disable bat (.disabled 后缀)

### 扭伤 + 衣物
**关键发现**: IL2CPP `Prefix return false` 在有 Postfix 时可能被忽略
- `IntegratedPatches3.cs`: Postfix 从错误的 `Cure()` → 正确的 `SprainedAnkleEnd(0/1, 0)` / `SprainedWristEnd(0/1, 0)`
- `CheatsTick.TickNoFallDamage()`: 每 30 帧清扭伤 + 衣物 HP 快照恢复
- `Cheats.cs:812/818` attribute patch 已含 NoFallDamage(同 GodMode 表达式)

### DynamicPatch 条件化
- `Patch_GearDegrade`: `() => true` → `InfiniteDurability || QuickCraft || DecayState.HasNonDefaultMultiplier()`
- `Patch_ObjectAnim_Play`: 非默认 DoorSwing 时
- `Patch_TorchCondition` x2: 非默认 TorchMin/MaxCondition 时

## v3.0.1 — 2026-05-04 FPS 优化 / 免费修理 / 烹饪扩展 / 物品名解析

### FPS 优化 (DynamicPatch.cs) — 7 个 patch 条件化
| Patch | 条件 |
|---|---|
| vp_FPSCamera.LateUpdate | NoRecoil/SuperAccuracy/RecoilScale/NoAimSway/GunZoom |
| vp_FPSController.GetSlopeMultiplier | SpeedTweaksEnabled |
| PlayerMovement.Update | SpeedTweaksEnabled |
| KeroseneLampItem.Update | LampMute/LampColor/LampRange非原版 |
| KeroseneLampItem.ReduceFuel | LampFuelNoDrain 或 BurnMult≠1 |
| ComputeModifiedPickupRange | PickupRange≠1 |
| GameAudioManager.SetRTPCValue | 背包音量≠100% |

### 免费修理修方法名
- 错的: `HasToolRequired` (不存在)
- 正确: `RepairHasRequiredTool` + `RepairHasRequiredMaterial`

### 物品名解析 (Menu.cs Spawner)
- `AccessTools` 反射取 `GearItem.m_DisplayName` / `m_LocalizedDisplayName.GetLocalizedString()`
- 失败时 silently fallback 硬编码翻译

## v2.x 历史

详见 git log,关键里程碑:
- v2.7.99 — 移除 UT/Predator 整合 + CaffeinatedSodas 整合 + FullSwing IL2CPP 修复 + UI 重排
- v2.7.98 — SkipIntro 修 + 全标签两列 + 脚步静音
- v2.7.97 — CT 复刻 buff + MapClickTP 仿射变换 + UI 重排
- v2.7.95 — 17 mod 整合 + 搜索栏 + 油灯动画修
- v2.7.93 — UI 修 + ScurvyViewer + 锁图标
- v2.7.91 — 魔法子弹定稿 + 删 ESP/自瞄 UI
- v2.7.83 — 目录整合 + UI 重排 + 武器瞄准 + 拖动记忆
- v2.7.75 — DynamicPatch 性能方法论 + 大量整合
- v2.7.55 — 商人 + 美洲狮 CT 复刻


---

# 子项目状态

## ModSettingsQuickNav (搁置)


## 状态：搁置（功能未实现成功）

## 目标

在 ModSettings 面板的 mod 切换箭头右边加一个下拉按钮，点击后弹出完整 mod 列表供直接选择（不用一个个箭头翻）。

## 尝试过的方案及失败原因

### v2.0 — IMGUI 浮动按钮 + 下拉列表
- 在 `MelonEvents.OnGUI` 中始终绘制一个按钮和下拉列表
- **失败原因**：OnGUI 在 IL2CPP 中每帧调用 2-4 次，跨 managed↔IL2CPP 桥接开销大，120 个 mod 环境下明显卡顿

### v2.1 — Harmony patch OnEnable/OnDisable 生命周期
- 只在 ModSettings 面板打开时订阅 OnGUI
- **失败原因**：面板打开期间仍然每帧多次 OnGUI 调用，依然卡

### v3.1 — 按键触发 + 按需订阅 OnGUI
- 按 Tab 才订阅 OnGUI 显示列表，选完立即取消订阅
- **失败原因**：用户不想用按键触发，要可视化按钮

### v3.2~3.4 — NGUI 原生按钮/标签 + 点击检测
- 在 `Button_Increase` 右边创建 UILabel "▼" + BoxCollider
- 点击检测用 `UICamera.hoveredObject`
- **失败原因**：按钮不响应点击。推测：
  1. IL2CPP 中动态创建的 NGUI widget 可能需要额外初始化步骤（UIPanel depth、widget anchoring）
  2. `UICamera.hoveredObject` 依赖 NGUI 的 Raycast 系统，动态添加的 collider 可能不在正确的 UICamera 管辖范围内
  3. 也可能是 UILabel 的 pivot/depth 导致被其他 widget 遮挡

## 可能的后续方向

1. **研究 NGUI UICamera 射线机制**：确认动态添加的 collider 是否被 UICamera 检测到（可在 Update 里 log `UICamera.hoveredObject` 的值看看鼠标到底 hover 在什么上面）
2. **克隆已有按钮而不是从零创建**：`Object.Instantiate(increaseBtn.gameObject)` 然后只改 label text 和 onClick，保留完整的 NGUI 组件链
3. **放弃 NGUI，改用屏幕坐标硬算**：把 NGUI 标签的世界坐标转到屏幕坐标，在 OnGUI 的 Repaint 事件中画一个覆盖按钮（只画一个按钮而不是整个列表，开销可接受）
4. **修改 ModSettings.dll 源码**：如果能拿到原作者源码（GitHub zeobviouslyfakeacc/ModSettings），直接在 `CreateModSelector` 里加 UIPopupList

## 当前部署状态

- `ModSettingsQuickNav.dll` 在 Mods 目录中（v3.4，不卡但按钮无效）
- `ModSettingsQuickNav.dll.disabled`（旧 v2.1）
- 建议：将 `.dll` 也改为 `.dll.disabled` 避免无用加载

## 文件位置

- 源码：`D:\Github\tld--2.55\ModSettingsQuickNav\src\ModSettingsQuickNav.cs`
- 项目：`D:\Github\tld--2.55\ModSettingsQuickNav\src\ModSettingsQuickNav.csproj`
- ModSettings 反编译源码（未编译成功）：`D:\Github\tld--2.55\ModSettings_src\`


## MotionTrackerLite


## Summary

轻量版动物雷达 mod，替代原版 MotionTracker（因 IL2CPP bridge 开销导致游戏卡顿）。
功能完整对等，性能零影响（关闭时无任何回调/patch 挂载）。

## Architecture

```
MotionTrackerLite/
├── src/
│   ├── MotionTrackerLite.csproj   — .NET 6, 部署到 Mods/
│   ├── ModMain.cs                 — MelonMod 入口, 热键 toggle, 动态订阅/退订
│   ├── Tracker.cs                 — 核心: 实体字典 + 位置计算(每5帧) + IMGUI 绘制
│   ├── TrackerPatches.cs          — 7 个 Harmony patch, 动态挂卸
│   ├── TrackerConfig.cs           — JSON 配置读写 (Regex 解析, 无外部依赖)
│   └── Settings.cs                — ModSettings UI 面板 (中文)
└── icons/                         — 29 张 PNG 图标 (从原版 AssetBundle 提取)
```

## Key Design Decisions

| Decision | Rationale |
|----------|-----------|
| 动态 Patch/Unpatch | 关闭时零 IL2CPP bridge 开销 |
| 集中式 Tick (每5帧) | 代替每实体独立 MonoBehaviour.Update |
| MelonEvents Subscribe/Unsubscribe | 关闭时无 OnUpdate/OnGUI 回调 |
| 纯 IMGUI + PNG 贴图 | 无 AssetBundle, 运行时加载 PNG |
| AccessTools.TypeByName | BeachcombingSpawner 编译时不可见 |
| Struct TrackedEntity | 避免 GC 压力, 字典批量遍历友好 |

## Harmony Patches (7 total, dynamic)

1. `BaseAi.Start` → 注册动物 (区分 9 种)
2. `BaseAi.EnterDead` → 注销死亡动物
3. `GearItem.ManualUpdate` → 注册/注销地面物品 (箭/煤/生鱼)
4. `Harvestable.Start` → 注册盐矿
5. `Container.Start` → 注册失物招领箱
6. `DynamicDecalsManager.TrySpawnDecalObject` → 注册喷漆标记
7. `BeachcombingSpawner.Start` → 注册海滩拾取物

## Tracked Entities

- **Animals (9)**: Wolf, Timberwolf, Bear, Moose, Cougar, Stag, Doe, Rabbit, Ptarmigan
- **Gear (3)**: Arrows, Coal, Raw Fish
- **Structures (3)**: Lost & Found, Salt Deposit, Beach Loot
- **Spraypaint**: Direction markers

## Icons

从原版 MotionTracker.dll 内嵌 AssetBundle 用 UnityPy 提取，存放 `icons/` 目录。
部署时复制到 `Mods/MotionTrackerLite_icons/`。

Icon 映射关系见 `Tracker.cs` 的 `AnimalIconMap`, `GearIconMap`, `StructureIconMap` 字典。

## Configuration

- JSON 文件: `Mods/MotionTrackerLite.json` (自动生成)
- ModSettings 面板: 游戏内 Options → Mod Settings → MotionTrackerLite
- 全部设置: 热键, 仅室外, 探测范围, 缩放, 透明度, 各实体显示开关, 图标大小

## Build & Deploy

```powershell
cd D:\Github\tld--2.55\MotionTrackerLite\src
dotnet build -c Release
# 自动部署 DLL 到 D:\Steam\steamapps\common\TheLongDark\Mods\
```

Icons 需手动部署 (或首次运行时):
```powershell
Copy-Item -Recurse D:\Github\tld--2.55\MotionTrackerLite\icons D:\Steam\steamapps\common\TheLongDark\Mods\MotionTrackerLite_icons
```

## Known Limitations

- FlockController (乌鸦) 在 TLD 2.55 中不存在，已移除
- 原版图标分辨率有限 (64×64 ~ 128×128)，来源是游戏内资源
- BeachcombingSpawner 需运行时 TypeByName 查找，某些版本可能无此类型

## Performance Profile

- **Radar OFF**: 0 callbacks, 0 patches, 0 allocations per frame
- **Radar ON**: 1× OnUpdate (every 5th frame: dictionary iteration), 1× OnGUI (draw calls)
- Typical entity count: 20-50 → negligible CPU cost
