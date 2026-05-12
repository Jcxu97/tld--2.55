using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace MotionTracker;

[HarmonyPatch(typeof(Harvestable), "Harvest")]
public class SaltHarvestPatch
{
	public static void Postfix(ref Harvestable __instance)
	{
		if (((Object)__instance).name.Contains("SaltDeposit") && Object.op_Implicit((Object)(object)((Component)__instance).gameObject.GetComponent<PingComponent>()))
		{
			PingComponent.ManualDelete(((Component)__instance).gameObject.GetComponent<PingComponent>());
		}
	}
}
