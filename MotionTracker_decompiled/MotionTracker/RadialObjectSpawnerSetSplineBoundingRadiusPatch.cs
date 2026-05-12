using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace MotionTracker;

[HarmonyPatch(typeof(RadialObjectSpawner), "SetSplineBoundingRadius")]
public class RadialObjectSpawnerSetSplineBoundingRadiusPatch
{
	public static void Postfix(ref RadialObjectSpawner __instance)
	{
		MyLogger.LogMessage("RadialObjectSpawner SetSplineBoundingRadius event: (" + ((Object)__instance).name + ":" + ((Object)__instance).GetInstanceID() + ").", 511, "Postfix", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Harmony.cs");
	}
}
