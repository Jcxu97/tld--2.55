using HarmonyLib;
using Il2Cpp;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Patches;

[HarmonyPatch(typeof(PlayerManager), "SetControlMode")]
internal static class PlayerManagerSetControlModePatch
{
	private static PlayerControlMode lastMode;

	internal static void Postfix(PlayerManager __instance, PlayerControlMode mode)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (mode != lastMode)
		{
			lastMode = mode;
			ModBaseEquippableComponent equippableModComponent = ((Component?)(object)__instance.m_ItemInHands).GetEquippableModComponent();
			if ((Object)(object)equippableModComponent != (Object)null && !string.IsNullOrEmpty(equippableModComponent.ImplementationType))
			{
				equippableModComponent?.OnControlModeChangedWhileEquipped?.Invoke();
			}
		}
	}
}
