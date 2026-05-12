using System;
using HarmonyLib;
using Il2Cpp;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using UnityEngine;

namespace ModSettings;

internal static class ModSettingsPatches
{
	[HarmonyPatch(typeof(Panel_OptionsMenu), "InitializeAutosaveMenuItems", new Type[] { })]
	private static class BuildModSettingsGUIPatch
	{
		private static void Postfix(Panel_OptionsMenu __instance)
		{
			ObjectPrefabs.Initialize(__instance);
			DateTime utcNow = DateTime.UtcNow;
			try
			{
				MelonLogger.Msg("Building Mod Settings GUI");
				ModSettingsMenu.BuildGUI(__instance);
			}
			catch (Exception ex)
			{
				MelonLogger.Error("Exception while building Mod Settings GUI\n" + ex.ToString());
				return;
			}
			try
			{
				MelonLogger.Msg("Building Custom Mode GUI");
				CustomModeMenu.BuildGUI();
			}
			catch (Exception ex2)
			{
				MelonLogger.Error("Exception while building Custom Mode GUI\n" + ex2.ToString());
				return;
			}
			MelonLogger.Msg("Done! Took " + (long)(DateTime.UtcNow - utcNow).TotalMilliseconds + " ms. Have a nice day!");
		}
	}

	[HarmonyPatch(typeof(Panel_OptionsMenu), "ConfigureMenu", new Type[] { })]
	private static class AddModSettingsButton
	{
		private static void Postfix(Panel_OptionsMenu __instance)
		{
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			Panel_OptionsMenu __instance2 = __instance;
			if (!ModSettingsMenu.HasVisibleModSettings(InterfaceManager.IsMainMenuEnabled()))
			{
				return;
			}
			BasicMenu basicMenu = __instance2.m_BasicMenu;
			if ((Object)(object)basicMenu == (Object)null)
			{
				return;
			}
			AddAnotherMenuItem(basicMenu, __instance2);
			BasicMenuItemModel val = basicMenu.m_ItemModelList[0];
			int itemCount = basicMenu.GetItemCount();
			Enumerator<BasicMenuItemModel> enumerator = basicMenu.m_ItemModelList.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.m_Id == "ModSettings")
				{
					return;
				}
			}
			basicMenu.AddItem("ModSettings", 19795, itemCount, "Mod Settings", "Change the configuration of your mods", (string)null, Action.op_Implicit((Action)delegate
			{
				ShowModSettings(__instance2);
			}), val.m_NormalTint, val.m_HighlightTint);
		}

		private static void ShowModSettings(Panel_OptionsMenu __instance)
		{
			GetModSettingsGUI(__instance).Enable(__instance);
		}

		private static void AddAnotherMenuItem(BasicMenu basicMenu, Panel_OptionsMenu optionsPanel)
		{
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Expected O, but got Unknown
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Expected O, but got Unknown
			BasicMenu basicMenu2 = basicMenu;
			if (!((Object)(object)GameObject.Find("SCRIPT_InterfaceManager/_GUI_Common/Camera/Anchor/Panel_OptionsMenu/Pages/MainTab/MenuRoot2/Menu/Left_Align/Grid/ModSettings MenuItem") != (Object)null))
			{
				GameObject obj = NGUITools.AddChild(((Component)basicMenu2.m_MenuGrid).gameObject, basicMenu2.m_BasicMenuItemPrefab);
				((Object)obj).name = "ModSettings MenuItem";
				BasicMenuItemView view = obj.GetComponent<BasicMenuItem>().m_View;
				int itemIndex = basicMenu2.m_MenuItems.Count;
				EventDelegate val = new EventDelegate(Callback.op_Implicit((Action)delegate
				{
					basicMenu2.OnItemClicked(itemIndex);
				}));
				view.m_Button.onClick.Add(val);
				EventDelegate val2 = new EventDelegate(Callback.op_Implicit((Action)delegate
				{
					basicMenu2.OnItemDoubleClicked(itemIndex);
				}));
				view.m_DoubleClickButton.m_OnDoubleClick.Add(val2);
				basicMenu2.m_MenuItems.Add(view);
			}
		}
	}

	[HarmonyPatch(typeof(Panel_OptionsMenu), "MainMenuTabOnEnable", new Type[] { })]
	private static class DisableModSettingsWhenBackPressed
	{
		private static void Prefix(Panel_OptionsMenu __instance)
		{
			GetSettingsTab(__instance).SetActive(false);
		}
	}

	[HarmonyPatch(typeof(Panel_OptionsMenu), "OnConfirmSettings", new Type[] { })]
	private static class CallModConfirmWhenButtonPressed
	{
		private static bool Prefix(Panel_OptionsMenu __instance)
		{
			ModSettingsGUI modSettingsGUI = GetModSettingsGUI(__instance);
			if (!((Component)modSettingsGUI).gameObject.activeInHierarchy)
			{
				return true;
			}
			GameAudioManager.PlayGuiConfirm();
			modSettingsGUI.CallOnConfirm();
			return false;
		}
	}

	private const int MOD_SETTINGS_ID = 19795;

	private static ModSettingsGUI GetModSettingsGUI(Panel_OptionsMenu panel)
	{
		return ((Component)((Component)panel).transform.Find("Pages/ModSettings")).GetComponent<ModSettingsGUI>();
	}

	private static GameObject GetSettingsTab(Panel_OptionsMenu panel)
	{
		return ((Component)((Component)panel).transform.Find("Pages/ModSettings")).gameObject;
	}
}
