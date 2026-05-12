using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace MotionTracker;

[HarmonyPatch(typeof(Container), "Awake")]
public class ContainerAwakePatch
{
	public static void Postfix(ref Container __instance)
	{
		((Object)__instance).name.Contains("CONTAINER_InaccessibleGear");
	}
}
