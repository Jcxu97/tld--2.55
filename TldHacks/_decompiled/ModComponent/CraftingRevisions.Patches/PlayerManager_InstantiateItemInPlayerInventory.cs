using System;
using HarmonyLib;
using Il2Cpp;

namespace CraftingRevisions.Patches;

[HarmonyPatch(typeof(PlayerManager), "InstantiateItemInPlayerInventory", new Type[]
{
	typeof(GearItem),
	typeof(int),
	typeof(float),
	typeof(InventoryInstantiateFlags)
})]
internal static class PlayerManager_InstantiateItemInPlayerInventory
{
	private static void Postfix(ref GearItem __result, float normalizedCondition)
	{
		if (WatchHandleCraftingSuccess.isExecuting && normalizedCondition < 0f)
		{
			__result.CurrentHP = __result.GearItemData.m_MaxHP;
		}
	}
}
