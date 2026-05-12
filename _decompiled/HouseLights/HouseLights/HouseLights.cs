using System;
using System.Collections.Generic;
using System.Linq;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppTLD.ModularElectrolizer;
using MelonLoader;
using UnityEngine;

namespace HouseLights;

internal class HouseLights : MelonMod
{
	private static AssetBundle? assetBundle;

	public static bool lightsOn = false;

	public static List<ElectrolizerConfig> electroSources = new List<ElectrolizerConfig>();

	public static List<ElectrolizerLightConfig> electroLightSources = new List<ElectrolizerLightConfig>();

	public static List<GameObject> orgObj = new List<GameObject>();

	public static List<GameObject> result = new List<GameObject>();

	public static List<GameObject> lightSwitches = new List<GameObject>();

	public static List<string> notReallyOutdoors = new List<string> { "DamTransitionZone" };

	private int type = 0;

	private RaycastHit hit;

	internal static AssetBundle HLbundle => assetBundle ?? throw new NullReferenceException("assetBundle");

	public override void OnInitializeMelon()
	{
		Settings.OnLoad();
		assetBundle = HouseLightsUtils.LoadFromStream("HouseLights.hlbundle");
		RegisterCommands();
	}

	internal static void Init()
	{
		electroSources.Clear();
		lightSwitches.Clear();
		lightsOn = false;
	}

	internal static void AddElectrolizer(AuroraModularElectrolizer light)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		ElectrolizerConfig electrolizerConfig = new ElectrolizerConfig();
		electrolizerConfig.electrolizer = light;
		electrolizerConfig.ranges = new float[light.m_LocalLights._size];
		electrolizerConfig.colors = (Color[])(object)new Color[light.m_LocalLights._size];
		ElectrolizerConfig electrolizerConfig2 = electrolizerConfig;
		for (int i = 0; i < light.m_LocalLights._size; i++)
		{
			float range = light.m_LocalLights[i].range;
			Color color = light.m_LocalLights[i].color;
			electrolizerConfig2.ranges[i] = range;
			electrolizerConfig2.colors[i] = color;
		}
		electroSources.Add(electrolizerConfig2);
	}

	internal static void AddElectrolizerLight(AuroraLightingSimple light)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		ElectrolizerLightConfig electrolizerLightConfig = new ElectrolizerLightConfig();
		electrolizerLightConfig.electrolizer = light;
		electrolizerLightConfig.ranges = new float[((Il2CppArrayBase<Light>)(object)light.m_LocalLights).Length];
		electrolizerLightConfig.colors = (Color[])(object)new Color[((Il2CppArrayBase<Light>)(object)light.m_LocalLights).Length];
		ElectrolizerLightConfig electrolizerLightConfig2 = electrolizerLightConfig;
		for (int i = 0; i < ((Il2CppArrayBase<Light>)(object)light.m_LocalLights).Length; i++)
		{
			float range = ((Il2CppArrayBase<Light>)(object)light.m_LocalLights)[i].range;
			Color color = ((Il2CppArrayBase<Light>)(object)light.m_LocalLights)[i].color;
			electrolizerLightConfig2.ranges[i] = range;
			electrolizerLightConfig2.colors[i] = color;
		}
		electroLightSources.Add(electrolizerLightConfig2);
	}

	internal static void GetSwitches()
	{
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		List<GameObject> rootObjects = HouseLightsUtils.GetRootObjects();
		List<GameObject> list = new List<GameObject>();
		List<GameObject> source = new List<GameObject>();
		int num = 0;
		orgObj = new List<GameObject>();
		foreach (GameObject item in rootObjects)
		{
			HouseLightsUtils.GetChildrenWithName(item, "houselightswitch", orgObj);
			HouseLightsUtils.GetChildrenWithName(item, "lightswitcha", orgObj);
			HouseLightsUtils.GetChildrenWithName(item, "lightswitchblack", orgObj);
			HouseLightsUtils.GetChildrenWithName(item, "switch_a_black", orgObj);
			HouseLightsUtils.GetChildrenWithName(item, "switch_a_white", orgObj);
			HouseLightsUtils.GetChildrenWithName(item, "switch_a_purple", orgObj);
			HouseLightsUtils.GetChildrenWithName(item, "switch_b_white", orgObj);
			foreach (GameObject item2 in orgObj)
			{
				if (item2.active)
				{
					int variant = ((((Object)item2).name.ToLowerInvariant().Contains("houselightswitch") || ((Object)item2).name.ToLowerInvariant().Contains("lightswitcha")) ? 1 : 2);
					item2.active = false;
					Vector3 position = item2.transform.position;
					Quaternion rotation = item2.transform.rotation;
					GameObject val = HouseLightsUtils.InstantiateSwitch(position, ((Quaternion)(ref rotation)).eulerAngles, variant);
					GameObject gameObject = ((Component)val.transform.FindChild("SM_LightSwitchBlack")).gameObject;
					list.Add(val);
					gameObject.layer = 19;
					lightSwitches.Add(gameObject);
					((Object)gameObject).name = "MOD_HouseLightSwitch";
					num++;
					if (!Object.op_Implicit((Object)(object)((Component)gameObject.transform).GetComponent<Collider>()))
					{
						BoxCollider val2 = gameObject.AddComponent<BoxCollider>();
						val2.size = new Vector3(0.1f, 0.1f, 0.1f);
					}
				}
			}
		}
		if (Settings.options.Debug)
		{
			MelonLogger.Msg("Light switches found: " + num + ".");
			MelonLogger.Msg("Custom switches created: " + source.Count() + ".");
		}
	}

	internal static void ToggleLightsState()
	{
		lightsOn = !lightsOn;
	}

	internal static void UpdateElectroLights(AuroraManager mngr)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0576: Unknown result type (might be due to invalid IL or missing references)
		//IL_057b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0686: Unknown result type (might be due to invalid IL or missing references)
		//IL_068b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0690: Unknown result type (might be due to invalid IL or missing references)
		//IL_06cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d1: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)GameManager.GetVpFPSPlayer()).gameObject.transform.position;
		for (int i = 0; i < electroSources.Count; i++)
		{
			if (!((Object)(object)electroSources[i].electrolizer != (Object)null) || electroSources[i].electrolizer.m_LocalLights == null)
			{
				continue;
			}
			float num = Mathf.Abs(Vector3.Distance(((Component)electroSources[i].electrolizer).gameObject.transform.position, position));
			if (num > (float)Settings.options.cullDistance && !mngr.AuroraIsActive())
			{
				electroSources[i].electrolizer.UpdateIntensity(1f, 0f);
				electroSources[i].electrolizer.UpdateLight(true);
				electroSources[i].electrolizer.UpdateEmissiveObjects(true);
				electroSources[i].electrolizer.UpdateAudio();
				continue;
			}
			for (int j = 0; j < electroSources[i].electrolizer.m_LocalLights._size; j++)
			{
				float num2 = electroSources[i].ranges[j];
				num2 *= Settings.options.rangeMultiplier;
				num2 = Math.Min(num2, 20f);
				electroSources[i].electrolizer.m_LocalLights[j].range = num2;
				electroSources[i].electrolizer.m_HasFlickerSet = !Settings.options.disableAuroraFlicker;
				ColorHSV val = ColorHSV.op_Implicit(electroSources[i].colors[j]);
				if (Settings.options.whiteLights)
				{
					val.s *= 0.15f;
				}
				electroSources[i].electrolizer.m_LocalLights[j].color = ColorHSV.op_Implicit(val);
				if (Settings.options.castShadows)
				{
					electroSources[i].electrolizer.m_LocalLights[j].shadows = (LightShadows)2;
				}
			}
			if (lightsOn && !mngr.AuroraIsActive())
			{
				if (!((Object)((Component)electroSources[i].electrolizer).gameObject).name.Contains("Alarm") && !((Object)((Component)electroSources[i].electrolizer).gameObject).name.Contains("Headlight") && !((Object)((Component)electroSources[i].electrolizer).gameObject).name.Contains("Taillight") && !((Object)((Component)electroSources[i].electrolizer).gameObject).name.Contains("Television") && !((Object)((Component)electroSources[i].electrolizer).gameObject).name.Contains("Computer") && !((Object)((Component)electroSources[i].electrolizer).gameObject).name.Contains("Machine") && !((Object)((Component)electroSources[i].electrolizer).gameObject).name.Contains("ControlBox") && !((Object)((Component)electroSources[i].electrolizer).gameObject).name.Contains("Interiorlight"))
				{
					electroSources[i].electrolizer.UpdateIntensity(1f, Settings.options.intensityValue);
					electroSources[i].electrolizer.UpdateLight(false);
					electroSources[i].electrolizer.UpdateEmissiveObjects(false);
					if (Settings.options.LightAudio)
					{
						electroSources[i].electrolizer.UpdateAudio();
					}
					else
					{
						electroSources[i].electrolizer.StopAudio();
					}
				}
			}
			else if (!mngr.AuroraIsActive())
			{
				electroSources[i].electrolizer.UpdateIntensity(1f, 0f);
				electroSources[i].electrolizer.UpdateLight(true);
				electroSources[i].electrolizer.UpdateEmissiveObjects(true);
				electroSources[i].electrolizer.UpdateAudio();
			}
			else
			{
				electroSources[i].electrolizer.UpdateIntensity(Time.deltaTime, mngr.m_NormalizedActive);
			}
		}
		for (int k = 0; k < electroLightSources.Count; k++)
		{
			if (!((Object)(object)electroLightSources[k].electrolizer != (Object)null) || electroLightSources[k].electrolizer.m_LocalLights == null)
			{
				continue;
			}
			float num3 = Mathf.Abs(Vector3.Distance(((Component)electroLightSources[k].electrolizer).gameObject.transform.position, position));
			if (num3 > (float)Settings.options.cullDistance && !mngr.AuroraIsActive())
			{
				electroLightSources[k].electrolizer.m_CurIntensity = 0f;
				electroLightSources[k].electrolizer.UpdateLight(true);
				electroLightSources[k].electrolizer.UpdateEmissiveObjects(true);
				electroLightSources[k].electrolizer.UpdateAudio();
				continue;
			}
			for (int l = 0; l < ((Il2CppArrayBase<Light>)(object)electroLightSources[k].electrolizer.m_LocalLights).Length; l++)
			{
				float num4 = electroLightSources[k].ranges[l];
				num4 *= Settings.options.rangeMultiplier;
				num4 = Math.Min(num4, 20f);
				((Il2CppArrayBase<Light>)(object)electroLightSources[k].electrolizer.m_LocalLights)[l].range = num4;
				ColorHSV val2 = ColorHSV.op_Implicit(electroLightSources[k].colors[l]);
				if (Settings.options.whiteLights)
				{
					val2.s *= 0.15f;
				}
				((Il2CppArrayBase<Light>)(object)electroLightSources[k].electrolizer.m_LocalLights)[l].color = ColorHSV.op_Implicit(val2);
				if (Settings.options.castShadows)
				{
					((Il2CppArrayBase<Light>)(object)electroLightSources[k].electrolizer.m_LocalLights)[l].shadows = (LightShadows)2;
				}
			}
			if (lightsOn && !mngr.AuroraIsActive())
			{
				if (!((Object)((Component)electroLightSources[k].electrolizer).gameObject).name.Contains("Alarm") && !((Object)((Component)electroLightSources[k].electrolizer).gameObject).name.Contains("Headlight") && !((Object)((Component)electroLightSources[k].electrolizer).gameObject).name.Contains("Taillight") && !((Object)((Component)electroLightSources[k].electrolizer).gameObject).name.Contains("Television") && !((Object)((Component)electroLightSources[k].electrolizer).gameObject).name.Contains("Computer") && !((Object)((Component)electroLightSources[k].electrolizer).gameObject).name.Contains("Machine") && !((Object)((Component)electroLightSources[k].electrolizer).gameObject).name.Contains("ControlBox") && !((Object)((Component)electroLightSources[k].electrolizer).gameObject).name.Contains("Interiorlight"))
				{
					electroLightSources[k].electrolizer.m_CurIntensity = Settings.options.intensityValue;
					electroLightSources[k].electrolizer.UpdateLight(false);
					electroLightSources[k].electrolizer.UpdateEmissiveObjects(false);
					if (Settings.options.LightAudio)
					{
						electroSources[k].electrolizer.UpdateAudio();
					}
					else
					{
						electroSources[k].electrolizer.StopAudio();
					}
				}
			}
			else if (!mngr.AuroraIsActive())
			{
				electroLightSources[k].electrolizer.m_CurIntensity = 0f;
				electroLightSources[k].electrolizer.UpdateLight(true);
				electroLightSources[k].electrolizer.UpdateEmissiveObjects(true);
				electroLightSources[k].electrolizer.UpdateAudio();
			}
			else
			{
				electroLightSources[k].electrolizer.UpdateIntensity(Time.deltaTime);
			}
		}
	}

	internal static void RegisterCommands()
	{
		uConsole.RegisterCommand("thl", DebugCommand.op_Implicit((Action)ToggleLightsState));
	}

	public static void InstantiateCustomSwitches(string sceneName)
	{
		//IL_04be: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0501: Unknown result type (might be due to invalid IL or missing references)
		//IL_051c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0530: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0600: Unknown result type (might be due to invalid IL or missing references)
		//IL_0550: Unknown result type (might be due to invalid IL or missing references)
		//IL_0564: Unknown result type (might be due to invalid IL or missing references)
		//IL_084d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0861: Unknown result type (might be due to invalid IL or missing references)
		//IL_087c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0890: Unknown result type (might be due to invalid IL or missing references)
		//IL_039f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0411: Unknown result type (might be due to invalid IL or missing references)
		//IL_042c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0440: Unknown result type (might be due to invalid IL or missing references)
		//IL_045b: Unknown result type (might be due to invalid IL or missing references)
		//IL_046f: Unknown result type (might be due to invalid IL or missing references)
		//IL_048a: Unknown result type (might be due to invalid IL or missing references)
		//IL_049e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0620: Unknown result type (might be due to invalid IL or missing references)
		//IL_0634: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_034b: Unknown result type (might be due to invalid IL or missing references)
		//IL_077d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0791: Unknown result type (might be due to invalid IL or missing references)
		//IL_071a: Unknown result type (might be due to invalid IL or missing references)
		//IL_072e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0749: Unknown result type (might be due to invalid IL or missing references)
		//IL_075d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0654: Unknown result type (might be due to invalid IL or missing references)
		//IL_0668: Unknown result type (might be due to invalid IL or missing references)
		//IL_0683: Unknown result type (might be due to invalid IL or missing references)
		//IL_0697: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0584: Unknown result type (might be due to invalid IL or missing references)
		//IL_0598: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_037f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0942: Unknown result type (might be due to invalid IL or missing references)
		//IL_0956: Unknown result type (might be due to invalid IL or missing references)
		//IL_0971: Unknown result type (might be due to invalid IL or missing references)
		//IL_0985: Unknown result type (might be due to invalid IL or missing references)
		//IL_0819: Unknown result type (might be due to invalid IL or missing references)
		//IL_082d: Unknown result type (might be due to invalid IL or missing references)
		//IL_09d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_09e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_09a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_09b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_06fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_08df: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_090e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0922: Unknown result type (might be due to invalid IL or missing references)
		switch (sceneName.ToLowerInvariant())
		{
		case "lakeregion":
			HouseLightsUtils.InstantiateSwitch(new Vector3(791.93f, 214.38f, 965.76f), new Vector3(0f, 265f, 0f), 0);
			break;
		case "trailera":
			HouseLightsUtils.InstantiateSwitch(new Vector3(-2.95f, 1.38f, 2.06f), new Vector3(0f, 180f, 0f), 0);
			break;
		case "communityhalla":
			HouseLightsUtils.InstantiateSwitch(new Vector3(0.128f, 1.38f, 4.2f), new Vector3(0f, 0f, 0f), 1);
			HouseLightsUtils.InstantiateSwitch(new Vector3(8.52f, 1.4f, 0.26f), new Vector3(0f, 0f, 0f), 1);
			HouseLightsUtils.InstantiateSwitch(new Vector3(6.91f, 1.54f, -3.97f), new Vector3(0f, 90f, 0f), 1);
			HouseLightsUtils.InstantiateSwitch(new Vector3(6.58f, 1.43f, 0.22f), new Vector3(0f, 270f, 0f), 3);
			HouseLightsUtils.InstantiateSwitch(new Vector3(-9.2f, 2.24f, 3.16f), new Vector3(0f, 180f, 0f), 3);
			HouseLightsUtils.InstantiateSwitch(new Vector3(9.05f, 1.41f, 0.16f), new Vector3(0f, 180f, 0f), 0);
			break;
		case "trailersshape":
			HouseLightsUtils.InstantiateSwitch(new Vector3(7.2f, 1.5f, -10.12f), new Vector3(0f, 0f, 0f), 1);
			HouseLightsUtils.InstantiateSwitch(new Vector3(0.59f, 1.55f, -5.92f), new Vector3(0f, 0f, 0f), 1);
			HouseLightsUtils.InstantiateSwitch(new Vector3(-6.53f, 1.52f, 3.76f), new Vector3(0f, 0f, 0f), 1);
			break;
		case "trailerb":
			HouseLightsUtils.InstantiateSwitch(new Vector3(3.88f, 1.34f, 2.07f), new Vector3(0f, 180f, 0f), 3);
			break;
		case "trailerc":
			HouseLightsUtils.InstantiateSwitch(new Vector3(-0.88f, 1.32f, 2.07f), new Vector3(0f, 180f, 0f), 0);
			break;
		case "trailerd":
			HouseLightsUtils.InstantiateSwitch(new Vector3(-3f, 1.37f, 2.07f), new Vector3(0f, 180f, 0f), 1);
			break;
		case "trailere":
			HouseLightsUtils.InstantiateSwitch(new Vector3(-2.94f, 1.34f, 2.07f), new Vector3(0f, 180f, 0f), 0);
			break;
		case "tracksregion":
			HouseLightsUtils.InstantiateSwitch(new Vector3(586.17f, 200.48f, 564.31f), new Vector3(0f, 270f, 0f), 3);
			break;
		case "mountainpassburiedcabin":
			HouseLightsUtils.InstantiateSwitch(new Vector3(3.89f, 1.26f, 0.82f), new Vector3(0f, 270f, 0f), 1);
			HouseLightsUtils.InstantiateSwitch(new Vector3(-0.52f, 5.17f, 1.711f), new Vector3(0f, 180f, 0f), 1);
			HouseLightsUtils.InstantiateSwitch(new Vector3(-0.54f, 5.13f, 1.78f), new Vector3(0f, 0f, 0f), 1);
			break;
		case "miltontrailerb":
			HouseLightsUtils.InstantiateSwitch(new Vector3(3.92f, 1.5f, 2.07f), new Vector3(0f, 180f, 0f), 3);
			break;
		case "huntinglodgea":
			HouseLightsUtils.InstantiateSwitch(new Vector3(7.27f, 1.18f, -1.58f), new Vector3(0f, 90f, 0f), 0);
			HouseLightsUtils.InstantiateSwitch(new Vector3(-1.17f, 1.51f, -5.01f), new Vector3(0f, 0f, 0f), 3);
			break;
		case "damtrailerb":
			HouseLightsUtils.InstantiateSwitch(new Vector3(4.01f, 1.49f, 2.07f), new Vector3(0f, 180f, 0f), 0);
			break;
		case "crashmountainregion":
			HouseLightsUtils.InstantiateSwitch(new Vector3(889.93f, 162.08f, 346.07f), new Vector3(0f, 180f, 0f), 3);
			break;
		case "coastalregion":
			HouseLightsUtils.InstantiateSwitch(new Vector3(757.9f, 25.51f, 646.78f), new Vector3(0f, 50f, 0f), 0);
			break;
		case "cannerytrailera":
			HouseLightsUtils.InstantiateSwitch(new Vector3(-3.02f, 1.42f, 2.79f), new Vector3(0f, 180f, 0f), 3);
			break;
		case "bunkerc":
			HouseLightsUtils.InstantiateSwitch(new Vector3(1.09f, 1.73f, 3.54f), new Vector3(0f, 0f, 0f), 3);
			HouseLightsUtils.InstantiateSwitch(new Vector3(-14.72f, 0.33f, 12.93f), new Vector3(0f, 0f, 0f), 3);
			break;
		case "bunkerb":
			HouseLightsUtils.InstantiateSwitch(new Vector3(1.13f, 1.67f, 3.54f), new Vector3(0f, 0f, 0f), 3);
			HouseLightsUtils.InstantiateSwitch(new Vector3(2.94f, 1.54f, 7.68f), new Vector3(0f, 90f, 0f), 3);
			HouseLightsUtils.InstantiateSwitch(new Vector3(-3.19f, 1.61f, 7.66f), new Vector3(0f, 270f, 0f), 3);
			break;
		case "bunkera":
			HouseLightsUtils.InstantiateSwitch(new Vector3(5.93f, 1.61f, 12.63f), new Vector3(0f, 180f, 0f), 3);
			HouseLightsUtils.InstantiateSwitch(new Vector3(1.18f, 1.65f, 1.62f), new Vector3(0f, 0f, 0f), 3);
			break;
		case "blackrocktrailerb":
			HouseLightsUtils.InstantiateSwitch(new Vector3(3.9726f, 1.4155f, 2.0799f), new Vector3(0f, 180f, 0f), 1);
			break;
		case "airfieldtrailerb":
			HouseLightsUtils.InstantiateSwitch(new Vector3(4.01f, 1.49f, 2.07f), new Vector3(0f, 180f, 0f), 0);
			break;
		}
	}

	public override void OnUpdate()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		if (!Settings.options.Placer)
		{
			return;
		}
		if (!GameManager.IsMainMenuActive() && (Object)(object)InputManager.instance != (Object)null && InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)61))
		{
			HUDMessage.AddMessage("Placed switch", false, false);
			if (Physics.Raycast(((Component)GameManager.GetMainCamera()).transform.position, ((Component)GameManager.GetMainCamera()).transform.TransformDirection(Vector3.forward), ref hit, 5f))
			{
				Vector3 point = ((RaycastHit)(ref hit)).point;
				Quaternion rotation = ((RaycastHit)(ref hit)).transform.rotation;
				Vector3 eulerAngles = ((Quaternion)(ref rotation)).eulerAngles;
				HouseLightsUtils.InstantiateSwitch(point, eulerAngles, type);
				GameAudioManager.PlaySound("Play_FlashlightOn", GameManager.GetPlayerObject());
			}
		}
		if (!GameManager.IsMainMenuActive() && (Object)(object)InputManager.instance != (Object)null && InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)45))
		{
			GameAudioManager.PlaySound("Play_FlashlightOff", GameManager.GetPlayerObject());
			switch (type)
			{
			case 0:
				HUDMessage.AddMessage("Switch Type: Industrial 02.", false, false);
				type = 3;
				break;
			case 1:
				HUDMessage.AddMessage("Switch Type: Industrial 01.", false, false);
				type = 0;
				break;
			case 3:
				HUDMessage.AddMessage("Switch Type: House Switch.", false, false);
				type = 1;
				break;
			case 2:
				break;
			}
		}
	}
}
