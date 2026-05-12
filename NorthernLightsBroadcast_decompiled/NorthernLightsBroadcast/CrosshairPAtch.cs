using HarmonyLib;
using Il2Cpp;

namespace NorthernLightsBroadcast;

[HarmonyPatch(typeof(PlayerManager), "ShouldSuppressCrosshairs")]
public class CrosshairPAtch
{
	public static void Postfix(PlayerManager __instance, ref bool __result)
	{
		if (TVLock.lockedInTVView)
		{
			__result = true;
		}
	}
}
