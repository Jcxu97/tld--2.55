using System;
using HarmonyLib;
using Il2CppAK.Wwise;
using Il2CppTLD.Gear;

namespace CraftingRevisions.Patches;

[HarmonyPatch(typeof(UserBlueprintData), "MakeRuntimeWwiseEvent", new Type[] { typeof(string) })]
internal class UserBlueprintData_MakeRuntimeWwiseEvent
{
	private static void Prefix(string eventName, ref bool __runOriginal, ref Event? __result)
	{
		__result = Utils.MakeAudioEvent(eventName);
		__runOriginal = false;
	}
}
