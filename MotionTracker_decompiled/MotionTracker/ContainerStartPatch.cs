using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace MotionTracker;

[HarmonyPatch(typeof(Container), "Start")]
public class ContainerStartPatch
{
	public static void Postfix(ref Container __instance)
	{
		((Object)__instance).name.Contains("CONTAINER_InaccessibleGear");
	}
}
