using HarmonyLib;
using Il2Cpp;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Patches;

[HarmonyPatch(typeof(PlayerManager), "ItemCanEquipInHands")]
internal static class PlayerManager_ItemCanEquipInHands
{
	private static void Postfix(GearItem gi, ref bool __result)
	{
		if (!__result && !((Object)(object)gi == (Object)null))
		{
			ModBaseEquippableComponent equippableModComponent = ((Component?)(object)gi).GetEquippableModComponent();
			if ((Object)(object)equippableModComponent != (Object)null && !string.IsNullOrEmpty(equippableModComponent.ImplementationType))
			{
				__result = true;
			}
		}
	}
}
