using HarmonyLib;
using Il2Cpp;
using ModComponent.Mapper;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Patches;

[HarmonyPatch(typeof(PlayerManager), "UnequipItemInHandsSkipAnimation")]
internal static class PlayerManager_UnequipItemInHandsSkipAnimation
{
	internal static void Prefix(PlayerManager __instance)
	{
		GearEquipper.OnUnequipped(((Component?)(object)__instance.m_ItemInHands).GetEquippableModComponent());
	}
}
