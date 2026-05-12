using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using AnimalFatFuel;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Collections.Generic;
using Il2CppTLD.Gear;
using Il2CppTLD.IntBackedUnit;
using MelonLoader;
using Microsoft.CodeAnalysis;
using ModSettings;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints | DebuggableAttribute.DebuggingModes.EnableEditAndContinue)]
[assembly: MelonInfo(typeof(AnimalFatFuelMain), "MORE_fuel_accelerant", "1.4.0", "hzb1130", null)]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: TargetFramework(".NETCoreApp,Version=v6.0", FrameworkDisplayName = ".NET 6.0")]
[assembly: AssemblyVersion("0.0.0.0")]
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
namespace AnimalFatFuel
{
	public class AnimalFatFuelMain : MelonMod
	{
		public static List<int> tempTinderInstances = new List<int>();

		public static List<int> tempCharcoalInstances = new List<int>();

		public override void OnInitializeMelon()
		{
			Settings.OnLoad();
		}
	}
	internal class AnimalFatFuelSettings : JsonModSettings
	{
		[Section("Tinder Fuel / 火引燃料设置")]
		[Name("Use Tinder as Fuel / 火引可以作为燃料")]
		public bool tinderAsFuel = false;

		[Name("Tinder Burn Time Multiplier / 火引燃烧时间倍率")]
		[Description("Based on original value / 基于原版燃烧时间")]
		[Slider(1f, 10f)]
		public int tinderBurnMultiplier = 1;

		[Section("Animal Fat Fuel / 动物脂肪燃料设置")]
		[Name("Enable Animal Fat as Fuel / 启用动物脂肪作为燃料")]
		public bool animalFatAsFuel = false;

		[Name("Burn Time Per KG (Min) / 燃烧时间(分钟/kg)")]
		[Slider(10f, 120f)]
		public int burnMinutesPerKg = 60;

		[Name("Fire Heat Increase / 火堆温度增加")]
		[Slider(1f, 20f)]
		public int heatIncrease = 5;

		[Section("Charcoal Fuel / 木炭燃料")]
		[Name("Enable Charcoal as Fuel / 启用木炭作为燃料")]
		public bool charcoalAsFuel = false;

		[Name("Burn Time (Minutes) / 燃烧时间(分钟)")]
		[Slider(5f, 60f)]
		public int charcoalBurnMinutes = 20;

		[Name("Heat Increase / 温度增加")]
		[Slider(1f, 20f)]
		public int charcoalHeatIncrease = 3;

		[Section("Accelerant Probability / 助燃剂概率消耗")]
		[Name("Enable Probability Consumption / 启用概率消耗")]
		public bool enableAccelerantProbability = false;

		[Name("Consume Chance 1/X / 消耗概率 1/X")]
		[Slider(1f, 10f)]
		public int accelerantConsumeChance = 2;

		protected override void OnChange(FieldInfo field, object oldValue, object newValue)
		{
			base.OnChange(field, oldValue, newValue);
			if (field.Name == "tinderAsFuel")
			{
				SetFieldVisible("tinderBurnMultiplier", (bool)newValue);
			}
			if (field.Name == "animalFatAsFuel")
			{
				bool visible = (bool)newValue;
				SetFieldVisible("burnMinutesPerKg", visible);
				SetFieldVisible("heatIncrease", visible);
			}
			if (field.Name == "charcoalAsFuel")
			{
				bool visible2 = (bool)newValue;
				SetFieldVisible("charcoalBurnMinutes", visible2);
				SetFieldVisible("charcoalHeatIncrease", visible2);
			}
			if (field.Name == "enableAccelerantProbability")
			{
				SetFieldVisible("accelerantConsumeChance", (bool)newValue);
			}
		}
	}
	internal static class Settings
	{
		public static AnimalFatFuelSettings options;

		public static void OnLoad()
		{
			options = new AnimalFatFuelSettings();
			options.AddToModSettings("更多的燃料v1.4");
			options.SetFieldVisible("tinderBurnMultiplier", options.tinderAsFuel);
			options.SetFieldVisible("burnMinutesPerKg", options.animalFatAsFuel);
			options.SetFieldVisible("heatIncrease", options.animalFatAsFuel);
			options.SetFieldVisible("charcoalBurnMinutes", options.charcoalAsFuel);
			options.SetFieldVisible("charcoalHeatIncrease", options.charcoalAsFuel);
			options.SetFieldVisible("accelerantConsumeChance", options.enableAccelerantProbability);
		}
	}
	[HarmonyPatch(typeof(Panel_FireStart), "OnStartFire")]
	internal static class Patch_Accelerant_ProbabilityConsume
	{
		private static void Prefix(Panel_FireStart __instance)
		{
			try
			{
				if (!Settings.options.enableAccelerantProbability)
				{
					return;
				}
				int selectedAccelerantIndex = __instance.m_SelectedAccelerantIndex;
				if (selectedAccelerantIndex < 0 || __instance.m_AccelerantList == null || selectedAccelerantIndex >= __instance.m_AccelerantList.Count)
				{
					return;
				}
				GearItem val = __instance.m_AccelerantList[selectedAccelerantIndex];
				if (!((Object)(object)val == (Object)null) && !(((Object)val).name != "GEAR_Accelerant"))
				{
					FireStarterItem fireStarterItem = val.m_FireStarterItem;
					if (!((Object)(object)fireStarterItem == (Object)null))
					{
						int accelerantConsumeChance = Settings.options.accelerantConsumeChance;
						int num = Random.Range(1, accelerantConsumeChance + 1);
						fireStarterItem.m_ConsumeOnUse = num == 1;
					}
				}
			}
			catch
			{
			}
		}
	}
	[HarmonyPatch(typeof(GearItem), "Awake")]
	internal static class PatchGearItemAwake
	{
		internal static void Postfix(GearItem __instance)
		{
			if ((Object)(object)__instance == (Object)null)
			{
				return;
			}
			if (Settings.options.animalFatAsFuel && ((Object)__instance).name.Contains("GEAR_AnimalFat"))
			{
				if (!((Object)(object)__instance.m_FuelSourceItem != (Object)null))
				{
					FuelSourceItem val = ((Component)__instance).gameObject.AddComponent<FuelSourceItem>();
					FuelCalculator.Apply(__instance, val);
					__instance.m_FuelSourceItem = val;
				}
			}
			else
			{
				FuelSourceItem fuelSourceItem = __instance.m_FuelSourceItem;
				if ((Object)(object)fuelSourceItem != (Object)null && fuelSourceItem.m_IsTinder)
				{
					fuelSourceItem.m_BurnDurationHours *= (float)Settings.options.tinderBurnMultiplier;
				}
			}
		}
	}
	[HarmonyPatch(typeof(Panel_FeedFire), "Enable", new Type[] { typeof(bool) })]
	internal static class Patch_FeedFire_Enable
	{
		private static void Prefix()
		{
			AnimalFatFuelMain.tempTinderInstances.Clear();
			AnimalFatFuelMain.tempCharcoalInstances.Clear();
			Inventory inventoryComponent = GameManager.GetInventoryComponent();
			if ((Object)(object)inventoryComponent == (Object)null)
			{
				return;
			}
			Enumerator<GearItemObject> enumerator = inventoryComponent.m_Items.GetEnumerator();
			while (enumerator.MoveNext())
			{
				GearItemObject current = enumerator.Current;
				GearItem val = GearItemObject.op_Implicit(current);
				if (!((Object)(object)val == (Object)null))
				{
					FuelSourceItem fuelSourceItem = val.m_FuelSourceItem;
					if (Settings.options.tinderAsFuel && (Object)(object)fuelSourceItem != (Object)null && fuelSourceItem.m_IsTinder)
					{
						AnimalFatFuelMain.tempTinderInstances.Add(val.m_InstanceID);
						fuelSourceItem.m_IsTinder = false;
					}
					if (Settings.options.charcoalAsFuel && ((Object)val).name == "GEAR_Charcoal" && (Object)(object)val.m_FuelSourceItem == (Object)null)
					{
						FuelSourceItem val2 = ((Component)val).gameObject.AddComponent<FuelSourceItem>();
						val2.m_BurnDurationHours = (float)Settings.options.charcoalBurnMinutes / 60f;
						val2.m_HeatIncrease = Settings.options.charcoalHeatIncrease;
						val2.m_HeatInnerRadius = 2f;
						val2.m_HeatOuterRadius = 5f;
						val2.m_IsTinder = false;
						val.m_FuelSourceItem = val2;
						AnimalFatFuelMain.tempCharcoalInstances.Add(val.m_InstanceID);
					}
				}
			}
		}
	}
	[HarmonyPatch(typeof(Panel_FeedFire), "ExitFeedFireInterface")]
	internal static class Patch_FeedFire_Exit
	{
		private static void Postfix()
		{
			Inventory inventoryComponent = GameManager.GetInventoryComponent();
			if ((Object)(object)inventoryComponent == (Object)null)
			{
				AnimalFatFuelMain.tempTinderInstances.Clear();
				return;
			}
			Enumerator<GearItemObject> enumerator = inventoryComponent.m_Items.GetEnumerator();
			while (enumerator.MoveNext())
			{
				GearItemObject current = enumerator.Current;
				GearItem val = GearItemObject.op_Implicit(current);
				if ((Object)(object)val == (Object)null)
				{
					continue;
				}
				FuelSourceItem fuelSourceItem = val.m_FuelSourceItem;
				if (!((Object)(object)fuelSourceItem == (Object)null))
				{
					bool flag;
					switch (((Object)val).name)
					{
					case "GEAR_Tinder":
					case "GEAR_PaperStack":
					case "GEAR_CattailTinder":
					case "GEAR_BarkTinder":
					case "GEAR_Newsprint":
					case "GEAR_NewsprintRoll":
					case "GEAR_CashBundle":
						flag = true;
						break;
					default:
						flag = false;
						break;
					}
					if (flag)
					{
						fuelSourceItem.m_IsTinder = true;
					}
					if (((Object)val).name == "GEAR_Charcoal" && !fuelSourceItem.m_IsTinder)
					{
						Object.Destroy((Object)(object)fuelSourceItem);
						val.m_FuelSourceItem = null;
					}
				}
			}
			AnimalFatFuelMain.tempTinderInstances.Clear();
		}
	}
	[HarmonyPatch(typeof(GearItemListEntry), "Update")]
	public static class Patch_GearItemListEntry_AnimalFat
	{
		private static void Postfix(GearItemListEntry __instance)
		{
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			GearItem gearItem = __instance.m_GearItem;
			if ((Object)(object)gearItem == (Object)null)
			{
				return;
			}
			UILabel conditionLabel = __instance.m_ConditionLabel;
			if (((Object)gearItem).name == "GEAR_Charcoal")
			{
				__instance.m_DisplayCondition = false;
				__instance.m_DisplayItemCount = true;
				if ((Object)(object)conditionLabel != (Object)null)
				{
					conditionLabel.text = "";
					((Behaviour)conditionLabel).enabled = true;
				}
			}
			else if (((Object)gearItem).name == "GEAR_AnimalFat" && !((Object)(object)conditionLabel == (Object)null))
			{
				if (__instance.m_IsSelected)
				{
					__instance.m_DisplayCondition = true;
					__instance.m_DisplayItemCount = false;
					float normalizedCondition = gearItem.GetNormalizedCondition();
					string value = Mathf.RoundToInt(normalizedCondition * 100f) + "%";
					float value2 = gearItem.GetItemWeightKG(false) / ItemWeight.FromKilograms(1f);
					conditionLabel.text = $"{value} {value2:0.0}kg";
					((Behaviour)conditionLabel).enabled = true;
				}
				else
				{
					__instance.m_DisplayCondition = false;
					__instance.m_DisplayItemCount = true;
					conditionLabel.text = "";
					((Behaviour)conditionLabel).enabled = true;
				}
			}
		}
	}
	[HarmonyPatch(typeof(FuelSourceItem), "GetModifiedBurnDurationHours")]
	internal static class PatchBurnDuration
	{
		private static bool Prefix(FuelSourceItem __instance, float normalizedCondition, ref float __result)
		{
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			if (!Settings.options.animalFatAsFuel)
			{
				return true;
			}
			GearItem component = ((Component)__instance).GetComponent<GearItem>();
			if ((Object)(object)component == (Object)null || !((Object)component).name.Contains("GEAR_AnimalFat"))
			{
				return true;
			}
			float num = component.GetItemWeightKG(false) / ItemWeight.FromKilograms(1f);
			Skill_Firestarting skillFireStarting = GameManager.GetSkillFireStarting();
			Il2CppStructArray<int> durationPercentIncrease = skillFireStarting.m_DurationPercentIncrease;
			int currentTierNumber = ((Skill)skillFireStarting).GetCurrentTierNumber();
			float num2 = 1f + (float)((Il2CppArrayBase<int>)(object)durationPercentIncrease)[currentTierNumber] / 100f;
			__result = (float)Settings.options.burnMinutesPerKg / 60f * num * num2;
			return false;
		}
	}
	internal static class FuelCalculator
	{
		public static void Apply(GearItem gear, FuelSourceItem fs)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			float num = gear.GetItemWeightKG(false) / ItemWeight.FromKilograms(1f);
			fs.m_BurnDurationHours = (float)Settings.options.burnMinutesPerKg / 60f * num;
			fs.m_HeatIncrease = Settings.options.heatIncrease;
			fs.m_HeatInnerRadius = 2.5f;
			fs.m_HeatOuterRadius = 6f;
			fs.m_IsTinder = false;
		}
	}
}
