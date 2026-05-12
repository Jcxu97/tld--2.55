using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace MotionTracker;

[HarmonyPatch(typeof(Container), "OnEnable")]
public class ContainerOnEnablePatch
{
	public static void Postfix(ref Container __instance)
	{
		((Object)__instance).name.Contains("CONTAINER_InaccessibleGear");
	}
}
