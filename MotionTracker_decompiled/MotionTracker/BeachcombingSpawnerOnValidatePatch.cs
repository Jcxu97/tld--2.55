using HarmonyLib;
using Il2CppTLD.Gameplay;
using UnityEngine;

namespace MotionTracker;

[HarmonyPatch(typeof(BeachcombingSpawner), "OnValidate")]
public class BeachcombingSpawnerOnValidatePatch
{
	public static void Postfix(ref BeachcombingSpawner __instance)
	{
		MyLogger.LogMessage("!!BeachcombingSpawner OnValidate event: (" + ((Object)__instance).name + ":" + ((Object)__instance).GetInstanceID() + ") with GameObject.activeSelf=" + ((Component)__instance).gameObject.activeSelf + ".", 334, "Postfix", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Harmony.cs");
	}
}
