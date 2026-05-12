using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppTLD.Gear;
using KeroseneLampTweaks;
using MelonLoader;
using Microsoft.CodeAnalysis;
using ModSettings;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: AssemblyTitle("KeroseneLampTweaks")]
[assembly: AssemblyCopyright("Created by Xpazeman")]
[assembly: AssemblyFileVersion("2.4.1")]
[assembly: MelonInfo(typeof(Main), "KeroseneLampTweaks", "2.4.1", "Romain, Xpazeman", null)]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: TargetFramework(".NETCoreApp,Version=v6.0", FrameworkDisplayName = ".NET 6.0")]
[assembly: AssemblyVersion("2.4.1.0")]
[module: System.Runtime.CompilerServices.RefSafetyRules(11)]
namespace Microsoft.CodeAnalysis
{
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	internal sealed class EmbeddedAttribute : Attribute
	{
	}
}
namespace System.Runtime.CompilerServices
{
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter, AllowMultiple = false, Inherited = false)]
	internal sealed class NullableAttribute : Attribute
	{
		public readonly byte[] NullableFlags;

		public NullableAttribute(byte P_0)
		{
			NullableFlags = new byte[1] { P_0 };
		}

		public NullableAttribute(byte[] P_0)
		{
			NullableFlags = P_0;
		}
	}
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
	internal sealed class NullableContextAttribute : Attribute
	{
		public readonly byte Flag;

		public NullableContextAttribute(byte P_0)
		{
			Flag = P_0;
		}
	}
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	[AttributeUsage(AttributeTargets.Module, AllowMultiple = false, Inherited = false)]
	internal sealed class RefSafetyRulesAttribute : Attribute
	{
		public readonly int Version;

		public RefSafetyRulesAttribute(int P_0)
		{
			Version = P_0;
		}
	}
}
namespace KeroseneLampTweaks
{
	internal class Main : MelonMod
	{
		public override void OnInitializeMelon()
		{
			Settings.OnLoad();
		}

		public static void ColorLamps(GameObject lamp)
		{
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			if (Settings.settings.lampColor == LampColor.Default)
			{
				return;
			}
			Color color = ((!((Object)lamp).name.Contains("Spelunkers") || !Settings.settings.spelunkerColor) ? GetNewColor(Settings.settings.lampColor) : GetNewColor(Settings.settings.spelunkersLampColor, isSpelunkers: true));
			foreach (Light componentsInChild in lamp.GetComponentsInChildren<Light>())
			{
				componentsInChild.color = color;
			}
			foreach (Light component in lamp.GetComponents<Light>())
			{
				component.color = color;
			}
		}

		public static Color GetNewColor(LampColor lampColor, bool isSpelunkers = false)
		{
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_011e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			//IL_018d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0182: Unknown result type (might be due to invalid IL or missing references)
			//IL_0187: Unknown result type (might be due to invalid IL or missing references)
			//IL_018c: Unknown result type (might be due to invalid IL or missing references)
			//IL_014f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0154: Unknown result type (might be due to invalid IL or missing references)
			//IL_0159: Unknown result type (might be due to invalid IL or missing references)
			Color result = default(Color);
			((Color)(ref result))..ctor(0.993f, 0.67f, 0.369f, 1f);
			switch (lampColor)
			{
			case LampColor.Red:
				result = Color32.op_Implicit(new Color32(byte.MaxValue, (byte)105, (byte)92, byte.MaxValue));
				return result;
			case LampColor.Yellow:
				result = Color32.op_Implicit(new Color32(byte.MaxValue, (byte)228, (byte)92, byte.MaxValue));
				return result;
			case LampColor.Blue:
				result = Color32.op_Implicit(new Color32((byte)92, (byte)105, byte.MaxValue, byte.MaxValue));
				return result;
			case LampColor.Cyan:
				result = Color32.op_Implicit(new Color32((byte)92, (byte)225, byte.MaxValue, byte.MaxValue));
				return result;
			case LampColor.Green:
				result = Color32.op_Implicit(new Color32((byte)91, (byte)216, (byte)95, byte.MaxValue));
				return result;
			case LampColor.Purple:
				result = Color32.op_Implicit(new Color32((byte)208, (byte)91, (byte)216, byte.MaxValue));
				return result;
			case LampColor.White:
				result = Color32.op_Implicit(new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
				return result;
			case LampColor.Custom:
				if (!isSpelunkers)
				{
					result = Color32.op_Implicit(new Color32((byte)Settings.settings.lampColorR, (byte)Settings.settings.lampColorG, (byte)Settings.settings.lampColorB, byte.MaxValue));
					return result;
				}
				result = Color32.op_Implicit(new Color32((byte)Settings.settings.spelunkersLampColorR, (byte)Settings.settings.spelunkersLampColorG, (byte)Settings.settings.spelunkersLampColorB, byte.MaxValue));
				return result;
			default:
				return result;
			}
		}
	}
	[HarmonyPatch(typeof(KeroseneLampItem), "ReduceFuel", new Type[] { typeof(float) })]
	public class KeroseneLampItem_ReduceFuel
	{
		public static void Prefix(ref KeroseneLampItem __instance, ref float hoursBurned)
		{
			GearItem gearItem = __instance.m_GearItem;
			float num = 1f;
			if (gearItem.CurrentHP < (float)Settings.settings.conditionThreshold)
			{
				num += (1f - gearItem.CurrentHP / (float)Settings.settings.conditionThreshold) * (float)(Settings.settings.maxPenalty / 100);
			}
			if (!gearItem.m_InPlayerInventory)
			{
				hoursBurned *= Settings.settings.placed_burn_multiplier * num;
			}
			else
			{
				hoursBurned *= Settings.settings.held_burn_multiplier * num;
			}
		}
	}
	[HarmonyPatch(typeof(KeroseneLampItem), "OnIgniteComplete")]
	public class KeroseneLampItem_TurnOn
	{
		public static void Postfix(ref KeroseneLampItem __instance)
		{
			GearItem gearItem = __instance.m_GearItem;
			gearItem.m_CurrentHP -= Settings.settings.turnOnDecay;
		}
	}
	[HarmonyPatch(typeof(KeroseneLampItem), "Update")]
	public class KeroseneLampItem_Update
	{
		private const float INDOOR_DEF_RNG = 25f;

		private const float INDOORCORE_DEF_RNG = 0.12f;

		private const float OUTDOOR_DEF_RNG = 20f;

		public static void Postfix(ref KeroseneLampItem __instance)
		{
			if (__instance.IsOn())
			{
				__instance.m_GearItem.m_CurrentHP = Mathf.Max(0f, __instance.m_GearItem.m_CurrentHP - Settings.settings.overTimeDecay * (Time.deltaTime / 300f) * (1f / GameManager.GetTimeOfDayComponent().m_DayLengthScale));
			}
			if (!__instance.m_GearItem.m_InPlayerInventory && Settings.settings.muteLamps)
			{
				__instance.StopLoopingAudio();
			}
			Light lightIndoor = __instance.m_LightIndoor;
			Light lightIndoorCore = __instance.m_LightIndoorCore;
			Light lightOutdoor = __instance.m_LightOutdoor;
			lightIndoor.range = 25f * Settings.settings.lamp_range;
			lightIndoorCore.range = 0.12f * Settings.settings.lamp_range;
			lightOutdoor.range = 20f * Settings.settings.lamp_range;
		}
	}
	[HarmonyPatch(typeof(FirstPersonLightSource), "TurnOnEffects")]
	internal class FirstPersonLightSource_Start
	{
		private const float INDOOR_DEF_RNG = 25f;

		private const float OUTDOOR_DEF_RNG = 20f;

		public static void Prefix(FirstPersonLightSource __instance)
		{
			if (((Object)((Component)__instance).gameObject).name.Contains("KerosceneLamp") || ((Object)((Component)__instance).gameObject).name.Contains("KeroseneLamp"))
			{
				__instance.m_LightIndoor.range = 25f * Settings.settings.lamp_range;
				__instance.m_LightOutdoor.range = 20f * Settings.settings.lamp_range;
				Main.ColorLamps(((Component)__instance).gameObject);
			}
		}
	}
	[HarmonyPatch(typeof(KeroseneLampIntensity), "Update")]
	internal class KeroseneLampIntensity_Update
	{
		public static void Prefix(KeroseneLampIntensity __instance)
		{
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Expected O, but got Unknown
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			Color color = ((!((Object)((Component)__instance).gameObject).name.Contains("Spelunkers") || !Settings.settings.spelunkerColor) ? Main.GetNewColor(Settings.settings.lampColor) : Main.GetNewColor(Settings.settings.spelunkersLampColor, isSpelunkers: true));
			Gradient val = new Gradient();
			GradientColorKey[] array = (GradientColorKey[])(object)new GradientColorKey[2];
			array[0].color = color;
			array[0].time = 0f;
			array[1].color = color;
			array[1].time = 1f;
			GradientAlphaKey[] array2 = (GradientAlphaKey[])(object)new GradientAlphaKey[2];
			array2[0].alpha = 1f;
			array2[0].time = 0f;
			array2[1].alpha = 1f;
			array2[1].time = 1f;
			val.SetKeys(Il2CppStructArray<GradientColorKey>.op_Implicit(array), Il2CppStructArray<GradientAlphaKey>.op_Implicit(array2));
			__instance.m_GlassColor = val;
			__instance.m_FlameColor = val;
			if (Object.op_Implicit((Object)(object)__instance.m_LitGlass))
			{
				((Renderer)__instance.m_LitGlass.GetComponent<MeshRenderer>()).material.SetColor("_Emission", __instance.m_GlassColor.Evaluate(0f));
			}
			GearItem gearItem = __instance.m_GearItem;
			if (Object.op_Implicit((Object)(object)gearItem))
			{
				Main.ColorLamps(((Component)gearItem.m_KeroseneLampItem).gameObject);
			}
		}
	}
	public enum LampColor
	{
		Default,
		Red,
		Yellow,
		Blue,
		Cyan,
		Green,
		Purple,
		White,
		Custom
	}
	internal class KeroseneLampTweaksSettings : JsonModSettings
	{
		[Section("煤油灯磨损")]
		[Name("开启损耗")]
		[Description("开启煤油灯时耐久度的损耗，默认=0")]
		[Slider(0f, 2f, 21, NumberFormat = "{0:0.00}%")]
		public float turnOnDecay;

		[Name("开启时随时间损耗(每小时)")]
		[Description("开启煤油灯时随着时间的推移耐久度的损耗，默认=0")]
		[Slider(0f, 1f, 101, NumberFormat = "{0:0.00}%")]
		public float overTimeDecay;

		[Section("燃油消耗速率")]
		[Name("放置状态消耗率")]
		[Description("煤油灯被放置时燃油的消耗率。1是默认值(4小时)，0是0消耗，2是消耗翻倍")]
		[Slider(0f, 2f, 201, NumberFormat = "{0:0.00}")]
		public float placed_burn_multiplier = 1f;

		[Name("手持状态消耗率")]
		[Description("用手提着煤油灯时燃油的消耗率。1是默认值(4小时)，0是0消耗，2是消耗翻倍")]
		[Slider(0f, 2f, 201, NumberFormat = "{0:0.00}")]
		public float held_burn_multiplier = 1f;

		[Section("燃油消耗惩罚机制")]
		[Name("煤油灯状态阈值")]
		[Description("高于此状态阈值的煤油灯不会受到燃油消耗惩罚 \n 0%：无惩罚\n 50%：耐久在50%到100%之间的煤油灯不会受到惩罚\n 80%：状态在80%到100%之间的煤油灯不会受到惩罚")]
		[Slider(0f, 100f, 101, NumberFormat = "{0:0}%")]
		public int conditionThreshold = 80;

		[Name("最大惩罚")]
		[Description("当煤油灯状态接近0%时，燃油消耗惩罚达到最大值。\n 当油灯的状态在状态阈值(无惩罚)和0%状态(完全惩罚)之间时，惩罚会线性增加\n 20%：接近阈值时消耗惩罚为0，接近0%状态时消耗惩罚增加20%，中间状态按比例增加\n 100%：接近阈值时消耗惩罚为0，接近0%状态时消耗惩罚增加100%(即翻倍)，中间状态按比例增加")]
		[Slider(0f, 100f, 101, NumberFormat = "{0:0}%")]
		public int maxPenalty = 100;

		[Section("亮度设置")]
		[Name("灯光照射范围修改")]
		[Description("1是默认值(室外20米，室内25米),0表示不发光，2是翻倍")]
		[Slider(0f, 2f, 201, NumberFormat = "{0:0.00}")]
		public float lamp_range = 1f;

		[Name("灯光颜色修改")]
		[Description("颜色修改")]
		[Choice(new string[] { "默认色", "红色", "黄色", "蓝色", "青色", "绿色", "紫色", "白色", "定制色" })]
		public LampColor lampColor;

		[Name("红色灯")]
		[Slider(0f, 255f)]
		public int lampColorR;

		[Name("绿色灯")]
		[Slider(0f, 255f)]
		public int lampColorG;

		[Name("蓝色灯")]
		[Slider(0f, 255f)]
		public int lampColorB;

		[Name("探洞者油灯颜色开关")]
		[Description("开启的话可为探洞者油灯更换不同的颜色")]
		public bool spelunkerColor;

		[Name("探洞者油灯颜色修改")]
		[Description("颜色修改")]
		[Choice(new string[] { "默认色", "红色", "黄色", "蓝色", "青色", "绿色", "紫色", "白色", "定制色" })]
		public LampColor spelunkersLampColor;

		[Name("探洞者红色油灯")]
		[Slider(0f, 255f)]
		public int spelunkersLampColorR;

		[Name("探洞者绿色油灯")]
		[Slider(0f, 255f)]
		public int spelunkersLampColorG;

		[Name("探洞者蓝色油灯")]
		[Slider(0f, 255f)]
		public int spelunkersLampColorB;

		[Section("其他")]
		[Name("煤油灯消音")]
		[Description("使灯具在开启和放置时保持静音")]
		public bool muteLamps;

		protected override void OnChange(FieldInfo field, object oldVal, object newVal)
		{
			RefreshFields();
		}

		internal void RefreshFields()
		{
			if (lampColor == LampColor.Custom)
			{
				SetFieldVisible("lampColorR", visible: true);
				SetFieldVisible("lampColorG", visible: true);
				SetFieldVisible("lampColorB", visible: true);
			}
			else
			{
				SetFieldVisible("lampColorR", visible: false);
				SetFieldVisible("lampColorG", visible: false);
				SetFieldVisible("lampColorB", visible: false);
			}
			SetFieldVisible("spelunkersLampColor", spelunkerColor);
			if (spelunkersLampColor == LampColor.Custom)
			{
				SetFieldVisible("spelunkersLampColorR", visible: true);
				SetFieldVisible("spelunkersLampColorG", visible: true);
				SetFieldVisible("spelunkersLampColorB", visible: true);
			}
			else
			{
				SetFieldVisible("spelunkersLampColorR", visible: false);
				SetFieldVisible("spelunkersLampColorG", visible: false);
				SetFieldVisible("spelunkersLampColorB", visible: false);
			}
		}
	}
	internal static class Settings
	{
		public static KeroseneLampTweaksSettings settings;

		public static void OnLoad()
		{
			settings.RefreshFields();
			settings.AddToModSettings("煤油灯DIY定制v2.4.1");
		}

		static Settings()
		{
			settings = new KeroseneLampTweaksSettings();
		}
	}
}
