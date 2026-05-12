using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Text.Json.Serialization;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using Microsoft.CodeAnalysis;
using ModData;
using TinyTweaks;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints | DebuggableAttribute.DebuggingModes.EnableEditAndContinue)]
[assembly: AssemblyTitle("TinyTweaks-BuryHumanCorpses")]
[assembly: AssemblyCopyright("Created by Waltz")]
[assembly: AssemblyFileVersion("1.3.0")]
[assembly: MelonInfo(typeof(BuryHumanCorpses), "TinyTweaks-BuryHumanCorpses", "1.3.0", "Waltz", null)]
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
	internal class BuryHumanCorpses : MelonMod
	{
		[HarmonyPatch(typeof(InputManager), "ExecuteAltFire")]
		public class CatchAltInteractionWithCorpse
		{
			public static void Prefix()
			{
				if (Object.op_Implicit((Object)(object)GameManager.GetPlayerManagerComponent()))
				{
					GameObject gameObjectUnderCrosshair = Utility.GetGameObjectUnderCrosshair();
					if (IsCorpse(gameObjectUnderCrosshair))
					{
						MelonCoroutines.Start(BuryCorpse(gameObjectUnderCrosshair));
					}
				}
			}
		}

		[HarmonyPatch(typeof(Panel_HUD), "SetHoverText")]
		public class ShowButtonPrompts
		{
			public static void Prefix(ref GameObject itemUnderCrosshairs)
			{
				if (IsCorpse(itemUnderCrosshairs))
				{
					((Behaviour)InterfaceManager.GetPanel<Panel_HUD>().m_EquipItemPopup).enabled = true;
					InterfaceManager.GetPanel<Panel_HUD>().m_EquipItemPopup.ShowGenericPopupWithDefaultActions("Search", "Bury");
				}
			}
		}

		[HarmonyPatch(typeof(Panel_GenericProgressBar), "ProgressBarEnded")]
		public class ProgressBarCallback
		{
			public static void Prefix(ref bool success, ref bool playerCancel)
			{
				if (inProgress)
				{
					if (!success)
					{
						interrupted = true;
					}
					inProgress = false;
				}
			}
		}

		[HarmonyPatch(typeof(SaveGameSystem), "SaveSceneData")]
		private static class SaveHarvestTimes
		{
			internal static void Prefix(ref SlotData slot)
			{
				string data = JsonSerializer.Serialize(buriedCorpses);
				dataManager.Save(data, saveDataTag);
			}
		}

		[HarmonyPatch(typeof(SaveGameSystem), "LoadSceneData")]
		private static class LoadHarvestTimes
		{
			internal static void Postfix(ref string name)
			{
				string text = dataManager.Load(saveDataTag);
				if (!string.IsNullOrEmpty(text))
				{
					buriedCorpses = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(text, Jsoning.GetDefaultOptions()) ?? new Dictionary<string, List<string>>();
				}
				if (!buriedCorpses.ContainsKey(GameManager.m_ActiveScene))
				{
					return;
				}
				foreach (string item in buriedCorpses[GameManager.m_ActiveScene])
				{
					Container obj = ContainerManager.FindContainerByGuid(item);
					if (obj != null)
					{
						((Component)obj).gameObject.SetActive(false);
					}
				}
			}
		}

		public static readonly string saveDataTag = "buryCorpses";

		public static ModDataManager dataManager = new ModDataManager("TinyTweaks");

		public static readonly int hoursToBury = 1;

		private static readonly float secondsToInteract = 3f;

		public static Dictionary<string, List<string>> buriedCorpses = new Dictionary<string, List<string>>();

		private static bool interrupted;

		public static bool inProgress;

		public static IEnumerator BuryCorpse(GameObject corpse)
		{
			GameManager.GetPlayerVoiceComponent().BlockNonCriticalVoiceForDuration(10f);
			interrupted = false;
			inProgress = true;
			InterfaceManager.GetPanel<Panel_GenericProgressBar>().Launch("Bury a friend", secondsToInteract, (float)hoursToBury * 60f, 0f, true, (OnExitDelegate)null);
			while (inProgress)
			{
				yield return (object)new WaitForEndOfFrame();
			}
			if (!interrupted)
			{
				corpse.active = false;
				Carrion crows = corpse.GetComponent<Carrion>();
				if ((Object)(object)crows != (Object)null)
				{
					crows.Destroy();
				}
				string guid = ObjectGuid.GetGuidFromGameObject(corpse.gameObject);
				if (buriedCorpses.ContainsKey(GameManager.m_ActiveScene))
				{
					buriedCorpses[GameManager.m_ActiveScene].Add(guid);
					yield break;
				}
				buriedCorpses[GameManager.m_ActiveScene] = new List<string> { guid };
			}
		}

		public static bool IsCorpse(GameObject corpse)
		{
			if ((Object)(object)corpse != (Object)null && (Object)(object)corpse.GetComponent<Container>() == (Object)null)
			{
				Transform parent = corpse.transform.GetParent();
				corpse = ((parent != null) ? ((Component)parent).gameObject : null);
			}
			if (!Object.op_Implicit((Object)(object)corpse) || (Object)(object)corpse.GetComponent<Container>() == (Object)null || !corpse.GetComponent<Container>().m_IsCorpse)
			{
				return false;
			}
			return true;
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
