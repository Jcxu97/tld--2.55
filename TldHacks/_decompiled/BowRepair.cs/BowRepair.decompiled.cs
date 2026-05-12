using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using BowRepairMod;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem;
using MelonLoader;
using Microsoft.CodeAnalysis;
using ModSettings;
using UnityEngine;
using UnityEngine.AddressableAssets;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints | DebuggableAttribute.DebuggingModes.EnableEditAndContinue)]
[assembly: AssemblyTitle("BowRepair")]
[assembly: AssemblyCopyright("Marcy")]
[assembly: AssemblyFileVersion("1.2.2")]
[assembly: MelonInfo(typeof(BowRepair), "BowRepair", "1.2.2", "Marcy", null)]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: TargetFramework(".NETCoreApp,Version=v6.0", FrameworkDisplayName = ".NET 6.0")]
[assembly: AssemblyVersion("1.2.2.0")]
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
namespace BowRepairMod
{
	public class BowRepair : MelonMod
	{
		public static ToolsItem simpletools;

		public static ToolsItem qualitytools;

		public static GearItem driedgut;

		public static GearItem scrapmetal;

		public static GearItem scraplead;

		public static GearItem softwood;

		public static GearItem hardwood;

		public static GearItem cloth;

		private static bool initialized;

		public override void OnInitializeMelon()
		{
			Settings.instance.AddToModSettings("可修复的弓v1.2.2");
			Settings.OnLoad();
		}

		public override void OnSceneWasInitialized(int buildIndex, string sceneName)
		{
			if (!initialized)
			{
				LoadItems();
				initialized = true;
			}
		}

		private static void LoadItems()
		{
			simpletools = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_SimpleTools")).WaitForCompletion().GetComponent<GearItem>()
				.m_ToolsItem;
			qualitytools = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_HighQualityTools")).WaitForCompletion().GetComponent<GearItem>()
				.m_ToolsItem;
			driedgut = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_GutDried")).WaitForCompletion().GetComponent<GearItem>();
			scrapmetal = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_ScrapMetal")).WaitForCompletion().GetComponent<GearItem>();
			scraplead = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_ScrapLead")).WaitForCompletion().GetComponent<GearItem>();
			softwood = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_Softwood")).WaitForCompletion().GetComponent<GearItem>();
			hardwood = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_Hardwood")).WaitForCompletion().GetComponent<GearItem>();
			cloth = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_Cloth")).WaitForCompletion().GetComponent<GearItem>();
		}
	}
	[HarmonyPatch(typeof(GearItem), "Awake")]
	internal static class BowItemPatch
	{
		public static void Postfix(ref GearItem __instance)
		{
			int[] array = Array.Empty<int>();
			if (((Object)__instance).name.Contains("GEAR_Bow") && !((Object)__instance).name.ToLowerInvariant().Contains("gear_bow_manufactured") && !((Object)__instance).name.ToLowerInvariant().Contains("gear_bow_woodwrights") && !((Object)__instance).name.ToLowerInvariant().Contains("gear_bow_bushcraft"))
			{
				switch (Settings.instance.BowRepairMode)
				{
				case 0:
				{
					switch (Settings.instance.BowMaterialNeed)
					{
					case 0:
						array = new int[2] { 1, 1 };
						break;
					case 1:
						array = new int[2] { 2, 1 };
						break;
					case 2:
						array = new int[2] { 3, 3 };
						break;
					}
					Repairable val4 = ((Component)__instance).gameObject.AddComponent<Repairable>();
					__instance.m_Repairable = val4;
					val4.m_RepairToolChoices = Utilities.Tools();
					val4.m_DurationMinutes = 65;
					val4.m_ConditionIncrease = 35f;
					val4.m_RequiredGear = Utilities.MakeNeededItemList(BowRepair.softwood, BowRepair.driedgut);
					val4.m_RequiredGearUnits = Il2CppStructArray<int>.op_Implicit(array);
					val4.m_RequiresToolToRepair = true;
					val4.m_RepairAudio = "Play_CraftingGeneric";
					break;
				}
				case 1:
				{
					switch (Settings.instance.BowMaterialNeed)
					{
					case 0:
						array = new int[2] { 1, 1 };
						break;
					case 1:
						array = new int[2] { 2, 1 };
						break;
					case 2:
						array = new int[2] { 3, 3 };
						break;
					}
					Millable val3 = ((Component)__instance).gameObject.AddComponent<Millable>();
					__instance.m_Millable = val3;
					val3.m_CanRestoreFromWornOut = true;
					val3.m_RecoveryDurationMinutes = 145;
					val3.m_RepairDurationMinutes = 50;
					val3.m_RepairRequiredGear = Utilities.MakeNeededItemList(BowRepair.softwood, BowRepair.scrapmetal);
					val3.m_RepairRequiredGearUnits = Il2CppStructArray<int>.op_Implicit(array);
					switch (Settings.instance.BowMaterialNeed)
					{
					case 0:
						array = new int[2] { 1, 1 };
						break;
					case 1:
						array = new int[2] { 2, 2 };
						break;
					case 2:
						array = new int[2] { 3, 4 };
						break;
					}
					val3.m_RestoreRequiredGear = Utilities.MakeNeededItemList(BowRepair.softwood, BowRepair.scrapmetal);
					val3.m_RestoreRequiredGearUnits = Il2CppStructArray<int>.op_Implicit(array);
					val3.m_Skill = (SkillType)5;
					break;
				}
				case 2:
				{
					switch (Settings.instance.BowMaterialNeed)
					{
					case 0:
						array = new int[2] { 1, 1 };
						break;
					case 1:
						array = new int[2] { 2, 1 };
						break;
					case 2:
						array = new int[2] { 3, 3 };
						break;
					}
					Repairable val = ((Component)__instance).gameObject.AddComponent<Repairable>();
					__instance.m_Repairable = val;
					val.m_RepairToolChoices = Utilities.Tools();
					val.m_DurationMinutes = 65;
					val.m_ConditionIncrease = 35f;
					val.m_RequiredGear = Utilities.MakeNeededItemList(BowRepair.softwood, BowRepair.driedgut);
					val.m_RequiredGearUnits = Il2CppStructArray<int>.op_Implicit(array);
					val.m_RequiresToolToRepair = true;
					val.m_RepairAudio = "Play_CraftingGeneric";
					Millable val2 = ((Component)__instance).gameObject.AddComponent<Millable>();
					__instance.m_Millable = val2;
					val2.m_CanRestoreFromWornOut = true;
					val2.m_RecoveryDurationMinutes = 145;
					val2.m_RepairDurationMinutes = 50;
					val2.m_RepairRequiredGear = Utilities.MakeNeededItemList(BowRepair.softwood, BowRepair.scrapmetal);
					val2.m_RepairRequiredGearUnits = Il2CppStructArray<int>.op_Implicit(array);
					switch (Settings.instance.BowMaterialNeed)
					{
					case 0:
						array = new int[2] { 1, 1 };
						break;
					case 1:
						array = new int[2] { 2, 2 };
						break;
					case 2:
						array = new int[2] { 3, 4 };
						break;
					}
					val2.m_RestoreRequiredGear = Utilities.MakeNeededItemList(BowRepair.softwood, BowRepair.scrapmetal);
					val2.m_RestoreRequiredGearUnits = Il2CppStructArray<int>.op_Implicit(array);
					val2.m_Skill = (SkillType)5;
					break;
				}
				}
			}
			if (!Settings.instance.DLCEnable)
			{
				return;
			}
			if (((Object)__instance).name.ToLowerInvariant().Contains("gear_bow_manufactured"))
			{
				switch (Settings.instance.SportBowRepairMode)
				{
				case 0:
				{
					switch (Settings.instance.SportBowMaterialNeed)
					{
					case 0:
						array = new int[2] { 1, 1 };
						break;
					case 1:
						array = new int[2] { 2, 1 };
						break;
					case 2:
						array = new int[2] { 3, 3 };
						break;
					}
					Repairable val8 = ((Component)__instance).gameObject.AddComponent<Repairable>();
					__instance.m_Repairable = val8;
					val8.m_RepairToolChoices = Utilities.Tools();
					val8.m_DurationMinutes = 65;
					val8.m_ConditionIncrease = 35f;
					val8.m_RequiredGear = Utilities.MakeNeededItemList(BowRepair.scraplead, BowRepair.scrapmetal);
					val8.m_RequiredGearUnits = Il2CppStructArray<int>.op_Implicit(array);
					val8.m_RequiresToolToRepair = true;
					val8.m_RepairAudio = "Play_CraftingGeneric";
					break;
				}
				case 1:
				{
					switch (Settings.instance.SportBowMaterialNeed)
					{
					case 0:
						array = new int[2] { 1, 1 };
						break;
					case 1:
						array = new int[2] { 2, 1 };
						break;
					case 2:
						array = new int[2] { 3, 3 };
						break;
					}
					Millable val7 = ((Component)__instance).gameObject.AddComponent<Millable>();
					__instance.m_Millable = val7;
					val7.m_CanRestoreFromWornOut = true;
					val7.m_RecoveryDurationMinutes = 145;
					val7.m_RepairDurationMinutes = 50;
					val7.m_RepairRequiredGear = Utilities.MakeNeededItemList(BowRepair.scraplead, BowRepair.scrapmetal);
					val7.m_RepairRequiredGearUnits = Il2CppStructArray<int>.op_Implicit(array);
					switch (Settings.instance.SportBowMaterialNeed)
					{
					case 0:
						array = new int[2] { 1, 1 };
						break;
					case 1:
						array = new int[2] { 2, 2 };
						break;
					case 2:
						array = new int[2] { 3, 4 };
						break;
					}
					val7.m_RestoreRequiredGear = Utilities.MakeNeededItemList(BowRepair.scraplead, BowRepair.scrapmetal);
					val7.m_RestoreRequiredGearUnits = Il2CppStructArray<int>.op_Implicit(array);
					val7.m_Skill = (SkillType)5;
					break;
				}
				case 2:
				{
					switch (Settings.instance.SportBowMaterialNeed)
					{
					case 0:
						array = new int[2] { 1, 1 };
						break;
					case 1:
						array = new int[2] { 2, 1 };
						break;
					case 2:
						array = new int[2] { 3, 3 };
						break;
					}
					Repairable val5 = ((Component)__instance).gameObject.AddComponent<Repairable>();
					__instance.m_Repairable = val5;
					val5.m_RepairToolChoices = Utilities.Tools();
					val5.m_DurationMinutes = 65;
					val5.m_ConditionIncrease = 35f;
					val5.m_RequiredGear = Utilities.MakeNeededItemList(BowRepair.scraplead, BowRepair.scrapmetal);
					val5.m_RequiredGearUnits = Il2CppStructArray<int>.op_Implicit(array);
					val5.m_RequiresToolToRepair = true;
					val5.m_RepairAudio = "Play_CraftingGeneric";
					Millable val6 = ((Component)__instance).gameObject.AddComponent<Millable>();
					__instance.m_Millable = val6;
					val6.m_CanRestoreFromWornOut = true;
					val6.m_RecoveryDurationMinutes = 145;
					val6.m_RepairDurationMinutes = 50;
					val6.m_RepairRequiredGear = Utilities.MakeNeededItemList(BowRepair.scraplead, BowRepair.scrapmetal);
					val6.m_RepairRequiredGearUnits = Il2CppStructArray<int>.op_Implicit(array);
					switch (Settings.instance.SportBowMaterialNeed)
					{
					case 0:
						array = new int[2] { 1, 1 };
						break;
					case 1:
						array = new int[2] { 2, 2 };
						break;
					case 2:
						array = new int[2] { 3, 4 };
						break;
					}
					val6.m_RestoreRequiredGear = Utilities.MakeNeededItemList(BowRepair.scraplead, BowRepair.scrapmetal);
					val6.m_RestoreRequiredGearUnits = Il2CppStructArray<int>.op_Implicit(array);
					val6.m_Skill = (SkillType)5;
					break;
				}
				}
			}
			if (((Object)__instance).name.ToLowerInvariant().Contains("gear_bow_woodwrights"))
			{
				switch (Settings.instance.WoodBowRepairMode)
				{
				case 0:
				{
					switch (Settings.instance.WoodBowMaterialNeed)
					{
					case 0:
						array = new int[2] { 1, 1 };
						break;
					case 1:
						array = new int[2] { 1, 2 };
						break;
					case 2:
						array = new int[2] { 2, 2 };
						break;
					}
					Repairable val11 = ((Component)__instance).gameObject.AddComponent<Repairable>();
					__instance.m_Repairable = val11;
					val11.m_RepairToolChoices = Utilities.Tools();
					val11.m_DurationMinutes = 60;
					val11.m_ConditionIncrease = 35f;
					val11.m_RequiredGear = Utilities.MakeNeededItemList(BowRepair.hardwood, BowRepair.scrapmetal);
					val11.m_RequiredGearUnits = Il2CppStructArray<int>.op_Implicit(array);
					val11.m_RequiresToolToRepair = true;
					val11.m_RepairAudio = "Play_CraftingWood";
					break;
				}
				case 1:
				{
					switch (Settings.instance.WoodBowMaterialNeed)
					{
					case 0:
						array = new int[2] { 1, 1 };
						break;
					case 1:
						array = new int[2] { 1, 2 };
						break;
					case 2:
						array = new int[2] { 2, 2 };
						break;
					}
					Millable val12 = ((Component)__instance).gameObject.AddComponent<Millable>();
					__instance.m_Millable = val12;
					val12.m_CanRestoreFromWornOut = true;
					val12.m_RecoveryDurationMinutes = 115;
					val12.m_RepairDurationMinutes = 45;
					val12.m_RepairRequiredGear = Utilities.MakeNeededItemList(BowRepair.hardwood, BowRepair.scrapmetal);
					val12.m_RepairRequiredGearUnits = Il2CppStructArray<int>.op_Implicit(array);
					switch (Settings.instance.WoodBowMaterialNeed)
					{
					case 0:
						array = new int[2] { 1, 1 };
						break;
					case 1:
						array = new int[2] { 1, 2 };
						break;
					case 2:
						array = new int[2] { 2, 2 };
						break;
					}
					val12.m_RestoreRequiredGear = Utilities.MakeNeededItemList(BowRepair.hardwood, BowRepair.scrapmetal);
					val12.m_RestoreRequiredGearUnits = Il2CppStructArray<int>.op_Implicit(array);
					val12.m_Skill = (SkillType)5;
					break;
				}
				case 2:
				{
					switch (Settings.instance.WoodBowMaterialNeed)
					{
					case 0:
						array = new int[2] { 1, 1 };
						break;
					case 1:
						array = new int[2] { 2, 2 };
						break;
					case 2:
						array = new int[2] { 3, 3 };
						break;
					}
					Repairable val9 = ((Component)__instance).gameObject.AddComponent<Repairable>();
					__instance.m_Repairable = val9;
					val9.m_RepairToolChoices = Utilities.Tools();
					val9.m_DurationMinutes = 60;
					val9.m_ConditionIncrease = 35f;
					val9.m_RequiredGear = Utilities.MakeNeededItemList(BowRepair.hardwood, BowRepair.scrapmetal);
					val9.m_RequiredGearUnits = Il2CppStructArray<int>.op_Implicit(array);
					val9.m_RequiresToolToRepair = true;
					val9.m_RepairAudio = "Play_CraftingWood";
					switch (Settings.instance.WoodBowMaterialNeed)
					{
					case 0:
						array = new int[2] { 1, 1 };
						break;
					case 1:
						array = new int[2] { 1, 2 };
						break;
					case 2:
						array = new int[2] { 2, 2 };
						break;
					}
					Millable val10 = ((Component)__instance).gameObject.AddComponent<Millable>();
					__instance.m_Millable = val10;
					val10.m_CanRestoreFromWornOut = true;
					val10.m_RecoveryDurationMinutes = 115;
					val10.m_RepairDurationMinutes = 45;
					val10.m_RepairRequiredGear = Utilities.MakeNeededItemList(BowRepair.hardwood, BowRepair.scrapmetal);
					val10.m_RepairRequiredGearUnits = Il2CppStructArray<int>.op_Implicit(array);
					switch (Settings.instance.WoodBowMaterialNeed)
					{
					case 0:
						array = new int[2] { 1, 1 };
						break;
					case 1:
						array = new int[2] { 2, 2 };
						break;
					case 2:
						array = new int[2] { 3, 3 };
						break;
					}
					val10.m_RestoreRequiredGear = Utilities.MakeNeededItemList(BowRepair.hardwood, BowRepair.scrapmetal);
					val10.m_RestoreRequiredGearUnits = Il2CppStructArray<int>.op_Implicit(array);
					val10.m_Skill = (SkillType)5;
					break;
				}
				}
			}
			if (!((Object)__instance).name.ToLowerInvariant().Contains("gear_bow_bushcraft"))
			{
				return;
			}
			switch (Settings.instance.BushBowMaterialNeed)
			{
			case 0:
				array = new int[2] { 1, 2 };
				break;
			case 1:
				array = new int[2] { 1, 3 };
				break;
			case 2:
				array = new int[2] { 2, 3 };
				break;
			}
			switch (Settings.instance.BushBowRepairMode)
			{
			case 0:
			{
				Repairable val15 = ((Component)__instance).gameObject.AddComponent<Repairable>();
				__instance.m_Repairable = val15;
				val15.m_RepairToolChoices = Utilities.Tools();
				val15.m_DurationMinutes = 60;
				val15.m_ConditionIncrease = 40f;
				val15.m_RequiredGear = Utilities.MakeNeededItemList(BowRepair.hardwood, BowRepair.cloth);
				val15.m_RequiredGearUnits = Il2CppStructArray<int>.op_Implicit(array);
				val15.m_RequiresToolToRepair = true;
				val15.m_RepairAudio = "Play_CraftingWood";
				break;
			}
			case 1:
			{
				switch (Settings.instance.BushBowMaterialNeed)
				{
				case 0:
					array = new int[2] { 1, 1 };
					break;
				case 1:
					array = new int[2] { 2, 1 };
					break;
				case 2:
					array = new int[2] { 2, 2 };
					break;
				}
				Millable val16 = ((Component)__instance).gameObject.AddComponent<Millable>();
				__instance.m_Millable = val16;
				val16.m_CanRestoreFromWornOut = true;
				val16.m_RecoveryDurationMinutes = 60;
				val16.m_RepairDurationMinutes = 30;
				val16.m_RepairRequiredGear = Utilities.MakeNeededItemList(BowRepair.hardwood, BowRepair.scrapmetal);
				val16.m_RepairRequiredGearUnits = Il2CppStructArray<int>.op_Implicit(array);
				switch (Settings.instance.BushBowMaterialNeed)
				{
				case 0:
					array = new int[2] { 1, 1 };
					break;
				case 1:
					array = new int[2] { 2, 1 };
					break;
				case 2:
					array = new int[2] { 3, 2 };
					break;
				}
				val16.m_RestoreRequiredGear = Utilities.MakeNeededItemList(BowRepair.hardwood, BowRepair.scrapmetal);
				val16.m_RestoreRequiredGearUnits = Il2CppStructArray<int>.op_Implicit(array);
				val16.m_Skill = (SkillType)5;
				break;
			}
			case 2:
			{
				switch (Settings.instance.BushBowMaterialNeed)
				{
				case 0:
					array = new int[2] { 1, 2 };
					break;
				case 1:
					array = new int[2] { 1, 3 };
					break;
				case 2:
					array = new int[2] { 2, 3 };
					break;
				}
				Repairable val13 = ((Component)__instance).gameObject.AddComponent<Repairable>();
				__instance.m_Repairable = val13;
				val13.m_RepairToolChoices = Utilities.Tools();
				val13.m_DurationMinutes = 60;
				val13.m_ConditionIncrease = 40f;
				val13.m_RequiredGear = Utilities.MakeNeededItemList(BowRepair.hardwood, BowRepair.cloth);
				val13.m_RequiredGearUnits = Il2CppStructArray<int>.op_Implicit(array);
				val13.m_RequiresToolToRepair = true;
				val13.m_RepairAudio = "Play_CraftingArrows";
				switch (Settings.instance.BushBowMaterialNeed)
				{
				case 0:
					array = new int[2] { 1, 1 };
					break;
				case 1:
					array = new int[2] { 2, 1 };
					break;
				case 2:
					array = new int[2] { 2, 2 };
					break;
				}
				Millable val14 = ((Component)__instance).gameObject.AddComponent<Millable>();
				__instance.m_Millable = val14;
				val14.m_CanRestoreFromWornOut = true;
				val14.m_RecoveryDurationMinutes = 60;
				val14.m_RepairDurationMinutes = 30;
				val14.m_RepairRequiredGear = Utilities.MakeNeededItemList(BowRepair.hardwood, BowRepair.scrapmetal);
				val14.m_RepairRequiredGearUnits = Il2CppStructArray<int>.op_Implicit(array);
				switch (Settings.instance.BushBowMaterialNeed)
				{
				case 0:
					array = new int[2] { 1, 1 };
					break;
				case 1:
					array = new int[2] { 2, 1 };
					break;
				case 2:
					array = new int[2] { 3, 2 };
					break;
				}
				val14.m_RestoreRequiredGear = Utilities.MakeNeededItemList(BowRepair.hardwood, BowRepair.scrapmetal);
				val14.m_RestoreRequiredGearUnits = Il2CppStructArray<int>.op_Implicit(array);
				val14.m_Skill = (SkillType)5;
				break;
			}
			}
		}
	}
	internal class Settings : JsonModSettings
	{
		internal static Settings instance;

		[Section("游戏基础设置")]
		[Name("生存弓修复")]
		[Description("定义弓的维修方式，需要重新加载场景才可生效。")]
		[Choice(new string[] { "手搓", "铣床", "手搓或铣床", "None" })]
		public int BowRepairMode = 2;

		[Name("修复材料")]
		[Description("定义维修所需的材料数量，需要重新加载场景才可生效")]
		[Choice(new string[] { "低", "中", "高" })]
		public int BowMaterialNeed = 1;

		[Section("其他选项")]
		[Name("启用DLC")]
		[Description("启用TFTFT DLC的项目设置。仅在安装DLC后启用，为安装DLC就启动可能会出现问题。")]
		public bool DLCEnable;

		[Section("DLC内容")]
		[Name("运动弓修复")]
		[Description("定义弓的维修方式，需要重新加载场景才可生效。")]
		[Choice(new string[] { "手搓", "铣床", "手搓或铣床", "None" })]
		public int SportBowRepairMode = 1;

		[Name("运动弓修复材料")]
		[Description("定义维修所需的材料数量，需要重新加载场景才可生效。")]
		[Choice(new string[] { "低", "中", "高" })]
		public int SportBowMaterialNeed = 1;

		[Name("木匠弓修复")]
		[Description("定义弓的维修方式，需要重新加载场景才可生效。")]
		[Choice(new string[] { "手搓", "铣床", "手搓或铣床", "None" })]
		public int WoodBowRepairMode = 2;

		[Name("木匠弓修复材料")]
		[Description("定义维修所需的材料数量，需要重新加载场景才可生效。")]
		[Choice(new string[] { "低", "中", "高" })]
		public int WoodBowMaterialNeed;

		[Name("丛林生存弓修复")]
		[Description("定义弓的维修方式，需要重新加载场景才可生效。")]
		[Choice(new string[] { "手搓", "铣床", "手搓或铣床", "None" })]
		public int BushBowRepairMode = 2;

		[Name("丛林生存弓修复材料")]
		[Description("定义维修所需的材料数量，需要重新加载场景才可生效。")]
		[Choice(new string[] { "低", "中", "高" })]
		public int BushBowMaterialNeed = 1;

		[Section("重置设置")]
		[Name("重置为默认")]
		[Description("将所有设置重置为默认，需要重新加载场景才可生效")]
		public bool ResetSettings;

		protected override void OnChange(FieldInfo field, object oldValue, object newValue)
		{
			RefreshFields();
		}

		protected override void OnConfirm()
		{
			ApplyReset();
			instance.ResetSettings = false;
			base.OnConfirm();
		}

		internal static void OnLoad()
		{
			instance.RefreshFields();
		}

		internal void RefreshFields()
		{
			if (instance.DLCEnable)
			{
				SetFieldVisible("SportBowRepairMode", visible: true);
				SetFieldVisible("WoodBowRepairMode", visible: true);
				SetFieldVisible("WoodBowMaterialNeed", visible: true);
				SetFieldVisible("SportBowMaterialNeed", visible: true);
				SetFieldVisible("BushBowMaterialNeed", visible: true);
				SetFieldVisible("BushBowRepairMode", visible: true);
			}
			else
			{
				SetFieldVisible("SportBowRepairMode", visible: false);
				SetFieldVisible("WoodBowRepairMode", visible: false);
				SetFieldVisible("WoodBowMaterialNeed", visible: false);
				SetFieldVisible("SportBowMaterialNeed", visible: false);
				SetFieldVisible("BushBowMaterialNeed", visible: false);
				SetFieldVisible("BushBowRepairMode", visible: false);
			}
		}

		public static void ApplyReset()
		{
			if (instance.ResetSettings)
			{
				instance.BowRepairMode = 2;
				instance.BowMaterialNeed = 1;
				instance.SportBowRepairMode = 1;
				instance.SportBowMaterialNeed = 1;
				instance.WoodBowRepairMode = 2;
				instance.WoodBowMaterialNeed = 0;
				instance.BushBowMaterialNeed = 1;
				instance.BushBowRepairMode = 2;
				instance.ResetSettings = false;
				instance.DLCEnable = false;
				instance.RefreshFields();
				instance.RefreshGUI();
			}
		}

		static Settings()
		{
			instance = new Settings();
		}
	}
	internal static class Utilities
	{
		public static Il2CppReferenceArray<GearItem> MakeNeededItemList(GearItem first_item, GearItem second_item)
		{
			GearItem[] array = (GearItem[])(object)new GearItem[2] { first_item, second_item };
			return Il2CppReferenceArray<GearItem>.op_Implicit(array);
		}

		public static Il2CppReferenceArray<ToolsItem> Tools()
		{
			ToolsItem[] array = (ToolsItem[])(object)new ToolsItem[2]
			{
				Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_SimpleTools")).WaitForCompletion().GetComponent<GearItem>()
					.m_ToolsItem,
				Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_HighQualityTools")).WaitForCompletion().GetComponent<GearItem>()
					.m_ToolsItem
			};
			return Il2CppReferenceArray<ToolsItem>.op_Implicit(array);
		}
	}
}
