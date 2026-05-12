using HarmonyLib;
using Il2Cpp;

namespace MotionTracker;

[HarmonyPatch(typeof(FlockController), "Start")]
public class FlockController_Start_Patch
{
	public static void Postfix(ref FlockController __instance)
	{
	}
}
