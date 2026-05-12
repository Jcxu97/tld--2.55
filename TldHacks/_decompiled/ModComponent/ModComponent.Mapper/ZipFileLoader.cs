using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using CraftingRevisions;
using GearSpawner;
using LocalizationUtilities;
using MelonLoader.Utils;
using ModComponent.AssetLoader;
using ModComponent.Utils;
using Newtonsoft.Json;

namespace ModComponent.Mapper;

internal static class ZipFileLoader
{
	internal static readonly List<byte[]> hashes = new List<byte[]>();

	internal static void Initialize()
	{
		LoadMCFilesInDirectory(MelonEnvironment.ModsDirectory, recursive: false);
	}

	private static void LoadMCFilesInDirectory(string directory, bool recursive)
	{
		string[] directories;
		if (recursive)
		{
			directories = Directory.GetDirectories(directory);
			for (int i = 0; i < directories.Length; i++)
			{
				LoadMCFilesInDirectory(directories[i], recursive: true);
			}
		}
		string[] files = Directory.GetFiles(directory, "*.modcomponent");
		Array.Sort(files);
		directories = files;
		foreach (string text in directories)
		{
			if (text.ToLower().EndsWith(".modcomponent"))
			{
				try
				{
					LoadMCFile(text);
				}
				catch (Exception value)
				{
					Logger.LogError($"Error Loading .modcomponent file\n'{value}'");
				}
			}
		}
	}

	private static void LoadMCFile(string MCFilePath)
	{
		string fileName = Path.GetFileName(MCFilePath);
		Path.GetFileNameWithoutExtension(MCFilePath);
		using FileStream fileStream = File.OpenRead(MCFilePath);
		string text = FileUtils.DetectZipFileType(fileStream);
		if (string.IsNullOrEmpty(text))
		{
			Logger.LogError("Unknown file compression " + fileName);
			fileStream.Dispose();
			return;
		}
		hashes.Add(SHA256.Create().ComputeHash(fileStream));
		fileStream.Seek(0L, SeekOrigin.Begin);
		if (text == "zip")
		{
			Logger.Log("Reading zip file: '" + fileName + "'");
			LoadZipFile(MCFilePath, fileStream);
			return;
		}
		Logger.LogError($"Unsupported compression type '{text}' for '{fileName}'");
	}

	private static void LoadZipFile(string MCFilePath, FileStream fileStream)
	{
		using ZipArchive zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Read);
		foreach (ZipArchiveEntry entry in zipArchive.Entries)
		{
			if (string.IsNullOrEmpty(entry.Name))
			{
				continue;
			}
			string fullName = entry.FullName;
			FileType fileType = GetFileType(entry.Name);
			if (fileType == FileType.other)
			{
				Logger.Log("skipping " + fullName + " " + fileType);
				continue;
			}
			using Stream stream = entry.Open();
			using MemoryStream memoryStream = new MemoryStream();
			stream.CopyTo(memoryStream);
			if (!TryHandleFile(MCFilePath, fullName, fileType, memoryStream))
			{
				break;
			}
		}
	}

	private static string ReadToString(MemoryStream memoryStream)
	{
		return Encoding.UTF8.GetString(memoryStream.ToArray());
	}

	private static string ReadToJsonString(MemoryStream memoryStream)
	{
		byte[] array = memoryStream.ToArray();
		int num = Array.IndexOf(array, (byte)123);
		if (num < 0)
		{
			throw new ArgumentException("MemoryStream has no Json content.", "memoryStream");
		}
		return Encoding.UTF8.GetString(new ReadOnlySpan<byte>(array, num, array.Length - num));
	}

	private static FileType GetFileType(string filename)
	{
		if (string.IsNullOrWhiteSpace(filename))
		{
			return FileType.other;
		}
		if (filename.EndsWith(".unity3d", StringComparison.Ordinal) || filename.EndsWith(".bundle", StringComparison.Ordinal))
		{
			return FileType.unity3d;
		}
		if (filename.EndsWith(".json", StringComparison.Ordinal))
		{
			return FileType.json;
		}
		if (filename.EndsWith(".txt", StringComparison.Ordinal))
		{
			return FileType.txt;
		}
		if (filename.EndsWith(".dll", StringComparison.Ordinal))
		{
			return FileType.dll;
		}
		if (filename.EndsWith(".bnk", StringComparison.Ordinal))
		{
			return FileType.bnk;
		}
		return FileType.other;
	}

	private static bool TryHandleFile(string zipFilePath, string internalPath, FileType fileType, MemoryStream unzippedFileStream)
	{
		switch (fileType)
		{
		case FileType.json:
			return TryHandleJson(zipFilePath, internalPath, ReadToJsonString(unzippedFileStream));
		case FileType.unity3d:
			return TryHandleUnity3d(zipFilePath, internalPath, unzippedFileStream.ToArray());
		case FileType.txt:
			return TryHandleTxt(zipFilePath, internalPath, ReadToString(unzippedFileStream));
		case FileType.dll:
			return TryLoadAssembly(zipFilePath, internalPath, unzippedFileStream.ToArray());
		case FileType.bnk:
			return TryRegisterSoundBank(zipFilePath, internalPath, unzippedFileStream.ToArray());
		default:
		{
			string text = Path.Combine(zipFilePath, internalPath);
			PackManager.SetItemPackNotWorking(zipFilePath, "Could not handle asset '" + text + "'");
			return false;
		}
		}
	}

	private static bool TryLoadAssembly(string zipFilePath, string internalPath, byte[] data)
	{
		try
		{
			Logger.LogDebug("Loading dll from zip at '" + internalPath + "'");
			Assembly.Load(data);
			return true;
		}
		catch (Exception ex)
		{
			string text = Path.Combine(zipFilePath, internalPath);
			PackManager.SetItemPackNotWorking(zipFilePath, "Could not load assembly '" + text + "'. " + ex.Message);
			return false;
		}
	}

	private static bool TryRegisterSoundBank(string zipFilePath, string internalPath, byte[] data)
	{
		try
		{
			Logger.LogDebug("Loading bnk from zip at '" + internalPath + "'");
			ModSoundBankManager.RegisterSoundBank(data);
			return true;
		}
		catch (Exception ex)
		{
			string text = Path.Combine(zipFilePath, internalPath);
			PackManager.SetItemPackNotWorking(zipFilePath, "Could not register sound bank '" + text + "'. " + ex.Message);
			return false;
		}
	}

	private static bool TryHandleJson(string zipFilePath, string internalPath, string text)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(zipFilePath);
		try
		{
			string fileNameWithoutExtension2 = Path.GetFileNameWithoutExtension(internalPath);
			if (internalPath.StartsWith("auto-mapped/"))
			{
				Logger.LogDebug("Reading automapped json from zip at '" + internalPath + "'");
				JsonHandler.RegisterJsonText(fileNameWithoutExtension2, text);
			}
			else if (internalPath.StartsWith("blueprints/"))
			{
				Logger.LogDebug("Reading blueprint json from zip at '" + internalPath + "'");
				BlueprintManager.AddBlueprintFromJson(text);
			}
			else if (internalPath.StartsWith("recipes/"))
			{
				Logger.LogDebug("Reading recipes json from zip at '" + internalPath + "'");
				RecipeManager.AddRecipeFromJson(text);
			}
			else if (internalPath.StartsWith("localizations/"))
			{
				Logger.LogDebug($"Reading localization json from zip at {zipFilePath} {internalPath} {text.Length}");
				LocalizationManager.LoadJsonLocalization(text);
			}
			else if (internalPath.StartsWith("bundle/"))
			{
				Logger.LogDebug("Reading json catalog from zip at '" + internalPath + "'");
				string fileName = Path.GetFileName(internalPath);
				AssetBundleProcessor.WriteCatalogToDisk(fileNameWithoutExtension, fileName, text);
			}
			else
			{
				if (!(internalPath.ToLowerInvariant() == "buildinfo.json"))
				{
					throw new NotSupportedException("Json file does not have a valid internal path: " + internalPath);
				}
				LogItemPackInformation(text);
			}
			return true;
		}
		catch (Exception ex)
		{
			string text2 = Path.Combine(zipFilePath, internalPath);
			PackManager.SetItemPackNotWorking(zipFilePath, "Could not load json '" + text2 + "'. " + ex.Message);
			return false;
		}
	}

	private static void LogItemPackInformation(string jsonText)
	{
		DependencyChecker.BuildFileEntry buildFileEntry = JsonConvert.DeserializeObject<DependencyChecker.BuildFileEntry>(jsonText);
		Logger.LogGreen($"Found: {buildFileEntry.Name} {buildFileEntry.Version} by {buildFileEntry.Author}");
	}

	private static bool TryHandleTxt(string zipFilePath, string internalPath, string text)
	{
		if (internalPath.StartsWith("gear-spawns/"))
		{
			try
			{
				Logger.LogDebug("Reading txt from zip at '" + internalPath + "'");
				SpawnManager.ParseSpawnInformation(text);
				return true;
			}
			catch (Exception ex)
			{
				string text2 = Path.Combine(zipFilePath, internalPath);
				PackManager.SetItemPackNotWorking(zipFilePath, "Could not load gear spawn '" + text2 + "'. " + ex.Message);
				return false;
			}
		}
		string text3 = Path.Combine(zipFilePath, internalPath);
		PackManager.SetItemPackNotWorking(zipFilePath, "Txt file not in the gear-spawns folder: '" + text3 + "'");
		return false;
	}

	private static bool TryHandleUnity3d(string zipFilePath, string internalPath, byte[] data)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(zipFilePath);
		string text = Path.Combine(zipFilePath, internalPath);
		if (internalPath.StartsWith("bundle/"))
		{
			Logger.LogDebug("Loading asset bundle from zip at '" + internalPath + "'");
			try
			{
				string fileName = Path.GetFileName(internalPath);
				AssetBundleProcessor.WriteAssettBundleToDisk(fileNameWithoutExtension, fileName, data);
				return true;
			}
			catch (Exception ex)
			{
				PackManager.SetItemPackNotWorking(zipFilePath, "Could not load asset bundle '" + text + "'. " + ex.Message);
				return false;
			}
		}
		PackManager.SetItemPackNotWorking(zipFilePath, "Asset bundle not in the bundle folder: '" + text + "'");
		return false;
	}
}
