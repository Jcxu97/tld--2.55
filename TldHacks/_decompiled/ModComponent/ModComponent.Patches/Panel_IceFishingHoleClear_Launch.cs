using HarmonyLib;
using Il2Cpp;

namespace ModComponent.Patches;

[HarmonyPatch(typeof(Panel_IceFishingHoleClear), "Launch")]
internal static class Panel_IceFishingHoleClear_Launch
{
	private static bool callOnce = true;

	internal static void Prefix(Panel_IceFishingHoleClear __instance)
	{
		if (callOnce)
		{
			__instance.InitializeFilteredUsableTools();
			callOnce = false;
		}
	}
}
