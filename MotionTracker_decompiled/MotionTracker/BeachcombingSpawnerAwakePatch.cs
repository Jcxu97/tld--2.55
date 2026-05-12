using HarmonyLib;
using Il2CppTLD.Gameplay;
using UnityEngine;

namespace MotionTracker;

[HarmonyPatch(typeof(BeachcombingSpawner), "Awake")]
public class BeachcombingSpawnerAwakePatch
{
	public static void Postfix(ref BeachcombingSpawner __instance)
	{
		MyLogger.LogMessage("!!BeachcombingSpawner Awake event: (" + ((Object)__instance).name + ":" + ((Object)__instance).GetInstanceID() + ") with " + __instance.m_ChildSpawners.Count + " child spawners.", 130, "Postfix", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Harmony.cs");
	}
}
