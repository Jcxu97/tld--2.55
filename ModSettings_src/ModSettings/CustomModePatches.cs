using System;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppSystem;
using UnityEngine;

namespace ModSettings;

internal static class CustomModePatches
{
	[HarmonyPatch(typeof(Panel_CustomXPSetup), "UpdateMenuNavigation")]
	private static class UpdateCustomModeDescriptionPatch
	{
		private static void Postfix(Panel_CustomXPSetup __instance, ref int index)
		{
			GameObject val = __instance.m_CustomXPMenuItemOrder[index];
			if (!((Object)(object)val == (Object)null))
			{
				DescriptionHolder component = val.GetComponent<DescriptionHolder>();
				if ((Object)(object)component != (Object)null)
				{
					__instance.m_TooltipLabel.text = component.Text;
				}
			}
		}
	}

	[HarmonyPatch(typeof(Panel_CustomXPSetup), "Enable", new Type[] { typeof(bool) })]
	private static class SetSettingsEnabled
	{
		private static void Prefix(bool enable)
		{
			CustomModeMenu.SetSettingsVisible(enable);
		}
	}

	[HarmonyPatch(typeof(InterfaceManager), "TryDestroyPanel_Internal", new Type[] { typeof(Type) })]
	private static class PreventCustomModePanelDestruction
	{
		private static bool Prefix(Type panelType, ref bool __result)
		{
			if (panelType == Il2CppType.Of<Panel_CustomXPSetup>())
			{
				__result = false;
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(Panel_CustomXPSetup), "DoScroll")]
	private static class CustomModeAbsoluteValueScrollPatch
	{
		private static void Prefix(Panel_CustomXPSetup __instance, ref float scrollAmount)
		{
			if (Mathf.Abs(scrollAmount) < 1f)
			{
				float height = __instance.m_ScrollPanel.height;
				float num = Math.Max(height, __instance.m_ScrollPanelHeight - height);
				scrollAmount *= 1923f / num;
			}
		}
	}

	[HarmonyPatch(typeof(Panel_CustomXPSetup), "OnContinue")]
	private static class CustomGameStartedPatch
	{
		private static void Prefix()
		{
			CustomModeMenu.CallOnConfirm();
		}
	}
}
