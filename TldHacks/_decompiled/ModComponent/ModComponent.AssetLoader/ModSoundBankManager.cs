using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Il2Cpp;
using MelonLoader.Utils;

namespace ModComponent.AssetLoader;

internal static class ModSoundBankManager
{
	private const int MEMORY_ALIGNMENT = 16;

	private static bool DelayLoadingSoundBanks = true;

	private static List<string> pendingPaths = new List<string>();

	private static List<byte[]> pendingBytes = new List<byte[]>();

	public static void RegisterSoundBank(string relativePath)
	{
		string text = Path.Combine(MelonEnvironment.ModsDirectory, relativePath);
		if (!File.Exists(text))
		{
			throw new FileNotFoundException("Sound bank '" + relativePath + "' could not be found at '" + text + "'.");
		}
		if (DelayLoadingSoundBanks)
		{
			Logger.Log("Adding sound bank '" + relativePath + "' to the list of pending sound banks.");
			pendingPaths.Add(text);
		}
		else
		{
			LoadSoundBank(text);
		}
	}

	public static void RegisterSoundBank(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("Data for sound bank was null");
		}
		if (DelayLoadingSoundBanks)
		{
			Logger.Log("Adding sound bank to the list of pending sound banks.");
			pendingBytes.Add(data);
		}
		else
		{
			LoadSoundBank(data);
		}
	}

	internal static void RegisterPendingSoundBanks()
	{
		Logger.Log("Registering pending sound banks.");
		DelayLoadingSoundBanks = false;
		foreach (string pendingPath in pendingPaths)
		{
			LoadSoundBank(pendingPath);
		}
		pendingPaths.Clear();
		foreach (byte[] pendingByte in pendingBytes)
		{
			LoadSoundBank(pendingByte);
		}
		pendingBytes.Clear();
	}

	private static void LoadSoundBank(string soundBankPath)
	{
		LoadSoundBank(File.ReadAllBytes(soundBankPath), soundBankPath);
	}

	private static void LoadSoundBank(byte[] data, string? soundBankPath = null)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Invalid comparison between Unknown and I4
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		IntPtr hglobal = Marshal.AllocHGlobal(data.Length + 16 - 1);
		IntPtr intPtr = new IntPtr((hglobal.ToInt64() + 16 - 1) / 16 * 16);
		Marshal.Copy(data, 0, intPtr, data.Length);
		uint num = default(uint);
		AKRESULT val = AkSoundEngine.LoadBankMemoryCopy(intPtr, (uint)data.Length, ref num);
		if ((int)val != 1)
		{
			if (string.IsNullOrEmpty(soundBankPath))
			{
				Logger.Log("Failed to load sound bank.");
			}
			else
			{
				Logger.Log("Failed to load sound bank from: '" + soundBankPath + "'");
			}
			Logger.Log($"Result was {val}.");
			Marshal.FreeHGlobal(hglobal);
		}
	}
}
