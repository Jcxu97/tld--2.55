using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Il2Cpp;

namespace TldHacks;

// v2.7.56: 学习 & 持久化游戏里每次 scene transition 的字段,解决 DLC Tale scene 跨区物品丢失
//   原因:Tale scene 的 save slot id 不等于 Unity scene 名,我们硬猜 → 游戏认作新 tale 初始化
//   做法:每次 OnSceneWasInitialized 读 GameManager.m_SceneTransitionData,按目标 scene 名存全量
//        字段;下次 TravelTo 到同一 scene 时复用其 FromSceneId/ToSceneId 等"真实" save slot id
//   格式:每行 "SceneName|FromSaveId|ToSaveId|LastOutdoor|LocIDOverride|SpawnPoint|SpawnAudio|ForceNextTrigger|Px|Py|Pz"
//   存储:D:\TLD-Mods\TldHacks\bin\Release\TldHacks_Transitions.txt(旁边,持久化跨档)
internal static class TransitionRecorder
{
    internal class Snapshot
    {
        public string FromSaveId = "";
        public string ToSaveId = "";
        public string LastOutdoor = "";
        public string LocIDOverride = "";
        public string SpawnPoint = "";
        public string SpawnAudio = "";
        public string ForceNextTrigger = "";
        public Vector3 PosBefore = Vector3.zero;
    }

    // key = 目标 Unity scene 名,value = 那次进去时的完整 transition 快照
    private static readonly Dictionary<string, Snapshot> _data = new Dictionary<string, Snapshot>(StringComparer.OrdinalIgnoreCase);

    private static string _path;

    public static void Init()
    {
        try
        {
            // 存游戏目录 Mods/ 同级,便于重装 mod 不丢
            var modsDir = Path.Combine(Directory.GetCurrentDirectory(), "Mods");
            if (!Directory.Exists(modsDir)) modsDir = Directory.GetCurrentDirectory();
            _path = Path.Combine(modsDir, "TldHacks_Transitions.txt");
            Load();
            ModMain.Log?.Msg($"[TransitionRecorder] loaded {_data.Count} snapshots from {_path}");
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[TransitionRecorder.Init] {ex.Message}"); }
    }

    private static void Load()
    {
        if (string.IsNullOrEmpty(_path) || !File.Exists(_path)) return;
        foreach (var line in File.ReadAllLines(_path))
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
            var parts = line.Split('|');
            if (parts.Length < 11) continue;
            var key = parts[0];
            var s = new Snapshot
            {
                FromSaveId = parts[1],
                ToSaveId = parts[2],
                LastOutdoor = parts[3],
                LocIDOverride = parts[4],
                SpawnPoint = parts[5],
                SpawnAudio = parts[6],
                ForceNextTrigger = parts[7],
                PosBefore = new Vector3(
                    float.TryParse(parts[8], out var x) ? x : 0f,
                    float.TryParse(parts[9], out var y) ? y : 0f,
                    float.TryParse(parts[10], out var z) ? z : 0f)
            };
            _data[key] = s;
        }
    }

    private static void Save()
    {
        try
        {
            if (string.IsNullOrEmpty(_path)) return;
            using var w = new StreamWriter(_path, false);
            w.WriteLine("# TldHacks transition snapshots — each row = target scene's last seen SceneTransitionData");
            w.WriteLine("# SceneName|FromSaveId|ToSaveId|LastOutdoor|LocIDOverride|SpawnPoint|SpawnAudio|ForceNextTrigger|Px|Py|Pz");
            foreach (var kv in _data)
            {
                var s = kv.Value;
                w.WriteLine($"{kv.Key}|{s.FromSaveId}|{s.ToSaveId}|{s.LastOutdoor}|{s.LocIDOverride}|{s.SpawnPoint}|{s.SpawnAudio}|{s.ForceNextTrigger}|{s.PosBefore.x:F2}|{s.PosBefore.y:F2}|{s.PosBefore.z:F2}");
            }
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[TransitionRecorder.Save] {ex.Message}"); }
    }

    // ModMain.OnSceneWasInitialized 里调用 —— 传入刚加载完的 scene 名
    public static void OnSceneInitialized(string sceneName)
    {
        try
        {
            var std = GameManager.m_SceneTransitionData;
            if (std == null) return;

            var snap = new Snapshot
            {
                FromSaveId       = std.m_SceneSaveFilenameCurrent ?? "",
                ToSaveId         = std.m_SceneSaveFilenameNextLoad ?? "",
                LastOutdoor      = std.m_LastOutdoorScene ?? "",
                LocIDOverride    = std.m_SceneLocationLocIDOverride ?? "",
                SpawnPoint       = std.m_SpawnPointName ?? "",
                SpawnAudio       = std.m_SpawnPointAudio ?? "",
                ForceNextTrigger = std.m_ForceNextSceneLoadTriggerScene ?? "",
                PosBefore        = std.m_PosBeforeInteriorLoad
            };

            // 只记入"真·跨场景"的 transition(两端都有 save id)
            if (string.IsNullOrEmpty(snap.FromSaveId) && string.IsNullOrEmpty(snap.ToSaveId)) return;

            // v2.7.60 修 bug:key 必须用 Unity scene 名(sceneName 参数),因为 TravelTo 里 Lookup 传的也是 Unity scene 名
            //   之前用 ToSaveId 当 key,在 DLC Tale scene 上 ToSaveId ≠ Unity sceneName 时会 lookup miss —— 等于这个 mod
            //   对真正需要它的 scene 不起作用。当前数据巧合 ToSaveId==sceneName 所以没触发
            _data[sceneName] = snap;
            Save();
            ModMain.Log?.Msg($"[TransitionRecorder] learned → {sceneName} (ToSaveId={snap.ToSaveId}, locID={snap.LocIDOverride})");
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[TransitionRecorder.OnScene] {ex.Message}"); }
    }

    // TravelTo 查表:目标 scene Unity 名 → Snapshot。找不到返回 null
    public static Snapshot Lookup(string targetSceneName)
    {
        _data.TryGetValue(targetSceneName, out var s);
        return s;
    }

    public static int Count => _data.Count;
}
