using System;
using System.Collections.Generic;
using System.IO;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ModComponent.AssetLoader;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Mapper;

internal static class AutoMapper
{
	private static readonly List<string> pendingAssetBundles = new List<string>();

	private static readonly Dictionary<string, string> pendingAssetBundleZipFileMap = new Dictionary<string, string>();

	private static List<string> mappedPrefabs = new List<string>();

	internal static void AutoMapPrefab(string bundlePath, string prefabName)
	{
		if (!mappedPrefabs.Contains(prefabName))
		{
			mappedPrefabs.Add(prefabName);
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(bundlePath);
			GameObject obj = AssetBundleUtils.LoadAsset<GameObject>(prefabName);
			if ((Object)(object)obj == (Object)null)
			{
				throw new Exception($"({fileNameWithoutExtension}) {prefabName} could not be loaded with Resources.Load");
			}
			GameObject val = ((Il2CppObjectBase)obj).TryCast<GameObject>();
			if ((Object)(object)val == (Object)null)
			{
				throw new NullReferenceException($"In AutoMapper.AutoMapPrefab, ({fileNameWithoutExtension}) {prefabName} loaded object was not a GameObject.");
			}
			if (((Object)val).name.StartsWith("GEAR_"))
			{
				MapModComponent(val);
			}
		}
	}

	private static void LoadAssetBundle(string relativePath)
	{
		LoadAssetBundle(ModAssetBundleManager.GetAssetBundle(relativePath));
	}

	internal static void LoadAssetBundle(AssetBundle assetBundle)
	{
		string[] array = Il2CppArrayBase<string>.op_Implicit((Il2CppArrayBase<string>)(object)assetBundle.GetAllAssetNames());
		foreach (string text in array)
		{
			if (text.EndsWith(".prefab"))
			{
				AutoMapPrefab(((Object)assetBundle).name, text);
			}
		}
	}

	internal static void MapModComponent(GameObject prefab)
	{
		if ((Object)(object)prefab == (Object)null)
		{
			throw new ArgumentNullException("Prefab was null in AutoMapper.MapModComponent");
		}
		ComponentJson.InitializeComponents(prefab);
		if ((Object)(object)prefab.GetModComponent() == (Object)null)
		{
			throw new NullReferenceException("In AutoMapper.MapModComponent, the mod component from the prefab was null.");
		}
		Logger.LogDebug("Mapping " + ((Object)prefab).name);
		try
		{
			ItemMapper.Map(prefab);
		}
		catch (Exception ex)
		{
			Logger.LogWarning(ex.ToString());
		}
	}

	internal static void AddAssetBundle(string relativePath)
	{
		if (pendingAssetBundles.Contains(relativePath))
		{
			Logger.LogWarning("AutoMapper already has '" + relativePath + "' on the list of pending asset bundles.");
		}
		else
		{
			pendingAssetBundles.Add(relativePath);
		}
	}

	internal static void AddAssetBundle(string relativePath, string zipFilePath)
	{
		AddAssetBundle(relativePath);
		if (pendingAssetBundleZipFileMap.ContainsKey(relativePath))
		{
			Logger.LogWarning("AutoMapper already has '" + relativePath + "' in the dictionary of pending asset bundle paths.");
		}
		else
		{
			pendingAssetBundleZipFileMap.Add(relativePath, zipFilePath);
		}
	}

	internal static void LoadPendingAssetBundles()
	{
		Logger.LogDebug("Loading the pending asset bundles");
		foreach (string pendingAssetBundle in pendingAssetBundles)
		{
			try
			{
				LoadAssetBundle(pendingAssetBundle);
			}
			catch (Exception value)
			{
				string text = $"Could not map the assets in the bundle at '{pendingAssetBundle}'. {value}";
				if (pendingAssetBundleZipFileMap.ContainsKey(pendingAssetBundle))
				{
					PackManager.SetItemPackNotWorking(pendingAssetBundleZipFileMap[pendingAssetBundle], text);
				}
				else
				{
					Logger.LogError(text);
				}
			}
		}
		pendingAssetBundles.Clear();
		pendingAssetBundleZipFileMap.Clear();
	}
}
