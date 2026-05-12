using System;
using System.Collections.Generic;
using System.Reflection;
using Il2Cpp;
using Il2CppInterop.Runtime;
using UnityEngine;
using SceneManager = UnityEngine.SceneManagement.SceneManager;
using LoadSceneMode = UnityEngine.SceneManagement.LoadSceneMode;

namespace TldHacks;

// 传送目的地(沿用 BunkerDefaults 的 5 个坐标 + Marsh/MountainPass 占位)
internal class Waypoint
{
    public string Label;
    public string LabelEn;
    public string Scene;
    public Vector3 Pos;
    public Waypoint(string label, string scene, float x, float y, float z, string labelEn = null)
    { Label = label; Scene = scene; Pos = new Vector3(x, y, z); LabelEn = labelEn ?? label; }
    public string DisplayLabel => I18n.IsEnglish ? LabelEn : Label;
}

internal static class Teleport
{
    // 已知可用坐标(BunkerDefaults 搬来的 5 个地堡) + 全部 15 个 region 的场景名。
    // v2.7.55 按用户实测打印的坐标更新 —— 修了 8 个原 Pos=0 的地图掉入地底问题
    //   区域中文名按 2.55 官方本地化:林狼雪岭/宜人山谷/孤寂沼地/荒芜据点/断开的铁路/破碎山道
    //   Pos == Vector3.zero 才走"只 LoadScene,不 TeleportPlayer"路径
    // v2.7.55b 参照 CT 列表补 9 个 prepper 地堡 / 应急舱 / 交易地点 —— 占位 Pos=0
    //   用户在各 scene 用"★打印坐标到 log"采准精确点后,替换这些占位行
    // v2.7.57 排序:按 scene 英文名字母序分组,同区内先主建筑、再地堡、再应急舱
    public static readonly List<Waypoint> Destinations = new()
    {
        // A —— AirfieldRegion(废弃机场)
        new Waypoint("废弃机场(控制塔)",       "AirfieldRegion",            81.90f, 160.30f, -455.30f, "Forsaken Airfield (Tower)"),
        new Waypoint("废弃机场·应急舱",         "AirfieldRegion",          1209.80f, 304.71f, -726.11f, "Forsaken Airfield · Pod"),
        // A —— AshCanyonRegion(灰烬峡谷)
        new Waypoint("灰烬峡谷地堡",            "AshCanyonRegion",          -42.12f, 172.95f, -796.68f, "Ash Canyon Bunker"),
        // B —— BlackrockRegion(黑岩监狱)
        new Waypoint("黑岩监狱地堡",            "BlackrockRegion",          705.04f, 373.98f,  816.38f, "Blackrock Bunker"),
        // C —— CanneryRegion(荒凉水湾)
        new Waypoint("荒凉水湾地堡",            "CanneryRegion",            328.37f, 344.50f,  833.16f, "Bleak Inlet Bunker"),
        new Waypoint("荒凉水湾·应急舱",         "CanneryRegion",            -39.50f, 103.38f,  501.57f, "Bleak Inlet · Pod"),
        // C —— CoastalRegion(沿海公路)
        new Waypoint("沿海公路·加油站",         "CoastalRegion",            760.51f,  24.00f,  645.93f, "Coastal Hwy · Gas Station"),
        new Waypoint("沿海公路·交易地点",       "CoastalRegion",            325.76f,  26.22f,  118.83f, "Coastal Hwy · Trader Spot"),
        // C —— CrashMountainRegion(林狼雪岭)
        new Waypoint("林狼雪岭(机舱)",         "CrashMountainRegion",      934.50f, 470.10f, 1174.40f, "Timberwolf Mtn (Plane)"),
        new Waypoint("林狼雪岭·地堡",           "CrashMountainRegion",     1675.41f, 207.32f,  968.21f, "Timberwolf Mtn · Bunker"),
        // L —— LakeRegion(神秘湖)
        new Waypoint("神秘湖·营地办公室",       "LakeRegion",              1015.87f,  25.91f,  450.86f, "Mystery Lake · Camp Office"),
        new Waypoint("神秘湖地堡",              "LakeRegion",              1029.06f,  91.99f,  -52.52f, "Mystery Lake Bunker"),
        // M —— MarshRegion(孤寂沼地)
        new Waypoint("孤寂沼地",                "MarshRegion",             1116.10f,-130.60f,  969.10f, "Forlorn Muskeg"),
        new Waypoint("孤寂沼地·地堡",           "MarshRegion",              593.07f, -83.38f, -104.89f, "Forlorn Muskeg · Bunker"),
        // M —— MiningRegion(污染区)
        new Waypoint("污染区(井架)",           "MiningRegion",            -169.20f, 201.80f,  231.70f, "Blackrock Mine (Rig)"),
        // M —— MountainPassRegion(破碎山道)
        new Waypoint("破碎山道(气象站)",       "MountainPassRegion",       532.10f, 592.10f, -601.20f, "Mountain Pass (Weather)"),
        // M —— MountainTownRegion(山间小镇)
        new Waypoint("山间小镇地堡",            "MountainTownRegion",      1828.20f, 444.39f, 1771.27f, "Milton Bunker"),
        // R —— RiverValleyRegion(寂静河谷)
        new Waypoint("寂静河谷·地堡",           "RiverValleyRegion",        363.44f, 238.61f,  375.49f, "Hushed River Valley · Bunker"),
        new Waypoint("寂静河谷·应急舱",         "RiverValleyRegion",        171.97f, 126.65f,  731.10f, "Hushed River Valley · Pod"),
        // R —— RuralRegion(宜人山谷)
        new Waypoint("宜人山谷(农庄)",         "RuralRegion",             1460.70f,  48.40f, 1032.40f, "Pleasant Valley (Farm)"),
        new Waypoint("宜人山谷·地堡",           "RuralRegion",              423.89f, 177.93f, 1458.51f, "Pleasant Valley · Bunker"),
        new Waypoint("宜人山谷·应急舱",         "RuralRegion",             1113.96f,  98.49f,  140.33f, "Pleasant Valley · Pod"),
        // T —— TracksRegion(断开的铁路)
        new Waypoint("断开的铁路(维修站)",     "TracksRegion",             588.20f, 199.00f,  565.50f, "Broken Railroad (Maint.)"),
        // W —— WhalingStationRegion(荒芜据点)
        new Waypoint("荒芜据点(孤寂灯塔)",     "WhalingStationRegion",     728.00f,  46.80f,  766.60f, "Desolation Point (Lighthouse)"),
    };

    // 正解:PlayerManager.TeleportPlayer(Vector3, Quaternion) 是公开方法。
    // 这和 DeveloperConsole 的 `tp` 命令内部调的一样。
    public static void MovePlayerTo(Vector3 pos)
    {
        try
        {
            var pm = GameManager.GetPlayerManagerComponent();
            if (pm == null) { Log("[TP] no PlayerManager"); return; }

            // 保持当前相机朝向
            Quaternion rot = Quaternion.identity;
            try
            {
                var cam = GameManager.GetVpFPSCamera();
                if (cam != null) rot = cam.transform.rotation;
            }
            catch { }

            pm.TeleportPlayer(pos, rot);
            TeleportFallGuard.Arm();
            Log($"[TP] TeleportPlayer({pos})");
        }
        catch (Exception ex) { Log($"[TP] {ex.Message}"); }
    }

    // 跨场景传送 pending state —— OnSceneWasInitialized 触发后等几帧传送
    private static Waypoint _pendingWaypoint;
    private static int _pendingFrames = 0;

    // 正式的跨场景传送 —— 参考 FastTravel.dll 流程:
    //   1) SaveGameSystem.SaveGame() 保存当前 state
    //   2) 构造 SceneTransitionData 填 from/to scene 名
    //   3) GameManager.LoadScene(sceneName, saveName) 触发场景切换
    //   4) OnSceneWasInitialized 后再等 60 帧调 TeleportPlayer 到精确坐标
    public static void TravelTo(Waypoint w)
    {
        try
        {
            var activeScene = SceneManager.GetActiveScene();
            bool hasExactCoord = w.Pos != Vector3.zero;

            if (activeScene.name == w.Scene || string.IsNullOrEmpty(w.Scene))
            {
                if (!hasExactCoord) { Log(I18n.IsEnglish ? $"[TP] → {w.LabelEn}: already in region, no exact coord" : $"[TP] → {w.Label}:已在本区,无精确坐标不传送"); return; }
                Log(I18n.IsEnglish ? $"[TP] → {w.LabelEn} (same region)" : $"[TP] → {w.Label} (同区)");
                MovePlayerTo(w.Pos);
                return;
            }

            Log(I18n.IsEnglish
                ? $"[TP] Cross-scene → {w.LabelEn} ({w.Scene}), saving + loading..."
                : $"[TP] 跨场景 → {w.Label} ({w.Scene}),开始保存 + 加载...");

            // 1) 存当前档 —— FastTravel 用的是 SaveGameSystem.SaveGame("autosave", currentSceneName) 有参版本
            //    反射先试 (string, string) 再 fallback 无参
            try
            {
                var sgt = typeof(SaveGameSystem);
                var saveTwoArg = sgt.GetMethod("SaveGame", BindingFlags.Static | BindingFlags.Public,
                    null, new[] { typeof(string), typeof(string) }, null);
                if (saveTwoArg != null)
                {
                    saveTwoArg.Invoke(null, new object[] { "autosave", activeScene.name });
                }
                else
                {
                    var saveNoArg = sgt.GetMethod("SaveGame", BindingFlags.Static | BindingFlags.Public, null, System.Type.EmptyTypes, null)
                                 ?? sgt.GetMethod("ForceSaveGame", BindingFlags.Static | BindingFlags.Public, null, System.Type.EmptyTypes, null);
                    saveNoArg?.Invoke(null, null);
                }
            }
            catch (Exception ex) { Log($"[TP.Save] {ex.Message}"); }

            // 2) 构造 SceneTransitionData —— 优先用 TransitionRecorder 记录的真实 save slot id
            //    DLC Tale scene 的 save slot id ≠ Unity scene 名,硬猜会让游戏把玩家 re-init 丢物品
            try
            {
                var snapTo   = TransitionRecorder.Lookup(w.Scene);         // 目标
                var snapFrom = TransitionRecorder.Lookup(activeScene.name); // 当前

                var std = new SceneTransitionData();
                std.m_SceneSaveFilenameCurrent  = (snapFrom != null && !string.IsNullOrEmpty(snapFrom.ToSaveId))
                                                      ? snapFrom.ToSaveId : activeScene.name;
                std.m_SceneSaveFilenameNextLoad = (snapTo != null && !string.IsNullOrEmpty(snapTo.ToSaveId))
                                                      ? snapTo.ToSaveId : w.Scene;
                std.m_TeleportPlayerSaveGamePosition = true;
                std.m_LastOutdoorScene = (snapTo != null && !string.IsNullOrEmpty(snapTo.LastOutdoor))
                                              ? snapTo.LastOutdoor : w.Scene;
                std.m_PosBeforeInteriorLoad = w.Pos;
                if (snapTo != null)
                {
                    if (!string.IsNullOrEmpty(snapTo.LocIDOverride))    std.m_SceneLocationLocIDOverride    = snapTo.LocIDOverride;
                    if (!string.IsNullOrEmpty(snapTo.SpawnPoint))       std.m_SpawnPointName                = snapTo.SpawnPoint;
                    if (!string.IsNullOrEmpty(snapTo.SpawnAudio))       std.m_SpawnPointAudio               = snapTo.SpawnAudio;
                    if (!string.IsNullOrEmpty(snapTo.ForceNextTrigger)) std.m_ForceNextSceneLoadTriggerScene = snapTo.ForceNextTrigger;
                }
                GameManager.m_SceneTransitionData = std;

                if (snapTo == null)
                    Log(I18n.IsEnglish
                        ? $"[TP.Std] ⚠ No history, fallback to scene name as save id — first visit to Tale scene may lose items"
                        : $"[TP.Std] ⚠ 无历史记录,fallback 用 scene 名当 save id — 首次去 Tale scene 可能丢物品");
                else
                    Log(I18n.IsEnglish
                        ? $"[TP.Std] Matched history transition (curId={std.m_SceneSaveFilenameCurrent}, nextId={std.m_SceneSaveFilenameNextLoad})"
                        : $"[TP.Std] 命中历史 transition (curId={std.m_SceneSaveFilenameCurrent}, nextId={std.m_SceneSaveFilenameNextLoad})");
            }
            catch (Exception ex) { Log($"[TP.Std] {ex.Message}"); }

            // 3) 标记 pending,让 OnSceneWasInitialized 在加载完后精确定位
            _pendingWaypoint = w;

            // 4) LoadScene
            try
            {
                string saveName = GetCurrentSaveName();
                GameManager.LoadScene(w.Scene, saveName);
            }
            catch (Exception ex)
            {
                Log($"[TP.Load] {ex.Message}");
                _pendingWaypoint = null;
            }
        }
        catch (Exception ex) { Log($"[TP] {ex.Message}"); }
    }

    private static string GetCurrentSaveName()
    {
        try
        {
            var m = typeof(SaveGameSystem).GetMethod("GetCurrentSaveName", BindingFlags.Static | BindingFlags.Public);
            if (m != null) { var r = m.Invoke(null, null); if (r is string s) return s; }
        }
        catch { }
        return "autosave";
    }

    // ModMain.OnSceneWasInitialized 会调这个
    public static void OnSceneLoaded(string sceneName)
    {
        if (_pendingWaypoint == null) return;
        if (sceneName != _pendingWaypoint.Scene) return;
        // 场景加载完成,等 60 帧(1 秒)让 player 生成 + 世界初始化
        _pendingFrames = 60;
    }

    // ModMain.OnUpdate 每帧调
    public static void TickPendingTeleport()
    {
        if (_pendingWaypoint == null || _pendingFrames <= 0) return;
        if (--_pendingFrames == 0)
        {
            var target = _pendingWaypoint;
            _pendingWaypoint = null;
            if (target.Pos == Vector3.zero)
            {
                Log(I18n.IsEnglish
                    ? $"[TP] Scene ready, using default landing ({target.LabelEn})"
                    : $"[TP] 场景就绪,使用默认落点({target.Label})");
                return;
            }
            MovePlayerTo(target.Pos);
            Log(I18n.IsEnglish
                ? $"[TP] Scene ready, TeleportPlayer → {target.Pos}"
                : $"[TP] 场景就绪,TeleportPlayer → {target.Pos}");
        }
    }

    private static void Log(string s) { CheatState.LastActionLog = s; ModMain.Log?.Msg(s); }
}

// 技能操作
internal static class Skills
{
    // 中文名 → SkillType。SkillType enum 在反编译里确认过
    public static readonly (string Label, SkillType Type)[] All =
    {
        ("生火",     SkillType.Firestarting),
        ("尸骸采集", SkillType.CarcassHarvesting),
        ("冰面钓鱼", SkillType.IceFishing),
        ("烹调",     SkillType.Cooking),
        ("步枪",     SkillType.Rifle),
        ("弓术",     SkillType.Archery),
        ("修补",     SkillType.ClothingRepair),
        ("工具修理", SkillType.ToolRepair),
        ("左轮手枪", SkillType.Revolver),
        ("制枪术",   SkillType.Gunsmithing),
    };

    public static int GetTier(SkillType t)
    {
        try
        {
            var sm = GameManager.GetSkillsManager();
            if (sm == null) return 0;
            var skill = sm.GetSkill(t);
            if (skill == null) return 0;
            return skill.GetPoints();
        }
        catch { return 0; }
    }

    public static void SetMax(SkillType t)
    {
        try
        {
            var sm = GameManager.GetSkillsManager();
            if (sm == null) { Log("[Skills] no SkillsManager"); return; }
            var skill = sm.GetSkill(t);
            if (skill == null) { Log($"[Skills] skill {t} not found"); return; }
            int max = skill.GetMaxPoints();
            // PointAssignmentMode:0 = None, 1 = Normal, 2 = ... 反射枚举值,直接传 1 比较稳
            var setP = skill.GetType().GetMethod("SetPoints", BindingFlags.Instance | BindingFlags.Public);
            if (setP != null)
            {
                var pam = setP.GetParameters()[1].ParameterType;
                var modeVal = Enum.ToObject(pam, 1);
                setP.Invoke(skill, new object[] { max, modeVal });
                Log($"[Skills] {t} = {max}");
            }
        }
        catch (Exception ex) { Log($"[Skills] {ex.Message}"); }
    }

    public static void SetAllMax()
    {
        foreach (var (_, t) in All) SetMax(t);
        Log(I18n.IsEnglish ? "[Skills] All skills maxed" : "[Skills] 所有技能满级");
    }

    private static void Log(string s) { CheatState.LastActionLog = s; ModMain.Log?.Msg(s); }
}

// 壮举解锁 —— 直接设内部进度字段到满值(仿 CT 做法)
internal static class Feats
{
    public static void UnlockAllFeats()
    {
        try
        {
            int ok = 0, fail = 0;
            var fm = GameManager.GetFeatsManager();
            if (fm == null) { Log("[Feats] FeatsManager null"); return; }
            int total = fm.GetNumFeats();
            Log($"[Feats] Starting unlock, {total} feats found");

            for (int i = 0; i < total; i++)
            {
                try
                {
                    var feat = fm.GetFeatFromIndex(i);
                    if (feat == null) { fail++; continue; }
                    string name = ((UnityEngine.Object)feat).name;

                    // 强制 SetNormalizedProgress(1) — 不管是否已 unlocked
                    feat.SetNormalizedProgress(1.0f);

                    // 额外：布尔型 feat 直接设 m_Unlocked 字段
                    var unlockField = feat.GetType().GetField("m_Unlocked",
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if (unlockField != null)
                        unlockField.SetValue(feat, true);

                    // 触发 HandleOnFeatUnlocked 显示 UI 通知
                    try
                    {
                        var handler = typeof(Feat).GetMethod("HandleOnFeatUnlocked",
                            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                        if (handler != null) handler.Invoke(feat, null);
                    }
                    catch { }

                    ok++;
                    ModMain.Log?.Msg($"[Feats] #{i} {name} → SetProgress(1) + unlocked={feat.IsUnlocked()}");
                }
                catch (Exception ex) { fail++; ModMain.Log?.Warning($"[Feats] #{i} failed: {ex.Message}"); }
            }
            Log(I18n.IsEnglish ? $"[Feats] Done: {ok} ok, {fail} failed" : $"[Feats] 完成: {ok} 成功, {fail} 失败");
        }
        catch (Exception ex) { Log($"[Feats] {ex.Message}"); }
    }

    private static void Log(string s) { CheatState.LastActionLog = s; ModMain.Log?.Msg(s); }
}

// 环境 / 身体 相关的"持续保持"类(每帧或每 tick 应用,每 tick 间隔游戏自己会重写)
internal static class ExtraOneShot
{
    private static bool _sprainDisabled = false;

    public static void TickSprainRisk()
    {
        // Wrist 有内建 force-off;Ankle 没有,靠 Harmony patch SprainedAnkleStart 拦
        try
        {
            var wrist = GameManager.GetSprainedWristComponent();
            if (CheatState.NoSprainRisk && !_sprainDisabled)
            {
                try { wrist?.SetForceNoSprainWrist(true); } catch { }
                _sprainDisabled = true;
            }
            else if (!CheatState.NoSprainRisk && _sprainDisabled)
            {
                try { wrist?.SetForceNoSprainWrist(false); } catch { }
                _sprainDisabled = false;
            }
        }
        catch { }
    }
}

// 物品 / 蓝图 / 治愈 等一次性操作
internal static class QuickActions
{
    // 一键修复所有背包物品耐久到满
    public static void RepairAllInventory()
    {
        int ok = 0, fail = 0, total = 0;
        try
        {
            var inv = GameManager.m_Inventory;
            if (inv == null) { CheatState.LastActionLog = I18n.IsEnglish ? "[RepairBag] No inventory?" : "[修复背包] 没有背包?"; return; }
            var list = inv.m_Items;
            if (list == null) { CheatState.LastActionLog = I18n.IsEnglish ? "[RepairBag] m_Items=null" : "[修复背包] m_Items=null"; return; }
            total = list.Count;
            for (int i = 0; i < total; i++)
            {
                try
                {
                    var obj = list[i];
                    if (obj == null) continue;
                    var gi = obj.m_GearItem;
                    if (gi != null) { if (Cheats.RestoreDurability(gi)) ok++; else fail++; }
                }
                catch { fail++; }
            }
            CheatState.LastActionLog = I18n.IsEnglish
                ? $"[RepairBag] ok {ok} / total {total} (failed {fail})"
                : $"[修复背包] 成功 {ok} / 总 {total} (失败 {fail})";
            ModMain.Log?.Msg($"[Repair] ok={ok} fail={fail} total={total}");
        }
        catch (Exception ex)
        {
            CheatState.LastActionLog = I18n.IsEnglish
                ? $"[RepairBag error] {ex.Message}"
                : $"[修复背包异常] {ex.Message}";
            ModMain.Log?.Warning($"[Repair] {ex.Message}");
        }
    }

    // 解锁蓝图 —— 反射找 BlueprintManager / BluePrintItem,设它们的 IsKnown / m_RequiresResearch
    public static void UnlockAllBlueprints()
    {
        try
        {
            int n = 0;
            var asm = typeof(GameManager).Assembly;
            // 找 BluePrintItem / BlueprintItem 类
            Type bpType = null;
            foreach (var t in asm.GetTypes())
            {
                if (t.Name == "BluePrintItem" || t.Name == "BlueprintItem") { bpType = t; break; }
            }
            if (bpType == null) { Log("[BP] BluePrintItem type not found"); return; }

            var all = UnityEngine.Object.FindObjectsOfType(Il2CppType.From(bpType));
            foreach (var bp in all)
            {
                try
                {
                    foreach (var fn in new[] { "m_RequiresResearch", "m_KnownByDefault", "m_IsKnown" })
                    {
                        var f = bpType.GetField(fn, BindingFlags.Instance | BindingFlags.Public);
                        if (f != null && f.FieldType == typeof(bool))
                            f.SetValue(bp, fn == "m_RequiresResearch" ? (object)false : (object)true);
                    }
                    n++;
                }
                catch { }
            }
            Log(I18n.IsEnglish ? $"[BP] Processed {n} blueprints" : $"[BP] 处理 {n} 个蓝图");
        }
        catch (Exception ex) { Log($"[BP] {ex.Message}"); }
    }

    // 获取特定武器:沿用 Cheats.SpawnItem
    public static void GiveWeapon(string prefab) { Cheats.SpawnItem(prefab, 1); Log($"[Give] {prefab}"); }

    // 固定温度 —— 简化版:调 Weather 的 SetTemperature(反射)
    public static void SetFixedTemperature(float celsius)
    {
        try
        {
            var w = GameManager.GetWeatherComponent();
            if (w == null) return;
            var m = w.GetType().GetMethod("SetTemperature", BindingFlags.Instance | BindingFlags.Public);
            m?.Invoke(w, new object[] { celsius });
            Log($"[Temp] {celsius:F0}°C");
        }
        catch (Exception ex) { Log($"[Temp] {ex.Message}"); }
    }

    private static void Log(string s) { CheatState.LastActionLog = s; ModMain.Log?.Msg(s); }
}
