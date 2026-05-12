using System;
using System.Collections.Generic;
using Il2Cpp;
using UnityEngine;

namespace ModSettings;

internal static class ModSettingsMenu
{
	private static readonly HashSet<ModSettingsBase> mainMenuSettings = new HashSet<ModSettingsBase>();

	private static readonly HashSet<ModSettingsBase> inGameSettings = new HashSet<ModSettingsBase>();

	private static readonly SortedDictionary<string, List<ModSettingsBase>> settingsByModName = new SortedDictionary<string, List<ModSettingsBase>>();

	private static ModSettingsGUI? modSettingsGUI = null;

	internal static void RegisterSettings(ModSettingsBase modSettings, string modName, MenuType menuType)
	{
		if (string.IsNullOrEmpty(modName))
		{
			throw new ArgumentException("[ModSettings] Mod name must be a non-empty string", "modName");
		}
		if (mainMenuSettings.Contains(modSettings) || inGameSettings.Contains(modSettings))
		{
			throw new ArgumentException("[ModSettings] Cannot add the same settings object multiple times", "modSettings");
		}
		if ((Object)(object)modSettingsGUI != (Object)null)
		{
			throw new InvalidOperationException("[ModSettings] RegisterSettings called after the GUI has been built.\nCall this method before Panel_CustomXPSetup::Awake, preferably from your mod's OnLoad method");
		}
		if (menuType != MenuType.InGameOnly)
		{
			mainMenuSettings.Add(modSettings);
		}
		if (menuType != MenuType.MainMenuOnly)
		{
			inGameSettings.Add(modSettings);
		}
		if (settingsByModName.TryGetValue(modName, out List<ModSettingsBase> value))
		{
			value.Add(modSettings);
			return;
		}
		value = new List<ModSettingsBase> { modSettings };
		settingsByModName.Add(modName, value);
	}

	internal static void BuildGUI(Panel_OptionsMenu panel)
	{
		modSettingsGUI = ModSettingsGUIBuilder.CreateModSettingsTab(panel).AddComponent<ModSettingsGUI>();
		foreach (KeyValuePair<string, List<ModSettingsBase>> item in settingsByModName)
		{
			ModSettingsGUIBuilder modSettingsGUIBuilder = new ModSettingsGUIBuilder(item.Key, modSettingsGUI);
			foreach (ModSettingsBase item2 in item.Value)
			{
				modSettingsGUIBuilder.AddSettings(item2);
			}
		}
	}

	internal static void SetSettingsVisible(bool isMainMenu, bool visible)
	{
		foreach (ModSettingsBase item in isMainMenu ? mainMenuSettings : inGameSettings)
		{
			item.SetMenuVisible(visible);
		}
	}

	internal static bool HasVisibleModSettings(bool isMainMenu)
	{
		foreach (ModSettingsBase item in isMainMenu ? mainMenuSettings : inGameSettings)
		{
			if (item.IsUserVisible())
			{
				return true;
			}
		}
		return false;
	}
}
