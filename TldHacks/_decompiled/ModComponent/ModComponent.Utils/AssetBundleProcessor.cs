using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader.Utils;
using ModComponent.Mapper;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;

namespace ModComponent.Utils;

internal class AssetBundleProcessor
{
	internal static string tempFolderName { get; set; } = "_ModComponentTemp";


	internal static string tempFolderPath { get; set; } = Path.Combine(MelonEnvironment.ModsDirectory, tempFolderName);


	internal static List<string> catalogFilePaths { get; set; } = new List<string>();


	internal static List<string> catalogsLoaded { get; set; } = new List<string>();


	internal static Dictionary<string, List<string>> catalogBundleList { get; set; } = new Dictionary<string, List<string>>();


	internal static Dictionary<string, string> catalogTestList { get; set; } = new Dictionary<string, string>();


	internal static List<string> bundleFilePaths { get; set; } = new List<string>();


	internal static List<string> bundleNames { get; set; } = new List<string>();


	internal static Dictionary<string, List<string>> bundleAssetList { get; set; } = new Dictionary<string, List<string>>();


	internal static void Initialize()
	{
		InitTempFolder();
		ZipFileLoader.Initialize();
		PreloadAssetBundles();
		LoadCatalogs();
		TestCatalogs();
	}

	internal static void InitTempFolder()
	{
		CleanupTempFolder();
		if (!Directory.Exists(tempFolderPath))
		{
			Logger.LogDebug("Creating temp folder (" + tempFolderName + ")");
			Directory.CreateDirectory(tempFolderPath);
		}
	}

	internal static void InitTempBundleFolder(string bundleName)
	{
		string text = Path.Combine(tempFolderPath, bundleName);
		if (!Directory.Exists(text))
		{
			Logger.LogDebug("Creating temp bundle folder (" + text + ")");
			Directory.CreateDirectory(text);
		}
	}

	internal static void CleanupTempFolder()
	{
		foreach (string bundleFilePath in bundleFilePaths)
		{
			if (bundleFilePath != null && File.Exists(bundleFilePath))
			{
				File.Delete(bundleFilePath);
			}
		}
		foreach (string catalogFilePath in catalogFilePaths)
		{
			if (catalogFilePath != null && File.Exists(catalogFilePath))
			{
				File.Delete(catalogFilePath);
			}
		}
		if (Directory.Exists(tempFolderPath))
		{
			Directory.Delete(tempFolderPath, recursive: true);
		}
	}

	internal static void PreloadAssetBundles()
	{
		foreach (string bundleFilePath in bundleFilePaths)
		{
			if (bundleAssetList.ContainsKey(bundleFilePath) || bundleFilePath == null || !File.Exists(bundleFilePath))
			{
				continue;
			}
			string fileName = Path.GetFileName(bundleFilePath);
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(bundleFilePath);
			Logger.LogDebug("Preloading (" + fileName + ")");
			List<string> list = new List<string>();
			AssetBundle val = AssetBundle.LoadFromFile(bundleFilePath);
			foreach (string item in (Il2CppArrayBase<string>)(object)val.GetAllAssetNames())
			{
				list.Add(item);
			}
			bundleAssetList.Add(bundleFilePath, list);
			bundleNames.Add(fileNameWithoutExtension);
			val.Unload(true);
		}
	}

	internal static void WriteAssettBundleToDisk(string bundleName, string filename, byte[] data)
	{
		InitTempBundleFolder(bundleName);
		string text = Path.Combine(tempFolderPath, bundleName, filename);
		if (File.Exists(text))
		{
			File.Delete(text);
		}
		FileStream fileStream = File.Create(text);
		fileStream.Write(data);
		fileStream.Close();
		Logger.LogDebug("Bundle Written (" + filename + ")");
		if (!text.Contains("unitybuiltinshaders"))
		{
			bundleFilePaths.Add(text);
		}
	}

	internal static void WriteCatalogToDisk(string bundleName, string filename, string data)
	{
		InitTempBundleFolder(bundleName);
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
		string text = Path.Combine(tempFolderPath, bundleName, filename);
		if (File.Exists(text))
		{
			File.Delete(text);
		}
		string text2 = null;
		List<string> list = new List<string>();
		ModContentCatalog modContentCatalog = JsonSerializer.Deserialize<ModContentCatalog>(data);
		if (modContentCatalog == null)
		{
			Logger.LogError("Catalog Failed - Could not deserialize json (" + fileNameWithoutExtension + ")");
			return;
		}
		if (modContentCatalog.m_InternalIds == null || modContentCatalog.m_InternalIds.Length == 0)
		{
			Logger.LogError("Catalog Failed - InternalIds empty (" + fileNameWithoutExtension + ")");
			return;
		}
		for (int i = 0; i < modContentCatalog.m_InternalIds.Length; i++)
		{
			string text3 = modContentCatalog.m_InternalIds[i];
			string extension = Path.GetExtension(text3);
			if (extension == ".bundle" || extension == ".unity3d")
			{
				modContentCatalog.m_InternalIds[i] = Path.Combine(tempFolderPath, bundleName, Path.GetFileName(text3));
				list.Add(Path.GetFileName(text3));
			}
			else if (text2 == null)
			{
				text2 = text3;
			}
		}
		if (!fileNameWithoutExtension.Contains("unitybuiltinshaders"))
		{
			catalogBundleList.Add(text, list);
		}
		modContentCatalog.m_LocatorId = fileNameWithoutExtension;
		Logger.LogDebug("Catalog m_InternalIds Patched (" + fileNameWithoutExtension + ")");
		data = JsonSerializer.Serialize(modContentCatalog);
		File.WriteAllText(text, data);
		Logger.LogDebug("Catalog Written (" + fileNameWithoutExtension + ")");
		catalogFilePaths.Add(text);
		if (text2 != null && !fileNameWithoutExtension.Contains("unitybuiltinshaders"))
		{
			catalogTestList.Add(text, text2);
		}
	}

	internal static void LoadCatalogs()
	{
		foreach (KeyValuePair<string, List<string>> catalogBundle in catalogBundleList)
		{
			string key = catalogBundle.Key;
			if (catalogBundle.Value != null && catalogBundle.Value.Count > 0 && LoadCatalog(key))
			{
				catalogsLoaded.Add(key);
			}
		}
	}

	internal static bool LoadCatalog(string catalogFilePath)
	{
		if (catalogFilePath == null || catalogFilePath == "")
		{
			Logger.LogError("Catalog Loaded - No Catalog Path");
			return true;
		}
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(catalogFilePath);
		string extension = Path.GetExtension(catalogFilePath);
		if (extension != ".json")
		{
			Logger.LogError("Catalog Failed - Invalid extension (" + extension + ")");
			return true;
		}
		try
		{
			IResourceLocator val = Addressables.LoadContentCatalogAsync(catalogFilePath, (string)null).WaitForCompletion();
			if (val != null && val.Keys != null)
			{
				Logger.LogDebug("Catalog Loaded (" + fileNameWithoutExtension + ") ");
				return true;
			}
		}
		catch (Exception ex)
		{
			Logger.LogError("Catalog Failed (" + fileNameWithoutExtension + ") " + ex.ToString());
			return false;
		}
		return false;
	}

	internal static void TestCatalogs()
	{
		foreach (string item in catalogsLoaded)
		{
			TestCatalog(item);
		}
	}

	internal static bool TestCatalog(string catalogFilePath)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(catalogFilePath);
		if (catalogTestList.ContainsKey(catalogFilePath))
		{
			try
			{
				string path = catalogTestList[catalogFilePath];
				string text = Path.GetExtension(path).ToLowerInvariant();
				string fileNameWithoutExtension2 = Path.GetFileNameWithoutExtension(path);
				switch (text)
				{
				case ".mat":
				{
					Material val3 = AssetBundleUtils.LoadAsset<Material>(fileNameWithoutExtension2);
					if ((Object)(object)val3 != (Object)null && ((Object)val3).name != null)
					{
						Logger.LogDebug("Catalog Test (" + fileNameWithoutExtension + ") (" + fileNameWithoutExtension2 + ") OK");
						return true;
					}
					Logger.LogError("Catalog Test (" + fileNameWithoutExtension + ") (" + fileNameWithoutExtension2 + ") Failed");
					return false;
				}
				case ".png":
				case ".jpg":
				{
					Texture2D val2 = AssetBundleUtils.LoadAsset<Texture2D>(fileNameWithoutExtension2);
					if ((Object)(object)val2 != (Object)null && ((Object)val2).name != null)
					{
						Logger.LogDebug("Catalog Test (" + fileNameWithoutExtension + ") (" + fileNameWithoutExtension2 + ") OK");
						return true;
					}
					Logger.LogError("Catalog Test (" + fileNameWithoutExtension + ") (" + fileNameWithoutExtension2 + ") Failed");
					return false;
				}
				case ".prefab":
				{
					GameObject val = AssetBundleUtils.LoadAsset<GameObject>(fileNameWithoutExtension2);
					if ((Object)(object)val != (Object)null && ((Object)val).name != null)
					{
						Logger.LogDebug("Catalog Test (" + fileNameWithoutExtension + ") (" + fileNameWithoutExtension2 + ") OK");
						return true;
					}
					Logger.LogError("Catalog Test (" + fileNameWithoutExtension + ") (" + fileNameWithoutExtension2 + ") Failed");
					return false;
				}
				default:
					Logger.LogError("Catalog Test Failed (" + fileNameWithoutExtension + ") (" + fileNameWithoutExtension2 + text + ") Unknown asset extension");
					return false;
				}
			}
			catch (Exception ex)
			{
				Logger.LogError("Catalog Test Failed (" + fileNameWithoutExtension + ") " + ex.ToString());
				return false;
			}
		}
		Logger.LogError("Catalog Test Failed (" + fileNameWithoutExtension + ") No test found");
		return false;
	}

	internal static void MapPrefabs()
	{
		foreach (KeyValuePair<string, List<string>> bundleAsset in bundleAssetList)
		{
			foreach (string item in bundleAsset.Value)
			{
				if (item.ToLower().EndsWith(".prefab"))
				{
					AutoMapper.AutoMapPrefab(Path.GetFileNameWithoutExtension(bundleAsset.Key), Path.GetFileNameWithoutExtension(item));
				}
			}
		}
	}

	internal static bool IsModComponentPrefab(string name)
	{
		foreach (KeyValuePair<string, List<string>> bundleAsset in bundleAssetList)
		{
			foreach (string item in bundleAsset.Value)
			{
				if (Path.GetFileNameWithoutExtension(item).ToLower() == name.ToLower())
				{
					return true;
				}
			}
		}
		return false;
	}

	internal static string? GetPrefabBundlePath(string name)
	{
		foreach (KeyValuePair<string, List<string>> bundleAsset in bundleAssetList)
		{
			foreach (string item in bundleAsset.Value)
			{
				if (Path.GetFileNameWithoutExtension(item).ToLower() == name.ToLower())
				{
					return bundleAsset.Key;
				}
			}
		}
		return null;
	}
}
