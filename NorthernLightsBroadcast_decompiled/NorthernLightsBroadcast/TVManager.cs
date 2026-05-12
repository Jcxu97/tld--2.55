using System;
using System.IO;
using AudioMgr;
using Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using Il2CppTLD.PDID;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

namespace NorthernLightsBroadcast;

[RegisterTypeInIl2Cpp]
public class TVManager : MonoBehaviour
{
	public enum TVState
	{
		Off,
		Static,
		Paused,
		Preparing,
		Error,
		Playing,
		Resume,
		Ended
	}

	public GearItem thisGearItem;

	public ObjectGuid objectGuid;

	public string thisGuid;

	public bool firstStartDone;

	public GameObject screenObject;

	public MeshRenderer objectRenderer;

	public TVPlayer tvplayer;

	public TVUI ui;

	public TVButton redbutton;

	public GameObject dummyCamera;

	public VideoPlayer videoPlayer;

	public bool isSetup;

	public bool isCRT;

	public string errorText;

	public Light ambilight;

	public Setting audioSetting;

	public double saveTime;

	public Shot playerAudio;

	public Shot staticAudio;

	public GraphicRaycaster graphicsRaycaster;

	public PointerEventData pointerEventData;

	public TVState currentState;

	public TVManager(IntPtr intPtr)
		: base(intPtr)
	{
	}

	public void Awake()
	{
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		if (isSetup)
		{
			return;
		}
		if (((Object)((Component)this).gameObject).name.Contains("OBJ_TelevisionB_LOD0") || ((Object)((Component)this).gameObject).name.Contains("GEAR_TV_CRT"))
		{
			screenObject = Object.Instantiate<GameObject>(NorthernLightsBroadcastMain.NLB_TV_CRT, ((Component)this).transform);
		}
		else if (((Object)((Component)this).gameObject).name.Contains("OBJ_Television_LOD0") || ((Object)((Component)this).gameObject).name.Contains("GEAR_TV_LCD"))
		{
			screenObject = Object.Instantiate<GameObject>(NorthernLightsBroadcastMain.NLB_TV_LCD, ((Component)this).transform);
		}
		else if (((Object)((Component)this).gameObject).name.Contains("OBJ_Television") || ((Object)((Component)this).gameObject).name.Contains("GEAR_TV_WALL"))
		{
			screenObject = Object.Instantiate<GameObject>(NorthernLightsBroadcastMain.NLB_TV_WALL, ((Component)this).transform);
		}
		((Object)screenObject).name = "NLB_TV";
		objectRenderer = ((Component)this).gameObject.GetComponent<MeshRenderer>();
		if (((Object)((Component)this).gameObject).name.Contains("GEAR_TV_CRT"))
		{
			isCRT = true;
			MelonLogger.Msg("CRT TV found");
			((Renderer)objectRenderer).sharedMaterial = NorthernLightsBroadcastMain.TelevisionB_Material_Cutout;
		}
		redbutton = ((Component)screenObject.transform.Find("PowerButton")).gameObject.AddComponent<TVButton>();
		redbutton.manager = this;
		ambilight = ((Component)screenObject.transform.Find("Ambilight")).gameObject.GetComponent<Light>();
		((Behaviour)ambilight).enabled = false;
		dummyCamera = ((Component)screenObject.transform.Find("CameraDummy")).gameObject;
		dummyCamera.transform.localPosition = dummyCamera.transform.localPosition + new Vector3(0f, 0f, 0.32f);
		CreateAudioSetting();
		staticAudio = AudioMaster.CreateShot(((Component)this).gameObject, (SourceType)4);
		staticAudio.AssignClip(NorthernLightsBroadcastMain.tvAudioManager.GetClip("static"));
		staticAudio._audioSource.loop = true;
		staticAudio.Stop();
		staticAudio.ApplySettings(audioSetting);
		playerAudio = AudioMaster.CreateShot(((Component)this).gameObject, (SourceType)4);
		playerAudio.ApplySettings(audioSetting);
		graphicsRaycaster = ((Component)screenObject.transform.Find("OSD")).gameObject.GetComponent<GraphicRaycaster>();
		videoPlayer = screenObject.GetComponent<VideoPlayer>();
		tvplayer = ((Component)this).gameObject.AddComponent<TVPlayer>();
		ui = ((Component)this).gameObject.AddComponent<TVUI>();
		thisGearItem = ((Component)this).gameObject.GetComponent<GearItem>();
		objectGuid = ((Component)this).gameObject.GetComponent<ObjectGuid>();
		if ((Object)(object)objectGuid != (Object)null)
		{
			thisGuid = ((PdidObjectBase)objectGuid).GetPDID();
		}
		string folder = SaveLoad.GetFolder(thisGuid);
		if (folder != null && Directory.Exists(folder))
		{
			ui.currentFolder = folder;
		}
		string lastPlayed = SaveLoad.GetLastPlayed(thisGuid);
		if (lastPlayed != null && File.Exists(lastPlayed))
		{
			ui.currentClip = lastPlayed;
		}
		if (((Object)((Component)this).gameObject).name.Contains("GEAR_TV_LCD") || ((Object)((Component)this).gameObject).name.Contains("GEAR_TV_CRT") || ((Object)((Component)this).gameObject).name.Contains("GEAR_TV_WALL"))
		{
			if ((Object)(object)objectGuid == (Object)null)
			{
				objectGuid = ((Component)this).gameObject.AddComponent<ObjectGuid>();
				thisGuid = ((PdidObjectBase)objectGuid).GetPDID();
				if (thisGuid == null)
				{
					objectGuid.MaybeRuntimeRegister();
					thisGuid = ((PdidObjectBase)objectGuid).GetPDID();
				}
			}
			else
			{
				thisGuid = ((PdidObjectBase)objectGuid).GetPDID();
			}
		}
		isSetup = true;
		if (SaveLoad.GetState(thisGuid) == TVState.Playing)
		{
			ui.Prepare();
		}
		else
		{
			SwitchState(SaveLoad.GetState(thisGuid));
		}
	}

	public void Update()
	{
		if (isSetup && currentState == TVState.Playing)
		{
			saveTime = videoPlayer.time;
			if (ui.OSDOpen && TVLock.lockedInTVView)
			{
				UpdateTimeText();
				UpdateTimeSlider();
			}
		}
	}

	public void UpdateTimeText()
	{
		string text = TimeSpan.FromSeconds(videoPlayer.time).ToString("hh\\:mm\\:ss");
		((TMP_Text)ui.timeText).text = text;
	}

	public void UpdateTimeSlider()
	{
		ui.progressBar.value = videoPlayer.frame;
		ui.progressBar.minValue = 1f;
		ui.progressBar.maxValue = videoPlayer.frameCount;
	}

	[HideFromIl2Cpp]
	public void SavePlaytime()
	{
		if (ui.currentClip != null)
		{
			FileStuff.AddFrameValueToFile(ui.currentClip, saveTime);
			FileStuff.SaveFrameFile();
		}
	}

	[HideFromIl2Cpp]
	public double GetPlayTime()
	{
		double num = FileStuff.GetFrameValueFromFile(ui.currentClip);
		if (num <= 0.0 || num + 1.0 >= videoPlayer.length)
		{
			num = 0.0;
		}
		return num;
	}

	[HideFromIl2Cpp]
	public void SwitchState(TVState newState)
	{
		currentState = newState;
		switch (newState)
		{
		case TVState.Off:
			ui.screenOff.SetActive(true);
			ui.screenPlayback.SetActive(false);
			ui.screenStatic.SetActive(false);
			ui.screenError.SetActive(false);
			ui.screenLoading.SetActive(false);
			ui.osdAudio.SetActive(false);
			ui.osdButtons.SetActive(false);
			ui.osdFileMenu.SetActive(false);
			ui.ActivateOSD(value: false);
			videoPlayer.Stop();
			staticAudio.Stop();
			((Behaviour)ambilight).enabled = false;
			redbutton.Glow(enabled: false);
			break;
		case TVState.Static:
			ui.screenOff.SetActive(false);
			ui.screenPlayback.SetActive(false);
			ui.screenStatic.SetActive(true);
			ui.screenError.SetActive(false);
			ui.screenLoading.SetActive(false);
			((Component)ui.playButton).gameObject.SetActive(true);
			((Component)ui.pauseButton).gameObject.SetActive(false);
			videoPlayer.Stop();
			staticAudio.Play();
			((TMP_Text)ui.playingNowText).text = "Stopped";
			redbutton.Glow(enabled: true);
			((Behaviour)ambilight).enabled = false;
			break;
		case TVState.Paused:
			ui.screenOff.SetActive(false);
			ui.screenStatic.SetActive(false);
			ui.screenError.SetActive(false);
			ui.screenLoading.SetActive(false);
			ui.screenPlayback.SetActive(true);
			((TMP_Text)ui.playingNowText).text = "Paused";
			((Component)ui.playButton).gameObject.SetActive(true);
			((Component)ui.pauseButton).gameObject.SetActive(false);
			videoPlayer.Pause();
			redbutton.Glow(enabled: true);
			staticAudio.Stop();
			((Behaviour)ambilight).enabled = false;
			break;
		case TVState.Playing:
			ui.screenOff.SetActive(false);
			ui.screenStatic.SetActive(false);
			ui.screenError.SetActive(false);
			ui.screenLoading.SetActive(false);
			ui.screenPlayback.SetActive(true);
			((Component)ui.playButton).gameObject.SetActive(false);
			((Component)ui.pauseButton).gameObject.SetActive(true);
			ui.ActivateOSD(value: false);
			staticAudio.Stop();
			redbutton.Glow(enabled: true);
			videoPlayer.time = FileStuff.GetFrameValueFromFile(ui.currentClip);
			videoPlayer.Play();
			((Behaviour)ambilight).enabled = false;
			SaveLoad.SetLastPlayed(thisGuid, ui.currentClip);
			break;
		case TVState.Error:
			((TMP_Text)ui.errorText).text = errorText;
			ui.screenOff.SetActive(false);
			ui.screenStatic.SetActive(false);
			ui.screenError.SetActive(true);
			ui.screenLoading.SetActive(false);
			ui.osdAudio.SetActive(false);
			ui.osdButtons.SetActive(false);
			ui.osdFileMenu.SetActive(false);
			redbutton.Glow(enabled: true);
			((TMP_Text)ui.playingNowText).text = "Error";
			staticAudio.Play();
			videoPlayer.Stop();
			((Behaviour)ambilight).enabled = false;
			break;
		case TVState.Preparing:
			ui.screenOff.SetActive(false);
			ui.screenStatic.SetActive(true);
			ui.screenError.SetActive(false);
			ui.screenLoading.SetActive(true);
			ui.screenPlayback.SetActive(true);
			redbutton.Glow(enabled: true);
			((TMP_Text)ui.playingNowText).text = "Loading";
			ui.osdAudio.SetActive(false);
			ui.osdButtons.SetActive(false);
			ui.osdFileMenu.SetActive(false);
			((Component)ui.playButton).gameObject.SetActive(true);
			((Component)ui.pauseButton).gameObject.SetActive(false);
			staticAudio.Stop();
			videoPlayer.Prepare();
			((Behaviour)ambilight).enabled = false;
			break;
		case TVState.Resume:
			videoPlayer.Play();
			redbutton.Glow(enabled: true);
			SwitchState(TVState.Playing);
			break;
		case TVState.Ended:
			videoPlayer.Stop();
			redbutton.Glow(enabled: true);
			saveTime = 0.0;
			SavePlaytime();
			if (Settings.options.playFolder)
			{
				ui.NextClip();
			}
			else
			{
				SwitchState(TVState.Static);
			}
			break;
		}
	}

	public void OnDestroy()
	{
		SavePlaytime();
		SaveLoad.SetState(thisGuid, currentState);
	}

	[HideFromIl2Cpp]
	private void CreateAudioSetting()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		audioSetting = new Setting((SourceType)4);
		audioSetting.spread = 20f;
		audioSetting.panStereo = 0f;
		audioSetting.dopplerLevel = 0.1f;
		audioSetting.maxDistance = 11f;
		audioSetting.minDistance = 0.01f;
		audioSetting.pitch = 1f;
		audioSetting.spatialBlend = 1f;
		audioSetting.rolloffFactor = 1.8f;
		audioSetting.spatialize = true;
		audioSetting.rolloffMode = (AudioRolloffMode)1;
		audioSetting.priority = 128;
		audioSetting.maxVolume = 0.5f;
		audioSetting.minVolume = 0.1f;
	}
}
