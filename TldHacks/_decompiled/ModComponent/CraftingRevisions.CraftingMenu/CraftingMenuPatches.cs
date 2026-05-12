using System;
using HarmonyLib;
using Il2Cpp;
using Il2CppSystem.Collections.Generic;
using Il2CppTLD.Gear;
using MelonLoader;
using UnityEngine;

namespace CraftingRevisions.CraftingMenu;

internal static class CraftingMenuPatches
{
	[HarmonyPatch(typeof(Panel_Crafting), "OnCategoryChanged")]
	internal static class Panel_Crafting_OnCategoryChanged
	{
		private static bool Prefix(Panel_Crafting __instance, int index)
		{
			if (index < 0)
			{
				return false;
			}
			__instance.m_CurrentCategory = (Category)index;
			__instance.ApplyFilter();
			return false;
		}
	}

	[HarmonyPatch(typeof(Panel_Crafting), "ItemPassesFilter")]
	internal static class Panel_Crafting_ItemPassesFilter
	{
		private static bool Prefix(Panel_Crafting __instance, BlueprintData bpi, ref bool __result, ref bool __runOriginal)
		{
			if (!__runOriginal)
			{
				MelonLogger.Error("Another mod tried to disable Panel_Crafting.ItemPassesFilter");
			}
			__result = MethodReplacements.ItemPassesFilter(__instance, bpi);
			return false;
		}
	}

	[HarmonyPatch(typeof(Panel_Crafting), "Initialize")]
	internal static class Panel_Crafting_Start
	{
		private static void Postfix(Panel_Crafting __instance)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Expected O, but got Unknown
			CategoryButtonNavigation categoryNavigation = __instance.m_CategoryNavigation;
			List<UIButton> navigationButtons = categoryNavigation.m_NavigationButtons;
			UIButton original = new UIButton();
			Enumerator<UIButton> enumerator = navigationButtons.GetEnumerator();
			while (enumerator.MoveNext())
			{
				UIButton current = enumerator.Current;
				if (((Object)current).name.ToLower().Contains("deco"))
				{
					original = current;
				}
			}
			UIButton materialButton = original.Instantiate();
			UIButton foodButton = original.Instantiate();
			((Object)foodButton).name = "Button_Food";
			((Object)materialButton).name = "Button_Material";
			EventDelegate.Set(materialButton.onClick, Callback.op_Implicit((Action)delegate
			{
				categoryNavigation.OnNavigationChanged(materialButton);
			}));
			EventDelegate.Set(foodButton.onClick, Callback.op_Implicit((Action)delegate
			{
				categoryNavigation.OnNavigationChanged(foodButton);
			}));
			foodButton.Move(0f, -62f, 0f);
			materialButton.Move(0f, -124f, 0f);
			navigationButtons.Add(materialButton);
			navigationButtons.Add(foodButton);
			materialButton.SetSpriteName("ico_crafting");
			foodButton.SetSpriteName("ico_Radial_food");
			navigationButtons[0].SetSpriteName("ico_Radial_pack");
		}
	}
}
