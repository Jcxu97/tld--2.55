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
    public string Scene;
    public Vector3 Pos;
    public Waypoint(string label, string scene, float x, float y, float z)
    { Label = label; Scene = scene; Pos = new Vector3(x, y, z); }
}

internal static class Teleport
{
    // 已知可用坐标(BunkerDefaults 搬来的 5 个地堡) + 全部 15 个 region 的场景名
    // Pos == Vector3.zero 时走"只 LoadScene,不 TeleportPlayer"路径 —— 让游戏默认落点生效
    public static readonly List<Waypoint> Destinations = new()
    {
        // —— 地堡(精确坐标,PVP 的 prepper cache 隐蔽点)——
        new Waypoint("神秘湖地堡",         "LakeRegion",              1029.06f,  91.99f,  -52.52f),
        new Waypoint("山间小镇地堡",       "MountainTownRegion",      1828.20f, 444.39f, 1771.27f),
        new Waypoint("荒凉水湾地堡",       "CanneryRegion",            328.37f, 344.50f,  833.16f),
        new Waypoint("黑岩地区地堡",       "BlackrockRegion",          705.04f, 373.98f,  816.38f),
        new Waypoint("灰烬峡谷地堡",       "AshCanyonRegion",          -42.12f, 172.95f, -796.68f),
        // —— 其他 region(Pos=0 走场景默认落点)——
        new Waypoint("沿海公路",           "CoastalRegion",           0f, 0f, 0f),
        new Waypoint("宁静山谷",           "RuralRegion",             0f, 0f, 0f),
        new Waypoint("针叶松林山",         "CrashMountainRegion",     0f, 0f, 0f),
        new Waypoint("荒野沼泽",           "MarshRegion",             0f, 0f, 0f),
        new Waypoint("晦暗湾",             "MiningRegion",            0f, 0f, 0f),
        new Waypoint("被弃机场(DLC)",     "AirfieldRegion",          0f, 0f, 0f),
        new Waypoint("捕鲸站",             "WhalingStationRegion",    0f, 0f, 0f),
        new Waypoint("废弃铁路",           "TracksRegion",            0f, 0f, 0f),
        new Waypoint("寂静河谷",           "RiverValleyRegion",       0f, 0f, 0f),
        new Waypoint("山间隘口",           "MountainPassRegion",      0f, 0f, 0f),
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
                if (!hasExactCoord) { Log($"[TP] → {w.Label}:已在本区,无精确坐标不传送"); return; }
                Log($"[TP] → {w.Label} (同区)");
                MovePlayerTo(w.Pos);
                return;
            }

            Log($"[TP] 跨场景 → {w.Label} ({w.Scene}),开始保存 + 加载...");

            // 1) 存当前档(反射避免签名问题)
            try
            {
                var sgt = typeof(SaveGameSystem);
                var saveMethod = sgt.GetMethod("SaveGame", BindingFlags.Static | BindingFlags.Public, null, System.Type.EmptyTypes, null)
                              ?? sgt.GetMethod("ForceSaveGame", BindingFlags.Static | BindingFlags.Public, null, System.Type.EmptyTypes, null);
                saveMethod?.Invoke(null, null);
            }
            catch (Exception ex) { Log($"[TP.Save] {ex.Message}"); }

            // 2) 构造 SceneTransitionData
            try
            {
                var std = new SceneTransitionData();
                std.m_SceneSaveFilenameCurrent = activeScene.name;
                std.m_SceneSaveFilenameNextLoad = w.Scene;
                std.m_TeleportPlayerSaveGamePosition = true;
                std.m_LastOutdoorScene = w.Scene;
                std.m_PosBeforeInteriorLoad = w.Pos;
                GameManager.m_SceneTransitionData = std;
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
                Log($"[TP] 场景就绪,使用默认落点({target.Label})");
                return;
            }
            MovePlayerTo(target.Pos);
            Log($"[TP] 场景就绪,TeleportPlayer → {target.Pos}");
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
        Log("[Skills] 所有技能满级");
    }

    private static void Log(string s) { CheatState.LastActionLog = s; ModMain.Log?.Msg(s); }
}

// 壮举解锁 —— 反射扫所有 Feat_XXX 类,设它们的单例 IsUnlocked = true
internal static class Feats
{
    public static void UnlockAllFeats()
    {
        try
        {
            int n = 0;
            var asm = typeof(GameManager).Assembly;
            foreach (var t in asm.GetTypes())
            {
                if (t.Name.StartsWith("Feat_") && typeof(MonoBehaviour).IsAssignableFrom(t))
                {
                    try
                    {
                        // 每个 Feat_XXX 是 MonoBehaviour,实例挂在场景里
                        var instances = UnityEngine.Object.FindObjectsOfType(Il2CppType.From(t));
                        if (instances == null) continue;
                        foreach (var inst in instances)
                        {
                            var f = t.GetField("m_IsUnlocked", BindingFlags.Instance | BindingFlags.Public)
                                 ?? t.GetField("IsUnlocked",   BindingFlags.Instance | BindingFlags.Public);
                            if (f != null) { f.SetValue(inst, true); n++; }
                        }
                    }
                    catch { }
                }
            }
            Log($"[Feats] 解锁 {n} 项壮举");
        }
        catch (Exception ex) { Log($"[Feats] {ex.Message}"); }
    }

    private static void Log(string s) { CheatState.LastActionLog = s; ModMain.Log?.Msg(s); }
}

// 环境 / 身体 相关的"持续保持"类(每帧或每 tick 应用,每 tick 间隔游戏自己会重写)
internal static class ExtraOneShot
{
    private static bool _windDisabled = false;
    private static bool _sprainDisabled = false;

    public static void TickStopWind()
    {
        try
        {
            var w = GameManager.GetWeatherComponent();
            if (w == null) return;
            if (CheatState.StopWind && !_windDisabled)
            {
                try { w.DisableWindEffect(); _windDisabled = true; } catch { }
            }
            else if (!CheatState.StopWind && _windDisabled)
            {
                try { w.EnableWindEffect(); _windDisabled = false; } catch { }
            }
        }
        catch { }
    }

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
        try
        {
            var inv = GameManager.m_Inventory;
            if (inv == null) { Log("[Repair] no inventory"); return; }
            int n = 0;
            var list = inv.m_Items;
            if (list == null) return;
            for (int i = 0; i < list.Count; i++)
            {
                var gi = list[i]?.m_GearItem;
                if (gi != null) { Cheats.RestoreDurability(gi); n++; }
            }
            Log($"[Repair] 修复 {n} 件背包物品");
        }
        catch (Exception ex) { Log($"[Repair] {ex.Message}"); }
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
            Log($"[BP] 处理 {n} 个蓝图");
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
