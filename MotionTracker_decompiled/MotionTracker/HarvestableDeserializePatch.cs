using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace MotionTracker;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
public class HarvestableDeserializePatch
{
	public static void Postfix(ref Harvestable __instance, string text)
	{
		if (!((Object)__instance).name.Contains("SaltDeposit"))
		{
			return;
		}
		if (__instance.IsHarvested())
		{
			if (Object.op_Implicit((Object)(object)((Component)__instance).gameObject.GetComponent<PingComponent>()))
			{
				PingComponent.ManualDelete(((Component)__instance).gameObject.GetComponent<PingComponent>());
			}
		}
		else if (!Object.op_Implicit((Object)(object)((Component)__instance).gameObject.GetComponent<PingComponent>()))
		{
			((Component)__instance).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.SaltDeposit);
		}
	}
}
