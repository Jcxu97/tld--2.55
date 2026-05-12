using System;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace ModComponent.API.Modifications;

[RegisterTypeInIl2Cpp(false)]
internal class PlayAkSound : MonoBehaviour
{
	public string SoundName = "";

	public bool PlayOnEnable;

	public void OnEnable()
	{
		if (PlayOnEnable)
		{
			PlaySound();
		}
	}

	public void PlaySound()
	{
		GameAudioManager.Play3DSound(SoundName, ((Component)this).gameObject);
	}

	public PlayAkSound(IntPtr intPtr)
		: base(intPtr)
	{
	}
}
