using HarmonyLib;
using Il2Cpp;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Patches;

[HarmonyPatch(typeof(PlayerManager), "TakeOffClothingItem")]
internal static class PlayerManager_TakeOffClothingItem
{
	internal static void Postfix(GearItem gi)
	{
		((Component?)(object)gi).GetComponentSafe<ModClothingComponent>()?.OnTakeOff?.Invoke();
	}
}
