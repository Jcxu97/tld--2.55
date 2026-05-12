using HarmonyLib;
using Il2Cpp;

namespace MotionTracker;

[HarmonyPatch(typeof(RadialObjectSpawner), "SetObjectToSpawnBoundingRadius")]
public class RadialObjectSpawnerSetObjectToSpawnBoundingRadiusPatch
{
	public static void Postfix(ref RadialObjectSpawner __instance)
	{
	}
}
