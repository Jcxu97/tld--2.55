using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace MotionTracker;

[HarmonyPatch(typeof(FlockChild), "Start")]
public class FlockPatch
{
	public static void Postfix(ref FlockChild __instance)
	{
		((Component)__instance).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.Crow);
	}
}
