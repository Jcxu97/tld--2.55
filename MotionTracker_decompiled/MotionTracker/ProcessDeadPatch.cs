using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace MotionTracker;

[HarmonyPatch(typeof(BaseAi), "ProcessDead")]
public class ProcessDeadPatch
{
	public static void Postfix(ref BaseAi __instance)
	{
		PingComponent.ManualDelete(((Component)__instance).gameObject.GetComponent<PingComponent>());
	}
}
