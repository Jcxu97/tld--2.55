# TldHacks 历史交接归档

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
