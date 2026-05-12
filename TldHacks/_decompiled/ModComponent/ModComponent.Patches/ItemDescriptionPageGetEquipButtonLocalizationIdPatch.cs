using HarmonyLib;
using Il2Cpp;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Patches;

[HarmonyPatch(typeof(ItemDescriptionPage), "GetEquipButtonLocalizationId")]
internal static class ItemDescriptionPageGetEquipButtonLocalizationIdPatch
{
	public static void Postfix(GearItem gi, ref string __result)
	{
		if (string.IsNullOrEmpty(__result))
		{
			ModBaseComponent modComponent = ((Component?)(object)gi).GetModComponent();
			if ((Object)(object)modComponent != (Object)null)
			{
				__result = modComponent.InventoryActionLocalizationId;
			}
		}
	}
}
