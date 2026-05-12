using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace MotionTracker;

[HarmonyPatch(typeof(Harvestable), "Start")]
public class SaltStartPatch
{
	public static void Postfix(ref Harvestable __instance)
	{
		if (((Object)__instance).name.Contains("SaltDeposit") && !__instance.IsHarvested() && !Object.op_Implicit((Object)(object)((Component)__instance).gameObject.GetComponent<PingComponent>()))
		{
			((Component)__instance).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.SaltDeposit);
		}
	}
}
