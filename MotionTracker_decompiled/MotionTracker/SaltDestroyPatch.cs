using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace MotionTracker;

[HarmonyPatch(typeof(Harvestable), "OnDestroy")]
public class SaltDestroyPatch
{
	public static void Postfix(ref Harvestable __instance)
	{
		((Object)__instance).name.Contains("SaltDeposit");
	}
}
