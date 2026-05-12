using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using Microsoft.CodeAnalysis;
using ModSettings;
using SilentWalker;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints | DebuggableAttribute.DebuggingModes.EnableEditAndContinue)]
[assembly: AssemblyTitle("SilentWalker")]
[assembly: AssemblyDescription(null)]
[assembly: AssemblyCompany(null)]
[assembly: AssemblyProduct("Fox Walker")]
[assembly: AssemblyCopyright("Copyright © 2024 (MIT License)")]
[assembly: AssemblyTrademark(null)]
[assembly: AssemblyFileVersion("1.0.0")]
[assembly: MelonInfo(typeof(Main), "Silent Walker", "1.0.0", "Lycanthor", "https://github.com/JesseWV/TLD-SilentWalker/releases/download/v1.0.0/SilentWalker.dll")]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: VerifyLoaderVersion("0.6.6", true)]
[assembly: MelonPriority(0)]
[assembly: MelonIncompatibleAssemblies(null)]
[assembly: TargetFramework(".NETCoreApp,Version=v6.0", FrameworkDisplayName = ".NET 6.0")]
[assembly: AssemblyVersion("1.0.0.0")]
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
namespace SilentWalker
{
	public static class BuildInfo
	{
		public const string Name = "SilentWalker";

		public const string Author = "Lycanthor";

		public const string Version = "1.0.0";

		public const string GUIName = "Silent Walker";

		public const string MelonLoaderVersion = "0.6.6";

		public const string[] PreviousAuthors = null;

		public const string Description = null;

		public const string Company = null;

		public const string DownloadLink = "https://github.com/JesseWV/TLD-SilentWalker/releases/download/v1.0.0/SilentWalker.dll";

		public const string Copyright = "Copyright © 2024 (MIT License)";

		public const string Trademark = null;

		public const string Product = "Fox Walker";

		public const string Culture = null;

		public const int Priority = 0;
	}
	public class Main : MelonMod
	{
		internal static Instance Logger { get; } = new Instance("SilentWalker");


		public override void OnInitializeMelon()
		{
			Logger.Msg("Version " + ((MelonBase)this).Info.Version + " loaded!");
			Settings.OnLoad();
		}
	}
	internal class Patches : MelonMod
	{
		[HarmonyPatch(typeof(GameAudioManager))]
		public static class FootStepSoundLevelPatches
		{
			private static uint lastLoggedID;

			private static float lastLoggedPercentage;

			[HarmonyPatch("SetRTPCValue", new Type[]
			{
				typeof(uint),
				typeof(float),
				typeof(GameObject)
			})]
			[HarmonyPrefix]
			public static void AdjustFootStepVolume(uint rtpcID, ref float rtpcValue, GameObject go)
			{
				int num = 100;
				float value = rtpcValue;
				switch (rtpcID)
				{
				default:
					return;
				case 2064316281u:
					num = Settings.Instance.inventoryWeightGeneralVolume;
					break;
				case 135115684u:
					num = Settings.Instance.inventoryWeightMetalVolume;
					break;
				case 1721946080u:
					num = Settings.Instance.inventoryWeightWaterVolume;
					break;
				case 330491720u:
					num = Settings.Instance.inventoryWeightWoodVolume;
					break;
				}
				rtpcValue *= (float)num / 100f;
				if (!Settings.debug_log)
				{
					return;
				}
				Dictionary<uint, string> dictionary = new Dictionary<uint, string>
				{
					{ 2064316281u, "INVENTORYWEIGHTGENERAL" },
					{ 135115684u, "INVENTORYWEIGHTMETAL" },
					{ 1721946080u, "INVENTORYWEIGHTWATER" },
					{ 330491720u, "INVENTORYWEIGHTWOOD" }
				};
				if (lastLoggedID != rtpcID || lastLoggedPercentage != (float)num)
				{
					lastLoggedID = rtpcID;
					lastLoggedPercentage = num;
					if (dictionary.TryGetValue(rtpcID, out var value2))
					{
						Main.Logger.Msg($"{value2}: {value} -> {rtpcValue} ({num}%)");
					}
				}
			}
		}

		[HarmonyPatch(typeof(FootStepSounds))]
		public static class FootStepSoundDisablePatches
		{
			[HarmonyPatch("PlayFootStepSound", new Type[]
			{
				typeof(Vector3),
				typeof(string),
				typeof(State)
			})]
			[HarmonyPrefix]
			public static bool SuppressFootsteps()
			{
				if (Settings.Instance.silenceFootSteps)
				{
					return false;
				}
				return true;
			}
		}
	}
	public class Settings : JsonModSettings
	{
		internal static bool debug_log;

		[Name("无声脚步")]
		[Description("完全消除自身脚步声\n启用该功能后，将忽略以下设置\n(设置会立即生效)")]
		public bool silenceFootSteps;

		[Name("金属声音")]
		[Description("修改库存中金属物品的声音，默认=100%\n(更改此设置后需要重新加载场景才能生效)")]
		[Slider(1f, 100f, NumberFormat = "{0:0}%")]
		public int inventoryWeightMetalVolume = 100;

		[Name("木材声音")]
		[Description("修改库存中木材物品的声音，默认=100%\n(更改此设置后需要重新加载场景才能生效)")]
		[Slider(1f, 100f, NumberFormat = "{0:0}%")]
		public int inventoryWeightWoodVolume = 100;

		[Name("水资源声音")]
		[Description("修改库存中水资源的声音，默认=100%\n(更改此设置后需要重新加载场景才能生效)")]
		[Slider(1f, 100f, NumberFormat = "{0:0}%")]
		public int inventoryWeightWaterVolume = 100;

		[Name("其他声音")]
		[Description("修改库存中其他物品的声音，默认=100%\n(更改此设置后需要重新加载场景才能生效)")]
		[Slider(1f, 100f, NumberFormat = "{0:0}%")]
		public int inventoryWeightGeneralVolume = 100;

		internal static Settings Instance { get; }

		internal static void OnLoad()
		{
			Instance.AddToModSettings("可定制的行走噪音");
			Instance.RefreshGUI();
		}

		protected override void OnChange(FieldInfo field, object oldValue, object newValue)
		{
			base.OnChange(field, oldValue, newValue);
			Instance.RefreshGUI();
		}

		static Settings()
		{
			Instance = new Settings();
			debug_log = true;
		}
	}
}
