using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using LocalizationUtilities;
using MelonLoader.Utils;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.AssetLoader;

internal static class ModAssetBundleManager
{
	private const string ASSET_NAME_LOCALIZATION = "localization";

	private const string ASSET_NAME_PREFIX_GEAR = "gear_";

	private const string ASSET_NAME_SUFFIX = "atlas";

	private const string ASSET_PATH_SUFFIX_PREFAB = ".prefab";

	private static readonly string[] RESOURCE_FOLDER = new string[6] { "assets/", "logimages/", "clothingpaperdoll/female/", "clothingpaperdoll/male/", "inventorygridicons/", "craftingicons/" };

	private static readonly Dictionary<string, AssetBundle> knownAssetBundles = new Dictionary<string, AssetBundle>();

	private static readonly Dictionary<string, string> knownAssetMappedNames = new Dictionary<string, string>();

	private static readonly Dictionary<string, AssetBundle> knownAssetNames = new Dictionary<string, AssetBundle>();

	internal static AssetBundle GetAssetBundle(string relativePath)
	{
		knownAssetBundles.TryGetValue(relativePath, out AssetBundle value);
		return value;
	}

	internal static bool IsKnownAsset(string? name)
	{
		return !string.IsNullOrEmpty(GetFullAssetName(name));
	}

	internal static Object LoadAsset(string name)
	{
		string fullAssetName = GetFullAssetName(name);
		if (knownAssetNames.TryGetValue(fullAssetName, out AssetBundle value))
		{
			return AssetBundleUtils.LoadAsset(value, fullAssetName);
		}
		throw new Exception("Unknown asset " + name + ". Did you forget to register an AssetBundle?");
	}

	internal static void RegisterAssetBundle(string relativePath)
	{
		if (string.IsNullOrEmpty(relativePath))
		{
			throw new ArgumentException("The relative path while registering an asset bundle was null or empty");
		}
		if (knownAssetBundles.ContainsKey(relativePath))
		{
			Logger.Log("AssetBundle '" + relativePath + "' has already been registered.");
			return;
		}
		string text = Path.Combine(MelonEnvironment.ModsDirectory, relativePath);
		if (File.Exists(text))
		{
			LoadAssetBundle(relativePath, text);
			return;
		}
		throw new FileNotFoundException("AssetBundle '" + relativePath + "' could not be found at '" + text + "'.");
	}

	internal static void RegisterAssetBundle(string relativePath, AssetBundle assetBundle)
	{
		if (string.IsNullOrEmpty(relativePath))
		{
			throw new ArgumentException("The relative path while registering an asset bundle was null or empty");
		}
		if (knownAssetBundles.ContainsKey(relativePath))
		{
			Logger.Log("AssetBundle '" + relativePath + "' has already been registered.");
			return;
		}
		if ((Object)(object)assetBundle == (Object)null)
		{
			throw new ArgumentNullException("Asset bundle '" + relativePath + "' was null");
		}
		LoadAssetBundle(relativePath, assetBundle);
	}

	internal static string GetAssetMappedName(string assetPath, string assetName)
	{
		if (assetName.StartsWith("gear_") && assetPath.EndsWith(".prefab"))
		{
			return assetName;
		}
		string assetPath2 = assetPath;
		assetPath2 = StripResourceFolder(assetPath2);
		int num = assetPath2.LastIndexOf(assetName);
		if (num != -1)
		{
			assetPath2 = assetPath2.Substring(0, num + assetName.Length);
		}
		return assetPath2;
	}

	internal static string GetAssetName(string assetPath)
	{
		return GetAssetName(assetPath, removeFileExtension: true);
	}

	internal static string GetAssetName(string assetPath, bool removeFileExtension)
	{
		string text = assetPath;
		int num = Math.Max(assetPath.LastIndexOf('/'), assetPath.LastIndexOf('\\'));
		if (num != -1)
		{
			text = text.Substring(num + 1);
		}
		if (removeFileExtension)
		{
			num = text.LastIndexOf('.');
			if (num != -1)
			{
				text = text.Substring(0, num);
			}
		}
		return text;
	}

	internal static string GetFullAssetName(string? name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return "";
		}
		string text = name.ToLowerInvariant();
		if (knownAssetNames.ContainsKey(text))
		{
			return text;
		}
		if (knownAssetMappedNames.ContainsKey(text))
		{
			return knownAssetMappedNames[text];
		}
		return "";
	}

	internal static AssetBundle GetAssetBundleFromFile(string fullPath)
	{
		AssetBundle val = AssetBundle.LoadFromFile(fullPath);
		if (Object.op_Implicit((Object)(object)val))
		{
			return val;
		}
		throw new Exception("Could not load AssetBundle from '" + fullPath + "'. The asset bundle might have been made with an incorrect version of Unity (should be 2019.4.19).");
	}

	private static void LoadAssetBundle(string relativePath, string fullPath)
	{
		LoadAssetBundle(relativePath, GetAssetBundleFromFile(fullPath));
	}

	private static void LoadAssetBundle(string relativePath, AssetBundle assetBundle)
	{
		knownAssetBundles.Add(relativePath, assetBundle);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("Registered AssetBundle '");
		stringBuilder.Append(relativePath);
		stringBuilder.Append("' with the following assets\n");
		foreach (string item in (Il2CppArrayBase<string>)(object)assetBundle.GetAllAssetNames())
		{
			string assetName = GetAssetName(item);
			if (assetName == "localization")
			{
				LocalizationManager.LoadLocalization(((Il2CppObjectBase)assetBundle.LoadAsset(item)).Cast<TextAsset>(), item);
				continue;
			}
			if (assetName.EndsWith("atlas"))
			{
				AtlasManager.LoadUiAtlas(assetBundle.LoadAsset(item));
				continue;
			}
			if (knownAssetNames.ContainsKey(item))
			{
				Logger.Log("Duplicate asset name '" + item + "'.");
				continue;
			}
			knownAssetNames.Add(item, assetBundle);
			string assetMappedName = GetAssetMappedName(item, assetName);
			knownAssetMappedNames.Add(assetMappedName, item);
			stringBuilder.Append("  ");
			stringBuilder.Append(assetMappedName);
			stringBuilder.Append(" => ");
			stringBuilder.Append(item);
			stringBuilder.Append("\n");
		}
		Logger.Log(stringBuilder.ToString().Trim());
	}

	private static string StripResourceFolder(string assetPath)
	{
		string result = assetPath;
		while (true)
		{
			string text = RESOURCE_FOLDER.Where((string eachResourceFolder) => result.StartsWith(eachResourceFolder)).FirstOrDefault();
			if (text == null)
			{
				break;
			}
			result = result.Substring(text.Length);
		}
		return result;
	}

	internal static string[] Trim(string[] values)
	{
		string[] array = new string[values.Length];
		for (int i = 0; i < values.Length; i++)
		{
			array[i] = values[i].Trim();
		}
		return array;
	}
}
