using HarmonyLib;
using Il2Cpp;
using ModComponent.Mapper;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Patches;

[HarmonyPatch(typeof(PlayerManager), "UnequipItemInHandsInternal")]
internal static class PlayerManager_UnequipItemInHandsInternalPatch
{
	internal static void Postfix(PlayerManager __instance)
	{
		GearEquipper.Unequip(((Component?)(object)__instance.m_ItemInHands).GetEquippableModComponent());
	}
}
