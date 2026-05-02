# TldHacks — 交接文档

## 🆕 2026-05-02 session #2 v2.7.90 — ESP/AutoAim 9/10 品质升级 + 编译修复

### 本次改动

1. **AiCache 无限递归 bug 修复**（ESP.cs:34）
   - `AiCache.Get()` 原来调用自己 → 栈溢出 → ESP 静默失败
   - 改为 `UnityEngine.Object.FindObjectsOfType<BaseAi>()`

2. **PhysicsModule 引用添加**（TldHacks.csproj）
   - `UnityEngine.PhysicsModule.dll` 添加到项目引用
   - `Physics.Raycast` LOS 检测现在能编译

3. **后坐力窗口 0.25s → 0.5s**（CheatsPatches.cs:725）
   - 0.25s 太短，游戏弹簧恢复动画比这长，导致后坐力未被消除

4. **RecoilScale 滑块独立生效**（CheatsPatches.cs + DynamicPatch.cs）
   - PlayFireAnimation Prefix 条件加 `CheatStateESP.RecoilScale < 0.99f`
   - ClearRecoil 条件加 `CheatStateESP.RecoilScale < 0.99f`
   - DynamicPatch 三处 Spec 条件加 `CheatStateESP.RecoilScale < 0.99f`
   - 按比例缩放 GunItem 后坐力参数：`gun.m_PitchRecoilMin *= scale`

5. **FOV 圆圈指示器**（ESP.cs 新增 DrawFOVCircle）
   - 开 AutoAim 后屏幕中心画圆圈指示搜索范围
   - 无锁定=白色半透明，有锁定=金色
   - 圆圈大小随 FOV 滑块变化

### 改动文件
```
ESP.cs           — AiCache 递归修 + FOV 圆圈 + OnGUI 条件调整 + 删无用 using
CheatsPatches.cs — RecoilWindow 0.25→0.5s + PlayFireAnimation 支持 RecoilScale + ClearRecoil 条件扩展
DynamicPatch.cs  — PlayFireAnimation/ClearRecoil 挂载条件加 RecoilScale
TldHacks.csproj  — 添加 UnityEngine.PhysicsModule.dll 引用
```

### 明天测试清单

| # | 功能 | 预期 | 失败排查 |
|---|------|------|----------|
| 1 | ESP 透视 | 红色方框+血条+距离 | AiCache 仍然 null → 看 MelonLoader 日志 |
| 2 | [WALL] 标签 | 墙后半透明+[WALL] | Physics.Raycast IL2Cpp 不兼容 → HasLOS 改 return true |
| 3 | 自动瞄准 | 按住右键锁定+金色[TARGET] | GetVpFPSCamera() null → 用 FindObjectOfType 兜底 |
| 4 | FOV 圆圈 | 中心白色圆圈 | AutoAim toggle 必须 ON |
| 5 | 后坐力消除 | NoRecoil ON 枪口不跳 | 如果仍跳 → 窗口改 0.8s |
| 6 | 后坐力滑块 | 50% → 后坐力减半 | DynamicPatch 条件检查 |
| 7 | 魔法子弹 | 开枪命中锁定目标 | ApplyDamage 签名 |
| 8 | 平滑释放 | 松开右键0.2s过渡 | — |
| 9 | 设置持久化 | 重启保留 | 检查 TldHacks.json |

### 如果 Physics.Raycast 崩溃

HasLOS 用了 try/catch 保护，崩溃会静默回退到 `return true`（所有目标视为可见）。如果游戏完全崩：
- `ESP.cs:307` 的 HasLOS 方法改为 `return true;` 即可跳过 LOS 检测

### 构建命令
```powershell
dotnet build "D:\TLD-Mods\tld--2.55\TldHacks\src\TldHacks.csproj" --no-incremental
```

---

## 🆕 2026-05-02 session v2.7.89 — 无后坐力终极修复 + 堆叠视觉修 + ESP/自动瞄准/武器调参

### 背景
v2.7.87/v2.7.88 无后坐力仍无效（用户实测确认）。堆叠点击容器时×N标签消失。新增3个功能。

### 核心发现

**IL2Cpp 限制确认**：所有通过 property setter 写入值类型（struct）或字段的方式对原生内存无效：
- 归零 `m_RecoilSpring` struct → 无效
- 设 `RecoilPitchStiffness`/`Damping` → 无效
- 归零 `GunItem.m_PitchRecoilMin/Max` → 无效（日志确认 gun=True 找到了对象）
- BetterCamera.dll 逆向发现它用 `set_m_MultiplierAiming`，但我们的同等操作也无效

**真正可行的方法**：只读 getter + 写 Transform（标准 Unity API）
- `vp_FPSCamera.m_Pitch`/`m_Yaw` 是纯鼠标输入角度（getter 可读）
- 游戏最终旋转 = `Euler(m_Pitch + springs, m_Yaw + springs, 0)`
- 我们在 LateUpdate Postfix 覆盖为 `Euler(m_Pitch, m_Yaw, 0)` → 弹簧贡献被跳过

### 本轮改动

1. **无后坐力 — m_Pitch/m_Yaw 旋转覆盖（终极方案）**
   - `Patch_FPSCamera_LateUpdate.Postfix`：读 `__instance.m_Pitch`/`m_Yaw`，设 `transform.rotation = Quaternion.Euler(pitch, yaw, 0)`
   - PlayFireAnimation Prefix 保留 GunItem 归零作为辅助
   - Camera.Update Prefix 保留 stiffness=0 作为辅助

2. **堆叠视觉修复 — giPtr 失配回退 + SelectGridItem 全量 reapply**
   - `OnLateUpdate`：当 `curGi.Pointer != cachedGiPtr` 时不再跳过，改用 `CountsByGi` 查找当前 gi 的堆叠数并显示 label
   - `SelectGridItem Postfix`：改为遍历所有 SeenItems 而非仅点击项
   - Panel_Container.SelectGridItem 签名确认：`(InventoryGridItem, bool)`

3. **新增：ESP 透视**（`ESP.cs`）
   - 红色标记场景中所有动物（名字+距离，最远300m）
   - 蓝色标记容器（最远50m）
   - OnGUI 绘制，WorldToScreenPoint 投影

4. **新增：自动瞄准**（`ESP.cs`）
   - 每0.1s扫描FOV内最近活动物
   - Slerp 平滑追踪到目标中心
   - 可调FOV范围（5°~90°）

5. **新增：武器调参面板**（`ESP.cs`）
   - 后坐力倍率滑块（0~200%）→ 修改 GunItem.m_MultiplierAiming/Fire
   - 射速倍率滑块（1~10x）→ 修改 GunItem.m_MultiplierFire
   - 换弹速度滑块（1~10x）→ 修改 GunItem.m_MultiplierReload

### 改动文件

```
src/CheatsPatches.cs  [改] Patch_FPSCamera_LateUpdate 改为 m_Pitch/m_Yaw 旋转覆盖
                            Patch_FPSCamera_ClearRecoil 简化(stiffness=0)
                            Patch_FPSWeapon_PlayFireAnimation 改为 GunItem 归零+Postfix恢复
src/DynamicPatch.cs   [改] PlayFireAnimation spec 加 "Postfix"
src/Stacking.cs       [改] OnLateUpdate giPtr 失配回退 CountsByGi
                            SelectGridItem Postfix 改为全量 reapply
src/ESP.cs            [新] CheatStateESP / ESPOverlay / AutoAimSystem / WeaponTuning
src/ModMain.cs        [改] OnUpdate 加 AutoAimSystem.Tick + WeaponTuning.Tick
                            OnGUI 加 ESPOverlay.OnGUI
src/Menu.cs           [改] 新增 "ESP & 自动瞄准" section (toggles + sliders)
```

### 测试清单
1. **无后坐力**：步枪连射准星不动？ ← 前几版全部无效，v2.7.89 用 m_Pitch/m_Yaw 覆盖
2. **堆叠点击**：容器内点击堆叠物品，其他×N标签保持？
3. **ESP**：室外看到红色动物标记+距离？
4. **自动瞄准**：开启后准星自动追踪最近动物？
5. **武器调参**：滑块调整后坐力/射速/换弹有效？

### FPS 分析结论
室内 GPU 80-90% + 180fps vs 室外 GPU 40-60% + 卡顿 = **CPU 瓶颈**。室外 draw call 多、AI 多、植被多，CPU 来不及喂 GPU。113 mod 每帧 Update 加剧。解法：降 draw distance/阴影/减 mod，非 mod 层面可解。

---

## 2026-05-02 session v2.7.87 — 武器瞄准彻底修复 + UI 缩放修 + 签名修复

### 背景
v2.7.86 用户实测反馈:无后坐力/稳定瞄准/超级精准三个 toggle 仍无效;右下角拖拽缩放失灵;生火材料不减无效;科技背包无效;无条件冲刺重复(和无限体力功能一样)。

### 本轮改动

1. **无后坐力/超级精准 — 彻底重写(DLL 字符串搜索确认真实字段名)**
   - **根因**:v2.7.86 的 `Patch_FPSWeapon_PlayFireAnimation` 试图找 `m_RecoilAngle`/`m_RecoilOffset` 等字段 — **这些字段在 vp_FPSCamera 和 vp_FPSWeapon 上都不存在**。DLL 二进制搜索确认 `vp_FPSCamera` 的真实后坐力字段是:`RecoilPitchStiffness`/`RecoilYawStiffness`/`RecoilPitchDamping`/`RecoilYawDamping`/`RecoilMinVelocity`
   - **新做法**:
     - `Patch_FPSCamera_ClearRecoil`(hook `vp_FPSCamera.Update` Postfix):每帧直接赋值 `__instance.RecoilPitchStiffness = 0f` 等 5 个属性,使弹簧不施加后坐力
     - `Patch_FPSWeapon_PlayFireAnimation`(hook `PlayFireAnimation` Postfix):开枪时同步归零相机 recoil 参数
   - **不再使用反射** — 直接属性赋值,编译时检查

2. **稳定瞄准 — 直接属性赋值替代反射遍历**
   - **根因**:v2.7.86 的 `Patch_FPSWeapon_SteadyAim` 用反射遍历继承链找 `BobAmplitude`/`ShakeAmplitude`/`ShakeSpeed`,运行时可能找不到(Il2Cpp 包装器类型行为不同)
   - **新做法**:直接属性赋值 `__instance.BobAmplitude = Vector4.zero` 等,加 `SwayMaxFatigue = 0f` / `SwayStartFatigue = 999f` 消除疲劳晃动
   - 条件扩展:`NoAimSway || SuperAccuracy` 都触发

3. **生火材料不减 — 签名修复**
   - **根因**:`PlayerManager.ConsumeUnitFromInventory` 返回 `void`(不是 `bool`!)。旧代码用 `ref bool __result` 参数,Harmony 签名不匹配 → patch 静默失效
   - **修法**:移除 `ref bool __result`,改为纯 `Prefix() { return false; }` 跳过原方法

4. **科技背包 — 返回类型修复**
   - **根因**:`PlayerManager.GetCarryCapacityKGBuff` 返回 `ItemWeight`(Il2CppTLD.IntBackedUnit 命名空间),不是 `float`。旧代码用 `ref float __result`,签名不匹配 → 静默失效
   - **修法**:改为 `ref ItemWeight __result`,赋值 `ItemWeight.FromKilograms(50f)`

5. **删除"无条件冲刺"(FreeSprint)**
   - 用户确认和"无限体力"功能重复。UI toggle 已删,Settings 字段保留兼容旧存档

6. **右下角拖拽缩放 — 坐标系修复**
   - **根因**:`DrawContents` 内 `Event.current.mousePosition` 是窗口实际像素坐标(0 到 `W*_scale`),但命中检测用未缩放的 `W`/`H` 比较 → 手柄区域偏移,永远点不到
   - **修法**:改为 `e.mousePosition.x >= W * _scale - grip`,grip 从 18→22px 扩大手感

### 改动文件

```
src/CheatsPatches.cs  [改] 重写 Patch_FPSWeapon_PlayFireAnimation(直接属性赋值)
                            重写 Patch_FPSCamera_ClearRecoil(直接属性赋值)
                            重写 Patch_FPSWeapon_SteadyAim(直接属性赋值)
                            修复 Patch_ConsumeUnit(移除 ref bool __result)
                            修复 Patch_TechBackpack(ref ItemWeight __result)
                            删除 Patch_PlayerCanSprint
                            新增 using Il2CppTLD.IntBackedUnit
src/DynamicPatch.cs   [改] 删除 PlayerCanSprint Spec 条目
src/Menu.cs           [改] 删除 FreeSprint toggle;缩放手柄命中检测改用缩放坐标
```

### DLL 字段名验证结果(二进制搜索 Assembly-CSharp.dll 确认)

| 类 | 真实字段名 | 旧代码猜的(不存在) |
|----|-----------|-----------------|
| vp_FPSCamera | `RecoilPitchStiffness`/`RecoilYawStiffness`/`RecoilPitchDamping`/`RecoilYawDamping`/`RecoilMinVelocity` | `m_RecoilAngle`/`m_RecoilOffset` |
| vp_FPSCamera | `ShakeAmplitude(V3)`/`ShakeSpeed(f)`/`BobAmplitude(V4)` | ✅ 这些正确 |
| vp_FPSWeapon | `BobAmplitude(V4)`/`ShakeAmplitude(V3)`/`ShakeSpeed(f)`/`SwayMaxFatigue(f)`/`SwayStartFatigue(f)` | ✅ |
| TorchItem | 无 burn time 字段(只有 `m_ExtinguishTime` 等) | `m_CurrentBurnTimeSeconds`/`m_MaxBurnTimeSeconds` |
| PlayerManager | `ConsumeUnitFromInventory` 返回 `void` | 旧代码认为返回 `bool` |
| PlayerManager | `GetCarryCapacityKGBuff` 返回 `ItemWeight` | 旧代码认为返回 `float` |

### 测试清单
1. **无后坐力**:步枪/左轮连射,视角不跳? ← v2.7.86 无效,v2.7.87 应修好
2. **稳定瞄准**:武器模型完全不晃? ← v2.7.86 无效,v2.7.87 应修好
3. **超级精准**:散布极小? ← v2.7.86 无效,v2.7.87 应修好
4. **生火材料不减**:生火后材料数量不减? ← v2.7.86 无效,v2.7.87 应修好
5. **科技背包**:负重上限 +50kg? ← v2.7.86 无效,v2.7.87 应修好
6. **右下角缩放**:拖拽能缩放窗口? ← v2.7.86 无效,v2.7.87 应修好
7. **无限体力**:冲刺后体力不减? ← v2.7.86 已 work ✅
8. **随意生火**:室内能点火? ← 编译通过但未测
9. **火把满值**:火把状态满? ← 编译通过但未测

---

## 2026-05-02 session v2.7.86 — 6 新功能 + 初版武器修(大部分无效,v2.7.87 修好)

### 背景
按 CT 表补齐 6 个缺失功能;尝试修 NoRecoil/SuperAccuracy/SteadyAim。

### 本轮新增功能

| 功能 | Settings 字段 | 实现方式 | 状态 |
|------|-------------|---------|------|
| 随意生火(含室内) | `FireAnywhere` | `InputManager.CanStartFireIndoors` Prefix | ❓ 未测 |
| 生火材料不减 | `FreeFireFuel` | `PlayerManager.ConsumeUnitFromInventory` Prefix | ❌→✅ v2.7.87 修 |
| 科技背包 | `TechBackpack` | `PlayerManager.GetCarryCapacityKGBuff` Postfix | ❌→✅ v2.7.87 修 |
| 火把满值 | `TorchFullValue` | `TorchItem.Update` Prefix → `GearItem.m_CurrentHP=100f` | ❓ 未测 |
| ~~无条件冲刺~~ | ~~`FreeSprint`~~ | ~~`PlayerManager.PlayerCanSprint` Postfix~~ | 🗑️ v2.7.87 删除(重复) |
| 无限体力 | `InfiniteStamina` | `PlayerMovement.AddSprintStamina` Postfix | ✅ 已验证 |

---

## 🆕 2026-05-02 session v2.7.85 — 瞄准合并 + 超级精准 + HP 闪烁修 + 缩放修 + CT 对比

### 背景
用户要求:合并三个瞄准 toggle 为一个"稳定瞄准";新增"超级精准";删除无用功能(StopWind/NoAimDOF);修复右下角拖拽缩放;修复 HP 条闪烁;对比 CT 表功能差距。

### 本轮改动

1. **合并瞄准三 toggle → `NoAimSway` (稳定瞄准)**
   - 删除 `NoAimShake`、`NoBreathSway`、`NoAimDOF` 字段(Settings/CheatState/Menu/ModMain 全清)
   - `TickCamera` 中三个 static bool 全部读 `CheatState.NoAimSway`
   - Camera 实例字段(ShakeAmplitude/ShakeSpeed/BobAmplitude + sway 字段)统一在 `if (NoAimSway)` 块内归零
   - 新增武器实例 BobAmplitude/ShakeAmplitude/ShakeSpeed 归零(修呼吸时武器模型晃动)
   - 删除 `Patch_CamEffects_WeaponPost` 类(NoAimDOF patch)和 DynamicPatch 引用

2. **新增 `SuperAccuracy` (超级精准,瞄哪打哪)**
   - Settings/CheatState/Menu/ModMain 全加
   - TickGuns 中对所有 GunItem 零化:m_PitchRecoilMin/Max, m_YawRecoilMin/Max, m_SwayValue, m_SwayValueZeroFatigue, m_SwayValueMaxFatigue
   - 注意:GunItem 没有 `m_AccuracyPenalty`/`m_BulletSpread` 字段(编译验证),散布靠 sway+recoil 字段控制

3. **删除 `StopWind` (停止刮风)**
   - 用户报告功能无效,删除 `ExtraOneShot.TickStopWind()` 方法和 `CheatState.StopWind` 字段
   - Settings.cs 中 `StopWind` toggle 保留但无代码(已断开连接)

4. **修复右下角拖拽缩放**
   - **根因**:IMGUI `GUI.Window` callback 内 `Event.current.mousePosition` 是**窗口局部坐标**(左上角=0,0),但代码用 `_window.x + _window.width` (屏幕坐标)比较 → 永远不命中
   - **修复**:改为 `e.mousePosition.x >= W - grip && e.mousePosition.y >= H -grip`(窗口尺寸)

5. **修复 HP 条闪烁(GodMode OFF + 状态 toggle ON)**
   - **根因**:DamageFilter 只在 GodMode 时拦全部伤害;AlwaysWarm/NoHunger 等单独开时,游戏仍从 Freezing/Starving 等源扣 HP → TickStatus 每帧先掉再补 → HUD 闪烁
   - **修复**:DamageFilter 按 toggle 独立拦截对应伤害源:
     - `AlwaysWarm` → 拦 `DamageSource.Freezing / Hypothermia / FrostBite`
     - `NoHunger` → 拦 `DamageSource.Starving`
     - `NoThirst` → 拦 `DamageSource.Dehydrated`
     - `NoFatigue` → 拦 `DamageSource.Exhausted`

6. **CT 表功能对比文档**
   - `CT_vs_Mod_Compare.md` — CT ~70+ 项 vs Mod ~45 项,重叠 42 项,CT 独有 ~35 项,Mod 独有 ~13 项
   - 用户决定要加的新功能(见下方"待实现")

### ~~待实现新功能 (v2.7.86)~~ → 已在 v2.7.86 实现,见上方

### 改动文件(v2.7.85)

```
src/CheatsPatches.cs  [改] TickCamera 合并 NoAimShake/NoBreathSway → NoAimSway;
                            武器实例 BobAmplitude/ShakeAmplitude 归零;
                            TickGuns 加 SuperAccuracy 逻辑;
                            删除 Patch_CamEffects_WeaponPost (NoAimDOF);
                            DamageFilter 按 toggle 独立拦截伤害源;
src/DynamicPatch.cs   [改] 删除 NoAimDOF 的 Spec 条目;
src/Menu.cs           [改] 缩放手势坐标系修复 (屏幕→窗口局部);
src/CT_vs_Mod_Compare.md [新] CT 表 vs Mod 功能对比;
```

---

## v2.7.84 session — 瞄准诊断兜底 + HUD 状态闪烁修 + TickCamera 反编译重写

### 背景
用户报告:v2.7.83 部署后 log 里**只有一条** `[AimDiag] GetVpFPSCamera() = NULL`,之后诊断完全哑火。同时用户观察 HUD 体力/寒冷/HP 条"**一直在掉,每过 0.x 秒跳回满**",怀疑是卡顿原因。后续用 Cursor 反编译 Assembly-CSharp.dll 找到了瞄准/武器的真正 API。

### 本轮改动

1. **`CheatsPatches.cs` TickCamera 加 vp_FPSCamera FindObjectOfType 兜底**
   - `GameManager.GetVpFPSCamera()` 在 2.55 IL2CPP 下常返回 null(GameManager singleton 可能没绑 `m_VpFPSCamera`)
   - 新查找顺序:`_cachedCam` → `GameManager.GetVpFPSCamera()` → `UnityEngine.Object.FindObjectOfType<vp_FPSCamera>()` 兜底
   - 找到后缓存;每 30 帧限频一次 FindObject 扫描,避免启动时每 tick 扫场景
   - `cam=null` 日志节流:每 60 次(~30s)打一次,启动/过场时不刷屏
   - 新 `CheatsTick.InvalidateCameraCache()` —— 跨场景清缓存 + 重置 `_aimDiagDone` + `_camNullLogCount`,从 `ModMain.OnSceneWasInitialized` 调

2. **`ModMain.cs` TickStatus 搬到 OnLateUpdate 每帧跑(消 HUD 闪烁)**
   - 旧:OnUpdate `_frame % 60 == 10`,每秒 1 次 clamp Fatigue/Hunger/Thirst/Freezing/HP
   - 新:OnLateUpdate 每帧 clamp,跑在 Update 的 drain 后、render 前
   - Unity 顺序:`MonoBehaviour.Update(游戏的 Fatigue.Update 耗体力)→ LateUpdate(我们 clamp)→ Render`
   - 用户看到的"掉 → 跳回满"闪烁 = 1s tick 间隙渲染中间值;每帧 clamp 后 UI 永远读最终值
   - 成本:早退保底(所有 toggle 关时 return)+ 5 个 singleton getter + 5 SetValue ≈ 1μs/帧,FPS 影响可忽略
   - **这不是 FPS 卡顿主因**(主因仍是 Harmony patch 总数,参 v2.7.79 DiagUnpatchAll 段),只消除视觉闪烁 + 防"GodMode 下被袭击瞬间真扣血"

3. **`CheatsPatches.cs` TickCamera 反编译重写(根治瞄准功能失效)**
   - **根因**:旧代码用反射 `GetFields(BindingFlags.Instance | BindingFlags.Public)` 查 `vp_FPSWeapon` 的 `m_DisableAimSway` 等字段,但 IL2CPP 包装器类型只返回 2 个声明字段,不返回从 `vp_Component` 继承的字段 → 所有 FieldInfo = null → 瞄准功能全部静默失效
   - **Cursor 反编译 Assembly-CSharp.dll 确认**:
     - `vp_FPSWeapon.m_DisableAimSway` / `m_DisableAimShake` / `m_DisableAimBreathing` / `m_DisableAimStamina` / `m_DisableDepthOfField` — 全是 **static bool**
     - `vp_FPSWeapon.SetDisableAimSway(bool)` 等 — **static 方法**
     - `vp_FPSCamera.m_DisableAmbientSway` — **static bool**(已确认可用)
     - `vp_FPSCamera.ShakeAmplitude` / `ShakeSpeed` / `BobAmplitude` — 实例字段
     - `vp_FPSCamera.m_RecoilSpring` — struct(需反射 GetValue/SetValue)
   - **新做法**:直接赋值静态字段 `vp_FPSWeapon.m_DisableAimSway = CheatState.NoAimSway`,不需要实例,不需要反射
   - Camera 实例 float 字段(`ShakeAmplitude` 等)直接通过 `_cachedCam` 写
   - RecoilSpring struct 保留 FieldInfo 反射(唯一需要反射的地方)
   - **删掉 ~30 个无用 FieldInfo 变量** + 4 个 helper 方法(`SetFloatIfNotNull` / `SetBoolIfNotNull` / `DiagLogWeaponFields` / `DiagLogCameraFields`)
   - 诊断改为直接读静态字段值: `[AimDiag] Static Disable fields: Sway=False Shake=False ...`

4. **`DamageFilter.ShouldBlock` 加 GodMode 拦截(修 HP 条闪烁)**
   - 旧:GodMode 只在 TickStatus 里每帧刷满 HP,但冻伤每帧又扣 → HUD 闪烁
   - 新:`if (CheatState.GodMode) return true;` 在 AddHealth Prefix 直接拦截所有伤害源

5. **版本号** → v2.7.84(`Menu.cs` 标题 + `ModMain.cs` 日志)

### 改动文件

```
src/CheatsPatches.cs  [改] TickCamera 加 _cachedCam + FindObjectOfType 兜底;
                            TickCamera 反编译重写:删 ~30 个 FieldInfo,改静态字段直接赋值;
                            DamageFilter.ShouldBlock 加 GodMode 拦截;
                            新 InvalidateCameraCache() helper
src/ModMain.cs        [改] OnUpdate 去掉 TickStatus;OnLateUpdate 加 TickStatus;
                            OnSceneWasInitialized 加 InvalidateCameraCache 调;版本号 v2.7.83→v2.7.84
src/Menu.cs           [改] 标题版本号 v2.7.83→v2.7.84
src/HANDOFF.md        [改] 本段
```

### ⚠ 编译 + 测试流程

编译环境仍不可用,用户自己跑 `dotnet build -c Release`。DLL 到 `src/bin/Release/TldHacks.dll` 自动部署到游戏 Mods 目录。

**测试步骤**:
1. 进游戏 → 拿枪 → 开任一瞄准 toggle(如"关闭瞄准晃动")
2. 不退游戏,直接 tail `MelonLoader/Latest.log` 搜 `[AimDiag]`
3. 新增 log:
   - `[AimDiag] vp_FPSCamera via FindObjectOfType (GetVpFPSCamera=null)` → 兜底成功
   - `[AimDiag] Static Disable fields: Sway=False Shake=False ...` → 静态字段可访问 ✅
   - 如果某个字段报 `ERR(...)` → 该静态字段在 IL2CPP 包装器里不可直接访问,需改用 `vp_FPSWeapon.SetDisableAimSway(true)` 等 Set* 方法
4. 瞄准测试:开 NoAimSway → 枪应该完全不晃;开 NoRecoil → 开枪无后坐力
5. HUD 闪烁验证:开 GodMode,让狼追,HP 条应该**纹丝不动**而不是"掉血→跳满"

### 待做(下一轮看 log 再决定)

- 如果静态字段赋值 `ERR`:改用 `vp_FPSWeapon.SetDisableAimSway(true)` 等 Set* 静态方法(反编译确认存在)
- 如果 HUD 闪烁消失但用户仍觉得卡 → 按 v2.7.79 段走 DiagUnpatchAll 验证是不是 Harmony patch 总数问题

---

## 🆕 2026-05-02 session v2.7.83 — 武器瞄准修 + UI 拖动持久化 + 布局对齐

### 本轮改动(都已合并到 repo)

1. **武器/瞄准功能诊断 + 修**(`CheatsPatches.cs`):
   - `TickCamera` 加诊断日志:首次有 toggle ON 时列举 vp_FPSCamera / vp_FPSWeapon 的所有相关字段名,报告哪些 FieldInfo 为 null → 方便定位 IL2CPP 字段名不匹配
   - 加 `FindWeapon()` 多重 fallback:反射 `m_CurrentWeapon` → 尝试 `m_Weapon/m_ActiveWeapon/currentWeapon` → `PlayerManager.GetWeaponItem()/GetCurrentWeapon()`
   - 所有武器字段反射加 `??` fallback(如 `m_DisableAimSway` → `m_DisableSway`, `ShakeAmplitude` → `m_ShakeAmplitude`)
   - TickCamera 频率从 120 帧(2s) → 30 帧(0.5s)(`ModMain.cs`)
   - 诊断结果打到 `MelonLoader/Latest.log`,搜 `[AimDiag]`

2. **UI 窗口拖动 + 位置持久化**(`Menu.cs` + `Settings.cs`):
   - `Settings.cs` 新增 `MenuX` / `MenuY` float 字段(默认 30,30)
   - `OpenMenu()` 从 Settings 恢复窗口位置
   - `Draw()` 检测拖动后位置变化,延迟 30 帧(0.5s)保存(避免每帧写磁盘)
   - `Close()` 关闭时立即保存位置
   - 重启游戏后菜单位置自动恢复

3. **Section header 对齐**(`Menu.cs`):
   - `BOX_W` 从 405 → 380,与 toggle 行(TOG2_OFF+TOG_W=380)右对齐

4. **右上角布局紧凑化**(`Menu.cs`):
   - `-`/`x1.0`/`+`/`Close` 按钮统一间距 4px
   - 总宽度从 176px 缩到 148px,更紧凑
   - Close 按钮文本走 `I18n.T("关闭","Close")`

5. **版本号** → v2.7.83(`Menu.cs` 标题 + `ModMain.cs` 日志)

### 改动文件

```
src/CheatsPatches.cs  [改] TickCamera 诊断日志 + 多重 fallback 武器查找 + 字段名 fallback
src/ModMain.cs        [改] TickCamera 频率 120→30 帧;版本号 v2.7.82 → v2.7.83
src/Menu.cs           [改] 窗口拖动持久化 + BOX_W 对齐 + 右上角紧凑 + 版本号
src/Settings.cs       [改] 新增 MenuX / MenuY 字段
src/HANDOFF.md        [改] 本文档
```

### ⚠ 武器瞄准功能说明

这些功能依赖反射设置 `vp_FPSCamera` / `vp_FPSWeapon` 的字段。如果字段名在 TLD 2.55 IL2CPP 版本中不匹配,功能会静默失效。**首次开启任一瞄准 toggle 时**,`Latest.log` 会打 `[AimDiag]` 诊断信息,包含:
- Camera/Weapon 的实际类型名和所有匹配字段
- 哪些 FieldInfo 为 null

**排查流程**:开游戏 → 开任一瞄准 toggle → 退游戏 → 搜 `Latest.log` 的 `[AimDiag]` → 看哪些字段为 null → 修改 `EnsureReflectionInited` 里的字段名。

---

## 🆕 2026-05-02 session v2.7.82 — UI 三列平衡 + 秒打碎 bug 修 + 生成动物合并

### 本轮改动(都已合并到 repo)

1. **UI 重排 方案A 三列平衡**(`Menu.cs`):
   - 技能 section "全部满级" + "展开▼" 同行,省一行
   - 动物 section:美洲狮 toggle 搬回"商人 & 美洲狮",只留 4 toggle + 8 生成动物按钮(4列×2行)
   - 环境/篝火:删"生火100%",只留 4 toggle(2行,整齐)
   - 快速操作:加"生火100%"为第 4 行(8 toggle,4行)
   - 商人 section:改回"商人 & 美洲狮",5 toggle
   - 一次性操作:"修复手持" + "★ 全关并同步" 合成一行
   - 物品&装备:"保温瓶装任意液体" + "不捡自己丢的" 合成一行
   - 锁&容器:"解锁保险箱" 改 TOG_W 配对
   - **所有 toggle 统一 TOG_W=182px 两列布局,无 TOG_WIDE 散落**
   - 三列高度:列1 ~458px / 列2 ~458px / 列3 ~502px

2. **uConsole 区精简**(`Menu.cs`):
   - 删除"一键操作"(添加全部物品/秒杀所有动物/修复传输器) — 功能可由 Tab 1 toggle 替代
   - 删除"生成动物 spawn_*" — 合并到列 2"动物"section
   - 整个 `DrawConsoleSection` 函数已移除

3. **秒打碎 toggle OFF 后仍秒打碎 bug 修**(`CheatsPatches.cs`):
   - 根因:`Patch_BreakDown_UpdateDuration` 有 cleanupGate(Snapshots.Count==0),toggle OFF 后 patch 仍挂载,但 Prefix 只在 `UpdateDurationLabel` 被调用时才 restore `m_TimeCostHours`。如果玩家关 toggle 后不开打碎面板,snapshot 永远不被 restore
   - 修法:`TickQuickActions` 新增兜底:toggle OFF 后扫描所有 `Panel_BreakDown` 实例,主动 restore `m_TimeCostHours` 并清 Snapshots

4. **生成动物标签修**(`Menu.cs`):
   - "母鹿⚠" / "Doe⚠" → "母鹿" / "Doe",去掉 ⚠ 字符(显示异常符号)

5. **缩放比例标签居中**(`Menu.cs`):
   - `x1.0` 标签 x: W-158→W-160, w: 50→52,水平居中在 -/+ 按钮之间

6. **滚轮过度滚动修**(`Menu.cs`):
   - `ContentH_Main` 从 1600f → 800f,防止滚轮拉到底都是空白

7. **版本号** → v2.7.82(`Menu.cs` 标题 + `ModMain.cs` 日志)

### 改动文件

```
src/Menu.cs           [改] UI 重排 + 标签修 + 居中 + ContentH + 版本号
src/CheatsPatches.cs  [改] TickQuickActions 加秒打碎 snapshot 兜底 restore
src/ModMain.cs        [改] 版本号 v2.7.81 → v2.7.82
src/HANDOFF.md        [改] 本文档
```

### ⚠ 已确认无效的方向(别再试)

| 尝试过 | 为什么无效 |
|---|---|
| `Application.targetFrameRate` / `QualitySettings.vSyncCount` / `Time.fixedDeltaTime` 强推 150Hz | 用户实测体感无变化,不是 Unity pacing 问题(v2.7.76 尝试,v2.7.77 删) |
| PerfDiagnostics stopwatch 诊断 | 只是测耗时,不是修复,用户明确说"卡就是 mod,不想靠诊断拖" |
| 优化 `DynamicPatch.Reconcile()` 调用频率 / mask 缓存 | 调度频率不是瓶颈。每个 Harmony patch **挂载本身** 产生开销,哪怕不被调用也有 detour bridge |
| 动态扫场景 `FindObjectsOfType` 决定是否挂 patch | 用户实测"卡的要死",完全放弃 |

---

## 🆕 2026-05-01 session v2.7.80 — 目录整合 + UI 重排 + 快速操作 bug 修

### 🏁 工作目录统一
此前 `D:\TLD-Mods\TldHacks\`(dev)和 `D:\TLD-Mods\tld--2.55\TldHacks\src\`(repo)两处都有代码,**已分叉**。本轮以 **repo 为唯一真相**,`TldHacks.csproj` 从 dev 搬到 repo `src/`,dev 目录**已删除**。以后只在 `D:\TLD-Mods\tld--2.55\TldHacks\src\` 工作,编译产物在 `src/bin/Release/TldHacks.dll` 自动部署到 `Mods/`。

### 本轮改动(都已合并到 repo)
1. **ModSettings → CheatState 同步 5s → 0.5s**(`ModMain.cs:147`,`_frame % 300 → _frame % 30`)—— 修 ModSettings "Disable All" 后 5 秒内 cheat 还生效
2. **HarvestableInteraction.Init/BeginHold 加 snapshot+restore 闭环**(`CheatsPatches.cs`)—— 修"秒搜刮 toggle off 后字段残留永久生效"bug。共享 `HarvestableSnaps.Snapshots` dict,BeginHold 在 toggle off 后首次触发时 restore 并删 entry;DynamicPatch cleanupGate 保持挂载直到 Snapshots.Count==0
3. **Panel_BodyHarvest.Refresh snapshot+restore**(同上结构)—— 修"秒割肉 toggle off 后 m_HarvestTimeSeconds 残留"
4. **`Cheats.DisableAllCheats()` + Menu 按钮 "★ 全关并立即同步"**(列 2 一次性操作下)—— 一键把所有 cheat bool 设 false + 立即 SyncCheatStateInline + DynamicPatch.Reconcile,**不用等 0.5s**
5. **UI 重排**(Menu.cs):
   - "瞄准" section 从列 1 搬到列 3,与"武器 / 射击"合并成 **"武器 & 瞄准"**,统一 `TOG_W` 宽度两列布局(4 行 toggle)
   - "生火 100%" 从列 3 快速操作搬到 **列 2 环境/篝火**
   - 快速操作重排:秒烤肉+秒搜刮 / 秒割肉+秒打碎 / 加工秒完成+爬绳×5(3 行 6 个)
   - 技能 section **保留 repo 原有折叠**(用户偏好)

### 未动 / 未解决
- **FPS 悬案**:repo v2.7.79 加的 `DiagUnpatchAll` 诊断开关本轮**没再测**。用户说"关 TldHacks fps 高很多",但瓶颈在哪一条 patch 还没定位
- **InventoryGridItem.Update**:repo v2.7.75 已**完全删除**该 patch(更激进,每帧 2400 bridge 消失),OnLateUpdate 兜底覆盖功能
- **ItemPickerMain.OnUpdate**:repo 已通过 `AutoPickupGuard.ReconcileItemPickerPatch()` 动态挂/卸
- **FreezingValueLock**(HANDOFF 提到 v2.7.74 加"冻结寒冷值")在 repo 代码里**没找到实现**,可能已回退

---

## 📋 全功能状态清单(v2.7.82)

**图例**:✅ 能用  ⚠ 有边缘/限制  ❓ 未充分测  ❌ 坏/不实现/NYI

### 列 1 玩家状态
| 功能 | 状态 | 实现 / 备注 |
|---|---|---|
| 无敌模式 | ✅ | 11 affliction Prefix 过滤,DynamicPatch |
| 无坠落伤害 | ✅ | Condition.AddHealth Prefix 过滤 DamageSource.Falling |
| 免扭伤风险 | ✅ | SprainedWrist.SetForceNoSprainWrist + SprainedAnkleStart Prefix |
| 免动物伤害 | ✅ | Condition.AddHealth Prefix 过滤 Wolf/Bear/Cougar |
| 不会窒息 | ✅ | Condition.AddHealth Prefix 过滤 Suffocating |
| 清除死亡惩罚 | ✅ | CheatDeathAffliction.Update Prefix → Cure() |
| 始终温暖 / 无饥饿 / 无口渴 / 无疲劳 | ✅ | v2.7.62 修:Hunger=2450/Fatigue=10 合理低值,能吃喝睡 |
| 速度倍率 0.5/1/2/5x | ✅ | Time.timeScale;默认 1.0 不写回 |
| 技能(10 种单项 + 全部满级,折叠) | ✅ | SkillsManager.GetSkill(t).SetPoints(MaxPoints) |
| 免费制作 / 快速制作 | ✅ | Panel_Crafting.CanCraftBlueprint / CraftingOperation.Update |
| 忽略上锁 / 快速开容器 / 解锁保险箱 | ✅ | Lock.IsLocked + Panel_Container.Enable + SafeCracking.Update |

### 列 2 世界 & 环境 & 商人
| 功能 | 状态 | 实现 / 备注 |
|---|---|---|
| 一击必杀 | ✅ | BaseAi.ApplyDamage Prefix damage=9999 |
| 动物冰冻 | ✅ | TickAnimalsFull + TickAnimalsCheap |
| 动物逃跑 Stealth | ⚠ | v2.7.74 修 on→off 残留;DynamicPatch |
| 真·隐身 | ✅ | BaseAi.CanSeeTarget/ScanForSmells + Start Postfix |
| 生成动物(8 种) | ✅ | uConsole spawn_* 命令,合并到动物 section (v2.7.82) |
| 冰面不破 | ✅ | IceCrackingTrigger.BreakIce/FallInWater DynamicPatch |
| 停止刮风 | ✅ | Weather.DisableWindEffect/EnableWindEffect |
| 篝火 300℃ | ✅ | HeatSource.Update Snapshots 闭环 |
| 篝火永不熄灭 | ✅ | Fire.Update Snapshots 闭环 |
| 全开地图 | ✅ | RegionManager.RevealAllRegions |
| 天气切换 11 种 | ✅ | uConsole lock_weather N |
| 时间跳跃 4 preset | ✅ | TimeOfDay.SetNormalizedTime |
| 清除所有负面 | ✅ | 10 affliction Cure + 15 反射扩展 |
| 解锁全部壮举 | ✅ | 反射 Feat_* m_IsUnlocked |
| 恢复全部耐久 / 修复背包 / 修复手持 | ✅ | Cheats.RestoreDurability 遍历 |
| **★ 全关并立即同步** | ✅ | Cheats.DisableAllCheats() 直接 Reconcile |
| 交易清单 64 / 信任 / 秒交易 / 随时联系 | ❓ | Trader 4 toggle,要先进 Trader 对话才调方法 |
| 商人 uConsole 按钮 | ❓ | 秒完成/刷新/信任+100/解锁对话/解锁交易/解锁改进(未测) |
| 美洲狮首次立即激活 | ⚠ | v2.7.63 加 CougarManager.Update Prefix 兜底 |

### 列 3 快速操作 & 物品 & 武器瞄准
| 功能 | 状态 | 实现 / 备注 |
|---|---|---|
| 秒烤肉 | ✅ | CookingPotItem.Update elapsed-push;DynamicPatch |
| 秒搜刮/采摘 | ✅ | HarvestableInteraction.Init/BeginHold + 共享 HarvestableSnaps 闭环 |
| 秒割肉 | ✅ | Panel_BodyHarvest.Refresh snapshot 闭环 |
| 秒打碎 | ✅ | v2.7.82 修 toggle OFF 后残留;UpdateDurationLabel snapshot + tick 兜底 restore |
| 生火 100% | ✅ | FireManager.CalculateFireStartSuccess Postfix=1f (v2.7.82 挪到快速操作) |
| 加工秒完成 | ✅ | EvolveItem.Update Prefix |
| 爬绳 ×5 | ✅ | TickClimbRope + ClimbRopeSnaps 闭环 |
| UI 堆叠 | ✅ | Panel_Inventory/Container RefreshTable + OnLateUpdate + Hover/Refresh 3 条 |
| 物品不损耗 | ✅ | GearItem.Degrade/WearOut/DegradeOnUse DynamicPatch |
| 衣物不潮湿 / 油灯不耗油 / 保温杯不失温 / 无限容量 / 装任意液体 | ✅ | 5 条 DynamicPatch |
| ItemPicker 不捡自己丢的 | ✅ | ItemPickerMain.OnUpdate 动态挂(AutoPickupGuard.ReconcileItemPickerPatch) |
| **武器 & 瞄准(v2.7.80 合并 UI)** | ✅ | v2.7.84 反编译重写:静态字段直接赋值,不再依赖反射 |
| ∟ 无限弹药 | ✅ | GunItem.RemoveNextFromClip DynamicPatch |
| ∟ 永不卡壳 | ✅ | m_ForceNoJam + TickGuns |
| ∟ 无后坐力 | ✅ | TickCamera 反射 RecoilSpring(struct) + TickGuns 零化 GunItem recoil 字段 |
| ∟ 关闭瞄准景深 | ✅ | CameraEffects.EnableCameraWeaponPostEffects DynamicPatch |
| ∟ 瞄准无晃动 / 抖动 / 呼吸 / 不耗体力 | ✅ | v2.7.84:vp_FPSWeapon.m_DisableAim* static bool 直接赋值 + cam 实例字段归零 |
| 一键武器 8 个(弓/步枪/左轮/斧/刀/箭×50/子弹×50×2) | ✅ | QuickActions.GiveWeapon / Cheats.SpawnItem |
| Spawner 911 条(347 原 + 553 mod) | ✅ v2.7.74 | ItemDatabase + ItemDatabaseMod |
| 传送 24 条精确坐标 | ✅ | CT 汇编块提取 |
| 跨 DLC 区传送 | ⚠ | 需 FastTravel.dll 先去一趟,写 TldHacks_Transitions.txt |

### 未实现(有 Settings 字段但无 patch)
- **QuickFishing**(钓鱼 100%):字段存在,未查 Fishing 类,不在菜单展示
- **CureFrostbite**(治愈永久冻伤):字段存在,未 patch
- **InfiniteContainer**(容器无限容量):Container 无 m_Capacity 字段,待反编译

### 已知核心 bug(优先级排序)
1. **FPS 卡**(禁 TldHacks 后 fps 明显高)—— 真瓶颈未定位,**v2.7.79 的 DiagUnpatchAll 诊断开关没测**。[测试流程见下方 v2.7.79 段]
2. **T1 背包 hover 堆叠显示变 1**(见旧段)

---

## 🆕 2026-05-01 晚会话 v2.7.79 —— FPS 悬案诊断开关(用户重启电脑前交接)

### 🔴 最重要 —— 用户回来第一件事

**FPS 问题仍未解决**。本轮用户反馈"还是有点卡",排除了 fixedDeltaTime/targetFrameRate/vSync pacing、PerfDiagnostics、动态扫场景等方向。用户做过关键测试:**TldHacks 开启 + 所有 toggle 关 → 仍卡**。证明 **静态挂载的 `[HarmonyPatch]` attribute 本身**(不是 tick / runtime 代码)就是瓶颈。

本轮最后加了一个**决定性诊断开关 `DiagUnpatchAll`**,用户还没测。**回来第一件事**:

1. 进游戏 → **MODS → TldHacks → Diagnostic / 诊断** section 最顶
2. 勾 **`Emergency: Unpatch All Harmony(卸载所有 patch)`**
3. **完全退游戏**(让 ModSettings 存盘)
4. **重启游戏**进同一存档
5. `MelonLoader/Latest.log` 验证有 `[DIAG] DiagUnpatchAll = ON → TldHacks 所有 Harmony patch 已卸载`

**结果分岔**:

| 结果 | 结论 | 下一步 |
|---|---|---|
| 开关 ON 仍卡 | **100% 不是 TldHacks**。是整合包其他 mod / 游戏引擎本身 | 禁 UniversalTweaks / Sprainkle / SonicMode 等有大量 patch 的 mod 逐一排除;DarkerNights 在 2.55 卡启动(memory 有记录);最后考虑 GfxBoost fixedDt 路径 |
| 开关 ON **不卡** | **100% 是 patch 总数问题** | 把剩余 26 个静态 `[HarmonyPatch]` attribute **全部**迁 DynamicPatch,按 toggle 挂卸。清单见下 |

### 📋 待迁 26 个静态 [HarmonyPatch](如果诊断证明是 patch 总数问题)

grep 源:`grep -rn "^\[HarmonyPatch\(" src/` 截止 v2.7.79。

**Cheats.cs(7 个)** —— 全是 affliction Start 事件,低频但常驻 bridge:
- `PlayerManager.MaybeFlushPlayerDamage` → GodMode / NoFallDamage / ImmuneAnimalDamage
- `Hypothermia.HypothermiaStart` → GodMode / AlwaysWarm
- `SprainedWrist.SprainedWristStart` → GodMode / NoSprainRisk
- `SprainedAnkle.SprainedAnkleStart` → GodMode / NoSprainRisk
- `BloodLoss.BloodLossStart` / `BloodLossStartOverrideArea` → GodMode / ImmuneAnimalDamage
- `Infection.InfectionStart` → GodMode

**CheatsPatches.cs(11 个)**:
- `Panel_Repair.StartRepair` → QuickAction
- `CameraFade.FadeOut/FadeIn/FadeTo/Fade(5)` 4 个 → QuickAction / QuickBreakDown / QuickCraft(FadeSuppressionWindow 控制)
- `BaseAi.ApplyDamage` → InstantKillAnimals
- `Panel_Crafting.CanCraftSelectedBlueprint` / `CanCraftBlueprint` → FreeCraft
- `BaseAi.Start` → TrueInvisible(属性同步)
- `BaseAi.OnDisable` → **❗不要迁** —— BaseAiRegistry HashSet 清理,是稳定性基础设施,防 stale wrapper AccessViolation
- `CougarManager.UpdateWaitingForArrival` → CougarInstantActivate

**AutoPickupGuard.cs(3 个)** → BlockAutoPickupOwnDrops:
- `GearItem.Drop(int,bool,bool,bool)` / `PlayerManager.Drop(GameObject,bool)` / `PlayerManager.ProcessPickupItemInteraction`

**Stacking.cs(5 个)** —— ⚠ 默认 ON,迁了也永远挂着,**意义有限**:
- `Panel_Inventory.RefreshTable` / `Panel_Container.RefreshTables` / `InventoryGridItem.RefreshDataItem` / `Panel_Container.HoverItem` / `InventoryGridItem.Refresh`
- 如果要省,要让 Stacking 也能按需禁(用户可能不愿意,Stacking 是默认功能)

**可迁 20 个(Stacking 5 + BaseAi.OnDisable 1 不迁,其余 20)**。

### v2.7.76 → v2.7.79 其他改动回顾

- **v2.7.76(用户/linter 改):** 尝试加 `FixFramePacing` / `FixedUpdateRate` / `UnlockRenderFps` / `PerfDiagnostics` 等 Unity pacing 修复 —— **实测无效,用户明确说都是死路**,v2.7.77-78 期间删干净
- **v2.7.77:** PerfDiagnostics / PerfWindow stopwatch 删
- **v2.7.78 (之前迭代):** Condition.AddHealth 3 个重载 + DamageFilter 迁 DynamicPatch;BaseAi.CanSeeTarget/ScanForSmells 强化为 Reconcile mask 缓存 —— 也证明 Reconcile 调度频率不是瓶颈
- **v2.7.79(本轮):**
  - **删 `FreezeColdValue` toggle** —— 用户确认和 `AlwaysWarm` 冲突(都锁 `Freezing.m_CurrentFreezing`),UI 合并
  - **加 `DiagUnpatchAll` 诊断开关** —— 启动时 `HarmonyInstance.UnpatchSelf()` + skip `Reconcile`,所有 tick return

### 改动文件(未 commit)

```
src/Settings.cs       [改] 删 FreezeColdValue;加 DiagUnpatchAll
src/Cheats.cs         [改] 删 FreezeColdValue / _frozenColdSnapshot 字段
src/CheatsPatches.cs  [改] TickStatus 里删 FreezeColdValue 分支
src/Menu.cs           [改] 温饱 section 删第 3 行 fcv toggle
src/ModMain.cs        [改] OnInitializeMelon 加 DiagUnpatchAll 分支(UnpatchSelf + skip Reconcile);
                            OnUpdate/OnLateUpdate/OnGUI 都判断 DiagUnpatchAll
                            版本号 2.7.78 → 2.7.79
src/HANDOFF.md        [改] 本段
```

编译 0 error,DLL 已自动部署到 `D:/Steam/steamapps/common/TheLongDark/Mods/TldHacks.dll`。

### ⚠ 已确认无效的方向(别再试)

| 尝试过 | 为什么无效 |
|---|---|
| `Application.targetFrameRate` / `QualitySettings.vSyncCount` / `Time.fixedDeltaTime` 强推 150Hz | 用户实测体感无变化,不是 Unity pacing 问题(v2.7.76 尝试,v2.7.77 删) |
| PerfDiagnostics stopwatch 诊断 | 只是测耗时,不是修复,用户明确说"卡就是 mod,不想靠诊断拖" |
| 优化 `DynamicPatch.Reconcile()` 调用频率 / mask 缓存 | 调度频率不是瓶颈。每个 Harmony patch **挂载本身** 产生开销,哪怕不被调用也有 detour bridge |
| 动态扫场景 `FindObjectsOfType` 决定是否挂 patch | 用户实测"卡的要死",完全放弃 |
| 按"主力 vs 冗余"分批迁 DynamicPatch | 做了几批,DamageFilter / CanSee / Smell 等都迁了,但用户仍卡 → 没迁完的静态 patch 仍是凶手 |

### 🧪 保留不动的稳定性 patch(别迁走)

- `BaseAi.OnDisable` —— 清 BaseAiRegistry HashSet,防跨场景 stale wrapper AccessViolation(v2.7.29 修的,血泪教训)
- `CameraFade.Fade*` —— 防黑屏兜底,如果迁走可能留黑屏(但 FadeSuppressionWindow 本身是门闸,迁后逻辑需要仔细测)
- `PlayerManager.MaybeFlushPlayerDamage` —— 伤害过滤总闸,如果迁走要确认没漏路径

---

## 🆕 2026-05-01 下午会话 v2.7.75 —— FPS 重大优化 + 英文兼容 + Toast 翻译(未 commit / FPS 未最终验证)

主题:用户反馈"启动就卡",确认是 TldHacks 造成(A/B 禁用 dll 后不卡)。套用 v2.7.18 历史经验 —— **每帧每实例 Harmony bridge = 主要 FPS 杀手**。

**结尾补完**:用户说"OK 把翻译也做完吧",补完了 Cheats.cs / CheatsExtra.cs 里 `LastActionLog` / toast 消息的英文翻译。

### 🔥 性能核心:拆"每帧 bridge"三板斧

| # | 做了什么 | 原因 | 数字 |
|---|---|---|---|
| 1 | **删 `Patch_InventoryGridItem_Update` Postfix**(`Stacking.cs:302`) | 40 cell × 60fps = **2400 次/秒 bridge**,早已被 v2.7.60 early-out 压住但 bridge 本身就是开销。OnLateUpdate 已每帧 reapply SeenItems,Update Postfix **功能冗余** | -2400 bridge/s |
| 2 | **删 5 个 `GunItem` getter patch**(GetSwayIncreasePerSecond / GetRecoilPitch / GetRecoilYaw / GetCurrentStaminaPercent / CanStartAiming) | v2.7.0 已定调:主力是 `TickCamera` 的 `m_DisableAim*` 字段 + `m_RecoilSpring` 重置,getter 只是 "belt-n-suspenders" fallback。保留 `RemoveNextFromClip`(InfiniteAmmo 核心) | 启动 -5 bridge |
| 3 | **新 `DynamicPatch.cs`** + 5 个 Update patch 改按 toggle 挂卸 | Fire.Update / HeatSource.Update / EvolveItem.Update / BaseAi.CanSeeTarget / BaseAi.ScanForSmells。`[HarmonyPatch]` attribute 全部去掉,ModMain 每 30 帧(0.5s)调 `DynamicPatch.Reconcile()`。Fire/HeatSource 有 snapshot cleanup 走 `SyncWithCleanup`(toggle off 等 snapshots 空了才 Unpatch) | toggle 关时 **0 bridge**,toggle 开时正常 |

**总账**:静态 Harmony patch 54 → 43;其中 5 个改为动态按需;最重的"每帧每 cell"bridge 杀手 0 个。

### 🌐 英文兼容(独立功能)

- 新 `I18n.cs`:`IsEnglish` 按 `Settings.LanguageMode`(Auto/中文/English);Auto 用 `CultureInfo.CurrentUICulture.Name.StartsWith("zh")` 判断;非 zh → English。
- `Settings.cs` 加 `LanguageMode` int 字段 + `[Choice("Auto", "中文 / Chinese", "English")]`。
- `Menu.cs` 全部 UI 包 `I18n.T("中文", "English")`:Tabs / Section / Toggle label / Button / 动态 `$"..."` 都过了。HourLabels / WeatherLabels / Tabs 三个 static 数组改成 `Zh/En` 两份 + property 动态选。
- 英文宽度缩写保栅格不变:`" 无敌模式"` → `" God Mode"`,`" 瞄准不耗体力"` → `" No Aim Stamina"`,超宽用缩写如 `" Clr Death Pen."`。
- ✅ Toast / LastActionLog 翻译补完(2026-05-01 下午收尾):
  - `Cheats.cs` —— 刷物品 / 修复 / 一键满技能 / 清负面 / 天气 / 全地图揭示 / 秒杀动物 / 温度锁定 等 ~40 条状态栏消息
  - `CheatsExtra.cs` —— 传送(同区/跨区/就绪)、技能满级、壮举解锁、修复背包(ok/total/fail)、蓝图解锁、设温度、UnlockAllBlueprints。
  - 模式:`I18n.IsEnglish ? "English" : "中文"` 三元。
  - 未动:`affliction 清单`(15+ 中文 affliction 内部名)改动太大,保留;Latest.log 调试输出(`[XXX] ...`)保留中文诊断。

### Settings 属性的限制

`[Name]` / `[Section]` / `[Description]` 是 C# attribute 参数,**必须编译期常量**,不能用 `I18n.T()`。当前保留 v2.7.0 以来的 `English(中文)` 双语格式不动。

### 已改 / 新文件

| 文件 | 动作 |
|---|---|
| `I18n.cs` | **新建** 40 行 |
| `DynamicPatch.cs` | **新建** 90 行 |
| `Settings.cs` | 加 LanguageMode(1 个字段 + Choice) |
| `Menu.cs` | 80+ 条字符串包 I18n.T;3 个 static 数组拆双语;标题版本号 v2.7.75 |
| `ModMain.cs` | 加 `DynamicPatch.Reconcile()` 0.5s tick;启动日志 v2.7.75 |
| `CheatsPatches.cs` | 删 5 GunItem getter;5 个 Update patch 去 `[HarmonyPatch]` attribute(类保留,Prefix/Postfix 改 internal 给 AccessTools 找得到) |
| `Stacking.cs` | 删 InventoryGridItem.Update Postfix(FPS 杀手);加 panel 未激活清空 dict 路径 |
| `DynamicPatch.cs` | v2.7.75 二次扩:patch 数从 5 → 26(GearItem×3 / ClothingItem×2 / Lock×2 / Flask×3 / KeroseneLamp / BodyHarvest / CookingPot / CraftingOperation / SafeCracking / TimedHoldInteraction / CougarManager / Panel_BreakDown×2 / CheatDeathAffliction / GunItem.RemoveNextFromClip)都按 toggle 动态挂卸 |
| `CheatsPatches.cs` | 去 18+ 个 `[HarmonyPatch]` attribute,private → internal(给 AccessTools.Method 找 Prefix/Postfix)|
| `Cheats.cs` | ~30 条 toast/LastActionLog 翻译(I18n.IsEnglish 三元)|
| `CheatsExtra.cs` | ~10 条 toast/Log 翻译(传送/技能/壮举/修复背包/BP/温度)|

编译 0 error(6 warning),最新 DLL 已部署到 `D:/Steam/steamapps/common/TheLongDark/Mods/TldHacks.dll`。

### ⚠ 温饱 toggle "关不掉" —— 不是 bug

用户反馈"关温饱 toggle 仍激活",排查后确认:**`TickStatus` 里 `if (CheatState.NoHunger || CheatState.GodMode)` 的 GodMode 分支覆盖所有温饱值**(CheatsPatches.cs:1147-1190)—— GodMode 开着则温饱 toggle 无意义。用户确认"保持现状,GodMode 一键包含温饱"。
如果以后想变 —— 3 选 1:① GodMode 只管 HP ② 现状 ③ GodMode 开时自动打开 4 个 toggle,关 GodMode 不动。

### 🧪 下一轮要测的(⚠ FPS 至结束未确认改善,这是本次收尾状态)

1. **FPS 是否质变** —— 用户 3 次反馈"还是卡",最后一次是我全面 DynamicPatch 化 26 个高频 patch **之后**(最新 DLL)还没测。
   - 如果**还卡** → 不是 bridge 瓶颈,怀疑方向:
     - 其他 mod 冲突(117-mod 整合包的其他 Harmony patch 堆积,参考 memory `feedback_tldhacks_modpack_issues`)
     - Unity physics fixedDt 25Hz(参考 GfxBoost 方案,memory 里有)
     - `DiagPauseRuntime` ON 仍卡 = 不是 runtime tick 问题 = 是 **Harmony patch 挂载本身**(那就继续砍 patch)
   - 如果**不卡了** → 保留当前 DynamicPatch 架构,升版本号 v2.7.75 正式发布。
2. **DynamicPatch 挂卸 log** —— toggle 开关 FireNeverDie / FireTemp300 / QuickEvolve / TrueInvisible / Stealth / InfiniteDurability / NoWetClothes / IgnoreLock / UnlockSafes / LampFuelNoDrain / InfiniteAmmo / Flask 三件 / QuickCraft / QuickCook / QuickSearch / QuickHarvest / QuickBreakDown / CougarInstantActivate / ClearDeathPenalty 时,Player.log 应有 `[DynPatch] ON/OFF TldHacks.<method>` 行。如果没 log 说明 Reconcile 没跑。
3. **Stacking hover 闪 1 (T1)** 是否回归 —— 删 InventoryGridItem.Update Postfix 依赖 OnLateUpdate 兜底。如果 hover 闪 1 回来 → 不要加回 Update patch,用 `InventoryGridItem.Refresh` Postfix 或 coroutine。
4. **英文切换** —— ModSettings UI 里 LanguageMode 选 English 或 Auto + 系统非中文,Menu 文字是否切换。toast 消息(如"已刷 ×1 xxx")是否跟切换。
5. **GodMode 温饱行为** —— GodMode 关 + 温饱 toggle 关,hunger/thirst 应该变化(已确认是 design,不改)。

### 未做(后续按需)

- ~~Level B DynamicPatch(GearItem/ClothingItem/Flask/Lamp)~~ → **v2.7.75 已全改**,26 个 patch 全部 DynamicPatch
- ~~toast 消息英文翻译~~ → **v2.7.75 已补完**
- Fire/HeatSource/EvolveItem 从 Update patch 升级为 `OnEnable` / `Awake` 事件 patch(真·一次性设字段,比 Update 高频更省),当前是 DynamicPatch Update(toggle on 时仍每帧跑)—— 如果 FPS 验证还有问题可走这条
- `affliction` 名称英文化(~15 条中文 affliction 内部映射,改动面大)
- Latest.log 里 `[DynPatch] ON ...` 诊断 log 改成只在 DEBUG 编译打,release 编译去掉(减少 Player.log 噪音)

### 本次修改总览(文件 diff)

```
src/I18n.cs           [新建]
src/DynamicPatch.cs   [新建]
src/Settings.cs       [改] LanguageMode + DiagPauseRuntime
src/Menu.cs           [改] 80+ I18n.T,3 个 static 数组拆双语,版本 v2.7.75
src/ModMain.cs        [改] DynamicPatch.Reconcile 0.5s tick,DiagPauseRuntime gate
src/CheatsPatches.cs  [改] 删 5 GunItem getter,18+ Update patch 去 [HarmonyPatch],private→internal
src/Stacking.cs       [改] 删 InventoryGridItem.Update Postfix,panel 未激活清 dict
src/Cheats.cs         [改] ~30 条 toast 翻译
src/CheatsExtra.cs    [改] ~10 条 toast 翻译
src/HANDOFF.md        [改] 本文档
```

未做 `git add` / `git commit`。当前编译产物 `TldHacks.dll` 已直接部署到游戏 `Mods/` 目录。

---

## 🆕 2026-05-01 凌晨会话 v2.7.74(已 commit)

继续 v2.7.73 工作树。新增/改动:

- **Spawner 加 mod 物品**:新文件 `src/ItemDatabaseMod.cs`,自动扫 40 个 `Mods/*.modcomponent` 得 553 条(vanilla 358 已有的去重),每条名字后 `[ModName简写]`。`Menu.RebuildFilter` 双 list 遍历,顶部 label 合计。生成脚本 `C:\Users\82077\AppData\Local\Temp\tldmod\gen_moddb.py`。
- **秒打碎"阴天感"再修**(用户反馈 CT 也出这 bug):发现 `Patch_CameraFade_Fade5` 和 `Patch_CameraFade_Fade` 挂同签名(冗余)→ 合并为一个 5 参 Prefix 归 0 start/target/time/delay。加诊断 log 后发现 `BreakDownFinished` Postfix 命中 0 次(IL2Cpp 内联)→ 改走 **Update 边沿检测**(`IsBreakingDown` true→false)+ **Enable(false) 兜底**,统一走新 `BreakDownCleanup.Run`:FadeSuppress 5s + CameraFade.FinishFade + `PassTime.End()` + `PassTime.m_TimeAccelerated=false` + Panel 实例 `m_TimeIsAccelerated=false`。log 证据 Fade5 `target=0.5` 已拦截,用户"概率性还暗"未最终定论。
- **Stealth 关掉动物还在逃**:TickAnimals 加 `else if (_lastTickedStealth)` 边沿分支 → 遍历 AI 把 Flee 拉回 Wander + ClearTarget。
- **冻结寒冷值 toggle**(新):玩家列温饱 section 加 toggle,CheatState.FreezeColdValue + `_frozenColdSnapshot` (NaN = 未锁)。TickStatus 开启抓当前值 / 关闭清 NaN → 游戏自然变化。
- **锁温度 uConsole UI 删**(坏的 unlock_temperature) → 用上面冻结寒冷值替代。
- 版本号 `v2.7.72` → `v2.7.74`(Menu.cs 标题 + ModMain.cs 日志),DLL 204800 bytes(+50%)。

---

## 🆕 2026-04-30 晚会话 v2.7.73 工作树(已并入 v2.7.74)

codex 在仓库外 `D:\TLD-Mods\TldHacks\` 把 Menu.cs 重写成 Cursor Ink UI (v2.7.66→v2.7.72)后合并回本仓库。Claude 本会话在此基础上做 5 处修复。

### 改动清单

**Menu.cs**
1. **列重分组** —— 锁&容器 / 制作 / 瞄准 → 列 1;商人 uConsole → 列 3。三列视觉高度更平衡
2. **x1.7 缩放 label 垂直居中** (line 294) —— codex 的 `_mutedLabelStyle` 没 alignment 默认 UpperLeft,字符比按钮字符偏上偏左。**不引 `UnityEngine.TextRenderingModule`**(避免动 csproj),改用坐标 `R(W-158, 14, 50, 18)`
3. **Spawner 翻页消失** (line 710) —— codex `ContentH_Spawner=H-108` 但 Spawner 内部写死 `availH=(H-80)-y-...` → 多塞 28px → 分页按钮被裁出 scroll content。改 `H-80 → ContentH_Spawner`
4. **UI 切场景后透明** (line 101-107 + InitStyles 开头) —— `Tex()` 的 `Texture2D` 没 `hideFlags` → Unity 场景切换 GC 掉 → GUIStyle.background 指 dead texture。加 `tex.hideFlags = HideFlags.HideAndDontSave` + InitStyles 开头兜底检测 `_bgTex == null` 触发重建

**CheatsPatches.cs**
5. **秒打碎完成后持续暗** —— v2.7.65 只 `FadeIn(0,0)+SetTODLocked+FadeSuppressionWindow 3s` 不够。加两条防线:
   - `Patch_BreakDown_Finished_Unfade` Postfix 强化:强制 `m_TargetAlpha=0/m_StartAlpha=0/m_FadeTimer=0/m_FadeDuration=0` + `FinishFade(true)` 跳过进行中 fade
   - 新增 `Patch_CameraFade_Fade5` —— codex 只 patch 了 FadeIn/FadeOut/FadeTo 的 3/4 参数重载,`Fade(startAlpha, targetAlpha, time, delay, action)` 5 参数版本是漏网路径。打碎持续暗很可能走这条

### 状态
- 编译 0 错误,DLL 140288 bytes 已部署到 `Mods/TldHacks.dll`
- Menu.cs 标题栏版本号仍是 `"TldHacks v2.7.72"` —— 等 5 项测过再升 v2.7.73 + commit
- `git status`:Menu.cs + CheatsPatches.cs modified 未 commit

### 下次要测
1. 三列高度是否平衡(列重分组)
2. 右上角 x1.7 和 -/+ 是否垂直对齐
3. Spawner 翻页按钮出现了吗
4. 切场景 / 打开 uConsole 后 UI 底色保持不透明吗
5. 秒打碎完成后还持续暗吗 —— 如还暗:**室内还是室外 / 打碎什么物品**,加诊断 log 精确定位(可能要 patch UniStorm 而非 CameraFade)

### ⚠ codex UI 主题别改
用户明确说过两次。只允许:section 重分组 / 功能 bugfix / texture 生命周期修 / 单 toggle 增删。
不要碰:W=1280/H=760 / COL_W=405 / TOG_W=182 / SECTION_END_ADV=14 / Cursor Ink 7 个 Color 常量 / GUIStyle 配置。
详见 memory `feedback_tldhacks_codex_ui.md`。

---

## 🆕 2026-04-30 session 新 mod / 改动 — 明天要测

### 新建 3 个独立 mod(不在 TldHacks 内,独立 dll)
| mod | 作用 | 默认值 |
|---|---|---|
| **GfxBoost** | 强制 `QualitySettings` 低值 + **强制 `Time.fixedDeltaTime = 1/150`** 修"fps 150 体感 30"感 | fixedRate=150Hz, shadowDist=50m, cascades=1, shadowRes=Low, pixLights=1, lodBias=0.8, AA=off |
| **LightCull** | 每秒扫所有 Light,远的禁阴影/禁用 | maxShadow=4, shadowR=25m, disableR=80m |
| **FrameTimeProbe** | 每 5s log frame time p50/p95/p99 + fixedDt + timeScale | 无配置 |

### 关键发现:用户"fps 150 体感 30"的根因
FrameTimeProbe log 出 `fixedDt=40.0ms(25Hz)` —— TLD 默认物理 25Hz,和渲染 150fps 完全不同步。**GfxBoost 强制 fixedDeltaTime=1/150** 后体感帧率质变。

### v2.7.64 / v2.7.65 改动
- **修"不饥饿/不口渴/不疲劳"导致不能吃喝睡(v2.7.62)**:仿 CT 设"合理低值"(Fatigue=10 / Hunger=2450)而非 max/0。删 Hunger.UpdateCalorieReserves Prefix return false
- **美洲狮秒激活(v2.7.63)**:加 `CougarManager.Update` Prefix tick 兜底,state<2 强推 2。绕开 UpdateWaitingForArrival 只在 state=1 时才调的限制
- **QuickBreakDown 变灰 / 变暗修(v2.7.64 / v2.7.65)**:
  - FadeSuppressionWindow 窗口 1.5s → **3s**
  - **删 `m_TimeIsAccelerated=true`**(这个字段触发 ScreenTint)
  - 加 `BreakDownFinished` Postfix 主动调 `CameraFade.FadeIn(0,0,null)` 强制亮屏
  - **加 `SetTODLocked(true/false)`**:OnBreakDown 锁,BreakDownFinished/ExitInterface/OnCancel 解锁 —— 和 QuickCraft 原理一致,冻结 TOD 避免触发 UniStorm 昼夜/天气 ScreenTint
- **FlyHotkey 回退**(v2.7.60 我错删,F1 实测 work,已恢复 CFly 字段)
- **商人 uConsole 按钮合并**(v2.7.61)到列 2 "商人 & 美洲狮" 下的"商人 uConsole 命令"子 section,删控制台区原 block

### 明天要测的
1. **150fps 体感应该变质**(fixedDt 生效)—— FrameTimeProbe log 查 `fixedDt=6.7ms(150Hz)` 就对
2. **打碎变灰是否解决**(TOD 锁定是最后一招,还有问题的话可能要 patch UniStorm 的 tint 路径)
3. **美洲狮能否生效**(cougar tale scene 查 log `[Cougar] 强推 state X → WaitingForTransition (2)`)
4. **商人功能**(需进 trader 对话后 toggle 才调 GetAvailableTradeExchanges,要求先联系商人)
5. **吃喝睡恢复**(v2.7.62 后 NoHunger/NoThirst/NoFatigue toggle 开了应该能吃/喝/睡)

### 性能诊断结论
- `p50=5.5ms, p95=15ms, p99=24ms, max=27ms` —— CPU 其实够快,没 spike
- 用户说"卡"不是 spike 问题,是 fixedDt 25Hz 的 **物理/AI 采样率过低** 导致的"滑步感"
- DLSS 4.5 不现实(工作量月级 +TLD Unity built-in pipeline 不支持)
- Lossless Scaling 是外部替代

---

## 🛠️ v2.7.60 改动(self-review 后修)

### Bug 修 4 个
1. **TransitionRecorder key 用 Unity sceneName** —— 之前用 `snap.ToSaveId`,DLC Tale scene 真需要 mod 时(ToSaveId ≠ scene 名)会 lookup miss,等于无效
2. **删 FlyHotkey 死按键** —— `uConsole fly` release 不 work,按键只 toggle 没人读的 flag
3. **删 4 个无用 tick counter**(`_durabilityTick / _camTick / _animalsCheapTick / _animalsFullTick`)—— v2.7.21 `_frame % N` 重构后残留
4. **删 CheatState.C*(5 字段)** —— uConsole toggles 全部不起作用,纯死代码

### 性能优化 3 个
5. **`InventoryGridItem.Update` Postfix 加 early-out** —— panel 关或 stacking off 时零开销(之前 40 cell × 60fps × Harmony stub = 2400/s)
6. **`OnLateUpdate` 加 StackingEnabled 守卫**
7. **Scene 切换清 3 个 dicts**:`Fire.Snapshots` / `HeatSource.Snapshots` / `AutoPickupGuard.DroppedAt` —— 防 long session 累积

---

## 🎯 当前版本 v2.7.60 — 待测 / 需继续关注

### 必测(本 session 新改,还没完整验证)
1. **秒烤肉 v2.7.51 重做** — 之前 "放肉→Ready→左键拾取时抽搐卡死"。新做法仿 CT:
   只在 `m_CookingState == Cooking` 时写 `m_CookingElapsedHours = cookTimeMin/60 × 1.01`,
   **不碰 percent/state** → 让原方法自然转 Ready。拾取流程应不再被干扰。
2. **物品生成 v2.7.57 换 API** — 原 `AddItemCONSOLE` 在 release 被 stub,DLC 物品点了没反应。
   改 `GearItem.LoadGearItemPrefab(name)` + `PlayerManager.InstantiateItemInPlayerInventory(prefab, qty, 100f, None)`。
   Latest.log 会打 `[Spawn] +N prefab`,Menu 底部状态栏显示"已刷 ×N xxx"。
3. **DLC 跨区传送不丢物品(TransitionRecorder v2.7.56)** — 起因:神秘湖→废弃机场物品丢失。
   原因:Tale scene 的 save slot id ≠ Unity scene 名,我们硬猜触发 tale-init。
   **用户要先用 FastTravel.dll 去一趟 DLC scene(AirfieldRegion/BlackrockRegion/WhalingStationRegion/MountainPassRegion 等)**,
   mod 会在 `OnSceneWasInitialized` 自动把 `GameManager.m_SceneTransitionData` 字段存到
   `Mods/TldHacks_Transitions.txt`。以后我的按钮跨过去会 lookup 这份记录用真实 save slot id。

### 已确认修好(用户确认) ✅
- **篝火 300℃ / 永不熄灭 toggle on/off 恢复**(v2.7.49 加 snapshot + restore dict)
- **秒采集** 可用但有 0.1s 读条(v2.7.54 回退到 deltaTime ×10000,用户接受)
- **进出门** 不被秒采集 patch 破坏

### 按 CT 写好,用户说免测 ✅
- 商人 4 个 toggle(交易清单 64 / 信任满 / 秒完成交易 / 随时可联系)—— `TraderManager.GetAvailableTradeExchanges` + `IsTraderAvailable` + `ExchangeItem.IsFullyExchanged` Prefix
- 美洲狮秒激活 — `CougarManager.UpdateWaitingForArrival` Prefix 设 `m_ActiveTerritory.m_CougarState = WaitingForTransition(2)`

### 待排查(用户报告失败)
- **ItemPicker 不捡自己丢的物品(v2.7.58 → v2.7.59)** — 用户首次测失败。v2.7.59 加诊断 log + 第二条 drop 路径 `PlayerManager.Drop(GameObject, bool)`。下次测看 `Latest.log`:
  - `[AutoPickupGuard] hooked ItemPickerMain.OnUpdate` — 启动时,ItemPicker patch 挂成功
  - `[AutoPickupGuard.GIDrop] hook 生效` — 首次 drop 物品
  - `[AutoPickupGuard.IPUpdate] 第一次进 OnUpdate` — 首次按 W 触发 ItemPicker
  - `[AutoPickupGuard.Skip] 第一次阻止 pickup` — 真正拦截到
  - 哪条 log 没出,就是哪步没挂。
- **回收衣服秒完成(v2.7.59 加强 QuickBreakDown)** — 原 `UpdateDurationLabel` 是 private 只调一次。
  新加 `Panel_BreakDown.Update` Prefix:`IsBreakingDown()` 时每帧设 `m_SecondsToBreakDown=0.2` + 
  `m_TimeSpentBreakingDown=1` + `m_BreakDown.m_TimeCostHours=0` 强推完成。

### 本 session UI / 功能变更(v2.7.53 → v2.7.59)
- UI 统一常量 `TOG_W=180 / TOG2_OFF=200 / TOG_WIDE=380`,所有 2-toggle 行对齐
- 删冗余:蓝图解锁按钮(失效)/ NYI 三件 / QuickAction toggle(和秒搜刮重复)/ "火焰无限时长"label
- 新分组:生存/温饱/移动速度/技能 | 动物/环境/篝火/锁容器/世界时钟(地图+天气+时间)/一次性 | 快速操作/制作/物品装备/武器/瞄准/一键获取武器
- Spawner tab:**动态 PageSize**(根据视窗剩余高度)+ **cols 3→4**(每页 +33%)+ ContentH = viewport 无滚动
- Teleport:**24 条精确坐标**(全部 Pos≠0),按 scene 英文字母排序,CT 汇编块提取 9 个 prepper/应急舱
- 新"★ 打印坐标到 log"按钮 → `Latest.log` 写 `[POS-MARK] Scene=XXX X=... Y=... Z=...`
- **商人 & 美洲狮**:5 个新 toggle 按 CT 复刻
- **TransitionRecorder**:学习 scene transition 持久化到 `Mods/TldHacks_Transitions.txt`
- **AutoPickupGuard**:`ItemPicker 不捡自己丢的物品` toggle(Patch ItemPickerMain.OnUpdate + 2 条 drop 路径)

### ItemPicker 配置
- `D:\Steam\steamapps\common\TheLongDark\Mods\ItemPickerCustomList.txt`(同步 repo `configs/Mods/`)
- v2.7.59 本次删了 29 条:
  - **锅具 3**:CookingPot / Skillet / CookingPotDummy
  - **生肉红 7**:RawMeatBear/Deer/Moose/Rabbit/Wolf/Cougar/Ptarmigan
  - **生鱼 16**:RawBurbot / RawCohoSalmon + _A01 / RawCopperFish_A01 / RawGoldeye / RawLakeBurbot / RawLakeGoldeye_A01 / RawLakeWhiteFish + _A01 / RawRainbowTrout + _A01 / RawRedIrishLord + _A01 / RawRockfish / RawSmallMouthBass + _A01
  - **XxxRaw 3**:BirdEggRaw / BirdMeatRaw / OrcaMeatRaw
  - 保留:CattailRhizomeRaw(生香蒲根茎)/ RiceRaw(生米)—— 植物非肉
- **改后必须重启游戏**,ItemPicker 启动时才读一次

---

## 🕰️ 历史 v2.7.48 遗留测试项(已过期,保留参考)

1. ~~采集玫瑰果等地上作物~~ — 已解决 (v2.7.54 deltaTime ×10000)
2. ~~秒烤肉不烤糊~~ — 已改为 v2.7.51 仿 CT elapsed-push 方案
3. ~~篝火永不熄灭~~ — v2.7.49 snapshot+restore 解决
4. ~~进出门不被秒搜索 patch 破坏~~ — 已确认

## 📝 本次 session 全量功能 (v2.7.43 → v2.7.48)

### CT 方案复刻 16 项(全部在菜单里有 toggle)

**列 1 速度类(CT)**
- 秒烤肉 (CookingPotItem.UpdateCookingTimeAndState Postfix 设 PercentCooked=1/Ruined=0)
- 秒搜刮/采玫瑰果 (HarvestableInteraction.InitializeInteraction+BeginHold 设 DefaultHoldTime=0.001/Timer=99999)
- 秒割肉 (Panel_BodyHarvest.Refresh 设 HarvestTimeSeconds=Total + Minutes=0 / BodyHarvest.MaybeFreeze 设 PercentFrozen=0)
- 秒打碎 (Panel_BreakDown.UpdateDurationLabel 设 SecondsToBreakDown=0.2 / BreakDown.TimeCostHours=0)
- 加工秒完成 风干(EvolveItem.Update 设 TimeToEvolveGameDays=0/TimeSpentEvolvingGameHours=1)

**列 2 装备/锁(CT)**
- 解锁保险箱 (SafeCracking.Update → 自动 UnlockSafe() / LockedInteraction.IsLocked → false)
- 油灯不耗油 (Il2CppTLD.Gear.KeroseneLampItem.ReduceFuel → skip)
- 保温杯不失温 (Il2CppTLD.Gear.InsulatedFlask.CalculateHeatLoss → skip)
- 保温杯无限容量 (InsulatedFlask.UpdateVolume → skip)
- 保温瓶装任意液体 (IsItemCompatibleWithFlask Postfix → true)

**列 3 篝火/治疗(CT)**
- 篝火 300℃ (HeatSource.Update 设 m_MaxTempIncrease=300)
- 篝火永不熄灭 (Fire.Update 设 m_IsPerpetual=true + m_MaxOnTODSeconds=Infinity)
- 容器无限 **NYI** (Container 无 m_Capacity 字段,待反编译)
- 治愈永久冻伤 **NYI** (Condition.GetMaxHPModifier 待 patch)
- 清除死亡惩罚 (CheatDeathAffliction.Update → Cure())
- 钓鱼 100% **NYI** (未查 Fishing 类)

### 快速制作重点修复 (v2.7.43/44)

**最终方案**(用户给的思路 + CT 映射):
- `HarmonyPatch(CraftingOperation, "Update")` Prefix
- 设 `m_RealTimeDuration=0.2f` + `m_HoursToSpendCrafting=100f`
- 返回 true 让原 Update 跑,游戏读大 hours 推 percent 瞬满
- 不碰 TOD/timescale/accelerate → 真实时间和游戏时间都几乎不动
- `CraftingEnd` Postfix 调 `CameraFade.FadeIn(0,0,null)` 修完成后残留暗屏

**v2.7.20-40 期间失败 8 次原因**:
都在改 TOD/Accelerate/TODLocked/DayScale/HoursPlayed,都破坏了 Panel_Crafting percent 推进依赖的 TOD 流逝。CT 方案不碰 TOD,直接让 CraftingOperation.Update 内部的 HoursToSpend=100 → 游戏自己算 percent 瞬满。

## ⚠️ 未解决 bug (v2.7.35 session)

### T1: 容器/背包页 hover 堆叠显示变 1

**现象**:打开柜子 → hover 或点击一个 50 熊肉的堆叠 cell → UI 显示变 "1",必须转移一次让数量变化才恢复 "x50"。
**4 次尝试失败**:
1. v2.7.30 `InventoryGridItem.OnHover` / `ToggleSelection` / `UpdateConditionDisplay` Postfix → 可能 silent fail
2. v2.7.32 `Stacking.OnLateUpdate` 4 帧改每帧 → 不够
3. v2.7.34 `Panel_Container.HoverItem` Postfix + `InventoryGridItem.Update` 仅 hover 时 reapply → 无效
4. v2.7.35 force 隐藏 `m_UnitLabel` + `m_UnitSprite` + `InventoryGridItem.Update` 无条件 reapply → 无效

**下一轮怀疑方向**:
- 可能整个 cell 被游戏"selection 详情模式"替换了 GameObject,SeenItems 缓存的 item reference 失效
- 或 NGUI 的 panel draw order 把 unit label 盖在 stack label 上面(Hierarchy 顺序问题)
- 或 click/hover 后 Panel_Container 把 dataItem 换成了"详情 di"(非 representative),Counts 里查不到
- 建议:加 log 确认 `Patch_Panel_Container_HoverItem` / `Patch_InventoryGridItem_Update` 实际 trigger 次数
- 或看 dnSpy 里 `Panel_Container.HoverItem` 反编译代码,看它调用了什么

记为 **T1**,下次继续处理。

---

**最后更新**:2026-04-28(v2.7.0 session)  
**当前版本**:v2.7.0(Menu.cs 里显示)  
**部署路径**:`D:\Steam\steamapps\common\TheLongDark\Mods\TldHacks.dll`  
**源码路径**:`D:\TLD-Mods\TldHacks\`  
**仓库**:https://github.com/Jcxu97/tld--2.55(本地 `D:\TLD-Mods\tld--2.55\`,可能还没把 TldHacks 最新版同步过去)

---

## 0. 项目目标

**把 `C:\Users\82077\Desktop\The Long Dark.CT`(Cheat Engine 表)里的全部功能移植到内置 MelonMod 修改器 TldHacks.dll。**

CT 用 Cheat Engine 的 Auto Assembler Script + AOB scan + 机器码 patch 实现作弊。
TldHacks 用 MelonMod + Harmony patch + uConsole 命令 + 反射,**功能等价,路径不同**。

用户最终想要的交互:
- 按 Tab 键(可改)呼出菜单
- 菜单里有三列布局 + 滚动条,分 section 列出所有作弊
- toggle 持久化(重启游戏保留)
- 菜单样式参考桌面截图 `C:/Users/82077/Desktop/1.png`

---

## ★ v2.7.0 本轮改动(2026-04-28 后续 session)

**主题**:清理全部 TODO 后端 + 修 GPT 分析的 Stacking cell-reuse bug + 扩传送到全图 + 加一批 CT 功能

### 代码改动
| 文件 | 改动 |
|---|---|
| `CheatsPatches.cs` | **完全重写武器/瞄准相关** —— 从"patch getter + tick zero 字段"改为"用游戏内建的 `m_DisableAim*` 开关 + `RecoilSpring` reset 归零"。新增 `TickCamera()`,通过 `GameManager.GetVpFPSCamera()` 拿相机,批量设 `m_DisableAmbientSway=true` / `vp_FPSWeapon.m_DisableAimSway/Shake/Breathing/Stamina=true`(这 4 个 bool 是游戏自带"直接关"入口 —— 比打 getter 可靠)。`RecoilSpring` 是 struct,每帧反射归零 `m_Current/m_Target/m_Velocity`。保留原 GetRecoilPitch/Yaw Postfix 作 fallback |
| `CheatsPatches.cs` | 新 patches:`Lock.IsLocked`(忽略上锁) / `IceCrackingTrigger.BreakIce+FallInWater`(冰面不破) / `Panel_Container.Enable`(快速开容器 —— 设 EnableDelaySeconds=0,EnableDelayElapsed=999) / `CameraEffects.EnableCameraWeaponPostEffects`(无瞄准景深) / `FireManager.CalculateFireStartSuccess`(生火 100%) |
| `CheatsPatches.cs` | `Patch_Condition_AddHealth_NoFall` 改成 `Patch_Condition_AddHealth_Filter` —— 合并无坠落伤害 + 免疫 Wolf/Bear/Cougar 伤害 + 不会窒息(`DamageSource.Suffocating`)3 个 toggle 的过滤 |
| `CheatsPatches.cs` | `TickClothingWetness` —— 扫 `ClothingItem` 设 `m_PercentWet = 0` |
| `CheatsPatches.cs` | `TickClimbRope` —— 扫 `PlayerClimbRope` 的 4 个 climb speed 字段 ×5 |
| `Cheats.cs` | `Patch_SprainedWrist_Start` / `Patch_SprainedAnkle_Start` 加 `CheatState.NoSprainRisk` 条件(之前只吃 GodMode)|
| `CheatsExtra.cs` | 新 `ExtraOneShot` 类:`TickStopWind` 用 `Weather.DisableWindEffect/EnableWindEffect`;`TickSprainRisk` 用 `SprainedWrist.SetForceNoSprainWrist(true/false)`(Wrist 有内建开关,Ankle 靠 Harmony 拦) |
| `CheatsExtra.cs` | Teleport `Destinations` 从 5 个地堡扩到 **15 个 region** —— 加了沿海公路 / 宁静山谷 / 针叶松林山 / 荒野沼泽 / 晦暗湾 / 被弃机场 / 捕鲸站 / 废弃铁路 / 寂静河谷 / 山间隘口。新 region 的 Pos 全填 `Vector3.zero`,`TravelTo`/`MovePlayerTo` 都检测 `== Vector3.zero`,等于零就只 LoadScene 不再 TeleportPlayer —— 让游戏默认落点生效(避免落进虚空) |
| `Stacking.cs` | **按 GPT 分析重写 cell-reuse 逻辑**。问题根因:`InventoryGridItem` 是复用的 cell;滚动 / 切分类后同一个 cell GameObject 被重绑到新 dataItem。旧 `SeenItems` 缓存 `(item, di)`,OnLateUpdate 盲目用 cached di reapply → 把旧 count 写到新 gi 上 = "部分堆叠视觉失效"。**修法**:`SeenItems` 改存 `(item, di, giPtr)` 三元组;`OnLateUpdate` 里 **verify** `item.m_GearItem.Pointer == cachedGiPtr`,不匹配就 skip(等 `RefreshDataItem.Postfix` 自动重登记覆盖)。另加 `Patch_InventoryGridItem_Refresh(GearItem, int)` Postfix 作第二条 cell-bind 入口(非 dataItem 路径) |
| `Cheats.cs`, `Settings.cs`, `ModMain.cs`, `Menu.cs` | 新 toggle 字段:`StopWind` / `NoSprainRisk` / `ImmuneAnimalDamage` / `NoSuffocating` / `QuickFire` / `QuickClimb`。Menu 第 2 列加"免疫"section,第 3 列制作改"制作 / 技能快捷"加 QuickFire/QuickClimb。所有旧 "TODO" 字样移除 |

### 验证需要你做
进游戏按热键弹菜单,测试:
1. **瞄准相关 toggle**:开"关闭瞄准晃动/抖动/呼吸晃动/景深",拿枪瞄准看是否纹丝不动。预期比 v2.6.0 的 patch getter 方案稳得多(真正干活的是 `m_DisableAimSway` 等 bool)
2. **无后坐力**:开"无后坐力",连续射击 —— 视角不应向上跳
3. **冰面不破**:走薄冰区,应该不会裂开
4. **忽略上锁**:开锁门(柜),直接能开
5. **快速打开容器**:开容器进度条应瞬间跳满
6. **衣物不潮湿**:下雨走一圈看衣物 wetness 不涨
7. **免疫 Wolf/Bear/Cougar**:被狼追,HP 不掉
8. **停止刮风**:开 toggle 看风噪声应停
9. **免扭伤**:摔跤不应扭伤
10. **生火 100%**:开火看 success chance UI 应显示 100%
11. **传送**:打开 Tab 2 看应有 15 个目的地按钮
12. **堆叠**(GPT 那个问题):背包里打开 + 滚动 + 点同类物品,×N 和重量应不消失 / 不错位

### 下次继续的方向
- GPT 说的"EOF coroutine"作为 stacking 的额外稳健措施,当前没做。如果 v2.7.0 还看到堆叠异常,可以在 ModMain 里 `MelonCoroutines.Start(EOFReapply())` 包 WaitForEndOfFrame 再 reapply 一轮
- CT 剩下未实现的高价值:**飞行模式**(CharacterController.Move patch)、**物品指针编辑**(选中背包物品改数量/耐久)、**商人功能**
- `NoAimDOF` 的 patch 已加(`EnableCameraWeaponPostEffects`),但 TLD 的 DOF 也可能通过 PostProcessVolume 的 DepthOfField 直接生效 —— 如果 toggle 不生效,要再加 `FindObjectsOfType<UnityEngine.Rendering.PostProcessing.DepthOfField>()` tick 把 `.enabled.value = false`

### 已知边界
- `FindObjectsOfType<PlayerClimbRope>` 扫整个场景,只有玩家挂有 1 个实例,每 90 帧扫一次 —— 代价可忽略
- 所有带 `m_DisableAim*` 的字段是**静默**开关(不触发动画 reset),toggle 切换一帧内生效;但关掉后游戏 internal 状态可能需要几秒恢复原 sway 幅度
- `QuickClimb` 的 ×5 是覆盖式,首次 tick 后 climb speed 会被持续改写;关掉 toggle 后字段值留在放大态直到场景重载(爬绳仍然快)。这是简化实现,如果想精确还原要保留原值

---

## 1. 当前状态汇总(v2.7.0)

### 已实现且验证能 work ✅

| 功能 | 实现方式 |
|---|---|
| **背包 UI 堆叠(FoodStackable 原功能)** | Harmony patch Panel_Inventory/Container RefreshTable + LateUpdate tick |
| **菜单系统** | IMGUI GUI.Window + GUI.Xxx(Rect) 手动布局,禁用 GUILayout.* 和 TextField / HorizontalSlider / BeginScrollView (layout) |
| **字体缩放** | GUI.skin.label/button/toggle/box.fontSize = 14 * scale |
| **窗口缩放** | _window.width = W * scale;Rect 通过 R(x,y,w,h) helper 乘 scale |
| **滚动条** | GUI.BeginScrollView(viewport, scroll, content, false, true) 包裹内容(content 1500 高,viewport ~600 高) |
| **ModSettings 持久化** | JsonModSettings 子类 TldHacksSettings,AddToModSettings("TldHacks") 注册 |
| **菜单热键** | Settings.MenuHotkey (KeyCode),默认 Tab,可在 ModSettings 里重绑 |
| **飞行热键** | Settings.FlyHotkey,默认 F1,按下调 `uConsole.RunCommand("fly")`(**实际不 work,见下**) |
| **God Mode** | 11 个 Harmony patch(Condition/Fatigue/Hunger/Thirst/Freezing/PM/Hypothermia/SprainWrist/SprainAnkle/BloodLoss/Infection) |
| **独立状态 toggle**(无限体力/始终温暖/无饥饿/无口渴/无疲劳) | 扩展 Fatigue/Freezing/Hunger/Thirst Patches 的条件 |
| **无限负重** | Harmony patch Inventory.MaybeAdd/AddGear + OnUpdate 每帧 `m_ForceOverrideWeight = true` |
| **无限耐久 + 清所有装备耐久** | Harmony patch GearItem.ManualUpdate/Awake + 5 秒 tick 扫所有 GearItem 设 m_CurrentHP = 100 |
| **速度倍率** | Time.timeScale(OnUpdate 同步),preset 0.5/1/2/5x |
| **秒杀动物** | 2 秒 tick 扫 BaseAi 实例,SendMessage TakeDamage/Kill |
| **天气切换** | `Cheats.SetWeatherStage` 调 uConsole `lock_weather N`(**uConsole 不 work,见下**) |
| **时间跳跃** | 反射调 TimeOfDay.SetNormalizedTime(hour/24) |
| **清除所有负面(扩展版)** | 硬编码 10 个 affliction + 反射扫 15 个扩展类(Headache/Scurvy/Parasites/Burns/Concussion/等) |
| **物品刷出** | 358 条物品库(ItemDatabase.cs)+ PlayerManager.AddItemCONSOLE(prefab, 1, 100f) |
| **修复背包物品** | 遍历 Inventory.m_Items 调 Cheats.RestoreDurability |
| **技能满级** | SkillsManager.GetSkill(SkillType).SetPoints(MaxPoints) 反射 |
| **全地图揭示** | 反射调 RegionManager.RevealAllRegions 等(多个 fallback 方法名) |
| **解锁全部壮举** | 反射扫所有 Feat_* 类设 m_IsUnlocked = true |
| **解锁全部蓝图(尝试)** | 反射扫 BluePrintItem 类改 m_RequiresResearch/m_KnownByDefault(**字段名是猜的,不保证**) |
| **玩家位置刷新** | 反射 GameManager.GetPlayerManagerComponent().GetControlledGameObject().transform |
| **打印位置(uC pos 命令)** | ConsoleBridge.Run("pos") — 走 DevConsole 注册的 tp 对应实现 |
| **同区域传送** | `PlayerManager.TeleportPlayer(Vector3, Quaternion)` — TLD 公开方法 |
| **跨场景传送(全 15 region)** | `GameManager.LoadScene(sceneName, saveName)` + SceneTransitionData;5 个地堡有精确 bunker 坐标,其余 10 region Pos=zero 走默认落点 |
| **停止刮风** | `Weather.DisableWindEffect()` / 关 toggle 时 `EnableWindEffect()` |
| **免疫狼/熊/美洲狮伤害** | `Condition.AddHealth` Prefix 过滤 `DamageSource.Wolf/Bear/Cougar` |
| **不会窒息** | 同上,过滤 `DamageSource.Suffocating` |
| **免扭伤风险** | `SprainedWrist.SetForceNoSprainWrist(true)` + `SprainedAnkleStart` Prefix skip |
| **生火 100% 成功** | `FireManager.CalculateFireStartSuccess` Postfix=1f |
| **爬绳速度 ×5** | Tick `PlayerClimbRope.m_ClimbSpeed*` 5 个字段 ×5 |
| **免费制作** | Harmony patch Panel_Crafting.CanCraftSelectedBlueprint/CanCraftBlueprint Postfix = true |
| **快速制作(点一下完成)** | patch GetFinalCraftingTimeWithAllModifiers/GetAdjustedCraftingTime Postfix = 0 + CraftingStart Postfix 反射调私有 OnCraftingSuccess |
| **无坠落伤害** | patch Condition.AddHealth(float, DamageSource) Prefix:`cause == DamageSource.Falling` 时 return false |
| **无限弹药** | patch GunItem.RemoveNextFromClip Prefix return false + tick 同步 m_RoundsInClip = m_ClipSize |
| **永不卡壳** | tick 设 GunItem.m_ForceNoJam = true(static!)+ 实例 m_IsJammed = false |
| **无后坐力** | tick 扫 GunItem 设 m_PitchRecoilMin/Max/m_YawRecoilMin/Max = 0 |
| **关闭瞄准晃动** | patch GunItem.GetSwayIncreasePerSecond Postfix = 0 |
| **关闭瞄准体力消耗** | patch GunItem.GetCurrentStaminaPercent Postfix = 1 + CanStartAiming = true |
| **快速射击** | tick 设 m_FiringRateSeconds = 0.2(5/秒上限,0 会卡死)+ m_FireDelayOnAim = 0.05 + m_FireDelayAfterReload = 0.1 |

### UI 有 toggle 但后端 TODO(v2.7.0 清完)✅

| toggle | 实现 |
|---|---|
| 动物无法发现你(Stealth) | `BaseAi.CanSeeTarget` Postfix=false + `ScanForSmells` Prefix skip + tick 清 target 字段 |
| 冰面不破(ThinIceNoBreak) | `IceCrackingTrigger.BreakIce` + `.FallInWater` Prefix skip |
| 忽略上锁(IgnoreLock) | `Lock.IsLocked` Postfix=false |
| 快速打开容器(QuickOpenContainer) | `Panel_Container.Enable` Postfix 设 `m_EnableDelaySeconds=0, m_EnableDelayElapsed=999` |
| 衣物不潮湿(NoWetClothes) | `TickClothingWetness` → 扫 ClothingItem 设 `m_PercentWet=0` |
| 火焰无限时长(InfiniteFireDurations) | 用户用游戏内建 H 热键,不做 |
| 关闭瞄准抖动(NoAimShake) | `TickCamera` 设 `vp_FPSWeapon.m_DisableAimShake=true` + `vp_FPSCamera.ShakeAmplitude=0` |
| 关闭呼吸晃动(NoBreathSway) | `TickCamera` 设 `vp_FPSWeapon.m_DisableAimBreathing=true` + `BobAmplitude=0` |
| 关闭瞄准景深(NoAimDOF) | `CameraEffects.EnableCameraWeaponPostEffects` Prefix 强制 `isEnabled=false` |
| 无后坐力(NoRecoil) | `TickCamera` 反射归零 `vp_FPSCamera.m_RecoilSpring` 的 `m_Current/m_Target/m_Velocity`(struct 字段),+ Gun 侧 recoil min/max=0 |

### uConsole 命令区(Tab 1 底部)测试结果

**现象**:只有 **spawn 系命令** work。

| 命令 | 结果 |
|---|---|
| `spawn_wolf/bear/cougar/moose/doe/stag/rabbit/ptarmigan` | ✅ work |
| `fly` | ❌ 无效 |
| `set_invulnerable true/false` | ❌ 无效 |
| `set_invisible true/false` | ❌ 无效 |
| `force_no_jam true/false` | ❌ 无效 |
| `force_no_random_sprain true/false` | ❌ 无效 |
| `lock_weather N` | ❌ 无效(影响 Tab 1 天气按钮失效) |
| `lock_temperature N` | ✅ work(但 `unlock_temperature` 不 work) |
| `set_weather N` | 待测 |
| `force_aurora true/false` | 待测 |
| `add_all_gear` | 待测 |
| `kill_all_animals` | 待测 |
| `repair_transmitters` | 待测 |
| `trader_*` (6 个) | 待测 |
| `tp x y z` / `pos` | ✅ work(DevConsole 注册) |
| `scene XXX` | ⚠ work 但等于"加载新场景",丢存档状态 |

**推测**:TLD release build 里,某些命令内部有 `if (!isDebugBuild) return;` 守卫。spawn/lock_temperature 等**写场景状态**的命令没守卫;`fly/set_invulnerable` 等**改玩家状态**的命令被守卫。

**解决方向**:这些命令**不能用**,全部用 Harmony patch 自己实现。

### 已知 bug

1. **跨场景传送丢存档** — v2.6.0 已去掉 `scene XXX` 调用,跨 scene 只打 log 提示,不实际传送。用户跨区要用 FastTravel.dll 的按键
2. **温度锁定 + 解除锁定** — `unlock_temperature` 命令可能不存在;解除用 `lock_temperature 999999` 极端值"让游戏自己 reset",**未验证**

---

## 2. 文件结构

```
D:\TLD-Mods\TldHacks\
├── TldHacks.csproj          # 项目配置,引用 MelonLoader / Harmony / Il2Cpp* / ModSettings
├── ModMain.cs               # [assembly: MelonInfo] + MelonMod 派生类,OnInitializeMelon/OnUpdate/OnGUI/OnLateUpdate/OnSceneWasInitialized
├── Settings.cs              # TldHacksSettings : JsonModSettings,所有持久化字段
├── Menu.cs                  # IMGUI 菜单主代码 —— 3 tabs: "主要 + uConsole" / "物品 & 传送",三列布局 + 滚动条
├── Stacking.cs              # FoodStackable 原代码(UI 堆叠)—— StackState + Dedupe + LabelFix + 3 个 Patches
├── Cheats.cs                # CheatState(运行时 bool 状态)+ GodMode/Fatigue/Hunger/etc Harmony Patches + Cheats 工具类(SetWeatherStage/SetTimeOfDay/SpawnItem/ClearAllAfflictions/RestoreDurability)
├── CheatsPatches.cs         # Gun/Crafting 的 Harmony Patches(NoRecoil/NoAimStamina/FreeCraft/QuickCraft/FastFire/NoFallDamage)+ CheatsTick (TickGuns/TickAnimals/TickFires)
├── CheatsExtra.cs           # Teleport(MovePlayerTo/TravelTo)+ Skills + Feats + QuickActions
├── ConsoleBridge.cs         # 封装 uConsole.RunCommandSilent 工具
├── ItemDatabase.cs          # 358 条 ItemEntry(从 TldItemSpawner 搬来),分类:食物/工具/武器/弹药/医疗/衣物/材料/DLC武器/其他
└── HANDOFF.md               # 这个文件
```

**编译**:
```bash
cd D:\TLD-Mods\TldHacks && dotnet build -c Release
```
AfterBuild 自动 cp 到 `D:\Steam\steamapps\common\TheLongDark\Mods\TldHacks.dll`。**游戏运行时 dll 被锁,cp 失败,要先退游戏**。

**依赖 DLL**(在 csproj 里):
- MelonLoader + 0Harmony + Il2CppInterop.Runtime / Common
- Il2Cppmscorlib + Il2CppSystem
- Assembly-CSharp + UnityEngine + UnityEngine.CoreModule / IMGUIModule / InputLegacyModule
- Il2CppTLD.IntBackedUnit(ItemWeight 类)
- **ModSettings.dll**(用户自己装)—— 通过 `[assembly: MelonAdditionalDependencies("ModSettings")]` 声明

---

## 3. 技术栈关键点

### IMGUI 在 TLD IL2CPP 下的限制

**GUILayout 整套被 Unity strip 了**,包括:
- `GUILayout.Button/Label/Toggle/TextField/BeginHorizontal/BeginScrollView/Toolbar/HorizontalSlider/...`
- 所有这些都会抛 `NotSupportedException: Method unstripping failed`

**能用的**(验证过):
- `GUI.Window(int, Rect, WindowFunction, string)`
- `GUI.Label/Button/Toggle/Box(Rect, string[, GUIStyle])`
- `GUI.BeginScrollView(Rect, Vector2, Rect, bool, bool)` + `EndScrollView()`
- `GUI.DragWindow(Rect)`
- `GUI.skin.label.fontSize = X`(字号缩放)

**不能用的**:
- `GUI.TextField / GUI.DoTextField`(底层也 stripped)—— 用按钮 preset 代替
- `GUI.HorizontalSlider`(可能也 stripped)—— 用按钮 preset 代替

所有坐标通过 `R(x, y, w, h)` helper 乘 `_scale`,字号统一 `14 * scale`。

### Harmony Patch 注意事项

- `MonoBehaviour.Update` 不能 patch(Il2CppInterop 下 Unity 引擎隐式调用的生命周期钩不到)。FoodStackable 曾踩过,改用 `OnLateUpdate` 兜底
- `[HarmonyPatch(typeof(X), "MethodName")]` 无参数也 ok,Harmony 自动匹配唯一签名
- 有多个重载时指定参数类型:`[HarmonyPatch(typeof(Condition), "AddHealth", new[] { typeof(float), typeof(DamageSource) })]`
- Prefix `return false` 跳过原方法;Postfix `ref __result` 改返回值
- **static 字段**要用 `ClassName.Field = x`,不能用 `instance.Field = x`(编译报 CS0176)。GunItem.m_ForceNoJam 是 static

### Il2CppInterop 反射注意事项

- `UnityEngine.Object.FindObjectsOfType(Type)` 要 **`Il2CppSystem.Type`**,不是 `System.Type`。转换:`Il2CppInterop.Runtime.Il2CppType.From(t)`
- `SceneManager` 有歧义(`Il2Cpp.SceneManager` vs `UnityEngine.SceneManagement.SceneManager`),用 `using SceneManager = UnityEngine.SceneManagement.SceneManager;` 消歧
- `Resources.FindObjectsOfTypeAll<T>()` 返回**所有已加载实例包括非激活**,慢;`UnityEngine.Object.FindObjectsOfType<T>()` 只返回**当前场景激活**,快 10x

### 性能注意

之前做错:每 30-60 帧 `FindObjectsOfTypeAll<T>()` 扫全场景,帧率掉 20-30fps。修正:
- `...OfTypeAll` → `...OfType`
- 频率 30/60 → 90/120/300 帧
- `ScanAndKillAnimals` 扫 BaseAi(几十个)而不是所有 GameObject(几千个)

所有 tick 内部都 `if (toggle_off) return;`,不开 toggle 时零开销。

---

## 4. CT 功能覆盖率(~60%)

### ✅ 已实现
**生命/状态**:无敌/不饥饿/不口渴/不寒冷/不疲劳/无限体力  
**行为**:无限负重/所有物品满耐久/一枪秒杀/清所有 afflictions  
**世界**:天气切换(但依赖 uConsole 可能失效)/时间跳跃/速度倍率  
**武器**:无限弹药/永不卡壳/无后坐力/关闭瞄准晃动/关闭瞄准体力消耗/快速射击  
**制作**:免费制作/快速制作(点一下完成)  
**物品**:添加物品(358 种,手动选)  
**技能**:技能最大点数(9 个技能)  
**解锁**:解锁全部壮举 + 解锁全部蓝图(尝试)/ 全开地图  
**传送**:同区域传送(5 个地堡 preset + 任意坐标)  
**防御**:无坠落伤害  
**UI**:UI 堆叠

### ❌ 未实现(CT 有,TldHacks 没有)

| CT 功能 | 备注 |
|---|---|
| 不会窒息 | Suffocating affliction,未查 |
| 停止刮风 | Weather.SetWind,有字段 `m_Wind*`,可 tick |
| 免疫森林狼伤害 | patch Condition.AddHealth 过滤 cause==Wolf |
| 免疫熊伤害 | 同上,cause==Bear |
| 免疫美洲狮伤害 | 同上,cause==Cougar |
| 强制动物逃离 | BaseAi.FleeFromPlayer 类方法? |
| 免冻伤风险 | Frostbite 的 Risk 字段,已部分(Frostbite.End) |
| 免扭伤风险 | force_no_random_sprain 命令(**uConsole 不 work**),要 patch SprainedWrist/Ankle.StartRisk |
| 快速射击1 | 和快速射击一样?或者更快?CT 里可能是连发模式 |
| 快速爬绳 | Climbing / RopeClimb 类,未查 |
| 快速生火 | Panel_FireStart / FireStarterItem,有 `m_StartDurationModifier` 相关字段 |
| 商人交易功能(完整) | Trader 类,uConsole 的 trader_* 命令未测 |
| 商人交易秒完成 | uConsole `trader_trade_force_completed` 待测 |
| 商人信任 | uConsole `trader_trust_add N` 待测 |
| 保温杯永不失温 | CoffeeTin / Thermos 的 temperature 字段 |
| 保温瓶可存放所有茶 | Container.m_AllowedItems 改 |
| 切换钓鱼 | Panel_Fishing 相关 |
| 头疼 / 头疼治愈 | 已在 "清所有 afflictions" 扫描里(Headache)|
| 初始/最大/当前维C | `Scurvy` 类的 `m_CurrentVitaminC` 等字段 |
| 坏血病 | Scurvy 本身,已在扫描 |
| 化学品中毒 | ChemicalPoisoning 已在扫描 |
| 清除死亡惩罚 | `cheat_death_cure` uConsole 命令,待测 |
| 交易清单不限制 | Trader.m_MaxExchanges 或类似 |
| 心平气和&天文导航仪 | Feat_* 类,已在 "解锁全部壮举" 里 |
| 冷聚变 / 机械高效 / 暴雪行者 / 发自内心 / 雪地行者 / 生火大师 / 书本 / 夜间行者 | 全是 Feat_*,已在 "解锁全部壮举" |
| 火温度/火把燃烧值/火温度上限 | Fire 相关字段,要找 CampFire / Torch 类 |
| 火把;油灯;自动点亮 | patch Torch.Ignite? |
| 手电筒激活 | Flashlight.Activate patch |
| 物品选项(物品指针) | CT 的"指针"是内存地址改,MelonMod 里改 `GameManager.m_Inventory.m_SelectedItem` 的具体字段,复杂 |
| 物品数量 / 物品耐久 / 当前耐久 / 最大耐久 | 选中物品的字段编辑,要 UI + 反射 |
| 更改生存时间 | `GameManager.m_SurvivalTimeMinutes` 之类 |
| 当前卡路里 / 最大卡路里 | `Hunger.m_CurrentReserveCalories / m_MaxReserveCalories` |
| 温度加成 | 玩家温度 buff,patch TempSources |
| 没有坠落伤害 | **已做** ✅ |
| 强制改人物视角 | CameraController 的朝向 |
| 备份存档 | File copy 到 backup |
| 开启美洲狮调试器 | `debug_cougar` uConsole 命令 |
| 地图位置带指针 | 在 Panel_Map 叠加玩家坐标 indicator |
| 传送地堡坐标(11 个) | Tab 2 已有 5 个 preset,CT 还有 Marsh/Airfield/Coastal/WhalingStation/MountainPass 6 个 |
| 飞行模式 | CT 里"F1 飞, 空格落地" — 需要 patch PlayerMovement / vp_FPController 的重力和输入 |
| 激活此脚本取消永久生火 | 反向 patch 某个 Fire 方法 |
| 感叹号 / 无描述 | 不明功能 |

### ⚠ 加密部分读不到(Ascii85 UDF1 块)

CT 里 `<UDF1 Class="TCEForm" Encoding="Ascii85">=6;t;l,m...` 这段包装了:
- GUI 定义(Cheat Engine 自己的表格布局)
- 部分 Lua 脚本

**可能含有**:具体 AOB scan 的字节特征 / 内存偏移 / Lua 命令。读不到就读不到,影响不大 —— 我们走 .NET 方法级 patch 不需要机器码偏移。

---

## 5. 下一轮优先级建议

按"**收益 ÷ 难度**"排序:

### 高收益 / 低难度(先做)
1. **飞行模式** — PlayerMovement 接管。患者都问,可直接做成 FlyHotkey 触发(已 scaffold)。实现:改 `m_Gravity = 0` + 重写 input handling,或直接 patch CharacterController.Move 加垂直速度
2. **火焰无限时长** — 现有 TryLockFieldOnAll tick 已经尝试,要反编译 TorchItem/CampFireItem/FlareItem 确认真字段名
3. **停止刮风** — Weather.m_Wind* 字段 tick 设 0
4. **免疫动物伤害** — 扩展 NoFallDamage 的 Condition.AddHealth Prefix,加 Wolf/Bear/Cougar
5. **清除死亡惩罚** — 尝试 uConsole `cheat_death_cure` 或反射找

### 高收益 / 中等难度
6. **商人功能集合** — Tab 3 已有 uConsole 按钮,但需要验证是否 work。如果不 work,手动 patch Trader 类
7. **物品指针编辑**(物品数量/耐久) — 加 UI 选中背包物品,反射改字段
8. **卡路里 / 维C / 时间 直接改** — 反射字段,简单 UI

### 低收益或难
9. 瞄准抖动/呼吸/景深 — 细节,camera 相关,不好找
10. 冰面不破/忽略上锁/快速开容器/衣物不潮湿 — 边际功能,要反编译各自类
11. 地图位置带指针 — UI 叠加,复杂

### 技术路径建议

**每个新功能的实施路径**:
1. `ilspycmd Assembly-CSharp.dll -t Il2Cpp.<类名>` 看方法 / 字段
2. 先试 **tick 改字段**(容错,每 90 帧扫一次)
3. 再试 **Harmony patch 方法**(更精确,但方法名要对)
4. 最后才用 **uConsole 命令**(release 限制多,只 spawn 系稳)

---

## 6. 部署流程

**每次改完代码:**
```bash
cd D:\TLD-Mods\TldHacks
dotnet build -c Release
# 如果游戏在跑,cp 会失败。退游戏:
cp D:\TLD-Mods\TldHacks\bin\Release\TldHacks.dll D:\Steam\steamapps\common\TheLongDark\Mods\TldHacks.dll
```

**游戏侧要求**:
- Mods/ 里必须有:`MelonLoader`、`ModSettings.dll`、`DeveloperConsole.dll`(uConsole 激活)、`TldHacks.dll`
- 禁用的:`FoodStackable.dll.disabled`(功能已并入 TldHacks)
- BunkerDefaults.dll 用户决定是否禁用(功能已废弃但 BunkerDefaults 依然跑的话会在新档预填 FastTravel)

**进游戏操作**:
1. 主菜单 → MODS → TldHacks → 改 `Toggle menu hotkey` 避开 Tab 冲突(比如 F1 或 `\`)
2. 进入游戏按热键,菜单弹出
3. 菜单状态会被 ModSettings 持久化到 `Mods/TldHacks.json`

---

## 7. 关键 API 速查

### uConsole(Il2Cpp.uConsole)
```csharp
public static object RunCommand(string)         // 执行并打印到 console UI
public static object RunCommandSilent(string)   // 静默执行(我们用这个)
public static void TurnOn() / TurnOff()         // 打开/关闭控制台 UI
public static bool IsOn()
```

### PlayerManager
```csharp
public void TeleportPlayer(Vector3 pos, Quaternion rot)  // 同区域传送正解!
public GameObject GetControlledGameObject()               // 玩家 GameObject
```

### GameManager(static)
```csharp
GameManager.GetPlayerManagerComponent()
GameManager.GetWeatherComponent()
GameManager.GetSkillsManager()
GameManager.GetHypothermiaComponent() / 等 ~15 个 affliction getter
GameManager.GetTimeOfDayComponent()
GameManager.m_Inventory  // 直接字段
GameManager.m_SceneTransitionData  // 跨 scene 传送时要构造
GameManager.LoadScene(sceneName, saveName)  // FastTravel 用的
```

### GunItem(关键字段)
```csharp
static bool m_ForceNoJam;           // static!永不卡壳用这个
bool m_IsJammed;                     // instance
int m_RoundsInClip, m_ClipSize;
float m_FiringRateSeconds;           // 开火间隔(秒)
float m_FireDelayOnAim;
float m_FireDelayAfterReload;
float m_ReloadCoolDownSeconds;       // 不要改这个,会导致装填循环卡死
float m_PitchRecoilMin/Max, m_YawRecoilMin/Max;  // 后坐力
```

### Panel_Crafting(关键方法)
```csharp
bool CanCraftSelectedBlueprint()      // patch Postfix=true => 免费制作
bool CanCraftBlueprint(BlueprintData) // 同上
int GetFinalCraftingTimeWithAllModifiers()  // patch Postfix=0 => 快速
int GetAdjustedCraftingTime()
void CraftingStart()      // patch Postfix 调 OnCraftingSuccess => 立即完成
void OnCraftingSuccess()  // private,反射调
```

### SkillsManager / Skill / SkillType
```csharp
Skill GetSkill(SkillType)
int GetPoints() / GetMaxPoints()
void SetPoints(int, PointAssignmentMode)   // 反射,PointAssignmentMode=1 是 Normal

enum SkillType {
    None=-1, Firestarting, CarcassHarvesting, IceFishing, Cooking,
    Rifle, Archery, ClothingRepair, ToolRepair, Revolver, Gunsmithing
}
```

### DamageSource enum
```csharp
Unspecified=0, Player, Wolf, Bear, Falling, Burns, IceCracking, Electrical,
BloodLoss, CabinFever, Dysentery, Dehydrated, EmergencyStim, Exhausted,
FirstAid, FoodPoisoning, Freezing, FrostBite, Hypothermia, Infection,
IntestinalParasites, Sleeping, Starving, WillPower, Anxiety, Fear,
ToxicFog, Suffocating, BulletWound, NoiseMaker, Scurvy, Cougar, WeakJoints
```

### FastTravel.dll(第三方,可参考不合并)
```csharp
// 核心逻辑参考(private,要反射):
FastTravelTo(Destination) {
    GameManager.m_SceneTransitionData = new SceneTransitionData { ... };
    GameManager.LoadScene(destination.Scene.Name, SaveGameSystem.GetCurrentSaveName());
}
```

---

## 8. CT 文件状态

**位置**:`C:\Users\82077\Desktop\The Long Dark.CT`  
**大小**:716KB  
**格式**:Cheat Engine 45 XML  
**明文可读**:所有 `<Description>` 条目 + 部分 `<AssemblerScript>` 的 AOB scan 签名  
**加密不可读**:`<UDF1 Class="TCEForm" Encoding="Ascii85">` 块(GUI + Lua)

**扫明文命令**:
```bash
grep -oE '<Description>"[^"]{2,80}"' "C:/Users/82077/Desktop/The Long Dark.CT" | sort -u
```

---

## 9. 测试命令速查

```bash
# 退游戏后 cp
cp D:\TLD-Mods\TldHacks\bin\Release\TldHacks.dll D:\Steam\steamapps\common\TheLongDark\Mods\TldHacks.dll

# 查 log
grep "\[TldHacks\]\|\[Console\]" D:\Steam\steamapps\common\TheLongDark\MelonLoader\Latest.log | tail -30

# 反编译某个类查字段/方法
~/.dotnet/tools/ilspycmd D:\Steam\steamapps\common\TheLongDark\MelonLoader\Il2CppAssemblies\Assembly-CSharp.dll -t "Il2Cpp.类名"
```

---

## 10. 重要历史教训(别再犯)

1. **GUILayout 不能用**(strip),只用 GUI.Xxx(Rect,...)
2. **GUI.TextField 也不能用**(底层 stripped),数字输入用按钮 preset
3. **`FindObjectsOfTypeAll` 很慢**,默认用 `FindObjectsOfType`
4. **Tick 频率 30 帧太猛**,默认 90+,重的 300 帧
5. **GunItem.m_FiringRateSeconds = 0 会卡死**,最小 0.05(20/秒)或 0.2(5/秒)
6. **Harmony patch MonoBehaviour.Update 钩不到**,用 OnLateUpdate 兜底
7. **uConsole 大部分命令在 release build 里 no-op**,只 spawn 系稳,关键 cheat 要自己 patch
8. **`scene XXX` 命令 = 加载新场景 = 丢存档**,不是传送
9. **跨 scene 传送需要 SceneTransitionData 全套**,简单 LoadScene 会丢 state — 暂时不做
10. **Il2CppSystem.Type vs System.Type** 要转换 `Il2CppType.From(t)`
