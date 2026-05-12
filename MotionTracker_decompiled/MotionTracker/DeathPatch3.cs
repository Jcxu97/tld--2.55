using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace MotionTracker;

[HarmonyPatch(typeof(BaseAi), "Despawn")]
public class DeathPatch3
{
	public static void Postfix(ref BaseAi __instance)
	{
		PingComponent.ManualDelete(((Component)__instance).gameObject.GetComponent<PingComponent>());
	}
}
