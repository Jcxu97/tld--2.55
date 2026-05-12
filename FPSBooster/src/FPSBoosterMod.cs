using System;
using System.Reflection;
using MelonLoader;

[assembly: MelonInfo(typeof(FPSBooster.FPSBoosterMod), "FPSBooster", "1.0.0", "TldHacks")]
[assembly: MelonGame("Hinterland", "TheLongDark")]

namespace FPSBooster;

public class FPSBoosterMod : MelonMod
{
    public override void OnInitializeMelon()
    {
        LoggerInstance.Msg("FPSBooster v1.0 — engine-level performance tuning for high-end hardware");
    }

    public override void OnSceneWasInitialized(int buildIndex, string sceneName)
    {
        ApplyOptimizations();
    }

    private void ApplyOptimizations()
    {
        int applied = 0;

        // ═══════════════════════════════════════════════════════════════
        // CPU MULTI-CORE
        // ═══════════════════════════════════════════════════════════════

        // Job Worker Count: maximize worker threads for Job System (physics, animation, pathfinding)
        try
        {
            var jobsType = Type.GetType("Unity.Jobs.LowLevel.Unsafe.JobsUtility, UnityEngine.CoreModule");
            if (jobsType != null)
            {
                var maxProp = jobsType.GetProperty("JobWorkerMaximumCount", BindingFlags.Static | BindingFlags.Public);
                var workerProp = jobsType.GetProperty("JobWorkerCount", BindingFlags.Static | BindingFlags.Public);
                if (maxProp != null && workerProp != null)
                {
                    int max = (int)maxProp.GetValue(null);
                    int target = Math.Min(max, Environment.ProcessorCount - 2);
                    workerProp.SetValue(null, target);
                    LoggerInstance.Msg($"  JobWorkerCount = {target} (max={max}, logical cores={Environment.ProcessorCount})");
                    applied++;
                }
            }
        }
        catch { }

        // Physics: don't block main thread waiting for transform sync
        applied += SetProperty("UnityEngine.Physics, UnityEngine.PhysicsModule",
            "autoSyncTransforms", false);

        // Physics: enable auto simulation on background
        applied += SetProperty("UnityEngine.Physics, UnityEngine.PhysicsModule",
            "reuseCollisionCallbacks", true);

        // ═══════════════════════════════════════════════════════════════
        // GPU UTILIZATION (RTX 5090 — 32GB VRAM)
        // ═══════════════════════════════════════════════════════════════

        // Texture Streaming: use way more VRAM for high-res mipmaps
        applied += SetProperty("UnityEngine.QualitySettings, UnityEngine.CoreModule",
            "streamingMipmapsActive", true);
        applied += SetProperty("UnityEngine.QualitySettings, UnityEngine.CoreModule",
            "streamingMipmapsMemoryBudget", 4096f);

        // LOD Bias: render high-detail models at greater distance (push work to GPU)
        applied += SetProperty("UnityEngine.QualitySettings, UnityEngine.CoreModule",
            "lodBias", 4f);

        // Max LOD Level: always use highest LOD (0 = best quality)
        applied += SetProperty("UnityEngine.QualitySettings, UnityEngine.CoreModule",
            "maximumLODLevel", 0);

        // Shadow distance: render shadows much further out
        applied += SetProperty("UnityEngine.QualitySettings, UnityEngine.CoreModule",
            "shadowDistance", 200f);

        // Shadow resolution: highest
        // ShadowResolution enum: 0=Low, 1=Medium, 2=High, 3=VeryHigh
        applied += SetProperty("UnityEngine.QualitySettings, UnityEngine.CoreModule",
            "shadowResolution", 3);

        // Shadow cascades: 4 for best quality
        applied += SetProperty("UnityEngine.QualitySettings, UnityEngine.CoreModule",
            "shadowCascades", 4);

        // Pixel light count: more realtime lights
        applied += SetProperty("UnityEngine.QualitySettings, UnityEngine.CoreModule",
            "pixelLightCount", 8);

        // Anisotropic filtering: force on all textures
        // AnisotropicFiltering enum: 0=Disable, 1=Enable, 2=ForceEnable
        applied += SetProperty("UnityEngine.QualitySettings, UnityEngine.CoreModule",
            "anisotropicFiltering", 2);

        // Skin weights: 4 bones (highest quality, uses GPU)
        // SkinWeights enum: 1=OneBone, 2=TwoBones, 4=FourBones, 255=Unlimited
        applied += SetProperty("UnityEngine.QualitySettings, UnityEngine.CoreModule",
            "skinWeights", 4);

        // ═══════════════════════════════════════════════════════════════
        // MEMORY & GC
        // ═══════════════════════════════════════════════════════════════

        // Incremental GC: spread collection across frames instead of freeze
        try
        {
            var gcType = Type.GetType("UnityEngine.Scripting.GarbageCollector, UnityEngine.CoreModule");
            if (gcType != null)
            {
                var modeProp = gcType.GetProperty("GCMode", BindingFlags.Static | BindingFlags.Public);
                if (modeProp != null)
                {
                    modeProp.SetValue(null, 1); // Mode.Enabled = incremental
                    LoggerInstance.Msg("  GCMode = Incremental");
                    applied++;
                }
            }
        }
        catch { }

        // ═══════════════════════════════════════════════════════════════
        // GENERAL
        // ═══════════════════════════════════════════════════════════════

        // runInBackground: never pause when window loses focus
        applied += SetProperty("UnityEngine.Application, UnityEngine.CoreModule",
            "runInBackground", true);

        // Target framerate: respect game's own setting (don't override)

        LoggerInstance.Msg($"[FPSBooster] {applied} engine optimizations applied.");
    }

    private int SetProperty(string typeName, string propertyName, object value)
    {
        try
        {
            var type = Type.GetType(typeName);
            if (type == null) return 0;

            var prop = type.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public);
            if (prop != null && prop.CanWrite)
            {
                var targetType = prop.PropertyType;
                object converted;

                if (targetType.IsEnum)
                    converted = Enum.ToObject(targetType, value);
                else
                    converted = Convert.ChangeType(value, targetType);

                prop.SetValue(null, converted);
                LoggerInstance.Msg($"  {propertyName} = {value}");
                return 1;
            }
        }
        catch { }
        return 0;
    }
}
