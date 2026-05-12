using HarmonyLib;
using Il2Cpp;

namespace MotionTracker;

[HarmonyPatch(typeof(FlockChild), "Update")]
public class FlockUpdatePatch
{
	public static void Postfix(ref FlockChild __instance)
	{
	}
}
