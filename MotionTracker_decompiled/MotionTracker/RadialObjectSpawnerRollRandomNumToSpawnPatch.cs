using HarmonyLib;
using Il2Cpp;

namespace MotionTracker;

[HarmonyPatch(typeof(RadialObjectSpawner), "RollRandomNumToSpawn")]
public class RadialObjectSpawnerRollRandomNumToSpawnPatch
{
	public static void Postfix(ref RadialObjectSpawner __instance)
	{
	}
}
