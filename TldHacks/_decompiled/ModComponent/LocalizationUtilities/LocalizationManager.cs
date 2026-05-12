using System;
using System.Collections.Generic;
using System.Text;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using Newtonsoft.Json;
using UnityEngine;

namespace LocalizationUtilities;

public static class LocalizationManager
{
	internal static HashSet<LocalizationSet> Localizations { get; private set; } = new HashSet<LocalizationSet>();


	public static void AddLocalizations(LocalizationSet set)
	{
		set.Validate();
		Localizations.Add(set);
	}

	public static void LoadLocalization(TextAsset asset, string path)
	{
		if (path.ToLower().EndsWith(".json", StringComparison.Ordinal))
		{
			LoadJsonLocalization(asset);
		}
		else
		{
			MelonLogger.Warning("Found localization '" + path + "' that could not be loaded.");
		}
	}

	private static string GetText(TextAsset textAsset)
	{
		byte[] array = Il2CppArrayBase<byte>.op_Implicit((Il2CppArrayBase<byte>)(object)textAsset.bytes);
		int num = Array.IndexOf(array, (byte)123);
		if (num < 0)
		{
			throw new ArgumentException("TextAsset has no Json content.", "textAsset");
		}
		return Encoding.UTF8.GetString(new ReadOnlySpan<byte>(array, num, array.Length - num));
	}

	public static bool LoadJsonLocalization(TextAsset textAsset)
	{
		return LoadJsonLocalization(GetText(textAsset));
	}

	public static bool LoadJsonLocalization(string contents)
	{
		if (string.IsNullOrWhiteSpace(contents))
		{
			return false;
		}
		try
		{
			LocalizationJSON localizationJSON = JsonConvert.DeserializeObject<LocalizationJSON>(contents);
			List<LocalizationEntry> list = new List<LocalizationEntry>();
			foreach (KeyValuePair<string, Dictionary<string, string>> item in localizationJSON)
			{
				list.Add(new LocalizationEntry(item.Key, item.Value));
			}
			AddLocalizations(new LocalizationSet(list));
			return true;
		}
		catch (Exception value)
		{
			MelonLogger.Error($"Failed to deserialize: {value}");
		}
		return false;
	}
}
