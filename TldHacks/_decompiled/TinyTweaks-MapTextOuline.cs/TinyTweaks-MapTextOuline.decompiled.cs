using System;
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
using ModSettings;
using TinyTweaks;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints | DebuggableAttribute.DebuggingModes.EnableEditAndContinue)]
[assembly: AssemblyTitle("TinyTweaks-MapTextOuline")]
[assembly: AssemblyCopyright("Created by Waltz")]
[assembly: AssemblyFileVersion("1.3.0")]
[assembly: MelonInfo(typeof(MapTextOutline), "TinyTweaks-MapTextOuline", "1.3.0", "Waltz", null)]
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
			options.AddToModSettings("[TinyTweaks]-地图高亮文本v1.3");
		}
	}
	internal class TTSettings : JsonModSettings
	{
		[Section("地图文本描边")]
		[Name("文本模版")]
		[Description("默认：黑色文本/白色轮廓")]
		[Choice(new string[] { "黑色文本/褐色轮廓", "黑色文本/白色轮廓", "明亮文本/黑色轮廓", "白色文本/黑色轮廓" })]
		public int outlineColor = 1;

		protected override void OnConfirm()
		{
			InterfaceManager.GetPanel<Panel_Map>().CreateObjectPools();
			base.OnConfirm();
		}
	}
	internal class MapTextOutline : MelonMod
	{
		[HarmonyPatch(typeof(Panel_Map), "CreateObjectPools")]
		public class EditTextVisual
		{
			public static void Postfix(Panel_Map __instance)
			{
				//IL_0060: Unknown result type (might be due to invalid IL or missing references)
				//IL_0084: Unknown result type (might be due to invalid IL or missing references)
				//IL_009a: Unknown result type (might be due to invalid IL or missing references)
				//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
				//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
				//IL_011e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0142: Unknown result type (might be due to invalid IL or missing references)
				//IL_0158: Unknown result type (might be due to invalid IL or missing references)
				//IL_017a: Unknown result type (might be due to invalid IL or missing references)
				//IL_019e: Unknown result type (might be due to invalid IL or missing references)
				//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
				for (int i = 0; i < __instance.m_TextPoolParent.childCount; i++)
				{
					UILabel component = ((Component)__instance.m_TextPoolParent.GetChild(i)).GetComponent<UILabel>();
					float num = 0.7f;
					switch (Settings.options.outlineColor)
					{
					case 0:
						((UIWidget)component).color = new Color(0.125f, 0.094f, 0.094f, 1f);
						component.effectStyle = (Effect)2;
						component.effectColor = new Color(0.5808823f, 0.5514917f, 0.4954584f, num);
						component.effectDistance = new Vector2(1.2f, 0.5f);
						break;
					case 1:
						((UIWidget)component).color = new Color(0.125f, 0.094f, 0.094f, 1f);
						component.effectStyle = (Effect)2;
						component.effectColor = new Color(1f, 1f, 1f, num);
						component.effectDistance = new Vector2(1.2f, 0.5f);
						break;
					case 2:
						((UIWidget)component).color = new Color(0.88f, 0.83f, 0.735f, 1f);
						component.effectStyle = (Effect)2;
						component.effectColor = new Color(0.125f, 0.094f, 0.094f, num);
						component.effectDistance = new Vector2(1.2f, 0.5f);
						break;
					case 3:
						((UIWidget)component).color = new Color(1f, 1f, 1f, 1f);
						component.effectStyle = (Effect)2;
						component.effectColor = new Color(0.125f, 0.094f, 0.094f, num);
						component.effectDistance = new Vector2(1.2f, 0.5f);
						break;
					}
				}
			}
		}

		public override void OnInitializeMelon()
		{
			Settings.OnLoad();
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
