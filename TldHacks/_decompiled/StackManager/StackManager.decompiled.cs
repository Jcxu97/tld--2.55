using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ComplexLogger;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Text.RegularExpressions;
using Il2CppTLD.PDID;
using MelonLoader;
using MelonLoader.Utils;
using Microsoft.CodeAnalysis;
using ModSettings;
using StackManager;
using StackManager.Utilities;
using StackManager.Utilities.Exceptions;
using StackManager.Utilities.JSON;
using StackManager.config;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: AssemblyTitle("StackManager")]
[assembly: AssemblyDescription(null)]
[assembly: AssemblyCompany(null)]
[assembly: AssemblyProduct("StackManager")]
[assembly: AssemblyCopyright("Copyright © 2023-2024")]
[assembly: AssemblyTrademark(null)]
[assembly: AssemblyFileVersion("1.0.6")]
[assembly: MelonInfo(typeof(StackManager.Main), "StackManager", "1.0.6", "The Illusion", null)]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: VerifyLoaderVersion("0.6.1", true)]
[assembly: MelonPriority(0)]
[assembly: MelonIncompatibleAssemblies(null)]
[assembly: TargetFramework(".NETCoreApp,Version=v6.0", FrameworkDisplayName = ".NET 6.0")]
[assembly: AssemblyVersion("1.0.6.0")]
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
namespace StackManager
{
	public static class BuildInfo
	{
		public const string Name = "StackManager";

		public const string Author = "The Illusion";

		public const string Version = "1.0.6";

		public const string GUIName = "Stack Manager";

		public const string MelonLoaderVersion = "0.6.1";

		public const string Description = null;

		public const string Company = null;

		public const string DownloadLink = null;

		public const string Copyright = "Copyright © 2023-2024";

		public const string Trademark = null;

		public const string Product = "StackManager";

		public const string Culture = null;

		public const int Priority = 0;
	}
	public class Settings : JsonModSettings
	{
		[Name("添加可堆叠行为")]
		[Description("启用的话，模组会为配置文件中的定义物品添加可堆叠行为(除了游戏硬机制物品不可堆叠外，大部分能堆叠的物品现在都可以无限堆叠了)")]
		public bool AddStack = true;

		[Name("设置堆叠物品的最大状态(即耐久)")]
		[Description("启用的话，每件物品都将以最佳状态(即最大耐久值)堆叠到一起，比如8张鹿皮堆叠到一起了，你启用的话只影响鹿皮，而你背包里的一张没被堆叠的熊皮还会保持原始耐久")]
		public bool UseMaxHP;

		internal static Settings Instance { get; }

		internal static void OnLoad()
		{
			Instance.AddToModSettings("堆叠管理器v1.0.6");
			Instance.RefreshGUI();
		}

		static Settings()
		{
			Instance = new Settings();
		}
	}
	public class Main : MelonMod
	{
		public class PostfixTrack
		{
			public static bool PostFixTrack { get; set; }

			public static float PostfixCondition { get; set; }

			public static GearItem? PostfixStack { get; set; }

			public static GearItem? PostfixGearToAdd { get; set; }

			public static float PostfixConstraint { get; set; }
		}

		public static ComplexLogger<Main> Logger { get; } = new ComplexLogger<Main>();


		public static Config Config { get; set; } = new Config();


		public static string ConfigFile { get; } = Path.Combine(MelonEnvironment.ModsDirectory, "StackManager", "config.json");


		public static Version CurrentVersion { get; } = new Version(1, 0, 1);


		public override void OnInitializeMelon()
		{
			Settings.OnLoad();
			if (!Directory.Exists(Path.Combine(MelonEnvironment.ModsDirectory, "StackManager")))
			{
				Logger.Log("Directory does not exist, making", FlaggedLoggingLevel.Debug, "OnInitializeMelon");
				Directory.CreateDirectory(Path.Combine(MelonEnvironment.ModsDirectory, "StackManager"));
				return;
			}
			if (!File.Exists(ConfigFile))
			{
				Logger.Log("file does not exist, making", FlaggedLoggingLevel.Debug, "OnInitializeMelon");
				if (SetupDefaultConfig())
				{
					JsonFile.Save(ConfigFile, Config);
				}
			}
			if (Config == null)
			{
				Config = JsonFile.Load<Config>(ConfigFile);
			}
			if (Config?.ConfigurationVersion != CurrentVersion)
			{
				UpdateConfig();
			}
		}

		public static bool UpdateConfig()
		{
			File.Delete(ConfigFile);
			Config.ConfigurationVersion = CurrentVersion;
			if (SetupDefaultConfig())
			{
				JsonFile.Save(ConfigFile, Config);
			}
			return true;
		}

		public static bool SetupDefaultConfig()
		{
			Logger.Log("Making default config", FlaggedLoggingLevel.Debug, "SetupDefaultConfig");
			Config = new Config();
			Config.ConfigurationVersion = CurrentVersion;
			Config.STACK_MERGE = new List<string>
			{
				"GEAR_BirchSaplingDried", "GEAR_BearHideDried", "GEAR_BottleAntibiotics", "GEAR_BottlePainKillers", "GEAR_Carrot", "GEAR_CoffeeTin", "GEAR_GreenTeaPackage", "GEAR_GutDried", "GEAR_LeatherDried", "GEAR_LeatherHideDried",
				"GEAR_MapleSaplingDried", "GEAR_MooseHideDried", "GEAR_PackMatches", "GEAR_Potato", "GEAR_RabbitPeltDried", "GEAR_StumpRemover", "GEAR_WolfPeltDried", "GEAR_WoodMatches"
			};
			Config.Advanced = new List<string> { "GEAR_CoffeeTin", "GEAR_GreenTeaPackage", "GEAR_Potato" };
			Config.AddStackableComponent = new List<string> { "GEAR_Potato", "GEAR_StumpRemover" };
			return true;
		}
	}
	public class StackingUtilities
	{
		[Obsolete("Use StackingUtilities.SetPostFix(GearItem, GearItem, float, float, bool) instead")]
		internal static void ResetPostfixParams()
		{
		}

		[Obsolete]
		public static bool SetPostFix(GearItem? PostfixStack, GearItem? PostfixGearToAdd, float PostfixCondition, float PostfixConstraint, bool reset)
		{
			Main.PostfixTrack.PostFixTrack = !reset;
			GearItem? postfixGearToAdd = Main.PostfixTrack.PostfixGearToAdd;
			if (!string.IsNullOrWhiteSpace((postfixGearToAdd != null) ? ((Object)postfixGearToAdd).name : null))
			{
				GearItem? postfixStack = Main.PostfixTrack.PostfixStack;
				if (!string.IsNullOrWhiteSpace((postfixStack != null) ? ((Object)postfixStack).name : null))
				{
					Main.Logger.Log($"SetPostFix({Main.PostfixTrack.PostFixTrack}, {((Object)Main.PostfixTrack.PostfixGearToAdd).name}, {Main.PostfixTrack.PostfixCondition}, {((Object)Main.PostfixTrack.PostfixStack).name}, {Main.PostfixTrack.PostfixConstraint})", FlaggedLoggingLevel.Debug, "SetPostFix");
					goto IL_0253;
				}
			}
			GearItem? postfixGearToAdd2 = Main.PostfixTrack.PostfixGearToAdd;
			if (!string.IsNullOrWhiteSpace((postfixGearToAdd2 != null) ? ((Object)postfixGearToAdd2).name : null))
			{
				Main.Logger.Log($"SetPostFix({Main.PostfixTrack.PostFixTrack}, {((Object)Main.PostfixTrack.PostfixGearToAdd).name}, {Main.PostfixTrack.PostfixCondition}, null, {Main.PostfixTrack.PostfixConstraint})", FlaggedLoggingLevel.Debug, "SetPostFix");
			}
			else
			{
				GearItem? postfixStack2 = Main.PostfixTrack.PostfixStack;
				if (!string.IsNullOrWhiteSpace((postfixStack2 != null) ? ((Object)postfixStack2).name : null))
				{
					Main.Logger.Log($"SetPostFix({Main.PostfixTrack.PostFixTrack}, null, {Main.PostfixTrack.PostfixCondition}, {((Object)Main.PostfixTrack.PostfixStack).name}, {Main.PostfixTrack.PostfixConstraint})", FlaggedLoggingLevel.Debug, "SetPostFix");
				}
			}
			goto IL_0253;
			IL_0253:
			if (reset)
			{
				Main.PostfixTrack.PostfixStack = null;
				Main.PostfixTrack.PostfixGearToAdd = null;
				Main.PostfixTrack.PostfixCondition = 0f;
				Main.PostfixTrack.PostfixConstraint = 0f;
			}
			else
			{
				Main.PostfixTrack.PostfixStack = PostfixStack;
				Main.PostfixTrack.PostfixGearToAdd = PostfixGearToAdd;
				Main.PostfixTrack.PostfixCondition = PostfixCondition;
				Main.PostfixTrack.PostfixConstraint = PostfixConstraint;
			}
			return true;
		}

		[Obsolete("Use StackingUtilities.SetNormalizedCondition instead")]
		public static float MaybeSetCondition(GearItem gi)
		{
			return gi.m_GearItemData.MaxHP * (gi.m_StackableItem.IsAStackOfItems ? gi.m_StackableItem.StackMultiplier : 1f);
		}

		public static float SetNormalizedCondition(GearItem gi, GearItem target)
		{
			return Math.Max(gi.GetNormalizedCondition(), target.GetNormalizedCondition());
		}

		public static void MergeIntoStack(GearItem targetStack, GearItem gearToAdd, int units)
		{
			if (!((Object)(object)gearToAdd == (Object)null) && !((Object)(object)targetStack == (Object)null))
			{
				float num = SetNormalizedCondition(gearToAdd, targetStack);
				Main.Logger.Log($"TargetCondition:{num}", FlaggedLoggingLevel.Debug, "MergeIntoStack");
				if (Main.Config.Advanced.Contains(((Object)gearToAdd).name))
				{
					targetStack.CurrentHP = targetStack.m_GearItemData.MaxHP;
				}
				targetStack.CurrentHP = Mathf.Clamp(num * targetStack.m_GearItemData.MaxHP, 0f, targetStack.m_GearItemData.MaxHP);
				StackableItem stackableItem = targetStack.m_StackableItem;
				stackableItem.m_Units += units;
				if (targetStack.m_StackableItem.m_ShareStackWithGear == null)
				{
					targetStack.m_StackableItem.m_ShareStackWithGear = new Il2CppReferenceArray<StackableItem>(0L);
				}
			}
		}

		public static bool CanBeMerged(GearItem? target, GearItem? item)
		{
			if ((Object)(object)target != (Object)null)
			{
				return (Object)(object)item != (Object)null;
			}
			return false;
		}

		public static bool UseDefaultStacking(GearItem gearItem)
		{
			if (!((Object)(object)gearItem == (Object)null))
			{
				return !Main.Config.STACK_MERGE.Contains(((Object)gearItem).name);
			}
			return true;
		}

		public static bool CanOperate(GearItem? gi, float condition)
		{
			if (!((Object)(object)gi == (Object)null) && condition > 0f)
			{
				return !UseDefaultStacking(gi);
			}
			return false;
		}

		public static bool Do(GearItem gearToAdd, float normalizedCondition, int numUnits)
		{
			GearItem closestMatchStackable = StackableItem.GetClosestMatchStackable(GameManager.GetInventoryComponent().m_Items, gearToAdd, normalizedCondition);
			if (!CanOperate(gearToAdd, normalizedCondition))
			{
				return true;
			}
			if ((Object)(object)closestMatchStackable == (Object)null)
			{
				Main.Logger.Log("targetStack:null", FlaggedLoggingLevel.Debug, "Do");
				return true;
			}
			if (!CanBeMerged(closestMatchStackable, gearToAdd))
			{
				Main.Logger.Log("GearItem cant be stacked", FlaggedLoggingLevel.Debug, "Do");
				if (!string.IsNullOrWhiteSpace(((Object)gearToAdd).name))
				{
					if (!string.IsNullOrWhiteSpace(((Object)closestMatchStackable).name))
					{
						Main.Logger.Log($"CanBeMerged({((Object)closestMatchStackable).name}, {((Object)gearToAdd).name})", FlaggedLoggingLevel.Debug, "Do");
					}
					else
					{
						Main.Logger.Log("CanBeMerged(null, " + ((Object)gearToAdd).name + ")", FlaggedLoggingLevel.Debug, "Do");
					}
				}
				else
				{
					Main.Logger.Log("gearToAdd.name:null", FlaggedLoggingLevel.Debug, "Do");
				}
				return true;
			}
			if ((Object)(object)closestMatchStackable.m_StackableItem != (Object)null)
			{
				if (Settings.Instance.UseMaxHP)
				{
					normalizedCondition = closestMatchStackable.m_GearItemData.MaxHP;
				}
				MergeIntoStack(closestMatchStackable, gearToAdd, numUnits);
				return true;
			}
			return false;
		}

		public static bool Do(GearItem gearToAdd, float normalizedCondition, int numUnits, ref GearItem? existingGearItem)
		{
			if ((Object)(object)gearToAdd == (Object)null)
			{
				return true;
			}
			if (!CanOperate(gearToAdd, normalizedCondition))
			{
				return true;
			}
			if (GameManager.GetInventoryComponent().TryStackingItem(gearToAdd))
			{
				Main.Logger.Log("Was able to stack item " + gearToAdd.DisplayName, FlaggedLoggingLevel.Debug, "Do");
				return false;
			}
			Main.Logger.Log("Was not able to stack item " + gearToAdd.DisplayName, FlaggedLoggingLevel.Debug, "Do");
			return true;
		}

		public static void LogEverthing(GearItem? gearToAdd, GearItem? targetStack, float normalizedCondition, int units)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if ((Object)(object)gearToAdd == (Object)null)
			{
				stringBuilder.AppendLine("gearToAdd:null");
			}
			if ((Object)(object)targetStack == (Object)null)
			{
				stringBuilder.AppendLine("targetStack:null");
			}
			float value = 0f;
			object value2 = null;
			object value3 = null;
			string value4 = string.Empty;
			string value5 = string.Empty;
			if (!((Object)(object)gearToAdd == (Object)null) && !((Object)(object)targetStack == (Object)null))
			{
				value = Math.Abs(gearToAdd.CurrentHP - targetStack.CurrentHP);
				value2 = CommonUtilities.GetGearItemPDID(gearToAdd);
				value3 = CommonUtilities.GetGearItemPDID(targetStack);
				value4 = (string.IsNullOrWhiteSpace(((Object)gearToAdd).name) ? ((Object)gearToAdd).name : "NULL");
				value5 = (string.IsNullOrWhiteSpace(((Object)targetStack).name) ? ((Object)targetStack).name : "NULL");
			}
			stringBuilder.AppendLine("Gear to add: ");
			stringBuilder.Append('x');
			stringBuilder.Append(units);
			stringBuilder.Append(' ');
			stringBuilder.Append(value4);
			stringBuilder.Append(' ');
			stringBuilder.Append(value2);
			stringBuilder.AppendLine("Target stack: ");
			stringBuilder.Append(value5);
			stringBuilder.Append(' ');
			stringBuilder.Append(value3);
			stringBuilder.AppendLine("Health Difference: ");
			stringBuilder.Append(value);
			Main.Logger.Log("Begin Detailed Log", FlaggedLoggingLevel.Always, LoggingSubType.IntraSeparator, "LogEverthing");
			Main.Logger.Log(stringBuilder.ToString(), FlaggedLoggingLevel.Always, "LogEverthing");
			Main.Logger.Log("End Detailed Log", FlaggedLoggingLevel.Always, LoggingSubType.IntraSeparator, "LogEverthing");
		}
	}
}
namespace StackManager.Utilities
{
	internal class CommonUtilities
	{
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
			return Regex.Replace(Regex.Replace(name, "(?:\\(\\d{0,}\\))", string.Empty), "(?:\\s\\d{0,})", string.Empty).Replace("(Clone)", string.Empty, StringComparison.InvariantCultureIgnoreCase).Replace("\0", string.Empty)
				.Trim();
		}

		public static bool IsPlayerAvailable(PlayerManager PlayerManagerComponent)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Invalid comparison between Unknown and I4
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Invalid comparison between Unknown and I4
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Invalid comparison between Unknown and I4
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Invalid comparison between Unknown and I4
			if ((Object)(object)PlayerManagerComponent == (Object)null)
			{
				return false;
			}
			bool num = (int)PlayerManagerComponent.m_ControlMode == 1;
			bool flag = (int)PlayerManagerComponent.m_ControlMode == 12;
			bool flag2 = (int)PlayerManagerComponent.m_ControlMode == 13;
			bool flag3 = (int)PlayerManagerComponent.m_ControlMode == 16;
			return num && flag && flag2 && flag3;
		}

		public static string? GetGearItemPDID(GearItem gi)
		{
			return ((PdidObjectBase)((Component)gi).GetComponent<ObjectGuid>()).PDID;
		}
	}
}
namespace StackManager.Utilities.JSON
{
	public class JsonFile
	{
		public static JsonSerializerOptions GetDefaultOptions()
		{
			return new JsonSerializerOptions
			{
				WriteIndented = true,
				IncludeFields = true
			};
		}

		public static void Save<T>(string configFileName, T Tinput, JsonSerializerOptions? options = null)
		{
			try
			{
				if (options == null)
				{
					options = GetDefaultOptions();
				}
				using FileStream fileStream = File.Open(configFileName, FileMode.Create, FileAccess.Write, FileShare.None);
				JsonSerializer.Serialize(fileStream, Tinput, options);
				fileStream.Dispose();
			}
			catch (Exception inner)
			{
				throw new BadMemeException("Attempting to save " + configFileName + " failed", inner);
			}
		}

		public static T? Load<T>(string configFileName, bool createFile = false, JsonSerializerOptions? options = null)
		{
			if (!File.Exists(configFileName))
			{
				if (!createFile)
				{
					throw new BadMemeException("Requested JSON file does not exist, " + configFileName);
				}
				Save(configFileName, default(T), options);
			}
			try
			{
				if (options == null)
				{
					options = GetDefaultOptions();
				}
				using FileStream fileStream = File.Open(configFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
				T? result = JsonSerializer.Deserialize<T>(fileStream, options);
				fileStream.Dispose();
				return result;
			}
			catch (Exception inner)
			{
				throw new BadMemeException("Attempting to load the config file failed, file: " + configFileName, inner);
			}
		}

		public static async Task<T?> LoadAsync<T>(string configFileName, bool createFile = false, JsonSerializerOptions? options = null)
		{
			if (!File.Exists(configFileName))
			{
				if (!createFile)
				{
					throw new BadMemeException("Requested JSON file does not exist, " + configFileName);
				}
				await SaveAsync(configFileName, default(T), options);
			}
			try
			{
				if (options == null)
				{
					options = GetDefaultOptions();
				}
				FileStream file = File.Open(configFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
				T result;
				try
				{
					T output = await JsonSerializer.DeserializeAsync<T>(file, options);
					await file.DisposeAsync();
					result = output;
				}
				finally
				{
					if (file != null)
					{
						await file.DisposeAsync();
					}
				}
				return result;
			}
			catch (Exception inner)
			{
				throw new BadMemeException("Attempting to load the config file failed, file: " + configFileName, inner);
			}
		}

		public static async Task SaveAsync<T>(string configFileName, T Tinput, JsonSerializerOptions? options = null)
		{
			_ = 2;
			try
			{
				if (options == null)
				{
					options = GetDefaultOptions();
				}
				FileStream file = File.Open(configFileName, FileMode.Create, FileAccess.Write, FileShare.None);
				try
				{
					await JsonSerializer.SerializeAsync(file, Tinput, options);
					await file.DisposeAsync();
				}
				finally
				{
					if (file != null)
					{
						await file.DisposeAsync();
					}
				}
			}
			catch (Exception inner)
			{
				throw new BadMemeException("Attempting to save " + configFileName + " failed", inner);
			}
		}
	}
}
namespace StackManager.Utilities.Exceptions
{
	[Serializable]
	public class BadMemeException : Exception
	{
		public BadMemeException()
		{
		}

		public BadMemeException(string message)
			: base(message)
		{
		}

		public BadMemeException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
namespace StackManager.Patches
{
	[HarmonyPatch(typeof(Container))]
	[HarmonyPatch("AddToExistingStackable")]
	[HarmonyPatch(new Type[]
	{
		typeof(GearItem),
		typeof(float),
		typeof(int)
	})]
	public class Container_AddToExistingStackable
	{
		public static bool Prefix(ref GearItem gearToAdd, float normalizedCondition, int numUnits)
		{
			if ((Object)(object)((Component)gearToAdd).gameObject.GetComponent<StackableItem>() != (Object)null)
			{
				return true;
			}
			return StackingUtilities.Do(gearToAdd, normalizedCondition, numUnits);
		}
	}
	[HarmonyPatch(typeof(GearItem), "Awake")]
	internal class DisableDecay
	{
		public static void Prefix(GearItem __instance)
		{
			if ((Object)(object)__instance == (Object)null || GameManager.IsMainMenuActive() || string.IsNullOrWhiteSpace(((Object)__instance).name) || (Object)(object)__instance.m_StackableItem != (Object)null || (Object)(object)((Component)__instance).gameObject.GetComponent<StackableItem>() != (Object)null || Main.Config == null)
			{
				return;
			}
			string text = CommonUtilities.NormalizeName(((Object)__instance).name);
			if (Settings.Instance.AddStack && Main.Config.AddStackableComponent.Contains(text))
			{
				Main.Logger.Log("AddStack: " + text, FlaggedLoggingLevel.Debug, "Prefix");
				StackableItem val = ((Component)__instance).gameObject.AddComponent<StackableItem>();
				val.m_DefaultUnitsInItem = 1;
				val.m_StackConditionDifferenceConstraint = 100f;
				val.m_StackSpriteName = string.Empty;
				val.m_ShareStackWithGear = Il2CppReferenceArray<StackableItem>.op_Implicit(Array.Empty<StackableItem>());
				if (val.m_Units == 0)
				{
					val.m_Units = 1;
				}
				__instance.m_StackableItem = val;
			}
		}

		public static void Postfix(GearItem __instance)
		{
			if (!((Object)(object)__instance == (Object)null) && !GameManager.IsMainMenuActive() && !string.IsNullOrWhiteSpace(((Object)__instance).name) && Main.Config != null)
			{
				string text = CommonUtilities.NormalizeName(((Object)__instance).name);
				StackableItem component = ((Component)__instance).gameObject.GetComponent<StackableItem>();
				if (Main.Config.STACK_MERGE.Contains(text) && (Object)(object)component != (Object)null)
				{
					component.m_StackConditionDifferenceConstraint = 100f;
				}
				if (text == "GEAR_CoffeeTin")
				{
					__instance.SetHaltDecay(true);
					__instance.CurrentHP = 1000f;
				}
				if (text == "GEAR_GreenTeaPackage")
				{
					__instance.SetHaltDecay(true);
					__instance.CurrentHP = 1500f;
				}
				if (text == "GEAR_Carrot")
				{
					__instance.SetHaltDecay(true);
					__instance.CurrentHP = 50f;
				}
				if (text == "GEAR_Potato")
				{
					__instance.SetHaltDecay(true);
					__instance.CurrentHP = 100f;
				}
			}
		}
	}
	public static class Extensions
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static LocalizedString CreateLocalizedString(string name)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Expected O, but got Unknown
			return new LocalizedString
			{
				m_LocalizationID = name
			};
		}
	}
	[HarmonyPatch(typeof(PlayerManager))]
	[HarmonyPatch("TryAddToExistingStackable")]
	[HarmonyPatch(/*Could not decode attribute arguments.*/)]
	public class PlayerManager_TryAddToExistingStackable
	{
		public static bool Prefix(ref GearItem gearToAdd, float normalizedCondition, int numUnits, ref GearItem existingGearItem)
		{
			if (GameManager.IsMainMenuActive())
			{
				return true;
			}
			if ((Object)(object)((Component)gearToAdd).gameObject.GetComponent<StackableItem>() != (Object)null)
			{
				return true;
			}
			return StackingUtilities.Do(gearToAdd, normalizedCondition, numUnits, ref existingGearItem);
		}
	}
}
namespace StackManager.config
{
	public class Config
	{
		[JsonInclude]
		public Version ConfigurationVersion { get; set; } = new Version();


		[JsonInclude]
		public List<string> STACK_MERGE { get; set; } = new List<string>();


		[JsonInclude]
		public List<string> Advanced { get; set; } = new List<string>();


		[JsonInclude]
		public List<string> AddStackableComponent { get; set; } = new List<string>();

	}
}
