using System;
using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.IntBackedUnit;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Patches;

[HarmonyPatch(typeof(PlayerManager), "UseInventoryItem", new Type[]
{
	typeof(GearItem),
	typeof(ItemLiquidVolume),
	typeof(bool)
})]
internal static class PlayerManagerUseInventoryItemPatch
{
	internal static bool Prefix(PlayerManager __instance, GearItem gi)
	{
		if ((Object)(object)((Component?)(object)gi).GetComponentSafe<FirstPersonItem>() != (Object)null)
		{
			return true;
		}
		if ((Object)(object)((Component?)(object)gi).GetEquippableModComponent() == (Object)null)
		{
			return true;
		}
		GearItem itemInHands = __instance.m_ItemInHands;
		if ((Object)(object)itemInHands != (Object)null)
		{
			__instance.UnequipItemInHands();
		}
		if ((Object)(object)gi != (Object)(object)itemInHands)
		{
			__instance.EquipItem(gi, false);
		}
		return false;
	}
}
