using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace MotionTracker;

[HarmonyPatch(typeof(Container), "UpdateContainer")]
public class ContainerUpdateContainerPatch
{
	public static void Postfix(ref Container __instance)
	{
		if (((Object)__instance).name.Contains("CONTAINER_InaccessibleGear") && !Object.op_Implicit((Object)(object)((Component)__instance).gameObject.GetComponent<PingComponent>()))
		{
			((Component)__instance).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.LostAndFoundBox);
		}
	}
}
