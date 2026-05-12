using HarmonyLib;
using Il2Cpp;

namespace MotionTracker;

[HarmonyPatch(typeof(RadialObjectSpawner), "SpawnAttemptOnceWithVisCheck")]
public class RadialObjectSpawnerSpawnAttemptOnceWithVisCheckPatch
{
	public static void Postfix(ref RadialObjectSpawner __instance)
	{
	}
}
