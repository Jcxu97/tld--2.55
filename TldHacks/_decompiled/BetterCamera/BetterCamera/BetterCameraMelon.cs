using System;
using System.IO;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace BetterCamera;

public class BetterCameraMelon : MelonMod
{
	public static bool canSavePicture;

	public static float cameraFOVBeforeAim;

	public static PlayerManager pm;

	public static vp_FPSCamera cam;

	public override void OnInitializeMelon()
	{
		Settings.instance.AddToModSettings("照相机模组v1.5.1");
	}

	public override void OnSceneWasInitialized(int buildIndex, string sceneName)
	{
		pm = GameManager.GetPlayerManagerComponent();
		cam = GameManager.m_vpFPSCamera;
		canSavePicture = false;
	}

	public override void OnUpdate()
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Invalid comparison between Unknown and I4
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Invalid comparison between Unknown and I4
		if (!GameManager.IsMainMenuActive() && (Object)(object)pm != (Object)null && !pm.PlayerIsZooming() && Settings.instance.dynazoom && (Object)(object)cam != (Object)null)
		{
			cameraFOVBeforeAim = cam.m_UnzoomedFieldOfView;
		}
		if (!GameManager.IsMainMenuActive() && (Object)(object)InputManager.instance != (Object)null && InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.instance.keyCode) && canSavePicture)
		{
			Texture2D photoTexture = GameManager.GetPhotoManager().PhotoTexture;
			string text = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".png";
			if ((Object)(object)photoTexture != (Object)null)
			{
				if (!Directory.Exists("Mods/SavedPhotos"))
				{
					Directory.CreateDirectory("Mods/SavedPhotos");
				}
				photoTexture.Save("Mods/SavedPhotos/" + text);
				if (Settings.instance.melonlogs)
				{
					MelonLogger.Msg("Photo saved as: " + text + " in Mods folder.");
				}
				if (Settings.instance.popups)
				{
					InterfaceManager.GetPanel<Panel_Subtitles>().ShowSubtitlesForced("Photo saved as: " + text + " in 'SavedPhotos' folder inside Mods folder.", 4f);
				}
				canSavePicture = false;
			}
			else
			{
				if (Settings.instance.popups)
				{
					InterfaceManager.GetPanel<Panel_Subtitles>().ShowSubtitlesForced("Failed to save photo.", 4f);
				}
				if (Settings.instance.melonlogs)
				{
					MelonLogger.Error("Could not encode camera photo: No Image Found.");
				}
				canSavePicture = false;
			}
		}
		if (!Settings.instance.dynazoom)
		{
			return;
		}
		if (!GameManager.IsMainMenuActive() && (Object)(object)InputManager.instance != (Object)null && (InputManager.GetScroll(InputManager.m_CurrentContext) > 0f || InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.instance.zoomin)) && (Object)(object)pm != (Object)null && pm.PlayerIsZooming() && (int)GameManager.m_vpFPSCamera.CurrentWeapon.m_GunItem.m_GunType == 3 && (Object)(object)cam != (Object)null)
		{
			cam.m_UnzoomedFieldOfView = Math.Clamp(cam.m_UnzoomedFieldOfView - 1.5f, 2f, 87f);
			if (Settings.instance.scrollsound)
			{
				GameAudioManager.PlayGUIScroll();
			}
		}
		if (!GameManager.IsMainMenuActive() && (Object)(object)InputManager.instance != (Object)null && (InputManager.GetScroll(InputManager.m_CurrentContext) < 0f || InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.instance.zoomout)) && (Object)(object)pm != (Object)null && pm.PlayerIsZooming() && (int)GameManager.m_vpFPSCamera.CurrentWeapon.m_GunItem.m_GunType == 3 && (Object)(object)cam != (Object)null)
		{
			cam.m_UnzoomedFieldOfView = Math.Clamp(cam.m_UnzoomedFieldOfView + 1.5f, 2f, 87f);
			if (Settings.instance.scrollsound)
			{
				GameAudioManager.PlayGUIScroll();
			}
		}
	}

	static BetterCameraMelon()
	{
		canSavePicture = false;
		cameraFOVBeforeAim = 60f;
	}
}
