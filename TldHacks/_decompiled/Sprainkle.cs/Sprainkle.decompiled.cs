using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using HarmonyLib;
using Il2Cpp;
using Il2CppSystem.Text.RegularExpressions;
using MelonLoader;
using Microsoft.CodeAnalysis;
using ModSettings;
using Sprainkle;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints | DebuggableAttribute.DebuggingModes.EnableEditAndContinue)]
[assembly: AssemblyTitle("Sprainkle")]
[assembly: AssemblyDescription(null)]
[assembly: AssemblyCompany(null)]
[assembly: AssemblyProduct("Sprainkle")]
[assembly: AssemblyCopyright("Copyright © 2023")]
[assembly: AssemblyTrademark(null)]
[assembly: AssemblyFileVersion("1.0.0")]
[assembly: MelonInfo(typeof(Main), "Sprainkle", "1.0.0", "The Illusion", null)]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: VerifyLoaderVersion("0.6.1", true)]
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
namespace Sprainkle
{
	public static class BuildInfo
	{
		public const string Name = "Sprainkle";

		public const string Author = "The Illusion";

		public const string Version = "1.0.0";

		public const string GUIName = "Sprainkle";

		public const string MelonLoaderVersion = "0.6.1";

		public const string Description = null;

		public const string Company = null;

		public const string DownloadLink = null;

		public const string Copyright = "Copyright © 2023";

		public const string Trademark = null;

		public const string Product = "Sprainkle";

		public const string Culture = null;

		public const int Priority = 0;
	}
	public class Settings : JsonModSettings
	{
		public enum Presets
		{
			Vanilla,
			Custom,
			Enhanced
		}

		[Section("常用设置")]
		[Name("启用MOD")]
		public bool EnableMod = true;

		[Name("预设")]
		[Description("(vanilla)默认/(custom)自定义/(enhanced)强化")]
		public Presets m_Presets;

		[Section("扭伤风险")]
		[Name("扭伤的最小坡度")]
		[Slider(0f, 90f, 91)]
		public int General_MinimumSlope = 30;

		[Name("扭伤几率增值")]
		[Description("在高于最小坡度时的扭伤几率")]
		[Slider(0f, 100f, 401)]
		public float General_MinimumSlopeIncrease = 1.5f;

		[Name("在斜坡上移动时的扭伤几率")]
		[Slider(0f, 100f, 401)]
		public float General_BaseChanceMoving = 15f;

		[Name("因负重而引发的扭伤风险几率")]
		[Slider(0f, 100f, 1001)]
		public float General_EncumberanceChance = 0.3f;

		[Name("因疲劳而引发的扭伤风险几率")]
		[Slider(0f, 100f, 1001)]
		public float General_ExhaustionChance = 0.3f;

		[Name("因冲刺而引发的扭伤风险几率")]
		[Slider(0f, 100f, 1001)]
		public float General_SprintingChance = 2f;

		[Name("因蹲伏而引发的扭伤风险几率")]
		[Description("The amount to reduce the chance of sprains when moving while crouched. 100: never sprain when crouched.")]
		[Slider(0f, 100f, 1001)]
		public float General_CrouchingChance = 75f;

		[Name("扭伤风险发生前的最小秒数")]
		[Description("在斜坡上移动多长时间才会触发扭伤风险")]
		[Slider(0f, 60f, 241)]
		public float General_MinSecondsRisk = 1.5f;

		[Name("移动时手腕扭伤几率")]
		[Slider(0f, 100f, 1001)]
		public float General_WristMovementChance = 50f;

		[Section("扭伤风险UI")]
		[Name("开启")]
		[Description("玩家在斜坡上移动多久才会显示警告UI，应低于扭伤风险发生前的最小秒数")]
		[Slider(0f, 10f, 101)]
		public float General_SprintUIOn = 0.5f;

		[Name("关闭")]
		[Description("玩家离开斜坡多久才会隐藏警告UI，应低于扭伤风险发生前的最小秒数")]
		[Slider(0f, 10f, 101)]
		public float General_SprintUIOff = 0.3f;

		[Section("脚裸选项")]
		[Name("允许脚裸扭伤")]
		[Description("禁用时，脚裸扭伤不会发生")]
		public bool AnkleSettings_Enabled = true;

		[Name("持续时间最小值")]
		[Slider(0f, 672f, 673)]
		public float AnkleSettings_Duration_Minimum = 48f;

		[Name("持续时间最大值")]
		[Slider(0f, 672f, 673)]
		public float AnkleSettings_Duration_Maximum = 72f;

		[Name("休息治愈时长")]
		[Slider(1f, 72f, 72)]
		public float AnkleSettings_RestHours = 4f;

		[Name("高处坠落的扭伤几率")]
		[Slider(0f, 100f, 401)]
		public float AnkleSettings_Chances_Fall = 35f;

		[Section("手腕选项")]
		[Name("允许手腕扭伤")]
		[Description("禁用时，手腕扭伤不会发生")]
		public bool WristSettings_Enabled = true;

		[Name("持续时间最小值")]
		[Slider(0f, 672f, 673)]
		public float WristSettings_Duration_Minimum = 48f;

		[Name("持续时间最大值")]
		[Slider(0f, 672f, 673)]
		public float WristSettings_Duration_Maximum = 72f;

		[Name("休息治愈时长")]
		[Slider(0f, 72f, 73)]
		public float WristSettings_RestHours = 2f;

		[Name("高处坠落的扭伤几率")]
		[Slider(0f, 100f, 401)]
		public float WristSettings_Chances_Fall = 35f;

		internal static Settings Instance { get; }

		protected override void OnConfirm()
		{
			PresetBuilder();
			base.OnConfirm();
		}

		protected override void OnChange(FieldInfo field, object oldValue, object newValue)
		{
			SetDisplay(Instance.EnableMod, Instance.EnableMod);
			base.OnChange(field, oldValue, newValue);
		}

		private void SetDisplay(bool active, bool preset)
		{
			SetFieldVisible("m_Presets", preset);
			SetFieldVisible("General_MinimumSlope", active);
			SetFieldVisible("General_MinimumSlopeIncrease", active);
			SetFieldVisible("General_EncumberanceChance", active);
			SetFieldVisible("General_ExhaustionChance", active);
			SetFieldVisible("General_SprintingChance", active);
			SetFieldVisible("General_CrouchingChance", active);
			SetFieldVisible("General_MinSecondsRisk", active);
			SetFieldVisible("General_WristMovementChance", active);
			SetFieldVisible("General_SprintUIOn", active);
			SetFieldVisible("General_SprintUIOff", active);
			SetFieldVisible("AnkleSettings_Duration_Minimum", active);
			SetFieldVisible("AnkleSettings_Duration_Maximum", active);
			SetFieldVisible("AnkleSettings_Chances_Fall", active);
			SetFieldVisible("AnkleSettings_RestHours", active);
			SetFieldVisible("WristSettings_Duration_Minimum", active);
			SetFieldVisible("WristSettings_Duration_Maximum", active);
			SetFieldVisible("WristSettings_Chances_Fall", active);
			SetFieldVisible("WristSettings_RestHours", active);
		}

		private void PresetBuilder()
		{
			switch (Instance.m_Presets)
			{
			case Presets.Vanilla:
				SetDisplay(active: false, preset: true);
				break;
			case Presets.Custom:
				SetDisplay(active: true, preset: true);
				break;
			case Presets.Enhanced:
				SetDisplay(active: false, preset: true);
				Instance.General_MinimumSlope = 70;
				Instance.General_MinimumSlopeIncrease = 0.25f;
				Instance.General_EncumberanceChance = 0f;
				Instance.General_ExhaustionChance = 0f;
				Instance.General_SprintingChance = 0f;
				Instance.General_CrouchingChance = 100f;
				Instance.General_MinSecondsRisk = 60f;
				Instance.General_WristMovementChance = 0f;
				Instance.General_SprintUIOn = 59f;
				Instance.General_SprintUIOff = 0f;
				Instance.AnkleSettings_Duration_Minimum = 0f;
				Instance.AnkleSettings_Duration_Maximum = 48f;
				Instance.AnkleSettings_Chances_Fall = 15f;
				Instance.AnkleSettings_RestHours = 1f;
				Instance.WristSettings_Duration_Minimum = 0f;
				Instance.WristSettings_Duration_Maximum = 24f;
				Instance.WristSettings_Chances_Fall = 10f;
				Instance.WristSettings_RestHours = 1f;
				break;
			}
		}

		internal void OnLoad()
		{
			Instance.AddToModSettings("扭伤功能优化v1.0");
			PresetBuilder();
			Instance.RefreshGUI();
		}

		static Settings()
		{
			Instance = new Settings();
		}
	}
	public class Main : MelonMod
	{
		public override void OnInitializeMelon()
		{
			Settings.Instance.OnLoad();
		}
	}
}
namespace Sprainkle.Utilities
{
	public class Logger
	{
		public static void Log(string message, params object[] parameters)
		{
			Melon<Main>.Logger.Msg(message ?? "", parameters);
		}

		public static void LogWarning(string message, params object[] parameters)
		{
			Melon<Main>.Logger.Warning(message ?? "", parameters);
		}

		public static void LogError(string message, params object[] parameters)
		{
			Melon<Main>.Logger.Error(message ?? "", parameters);
		}

		public static void LogSeperator(params object[] parameters)
		{
			Melon<Main>.Logger.Msg("==============================================================================", parameters);
		}

		public static void LogStarter()
		{
			Melon<Main>.Logger.Msg("Mod loaded with v1.0.0");
		}
	}
	public class SceneUtilities
	{
		public static bool IsScenePlayable(string? sceneName = null)
		{
			string empty = string.Empty;
			empty = ((sceneName != null) ? sceneName : GameManager.m_ActiveScene);
			if (!(empty == "Empty") && !(empty == "Boot") && !empty.StartsWith("MainMenu", StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}
			return false;
		}

		public static bool IsSceneBase(string? sceneName = null)
		{
			string text = string.Empty;
			if (sceneName == null)
			{
				text = GameManager.m_ActiveScene;
			}
			if (sceneName != null)
			{
				text = sceneName;
			}
			return text.Contains("Region", StringComparison.InvariantCultureIgnoreCase) || text.Contains("Zone", StringComparison.InvariantCultureIgnoreCase);
		}

		public static bool IsSceneAdditive(string? sceneName = null)
		{
			string text = string.Empty;
			if (sceneName == null)
			{
				text = GameManager.m_ActiveScene;
			}
			if (sceneName != null)
			{
				text = sceneName;
			}
			return text.Contains("SANDBOX", StringComparison.InvariantCultureIgnoreCase) || text.EndsWith("DARKWALKER", StringComparison.InvariantCultureIgnoreCase) || text.EndsWith("DLC01", StringComparison.InvariantCultureIgnoreCase);
		}
	}
	internal class CommonUtilities
	{
		private static string pattern = "(?:\\(\\d{1,}\\))";

		[return: NotNullIfNotNull("name")]
		public static GearItem GetGearItemPrefab(string name)
		{
			return GearItem.LoadGearItemPrefab(name);
		}

		[return: NotNullIfNotNull("name")]
		public static ToolsItem GetToolItemPrefab(string name)
		{
			return ((Component)GearItem.LoadGearItemPrefab(name)).GetComponent<ToolsItem>();
		}

		[return: NotNullIfNotNull("name")]
		public static ClothingItem GetClothingItemPrefab(string name)
		{
			return ((Component)GearItem.LoadGearItemPrefab(name)).GetComponent<ClothingItem>();
		}

		[return: NotNullIfNotNull("name")]
		public static string? NormalizeName(string name)
		{
			name.Replace("(Clone)", "", StringComparison.InvariantCultureIgnoreCase).Trim();
			name = Regex.Replace(name, pattern, "");
			return name;
		}
	}
}
namespace Sprainkle.Utilities.Exceptions
{
	[Serializable]
	public class BadMemeException : Exception
	{
		public BadMemeException(string message)
			: base(message)
		{
		}
	}
}
namespace Sprainkle.Patches
{
	[HarmonyPatch(typeof(SprainedAnkle), "SprainedAnkleStart", new Type[]
	{
		typeof(string),
		typeof(AfflictionOptions)
	})]
	internal class SprainedAnkle_SprainedAnkleStart
	{
		public static void Postfix(SprainedAnkle __instance)
		{
			if (!GameManager.GetPlayerManagerComponent().PlayerIsDead() && !InterfaceManager.IsPanelEnabled<Panel_ChallengeComplete>() && (!GameManager.InCustomMode() || GameManager.GetCustomMode().m_EnableSprains) && Settings.Instance.AnkleSettings_Enabled)
			{
				__instance.m_DurationHoursMin = Settings.Instance.AnkleSettings_Duration_Minimum;
				__instance.m_DurationHoursMax = Settings.Instance.AnkleSettings_Duration_Maximum;
				__instance.m_ChanceSprainAfterFall = Settings.Instance.AnkleSettings_Chances_Fall;
				__instance.m_NumHoursRestForCure = Settings.Instance.AnkleSettings_RestHours;
			}
		}
	}
	[HarmonyPatch(typeof(SprainedWrist), "SprainedWristStart", new Type[]
	{
		typeof(string),
		typeof(AfflictionOptions)
	})]
	internal class SprainedWrist_SprainedWristStart
	{
		public static void Postfix(SprainedWrist __instance)
		{
			if (!GameManager.GetPlayerManagerComponent().PlayerIsDead() && !InterfaceManager.IsPanelEnabled<Panel_ChallengeComplete>() && (!GameManager.InCustomMode() || GameManager.GetCustomMode().m_EnableSprains) && !__instance.m_IsNoSprainWristForced && Settings.Instance.WristSettings_Enabled)
			{
				__instance.m_DurationHoursMin = Settings.Instance.WristSettings_Duration_Minimum;
				__instance.m_DurationHoursMax = Settings.Instance.WristSettings_Duration_Maximum;
				__instance.m_ChanceSprainAfterFall = Settings.Instance.WristSettings_Chances_Fall;
				__instance.m_NumHoursRestForCure = Settings.Instance.WristSettings_RestHours;
			}
		}
	}
	[HarmonyPatch(typeof(Sprains), "Update")]
	internal class Sprains_Update
	{
		public static void Postfix(Sprains __instance)
		{
			if (!GameManager.m_IsPaused && !GameManager.s_IsGameplaySuspended && !GameManager.GetPlayerManagerComponent().m_God && (!GameManager.InCustomMode() || GameManager.GetCustomMode().m_EnableSprains) && GameManager.IsFrameValidToUpdate((GameplayComponent)0))
			{
				__instance.m_MinSecondsForSlopeRisk = Settings.Instance.General_MinSecondsRisk;
				__instance.m_MinSlopeDegreesForSprain = Settings.Instance.General_MinimumSlope;
				__instance.m_BaseChanceWhenMovingOnSlope = Settings.Instance.General_BaseChanceMoving;
				__instance.m_ChanceIncreaseEncumbered = Settings.Instance.General_EncumberanceChance;
				__instance.m_ChanceIncreaseExhausted = Settings.Instance.General_ExhaustionChance;
				__instance.m_ChanceIncreaseSprinting = Settings.Instance.General_SprintingChance;
				__instance.m_ChanceReduceWhenCrouchedPercent = Settings.Instance.General_CrouchingChance;
				__instance.m_MinSecondsBeforeHidingWarning = Settings.Instance.General_SprintUIOff;
				__instance.m_MinSecondsToShowWarning = Settings.Instance.General_SprintUIOn;
			}
		}
	}
}
