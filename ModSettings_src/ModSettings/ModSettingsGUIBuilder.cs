using System.Collections.Generic;
using System.Reflection;
using Il2Cpp;
using UnityEngine;

namespace ModSettings;

internal class ModSettingsGUIBuilder : GUIBuilder
{
	private class MenuGroup : Group
	{
		private readonly string modName;

		private readonly ModSettingsGUI modSettings;

		internal MenuGroup(string modName, ModSettingsGUI modSettings)
		{
			this.modName = modName;
			this.modSettings = modSettings;
		}

		protected override void SetVisible(bool visible)
		{
			if (visible)
			{
				modSettings.AddModSelector(modName);
			}
			else
			{
				modSettings.RemoveModSelector(modName);
			}
		}
	}

	private readonly ModSettingsGUI settingsGUI;

	private readonly MenuGroup menuGroup;

	private readonly List<ModSettingsBase> tabSettings;

	internal static GameObject CreateModSettingsTab(Panel_OptionsMenu panel)
	{
		Transform val = ((Component)panel).transform.Find("Pages");
		GameObject val2 = Object.Instantiate<GameObject>(panel.m_QualityTab, val);
		((Object)val2).name = "ModSettings";
		Transform obj = val2.transform.Find("TitleDisplay");
		obj.GetChild(0);
		Transform child = obj.GetChild(1);
		Transform child2 = obj.GetChild(2);
		Transform child3 = obj.GetChild(3);
		((Component)child2).gameObject.SetActive(false);
		((Component)child3).gameObject.SetActive(false);
		Object.Destroy((Object)(object)((Component)child).GetComponent<UILocalize>());
		((Component)child).GetComponent<UILabel>().text = "Mod Settings";
		panel.m_Tabs.Add(val2);
		return val2;
	}

	internal ModSettingsGUIBuilder(string modName, ModSettingsGUI settingsGUI)
		: this(modName, settingsGUI, settingsGUI.CreateModTab(modName))
	{
	}

	private ModSettingsGUIBuilder(string modName, ModSettingsGUI settingsGUI, ModTab modTab)
		: base(modTab.uiGrid, modTab.menuItems)
	{
		this.settingsGUI = settingsGUI;
		menuGroup = new MenuGroup(modName, settingsGUI);
		tabSettings = modTab.modSettings;
	}

	internal override void AddSettings(ModSettingsBase modSettings)
	{
		base.AddSettings(modSettings);
		tabSettings.Add(modSettings);
		menuGroup.NotifyChildAdded(modSettings.IsVisible());
		modSettings.AddVisibilityListener(delegate(bool visible)
		{
			menuGroup.NotifyChildVisible(visible);
		});
	}

	protected override void SetSettingsField(ModSettingsBase modSettings, FieldInfo field, object newValue)
	{
		base.SetSettingsField(modSettings, field, newValue);
		settingsGUI.NotifySettingsNeedConfirmation();
	}
}
