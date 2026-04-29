using System;
using System.Collections.Generic;
using MelonLoader;
using ModSettings;
using UnityEngine;
using Il2Cpp;
using Il2CppInterop.Runtime;

[assembly: MelonInfo(typeof(LightCull.ModMain), "LightCull", "1.0.0", "user")]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: MelonAdditionalDependencies("ModSettings")]

namespace LightCull;

public class LightCullSettings : JsonModSettings
{
    [Section("LightCull —— 场景光源剔除(对付多蜡烛/HouseLights/AmbientLights 的性能优化)")]

    [Name("启用 LightCull")]
    [Description("关闭即走原生 Unity 光照;开启后每秒扫场景 Light,按距离剔除阴影/禁用")]
    public bool Enabled = true;

    [Name("最多保留几个带阴影的光源")]
    [Description("玩家周围距离最近的 N 个动态光保留 shadows,其余强制 shadows=None")]
    [Slider(1, 16, 16)]
    public int MaxShadowLights = 4;

    [Name("阴影半径(米)")]
    [Description("距离玩家超过此半径的光 shadows=None,省 shadow map 渲染")]
    [Slider(5, 100, 20)]
    public int ShadowRadius = 25;

    [Name("禁用半径(米)")]
    [Description("距离超过此半径的光直接 enabled=false,完全不参与光照")]
    [Slider(10, 500, 20)]
    public int DisableRadius = 80;

    [Name("扫描周期(帧,60 帧 = 1 秒)")]
    [Slider(30, 600, 20)]
    public int ScanPeriodFrames = 60;

    [Name("打印统计到 log")]
    [Description("每次扫描 log 一行:'扫到 N 盏灯,禁用 X,shadows=None Y'")]
    public bool LogStats = false;
}

public class ModMain : MelonMod
{
    internal static MelonLogger.Instance Log;
    internal static LightCullSettings Settings;
    private int _frame = 0;

    public override void OnInitializeMelon()
    {
        Log = LoggerInstance;
        try
        {
            Settings = new LightCullSettings();
            Settings.AddToModSettings("LightCull");
            Log.Msg($"LightCull v1.0 loaded — enabled={Settings.Enabled}, maxShadow={Settings.MaxShadowLights}, shadowR={Settings.ShadowRadius}m, disableR={Settings.DisableRadius}m");
        }
        catch (Exception ex) { Log.Error($"[Init] {ex}"); }
    }

    public override void OnUpdate()
    {
        if (Settings == null || !Settings.Enabled) return;
        _frame++;
        if ((_frame % Settings.ScanPeriodFrames) != 0) return;
        try { ScanAndCull(); }
        catch (Exception ex) { Log?.Warning($"[Scan] {ex.Message}"); }
    }

    private void ScanAndCull()
    {
        // 拿玩家位置(GameManager 可能还没 ready —— 主菜单时跳过)
        Vector3 pPos;
        try
        {
            var pm = GameManager.GetPlayerManagerComponent();
            if (pm == null) return;
            var tr = GameManager.GetPlayerTransform();
            if (tr == null) return;
            pPos = tr.position;
        }
        catch { return; }

        // FindObjectsOfType<Light> —— IL2Cpp 返回 Il2CppReferenceArray<Light>
        Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppArrayBase<Light> lights;
        try { lights = UnityEngine.Object.FindObjectsOfType<Light>(); }
        catch { return; }
        if (lights == null || lights.Length == 0) return;

        float shadowR2 = Settings.ShadowRadius * (float)Settings.ShadowRadius;
        float disableR2 = Settings.DisableRadius * (float)Settings.DisableRadius;

        // 一次遍历:
        //   超出 disable 半径 → enabled=false
        //   超出 shadow 半径 → shadows=None,enabled=true
        //   其它 → 候选 shadow,按距离排序取最近 MaxShadowLights 个
        var candidates = new List<(Light light, float d2)>();
        int disabledCount = 0, noShadowCount = 0, keptCount = 0;

        for (int i = 0; i < lights.Length; i++)
        {
            var L = lights[i];
            if (L == null) continue;
            Vector3 lp;
            try { lp = L.transform.position; } catch { continue; }
            float d2 = (lp - pPos).sqrMagnitude;
            if (d2 > disableR2)
            {
                try { if (L.enabled) { L.enabled = false; disabledCount++; } } catch { }
            }
            else if (d2 > shadowR2)
            {
                try
                {
                    if (!L.enabled) L.enabled = true;
                    if (L.shadows != LightShadows.None) { L.shadows = LightShadows.None; noShadowCount++; }
                }
                catch { }
            }
            else
            {
                try { if (!L.enabled) L.enabled = true; } catch { }
                candidates.Add((L, d2));
            }
        }

        // 候选按距离升序排;前 MaxShadowLights 个保留 Soft shadow,其余 None
        candidates.Sort((a, b) => a.d2.CompareTo(b.d2));
        int max = Settings.MaxShadowLights;
        for (int i = 0; i < candidates.Count; i++)
        {
            var L = candidates[i].light;
            try
            {
                if (i < max)
                {
                    if (L.shadows == LightShadows.None) L.shadows = LightShadows.Soft;
                    keptCount++;
                }
                else
                {
                    if (L.shadows != LightShadows.None) { L.shadows = LightShadows.None; noShadowCount++; }
                }
            }
            catch { }
        }

        if (Settings.LogStats)
            Log?.Msg($"[LightCull] 扫 {lights.Length} 盏,禁 {disabledCount},去阴影 {noShadowCount},保留阴影 {keptCount}");
    }
}
