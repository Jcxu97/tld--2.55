using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace MotionTracker;

[HarmonyPatch(typeof(Container), "OnDestroy")]
public class ContainerOnDestroyPatch
{
	public static void Postfix(ref Container __instance)
	{
		if (((Object)__instance).name.Contains("CONTAINER_InaccessibleGear") && Object.op_Implicit((Object)(object)((Component)__instance).gameObject.GetComponent<PingComponent>()))
		{
			PingComponent.ManualDelete(((Component)__instance).gameObject.GetComponent<PingComponent>());
		}
	}
}
