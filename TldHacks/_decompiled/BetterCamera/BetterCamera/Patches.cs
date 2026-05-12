using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace BetterCamera;

internal static class Patches
{
	[HarmonyPatch(typeof(PhotoManager), "SetPhotoTexture")]
	public static class PicturePatch
	{
		public static void Postfix(ref PhotoManager __instance)
		{
			GameManager.m_vpFPSCamera.m_UnzoomedFieldOfView = BetterCameraMelon.cameraFOVBeforeAim;
			BetterCameraMelon.canSavePicture = true;
			if (Settings.instance.popups)
			{
				HUDMessage.AddMessage("Press '" + ((object)(KeyCode)(ref Settings.instance.keyCode)).ToString() + "' to save photo.", true, true);
			}
		}
	}

	[HarmonyPatch(typeof(GunItem), "Awake")]
	public static class CameraStatsPatch
	{
		public static void Postfix(ref GunItem __instance)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Invalid comparison between Unknown and I4
			if ((int)__instance.m_GunType == 3)
			{
				__instance.m_SupportsUnload = Settings.instance.unloading;
				__instance.m_ClipSize = Settings.instance.clipsize;
				__instance.m_RoundsToReloadPerClip = Settings.instance.clipsize;
				if (Settings.instance.tooltip)
				{
					__instance.m_FireButtonLabel = "拍照";
				}
				__instance.m_MultiplierAiming = Settings.instance.aimspeed;
			}
		}
	}

	[HarmonyPatch(typeof(GunItem), "ZoomEnd")]
	public static class CameraStopAimPatch
	{
		public static void Postfix(ref GunItem __instance)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Invalid comparison between Unknown and I4
			if ((int)__instance.m_GunType == 3)
			{
				GameManager.m_vpFPSCamera.m_UnzoomedFieldOfView = BetterCameraMelon.cameraFOVBeforeAim;
			}
		}
	}
}
