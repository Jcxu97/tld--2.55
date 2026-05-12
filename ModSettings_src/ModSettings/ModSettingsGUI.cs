using System;
using System.Collections.Generic;
using Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using UnityEngine;

namespace ModSettings;

[RegisterTypeInIl2Cpp]
internal class ModSettingsGUI : MonoBehaviour
{
	private readonly Dictionary<string, ModTab> modTabs = new Dictionary<string, ModTab>();

	private ModTab? currentTab;

	private int selectedIndex;

	private string? previousMod;

	private ConsoleComboBox modSelector;

	private UIPanel scrollPanel;

	private Transform scrollPanelOffset;

	private GameObject scrollBar;

	private UISlider scrollBarSlider;

	public ModSettingsGUI(IntPtr ptr)
		: base(ptr)
	{
	}

	private void Awake()
	{
		Build();
	}

	[HideFromIl2Cpp]
	internal void Build()
	{
		Transform contentArea = ((Component)this).transform.Find("GameObject");
		DestroyOldSettings(contentArea);
		modSelector = CreateModSelector(contentArea);
		scrollPanel = CreateScrollPanel(contentArea);
		scrollPanelOffset = CreateOffsetTransform(scrollPanel);
		scrollBar = CreateScrollBar(scrollPanel);
		scrollBarSlider = scrollBar.GetComponentInChildren<UISlider>(true);
	}

	[HideFromIl2Cpp]
	private static void DestroyOldSettings(Transform contentArea)
	{
		for (int num = contentArea.childCount - 1; num >= 2; num--)
		{
			Transform child = contentArea.GetChild(num);
			child.parent = null;
			Object.Destroy((Object)(object)((Component)child).gameObject);
		}
	}

	[HideFromIl2Cpp]
	private ConsoleComboBox CreateModSelector(Transform contentArea)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		ConsoleComboBox modSelector = ((Component)contentArea.Find("Quality")).GetComponent<ConsoleComboBox>();
		modSelector.items.Clear();
		int num = 200;
		Vector3 val = new Vector3((float)num, 0f, 0f);
		Transform transform = ((Component)modSelector).transform;
		transform.localPosition -= val;
		Transform obj = transform.Find("Button_Increase");
		obj.localPosition += val;
		Transform obj2 = transform.Find("Label_Value");
		obj2.localPosition += val / 2f;
		Transform obj3 = transform.Find("Console_Background");
		obj3.localPosition += val / 2f;
		UISprite component = ((Component)obj3).GetComponent<UISprite>();
		((UIWidget)component).width = ((UIWidget)component).width + num;
		EventDelegate.Set(modSelector.onChange, Callback.op_Implicit((Action)delegate
		{
			SelectMod(modSelector.value);
		}));
		return modSelector;
	}

	[HideFromIl2Cpp]
	private static UIPanel CreateScrollPanel(Transform contentArea)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		UIPanel obj = NGUITools.AddChild<UIPanel>(((Component)contentArea).gameObject);
		((Object)((Component)obj).gameObject).name = "ScrollPanel";
		obj.baseClipRegion = new Vector4(0f, 0f, 2000f, 520f);
		obj.clipOffset = new Vector2(500f, -260f);
		obj.clipping = (Clipping)3;
		obj.depth = 100;
		return obj;
	}

	[HideFromIl2Cpp]
	private static Transform CreateOffsetTransform(UIPanel scrollPanel)
	{
		GameObject obj = NGUITools.AddChild(((Component)scrollPanel).gameObject);
		((Object)obj).name = "Offset";
		return obj.transform;
	}

	[HideFromIl2Cpp]
	private GameObject CreateScrollBar(UIPanel scrollPanel)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		GameObject gameObject = ((Component)InterfaceManager.LoadPanel<Panel_CustomXPSetup>().m_Scrollbar.transform.GetChild(0)).gameObject;
		GameObject val = NGUITools.AddChild(((Component)this).gameObject, gameObject);
		((Object)val).name = "Scrollbar";
		val.transform.localPosition = Vector2.op_Implicit(new Vector2(415f, -40f));
		int num = (int)scrollPanel.height;
		UISlider slider = val.GetComponentInChildren<UISlider>(true);
		((UIWidget)((Component)((UIProgressBar)slider).backgroundWidget).GetComponent<UISprite>()).height = num;
		((UIWidget)((Component)((UIProgressBar)slider).foregroundWidget).GetComponent<UISprite>()).height = num;
		((UIWidget)((Component)val.transform.Find("glow")).GetComponent<UISprite>()).height = num + 44;
		EventDelegate.Set(((UIProgressBar)slider).onChange, Callback.op_Implicit((Action)delegate
		{
			OnScroll(slider, playSound: true);
		}));
		return val;
	}

	private void Update()
	{
		if (currentTab == null)
		{
			return;
		}
		if (InputManager.GetEscapePressed((MonoBehaviour)(object)InterfaceManager.GetPanel<Panel_OptionsMenu>()))
		{
			InterfaceManager.GetPanel<Panel_OptionsMenu>().OnCancel();
			return;
		}
		InterfaceManager.GetPanel<Panel_OptionsMenu>().UpdateMenuNavigationGeneric(ref selectedIndex, currentTab.menuItems);
		EnsureSelectedSettingVisible();
		UpdateDescriptionLabel();
		if (currentTab.scrollBarHeight > 0f)
		{
			float axisScrollWheel = InputManager.GetAxisScrollWheel((MonoBehaviour)(object)InterfaceManager.GetPanel<Panel_OptionsMenu>());
			float num = 60f / currentTab.scrollBarHeight;
			if (axisScrollWheel < 0f)
			{
				UISlider obj = scrollBarSlider;
				((UIProgressBar)obj).value = ((UIProgressBar)obj).value + num;
			}
			else if (axisScrollWheel > 0f)
			{
				UISlider obj2 = scrollBarSlider;
				((UIProgressBar)obj2).value = ((UIProgressBar)obj2).value - num;
			}
		}
		HandleModSelectorKeys();
	}

	[HideFromIl2Cpp]
	private void HandleModSelectorKeys()
	{
		if (modSelector == null || modSelector.items.Count == 0) return;
		int jump = 0;
		if (Input.GetKeyDown(KeyCode.PageDown)) jump = 10;
		else if (Input.GetKeyDown(KeyCode.PageUp)) jump = -10;
		else if (Input.GetKeyDown(KeyCode.Home)) jump = -9999;
		else if (Input.GetKeyDown(KeyCode.End)) jump = 9999;
		if (jump == 0) return;

		int idx = modSelector.items.IndexOf(modSelector.value);
		if (idx < 0) idx = 0;
		idx = Mathf.Clamp(idx + jump, 0, modSelector.items.Count - 1);
		string target = modSelector.items[idx];
		modSelector.value = target;
		SelectMod(target);
		GameAudioManager.PlayGUIButtonClick();
	}

	[HideFromIl2Cpp]
	private void OnScroll(UISlider slider, bool playSound)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		scrollPanelOffset.localPosition = Vector2.op_Implicit(new Vector2(0f, ((UIProgressBar)slider).value * (currentTab?.scrollBarHeight ?? 0f)));
		if (playSound)
		{
			GameAudioManager.PlayGUIScroll();
		}
	}

	[HideFromIl2Cpp]
	private void SelectMod(string modName)
	{
		if (currentTab != null)
		{
			((Component)currentTab.uiGrid).gameObject.active = false;
		}
		selectedIndex = 0;
		currentTab = modTabs[modName];
		((Component)currentTab.uiGrid).gameObject.active = true;
		ResizeScrollBar(currentTab);
		EnsureSelectedSettingVisible();
	}

	[HideFromIl2Cpp]
	private void UpdateDescriptionLabel()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = currentTab.menuItems[selectedIndex];
		DescriptionHolder component = val.GetComponent<DescriptionHolder>();
		if (!((Object)(object)component == (Object)null))
		{
			UILabel optionDescriptionLabel = InterfaceManager.GetPanel<Panel_OptionsMenu>().m_OptionDescriptionLabel;
			optionDescriptionLabel.text = component.Text;
			((Component)optionDescriptionLabel).transform.parent = val.transform;
			((Component)optionDescriptionLabel).transform.localPosition = new Vector3(655f, 0f);
			((Component)optionDescriptionLabel).gameObject.SetActive(true);
		}
	}

	[HideFromIl2Cpp]
	private void EnsureSelectedSettingVisible()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		if (Utils.GetMenuMovementVertical((MonoBehaviour)(object)InterfaceManager.GetPanel<Panel_OptionsMenu>(), true, false) == 0f)
		{
			return;
		}
		if (selectedIndex == 0)
		{
			((UIProgressBar)scrollBarSlider).value = 0f;
			return;
		}
		float num = 0f - currentTab.menuItems[selectedIndex].transform.localPosition.y;
		float num2 = scrollPanelOffset.localPosition.y + 33f;
		float num3 = scrollPanelOffset.localPosition.y + scrollPanel.height - 33f;
		if (num < num2)
		{
			UISlider obj = scrollBarSlider;
			((UIProgressBar)obj).value = ((UIProgressBar)obj).value + (num - num2) / currentTab.scrollBarHeight;
			GameAudioManager.PlayGUIScroll();
		}
		else if (num > num3)
		{
			UISlider obj2 = scrollBarSlider;
			((UIProgressBar)obj2).value = ((UIProgressBar)obj2).value + (num - num3) / currentTab.scrollBarHeight;
			GameAudioManager.PlayGUIScroll();
		}
	}

	[HideFromIl2Cpp]
	internal void Enable(Panel_OptionsMenu parentMenu)
	{
		GameAudioManager.PlayGUIButtonClick();
		parentMenu.SetTabActive(((Component)this).gameObject);
	}

	private void OnEnable()
	{
		ModSettingsMenu.SetSettingsVisible(InterfaceManager.IsMainMenuEnabled(), visible: true);
		if (modSelector.items.Count > 0)
		{
			modSelector.items.Sort();
			string text = ((previousMod != null && modSelector.items.Contains(previousMod)) ? previousMod : modSelector.items[0]);
			modSelector.value = text;
			SelectMod(text);
		}
	}

	private void OnDisable()
	{
		ModSettingsMenu.SetSettingsVisible(InterfaceManager.IsMainMenuEnabled(), visible: false);
		ConsoleComboBox obj = modSelector;
		previousMod = ((obj != null) ? obj.value : null);
		foreach (ModTab value in modTabs.Values)
		{
			value.requiresConfirmation = false;
		}
		SetConfirmButtonVisible(value: false);
	}

	[HideFromIl2Cpp]
	internal void NotifySettingsNeedConfirmation()
	{
		currentTab.requiresConfirmation = true;
		SetConfirmButtonVisible(value: true);
	}

	[HideFromIl2Cpp]
	private void SetConfirmButtonVisible(bool value)
	{
		Panel_OptionsMenu panel = InterfaceManager.GetPanel<Panel_OptionsMenu>();
		if ((Object)(object)panel == (Object)null)
		{
			if (value)
			{
				throw new NullReferenceException("Could not get Panel_OptionsMenu");
			}
		}
		else
		{
			panel.m_SettingsNeedConfirmation = value;
		}
	}

	[HideFromIl2Cpp]
	internal void CallOnConfirm()
	{
		foreach (ModTab value in modTabs.Values)
		{
			if (!value.requiresConfirmation)
			{
				continue;
			}
			value.requiresConfirmation = false;
			foreach (ModSettingsBase modSetting in value.modSettings)
			{
				modSetting.CallOnConfirm();
			}
		}
		SetConfirmButtonVisible(value: false);
	}

	[HideFromIl2Cpp]
	internal ModTab CreateModTab(string modName)
	{
		UIGrid val = CreateUIGrid(modName);
		List<GameObject> val2 = new List<GameObject>();
		val2.Add(((Component)modSelector).gameObject);
		ModTab modTab = new ModTab(val, val2);
		val.onReposition = OnReposition.op_Implicit((Action)delegate
		{
			ResizeScrollBar(modTab);
		});
		modTabs.Add(modName, modTab);
		return modTab;
	}

	[HideFromIl2Cpp]
	internal void AddModSelector(string modName)
	{
		modSelector.items.Add(modName);
	}

	[HideFromIl2Cpp]
	internal void RemoveModSelector(string modName)
	{
		modSelector.items.Remove(modName);
	}

	[HideFromIl2Cpp]
	private UIGrid CreateUIGrid(string modName)
	{
		UIGrid obj = NGUITools.AddChild<UIGrid>(((Component)scrollPanelOffset).gameObject);
		((Object)((Component)obj).gameObject).name = "Mod settings grid (" + modName + ")";
		((Component)obj).gameObject.SetActive(false);
		obj.arrangement = (Arrangement)1;
		obj.cellHeight = 33f;
		obj.hideInactive = true;
		return obj;
	}

	[HideFromIl2Cpp]
	private void ResizeScrollBar(ModTab modTab)
	{
		int childCount = ((Component)modTab.uiGrid).transform.childCount;
		if (modTab == currentTab)
		{
			float num = ((UIProgressBar)scrollBarSlider).value * modTab.scrollBarHeight;
			float num2 = childCount * 33;
			modTab.scrollBarHeight = num2 - scrollPanel.height;
			((Component)scrollBarSlider).GetComponent<ScrollbarThumbResizer>().SetNumSteps((int)scrollPanel.height, (int)num2);
			((UIProgressBar)scrollBarSlider).value = Mathf.Clamp01(num / Mathf.Max(1f, modTab.scrollBarHeight));
			OnScroll(scrollBarSlider, playSound: false);
		}
		else
		{
			modTab.scrollBarHeight = (float)(childCount * 33) - scrollPanel.height;
		}
		GameObject obj = scrollBar;
		ModTab? modTab2 = currentTab;
		obj.SetActive(modTab2 != null && modTab2.scrollBarHeight > 0f);
	}
}
