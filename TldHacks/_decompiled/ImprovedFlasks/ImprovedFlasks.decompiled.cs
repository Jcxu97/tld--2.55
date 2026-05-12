using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using ComplexLogger;
using HarmonyLib;
using Il2Cpp;
using Il2CppSystem.Collections.Generic;
using Il2CppTLD.Gear;
using Il2CppTLD.IntBackedUnit;
using ImprovedFlasks;
using ImprovedFlasks.Utilities;
using MelonLoader;
using Microsoft.CodeAnalysis;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints | DebuggableAttribute.DebuggingModes.EnableEditAndContinue)]
[assembly: AssemblyTitle("ImprovedFlasks")]
[assembly: AssemblyDescription(null)]
[assembly: AssemblyCompany(null)]
[assembly: AssemblyProduct("ImprovedFlasks")]
[assembly: AssemblyCopyright("Copyright ©Fuar 2024")]
[assembly: AssemblyTrademark(null)]
[assembly: AssemblyFileVersion("1.2.3")]
[assembly: MelonInfo(typeof(Main), "Improved Flasks", "1.2.3", "Fuar", null)]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: MelonPriority(0)]
[assembly: MelonIncompatibleAssemblies(null)]
[assembly: TargetFramework(".NETCoreApp,Version=v6.0", FrameworkDisplayName = ".NET 6.0")]
[assembly: AssemblyVersion("1.2.3.0")]
[module: RefSafetyRules(11)]
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
namespace ImprovedFlasks
{
	public static class BuildInfo
	{
		public const string Name = "ImprovedFlasks";

		public const string Author = "Fuar";

		public const string Version = "1.2.3";

		public const string GUIName = "Improved Flasks";

		public const string Description = null;

		public const string Company = null;

		public const string DownloadLink = null;

		public const string Copyright = "Copyright ©Fuar 2024";

		public const string Trademark = null;

		public const string Product = "ImprovedFlasks";

		public const string Culture = null;

		public const int Priority = 0;
	}
	public class Main : MelonMod
	{
		internal static ComplexLogger<Main> Logger = new ComplexLogger<Main>();

		public override void OnInitializeMelon()
		{
			Logger.Log("Improved Flasks is online.", (FlaggedLoggingLevel)0, "OnInitializeMelon");
		}
	}
}
namespace ImprovedFlasks.Utilities
{
	internal class FlaskUtils
	{
		public static void ConsumeFromFlask(InsulatedFlask flaskItem)
		{
			List<GearItem> val = new List<GearItem>();
			flaskItem.GetAllItems(val);
			if (val.Count == 0)
			{
				GameAudioManager.PlayGUIError();
				HUDMessage.AddMessage("Flask is empty.", false, false);
				return;
			}
			FoodItem foodItem = val[0].m_FoodItem;
			if ((Object)(object)foodItem != (Object)null)
			{
				if (foodItem.m_IsDrink && GameManager.GetThirstComponent().m_CurrentThirst / GameManager.GetThirstComponent().m_MaxThirst < 0.01f)
				{
					HUDMessage.AddMessage(Localization.Get("GAMEPLAY_Youarenotthirsty"), false, true);
					GameAudioManager.PlayGUIError();
				}
				else if (!foodItem.m_IsDrink && GameManager.GetHungerComponent().m_MaxReserveCalories - GameManager.GetHungerComponent().GetCalorieReserves() < 10f)
				{
					GameAudioManager.PlayGUIError();
					HUDMessage.AddMessage(Localization.Get("GAMEPLAY_Youaretoofulltoeat"), false, true);
				}
				else if (flaskItem.TryRemoveItem(foodItem.m_GearItem))
				{
					PlayerManager playerManagerComponent = GameManager.GetPlayerManagerComponent();
					playerManagerComponent.UseFoodInventoryItem(foodItem.m_GearItem);
					Main.Logger.Log(((Object)foodItem).name + " consumed.", (FlaggedLoggingLevel)2, "ConsumeFromFlask");
					if (!playerManagerComponent.ShouldDestroyFoodAfterEating(playerManagerComponent.m_FoodItemEaten) && flaskItem.TryAddItem(playerManagerComponent.m_FoodItemEaten))
					{
						Main.Logger.Log(((Object)foodItem).name + " not fully consumed, added back into flask.", (FlaggedLoggingLevel)2, "ConsumeFromFlask");
					}
				}
			}
			else
			{
				Main.Logger.Log("Consumable is null!", (FlaggedLoggingLevel)16, "ConsumeFromFlask");
			}
		}
	}
}
namespace ImprovedFlasks.Patches
{
	internal class FlaskPatches
	{
		[HarmonyPatch(typeof(GearItem), "Awake")]
		public class AddFlaskToFoodFilter
		{
			public static void Postfix(GearItem __instance)
			{
				//IL_003c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0046: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
				//IL_0136: Unknown result type (might be due to invalid IL or missing references)
				//IL_0140: Unknown result type (might be due to invalid IL or missing references)
				//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
				//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
				//IL_0234: Unknown result type (might be due to invalid IL or missing references)
				//IL_023e: Unknown result type (might be due to invalid IL or missing references)
				//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
				//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
				//IL_033e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0348: Unknown result type (might be due to invalid IL or missing references)
				if (((Object)__instance).name.StartsWith("GEAR_InsulatedFlask_A"))
				{
					__instance.GearItemData.m_Type = (GearType)9;
					InsulatedFlask val = ((Component)__instance).GetComponent<InsulatedFlask>();
					if ((Object)(object)val == (Object)null)
					{
						val = ((Component)__instance).gameObject.AddComponent<InsulatedFlask>();
					}
					val.m_Capacity = ItemLiquidVolume.Liter * 1f;
					val.m_FallDamagePerMeter = 2f;
					val.m_PercentHeatLossPerMinuteIndoors = 0.15f;
					val.m_PercentHeatLossPerMinuteOutdoors = 0.25f;
					val.m_RangeToPreventHeatLossWhenNextToFire = 2f;
				}
				else if (((Object)__instance).name.StartsWith("GEAR_InsulatedFlask_B"))
				{
					__instance.GearItemData.m_Type = (GearType)9;
					InsulatedFlask val2 = ((Component)__instance).GetComponent<InsulatedFlask>();
					if ((Object)(object)val2 == (Object)null)
					{
						val2 = ((Component)__instance).gameObject.AddComponent<InsulatedFlask>();
					}
					val2.m_Capacity = ItemLiquidVolume.Liter * 1.5f;
					val2.m_FallDamagePerMeter = 4f;
					val2.m_PercentHeatLossPerMinuteIndoors = 0.12f;
					val2.m_PercentHeatLossPerMinuteOutdoors = 0.22f;
					val2.m_RangeToPreventHeatLossWhenNextToFire = 3f;
				}
				else if (((Object)__instance).name.StartsWith("GEAR_InsulatedFlask_C"))
				{
					__instance.GearItemData.m_Type = (GearType)9;
					InsulatedFlask val3 = ((Component)__instance).GetComponent<InsulatedFlask>();
					if ((Object)(object)val3 == (Object)null)
					{
						val3 = ((Component)__instance).gameObject.AddComponent<InsulatedFlask>();
					}
					val3.m_Capacity = ItemLiquidVolume.Liter * 2f;
					val3.m_FallDamagePerMeter = 6f;
					val3.m_PercentHeatLossPerMinuteIndoors = 0.09f;
					val3.m_PercentHeatLossPerMinuteOutdoors = 0.19f;
					val3.m_RangeToPreventHeatLossWhenNextToFire = 4f;
				}
				else if (((Object)__instance).name.StartsWith("GEAR_InsulatedFlask_D"))
				{
					__instance.GearItemData.m_Type = (GearType)9;
					InsulatedFlask val4 = ((Component)__instance).GetComponent<InsulatedFlask>();
					if ((Object)(object)val4 == (Object)null)
					{
						val4 = ((Component)__instance).gameObject.AddComponent<InsulatedFlask>();
					}
					val4.m_Capacity = ItemLiquidVolume.Liter * 2.5f;
					val4.m_FallDamagePerMeter = 8f;
					val4.m_PercentHeatLossPerMinuteIndoors = 0.06f;
					val4.m_PercentHeatLossPerMinuteOutdoors = 0.16f;
					val4.m_RangeToPreventHeatLossWhenNextToFire = 5f;
				}
				else if (((Object)__instance).name.StartsWith("GEAR_InsulatedFlask_E"))
				{
					__instance.GearItemData.m_Type = (GearType)9;
					InsulatedFlask val5 = ((Component)__instance).GetComponent<InsulatedFlask>();
					if ((Object)(object)val5 == (Object)null)
					{
						val5 = ((Component)__instance).gameObject.AddComponent<InsulatedFlask>();
					}
					val5.m_Capacity = ItemLiquidVolume.Liter * 3f;
					val5.m_FallDamagePerMeter = 10f;
					val5.m_PercentHeatLossPerMinuteIndoors = 0.03f;
					val5.m_PercentHeatLossPerMinuteOutdoors = 0.13f;
					val5.m_RangeToPreventHeatLossWhenNextToFire = 6f;
				}
				else if (((Object)__instance).name.StartsWith("GEAR_InsulatedFlask_F"))
				{
					__instance.GearItemData.m_Type = (GearType)9;
					InsulatedFlask val6 = ((Component)__instance).GetComponent<InsulatedFlask>();
					if ((Object)(object)val6 == (Object)null)
					{
						val6 = ((Component)__instance).gameObject.AddComponent<InsulatedFlask>();
					}
					val6.m_Capacity = ItemLiquidVolume.Liter * 3.5f;
					val6.m_FallDamagePerMeter = 12f;
					val6.m_PercentHeatLossPerMinuteIndoors = 0.01f;
					val6.m_PercentHeatLossPerMinuteOutdoors = 0.1f;
					val6.m_RangeToPreventHeatLossWhenNextToFire = 7f;
				}
				else if (((Object)__instance).name.StartsWith("GEAR_InsulatedFlask_G"))
				{
					__instance.GearItemData.m_Type = (GearType)9;
					InsulatedFlask val7 = ((Component)__instance).GetComponent<InsulatedFlask>();
					if ((Object)(object)val7 == (Object)null)
					{
						val7 = ((Component)__instance).gameObject.AddComponent<InsulatedFlask>();
					}
					val7.m_Capacity = ItemLiquidVolume.Liter * 4f;
					val7.m_FallDamagePerMeter = 14f;
					val7.m_PercentHeatLossPerMinuteIndoors = 0f;
					val7.m_PercentHeatLossPerMinuteOutdoors = 0.05f;
					val7.m_RangeToPreventHeatLossWhenNextToFire = 8f;
				}
			}
		}
	}
	internal class InventoryPatches
	{
		[HarmonyPatch(typeof(ItemDescriptionPage), "UpdateButtons")]
		public class FlaskButtonsChange
		{
			public static void Postfix(ItemDescriptionPage __instance, ref GearItem gi)
			{
				if ((Object)(object)__instance == (Object)null || (Object)(object)gi == (Object)null || !Object.op_Implicit((Object)(object)gi.m_InsulatedFlask))
				{
					return;
				}
				__instance.m_Label_MouseButtonEquip.text = "Drink";
				__instance.m_MouseButtonExamine.SetActive(true);
				__instance.m_Label_MouseButtonExamine.text = "Transfer";
				__instance.m_Label_MouseButtonEquip.text = "饮用";
				__instance.m_Label_MouseButtonExamine.text = "倒入";
				__instance.m_OnActionsDelegate = __instance.m_OnEquipDelegate;
				if ((Object)(object)__instance.m_MouseButtonExamine != (Object)null)
				{
					UIButton component = __instance.m_MouseButtonExamine.GetComponent<UIButton>();
					if ((Object)(object)component != (Object)null)
					{
						((UIButtonColor)component).isEnabled = true;
					}
				}
			}
		}

		[HarmonyPatch(typeof(ItemDescriptionPage), "OnEquip")]
		public class OverrideDrinkButtonOnClickForFlask
		{
			public static bool Prefix(ItemDescriptionPage __instance)
			{
				if ((Object)(object)__instance == (Object)null || (Object)(object)InterfaceManager.GetPanel<Panel_Inventory>() == (Object)null)
				{
					return true;
				}
				if (!((Behaviour)InterfaceManager.GetPanel<Panel_Inventory>()).isActiveAndEnabled)
				{
					return true;
				}
				InventoryGridDataItem currentlySelectedItem = InterfaceManager.GetPanel<Panel_Inventory>().GetCurrentlySelectedItem();
				if (currentlySelectedItem != null && (Object)(object)currentlySelectedItem.m_GearItem != (Object)null && Object.op_Implicit((Object)(object)currentlySelectedItem.m_GearItem.m_InsulatedFlask))
				{
					FlaskUtils.ConsumeFromFlask(currentlySelectedItem.m_GearItem.m_InsulatedFlask);
					return false;
				}
				return true;
			}
		}
	}
	internal class RadialPatches
	{
		[HarmonyPatch(typeof(Panel_ActionsRadial), "GetDrinkItemsInInventory")]
		public class AddFlaskToRadial
		{
			public static bool Prefix => false;

			public static void Postfix(Panel_ActionsRadial __instance, ref List<GearItem> __result)
			{
				//IL_0033: Unknown result type (might be due to invalid IL or missing references)
				//IL_0038: Unknown result type (might be due to invalid IL or missing references)
				//IL_010a: Unknown result type (might be due to invalid IL or missing references)
				//IL_010f: Unknown result type (might be due to invalid IL or missing references)
				__instance.m_TempGearItemListNormal.Clear();
				__instance.m_TempGearItemListFavorites.Clear();
				GearItem potableWaterSupply = GameManager.GetInventoryComponent().GetPotableWaterSupply();
				if ((Object)(object)potableWaterSupply != (Object)null && potableWaterSupply.m_WaterSupply.m_VolumeInLiters > ItemLiquidVolume.Zero)
				{
					__instance.m_TempGearItemListFavorites.Add(potableWaterSupply);
				}
				for (int i = 0; i < GameManager.GetInventoryComponent().m_Items.Count; i++)
				{
					GearItem val = GearItemObject.op_Implicit(GameManager.GetInventoryComponent().m_Items[i]);
					if (Object.op_Implicit((Object)(object)val) && !Object.op_Implicit((Object)(object)val.m_EnergyBoost) && Object.op_Implicit((Object)(object)val.m_FoodItem) && val.m_FoodItem.m_IsDrink && (!val.IsWornOut() || val.m_IsInSatchel))
					{
						if (val.m_IsInSatchel)
						{
							__instance.m_TempGearItemListFavorites.Add(val);
						}
						else
						{
							__instance.m_TempGearItemListNormal.Add(val);
						}
					}
					else if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)val.m_InsulatedFlask) && val.m_InsulatedFlask.m_VolumeLitres > ItemLiquidVolume.Zero)
					{
						__instance.m_TempGearItemListFavorites.Add(val);
					}
				}
				Enumerator<GearItem> enumerator = __instance.m_TempGearItemListNormal.GetEnumerator();
				while (enumerator.MoveNext())
				{
					GearItem current = enumerator.Current;
					__instance.m_TempGearItemListFavorites.Add(current);
				}
				__result = __instance.m_TempGearItemListFavorites;
			}
		}

		[HarmonyPatch(typeof(Panel_ActionsRadial), "UseItem")]
		public class OverrideDefaultAction
		{
			public static bool Prefix(ref GearItem gi)
			{
				return !Object.op_Implicit((Object)(object)gi.m_InsulatedFlask);
			}

			public static void Postfix(ref GearItem gi)
			{
				if (Object.op_Implicit((Object)(object)gi.m_InsulatedFlask))
				{
					InterfaceManager.GetPanel<Panel_Inventory>().UpdateFilteredInventoryList();
					FlaskUtils.ConsumeFromFlask(gi.m_InsulatedFlask);
				}
			}
		}

		[HarmonyPatch(typeof(RadialMenuArm), "SetRadialInfoGear")]
		public class AddDrinkNameToRadialItem
		{
			public static void Postfix(RadialMenuArm __instance)
			{
				if (Object.op_Implicit((Object)(object)__instance.m_GearItem.m_InsulatedFlask))
				{
					__instance.m_NameWhenHoveredOver = __instance.m_GearItem.m_InsulatedFlask.m_Items[0].m_GearItem.DisplayName.Replace("Cup of", "");
				}
			}
		}
	}
	[HarmonyPatch(typeof(ItemDescriptionPage), "UpdateInsulatedFlaskIndicators")]
	internal static class ItemDescriptionPage_UpdateInsulatedFlaskIndicators
	{
		public static void Postfix(ItemDescriptionPage __instance, InsulatedFlask insulatedFlask)
		{
			if (!((Object)(object)insulatedFlask == (Object)null))
			{
				UISprite flaskHotFillSprite = __instance.m_FlaskHotFillSprite;
				object obj;
				if (flaskHotFillSprite == null)
				{
					obj = null;
				}
				else
				{
					Transform transform = ((Component)flaskHotFillSprite).transform;
					obj = ((transform != null) ? transform.parent : null);
				}
				if ((Object)obj != (Object)null)
				{
					((Component)((Component)flaskHotFillSprite).transform.parent).gameObject.SetActive(false);
				}
				GameObject flaskHot = __instance.m_FlaskHot;
				UILabel val = ((flaskHot != null) ? flaskHot.GetComponentInChildren<UILabel>() : null);
				if ((Object)(object)val != (Object)null)
				{
					int value = (int)insulatedFlask.m_HeatPercent;
					val.text = $"{Localization.Get("GAMEPLAY_Hot")} ({value}%)";
				}
			}
		}
	}
	[HarmonyPatch(typeof(Panel_InsulatedFlask), "RefreshTables")]
	internal static class Panel_InsulatedFlask_RefreshTables
	{
		public static void Postfix(Panel_InsulatedFlask __instance)
		{
			if (!((Object)(object)((__instance != null) ? __instance.m_InsulatedFlask : null) == (Object)null))
			{
				float heatPercent = __instance.m_InsulatedFlask.m_HeatPercent;
				int value = (int)heatPercent;
				string value2 = $"{value}%";
				string value3 = ((heatPercent > 0f) ? "be7817" : "5b828f");
				string displayNameWithoutConditionForInventoryInterfaces = __instance.m_InsulatedFlask.GearItem.GetDisplayNameWithoutConditionForInventoryInterfaces();
				__instance.m_ContainerUI.m_ContainerTitle.text = $"{displayNameWithoutConditionForInventoryInterfaces} [{value3}]({value2})[-]";
			}
		}
	}
}
