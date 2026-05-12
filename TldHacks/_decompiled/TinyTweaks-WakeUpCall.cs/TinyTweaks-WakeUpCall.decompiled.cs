using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Text.Json.Serialization;
using HarmonyLib;
using Il2Cpp;
using Il2CppSystem;
using MelonLoader;
using Microsoft.CodeAnalysis;
using ModSettings;
using TinyTweaks;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints | DebuggableAttribute.DebuggingModes.EnableEditAndContinue)]
[assembly: AssemblyTitle("TinyTweaks-WakeUpCall")]
[assembly: AssemblyCopyright("Created by Waltz")]
[assembly: AssemblyFileVersion("1.3.0")]
[assembly: MelonInfo(typeof(WakeUpCall), "TinyTweaks-WakeUpCall", "1.3.0", "Waltz", null)]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: TargetFramework(".NETCoreApp,Version=v6.0", FrameworkDisplayName = ".NET 6.0")]
[assembly: AssemblyVersion("1.3.0.0")]
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
namespace TinyTweaks
{
	internal static class Settings
	{
		public static TTSettings options;

		public static void OnLoad()
		{
			options = new TTSettings();
			options.AddToModSettings("[Tiny Tweaks]多功能模块v1.3");
		}
	}
	internal class TTSettings : JsonModSettings
	{
		[Section("唤醒设置")]
		[Name("显示睡眠时间")]
		[Description("睡觉时显示当前时间\n\n 默认=关闭")]
		public bool showTime;

		[Name("极光唤醒")]
		[Description("被极光的光和声音自然唤醒\n\n 默认=开启")]
		public bool auroraSense = true;

		[Name("睡眠微光")]
		[Description("睡觉时不完全漆黑，保留微弱光线\n\n 默认=关闭")]
		public bool noPitchBlack;

		protected override void OnConfirm()
		{
			base.OnConfirm();
		}
	}
	internal class WakeUpCall : MelonMod
	{
		[HarmonyPatch(typeof(Rest), "BeginSleeping", new Type[]
		{
			typeof(Bed),
			typeof(int),
			typeof(int),
			typeof(float),
			typeof(PassTimeOptions),
			typeof(Action)
		})]
		private static class ManualWakeUp
		{
			internal static void Prefix(ref PassTimeOptions options, ref Action onSleepEnd)
			{
				wentSleepDuringAurora = GameManager.GetAuroraManager().IsFullyActive();
				options = (PassTimeOptions)4;
				Action action = delegate
				{
					WakeUp();
				};
				InterfaceManager.GetPanel<Panel_HUD>().m_AccelTimePopup.m_CancelCallback = Action.op_Implicit(action);
				Panel_HUD panel = InterfaceManager.GetPanel<Panel_HUD>();
				((Component)panel.m_Sprite_SystemFadeOverlay).transform.SetParent(panel.m_NonEssentialHud.transform);
				((UIWidget)panel.m_Sprite_SystemFadeOverlay).depth = 0;
				((UIRect)panel.m_Sprite_SystemFadeOverlay).OnEnable();
				panel.HideHudElements(true);
				Action action2 = delegate
				{
					PostWakeUp();
				};
				onSleepEnd = Action.op_Implicit(action2);
				CameraFade.Fade(0f, Settings.options.noPitchBlack ? fadeAlpha : 1f, 0.5f, 0f, (Action)null);
			}
		}

		[HarmonyPatch(typeof(AuroraManager), "UpdateAuroraValue")]
		private static class AuroraWakeUp
		{
			internal static void Postfix(ref AuroraManager __instance)
			{
				if (Settings.options.auroraSense && GameManager.GetRestComponent().IsSleeping() && __instance.IsFullyActive() && !wentSleepDuringAurora)
				{
					WakeUp();
				}
			}
		}

		[HarmonyPatch(typeof(InterfaceManager), "SetTimeWidgetActive")]
		private static class EnableTimeWidget
		{
			internal static void Prefix(ref bool active)
			{
				if (Settings.options.showTime && GameManager.GetRestComponent().IsSleeping() && !active)
				{
					active = true;
				}
			}
		}

		[HarmonyPatch(typeof(Panel_Rest), "Enable", new Type[]
		{
			typeof(bool),
			typeof(bool)
		})]
		private static class EnableDOF
		{
			internal static void Postfix(ref bool enable)
			{
				if (Settings.options.noPitchBlack && GameManager.GetRestComponent().IsSleeping() && !enable)
				{
					GameManager.GetCameraEffects().DepthOfFieldTurnOn();
				}
			}
		}

		private static float fadeAlpha;

		private static bool wentSleepDuringAurora;

		private static bool gameStarted;

		public override void OnInitializeMelon()
		{
			Settings.OnLoad();
		}

		public override void OnSceneWasInitialized(int buildIndex, string sceneName)
		{
			gameStarted = true;
		}

		public override void OnUpdate()
		{
			if (gameStarted && InputManager.GetPauseMenuTogglePressed(InputManager.m_CurrentContext) && GameManager.GetRestComponent().IsSleeping())
			{
				WakeUp();
			}
		}

		public static void WakeUp()
		{
			GameManager.GetRestComponent().m_InterruptionAfterSecondsSleeping = 1;
		}

		public static void PostWakeUp()
		{
			Panel_HUD panel = InterfaceManager.GetPanel<Panel_HUD>();
			((Component)panel.m_Sprite_SystemFadeOverlay).transform.SetParent(((Component)panel).transform);
			((UIWidget)panel.m_Sprite_SystemFadeOverlay).depth = 50;
			panel.HideHudElements(false);
			if (Settings.options.noPitchBlack)
			{
				GameManager.GetCameraEffects().DepthOfFieldTurnOff(false);
			}
			CameraFade.Fade(Settings.options.noPitchBlack ? fadeAlpha : 1f, 0f, 0.5f, 0f, (Action)null);
			if (Settings.options.showTime)
			{
				InterfaceManager.GetInstance().SetTimeWidgetActive(false);
			}
		}

		static WakeUpCall()
		{
			fadeAlpha = 0.3f;
		}
	}
	public class Jsoning
	{
		public static JsonSerializerOptions GetDefaultOptions()
		{
			JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
			{
				IncludeFields = true,
				AllowTrailingCommas = true,
				ReadCommentHandling = JsonCommentHandling.Skip,
				WriteIndented = false,
				DefaultIgnoreCondition = JsonIgnoreCondition.Never
			};
			jsonSerializerOptions.Converters.Add(new Vector3Converter());
			jsonSerializerOptions.Converters.Add(new QuaternionConverter());
			jsonSerializerOptions.Converters.Add(new ColorConverter());
			return jsonSerializerOptions;
		}
	}
	public class Vector3Converter : JsonConverter<Vector3>
	{
		public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			if (reader.TokenType == JsonTokenType.StartArray)
			{
				reader.Read();
				num = reader.GetSingle();
				reader.Read();
				num2 = reader.GetSingle();
				reader.Read();
				num3 = reader.GetSingle();
				reader.Read();
			}
			else if (reader.TokenType == JsonTokenType.StartObject)
			{
				while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
				{
					string @string = reader.GetString();
					reader.Read();
					switch (@string)
					{
					case "x":
						num = reader.GetSingle();
						break;
					case "y":
						num2 = reader.GetSingle();
						break;
					case "z":
						num3 = reader.GetSingle();
						break;
					default:
						reader.Skip();
						break;
					}
				}
			}
			return new Vector3(num, num2, num3);
		}

		public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			writer.WriteStartArray();
			writer.WriteNumberValue(value.x);
			writer.WriteNumberValue(value.y);
			writer.WriteNumberValue(value.z);
			writer.WriteEndArray();
		}
	}
	public class QuaternionConverter : JsonConverter<Quaternion>
	{
		public override Quaternion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 1f;
			if (reader.TokenType == JsonTokenType.StartArray)
			{
				reader.Read();
				num = reader.GetSingle();
				reader.Read();
				num2 = reader.GetSingle();
				reader.Read();
				num3 = reader.GetSingle();
				reader.Read();
				num4 = reader.GetSingle();
				reader.Read();
			}
			else if (reader.TokenType == JsonTokenType.StartObject)
			{
				while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
				{
					string @string = reader.GetString();
					reader.Read();
					switch (@string)
					{
					case "x":
						num = reader.GetSingle();
						break;
					case "y":
						num2 = reader.GetSingle();
						break;
					case "z":
						num3 = reader.GetSingle();
						break;
					case "w":
						num4 = reader.GetSingle();
						break;
					default:
						reader.Skip();
						break;
					}
				}
			}
			return new Quaternion(num, num2, num3, num4);
		}

		public override void Write(Utf8JsonWriter writer, Quaternion value, JsonSerializerOptions options)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			writer.WriteStartArray();
			writer.WriteNumberValue(value.x);
			writer.WriteNumberValue(value.y);
			writer.WriteNumberValue(value.z);
			writer.WriteNumberValue(value.w);
			writer.WriteEndArray();
		}
	}
	public class ColorConverter : JsonConverter<Color>
	{
		public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			//IL_0145: Unknown result type (might be due to invalid IL or missing references)
			//IL_0149: Unknown result type (might be due to invalid IL or missing references)
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 1f;
			if (reader.TokenType == JsonTokenType.StartArray)
			{
				reader.Read();
				num = reader.GetSingle();
				reader.Read();
				num2 = reader.GetSingle();
				reader.Read();
				num3 = reader.GetSingle();
				if (reader.Read() && reader.TokenType == JsonTokenType.Number)
				{
					num4 = reader.GetSingle();
					reader.Read();
				}
			}
			else if (reader.TokenType == JsonTokenType.StartObject)
			{
				while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
				{
					string @string = reader.GetString();
					reader.Read();
					switch (@string)
					{
					case "r":
						num = reader.GetSingle();
						break;
					case "g":
						num2 = reader.GetSingle();
						break;
					case "b":
						num3 = reader.GetSingle();
						break;
					case "a":
						num4 = reader.GetSingle();
						break;
					default:
						reader.Skip();
						break;
					}
				}
			}
			return new Color(num, num2, num3, num4);
		}

		public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			writer.WriteStartArray();
			writer.WriteNumberValue(value.r);
			writer.WriteNumberValue(value.g);
			writer.WriteNumberValue(value.b);
			writer.WriteNumberValue(value.a);
			writer.WriteEndArray();
		}
	}
	public class Utility
	{
		public const string globalModVersion = "1.3.0";

		public static bool IsScenePlayable()
		{
			return !string.IsNullOrEmpty(GameManager.m_ActiveScene) && !GameManager.m_ActiveScene.Contains("MainMenu") && !(GameManager.m_ActiveScene == "Boot") && !(GameManager.m_ActiveScene == "Empty");
		}

		public static bool IsScenePlayable(string scene)
		{
			return !string.IsNullOrEmpty(scene) && !scene.Contains("MainMenu") && !(scene == "Boot") && !(scene == "Empty");
		}

		public static GameObject GetGameObjectUnderCrosshair()
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Invalid comparison between Unknown and I4
			GameObject val = null;
			PlayerManager playerManagerComponent = GameManager.GetPlayerManagerComponent();
			float maxPickupRange = GameManager.GetGlobalParameters().m_MaxPickupRange;
			float num = playerManagerComponent.ComputeModifiedPickupRange(maxPickupRange);
			if ((int)playerManagerComponent.GetControlMode() == 16)
			{
				num = 50f;
			}
			return GameManager.GetPlayerManagerComponent().GetInteractiveObjectUnderCrosshairs(num);
		}
	}
}
