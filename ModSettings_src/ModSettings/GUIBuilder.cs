using System;
using System.Collections.Generic;
using System.Reflection;
using Il2Cpp;
using Il2CppSystem.Collections.Generic;
using UnityEngine;

namespace ModSettings;

internal abstract class GUIBuilder
{
	protected class Header : Group
	{
		private readonly List<GameObject> guiObjects;

		internal Header(params GameObject[] guiObjects)
		{
			this.guiObjects = new List<GameObject>(guiObjects);
		}

		protected override void SetVisible(bool visible)
		{
			foreach (GameObject guiObject in guiObjects)
			{
				NGUITools.SetActiveSelf(guiObject, visible);
			}
		}
	}

	internal const int gridCellHeight = 33;

	protected readonly UIGrid uiGrid;

	protected readonly List<GameObject> menuItems;

	protected Header lastHeader;

	protected GUIBuilder(UIGrid uiGrid, List<GameObject> menuItems)
	{
		this.uiGrid = uiGrid;
		this.menuItems = menuItems;
	}

	internal virtual void AddSettings(ModSettingsBase modSettings)
	{
		FieldInfo[] fields = modSettings.GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			Attributes.GetAttributes(fieldInfo, out SectionAttribute section, out NameAttribute name, out DescriptionAttribute description, out SliderAttribute slider, out ChoiceAttribute choice);
			if (section != null)
			{
				AddHeader(section);
			}
			else if (lastHeader == null)
			{
				AddPaddingHeader();
			}
			if (slider != null)
			{
				AddSliderSetting(modSettings, fieldInfo, name, description, slider);
				continue;
			}
			if (choice != null)
			{
				AddChoiceSetting(modSettings, fieldInfo, name, description, choice);
				continue;
			}
			Type fieldType = fieldInfo.FieldType;
			if (fieldType == typeof(KeyCode))
			{
				AddKeySetting(modSettings, fieldInfo, name, description);
				continue;
			}
			if (fieldType.IsEnum)
			{
				AddChoiceSetting(modSettings, fieldInfo, name, description, ChoiceAttribute.ForEnumType(fieldType));
				continue;
			}
			if (fieldType == typeof(bool))
			{
				AddChoiceSetting(modSettings, fieldInfo, name, description, ChoiceAttribute.YesNoAttribute);
				continue;
			}
			if (AttributeFieldTypes.IsFloatType(fieldType))
			{
				AddSliderSetting(modSettings, fieldInfo, name, description, SliderAttribute.DefaultFloatRange);
				continue;
			}
			if (AttributeFieldTypes.IsIntegerType(fieldType))
			{
				AddSliderSetting(modSettings, fieldInfo, name, description, SliderAttribute.DefaultIntRange);
				continue;
			}
			throw new ArgumentException("Unsupported field type: " + fieldType.Name);
		}
	}

	protected virtual void SetSettingsField(ModSettingsBase modSettings, FieldInfo field, object newValue)
	{
		modSettings.SetFieldValue(field, newValue);
	}

	private void AddHeader(SectionAttribute section)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = NGUITools.AddChild(((Component)uiGrid).gameObject);
		GameObject val2 = NGUITools.AddChild(((Component)uiGrid).gameObject);
		GameObject obj = NGUITools.AddChild(val2, ObjectPrefabs.HeaderLabelPrefab);
		obj.SetActive(true);
		obj.transform.localPosition = Vector2.op_Implicit(new Vector2(-70f, 0f));
		((Object)obj).name = "Custom Header (" + section.Title + ")";
		SetLabelText(obj.transform, section.Title, section.Localize);
		lastHeader = new Header(val2, val);
	}

	private void AddPaddingHeader()
	{
		GameObject val = NGUITools.AddChild(((Component)uiGrid).gameObject);
		lastHeader = new Header(val);
	}

	private void AddKeySetting(ModSettingsBase modSettings, FieldInfo field, NameAttribute name, DescriptionAttribute description)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		ModSettingsBase modSettings2 = modSettings;
		FieldInfo field2 = field;
		GameObject val = CreateSetting(name, description, ObjectPrefabs.KeyEntryPrefab, "Label");
		GameObject gameObject = ((Component)val.transform.FindChild("Keybinding_Button")).gameObject;
		CustomKeybinding customKeybinding = val.AddComponent<CustomKeybinding>();
		customKeybinding.keyRebindingButton = gameObject.GetComponent<KeyRebindingButton>();
		customKeybinding.currentKeycodeSetting = (KeyCode)field2.GetValue(modSettings2);
		customKeybinding.RefreshLabelValue();
		EventDelegate.Set(gameObject.GetComponent<UIButton>().onClick, Callback.op_Implicit((Action)customKeybinding.OnClick));
		customKeybinding.OnChange = delegate
		{
			UpdateKeyValue(modSettings2, field2, customKeybinding);
		};
		modSettings2.AddRefreshAction(delegate
		{
			UpdateKeyChoice(modSettings2, field2, customKeybinding);
		});
		SetVisibilityListener(modSettings2, field2, val, lastHeader);
	}

	private void UpdateKeyValue(ModSettingsBase modSettings, FieldInfo field, CustomKeybinding customKeybinding)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		SetSettingsField(modSettings, field, customKeybinding.currentKeycodeSetting);
	}

	private void UpdateKeyChoice(ModSettingsBase modSettings, FieldInfo field, CustomKeybinding customKeybinding)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		KeyCode val = (customKeybinding.currentKeycodeSetting = (KeyCode)field.GetValue(modSettings));
		customKeybinding.keyRebindingButton.SetValueLabel(val.ToString());
	}

	private void AddChoiceSetting(ModSettingsBase modSettings, FieldInfo field, NameAttribute name, DescriptionAttribute description, ChoiceAttribute choice)
	{
		ModSettingsBase modSettings2 = modSettings;
		FieldInfo field2 = field;
		GameObject val = CreateSetting(name, description, ObjectPrefabs.ComboBoxPrefab, "Label");
		ConsoleComboBox comboBox = val.GetComponent<ConsoleComboBox>();
		comboBox.items.Clear();
		string[] names = choice.Names;
		foreach (string text in names)
		{
			comboBox.items.Add(text);
		}
		comboBox.m_Localize = choice.Localize;
		EventDelegate.Set(comboBox.onChange, Callback.op_Implicit((Action)delegate
		{
			UpdateChoiceValue(modSettings2, field2, comboBox.GetCurrentIndex());
		}));
		modSettings2.AddRefreshAction(delegate
		{
			UpdateChoiceComboBox(modSettings2, field2, comboBox);
		});
		SetVisibilityListener(modSettings2, field2, val, lastHeader);
	}

	private void UpdateChoiceValue(ModSettingsBase modSettings, FieldInfo field, int selectedIndex)
	{
		Type type = field.FieldType;
		if (type.IsEnum)
		{
			type = Enum.GetUnderlyingType(type);
		}
		SetSettingsField(modSettings, field, Convert.ChangeType(selectedIndex, type, null));
	}

	private static void UpdateChoiceComboBox(ModSettingsBase modSettings, FieldInfo field, ConsoleComboBox comboBox)
	{
		int num = Convert.ToInt32(field.GetValue(modSettings));
		comboBox.value = comboBox.items[num];
	}

	private void AddSliderSetting(ModSettingsBase modSettings, FieldInfo field, NameAttribute name, DescriptionAttribute description, SliderAttribute range)
	{
		ModSettingsBase modSettings2 = modSettings;
		FieldInfo field2 = field;
		GameObject val = CreateSetting(name, description, ObjectPrefabs.SliderPrefab, "Label_FOV");
		ConsoleSlider component = val.GetComponent<ConsoleSlider>();
		UILabel uiLabel = component.m_SliderObject.GetComponentInChildren<UILabel>();
		UISlider uiSlider = component.m_SliderObject.GetComponentInChildren<UISlider>();
		bool flag = AttributeFieldTypes.IsFloatType(field2.FieldType);
		float from = (flag ? range.From : Mathf.Round(range.From));
		float to = (flag ? range.To : Mathf.Round(range.To));
		int num = range.NumberOfSteps;
		if (num < 0)
		{
			num = (flag ? 1 : (Mathf.RoundToInt(Mathf.Abs(from - to)) + 1));
		}
		string numberFormat = range.NumberFormat;
		if (string.IsNullOrEmpty(numberFormat))
		{
			numberFormat = (flag ? "{0:F1}" : "{0:D}");
		}
		Callback val2 = Callback.op_Implicit((Action)delegate
		{
			UpdateSliderValue(modSettings2, field2, uiSlider, uiLabel, from, to, numberFormat);
		});
		EventDelegate.Set(component.onChange, val2);
		EventDelegate.Set(((UIProgressBar)uiSlider).onChange, val2);
		modSettings2.AddRefreshAction(delegate
		{
			UpdateSlider(modSettings2, field2, uiSlider, uiLabel, from, to, numberFormat);
		});
		float num2 = Convert.ToSingle(field2.GetValue(modSettings2));
		((UIProgressBar)uiSlider).value = (num2 - from) / (to - from);
		((UIProgressBar)uiSlider).numberOfSteps = num;
		UpdateSliderLabel(field2, uiLabel, num2, numberFormat);
		SetVisibilityListener(modSettings2, field2, val, lastHeader);
	}

	private void UpdateSliderValue(ModSettingsBase modSettings, FieldInfo field, UISlider slider, UILabel label, float from, float to, string numberFormat)
	{
		float num = from + ((UIProgressBar)slider).value * (to - from);
		if (!SliderMatchesField(modSettings, field, num))
		{
			if (AttributeFieldTypes.IsIntegerType(field.FieldType))
			{
				num = Mathf.Round(num);
			}
			UpdateSliderLabel(field, label, num, numberFormat);
			SetSettingsField(modSettings, field, Convert.ChangeType(num, field.FieldType, null));
			if (modSettings.IsVisible() && ((UIProgressBar)slider).numberOfSteps > 1)
			{
				GameAudioManager.PlayGUISlider();
			}
		}
	}

	private static void UpdateSlider(ModSettingsBase modSettings, FieldInfo field, UISlider slider, UILabel label, float from, float to, string numberFormat)
	{
		float sliderValue = from + ((UIProgressBar)slider).value * (to - from);
		if (!SliderMatchesField(modSettings, field, sliderValue))
		{
			float num = Convert.ToSingle(field.GetValue(modSettings));
			((UIProgressBar)slider).value = (num - from) / (to - from);
			UpdateSliderLabel(field, label, num, numberFormat);
		}
	}

	private static bool SliderMatchesField(ModSettingsBase modSettings, FieldInfo field, float sliderValue)
	{
		if (AttributeFieldTypes.IsFloatType(field.FieldType))
		{
			float num = Convert.ToSingle(field.GetValue(modSettings));
			return sliderValue == num;
		}
		long num2 = Convert.ToInt64(field.GetValue(modSettings));
		long num3 = (long)Mathf.Round(sliderValue);
		return num2 == num3;
	}

	private static void UpdateSliderLabel(FieldInfo field, UILabel label, float value, string numberFormat)
	{
		if (AttributeFieldTypes.IsFloatType(field.FieldType))
		{
			label.text = string.Format(numberFormat, value);
			return;
		}
		long num = (long)Mathf.Round(value);
		label.text = string.Format(numberFormat, num);
	}

	private GameObject CreateSetting(NameAttribute name, DescriptionAttribute description, GameObject prefab, string labelName)
	{
		GameObject val = NGUITools.AddChild(((Component)uiGrid).gameObject, prefab);
		((Object)val).name = "Custom Setting (" + name.Name + ")";
		SetLabelText(val.transform.Find(labelName), name.Name, name.Localize);
		val.AddComponent<DescriptionHolder>().SetDescription(description?.Description ?? string.Empty, description?.Localize ?? false);
		menuItems.Add(val);
		return val;
	}

	private static void SetLabelText(Transform transform, string text, bool localize)
	{
		if (localize)
		{
			((Component)transform).GetComponent<UILocalize>().key = text;
			return;
		}
		Object.Destroy((Object)(object)((Component)transform).GetComponent<UILocalize>());
		((Component)transform).GetComponent<UILabel>().text = text;
	}

	private void SetVisibilityListener(ModSettingsBase modSettings, FieldInfo field, GameObject guiObject, Header header)
	{
		GameObject guiObject2 = guiObject;
		Header header2 = header;
		bool flag = modSettings.IsFieldVisible(field);
		if (guiObject2.activeSelf != flag)
		{
			guiObject2.SetActive(flag);
		}
		header2?.NotifyChildAdded(flag);
		modSettings.AddVisibilityListener(field, delegate(bool visible)
		{
			guiObject2.SetActive(visible);
			header2?.NotifyChildVisible(visible);
			uiGrid.repositionNow = true;
		});
	}
}
