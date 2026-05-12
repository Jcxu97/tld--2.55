using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppTLD.ModularElectrolizer;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using AssetBundle = UnityEngine.AssetBundle;

namespace TldHacks;

// ═══════════════════════════════════════════════════════════════════════════
// HouseLights 整合 (原 mod by DemonBunnyBon)
// 室内灯光开关 — 非极光时也能亮灯,通过墙上 switch 交互 toggle
// ═══════════════════════════════════════════════════════════════════════════

internal sealed class HLElectrolizerConfig
{
    public AuroraModularElectrolizer electrolizer;
    public float[] ranges;
    public Color[] colors;
    public bool skipWhenOn;
}

internal sealed class HLElectroLightConfig
{
    public AuroraLightingSimple electrolizer;
    public float[] ranges;
    public Color[] colors;
    public bool skipWhenOn;
}

internal static class HouseLightsState
{
    internal static AssetBundle HLBundle;
    internal static bool LightsOn;
    internal static List<HLElectrolizerConfig> ElectroSources = new();
    internal static List<HLElectroLightConfig> ElectroLightSources = new();
    internal static List<GameObject> LightSwitches = new();

    internal static readonly List<string> NotReallyOutdoors = new() { "DamTransitionZone" };

    internal static Shader VanillaShader;

    internal static void LoadBundle()
    {
        if (HLBundle != null) return;
        try
        {
            using var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("TldHacks.HouseLights.hlbundle");
            if (stream == null)
            {
                MelonLogger.Warning("[HouseLights] hlbundle not found as embedded resource");
                return;
            }
            var ms = new MemoryStream((int)stream.Length);
            stream.CopyTo(ms);
            var il2ms = new Il2CppSystem.IO.MemoryStream(
                (Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte>)ms.ToArray());
            HLBundle = AssetBundle.LoadFromStream((Il2CppSystem.IO.Stream)(object)il2ms);
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"[HouseLights] 加载 bundle 失败: {ex.Message}");
        }
    }

    internal static void Init()
    {
        ElectroSources.Clear();
        ElectroLightSources.Clear();
        LightSwitches.Clear();
        LightsOn = false;
    }

    internal static void AddElectrolizer(AuroraModularElectrolizer light)
    {
        var cfg = new HLElectrolizerConfig();
        cfg.electrolizer = light;
        cfg.ranges = new float[light.m_LocalLights._size];
        cfg.colors = new Color[light.m_LocalLights._size];
        for (int i = 0; i < light.m_LocalLights._size; i++)
        {
            cfg.ranges[i] = light.m_LocalLights[i].range;
            cfg.colors[i] = light.m_LocalLights[i].color;
        }
        string goName = light.gameObject.name;
        cfg.skipWhenOn = goName.Contains("Alarm") || goName.Contains("Headlight") ||
            goName.Contains("Taillight") || goName.Contains("Television") ||
            goName.Contains("Computer") || goName.Contains("Machine") ||
            goName.Contains("ControlBox") || goName.Contains("Interiorlight");
        ElectroSources.Add(cfg);
    }

    internal static void AddElectrolizerLight(AuroraLightingSimple light)
    {
        var cfg = new HLElectroLightConfig();
        cfg.electrolizer = light;
        int len = ((Il2CppArrayBase<Light>)(object)light.m_LocalLights).Length;
        cfg.ranges = new float[len];
        cfg.colors = new Color[len];
        string goName = light.gameObject.name;
        cfg.skipWhenOn = goName.Contains("Alarm") || goName.Contains("Headlight") ||
            goName.Contains("Taillight") || goName.Contains("Television") ||
            goName.Contains("Computer") || goName.Contains("Machine") ||
            goName.Contains("ControlBox") || goName.Contains("Interiorlight");
        for (int i = 0; i < len; i++)
        {
            cfg.ranges[i] = ((Il2CppArrayBase<Light>)(object)light.m_LocalLights)[i].range;
            cfg.colors[i] = ((Il2CppArrayBase<Light>)(object)light.m_LocalLights)[i].color;
        }
        ElectroLightSources.Add(cfg);
    }

    internal static void GetSwitches()
    {
        var rootObjects = GetRootObjects();
        var orgObj = new List<GameObject>();

        foreach (var item in rootObjects)
        {
            orgObj.Clear();
            GetChildrenWithName(item, "houselightswitch", orgObj);
            GetChildrenWithName(item, "lightswitcha", orgObj);
            GetChildrenWithName(item, "lightswitchblack", orgObj);
            GetChildrenWithName(item, "switch_a_black", orgObj);
            GetChildrenWithName(item, "switch_a_white", orgObj);
            GetChildrenWithName(item, "switch_a_purple", orgObj);
            GetChildrenWithName(item, "switch_b_white", orgObj);

            foreach (var obj in orgObj)
            {
                if (!obj.active) continue;
                string switchName = obj.name;
                int variant = (switchName.IndexOf("houselightswitch", System.StringComparison.OrdinalIgnoreCase) >= 0
                    || switchName.IndexOf("lightswitcha", System.StringComparison.OrdinalIgnoreCase) >= 0) ? 1 : 2;
                obj.active = false;
                Vector3 pos = obj.transform.position;
                Quaternion rot = obj.transform.rotation;
                var sw = InstantiateSwitch(pos, rot.eulerAngles, variant);
                if (sw == null) continue;
                var mesh = sw.transform.FindChild("SM_LightSwitchBlack")?.gameObject;
                if (mesh == null) continue;
                mesh.layer = 19;
                LightSwitches.Add(mesh);
                mesh.name = "MOD_HouseLightSwitch";
                if (mesh.GetComponent<Collider>() == null)
                {
                    var col = mesh.AddComponent<BoxCollider>();
                    col.size = new Vector3(0.1f, 0.1f, 0.1f);
                }
            }
        }
    }

    internal static void UpdateElectroLights(AuroraManager mngr)
    {
        var player = GameManager.GetVpFPSPlayer();
        if (player == null) return;
        Vector3 playerPos = player.gameObject.transform.position;
        float cullDist = CheatState.HL_CullDistance;
        float rangeMult = CheatState.HL_RangeMultiplier;
        bool whiteLights = CheatState.HL_WhiteLights;
        bool shadows = CheatState.HL_CastShadows;
        bool noFlicker = CheatState.HL_NoFlicker;
        float intensity = CheatState.HL_Intensity;
        bool audio = CheatState.HL_LightAudio;

        for (int i = 0; i < ElectroSources.Count; i++)
        {
            var src = ElectroSources[i];
            if (src.electrolizer == null || src.electrolizer.m_LocalLights == null) continue;

            float dist = Mathf.Abs(Vector3.Distance(
                src.electrolizer.gameObject.transform.position, playerPos));

            if (dist > cullDist && !mngr.AuroraIsActive())
            {
                src.electrolizer.UpdateIntensity(1f, 0f);
                src.electrolizer.UpdateLight(true);
                src.electrolizer.UpdateEmissiveObjects(true);
                src.electrolizer.UpdateAudio();
                continue;
            }

            for (int j = 0; j < src.electrolizer.m_LocalLights._size; j++)
            {
                float r = Math.Min(src.ranges[j] * rangeMult, 20f);
                src.electrolizer.m_LocalLights[j].range = r;
                src.electrolizer.m_HasFlickerSet = !noFlicker;
                Color c = src.colors[j];
                if (whiteLights) { Color.RGBToHSV(c, out float h, out float ss, out float vv); c = Color.HSVToRGB(h, ss * 0.15f, vv); }
                src.electrolizer.m_LocalLights[j].color = c;
                if (shadows) src.electrolizer.m_LocalLights[j].shadows = (LightShadows)2;
            }

            if (LightsOn && !mngr.AuroraIsActive())
            {
                if (src.skipWhenOn) continue;
                src.electrolizer.UpdateIntensity(1f, intensity);
                src.electrolizer.UpdateLight(false);
                src.electrolizer.UpdateEmissiveObjects(false);
                if (audio) src.electrolizer.UpdateAudio();
                else src.electrolizer.StopAudio();
            }
            else if (!mngr.AuroraIsActive())
            {
                src.electrolizer.UpdateIntensity(1f, 0f);
                src.electrolizer.UpdateLight(true);
                src.electrolizer.UpdateEmissiveObjects(true);
                src.electrolizer.UpdateAudio();
            }
            else
            {
                src.electrolizer.UpdateIntensity(Time.deltaTime, mngr.m_NormalizedActive);
            }
        }

        for (int k = 0; k < ElectroLightSources.Count; k++)
        {
            var src = ElectroLightSources[k];
            if (src.electrolizer == null || src.electrolizer.m_LocalLights == null) continue;

            float dist = Mathf.Abs(Vector3.Distance(
                src.electrolizer.gameObject.transform.position, playerPos));

            if (dist > cullDist && !mngr.AuroraIsActive())
            {
                src.electrolizer.m_CurIntensity = 0f;
                src.electrolizer.UpdateLight(true);
                src.electrolizer.UpdateEmissiveObjects(true);
                src.electrolizer.UpdateAudio();
                continue;
            }

            for (int l = 0; l < ((Il2CppArrayBase<Light>)(object)src.electrolizer.m_LocalLights).Length; l++)
            {
                float r = Math.Min(src.ranges[l] * rangeMult, 20f);
                ((Il2CppArrayBase<Light>)(object)src.electrolizer.m_LocalLights)[l].range = r;
                Color c = src.colors[l];
                if (whiteLights) { Color.RGBToHSV(c, out float h, out float s, out float vv); c = Color.HSVToRGB(h, s * 0.15f, vv); }
                ((Il2CppArrayBase<Light>)(object)src.electrolizer.m_LocalLights)[l].color = c;
                if (shadows) ((Il2CppArrayBase<Light>)(object)src.electrolizer.m_LocalLights)[l].shadows = (LightShadows)2;
            }

            if (LightsOn && !mngr.AuroraIsActive())
            {
                if (src.skipWhenOn) continue;
                src.electrolizer.m_CurIntensity = intensity;
                src.electrolizer.UpdateLight(false);
                src.electrolizer.UpdateEmissiveObjects(false);
                if (audio) src.electrolizer.UpdateAudio();
                else src.electrolizer.StopAudio();
            }
            else if (!mngr.AuroraIsActive())
            {
                src.electrolizer.m_CurIntensity = 0f;
                src.electrolizer.UpdateLight(true);
                src.electrolizer.UpdateEmissiveObjects(true);
                src.electrolizer.UpdateAudio();
            }
            else
            {
                src.electrolizer.UpdateIntensity(Time.deltaTime);
            }
        }
    }

    internal static void InstantiateCustomSwitches(string sceneName)
    {
        if (HLBundle == null) return;
        switch (sceneName.ToLowerInvariant())
        {
            case "lakeregion":
                InstantiateSwitch(new Vector3(791.93f, 214.38f, 965.76f), new Vector3(0f, 265f, 0f), 0);
                break;
            case "trailera":
                InstantiateSwitch(new Vector3(-2.95f, 1.38f, 2.06f), new Vector3(0f, 180f, 0f), 0);
                break;
            case "communityhalla":
                InstantiateSwitch(new Vector3(0.128f, 1.38f, 4.2f), new Vector3(0f, 0f, 0f), 1);
                InstantiateSwitch(new Vector3(8.52f, 1.4f, 0.26f), new Vector3(0f, 0f, 0f), 1);
                InstantiateSwitch(new Vector3(6.91f, 1.54f, -3.97f), new Vector3(0f, 90f, 0f), 1);
                InstantiateSwitch(new Vector3(6.58f, 1.43f, 0.22f), new Vector3(0f, 270f, 0f), 3);
                InstantiateSwitch(new Vector3(-9.2f, 2.24f, 3.16f), new Vector3(0f, 180f, 0f), 3);
                InstantiateSwitch(new Vector3(9.05f, 1.41f, 0.16f), new Vector3(0f, 180f, 0f), 0);
                break;
            case "trailersshape":
                InstantiateSwitch(new Vector3(7.2f, 1.5f, -10.12f), new Vector3(0f, 0f, 0f), 1);
                InstantiateSwitch(new Vector3(0.59f, 1.55f, -5.92f), new Vector3(0f, 0f, 0f), 1);
                InstantiateSwitch(new Vector3(-6.53f, 1.52f, 3.76f), new Vector3(0f, 0f, 0f), 1);
                break;
            case "trailerb":
                InstantiateSwitch(new Vector3(3.88f, 1.34f, 2.07f), new Vector3(0f, 180f, 0f), 3);
                break;
            case "trailerc":
                InstantiateSwitch(new Vector3(-0.88f, 1.32f, 2.07f), new Vector3(0f, 180f, 0f), 0);
                break;
            case "trailerd":
                InstantiateSwitch(new Vector3(-3f, 1.37f, 2.07f), new Vector3(0f, 180f, 0f), 1);
                break;
            case "trailere":
                InstantiateSwitch(new Vector3(-2.94f, 1.34f, 2.07f), new Vector3(0f, 180f, 0f), 0);
                break;
            case "tracksregion":
                InstantiateSwitch(new Vector3(586.17f, 200.48f, 564.31f), new Vector3(0f, 270f, 0f), 3);
                break;
            case "mountainpassburiedcabin":
                InstantiateSwitch(new Vector3(3.89f, 1.26f, 0.82f), new Vector3(0f, 270f, 0f), 1);
                InstantiateSwitch(new Vector3(-0.52f, 5.17f, 1.711f), new Vector3(0f, 180f, 0f), 1);
                InstantiateSwitch(new Vector3(-0.54f, 5.13f, 1.78f), new Vector3(0f, 0f, 0f), 1);
                break;
            case "miltontrailerb":
                InstantiateSwitch(new Vector3(3.92f, 1.5f, 2.07f), new Vector3(0f, 180f, 0f), 3);
                break;
            case "huntinglodgea":
                InstantiateSwitch(new Vector3(7.27f, 1.18f, -1.58f), new Vector3(0f, 90f, 0f), 0);
                InstantiateSwitch(new Vector3(-1.17f, 1.51f, -5.01f), new Vector3(0f, 0f, 0f), 3);
                break;
            case "damtrailerb":
                InstantiateSwitch(new Vector3(4.01f, 1.49f, 2.07f), new Vector3(0f, 180f, 0f), 0);
                break;
            case "crashmountainregion":
                InstantiateSwitch(new Vector3(889.93f, 162.08f, 346.07f), new Vector3(0f, 180f, 0f), 3);
                break;
            case "coastalregion":
                InstantiateSwitch(new Vector3(757.9f, 25.51f, 646.78f), new Vector3(0f, 50f, 0f), 0);
                break;
            case "cannerytrailera":
                InstantiateSwitch(new Vector3(-3.02f, 1.42f, 2.79f), new Vector3(0f, 180f, 0f), 3);
                break;
            case "bunkerc":
                InstantiateSwitch(new Vector3(1.09f, 1.73f, 3.54f), new Vector3(0f, 0f, 0f), 3);
                InstantiateSwitch(new Vector3(-14.72f, 0.33f, 12.93f), new Vector3(0f, 0f, 0f), 3);
                break;
            case "bunkerb":
                InstantiateSwitch(new Vector3(1.13f, 1.67f, 3.54f), new Vector3(0f, 0f, 0f), 3);
                InstantiateSwitch(new Vector3(2.94f, 1.54f, 7.68f), new Vector3(0f, 90f, 0f), 3);
                InstantiateSwitch(new Vector3(-3.19f, 1.61f, 7.66f), new Vector3(0f, 270f, 0f), 3);
                break;
            case "bunkera":
                InstantiateSwitch(new Vector3(5.93f, 1.61f, 12.63f), new Vector3(0f, 180f, 0f), 3);
                InstantiateSwitch(new Vector3(1.18f, 1.65f, 1.62f), new Vector3(0f, 0f, 0f), 3);
                break;
            case "blackrocktrailerb":
                InstantiateSwitch(new Vector3(3.9726f, 1.4155f, 2.0799f), new Vector3(0f, 180f, 0f), 1);
                break;
            case "airfieldtrailerb":
                InstantiateSwitch(new Vector3(4.01f, 1.49f, 2.07f), new Vector3(0f, 180f, 0f), 0);
                break;
        }
    }

    // ——— Utility ———

    internal static List<GameObject> GetRootObjects()
    {
        var list = new List<GameObject>();
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            var roots = scene.GetRootGameObjects();
            if (roots != null) foreach (var go in roots) list.Add(go);
        }
        return list;
    }

    internal static void GetChildrenWithName(GameObject obj, string name, List<GameObject> result)
    {
        if (obj.transform.childCount <= 0) return;
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            var child = obj.transform.GetChild(i).gameObject;
            if (child.name.IndexOf(name, System.StringComparison.OrdinalIgnoreCase) >= 0) result.Add(child);
            GetChildrenWithName(child, name, result);
        }
    }

    internal static GameObject InstantiateSwitch(Vector3 pos, Vector3 rot, int variant)
    {
        if (HLBundle == null) return null;
        string asset = variant switch
        {
            1 => "OBJ_SwitchHLB",
            2 => "OBJ_SwitchHLC",
            3 => "OBJ_SwitchHLD",
            _ => "OBJ_SwitchHL"
        };
        var go = UnityEngine.Object.Instantiate(HLBundle.LoadAsset<GameObject>(asset));

        if (VanillaShader == null) VanillaShader = Shader.Find("Shader Forge/TLD_StandardDiffuse");
        var renderers = go.GetComponentsInChildren<MeshRenderer>();
        foreach (var mr in renderers)
        {
            Texture tex = mr.material.mainTexture;
            Material mat = new Material(VanillaShader);
            mat.mainTexture = tex;
            mr.material = mat;
        }

        go.transform.eulerAngles = rot;
        go.transform.position = pos;
        go.transform.localScale = Vector3.one;
        return go;
    }

    internal static bool IsMenu()
    {
        return GameManager.m_ActiveScene != null &&
            (GameManager.m_ActiveScene.Contains("MainMenu") || InterfaceManager.IsMainMenuEnabled());
    }

    internal static bool ShouldRun()
    {
        if (IsMenu() || InterfaceManager.IsMainMenuEnabled()) return false;
        bool isOutdoor = GameManager.IsOutDoorsScene(GameManager.m_ActiveScene);
        if (isOutdoor && !CheatState.HL_EnableOutside && !NotReallyOutdoors.Contains(GameManager.m_ActiveScene))
            return false;
        return true;
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// Harmony Patches (走 DynamicPatch)
// ═══════════════════════════════════════════════════════════════════════════

internal static class Patch_HL_GameManager_InstantiatePlayer
{
    internal static void Prefix()
    {
        if (!HouseLightsState.ShouldRun()) return;
        HouseLightsState.LoadBundle();
        HouseLightsState.InstantiateCustomSwitches(GameManager.m_ActiveScene);
        HouseLightsState.Init();
        HouseLightsState.GetSwitches();
    }
}

internal static class Patch_HL_Electrolizer_Initialize
{
    internal static void Postfix(AuroraModularElectrolizer __instance)
    {
        if (!HouseLightsState.ShouldRun()) return;
        var toggles = __instance.gameObject.GetComponentsInParent<AuroraActivatedToggle>();
        var screens = __instance.gameObject.GetComponentsInChildren<AuroraScreenDisplay>();
        if ((toggles == null || toggles.Count == 0) && (screens == null || screens.Count == 0))
            HouseLightsState.AddElectrolizer(__instance);
        __instance.m_HasFlickerSet = !CheatState.HL_NoFlicker;
    }
}

internal static class Patch_HL_AuroraManager_RegisterLightSimple
{
    internal static void Postfix(AuroraManager __instance, AuroraLightingSimple auroraLightSimple)
    {
        if (!HouseLightsState.ShouldRun()) return;
        HouseLightsState.AddElectrolizerLight(auroraLightSimple);
    }
}

internal static class Patch_HL_AuroraManager_UpdateForceAurora
{
    private static int _skip;
    internal static void Postfix(AuroraManager __instance)
    {
        if (++_skip < 5) return;
        _skip = 0;
        if (!HouseLightsState.ShouldRun()) return;
        if (HouseLightsState.ElectroSources.Count > 0 || HouseLightsState.ElectroLightSources.Count > 0)
            HouseLightsState.UpdateElectroLights(__instance);
    }
}

internal static class Patch_HL_PlayerManager_UpdateHUDText
{
    private static string _textOn, _textOff;

    internal static void Postfix(PlayerManager __instance, Panel_HUD hud)
    {
        if (HouseLightsState.IsMenu()) return;
        if (GameManager.GetMainCamera() == null) return;
        var obj = __instance.GetInteractiveObjectUnderCrosshairs(CheatState.HL_InteractDistance);
        if (obj != null && obj.name == "MOD_HouseLightSwitch")
        {
            _textOn ??= I18n.T("关灯", "Turn Lights Off");
            _textOff ??= I18n.T("开灯", "Turn Lights On");
            hud.SetHoverText(HouseLightsState.LightsOn ? _textOn : _textOff, obj, (HoverTextState)3);
        }
    }
}

internal static class Patch_HL_PlayerManager_ProcessInteraction
{
    internal static void Postfix(PlayerManager __instance, ref bool __result)
    {
        if (HouseLightsState.IsMenu()) return;
        var obj = __instance.GetInteractiveObjectUnderCrosshairs(CheatState.HL_InteractDistance);
        if (obj == null || obj.name != "MOD_HouseLightSwitch") return;
        HouseLightsState.LightsOn = !HouseLightsState.LightsOn;
        GameAudioManager.PlaySound("Stop_RadioAurora", __instance.gameObject);
        float x = obj.transform.localScale.x;
        float y = obj.transform.localScale.y;
        float z = obj.transform.localScale.z;
        obj.transform.localScale = new Vector3(x, y * -1f, z);
        __result = true;
    }
}

internal static class Patch_HL_Weather_IsTooDarkForAction
{
    internal static void Postfix(Weather __instance, ref bool __result)
    {
        if (!HouseLightsState.IsMenu() && __result &&
            GameManager.GetWeatherComponent().IsIndoorScene() &&
            HouseLightsState.LightsOn)
        {
            __result = false;
        }
    }
}
