using HarmonyLib;
using Il2Cpp;

namespace MotionTracker;

[HarmonyPatch(typeof(FlockController), "Update")]
public class FlockController_Update_Patch
{
	public static void Postfix(ref FlockController __instance)
	{
	}
}
