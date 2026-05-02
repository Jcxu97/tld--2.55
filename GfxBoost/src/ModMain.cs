using System;
using MelonLoader;
using ModSettings;
using UnityEngine;
using UnityEngine.Rendering;

[assembly: MelonInfo(typeof(GfxBoost.ModMain), "GfxBoost", "1.0.0", "user")]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: MelonAdditionalDependencies("ModSettings")]

namespace GfxBoost;

public class GfxSettings : JsonModSettings
{
    [Section("GfxBoost —— 运行时改 QualitySettings,野外 fps 提升 20-40")]

    [Name("启用")]
    [Description("关闭后字段不再被写入(但已写的不会回退,需重启游戏)")]
    public bool Enabled = true;

    [Name("阴影距离(米)")]
    [Description("超过此距离的物体不画阴影。野外省最多。默认 TLD 约 150m,推 30")]
    [Slider(20, 200, 18)]
    public int ShadowDistance = 30;

    [Name("阴影级联数 (Cascades)")]
    [Description("Directional Light 阴影切片。1 = 一级(最快);2/4 = 多级(质量好但慢)")]
    [Choice("1", "2", "4")]
    public int ShadowCascades = 0;  // index 0 = "1"

    [Name("阴影分辨率")]
    [Description("Low / Medium / High / Very High —— Low 快一倍")]
    [Choice("Low", "Medium", "High", "VeryHigh")]
    public int ShadowResolution = 0;  // index 0 = Low

    [Name("Pixel Light 数")]
    [Description("每物体最多几个 per-pixel 光,其它降级 vertex。1 = 只主光,最快")]
    [Slider(1, 8, 8)]
    public int PixelLightCount = 1;

    [Name("LOD Bias(0.5-2.0,×10)")]
    [Description("远处物体切换 LOD 门槛。小值 = 更激进换低模 = 更快。默认 0.8(×10=8)")]
    [Slider(3, 20, 18)]
    public int LodBiasX10 = 8;

    [Name("禁用各向异性过滤")]
    [Description("关闭 texture anisotropic filter。视觉损失极小,省一点 GPU")]
    public bool DisableAniso = true;

    [Name("禁用抗锯齿 MSAA")]
    [Description("关闭 MSAA。边缘稍锯齿,省显存 + 几 ms/帧")]
    public bool DisableMSAA = true;

    [Name("Soft particles 软粒子")]
    [Description("关闭 = 粒子边缘硬,省一点 GPU")]
    public bool DisableSoftParticles = true;

    [Name("实时反射探针")]
    [Description("关闭 realtime reflection probe 更新。TLD 可能用于水面/金属,视觉小损")]
    public bool DisableRealtimeReflections = true;

    [Name("关闭远处树 billboard 阴影")]
    [Description("LODGroup 最低层(billboard)不投影。视觉几乎无损但减少大量 draw call")]
    public bool DisableBillboardShadows = true;

    [Name("Log 一次生效值到 log")]
    public bool LogOnce = true;

    [Section("物理帧率(超级重要 —— 60 Hz 解决'150fps 体感30'感)")]

    [Name("强制 Unity Time.fixedDeltaTime")]
    [Description("TLD 默认 40ms (25Hz) —— 物理/AI/角色移动只 25 次/秒,即使渲染 150fps 体感仍 25fps。调到 60 Hz 或更高手感质变")]
    [Choice("不改(用游戏默认)", "60 Hz (16.67ms)", "75 Hz (13.33ms)", "100 Hz (10ms)", "120 Hz (8.33ms)", "150 Hz (6.67ms)")]
    public int FixedRate = 5;  // 默认 150 Hz —— 好电脑拉满
}

public class ModMain : MelonMod
{
    internal static MelonLogger.Instance Log;
    internal static GfxSettings Settings;
    private static bool _logged;
    private int _frame = 0;

    public override void OnInitializeMelon()
    {
        Log = LoggerInstance;
        try
        {
            Settings = new GfxSettings();
            Settings.AddToModSettings("GfxBoost");
            Log.Msg("GfxBoost v1.0 loaded");
        }
        catch (Exception ex) { Log.Error($"[Init] {ex}"); }
    }

    public override void OnUpdate()
    {
        if (Settings == null || !Settings.Enabled) return;
        _frame++;
        // 每 60 帧(1s)重写一次 —— TLD 改 quality preset 或 scene load 可能重置
        if ((_frame % 60) != 0) return;
        Apply();
    }

    public override void OnSceneWasInitialized(int buildIndex, string sceneName)
    {
        if (Settings == null || !Settings.Enabled) return;
        Apply();
        if (Settings.DisableBillboardShadows) StripBillboardShadows();
    }

    private void Apply()
    {
        try
        {
            QualitySettings.shadowDistance = Settings.ShadowDistance;
            QualitySettings.shadowCascades = IdxToCascades(Settings.ShadowCascades);
            QualitySettings.shadowResolution = IdxToShadowRes(Settings.ShadowResolution);
            QualitySettings.pixelLightCount = Settings.PixelLightCount;
            QualitySettings.lodBias = Settings.LodBiasX10 / 10f;
            QualitySettings.anisotropicFiltering = Settings.DisableAniso
                ? AnisotropicFiltering.Disable : AnisotropicFiltering.Enable;
            QualitySettings.antiAliasing = Settings.DisableMSAA ? 0 : 2;
            QualitySettings.softParticles = !Settings.DisableSoftParticles;
            QualitySettings.realtimeReflectionProbes = !Settings.DisableRealtimeReflections;

            // 强制 fixedDeltaTime —— 默认 TLD 是 0.04 (25Hz),角色/物理感觉卡
            switch (Settings.FixedRate)
            {
                case 1: Time.fixedDeltaTime = 1f / 60f; break;   // 16.67ms
                case 2: Time.fixedDeltaTime = 1f / 75f; break;   // 13.33ms
                case 3: Time.fixedDeltaTime = 1f / 100f; break;  // 10ms
                case 4: Time.fixedDeltaTime = 1f / 120f; break;  // 8.33ms
                case 5: Time.fixedDeltaTime = 1f / 150f; break;  // 6.67ms
                // case 0 = 不改
            }

            if (Settings.LogOnce && !_logged)
            {
                _logged = true;
                Log?.Msg($"[GfxBoost] applied: shadowDist={QualitySettings.shadowDistance}m cascades={QualitySettings.shadowCascades} res={QualitySettings.shadowResolution} pixLights={QualitySettings.pixelLightCount} lodBias={QualitySettings.lodBias:F2} aniso={QualitySettings.anisotropicFiltering} AA={QualitySettings.antiAliasing} softP={QualitySettings.softParticles} rtReflect={QualitySettings.realtimeReflectionProbes} fixedDt={Time.fixedDeltaTime * 1000f:F2}ms({1f / Time.fixedDeltaTime:F0}Hz)");
            }
        }
        catch (Exception ex) { Log?.Warning($"[Apply] {ex.Message}"); }
    }

    private void StripBillboardShadows()
    {
        try
        {
            var lodGroups = UnityEngine.Object.FindObjectsOfType<LODGroup>();
            if (lodGroups == null) return;
            int stripped = 0;
            for (int i = 0; i < lodGroups.Length; i++)
            {
                var lg = lodGroups[i];
                if (lg == null) continue;
                var lods = lg.GetLODs();
                if (lods == null || lods.Length < 2) continue;
                // 最后一级 LOD 的所有 renderer 关阴影
                var lastLod = lods[lods.Length - 1];
                var renderers = lastLod.renderers;
                if (renderers == null) continue;
                for (int r = 0; r < renderers.Length; r++)
                {
                    var rend = renderers[r];
                    if (rend == null) continue;
                    if (rend.shadowCastingMode != ShadowCastingMode.Off)
                    {
                        rend.shadowCastingMode = ShadowCastingMode.Off;
                        stripped++;
                    }
                }
            }
            if (stripped > 0)
                Log?.Msg($"[GfxBoost] billboard shadow stripped: {stripped} renderers across {lodGroups.Length} LODGroups");
        }
        catch (Exception ex) { Log?.Warning($"[StripBillboard] {ex.Message}"); }
    }

    private static int IdxToCascades(int i) => i switch { 0 => 1, 1 => 2, 2 => 4, _ => 1 };
    private static ShadowResolution IdxToShadowRes(int i) => i switch
    {
        0 => ShadowResolution.Low,
        1 => ShadowResolution.Medium,
        2 => ShadowResolution.High,
        3 => ShadowResolution.VeryHigh,
        _ => ShadowResolution.Low
    };
}
