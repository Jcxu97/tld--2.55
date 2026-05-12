using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace MotionTracker;

[HarmonyPatch(typeof(GearItem), "OnDestroy")]
public class GearItemDestroyPatch
{
	public static void Postfix(ref GearItem __instance)
	{
		if (!((Object)((Component)__instance).gameObject).name.Contains("Arrow") && !((Object)((Component)__instance).gameObject).name.Contains("Coal"))
		{
			PingComponent.IsRawFish(__instance);
		}
		if (Object.op_Implicit((Object)(object)((Component)__instance).gameObject.GetComponent<PingComponent>()))
		{
			if (!((Object)((Component)__instance).gameObject).name.Contains("Arrow") && !((Object)((Component)__instance).gameObject).name.Contains("Coal"))
			{
				PingComponent.IsRawFish(__instance);
			}
			PingComponent.ManualDelete(((Component)__instance).gameObject.GetComponent<PingComponent>());
		}
		else if (!((Object)((Component)__instance).gameObject).name.Contains("Arrow") && !((Object)((Component)__instance).gameObject).name.Contains("Coal"))
		{
			PingComponent.IsRawFish(__instance);
		}
	}
}
