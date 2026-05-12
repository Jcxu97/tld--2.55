using Il2Cpp;
using UnityEngine;

namespace ModSettings;

internal static class ObjectPrefabs
{
	private static bool isInitialized;

	internal static GameObject ComboBoxPrefab { get; private set; }

	internal static GameObject CustomComboBoxPrefab { get; private set; }

	internal static GameObject DisplayPrefab { get; private set; }

	internal static GameObject EmptyPrefab { get; private set; }

	internal static GameObject HeaderLabelPrefab { get; private set; }

	internal static GameObject KeyEntryPrefab { get; private set; }

	internal static GameObject SliderPrefab { get; private set; }

	internal static GameObject TextEntryPrefab { get; private set; }

	internal static void Initialize(Panel_OptionsMenu optionsPanel)
	{
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		if (!isInitialized)
		{
			HeaderLabelPrefab = Object.Instantiate<GameObject>(((Component)InterfaceManager.LoadPanel<Panel_CustomXPSetup>().m_ScrollPanelOffsetTransform.GetChild(0).Find("Header")).gameObject);
			HeaderLabelPrefab.SetActive(false);
			ComboBoxPrefab = Object.Instantiate<GameObject>(((Component)InterfaceManager.LoadPanel<Panel_CustomXPSetup>().m_AllowInteriorSpawnPopupList).gameObject);
			ComboBoxPrefab.SetActive(false);
			CustomComboBoxPrefab = MakeCustomComboBoxPrefab();
			CustomComboBoxPrefab.SetActive(false);
			DisplayPrefab = MakeDisplayPrefab();
			DisplayPrefab.SetActive(false);
			EmptyPrefab = MakeEmptyPrefab();
			EmptyPrefab.SetActive(false);
			KeyEntryPrefab = MakeKeyEntryPrefab(optionsPanel);
			KeyEntryPrefab.SetActive(false);
			TextEntryPrefab = MakeTextEntryPrefab();
			TextEntryPrefab.SetActive(false);
			Object.DestroyImmediate((Object)(object)optionsPanel.m_FieldOfViewSlider.m_SliderObject.GetComponent<GenericSliderSpawner>());
			SliderPrefab = Object.Instantiate<GameObject>(((Component)optionsPanel.m_FieldOfViewSlider).gameObject);
			SliderPrefab.SetActive(false);
			SliderPrefab.transform.Find("Label_FOV").localPosition = new Vector3(-10f, 0f, -1f);
			BoxCollider componentInChildren = SliderPrefab.GetComponentInChildren<BoxCollider>();
			componentInChildren.center = new Vector3(150f, 0f);
			componentInChildren.size = new Vector3(900f, 30f);
			isInitialized = true;
		}
	}

	private static GameObject MakeCustomComboBoxPrefab()
	{
		GameObject obj = Object.Instantiate<GameObject>(ComboBoxPrefab);
		Object.DestroyImmediate((Object)(object)obj.GetComponent<ConsoleComboBox>());
		return obj;
	}

	private static GameObject MakeDisplayPrefab()
	{
		GameObject obj = Object.Instantiate<GameObject>(ComboBoxPrefab);
		Object.DestroyImmediate((Object)(object)obj.GetComponent<ConsoleComboBox>());
		obj.DestroyChild("Button_Decrease");
		obj.DestroyChild("Button_Increase");
		return obj;
	}

	private static GameObject MakeEmptyPrefab()
	{
		GameObject obj = Object.Instantiate<GameObject>(ComboBoxPrefab);
		Object.DestroyImmediate((Object)(object)obj.GetComponent<ConsoleComboBox>());
		obj.DestroyChild("Button_Decrease");
		obj.DestroyChild("Button_Increase");
		obj.DestroyChild("Label_Value");
		return obj;
	}

	private static GameObject MakeKeyEntryPrefab(Panel_OptionsMenu optionsPanel)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = Object.Instantiate<GameObject>(ComboBoxPrefab);
		GameObject obj = Object.Instantiate<GameObject>(((Component)optionsPanel.m_RebindingTab.transform.FindChild("GameObject").FindChild("LeftSide").FindChild("Button_Rebinding")).gameObject);
		obj.transform.position = val.transform.FindChild("Label_Value").position;
		obj.transform.parent = val.transform;
		((Object)obj).name = "Keybinding_Button";
		Object.DestroyImmediate((Object)(object)val.GetComponent<ConsoleComboBox>());
		val.DestroyChild("Button_Decrease");
		val.DestroyChild("Button_Increase");
		val.DestroyChild("Label_Value");
		obj.DestroyChild("Label_Name");
		return val;
	}

	private static GameObject MakeTextEntryPrefab()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = Object.Instantiate<GameObject>(ComboBoxPrefab);
		GameObject obj = Object.Instantiate<GameObject>(((Component)InterfaceManager.LoadPanel<Panel_Confirmation>().m_GenericMessageGroup.m_InputField).gameObject);
		obj.transform.position = val.transform.FindChild("Label_Value").position;
		obj.transform.parent = val.transform;
		((Object)obj).name = "Text_Box";
		obj.GetComponent<TextInputField>().m_MaxLength = 25u;
		Object.DestroyImmediate((Object)(object)val.GetComponent<ConsoleComboBox>());
		val.DestroyChild("Button_Decrease");
		val.DestroyChild("Button_Increase");
		val.DestroyChild("Label_Value");
		obj.DestroyChild("bg");
		obj.DestroyChild("glow");
		val.AddComponent<UIButton>();
		return val;
	}

	private static GameObject GetChild(this GameObject parent, string childName)
	{
		return ((Component)parent.transform.FindChild(childName)).gameObject;
	}

	private static void DestroyChild(this GameObject parent, string childName)
	{
		object obj;
		if (parent == null)
		{
			obj = null;
		}
		else
		{
			Transform transform = parent.transform;
			if (transform == null)
			{
				obj = null;
			}
			else
			{
				Transform obj2 = transform.FindChild(childName);
				obj = ((obj2 != null) ? ((Component)obj2).gameObject : null);
			}
		}
		GameObject val = (GameObject)obj;
		if ((Object)(object)val != (Object)null)
		{
			Object.DestroyImmediate((Object)(object)val);
		}
	}
}
