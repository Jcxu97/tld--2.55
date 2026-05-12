using HarmonyLib;
using Il2Cpp;

namespace MotionTracker;

[HarmonyPatch(typeof(RadialObjectSpawner), "SpawnAttemptAllNoVisChecks")]
public class RadialObjectSpawnerSpawnAttemptAllNoVisChecksPatch
{
	public static void Postfix(ref RadialObjectSpawner __instance)
	{
	}
}
