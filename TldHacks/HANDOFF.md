# HANDOFF — TldHacks
> Last updated: 2026-05-13 02:30

## 当前目标
v6.6 性能微优化已完成（GC 压制 + Camera patch 内部优化）

## 已完成（本轮）
- GC 压制三件套：SustainedLowLatency + 暂停时 Gen0 + 场景切换 Gen2
- vp_FPSCamera Prefix→Postfix 条件缓存（消除重复 WeaponCache 调用）
- vp_FPSWeapon Postfix 冗余检查移除
- 尝试 NativeDetour 方案（MonoMod.RuntimeDetour）→ 证实不可用（环境崩溃）
- 尝试 TickOverride 方案 → 收益不明显，已删除

## 下一步
- 无紧急性能问题
- 任务 A/B（NOP 枪械 + ImprovedFlasks 集成）待实施

## 未决问题
- 无

## 关键文件
- `src/ModMain.cs` — GC 压制代码（line 53 + OnUpdate + OnSceneWasInitialized）
- `src/CheatsPatches.cs` — Camera/Weapon patch 条件缓存优化
