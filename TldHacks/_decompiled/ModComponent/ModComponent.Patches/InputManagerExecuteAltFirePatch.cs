using HarmonyLib;
using Il2Cpp;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Patches;

[HarmonyPatch(typeof(InputManager), "ExecuteAltFire")]
internal static class InputManagerExecuteAltFirePatch
{
	public static bool Prefix()
	{
		PlayerManager playerManagerComponent = GameManager.GetPlayerManagerComponent();
		if ((Object)(object)playerManagerComponent == (Object)null || InterfaceManager.IsOverlayActiveImmediate() || playerManagerComponent.IsInPlacementMode() || playerManagerComponent.ItemInHandsPlaceable())
		{
			return true;
		}
		ModBaseEquippableComponent equippableModComponent = ((Component?)(object)playerManagerComponent.m_ItemInHands).GetEquippableModComponent();
		if ((Object)(object)equippableModComponent == (Object)null)
		{
			return true;
		}
		equippableModComponent.OnSecondaryAction?.Invoke();
		return false;
	}
}
