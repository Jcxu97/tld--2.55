using System;
using HarmonyLib;
using Il2Cpp;

namespace MotionTracker;

[HarmonyPatch(typeof(Panel_Base), "Enable", new Type[] { typeof(bool) })]
public class PanelPatch
{
	public static void Postfix(ref Panel_Base __instance, bool enable)
	{
		PingManager.inMenu = enable;
	}
}
