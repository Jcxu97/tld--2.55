using HarmonyLib;
using Il2Cpp;

namespace MotionTracker;

[HarmonyPatch(typeof(RadialObjectSpawner), "Start")]
public class RadialObjectSpawnerStartPatch
{
	public static void Postfix(ref RadialObjectSpawner __instance)
	{
	}
}
