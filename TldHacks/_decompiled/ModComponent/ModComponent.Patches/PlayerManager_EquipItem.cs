using HarmonyLib;
using Il2Cpp;
using ModComponent.Mapper;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Patches;

[HarmonyPatch(typeof(PlayerManager), "EquipItem")]
internal static class PlayerManager_EquipItem
{
	internal static void Prefix(PlayerManager __instance, GearItem gi)
	{
		if ((Object)(object)((Component?)(object)__instance.m_ItemInHands).GetEquippableModComponent() != (Object)null)
		{
			__instance.UnequipItemInHands();
		}
	}

	internal static void Postfix(PlayerManager __instance)
	{
		GearEquipper.Equip(((Component?)(object)__instance.m_ItemInHands).GetEquippableModComponent());
	}
}
