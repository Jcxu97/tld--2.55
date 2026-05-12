using HarmonyLib;
using Il2Cpp;

namespace MotionTracker;

[HarmonyPatch(typeof(RadialObjectSpawner), "DisableSplineMeshUpdating")]
public class RadialObjectSpawnerDisableSplineMeshUpdatingPatch
{
	public static void Postfix(ref RadialObjectSpawner __instance)
	{
	}
}
