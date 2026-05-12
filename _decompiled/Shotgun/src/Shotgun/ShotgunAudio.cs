using System;
using System.IO;
using AudioMgr;
using Il2Cpp;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Shotgun;

internal static class ShotgunAudio
{
	private static AudioClip _clipSlug;
	private static AudioClip _clipBuckshot;
	private static AudioClip _clipBirdshot;
	private static Shot _shot;
	private static bool _ready;

	internal static void Init()
	{
		string dir = Path.Combine(MelonEnvironment.ModsDirectory, "Shotgun");

		var cam = GameManager.GetMainCamera();
		if (cam != null)
		{
			_shot = AudioMaster.CreateShot(cam.gameObject, (AudioMaster.SourceType)0);
			if (_shot != null && _shot._audioSource != null)
			{
				_shot._audioSource.spatialBlend = 0f;
				_shot._audioSource.volume = 1f;
				MelonLogger.Msg("[散弹枪] AudioMaster Shot OK");
			}
			else
				MelonLogger.Error("[散弹枪] AudioMaster Shot 失败");
		}

		_clipSlug = LoadWavBytes(Path.Combine(dir, "slug.wav"), "slug");
		_clipBuckshot = LoadWavBytes(Path.Combine(dir, "buckshot.wav"), "buckshot");
		_clipBirdshot = LoadWavBytes(Path.Combine(dir, "birdshot.wav"), "birdshot");

		_ready = _shot != null && _shot._audioSource != null && (_clipSlug != null || _clipBuckshot != null || _clipBirdshot != null);
		MelonLogger.Msg($"[散弹枪] 音频={_ready}: slug={_clipSlug != null} buck={_clipBuckshot != null} bird={_clipBirdshot != null}");
	}

	internal static void PlayFire()
	{
		if (!_ready) return;
		AudioClip clip = GetClip();
		if (clip == null) return;
		_shot._audioSource.PlayOneShot(clip, 1f);
	}

	private static AudioClip GetClip()
	{
		var pm = GameManager.GetPlayerManagerComponent();
		if (pm == null || pm.m_ItemInHands == null) return _clipBuckshot;
		var gun = pm.m_ItemInHands.m_GunItem;
		if (gun == null || gun.ValidAmmo == null || gun.m_SelectedAmmoIndex < 0) return _clipBuckshot;
		try
		{
			var ammo = gun.ValidAmmo[gun.m_SelectedAmmoIndex];
			if (ammo != null)
			{
				string n = ((Object)ammo).name ?? "";
				if (n.Contains("Slug")) return _clipSlug ?? _clipBuckshot;
				if (n.Contains("Birdshot")) return _clipBirdshot ?? _clipBuckshot;
			}
		}
		catch { }
		return _clipBuckshot;
	}

	private static AudioClip LoadWavBytes(string path, string name)
	{
		if (!File.Exists(path)) { MelonLogger.Warning($"[散弹枪] 缺少: {path}"); return null; }
		try
		{
			byte[] data = File.ReadAllBytes(path);
			if (data.Length < 44) return null;

			int channels = BitConverter.ToInt16(data, 22);
			int sampleRate = BitConverter.ToInt32(data, 24);
			int bitsPerSample = BitConverter.ToInt16(data, 34);

			int dataStart = 44;
			for (int i = 12; i < data.Length - 8; i++)
			{
				if (data[i] == 0x64 && data[i+1] == 0x61 && data[i+2] == 0x74 && data[i+3] == 0x61)
				{ dataStart = i + 8; break; }
			}

			int bytesPerSample = bitsPerSample / 8;
			int totalSamples = (data.Length - dataStart) / bytesPerSample;
			float[] pcm = new float[totalSamples];

			for (int i = 0; i < totalSamples && dataStart + i * 2 + 1 < data.Length; i++)
				pcm[i] = BitConverter.ToInt16(data, dataStart + i * 2) / 32768f;

			AudioClip clip = AudioClip.Create(name, totalSamples / channels, channels, sampleRate, false);
			clip.SetData(pcm, 0);
			MelonLogger.Msg($"[散弹枪] 音频加载: {name} {totalSamples/channels/sampleRate:F1}s");
			return clip;
		}
		catch (Exception e)
		{
			MelonLogger.Error($"[散弹枪] 加载失败 {name}: {e.Message}");
			return null;
		}
	}
}
