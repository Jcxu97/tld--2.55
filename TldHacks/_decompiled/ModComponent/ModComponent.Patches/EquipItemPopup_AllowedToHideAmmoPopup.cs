using HarmonyLib;
using Il2Cpp;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Patches;

[HarmonyPatch(typeof(EquipItemPopup), "AllowedToHideAmmoPopup")]
internal static class EquipItemPopup_AllowedToHideAmmoPopup
{
	internal static void Postfix(ref bool __result)
	{
		if (!__result)
		{
			__result = (Object)(object)((Component?)(object)GameManager.GetPlayerManagerComponent().m_ItemInHands).GetModComponent() != (Object)null;
		}
	}
}
