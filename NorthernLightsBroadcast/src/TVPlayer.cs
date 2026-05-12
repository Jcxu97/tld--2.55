using System;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppSystem;
using MelonLoader;
using UnityEngine;
using UnityEngine.Video;

namespace NorthernLightsBroadcast;

[RegisterTypeInIl2Cpp]
public class TVPlayer : MonoBehaviour
{
	public TVManager manager;

	public bool isSetup;

	public TVPlayer(System.IntPtr intPtr)
		: base(intPtr)
	{
	}

	public void Awake()
	{
		if (!isSetup)
		{
			manager = ((Component)this).GetComponent<TVManager>();
			var vp = manager.videoPlayer;
			vp.audioOutputMode = (VideoAudioOutputMode)0; // None - audio handled by WasapiOut
			vp.targetCameraAlpha = 1f;
			vp.isLooping = false;
			vp.aspectRatio = (VideoAspectRatio)3;
			vp.loopPointReached += (System.Action<VideoPlayer>)PlaybackEnd;
			vp.started += (System.Action<VideoPlayer>)PlaybackStarted;
			vp.prepareCompleted += (System.Action<VideoPlayer>)PrepareCompleted;
			vp.errorReceived += (System.Action<VideoPlayer, string>)Error;
			isSetup = true;
		}
	}

	public void Error(VideoPlayer source, string message)
	{
		MelonLogger.Msg("[NLB] Error on videoplayback -> " + message);
		manager.errorText = message;
		manager.SwitchState(TVManager.TVState.Error);
	}

	public void PlaybackStarted(VideoPlayer source)
	{
		if (source.frameCount > 0 && manager.ui.progressBar != null)
		{
			manager.ui.progressBar.maxValue = (float)source.frameCount;
		}
	}

	public void PrepareCompleted(VideoPlayer source)
	{
		if (source.frameCount > 0 && manager.ui.progressBar != null)
		{
			manager.ui.progressBar.maxValue = (float)source.frameCount;
		}
		manager.SwitchState(TVManager.TVState.Playing);
	}

	public void PlaybackEnd(VideoPlayer source)
	{
		manager.SwitchState(TVManager.TVState.Ended);
	}
}
