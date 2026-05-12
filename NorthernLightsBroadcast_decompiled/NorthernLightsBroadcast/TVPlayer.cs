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

	public TVPlayer(IntPtr intPtr)
		: base(intPtr)
	{
	}

	public void Awake()
	{
		if (!isSetup)
		{
			manager = ((Component)this).GetComponent<TVManager>();
			manager.videoPlayer.audioOutputMode = (VideoAudioOutputMode)1;
			manager.videoPlayer.SetTargetAudioSource((ushort)0, manager.playerAudio._audioSource);
			manager.videoPlayer.targetCameraAlpha = 1f;
			manager.videoPlayer.isLooping = false;
			manager.videoPlayer.aspectRatio = (VideoAspectRatio)3;
			manager.videoPlayer.loopPointReached = (((Delegate)(object)manager.videoPlayer.loopPointReached == (Delegate)null) ? EventHandler.op_Implicit((Action<VideoPlayer>)PlaybackEnd) : ((Il2CppObjectBase)Delegate.Combine((Delegate)(object)manager.videoPlayer.loopPointReached, (Delegate)(object)Action<VideoPlayer>.op_Implicit((Action<VideoPlayer>)PlaybackEnd))).Cast<EventHandler>());
			manager.videoPlayer.started = (((Delegate)(object)manager.videoPlayer.started == (Delegate)null) ? EventHandler.op_Implicit((Action<VideoPlayer>)PlaybackStarted) : ((Il2CppObjectBase)Delegate.Combine((Delegate)(object)manager.videoPlayer.started, (Delegate)(object)Action<VideoPlayer>.op_Implicit((Action<VideoPlayer>)PlaybackStarted))).Cast<EventHandler>());
			manager.videoPlayer.prepareCompleted = (((Delegate)(object)manager.videoPlayer.prepareCompleted == (Delegate)null) ? EventHandler.op_Implicit((Action<VideoPlayer>)PrepareCompleted) : ((Il2CppObjectBase)Delegate.Combine((Delegate)(object)manager.videoPlayer.prepareCompleted, (Delegate)(object)Action<VideoPlayer>.op_Implicit((Action<VideoPlayer>)PrepareCompleted))).Cast<EventHandler>());
			manager.videoPlayer.errorReceived = (((Delegate)(object)manager.videoPlayer.errorReceived == (Delegate)null) ? ErrorEventHandler.op_Implicit((Action<VideoPlayer, string>)Error) : ((Il2CppObjectBase)Delegate.Combine((Delegate)(object)manager.videoPlayer.errorReceived, (Delegate)(object)Action<VideoPlayer, string>.op_Implicit((Action<VideoPlayer, string>)Error))).Cast<ErrorEventHandler>());
			isSetup = true;
		}
	}

	public void Error(VideoPlayer source, string message)
	{
		MelonLogger.Msg("Error on videoplayback -> " + message);
		manager.errorText = message;
		manager.SwitchState(TVManager.TVState.Error);
	}

	public void PlaybackStarted(VideoPlayer source)
	{
		manager.SwitchState(TVManager.TVState.Playing);
	}

	public void PrepareCompleted(VideoPlayer source)
	{
		manager.SwitchState(TVManager.TVState.Playing);
	}

	public void PlaybackEnd(VideoPlayer source)
	{
		manager.SwitchState(TVManager.TVState.Ended);
	}
}
