using System;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Collections.Generic;
using Il2CppTLD.Cooking;
using Il2CppTLD.Gear;
using Il2CppTLD.UI.Scroll;
using UnityEngine;

namespace BlueprintCleaner;

internal class Patches
{
	[HarmonyPatch(typeof(BlueprintManager), "LoadAddressableBlueprints")]
	internal static class BlueprintManager_LoadAddressableBlueprints
	{
		private static void Postfix(BlueprintManager __instance)
		{
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			if (Main.vanillaDisplay)
			{
				return;
			}
			blueprintDuplicates.Clear();
			activeBlueprint.Clear();
			Enumerator<BlueprintData> enumerator = __instance.m_AllBlueprints.GetEnumerator();
			while (enumerator.MoveNext())
			{
				BlueprintData current = enumerator.Current;
				string text = null;
				if (((Object)current).name == "BLUEPRINT_GEAR_NoiseMaker_A")
				{
					continue;
				}
				CraftingLocation requiredCraftingLocation;
				if ((Object)(object)current.m_CraftedResultDecoration != (Object)null)
				{
					string name = ((Object)current.m_CraftedResultDecoration).name;
					requiredCraftingLocation = current.m_RequiredCraftingLocation;
					text = name + ((object)(CraftingLocation)(ref requiredCraftingLocation)).ToString();
				}
				else if ((Object)(object)current.m_CraftedResultGear != (Object)null)
				{
					string name2 = ((Object)current.m_CraftedResultGear).name;
					requiredCraftingLocation = current.m_RequiredCraftingLocation;
					text = name2 + ((object)(CraftingLocation)(ref requiredCraftingLocation)).ToString();
				}
				if (text != null)
				{
					if (blueprintDuplicates.ContainsKey(text))
					{
						blueprintDuplicates[text].Add(((Object)current).name);
						continue;
					}
					blueprintDuplicates[text] = new List<string>();
					blueprintDuplicates[text].Add(((Object)current).name);
				}
			}
			List<string> val = new List<string>();
			Enumerator<string, List<string>> enumerator2 = blueprintDuplicates.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				KeyValuePair<string, List<string>> current2 = enumerator2.Current;
				if (current2.Value.Count == 1)
				{
					val.Add(current2.Key);
				}
			}
			Enumerator<string> enumerator3 = val.GetEnumerator();
			while (enumerator3.MoveNext())
			{
				string current3 = enumerator3.Current;
				blueprintDuplicates.Remove(current3);
			}
			List<string> val2 = new List<string>();
			foreach (string item in Main.blueprintsRemoved)
			{
				enumerator2 = blueprintDuplicates.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					KeyValuePair<string, List<string>> current5 = enumerator2.Current;
					if (current5.Value != null && current5.Value.Contains(item))
					{
						current5.Value.Remove(item);
						if (current5.Value == null || current5.Value.Count == 0)
						{
							val2.Add(current5.Key);
						}
					}
				}
			}
			enumerator3 = val2.GetEnumerator();
			while (enumerator3.MoveNext())
			{
				string current6 = enumerator3.Current;
				blueprintDuplicates[current6] = new List<string>();
			}
			enumerator2 = blueprintDuplicates.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				KeyValuePair<string, List<string>> current7 = enumerator2.Current;
				activeBlueprint[current7.Key] = ((current7.Value != null && current7.Value.Count > 0) ? current7.Value[0] : "");
			}
		}
	}

	[HarmonyPatch(typeof(Panel_Crafting), "ItemPassesFilter")]
	internal static class Panel_Crafting_ItemPassesFilter
	{
		private static void Postfix(Panel_Crafting __instance, BlueprintData bpi, ref bool __result)
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			if (Main.vanillaDisplay || !__result)
			{
				return;
			}
			CraftingLocation requiredCraftingLocation;
			if ((Object)(object)bpi.m_CraftedResultDecoration != (Object)null)
			{
				string name = ((Object)bpi.m_CraftedResultDecoration).name;
				requiredCraftingLocation = bpi.m_RequiredCraftingLocation;
				string text = name + ((object)(CraftingLocation)(ref requiredCraftingLocation)).ToString();
				string name2 = ((Object)bpi).name;
				if (blueprintDuplicates.ContainsKey(text))
				{
					if (activeBlueprint[text] == name2)
					{
						__result = true;
					}
					else
					{
						__result = false;
					}
				}
				else if (Main.blueprintsRemoved.Contains(name2))
				{
					__result = false;
				}
			}
			else
			{
				if (!((Object)(object)bpi.m_CraftedResultGear != (Object)null))
				{
					return;
				}
				string name3 = ((Object)bpi.m_CraftedResultGear).name;
				requiredCraftingLocation = bpi.m_RequiredCraftingLocation;
				string text2 = name3 + ((object)(CraftingLocation)(ref requiredCraftingLocation)).ToString();
				string name4 = ((Object)bpi).name;
				if (blueprintDuplicates.ContainsKey(text2))
				{
					if (activeBlueprint[text2] == name4)
					{
						__result = true;
					}
					else
					{
						__result = false;
					}
				}
				else if (Main.blueprintsRemoved.Contains(name4))
				{
					__result = false;
				}
			}
		}
	}

	[HarmonyPatch(typeof(Panel_Crafting), "HandleInput")]
	internal static class Panel_Crafting_HandleInput
	{
		private static void Postfix(Panel_Crafting __instance)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_027e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_02be: Unknown result type (might be due to invalid IL or missing references)
			//IL_0189: Unknown result type (might be due to invalid IL or missing references)
			//IL_018e: Unknown result type (might be due to invalid IL or missing references)
			//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
			if (InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.settings.viewKey))
			{
				Main.vanillaDisplay = !Main.vanillaDisplay;
				__instance.OnCategoryChanged(__instance.m_CategoryNavigation.m_CurrentIndex);
			}
			if (Main.vanillaDisplay)
			{
				return;
			}
			CraftingLocation requiredCraftingLocation;
			if (InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.settings.rightKey))
			{
				if ((Object)(object)__instance.SelectedBPI.m_CraftedResultDecoration != (Object)null)
				{
					string name = ((Object)__instance.SelectedBPI.m_CraftedResultDecoration).name;
					requiredCraftingLocation = __instance.SelectedBPI.m_RequiredCraftingLocation;
					string text = name + ((object)(CraftingLocation)(ref requiredCraftingLocation)).ToString();
					string name2 = ((Object)__instance.SelectedBPI).name;
					int num = 0;
					if (!activeBlueprint.ContainsKey(text))
					{
						return;
					}
					int count = blueprintDuplicates[text].Count;
					for (int i = 0; i < count; i++)
					{
						if (blueprintDuplicates[text][i] == name2)
						{
							num = i;
						}
					}
					if (num == count - 1)
					{
						activeBlueprint[text] = blueprintDuplicates[text][0];
					}
					else
					{
						activeBlueprint[text] = blueprintDuplicates[text][num + 1];
					}
					lastBlueprintSelected = activeBlueprint[text];
					__instance.OnCategoryChanged(__instance.m_CategoryNavigation.m_CurrentIndex);
				}
				else
				{
					if (!((Object)(object)__instance.SelectedBPI.m_CraftedResultGear != (Object)null))
					{
						return;
					}
					string name3 = ((Object)__instance.SelectedBPI.m_CraftedResultGear).name;
					requiredCraftingLocation = __instance.SelectedBPI.m_RequiredCraftingLocation;
					string text2 = name3 + ((object)(CraftingLocation)(ref requiredCraftingLocation)).ToString();
					string name4 = ((Object)__instance.SelectedBPI).name;
					int num2 = 0;
					if (!activeBlueprint.ContainsKey(text2))
					{
						return;
					}
					int count2 = blueprintDuplicates[text2].Count;
					for (int j = 0; j < count2; j++)
					{
						if (blueprintDuplicates[text2][j] == name4)
						{
							num2 = j;
						}
					}
					if (num2 == count2 - 1)
					{
						activeBlueprint[text2] = blueprintDuplicates[text2][0];
					}
					else
					{
						activeBlueprint[text2] = blueprintDuplicates[text2][num2 + 1];
					}
					lastBlueprintSelected = activeBlueprint[text2];
					__instance.OnCategoryChanged(__instance.m_CategoryNavigation.m_CurrentIndex);
				}
			}
			else
			{
				if (!InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.settings.leftKey))
				{
					return;
				}
				if ((Object)(object)__instance.SelectedBPI.m_CraftedResultDecoration != (Object)null)
				{
					string name5 = ((Object)__instance.SelectedBPI.m_CraftedResultDecoration).name;
					requiredCraftingLocation = __instance.SelectedBPI.m_RequiredCraftingLocation;
					string text3 = name5 + ((object)(CraftingLocation)(ref requiredCraftingLocation)).ToString();
					string name6 = ((Object)__instance.SelectedBPI).name;
					int num3 = 0;
					if (!activeBlueprint.ContainsKey(text3))
					{
						return;
					}
					int count3 = blueprintDuplicates[text3].Count;
					for (int k = 0; k < count3; k++)
					{
						if (blueprintDuplicates[text3][k] == name6)
						{
							num3 = k;
						}
					}
					if (num3 == 0)
					{
						activeBlueprint[text3] = blueprintDuplicates[text3][count3 - 1];
					}
					else
					{
						activeBlueprint[text3] = blueprintDuplicates[text3][num3 - 1];
					}
					lastBlueprintSelected = activeBlueprint[text3];
					__instance.OnCategoryChanged(__instance.m_CategoryNavigation.m_CurrentIndex);
				}
				else
				{
					if (!((Object)(object)__instance.SelectedBPI.m_CraftedResultGear != (Object)null))
					{
						return;
					}
					string name7 = ((Object)__instance.SelectedBPI.m_CraftedResultGear).name;
					requiredCraftingLocation = __instance.SelectedBPI.m_RequiredCraftingLocation;
					string text4 = name7 + ((object)(CraftingLocation)(ref requiredCraftingLocation)).ToString();
					string name8 = ((Object)__instance.SelectedBPI).name;
					int num4 = 0;
					if (!activeBlueprint.ContainsKey(text4))
					{
						return;
					}
					int count4 = blueprintDuplicates[text4].Count;
					for (int l = 0; l < count4; l++)
					{
						if (blueprintDuplicates[text4][l] == name8)
						{
							num4 = l;
						}
					}
					if (num4 == 0)
					{
						activeBlueprint[text4] = blueprintDuplicates[text4][count4 - 1];
					}
					else
					{
						activeBlueprint[text4] = blueprintDuplicates[text4][num4 - 1];
					}
					lastBlueprintSelected = activeBlueprint[text4];
					__instance.OnCategoryChanged(__instance.m_CategoryNavigation.m_CurrentIndex);
				}
			}
		}
	}

	[HarmonyPatch(typeof(Panel_Crafting), "OnCategoryChanged")]
	internal static class Panel_Crafting_OnCategoryChanged
	{
		private static void Postfix(Panel_Crafting __instance)
		{
			if (scrollValue.HasValue)
			{
				__instance.m_ScrollBehaviour.m_ScrollBar.scrollValue = scrollValue.Value;
				scrollValue = null;
			}
			if (lastBlueprintSelected == null)
			{
				return;
			}
			int num = 0;
			Enumerator<BlueprintData> enumerator = __instance.m_FilteredBlueprints.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (((Object)enumerator.Current).name == lastBlueprintSelected)
				{
					__instance.m_ScrollBehaviour.SetSelectedIndex(num, true, true);
					lastBlueprintSelected = null;
					return;
				}
				num++;
			}
			lastBlueprintSelected = null;
		}
	}

	[HarmonyPatch(typeof(Panel_Crafting), "Enable", new Type[]
	{
		typeof(bool),
		typeof(bool)
	})]
	internal static class Panel_Crafting_Enable
	{
		private static void Postfix(Panel_Crafting __instance, bool enable)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			((UIWidget)__instance.m_FilterLabel).color = new Color(0f, 0f, 0f, 0f);
		}
	}

	[HarmonyPatch(typeof(BlueprintData), "GetDisplayedNameWithCount")]
	internal static class BlueprintData_GetDisplayedNameWithCount
	{
		private static void Postfix(BlueprintData __instance, ref string __result)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			string text = __result;
			if (Main.vanillaDisplay)
			{
				return;
			}
			GearItem craftedResultGear = __instance.m_CraftedResultGear;
			string obj = ((craftedResultGear != null) ? ((Object)craftedResultGear).name : null);
			CraftingLocation requiredCraftingLocation = __instance.m_RequiredCraftingLocation;
			string text2 = obj + ((object)(CraftingLocation)(ref requiredCraftingLocation)).ToString();
			int value = 0;
			int num = 0;
			if ((Object)(object)__instance.m_CraftedResultDecoration != (Object)null)
			{
				string name = ((Object)__instance.m_CraftedResultDecoration).name;
				requiredCraftingLocation = __instance.m_RequiredCraftingLocation;
				string text3 = name + ((object)(CraftingLocation)(ref requiredCraftingLocation)).ToString();
				if (!blueprintDuplicates.ContainsKey(text3))
				{
					return;
				}
				Enumerator<string> enumerator = blueprintDuplicates[text3].GetEnumerator();
				while (enumerator.MoveNext())
				{
					string current = enumerator.Current;
					num++;
					if (((Object)__instance).name == current)
					{
						value = num;
					}
				}
				__result = text + $" [{value}/{num}]";
			}
			else
			{
				if (!((Object)(object)__instance.m_CraftedResultGear != (Object)null) || !blueprintDuplicates.ContainsKey(text2))
				{
					return;
				}
				Enumerator<string> enumerator = blueprintDuplicates[text2].GetEnumerator();
				while (enumerator.MoveNext())
				{
					string current2 = enumerator.Current;
					num++;
					if (((Object)__instance).name == current2)
					{
						value = num;
					}
				}
				__result = text + $" [{value}/{num}]";
			}
		}
	}

	[HarmonyPatch(typeof(BlueprintDisplayItem), "OnItemClick")]
	internal static class BlueprintDisplayItem_OnItemClick
	{
		private static void Postfix(BlueprintDisplayItem __instance)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0247: Unknown result type (might be due to invalid IL or missing references)
			//IL_024c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_030b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0310: Unknown result type (might be due to invalid IL or missing references)
			//IL_0149: Unknown result type (might be due to invalid IL or missing references)
			//IL_014e: Unknown result type (might be due to invalid IL or missing references)
			if (!Input.GetKey(Settings.settings.HoldKey) || !((Object)(object)__instance.m_BlueprintData != (Object)null))
			{
				return;
			}
			string name = ((Object)__instance.m_BlueprintData).name;
			Panel_Crafting panel = InterfaceManager.GetPanel<Panel_Crafting>();
			scrollValue = panel.m_ScrollBehaviour.m_ScrollBar.scrollValue;
			CraftingLocation requiredCraftingLocation;
			if (Main.blueprintsRemoved.Contains(name))
			{
				Main.blueprintsRemoved.Remove(name);
				if ((Object)(object)__instance.m_BlueprintData.m_CraftedResultDecoration != (Object)null)
				{
					string name2 = ((Object)__instance.m_BlueprintData.m_CraftedResultDecoration).name;
					requiredCraftingLocation = __instance.m_BlueprintData.m_RequiredCraftingLocation;
					string text = name2 + ((object)(CraftingLocation)(ref requiredCraftingLocation)).ToString();
					if (blueprintDuplicates.ContainsKey(text))
					{
						if (blueprintDuplicates[text] == null || blueprintDuplicates[text].Count == 0)
						{
							blueprintDuplicates[text] = new List<string>();
							blueprintDuplicates[text].Add(name);
						}
						else
						{
							blueprintDuplicates[text].Add(name);
						}
					}
				}
				else if ((Object)(object)__instance.m_BlueprintData.m_CraftedResultGear != (Object)null)
				{
					string name3 = ((Object)__instance.m_BlueprintData.m_CraftedResultGear).name;
					requiredCraftingLocation = __instance.m_BlueprintData.m_RequiredCraftingLocation;
					string text2 = name3 + ((object)(CraftingLocation)(ref requiredCraftingLocation)).ToString();
					if (blueprintDuplicates.ContainsKey(text2))
					{
						if (blueprintDuplicates[text2] == null || blueprintDuplicates[text2].Count == 0)
						{
							blueprintDuplicates[text2] = new List<string>();
							blueprintDuplicates[text2].Add(name);
							activeBlueprint[text2] = blueprintDuplicates[text2][0];
						}
						else
						{
							blueprintDuplicates[text2].Add(name);
							activeBlueprint[text2] = blueprintDuplicates[text2][0];
						}
					}
				}
			}
			else
			{
				Main.blueprintsRemoved.Add(name);
				if ((Object)(object)__instance.m_BlueprintData.m_CraftedResultDecoration != (Object)null)
				{
					string name4 = ((Object)__instance.m_BlueprintData.m_CraftedResultDecoration).name;
					requiredCraftingLocation = __instance.m_BlueprintData.m_RequiredCraftingLocation;
					string text3 = name4 + ((object)(CraftingLocation)(ref requiredCraftingLocation)).ToString();
					if (blueprintDuplicates.ContainsKey(text3))
					{
						blueprintDuplicates[text3].Remove(name);
						if (blueprintDuplicates[text3] == null || blueprintDuplicates[text3].Count == 0)
						{
							activeBlueprint[text3] = "";
						}
						else
						{
							activeBlueprint[text3] = blueprintDuplicates[text3][0];
						}
					}
				}
				else if ((Object)(object)__instance.m_BlueprintData.m_CraftedResultGear != (Object)null)
				{
					string name5 = ((Object)__instance.m_BlueprintData.m_CraftedResultGear).name;
					requiredCraftingLocation = __instance.m_BlueprintData.m_RequiredCraftingLocation;
					string text4 = name5 + ((object)(CraftingLocation)(ref requiredCraftingLocation)).ToString();
					if (blueprintDuplicates.ContainsKey(text4))
					{
						blueprintDuplicates[text4].Remove(name);
						if (blueprintDuplicates[text4] == null || blueprintDuplicates[text4].Count == 0)
						{
							activeBlueprint[text4] = "";
						}
						else
						{
							activeBlueprint[text4] = blueprintDuplicates[text4][0];
						}
					}
				}
			}
			panel.OnCategoryChanged(panel.m_CategoryNavigation.m_CurrentIndex);
			Main.SaveListToJson(Main.blueprintsRemoved);
		}
	}

	[HarmonyPatch(typeof(RecipeBook), "OnRecipesLoaded")]
	internal static class RecipeBook_OnRecipesLoaded
	{
		private static void Postfix(RecipeBook __instance)
		{
			if (Main.vanillaDisplay)
			{
				return;
			}
			recipeDuplicates.Clear();
			activeRecipe.Clear();
			Enumerator<RecipeData> enumerator = __instance.AllRecipes.GetEnumerator();
			while (enumerator.MoveNext())
			{
				RecipeData current = enumerator.Current;
				string name = ((Object)current.m_DishBlueprint.m_CraftedResultGear).name;
				if (name != null)
				{
					if (recipeDuplicates.ContainsKey(name))
					{
						recipeDuplicates[name].Add(((Object)current).name);
						continue;
					}
					recipeDuplicates[name] = new List<string>();
					recipeDuplicates[name].Add(((Object)current).name);
				}
			}
			List<string> val = new List<string>();
			Enumerator<string, List<string>> enumerator2 = recipeDuplicates.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				KeyValuePair<string, List<string>> current2 = enumerator2.Current;
				if (current2.Value.Count == 1)
				{
					val.Add(current2.Key);
				}
			}
			Enumerator<string> enumerator3 = val.GetEnumerator();
			while (enumerator3.MoveNext())
			{
				string current3 = enumerator3.Current;
				recipeDuplicates.Remove(current3);
			}
			List<string> val2 = new List<string>();
			foreach (string item in Main.blueprintsRemoved)
			{
				enumerator2 = recipeDuplicates.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					KeyValuePair<string, List<string>> current5 = enumerator2.Current;
					if (current5.Value != null && current5.Value.Contains(item))
					{
						current5.Value.Remove(item);
						if (current5.Value == null || current5.Value.Count == 0)
						{
							val2.Add(current5.Key);
						}
					}
				}
			}
			enumerator3 = val2.GetEnumerator();
			while (enumerator3.MoveNext())
			{
				string current6 = enumerator3.Current;
				recipeDuplicates[current6] = new List<string>();
			}
			enumerator2 = recipeDuplicates.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				KeyValuePair<string, List<string>> current7 = enumerator2.Current;
				activeRecipe[current7.Key] = ((current7.Value != null && current7.Value.Count > 0) ? current7.Value[0] : "");
			}
		}
	}

	[HarmonyPatch(typeof(CookingToolPanelFilterButton), "IsGearItemInFilter")]
	internal static class CookingToolPanelFilterButton_IsGearItemInFilter
	{
		private static void Postfix(CookingToolPanelFilterButton __instance, ref bool __result)
		{
			if (!__result || Main.vanillaDisplay)
			{
				return;
			}
			cookableItemDuplicates.Clear();
			activeCookableItem.Clear();
			Enumerator<GearItemObject> enumerator = GameManager.GetInventoryComponent().m_Items.GetEnumerator();
			while (enumerator.MoveNext())
			{
				GearItemObject current = enumerator.Current;
				if ((Object)(object)current.m_GearItem != (Object)null && (Object)(object)current.m_GearItem.m_Cookable != (Object)null)
				{
					string name = ((Object)current.m_GearItem).name;
					if (cookableItemDuplicates.ContainsKey(name))
					{
						cookableItemDuplicates[name].Add(((Object)current.m_GearItem).GetInstanceID());
						continue;
					}
					cookableItemDuplicates[name] = new List<int>();
					cookableItemDuplicates[name].Add(((Object)current.m_GearItem).GetInstanceID());
				}
			}
			List<string> val = new List<string>();
			Enumerator<string, List<int>> enumerator2 = cookableItemDuplicates.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				KeyValuePair<string, List<int>> current2 = enumerator2.Current;
				if (current2.Value.Count == 1)
				{
					val.Add(current2.Key);
				}
			}
			Enumerator<string> enumerator3 = val.GetEnumerator();
			while (enumerator3.MoveNext())
			{
				string current3 = enumerator3.Current;
				cookableItemDuplicates.Remove(current3);
			}
			enumerator2 = cookableItemDuplicates.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				KeyValuePair<string, List<int>> current4 = enumerator2.Current;
				activeCookableItem[current4.Key] = current4.Value[0];
			}
		}
	}

	[HarmonyPatch(typeof(CookingToolPanelFilterButton), "PassesFilter")]
	internal static class CookingToolPanelFilterButton_PassesFilter
	{
		private static void Postfix(CookingToolPanelFilterButton __instance, CookableItem cookableItem, ref bool __result)
		{
			if (Main.vanillaDisplay || !__result)
			{
				return;
			}
			string name = ((Object)cookableItem.m_GearItem).name;
			if ((Object)(object)cookableItem.m_Recipe != (Object)null)
			{
				string name2 = ((Object)cookableItem.m_Recipe).name;
				if (recipeDuplicates.ContainsKey(name))
				{
					if (activeRecipe[name] == name2)
					{
						__result = true;
					}
					else
					{
						__result = false;
					}
				}
				else if (Main.blueprintsRemoved.Contains(name2))
				{
					__result = false;
				}
			}
			else if (Main.blueprintsRemoved.Contains(name))
			{
				__result = false;
			}
			else if (cookableItemDuplicates.ContainsKey(name))
			{
				if (activeCookableItem[name] == ((Object)cookableItem.m_GearItem).GetInstanceID())
				{
					__result = true;
				}
				else
				{
					__result = false;
				}
			}
		}
	}

	[HarmonyPatch(typeof(Panel_Cooking), "Update")]
	internal static class Panel_Cooking_Update
	{
		private static void Postfix(Panel_Cooking __instance)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0228: Unknown result type (might be due to invalid IL or missing references)
			if (InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.settings.viewKey))
			{
				Main.vanillaDisplay = !Main.vanillaDisplay;
				__instance.RefreshFoodList();
			}
			if (Main.vanillaDisplay)
			{
				return;
			}
			if (InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.settings.rightKey))
			{
				if ((Object)(object)__instance.GetSelectedCookableItem().m_Recipe != (Object)null)
				{
					string name = ((Object)__instance.GetSelectedCookableItem().m_GearItem).name;
					string name2 = ((Object)__instance.GetSelectedCookableItem().m_Recipe).name;
					int num = 0;
					if (!activeRecipe.ContainsKey(name))
					{
						return;
					}
					int count = recipeDuplicates[name].Count;
					for (int i = 0; i < count; i++)
					{
						if (recipeDuplicates[name][i] == name2)
						{
							num = i;
						}
					}
					if (num == count - 1)
					{
						activeRecipe[name] = recipeDuplicates[name][0];
					}
					else
					{
						activeRecipe[name] = recipeDuplicates[name][num + 1];
					}
					lastRecipeSelected = activeRecipe[name];
					__instance.RefreshFoodList();
				}
				else
				{
					if (!((Object)(object)__instance.GetSelectedCookableItem().m_GearItem != (Object)null))
					{
						return;
					}
					string name3 = ((Object)__instance.GetSelectedCookableItem().m_GearItem).name;
					int instanceID = ((Object)__instance.GetSelectedCookableItem().m_GearItem).GetInstanceID();
					int num2 = 0;
					if (!activeCookableItem.ContainsKey(name3))
					{
						return;
					}
					int count2 = cookableItemDuplicates[name3].Count;
					for (int j = 0; j < count2; j++)
					{
						if (cookableItemDuplicates[name3][j] == instanceID)
						{
							num2 = j;
						}
					}
					if (num2 == count2 - 1)
					{
						activeCookableItem[name3] = cookableItemDuplicates[name3][0];
					}
					else
					{
						activeCookableItem[name3] = cookableItemDuplicates[name3][num2 + 1];
					}
					lastCookableItemSelected = activeCookableItem[name3];
					__instance.RefreshFoodList();
				}
			}
			else
			{
				if (!InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.settings.leftKey))
				{
					return;
				}
				if ((Object)(object)__instance.GetSelectedCookableItem().m_Recipe != (Object)null)
				{
					string name4 = ((Object)__instance.GetSelectedCookableItem().m_GearItem).name;
					string name5 = ((Object)__instance.GetSelectedCookableItem().m_Recipe).name;
					int num3 = 0;
					if (!activeRecipe.ContainsKey(name4))
					{
						return;
					}
					int count3 = recipeDuplicates[name4].Count;
					for (int k = 0; k < count3; k++)
					{
						if (recipeDuplicates[name4][k] == name5)
						{
							num3 = k;
						}
					}
					if (num3 == 0)
					{
						activeRecipe[name4] = recipeDuplicates[name4][count3 - 1];
					}
					else
					{
						activeRecipe[name4] = recipeDuplicates[name4][num3 - 1];
					}
					lastRecipeSelected = activeRecipe[name4];
					__instance.RefreshFoodList();
				}
				else
				{
					if (!((Object)(object)__instance.GetSelectedCookableItem().m_GearItem != (Object)null))
					{
						return;
					}
					string name6 = ((Object)__instance.GetSelectedCookableItem().m_GearItem).name;
					int instanceID2 = ((Object)__instance.GetSelectedCookableItem().m_GearItem).GetInstanceID();
					int num4 = 0;
					if (!activeCookableItem.ContainsKey(name6))
					{
						return;
					}
					int count4 = cookableItemDuplicates[name6].Count;
					for (int l = 0; l < count4; l++)
					{
						if (cookableItemDuplicates[name6][l] == instanceID2)
						{
							num4 = l;
						}
					}
					if (num4 == 0)
					{
						activeCookableItem[name6] = cookableItemDuplicates[name6][count4 - 1];
					}
					else
					{
						activeCookableItem[name6] = cookableItemDuplicates[name6][num4 - 1];
					}
					lastCookableItemSelected = activeCookableItem[name6];
					__instance.RefreshFoodList();
				}
			}
		}
	}

	[HarmonyPatch(typeof(Panel_Cooking), "RefreshFoodList")]
	internal static class Panel_Cooking_RefreshFoodList
	{
		private static void Postfix(Panel_Cooking __instance)
		{
			if (scrollValue.HasValue)
			{
				__instance.m_ScrollBehaviour.m_ScrollBar.scrollValue = scrollValue.Value;
				scrollValue = null;
			}
			if (lastRecipeSelected != null)
			{
				int num = 0;
				Enumerator<CookableItem> enumerator = __instance.m_FoodList.GetEnumerator();
				while (enumerator.MoveNext())
				{
					CookableItem current = enumerator.Current;
					if ((Object)(object)current.m_Recipe != (Object)null && ((Object)current.m_Recipe).name == lastRecipeSelected)
					{
						__instance.m_ScrollBehaviour.SetSelectedIndex(num, true, true);
						lastRecipeSelected = null;
						return;
					}
					num++;
				}
				lastRecipeSelected = null;
			}
			else
			{
				if (!lastCookableItemSelected.HasValue)
				{
					return;
				}
				int num2 = 0;
				Enumerator<CookableItem> enumerator = __instance.m_FoodList.GetEnumerator();
				while (enumerator.MoveNext())
				{
					if (((Object)enumerator.Current.m_GearItem).GetInstanceID() == lastCookableItemSelected)
					{
						__instance.m_ScrollBehaviour.SetSelectedIndex(num2, true, true);
						lastCookableItemSelected = null;
						return;
					}
					num2++;
				}
				lastCookableItemSelected = null;
			}
		}
	}

	[HarmonyPatch(typeof(CookableItem), "GetDisplayName")]
	internal static class CookableItem_GetDisplayName
	{
		private static void Postfix(CookableItem __instance, ref string __result)
		{
			if (Main.vanillaDisplay)
			{
				return;
			}
			string name = ((Object)__instance.m_GearItem).name;
			int value = 0;
			int num = 0;
			if ((Object)(object)__instance.m_Recipe != (Object)null)
			{
				if (!recipeDuplicates.ContainsKey(name))
				{
					return;
				}
				Enumerator<string> enumerator = recipeDuplicates[name].GetEnumerator();
				while (enumerator.MoveNext())
				{
					string current = enumerator.Current;
					num++;
					if (((Object)__instance.m_Recipe).name == current)
					{
						value = num;
					}
				}
				string text = __result;
				__result = text + $" [{value}/{num}]";
			}
			else
			{
				if (!cookableItemDuplicates.ContainsKey(name))
				{
					return;
				}
				Enumerator<int> enumerator2 = cookableItemDuplicates[name].GetEnumerator();
				while (enumerator2.MoveNext())
				{
					int current2 = enumerator2.Current;
					num++;
					if (((Object)__instance.m_GearItem).GetInstanceID() == current2)
					{
						value = num;
					}
				}
				string text2 = __result;
				__result = text2 + $" [{value}/{num}]";
			}
		}
	}

	[HarmonyPatch(typeof(ScrollBehaviourItem), "OnClick")]
	internal static class ScrollBehaviourItem_OnItemClick
	{
		private static void Postfix(ScrollBehaviourItem __instance)
		{
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			Panel_Cooking val = default(Panel_Cooking);
			CookableListItem val2 = default(CookableListItem);
			if (GameManager.m_ActiveScene == "Boot" || GameManager.IsEmptySceneActive() || GameManager.IsMainMenuActive() || (Object)(object)__instance == (Object)null || (Object)(object)((Component)__instance).gameObject == (Object)null || !Input.GetKey(Settings.settings.HoldKey) || !InterfaceManager.TryGetPanel<Panel_Cooking>(ref val) || !((Component)__instance).gameObject.TryGetComponent<CookableListItem>(ref val2) || val2.m_Cookable == null)
			{
				return;
			}
			scrollValue = val.m_ScrollBehaviour.m_ScrollBar.scrollValue;
			string text = (((Object)(object)val2.m_Cookable.m_Recipe != (Object)null) ? ((Object)val2.m_Cookable.m_Recipe).name : ((Object)val2.m_Cookable.m_GearItem).name);
			if (Main.blueprintsRemoved.Contains(text))
			{
				Main.blueprintsRemoved.Remove(text);
				if ((Object)(object)val2.m_Cookable.m_Recipe != (Object)null)
				{
					string name = ((Object)val2.m_Cookable.m_GearItem).name;
					if (recipeDuplicates.ContainsKey(name))
					{
						if (recipeDuplicates[name] == null || recipeDuplicates[name].Count == 0)
						{
							recipeDuplicates[name] = new List<string>();
							recipeDuplicates[name].Add(text);
						}
						else
						{
							recipeDuplicates[name].Add(text);
						}
					}
				}
				else if ((Object)(object)val2.m_Cookable.m_GearItem != (Object)null)
				{
					_ = ((Object)val2.m_Cookable.m_GearItem).name;
				}
			}
			else
			{
				Main.blueprintsRemoved.Add(text);
				if ((Object)(object)val2.m_Cookable.m_Recipe != (Object)null)
				{
					string name2 = ((Object)val2.m_Cookable.m_GearItem).name;
					if (recipeDuplicates.ContainsKey(name2))
					{
						recipeDuplicates[name2].Remove(text);
						if (recipeDuplicates[name2] == null || recipeDuplicates[name2].Count == 0)
						{
							activeRecipe[name2] = "";
						}
						else
						{
							activeRecipe[name2] = recipeDuplicates[name2][0];
						}
					}
				}
				else if ((Object)(object)val2.m_Cookable.m_GearItem != (Object)null)
				{
					_ = ((Object)val2.m_Cookable.m_GearItem).name;
				}
			}
			val.RefreshFoodList();
			Main.SaveListToJson(Main.blueprintsRemoved);
		}
	}

	[HarmonyPatch(typeof(ScrollBehaviour), "RefreshItems")]
	internal static class ScrollBehaviour_RefreshItems
	{
		private static void Postfix(ScrollBehaviour __instance)
		{
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_021d: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			//IL_0164: Unknown result type (might be due to invalid IL or missing references)
			//IL_0188: Unknown result type (might be due to invalid IL or missing references)
			//IL_04a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_0514: Unknown result type (might be due to invalid IL or missing references)
			//IL_040c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0431: Unknown result type (might be due to invalid IL or missing references)
			//IL_0456: Unknown result type (might be due to invalid IL or missing references)
			//IL_047b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0353: Unknown result type (might be due to invalid IL or missing references)
			//IL_0378: Unknown result type (might be due to invalid IL or missing references)
			//IL_039d: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_02df: Unknown result type (might be due to invalid IL or missing references)
			//IL_0304: Unknown result type (might be due to invalid IL or missing references)
			//IL_0329: Unknown result type (might be due to invalid IL or missing references)
			if (__instance.m_VisibleItems == null)
			{
				return;
			}
			BlueprintDisplayItem val = default(BlueprintDisplayItem);
			CookableListItem val3 = default(CookableListItem);
			for (int i = 0; i < ((Il2CppArrayBase<ScrollBehaviourItem>)(object)__instance.m_VisibleItems).Count; i++)
			{
				if (((Component)((Il2CppArrayBase<ScrollBehaviourItem>)(object)__instance.m_VisibleItems)[i]).TryGetComponent<BlueprintDisplayItem>(ref val))
				{
					BlueprintDisplayItem val2 = val;
					((UIButtonColor)val2.m_Button).SetState((State)0, true);
					if (val2.m_DisplayName.mText == "")
					{
						((UIButtonColor)val2.m_Button).mDefaultColor = new Color(0.235f, 0.235f, 0.235f, 0.274f);
						((UIButtonColor)val2.m_Button).defaultColor = new Color(0.235f, 0.235f, 0.235f, 0.274f);
						((UIButtonColor)val2.m_Button).mStartingColor = new Color(0.235f, 0.235f, 0.235f, 0.274f);
						((UIButtonColor)val2.m_Button).hover = new Color(0.431f, 0.513f, 0.486f, 0.078f);
					}
					else if (Main.blueprintsRemoved.Contains(((Object)val2.m_BlueprintData).name))
					{
						((UIButtonColor)val2.m_Button).mDefaultColor = new Color(1f, 0.5f, 0.5f, 0.2f);
						((UIButtonColor)val2.m_Button).defaultColor = new Color(1f, 0.5f, 0.5f, 0.2f);
						((UIButtonColor)val2.m_Button).mStartingColor = new Color(1f, 0.5f, 0.5f, 0.2f);
						((UIButtonColor)val2.m_Button).hover = new Color(1f, 0.5f, 0.5f, 0.2f);
					}
					else
					{
						((UIButtonColor)val2.m_Button).mDefaultColor = new Color(0.235f, 0.235f, 0.235f, 0.274f);
						((UIButtonColor)val2.m_Button).defaultColor = new Color(0.235f, 0.235f, 0.235f, 0.274f);
						((UIButtonColor)val2.m_Button).mStartingColor = new Color(0.235f, 0.235f, 0.235f, 0.274f);
						((UIButtonColor)val2.m_Button).hover = new Color(0.431f, 0.513f, 0.486f, 0.078f);
					}
					continue;
				}
				if (!((Component)((Il2CppArrayBase<ScrollBehaviourItem>)(object)__instance.m_VisibleItems)[i]).TryGetComponent<CookableListItem>(ref val3))
				{
					break;
				}
				CookableListItem val4 = val3;
				((UIButtonColor)val4.m_Button).SetState((State)0, true);
				if ((Object)(object)val4.m_Cookable.m_Recipe == (Object)null)
				{
					if ((Object)(object)val4.m_Cookable.m_GearItem == (Object)null)
					{
						break;
					}
					if (Main.blueprintsRemoved.Contains(((Object)val4.m_Cookable.m_GearItem).name))
					{
						((UIButtonColor)val4.m_Button).mDefaultColor = new Color(1f, 0.5f, 0.5f, 0.2f);
						((UIButtonColor)val4.m_Button).defaultColor = new Color(1f, 0.5f, 0.5f, 0.2f);
						((UIButtonColor)val4.m_Button).mStartingColor = new Color(1f, 0.5f, 0.5f, 0.2f);
						((UIButtonColor)val4.m_Button).hover = new Color(1f, 0.5f, 0.5f, 0.2f);
					}
					else
					{
						((UIButtonColor)val4.m_Button).mDefaultColor = new Color(0.235f, 0.235f, 0.235f, 0.274f);
						((UIButtonColor)val4.m_Button).defaultColor = new Color(0.235f, 0.235f, 0.235f, 0.274f);
						((UIButtonColor)val4.m_Button).mStartingColor = new Color(0.235f, 0.235f, 0.235f, 0.274f);
						((UIButtonColor)val4.m_Button).hover = new Color(0.431f, 0.513f, 0.486f, 0.078f);
					}
				}
				else if (Main.blueprintsRemoved.Contains(((Object)val4.m_Cookable.m_Recipe).name))
				{
					((UIButtonColor)val4.m_Button).mDefaultColor = new Color(1f, 0.5f, 0.5f, 0.2f);
					((UIButtonColor)val4.m_Button).defaultColor = new Color(1f, 0.5f, 0.5f, 0.2f);
					((UIButtonColor)val4.m_Button).mStartingColor = new Color(1f, 0.5f, 0.5f, 0.2f);
					((UIButtonColor)val4.m_Button).hover = new Color(1f, 0.5f, 0.5f, 0.2f);
				}
				else
				{
					((UIButtonColor)val4.m_Button).mDefaultColor = new Color(0.235f, 0.235f, 0.235f, 0.274f);
					((UIButtonColor)val4.m_Button).defaultColor = new Color(0.235f, 0.235f, 0.235f, 0.274f);
					((UIButtonColor)val4.m_Button).mStartingColor = new Color(0.235f, 0.235f, 0.235f, 0.274f);
					((UIButtonColor)val4.m_Button).hover = new Color(0.431f, 0.513f, 0.486f, 0.078f);
				}
			}
		}
	}

	public static bool alreadyRefreshed = false;

	public static float? scrollValue;

	public static int? firstLine;

	public static int? lastIndex;

	public static string lastBlueprintSelected;

	public static string lastRecipeSelected;

	public static int? lastCookableItemSelected;

	public static Dictionary<string, List<string>> blueprintDuplicates = new Dictionary<string, List<string>>();

	public static Dictionary<string, string> activeBlueprint = new Dictionary<string, string>();

	public static Dictionary<string, List<string>> recipeDuplicates = new Dictionary<string, List<string>>();

	public static Dictionary<string, string> activeRecipe = new Dictionary<string, string>();

	public static Dictionary<string, List<int>> cookableItemDuplicates = new Dictionary<string, List<int>>();

	public static Dictionary<string, int> activeCookableItem = new Dictionary<string, int>();
}
