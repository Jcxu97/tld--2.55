using HarmonyLib;
using Il2Cpp;

namespace MotionTracker;

[HarmonyPatch(typeof(RadialObjectSpawner), "OnDestroy")]
public class RadialObjectSpawnerOnDestroyPatch
{
	public static void Postfix(ref RadialObjectSpawner __instance)
	{
	}
}
