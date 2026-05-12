using System;
using System.Collections.Generic;
using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.Cooking;
using UnityEngine;

namespace CraftingRevisions;

[HarmonyPatch]
public static class RecipeManager
{
	private static HashSet<string> jsonUserRecipes = new HashSet<string>();

	private static List<RecipeData> userRecipes = new List<RecipeData>();

	public static void AddRecipeFromJson(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			throw new ArgumentException("Recipe text contains no information", "text");
		}
		jsonUserRecipes.Add(text);
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(RecipeBook), "Start")]
	private static void RecipeBook_Start(RecipeBook __instance)
	{
		foreach (string jsonUserRecipe in jsonUserRecipes)
		{
			ModUserRecipe modUserRecipe = ModUserRecipe.ParseFromJson(jsonUserRecipe);
			try
			{
				if (modUserRecipe.Validate())
				{
					RecipeData recipeData = modUserRecipe.GetRecipeData();
					__instance.AllRecipes.Add(recipeData);
					Logger.Log("Added Recipe " + modUserRecipe.RecipeName);
				}
			}
			catch (Exception ex)
			{
				Logger.LogError("Recipe Exception" + modUserRecipe.RecipeName + "\n" + ex);
			}
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(CookableListItem), "SetCookable")]
	private static void CookableListItem_SetCookable(CookableListItem __instance, CookableItem cookableItem, CookingPotItem cookingPot)
	{
		if ((Object)(object)((Component)cookableItem.m_GearItem).GetComponent<Cookable>().m_CookedPrefab != (Object)null)
		{
			((UIWidget)__instance.m_ItemIcon).mainTexture = (Texture)(object)((Component)cookableItem.m_GearItem).GetComponent<Cookable>().m_CookedPrefab.GetInventoryIconTexture();
			((Behaviour)((Component)__instance).gameObject.GetComponentInChildren<UITexture>()).enabled = true;
		}
	}
}
