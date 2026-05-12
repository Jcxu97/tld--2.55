using HarmonyLib;
using Il2Cpp;
using ModComponent.API.Behaviours;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Patches;

[HarmonyPatch(typeof(FireManager), "PlayerStartFire")]
internal static class FireManager_PlayerStartFire
{
	internal static void Postfix(FireStarterItem starter)
	{
		ModFireStarterBehaviour componentSafe = ((Component?)(object)starter).GetComponentSafe<ModFireStarterBehaviour>();
		if (!((Object)(object)componentSafe == (Object)null) && componentSafe.RuinedAfterUse)
		{
			GearItem component = ((Component)starter).GetComponent<GearItem>();
			if ((Object)(object)component != (Object)null)
			{
				component.BreakOnUse();
			}
		}
	}
}
