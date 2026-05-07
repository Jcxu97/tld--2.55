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
