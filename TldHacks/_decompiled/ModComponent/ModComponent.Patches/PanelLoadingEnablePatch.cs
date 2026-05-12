using HarmonyLib;
using Il2Cpp;
using ModComponent.Mapper;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Patches;

[HarmonyPatch(typeof(Panel_Loading), "Enable")]
internal static class PanelLoadingEnablePatch
{
	public static void Prefix(bool enable)
	{
		if (enable)
		{
			GearEquipper.Unequip(((Component?)(object)GameManager.GetPlayerManagerComponent().m_ItemInHands).GetEquippableModComponent());
		}
	}
}
