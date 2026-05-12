using HarmonyLib;
using Il2Cpp;

namespace NorthernLightsBroadcast;

[HarmonyPatch(typeof(InterfaceManager), "ShouldEnableMousePointer")]
public class CursorPatch
{
	public static void Postfix(ref bool __result)
	{
		if (TVLock.lockedInTVView)
		{
			__result = true;
		}
	}
}
