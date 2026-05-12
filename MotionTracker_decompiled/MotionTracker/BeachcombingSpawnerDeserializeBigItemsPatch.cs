using HarmonyLib;
using Il2CppTLD.Gameplay;
using UnityEngine;

namespace MotionTracker;

[HarmonyPatch(typeof(BeachcombingSpawner), "DeserializeBigItems")]
public class BeachcombingSpawnerDeserializeBigItemsPatch
{
	public static void Prefix(ref BeachcombingSpawner __instance)
	{
		MyLogger.LogMessage("!!BeachcombingSpawner DeserializeBigItems event: (" + ((Object)__instance).name + ":" + ((Object)__instance).GetInstanceID() + ") with GameObject.activeSelf=" + ((Component)__instance).gameObject.activeSelf + ".", 377, "Prefix", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Harmony.cs");
	}
}
