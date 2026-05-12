using System;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppTLD.ModularElectrolizer;
using MelonLoader;
using UnityEngine;

namespace HouseLights;

internal class Patches
{
	[HarmonyPatch(typeof(GameManager), "InstantiatePlayerObject")]
	internal class GameManager_InstantiatePlayerObject
	{
		public static void Prefix()
		{
			if (!HouseLightsUtils.IsMenu() && !InterfaceManager.IsMainMenuEnabled() && (!GameManager.IsOutDoorsScene(GameManager.m_ActiveScene) || Settings.options.enableOutside || HouseLights.notReallyOutdoors.Contains(GameManager.m_ActiveScene)))
			{
				if (Settings.options.Debug)
				{
					MelonLogger.Msg("Scene Init");
				}
				HouseLights.InstantiateCustomSwitches(GameManager.m_ActiveScene);
				HouseLights.Init();
				HouseLights.GetSwitches();
			}
		}
	}

	[HarmonyPatch(typeof(AuroraModularElectrolizer), "Initialize")]
	internal class AuroraElectrolizer_Initialize
	{
		private static void Postfix(AuroraModularElectrolizer __instance)
		{
			if (!HouseLightsUtils.IsMenu() && !InterfaceManager.IsMainMenuEnabled() && (!GameManager.IsOutDoorsScene(GameManager.m_ActiveScene) || HouseLights.notReallyOutdoors.Contains(GameManager.m_ActiveScene) || Settings.options.enableOutside))
			{
				AuroraActivatedToggle[] array = Il2CppArrayBase<AuroraActivatedToggle>.op_Implicit(((Component)__instance).gameObject.GetComponentsInParent<AuroraActivatedToggle>());
				AuroraScreenDisplay[] array2 = Il2CppArrayBase<AuroraScreenDisplay>.op_Implicit(((Component)__instance).gameObject.GetComponentsInChildren<AuroraScreenDisplay>());
				if (array.Length == 0 && array2.Length == 0)
				{
					HouseLights.AddElectrolizer(__instance);
				}
				__instance.m_HasFlickerSet = !Settings.options.disableAuroraFlicker;
			}
		}
	}

	[HarmonyPatch(typeof(AuroraManager), "RegisterAuroraLightSimple", new Type[] { typeof(AuroraLightingSimple) })]
	internal class AuroraManager_RegisterLightSimple
	{
		private static void Postfix(AuroraManager __instance, AuroraLightingSimple auroraLightSimple)
		{
			if (!HouseLightsUtils.IsMenu() && !InterfaceManager.IsMainMenuEnabled() && (!GameManager.IsOutDoorsScene(GameManager.m_ActiveScene) || HouseLights.notReallyOutdoors.Contains(GameManager.m_ActiveScene) || Settings.options.enableOutside))
			{
				HouseLights.AddElectrolizerLight(auroraLightSimple);
			}
		}
	}

	[HarmonyPatch(typeof(AuroraManager), "UpdateForceAurora")]
	internal class AuroraManager_UpdateForceAurora
	{
		private static void Postfix(AuroraManager __instance)
		{
			if (!HouseLightsUtils.IsMenu() && !InterfaceManager.IsMainMenuEnabled() && (!GameManager.IsOutDoorsScene(GameManager.m_ActiveScene) || HouseLights.notReallyOutdoors.Contains(GameManager.m_ActiveScene) || Settings.options.enableOutside) && HouseLights.electroSources.Count > 0)
			{
				HouseLights.UpdateElectroLights(__instance);
			}
		}
	}

	[HarmonyPatch(typeof(PlayerManager), "UpdateHUDText", new Type[] { typeof(Panel_HUD) })]
	internal class PlayerManage_UpdateHUDText
	{
		private static void Postfix(PlayerManager __instance, Panel_HUD hud)
		{
			if (!HouseLightsUtils.IsMenu() && !((Object)(object)GameManager.GetMainCamera() == (Object)null))
			{
				GameObject interactiveObjectUnderCrosshairs = __instance.GetInteractiveObjectUnderCrosshairs((float)Settings.options.InteractDistance);
				if ((Object)(object)interactiveObjectUnderCrosshairs != (Object)null && ((Object)interactiveObjectUnderCrosshairs).name == "MOD_HouseLightSwitch")
				{
					string text = ((!HouseLights.lightsOn) ? "Turn Lights On" : "Turn Lights Off");
					hud.SetHoverText(text, interactiveObjectUnderCrosshairs, (HoverTextState)3);
				}
			}
		}
	}

	[HarmonyPatch(typeof(PlayerManager), "InteractiveObjectsProcessInteraction")]
	internal class PlayerManager_InteractiveObjectsProcessInteraction
	{
		private static void Postfix(PlayerManager __instance, ref bool __result)
		{
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			if (!HouseLightsUtils.IsMenu())
			{
				GameObject interactiveObjectUnderCrosshairs = __instance.GetInteractiveObjectUnderCrosshairs((float)Settings.options.InteractDistance);
				if ((Object)(object)interactiveObjectUnderCrosshairs != (Object)null && ((Object)interactiveObjectUnderCrosshairs).name == "MOD_HouseLightSwitch")
				{
					HouseLights.ToggleLightsState();
					GameAudioManager.PlaySound("Stop_RadioAurora", ((Component)__instance).gameObject);
					float x = interactiveObjectUnderCrosshairs.transform.localScale.x;
					float y = interactiveObjectUnderCrosshairs.transform.localScale.y;
					float z = interactiveObjectUnderCrosshairs.transform.localScale.z;
					interactiveObjectUnderCrosshairs.transform.localScale = new Vector3(x, y * -1f, z);
					__result = true;
				}
			}
		}
	}

	[HarmonyPatch(typeof(Weather), "IsTooDarkForAction", new Type[] { typeof(ActionsToBlock) })]
	internal class Weather_IsTooDarkForAction
	{
		private static void Postfix(Weather __instance, ref bool __result)
		{
			if (!HouseLightsUtils.IsMenu() && __result && GameManager.GetWeatherComponent().IsIndoorScene() && HouseLights.lightsOn)
			{
				__result = false;
			}
		}
	}
}
