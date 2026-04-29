# TldHacks — 交接文档

## 🎯 当前版本 v2.7.59 — 待测 / 需继续关注

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
