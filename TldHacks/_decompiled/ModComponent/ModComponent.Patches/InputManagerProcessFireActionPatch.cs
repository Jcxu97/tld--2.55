using HarmonyLib;
using Il2Cpp;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Patches;

[HarmonyPatch(typeof(InputManager), "ProcessFireAction")]
internal static class InputManagerProcessFireActionPatch
{
	public static bool Prefix(MonoBehaviour context)
	{
		PlayerManager playerManagerComponent = GameManager.GetPlayerManagerComponent();
		if ((Object)(object)playerManagerComponent == (Object)null || GameManager.ControlsLocked() || InterfaceManager.IsOverlayActiveImmediate() || !InputManager.GetFirePressed(context) || InputManager.GetFireReleased(context))
		{
			return true;
		}
		ModBaseEquippableComponent equippableModComponent = ((Component?)(object)playerManagerComponent.m_ItemInHands).GetEquippableModComponent();
		if (equippableModComponent == null || equippableModComponent.Implementation == null)
		{
			return true;
		}
		equippableModComponent.OnPrimaryAction?.Invoke();
		return false;
	}
}
