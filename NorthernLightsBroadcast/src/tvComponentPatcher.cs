using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace NorthernLightsBroadcast;

[HarmonyPatch(typeof(GearItem), "Awake")]
public class tvComponentPatcher
{
	public static void Postfix(ref GearItem __instance)
	{
		if ((((Object)__instance).name.Contains("GEAR_TV_LCD") || ((Object)__instance).name.Contains("GEAR_TV_CRT") || ((Object)__instance).name.Contains("GEAR_TV_WALL")) && (Object)(object)((Component)__instance).gameObject.GetComponent<TVManager>() == (Object)null)
		{
			((Component)__instance).gameObject.AddComponent<TVManager>();
		}
	}
}
