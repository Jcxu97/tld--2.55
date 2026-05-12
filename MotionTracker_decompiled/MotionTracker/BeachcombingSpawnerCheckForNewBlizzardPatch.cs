using HarmonyLib;
using Il2CppTLD.Gameplay;

namespace MotionTracker;

[HarmonyPatch(typeof(BeachcombingSpawner), "CheckForNewBlizzard")]
public class BeachcombingSpawnerCheckForNewBlizzardPatch
{
	public static void Prefix(ref BeachcombingSpawner __instance)
	{
	}
}
