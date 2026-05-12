using HarmonyLib;
using Il2Cpp;

namespace MotionTracker;

[HarmonyPatch(typeof(RadialObjectSpawner), "Awake")]
public class RadialObjectSpawnerAwakePatch
{
	public static void Postfix(ref RadialObjectSpawner __instance)
	{
	}
}
