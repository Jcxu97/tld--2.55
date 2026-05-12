using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace MotionTracker;

[HarmonyPatch(typeof(FlockController), "destroyBirds")]
public class FlockController_destroyBirds_Patch
{
	public static void Postfix(ref FlockController __instance)
	{
		PingComponent.ManualDelete(((Component)__instance).gameObject.GetComponent<PingComponent>());
	}
}
