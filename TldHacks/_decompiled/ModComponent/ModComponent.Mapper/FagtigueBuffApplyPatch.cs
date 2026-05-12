using HarmonyLib;
using Il2Cpp;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Mapper;

[HarmonyPatch(typeof(FatigueBuff), "Apply")]
internal static class FagtigueBuffApplyPatch
{
	public static void Postfix(FatigueBuff __instance)
	{
		GearItem componentSafe = ((Component?)(object)__instance).GetComponentSafe<GearItem>();
		if (!((Object)(object)componentSafe == (Object)null))
		{
			BuffCauseTracker.SetCause((AfflictionType)10, componentSafe.DisplayName);
		}
	}
}
