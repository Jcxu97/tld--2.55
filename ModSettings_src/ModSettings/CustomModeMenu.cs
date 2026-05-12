using System;
using System.Collections.Generic;
using Il2Cpp;

namespace ModSettings;

internal static class CustomModeMenu
{
	private static readonly int numPositions;

	private static readonly HashSet<ModSettingsBase> settings;

	private static readonly List<ModSettingsBase>[] settingsAtPosition;

	private static bool guiBuilt;

	static CustomModeMenu()
	{
		numPositions = Position.BelowAll.Index - Position.AboveAll.Index + 1;
		settings = new HashSet<ModSettingsBase>();
		settingsAtPosition = new List<ModSettingsBase>[numPositions];
		guiBuilt = false;
		for (int i = 0; i < numPositions; i++)
		{
			settingsAtPosition[i] = new List<ModSettingsBase>();
		}
	}

	internal static void RegisterSettings(ModSettingsBase modSettings, Position targetPosition)
	{
		if (targetPosition == null)
		{
			throw new ArgumentNullException("targetPosition");
		}
		if (settings.Contains(modSettings))
		{
			throw new ArgumentException("[ModSettings] Cannot add the same settings object multiple times", "modSettings");
		}
		if (guiBuilt)
		{
			throw new InvalidOperationException("[ModSettings] RegisterSettings called after the GUI has been built.\nCall this method before Panel_CustomXPSetup::Awake, preferably from your mod's OnLoad method");
		}
		settings.Add(modSettings);
		settingsAtPosition[targetPosition.Index].Add(modSettings);
	}

	internal static void BuildGUI()
	{
		guiBuilt = true;
		CustomModeGUIBuilder customModeGUIBuilder = new CustomModeGUIBuilder(InterfaceManager.LoadPanel<Panel_CustomXPSetup>());
		for (int i = 0; i < numPositions; i++)
		{
			foreach (ModSettingsBase item in settingsAtPosition[i])
			{
				customModeGUIBuilder.AddSettings(item);
			}
			customModeGUIBuilder.NextSection();
		}
		customModeGUIBuilder.Finish();
	}

	internal static void SetSettingsVisible(bool enable)
	{
		foreach (ModSettingsBase setting in settings)
		{
			setting.SetMenuVisible(enable);
		}
	}

	internal static void CallOnConfirm()
	{
		foreach (ModSettingsBase setting in settings)
		{
			setting.CallOnConfirm();
		}
	}
}
