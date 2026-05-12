using Il2Cpp;

namespace ModComponent.Utils;

internal static class EquipItemPopupUtils
{
	public static void ShowItemPopups(string primaryAction, string secondaryAction, bool showAmmo, bool showReload, bool showHolster)
	{
		EquipItemPopup equipItemPopup = InterfaceManager.GetPanel<Panel_HUD>().m_EquipItemPopup;
		ShowItemIcons(equipItemPopup, primaryAction, secondaryAction, showAmmo);
		if (Utils.IsGamepadActive())
		{
			equipItemPopup.m_ButtonPromptFire.ShowPromptForKey(primaryAction, "Fire", false);
			MaybeRepositionFireButtonPrompt(equipItemPopup, secondaryAction);
			equipItemPopup.m_ButtonPromptAltFire.ShowPromptForKey(secondaryAction, "AltFire", false);
			MaybeRepositionAltFireButtonPrompt(equipItemPopup, primaryAction);
		}
		else
		{
			equipItemPopup.m_ButtonPromptFire.ShowPromptForKey(secondaryAction, "AltFire", false);
			MaybeRepositionFireButtonPrompt(equipItemPopup, primaryAction);
			equipItemPopup.m_ButtonPromptAltFire.ShowPromptForKey(primaryAction, "Interact", false);
			MaybeRepositionAltFireButtonPrompt(equipItemPopup, secondaryAction);
		}
		string text = (showReload ? Localization.Get("GAMEPLAY_Reload") : string.Empty);
		equipItemPopup.m_ButtonPromptReload.ShowPromptForKey(text, "Reload", false);
		string text2 = (showHolster ? Localization.Get("GAMEPLAY_HolsterPrompt") : string.Empty);
		equipItemPopup.m_ButtonPromptHolster.ShowPromptForKey(text2, "Holster", false);
	}

	internal static void MaybeRepositionAltFireButtonPrompt(EquipItemPopup equipItemPopup, string otherAction)
	{
		equipItemPopup.MaybeRepositionAltFireButtonPrompt(otherAction);
	}

	internal static void MaybeRepositionFireButtonPrompt(EquipItemPopup equipItemPopup, string otherAction)
	{
		equipItemPopup.MaybeRepositionFireButtonPrompt(otherAction);
	}

	internal static void ShowItemIcons(EquipItemPopup equipItemPopup, string primaryAction, string secondaryAction, bool showAmmo)
	{
		equipItemPopup.ShowItemIcons(primaryAction, secondaryAction, showAmmo);
	}
}
