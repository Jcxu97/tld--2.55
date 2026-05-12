using System;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using UnityEngine;

namespace ModSettings;

internal static class SliderFixPatches
{
	[HarmonyPatch(typeof(Panel_OptionsMenu), "UpdateMenuNavigationGeneric")]
	private static class DisableTimerForSteplessSliderMove
	{
		private static void Postfix(ref int index, List<GameObject> menuItems)
		{
			GameObject obj = menuItems[index];
			ConsoleSlider val = ((obj != null) ? obj.GetComponentInChildren<ConsoleSlider>() : null);
			if (!((Object)(object)val == (Object)null) && !((Object)(object)val.m_Slider == (Object)null) && ((UIProgressBar)val.m_Slider).numberOfSteps <= 1 && GetTimeredMenuInputHorizontal() == 0f)
			{
				MoveSlider(val, GetRawMenuInputHorizontal());
			}
		}
	}

	[HarmonyPatch(typeof(Panel_CustomXPSetup), "DoMainScreenControls")]
	private static class MakeControllersMoveSlidersInCustomSettingsPanel
	{
		private static void Postfix(Panel_CustomXPSetup __instance)
		{
			int customXPSelectedButtonIndex = __instance.m_CustomXPSelectedButtonIndex;
			ConsoleSlider componentInChildren = __instance.m_CustomXPMenuItemOrder[customXPSelectedButtonIndex].GetComponentInChildren<ConsoleSlider>();
			if (Object.op_Implicit((Object)(object)componentInChildren) && Object.op_Implicit((Object)(object)componentInChildren.m_Slider))
			{
				float movement = ((((UIProgressBar)componentInChildren.m_Slider).numberOfSteps <= 1) ? GetRawMenuInputHorizontal() : GetTimeredMenuInputHorizontal());
				MoveSlider(componentInChildren, movement);
			}
		}
	}

	[HarmonyPatch(typeof(ConsoleSlider), "OnIncrease", new Type[] { })]
	private static class UpdateOnIncrease
	{
		private static bool Prefix(ConsoleSlider __instance)
		{
			return PatchSteplessMovement(__instance, 1f);
		}
	}

	[HarmonyPatch(typeof(ConsoleSlider), "OnDecrease", new Type[] { })]
	private static class UpdateOnDecrease
	{
		private static bool Prefix(ConsoleSlider __instance)
		{
			return PatchSteplessMovement(__instance, -1f);
		}
	}

	[HarmonyPatch(typeof(UISlider), "OnStart", new Type[] { })]
	private static class FixSliderForegroundBarColor
	{
		private static void Postfix(UISlider __instance)
		{
			if (!Object.op_Implicit((Object)(object)((Il2CppObjectBase)__instance).TryCast<UIScrollBar>()) && !((Delegate)(object)((UIProgressBar)__instance).onDragFinished == (Delegate)null))
			{
				GameObject gameObject = ((Component)((UIProgressBar)__instance).mFG).gameObject;
				if (!Object.op_Implicit((Object)(object)gameObject.GetComponent<SliderBarDepthFixer>()))
				{
					gameObject.AddComponent<SliderBarDepthFixer>();
				}
			}
		}
	}

	internal class SliderBarDepthFixer : MonoBehaviour
	{
		private const int TARGET_DEPTH = 25;

		static SliderBarDepthFixer()
		{
			ClassInjector.RegisterTypeInIl2Cpp<SliderBarDepthFixer>();
		}

		public SliderBarDepthFixer(IntPtr ptr)
			: base(ptr)
		{
		}

		private void OnEnable()
		{
			((Component)this).gameObject.GetComponentInChildren<UIWidget>().depth = 25;
		}
	}

	private const float MENU_DEADZONE = 0.05f;

	private const float MOVEMENT_SPEED = 0.01f;

	private static bool PatchSteplessMovement(ConsoleSlider consoleSlider, float direction)
	{
		UISlider slider = consoleSlider.m_Slider;
		if (!((Behaviour)slider).enabled || ((UIProgressBar)slider).numberOfSteps >= 2)
		{
			return true;
		}
		float num = direction * 0.01f * Mathf.Abs(GetRawMenuInputHorizontal());
		((UIProgressBar)slider).value = ((UIProgressBar)slider).mValue + num;
		if (EventDelegate.IsValid(consoleSlider.onChange))
		{
			EventDelegate.Execute(consoleSlider.onChange);
		}
		return false;
	}

	private static void MoveSlider(ConsoleSlider slider, float movement)
	{
		if (movement < 0f)
		{
			slider.OnDecrease();
		}
		else if (movement > 0f)
		{
			slider.OnIncrease();
		}
	}

	private static float GetTimeredMenuInputHorizontal()
	{
		return InterfaceManager.GetPanel<Panel_OptionsMenu>().GetGenericSliderMovementHorizontal();
	}

	private static float GetRawMenuInputHorizontal()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		Panel_OptionsMenu panel = InterfaceManager.GetPanel<Panel_OptionsMenu>();
		float menuNavigationDeadzone = InputSystemRewired.m_MenuNavigationDeadzone;
		InputSystemRewired.m_MenuNavigationDeadzone = 0.05f;
		float result = InputManager.GetMenuNavigationPrimary((MonoBehaviour)(object)panel).x + InputManager.GetMenuNavigationSecondary((MonoBehaviour)(object)panel).x;
		InputSystemRewired.m_MenuNavigationDeadzone = menuNavigationDeadzone;
		return result;
	}
}
