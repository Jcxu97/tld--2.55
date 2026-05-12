using System;
using Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace ModSettings;

internal class CustomKeybinding : MonoBehaviour
{
	internal KeyCode currentKeycodeSetting;

	internal KeyRebindingButton keyRebindingButton;

	internal Action OnChange;

	private bool searchingForKey;

	private bool ignoreNextOnClick;

	static CustomKeybinding()
	{
		ClassInjector.RegisterTypeInIl2Cpp<CustomKeybinding>();
	}

	public CustomKeybinding(IntPtr intPtr)
		: base(intPtr)
	{
	}

	public void Update()
	{
		if (searchingForKey)
		{
			MaybeUpdateKey();
		}
		if (((Component)this).gameObject.activeSelf && !searchingForKey && keyRebindingButton.m_ValueLabel.text != currentKeycodeSetting.ToString())
		{
			RefreshLabelValue();
		}
	}

	[HideFromIl2Cpp]
	internal void OnClick()
	{
		if (ignoreNextOnClick)
		{
			ignoreNextOnClick = false;
			return;
		}
		searchingForKey = true;
		keyRebindingButton.SetSelected(true);
		keyRebindingButton.SetValueLabel(string.Empty);
		GameAudioManager.PlayGUIButtonClick();
	}

	[HideFromIl2Cpp]
	internal void RefreshLabelValue()
	{
		keyRebindingButton.SetValueLabel(currentKeycodeSetting.ToString());
	}

	[HideFromIl2Cpp]
	private void MaybeUpdateKey()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Invalid comparison between Unknown and I4
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Invalid comparison between Unknown and I4
		KeyCode? pressedKeyCode = GetPressedKeyCode();
		if (pressedKeyCode.HasValue)
		{
			currentKeycodeSetting = pressedKeyCode.Value;
			searchingForKey = false;
			ignoreNextOnClick = (int)currentKeycodeSetting >= 323 && (int)currentKeycodeSetting <= 329;
			keyRebindingButton.SetSelected(false);
			keyRebindingButton.SetValueLabel(pressedKeyCode.ToString());
			OnChange();
		}
	}

	[HideFromIl2Cpp]
	private KeyCode? GetPressedKeyCode()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (InputManager.GetKeyDown(InputManager.m_CurrentContext, (KeyCode)8))
		{
			return (KeyCode)0;
		}
		foreach (KeyCode value in Enum.GetValues(typeof(KeyCode)))
		{
			if (InputManager.GetKeyDown(InputManager.m_CurrentContext, value))
			{
				return value;
			}
		}
		return null;
	}
}
