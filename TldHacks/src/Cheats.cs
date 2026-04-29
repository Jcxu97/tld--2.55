using System;
using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TldHacks;

// 所有作弊相关的运行时状态。静态,方便 Harmony patch 里 access
// 状态写入由 Menu / Settings 触发,OnInitializeMelon 会从 Settings 同步过来
internal static class CheatState
{
    // Life / 生命
    public static bool GodMode;
    public static bool NoFallDamage;
    // Status / 状态(注:InfiniteStamina 去掉 —— UniversalTweaks / 其他 mod 已覆盖)
    public static bool AlwaysWarm;
    public static bool NoHunger;
    public static bool NoThirst;
    public static bool NoFatigue;
    // Movement / 移动(注:InfiniteCarry 去掉 —— 其他 mod 已覆盖)
    public static float SpeedMultiplier = 1f;
    // Animals / 动物
    public static bool InstantKillAnimals;
    public static bool FreezeAnimals;
    public static bool Stealth;         // 动物自动逃跑(Flee mode)
    public static bool TrueInvisible;   // 真·隐身(AI 检测不到玩家,不会主动扑也不会逃)
    // World
    public static bool ThinIceNoBreak;
    public static bool IgnoreLock;
    public static bool QuickOpenContainer;
    // Items / Fire
    public static bool InfiniteDurability;
    public static bool NoWetClothes;
    // InfiniteFireDurations 去除 —— 其他 mod 已覆盖
    // Crafting
    public static bool FreeCraft;
    public static bool QuickCraft;
    // Weapons
    public static bool InfiniteAmmo;
    public static bool NoJam;
    public static bool NoRecoil;
    // Aiming
    public static bool NoAimSway;
    public static bool NoAimShake;
    public static bool NoBreathSway;
    public static bool NoAimStamina;
    public static bool NoAimDOF;
    // Environment / body
    public static bool StopWind;
    public static bool NoSprainRisk;
    public static bool ImmuneAnimalDamage; // Wolf/Bear/Cougar 攻击不扣血
    public static bool NoSuffocating;      // 不会窒息
    // Skills / shortcuts
    public static bool QuickFire;          // 生火 100% 成功
    public static bool QuickClimb;         // 爬绳速度 ×5
    public static bool QuickAction;        // 采集 / 修理 / 拆解 自动时间加速
    // CT 复刻 v2.7.45+
    public static bool QuickCook;          // 秒烤肉
    public static bool QuickSearch;        // 秒搜索
    public static bool QuickHarvest;       // 秒割肉
    public static bool QuickBreakDown;     // 秒打碎
    public static bool UnlockSafes;        // 解锁保险箱/门/柜子
    public static bool LampFuelNoDrain;    // 防风油灯油不减
    public static bool FlaskNoHeatLoss;    // 保温杯永不失温
    public static bool FlaskInfiniteVol;   // 保温杯存放无限
    public static bool FlaskAnyItem;       // 保温瓶装任意
    public static bool QuickEvolve;        // 加工秒完成
    public static bool InfiniteContainer;  // 容器无限
    public static bool FireTemp300;        // 篝火 300℃
    public static bool FireNeverDie;       // 篝火永不熄
    public static bool CureFrostbite;      // 治愈永久冻伤
    public static bool ClearDeathPenalty;  // 清除死亡惩罚
    public static bool QuickFishing;       // 钓鱼 100%
    // v2.7.55 商人 + 美洲狮
    public static bool TraderUnlimitedList;   // 交易清单上限 → 64
    public static bool TraderMaxTrust;        // 信任拉满
    public static bool TraderInstantExchange; // 交易秒完成
    public static bool TraderAlwaysAvailable; // 随时可联系商人
    public static bool CougarInstantActivate; // 新档首次立即激活美洲狮
    // v2.7.58 与 ItemPicker mod 交互 —— 不让 W 键自动拾取捡回玩家自己丢的物品
    public static bool BlockAutoPickupOwnDrops;
    // Display
    public static string PositionText = "";
    // v2.7.29:LastActionLog 截断到 200 字符避免 UI 溢出
    private static string _lastActionLog = "";
    public static string LastActionLog
    {
        get => _lastActionLog;
        set => _lastActionLog = (value != null && value.Length > 200) ? value.Substring(0, 200) : (value ?? "");
    }

    // v2.7.60 删除:4 个 C* (CInvulnerable/CInvisible/CNoJamConsole/CNoSprain)
    //   都是 uConsole-based,release build 不 work,字段永远 false 且无人读 —— 死代码
    // ⚠ CFly 保留:`fly` 命令在用户的游戏里实际 work(F1 切换)—— 删除会误伤
    public static bool CFly;

    // Fast fire(tick 类,持久化)
    public static bool FastFire;
}

// 工具函数:刷物品/清 affliction/天气/时间/耐久恢复
internal static class Cheats
{
    // v2.7.57 换实现 —— AddItemCONSOLE 对部分物品(DLC / 特殊)返回 null 无响应(console 守卫)
    //   稳定公开 API:GearItem.LoadGearItemPrefab(name) → PlayerManager.InstantiateItemInPlayerInventory
    //   DLC 物品、特殊物品、release build 都 work
    public static void SpawnItem(string prefabName, int quantity)
    {
        try
        {
            var pm = GameManager.GetPlayerManagerComponent();
            if (pm == null) { ModMain.Log?.Warning("[Spawn] PlayerManager not ready"); return; }

            // 1) 先 LoadGearItemPrefab —— TLD 自己用这个方法加载 prefab,稳定
            GearItem prefab = null;
            try { prefab = GearItem.LoadGearItemPrefab(prefabName); } catch { }

            if (prefab == null)
            {
                // 2) fallback:AddItemCONSOLE(和以前一样,某些物品可能成功)
                try
                {
                    var r = pm.AddItemCONSOLE(prefabName, quantity, 100f);
                    if (r != null)
                    {
                        ModMain.Log?.Msg($"[Spawn] CONSOLE fallback +{quantity} {prefabName}");
                        return;
                    }
                }
                catch { }
                ModMain.Log?.Error($"[Spawn] prefab not found: {prefabName}");
                CheatState.LastActionLog = $"刷物品失败:{prefabName}(prefab 未找到)";
                return;
            }

            // 3) 用 InstantiateItemInPlayerInventory —— flags=0 即 InventoryInstantiateFlags.None
            try
            {
                pm.InstantiateItemInPlayerInventory(prefab, quantity, 100f, PlayerManager.InventoryInstantiateFlags.None);
                ModMain.Log?.Msg($"[Spawn] +{quantity} {prefabName}");
                CheatState.LastActionLog = $"已刷 ×{quantity} {prefabName}";
            }
            catch (Exception ex)
            {
                ModMain.Log?.Error($"[Spawn.Instantiate] {ex.Message}");
                // 最后 fallback 再试 CONSOLE
                try { pm.AddItemCONSOLE(prefabName, quantity, 100f); } catch { }
            }
        }
        catch (Exception ex) { ModMain.Log?.Error($"[Spawn] {ex.Message}"); }
    }

    public static void ClearAllAfflictions()
    {
        int cleared = 0;
        var log = new System.Text.StringBuilder();
        // v2.7.11:放弃依赖 End() —— 许多 End 方法要求"满足治愈条件"才真正清除。
        // 直接用 wrapper 类型 setter 强置内部状态为 default,外加 End() 走一遍音效/UI 刷新。

        // —— Hypothermia ——
        try { var c = GameManager.GetHypothermiaComponent(); if (c != null) {
            bool had = c.HasHypothermia();
            try { c.m_Active = false; } catch { }
            try { c.m_ElapsedHours = 0f; } catch { }
            try { c.m_ElapsedWarmTime = 0f; } catch { }
            try { c.m_StartHasBeenCalled = false; } catch { }
            try { c.m_SuppressHypothermia = true; c.HypothermiaEnd(true); c.m_SuppressHypothermia = false; } catch { }
            if (had) { cleared++; log.Append("低温; "); }
        }} catch (Exception ex) { log.Append($"低温-err:{ex.Message}; "); }

        // —— Frostbite ——
        try { var c = GameManager.GetFrostbiteComponent(); if (c != null) {
            bool had = c.HasFrostbite();
            try { c.FrostbiteEnd(); } catch { }
            try { c.m_SuppressFrostbite = true; } catch { }
            if (had) { cleared++; log.Append("冻伤; "); }
        }} catch (Exception ex) { log.Append($"冻伤-err:{ex.Message}; "); }

        // —— CabinFever ——
        try { var c = GameManager.GetCabinFeverComponent(); if (c != null) {
            bool had = c.HasCabinFever();
            try { c.m_Active = false; } catch { }
            try { c.CabinFeverEnd(); c.ClearCabinFeverRisk(); } catch { }
            if (had) { cleared++; log.Append("幽闭症; "); }
        }} catch (Exception ex) { log.Append($"幽闭症-err:{ex.Message}; "); }

        // —— Dysentery ——
        try { var c = GameManager.GetDysenteryComponent(); if (c != null) {
            bool had = c.HasDysentery();
            try { c.m_Active = false; } catch { }
            try { c.DysenteryEnd(true); } catch { }
            if (had) { cleared++; log.Append("痢疾; "); }
        }} catch (Exception ex) { log.Append($"痢疾-err:{ex.Message}; "); }

        // —— FoodPoisoning ——
        try { var c = GameManager.GetFoodPoisoningComponent(); if (c != null) {
            bool had = c.HasFoodPoisoning();
            try { c.m_Active = false; } catch { }
            try { c.FoodPoisoningEnd(true); } catch { }
            if (had) { cleared++; log.Append("食物中毒; "); }
        }} catch (Exception ex) { log.Append($"食物中毒-err:{ex.Message}; "); }

        // —— SprainedWrist / Ankle ——
        try { var c = GameManager.GetSprainedWristComponent(); if (c != null) {
            bool had = c.HasSprainedWrist();
            try { c.SetForceNoSprainWrist(true); } catch { }
            // 没 Active 字段,直接 End(0)
            try { c.SprainedWristEnd(0, (AfflictionOptions)0); } catch { }
            try { c.SprainedWristEnd(1, (AfflictionOptions)0); } catch { } // 另一只手
            if (had) { cleared++; log.Append("扭腕; "); }
        }} catch (Exception ex) { log.Append($"扭腕-err:{ex.Message}; "); }

        try { var c = GameManager.GetSprainedAnkleComponent(); if (c != null) {
            bool had = c.HasSprainedAnkle();
            try { c.SprainedAnkleEnd(0, (AfflictionOptions)0); } catch { }
            try { c.SprainedAnkleEnd(1, (AfflictionOptions)0); } catch { }
            if (had) { cleared++; log.Append("扭踝; "); }
        }} catch (Exception ex) { log.Append($"扭踝-err:{ex.Message}; "); }

        // —— BloodLoss ——
        try { var c = GameManager.GetBloodLossComponent(); if (c != null) {
            bool had = c.HasBloodLoss();
            // 对每个身体部位都 End
            for (int i = 0; i < 6; i++) try { c.BloodLossEnd(i, (AfflictionOptions)0); } catch { }
            if (had) { cleared++; log.Append("出血; "); }
        }} catch (Exception ex) { log.Append($"出血-err:{ex.Message}; "); }

        // —— Infection ——
        try { var c = GameManager.GetInfectionComponent(); if (c != null) {
            bool had = c.HasInfection();
            for (int i = 0; i < 6; i++) try { c.InfectionEnd(i); } catch { }
            if (had) { cleared++; log.Append("感染; "); }
        }} catch (Exception ex) { log.Append($"感染-err:{ex.Message}; "); }

        // —— BrokenRib ——
        try { var c = GameManager.GetBrokenRibComponent(); if (c != null) {
            bool had = c.HasBrokenRib();
            for (int i = 0; i < 6; i++) try { c.BrokenRibEnd(i, true); } catch { }
            if (had) { cleared++; log.Append("骨折; "); }
        }} catch (Exception ex) { log.Append($"骨折-err:{ex.Message}; "); }

        // —— SprainPain (扭伤疼痛 / "疼痛"主来源) —— v2.7.21 新增
        try { var c = GameManager.GetSprainPainComponent(); if (c != null) {
            bool had = c.HasSprainPain();
            try { c.Cure(); } catch { }
            if (had) { cleared++; log.Append("疼痛; "); }
        }} catch (Exception ex) { log.Append($"疼痛-err:{ex.Message}; "); }

        // —— Headache (头痛) —— v2.7.21 新增
        try { var c = GameManager.GetHeadacheComponent(); if (c != null) {
            bool had = c.HasHeadache();
            try { c.Cure(); } catch { }
            if (had) { cleared++; log.Append("头痛; "); }
        }} catch (Exception ex) { log.Append($"头痛-err:{ex.Message}; "); }

        // —— Burns (烧伤) —— v2.7.21 新增
        try { var c = GameManager.GetBurnsComponent(); if (c != null) {
            bool had = c.HasBurns();
            try { c.m_Active = false; } catch { }
            try { c.BurnsEnd(true); } catch { }
            if (had) { cleared++; log.Append("烧伤; "); }
        }} catch (Exception ex) { log.Append($"烧伤-err:{ex.Message}; "); }

        // —— IntestinalParasites (肠寄生虫) —— v2.7.21 新增
        try { var c = GameManager.GetIntestinalParasitesComponent(); if (c != null) {
            bool had = c.HasIntestinalParasites();
            try { c.m_HasParasites = false; c.m_HasParasiteRisk = false; } catch { }
            try { c.IntestinalParasitesEnd(true); } catch { }
            if (had) { cleared++; log.Append("肠寄生虫; "); }
        }} catch (Exception ex) { log.Append($"肠寄生虫-err:{ex.Message}; "); }

        string summary = cleared > 0 ? $"已清 {cleared} 项: {log}" : $"未检测到活跃负面; 尝试过: {log}";
        ModMain.Log?.Msg($"[Cheats] {summary}");
        CheatState.LastActionLog = summary;
    }

    // 真正切天气:WeatherTransition.ActivateWeatherSetImmediate(WeatherStage)
    // 这是 public 方法,不走 uConsole(release build 下 no-op)。
    // 从场景里找 WeatherTransition 实例 —— 反射调 GameManager 或 FindObjectOfType 兜底
    public static void SetWeatherStage(int stage)
    {
        try
        {
            WeatherTransition wt = null;

            // 尝试 GameManager.GetWeatherTransitionComponent()(如果存在)
            try
            {
                var m = typeof(GameManager).GetMethod("GetWeatherTransitionComponent", BindingFlags.Static | BindingFlags.Public);
                if (m != null) wt = m.Invoke(null, null) as WeatherTransition;
            }
            catch { }

            // 兜底:场景里找
            if (wt == null)
            {
                try { wt = UnityEngine.Object.FindObjectOfType<WeatherTransition>(); } catch { }
            }

            if (wt == null)
            {
                ModMain.Log?.Warning("[Weather] WeatherTransition not found in scene");
                CheatState.LastActionLog = "[Weather] 没找到 WeatherTransition";
                return;
            }

            // WeatherStage 是 enum,int 可以直接转
            WeatherStage stageEnum = (WeatherStage)stage;
            wt.ActivateWeatherSetImmediate(stageEnum);
            ModMain.Log?.Msg($"[Weather] ActivateWeatherSetImmediate({stageEnum})");
            CheatState.LastActionLog = $"[Weather] → {stageEnum}";
        }
        catch (Exception ex) { ModMain.Log?.Error($"[Weather] {ex.Message}"); }
    }

    public static void SetTimeOfDay(float hour)
    {
        try
        {
            var tod = GameManager.GetTimeOfDayComponent();
            if (tod == null) { ModMain.Log?.Error("[Cheats.Time] ToD null"); return; }
            float norm = Mathf.Clamp01(hour / 24f);
            var m1 = tod.GetType().GetMethod("SetNormalizedTime", BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(float) }, null);
            if (m1 != null) { m1.Invoke(tod, new object[] { norm }); }
            else
            {
                var m2 = tod.GetType().GetMethod("SetNormalizedTime", BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(float), typeof(bool) }, null);
                if (m2 == null) { ModMain.Log?.Error("[Cheats.Time] no SetNormalizedTime"); return; }
                m2.Invoke(tod, new object[] { norm, false });
            }
            ModMain.Log?.Msg($"[Cheats] Time → {hour:F1}h");
        }
        catch (Exception ex) { ModMain.Log?.Error($"[Cheats.Time] {ex.Message}"); }
    }

    // 每 300 帧扫一次 GearItem 刷耐久(需要 InfiniteDurability toggle on 才走 tick)
    // 手动按钮不调这个,改调 RestoreAllSceneGear 并记录 count 到 LastActionLog
    public static void TickInfiniteDurability()
    {
        try
        {
            var gears = UnityEngine.Object.FindObjectsOfType<GearItem>();
            if (gears == null) return;
            foreach (var g in gears) RestoreDurability(g);
        }
        catch { }
    }

    // 手动按钮:恢复场景所有 GearItem + 背包内所有 + 结果写 LastActionLog
    public static void RestoreAllSceneGear()
    {
        int scene = 0, bag = 0, fail = 0;
        try
        {
            var gears = UnityEngine.Object.FindObjectsOfType<GearItem>();
            if (gears != null)
                foreach (var g in gears) { if (RestoreDurability(g)) scene++; else fail++; }
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[RestoreAll.Scene] {ex.Message}"); }

        try
        {
            var inv = GameManager.m_Inventory;
            if (inv?.m_Items != null)
            {
                for (int i = 0; i < inv.m_Items.Count; i++)
                {
                    try
                    {
                        var obj = inv.m_Items[i];
                        if (obj == null) continue;
                        var gi = obj.m_GearItem;
                        if (gi != null) { if (RestoreDurability(gi)) bag++; else fail++; }
                    }
                    catch { fail++; }
                }
            }
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[RestoreAll.Bag] {ex.Message}"); }

        string msg = $"场景 {scene} 件 + 背包 {bag} 件 恢复满 (失败 {fail})";
        CheatState.LastActionLog = msg;
        ModMain.Log?.Msg($"[RestoreAll] {msg}");
    }

    // v2.7.14:用游戏官方 SetNormalizedHP(1f) 替代直接写 m_CurrentHP —— 更可靠
    internal static bool RestoreDurability(GearItem gear)
    {
        if (gear == null) return false;
        try
        {
            gear.SetNormalizedHP(1f, true);
            return true;
        }
        catch { }
        // fallback 直接字段
        try { gear.m_CurrentHP = 100f; return true; } catch { }
        return false;
    }

    // 修复玩家当前手持物品 —— 一键
    public static void RepairItemInHands()
    {
        try
        {
            var pm = GameManager.GetPlayerManagerComponent();
            if (pm == null) { CheatState.LastActionLog = "[修复手持] no PM"; return; }
            var item = pm.m_ItemInHands;
            if (item == null) { CheatState.LastActionLog = "[修复手持] 手上没东西"; return; }
            bool ok = RestoreDurability(item);
            CheatState.LastActionLog = ok ? $"[修复手持] {item.name} → 100%" : $"[修复手持失败] {item.name}";
            ModMain.Log?.Msg($"[Repair.Hands] {item.name} ok={ok}");
        }
        catch (Exception ex)
        {
            CheatState.LastActionLog = $"[修复手持异常] {ex.Message}";
            ModMain.Log?.Warning($"[Repair.Hands] {ex.Message}");
        }
    }

    // v2.7.29 删:RestoreClothingDurability 无调用点且反射 GetField 对 Il2Cpp property 无效

    // 扫场景所有 BaseAi 实例直接调 DebugKill / ApplyDamage —— 比 SendMessage 可靠
    public static void ScanAndKillAnimals()
    {
        try
        {
            var ais = UnityEngine.Object.FindObjectsOfType<BaseAi>();
            if (ais == null) return;
            int kills = 0;
            foreach (var ai in ais)
            {
                if (ai == null) continue;
                try
                {
                    // v2.7.29:BaseAi 是 Il2Cpp MonoBehaviour,直接 .gameObject 访问即可(避免 (Component) 转型在 Il2CppInterop 下抛 InvalidCastException)
                    var go = ai.gameObject;
                    if (go == null || !go.activeInHierarchy) continue;
                    // 优先 DebugKill(一步到位),失败了 fallback 到 ApplyDamage 9999
                    bool killed = false;
                    try { ai.DebugKill(); killed = true; } catch { }
                    if (!killed)
                    {
                        try { ai.ApplyDamage(9999f, 0f, DamageSource.Player, ""); killed = true; } catch { }
                    }
                    if (!killed)
                    {
                        try { ai.EnterDead(); killed = true; } catch { }
                    }
                    if (killed) kills++;
                    if (kills > 30) break;
                }
                catch { }
            }
            if (kills > 0) ModMain.Log?.Msg($"[Cheats.Kill] killed {kills}");
            CheatState.LastActionLog = $"已击杀 {kills} 只";
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[Cheats.Kill] {ex.Message}"); }
    }

    // 全地图揭示 —— 反射调 RegionManager 的各种 "RevealAll" 方法
    public static void RevealFullMap()
    {
        try
        {
            object rm = null;
            try
            {
                var instField = typeof(GameManager).GetField("Instance", BindingFlags.Static | BindingFlags.Public);
                var inst = instField?.GetValue(null);
                if (inst != null)
                {
                    var m = inst.GetType().GetMethod("GetRegionManagerComponent", BindingFlags.Instance | BindingFlags.Public);
                    rm = m?.Invoke(inst, null);
                }
            }
            catch { }
            if (rm == null) { ModMain.Log?.Warning("[Map] no RegionManager"); return; }

            foreach (var name in new[] { "RevealAllRegions", "SetAllRegionsExplored", "UnlockAllRegions", "DiscoverAllRegions" })
            {
                try
                {
                    var m = rm.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.Public);
                    if (m != null) { m.Invoke(rm, null); ModMain.Log?.Msg($"[Map] called {name}"); return; }
                }
                catch { }
            }
            ModMain.Log?.Warning("[Map] no reveal method found on RegionManager");
        }
        catch (Exception ex) { ModMain.Log?.Error($"[Map] {ex.Message}"); }
    }

    // 玩家位置,每 10 帧更新。PositionText 给 Menu 显示用
    // v2.7.29:去反射,直接调强类型 GameManager.GetPlayerTransform()
    //   之前反射 pm.GetType().GetMethod("GetControlledGameObject") 对 Il2Cpp wrapper 抛 NotSupportedException
    public static void UpdatePlayerPosition()
    {
        try
        {
            var tr = GameManager.GetPlayerTransform();
            if (tr != null)
            {
                var p = tr.position;
                CheatState.PositionText = $"X:{p.x:F1}  Y:{p.y:F1}  Z:{p.z:F1}";
            }
        }
        catch (System.Exception ex) { ModMain.Log?.Warning($"[UpdatePos] {ex.Message}"); }
    }

    // v2.7.55:在当前场景写一条 POS-MARK 到 Latest.log —— 用于采集传送坐标
    //   mod 作者事后 grep `\[POS-MARK\]` 就能拿到场景名 + 精确坐标
    public static void PrintPositionToLog()
    {
        try
        {
            var tr = GameManager.GetPlayerTransform();
            if (tr == null) { ModMain.Log?.Warning("[POS-MARK] player transform null"); return; }
            var p = tr.position;
            string scene = "?";
            try { scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name; } catch { }
            ModMain.Log?.Msg($"[POS-MARK] Scene={scene} X={p.x:F2} Y={p.y:F2} Z={p.z:F2}");
            CheatState.PositionText = $"X:{p.x:F1}  Y:{p.y:F1}  Z:{p.z:F1}";
            CheatState.LastActionLog = $"POS-MARK {scene} ({p.x:F1},{p.y:F1},{p.z:F1})";
        }
        catch (System.Exception ex) { ModMain.Log?.Warning($"[POS-MARK] {ex.Message}"); }
    }
}

// —————————————— Harmony patches ——————————————

// v2.7.25 FPS 修:删了 Condition/Fatigue/Hunger/Thirst/Freezing 的每帧 Update Postfix
// 这些本是主要 FPS 杀手 —— 每帧 5 次 Harmony detour(native→managed→native 跳转)
// 现在改成 TickStats() 每 60 帧(1s)统一 tick 写一次字段,配合 Condition.AddHealth Prefix 拦负 HP 无遗漏
// 保留 Hunger.UpdateCalorieReserves Prefix(低频方法)和 PlayerManager.MaybeFlushPlayerDamage 等

// v2.7.62 删除 Patch_Hunger_UpdateCalorieReserves 的 Prefix return false —— 会让整个 Hunger 循环
//   跳过,吃东西的 AddCalorieReserve 累加、UI 显示都受影响。改走 TickStatus 压值方案(见 TickStatus)

[HarmonyPatch(typeof(PlayerManager), "MaybeFlushPlayerDamage")]
internal static class Patch_PM_FlushDamage
{
    private static bool Prefix() => !CheatState.GodMode;
}

[HarmonyPatch(typeof(Hypothermia), "HypothermiaStart")]
internal static class Patch_Hypothermia_Start
{
    private static bool Prefix() => !CheatState.GodMode;
}

[HarmonyPatch(typeof(SprainedWrist), "SprainedWristStart")]
internal static class Patch_SprainedWrist_Start
{
    private static bool Prefix() => !(CheatState.GodMode || CheatState.NoSprainRisk);
}

[HarmonyPatch(typeof(SprainedAnkle), "SprainedAnkleStart")]
internal static class Patch_SprainedAnkle_Start
{
    private static bool Prefix() => !(CheatState.GodMode || CheatState.NoSprainRisk);
}

[HarmonyPatch(typeof(BloodLoss), "BloodLossStart")]
internal static class Patch_BloodLoss_Start
{
    private static bool Prefix() => !(CheatState.GodMode || CheatState.ImmuneAnimalDamage);
}

[HarmonyPatch(typeof(BloodLoss), "BloodLossStartOverrideArea")]
internal static class Patch_BloodLoss_StartOverride
{
    private static bool Prefix() => !(CheatState.GodMode || CheatState.ImmuneAnimalDamage);
}

[HarmonyPatch(typeof(Infection), "InfectionStart")]
internal static class Patch_Infection_Start
{
    private static bool Prefix() => !CheatState.GodMode;
}

// v2.7.15:ManualUpdate Postfix 强制 100 太粗暴 —— 用户要的是"不衰减",不是"自动满"
// 删掉 Postfix,只靠 Degrade / WearOut / DegradeOnUse 3 个 Prefix 拦衰减源头
// 想一次性拉满用"恢复全部耐久"按钮 / "修复背包物品"按钮

// InfiniteDurability 兜底:拦衰减 3 个源头 Degrade / WearOut / DegradeOnUse
[HarmonyPatch(typeof(GearItem), "Degrade", new System.Type[] { typeof(float) })]
internal static class Patch_GearItem_Degrade
{
    private static bool Prefix() => !CheatState.InfiniteDurability;
}

[HarmonyPatch(typeof(GearItem), "WearOut")]
internal static class Patch_GearItem_WearOut
{
    private static bool Prefix() => !CheatState.InfiniteDurability;
}

[HarmonyPatch(typeof(GearItem), "DegradeOnUse")]
internal static class Patch_GearItem_DegradeOnUse
{
    private static bool Prefix() => !CheatState.InfiniteDurability;
}

// Patch_GearItem_Awake 也删 —— 同样是'强制到 100',不符合用户'不损耗'要求

// Patch_Inventory_MaybeAdd / Patch_Inventory_AddGear 已去除(InfiniteCarry 功能交给其他 mod)
