using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using Il2CppTLD.OptionalContent;
using MelonLoader.Utils;
using Newtonsoft.Json;

namespace ModComponent.Utils;

internal static class DependencyChecker
{
	internal sealed class ErrorEntry
	{
		public string Mod { get; set; }

		public bool MissingDlc { get; set; }

		public List<string> Missing { get; set; } = new List<string>();


		public override string ToString()
		{
			return string.Concat(Mod + "|" + MissingDlc + "|" + string.Join(",", Missing));
		}
	}

	internal sealed class DepEntry
	{
		public string Mod { get; set; }

		public bool RequiresDLC { get; set; }

		public string[] Requires { get; set; } = Array.Empty<string>();


		public override string ToString()
		{
			return string.Concat(Mod + "|" + RequiresDLC + "|" + string.Join(",", Requires));
		}
	}

	internal sealed class BuildFileEntry
	{
		public string Mod { get; set; }

		public string Name { get; set; }

		public string Version { get; set; }

		public string Author { get; set; }

		public string[] Requires { get; set; }

		public bool RequiresDLC { get; set; }

		internal DepEntry AsDepEntry()
		{
			return new DepEntry
			{
				Mod = Mod,
				RequiresDLC = RequiresDLC,
				Requires = Requires
			};
		}
	}

	private static string depFileUrl = "https://raw.githubusercontent.com/TLD-Mods/ModLists/master/dependency_files/modcomponent.json";

	private static List<string> filePaths = new List<string>();

	private static List<string> fileNames = new List<string>();

	private static List<DepEntry> BuildDepEntries = new List<DepEntry>();

	private static List<DepEntry> GlobalDepEntries = new List<DepEntry>();

	private static List<DepEntry> UserDepEntries = new List<DepEntry>();

	private static List<ErrorEntry> DepErrors = new List<ErrorEntry>();

	internal static void RunChecks()
	{
		Logger.LogDebug("----------------------------------");
		Logger.Log("Running Dependency Checks..");
		ReadFiles();
		ReadGlobalDepEntries();
		ProcessDepEntries();
		ThrowErrors();
		Logger.LogDebug("----------------------------------");
	}

	internal static void AddEntry(string MCFileName, string[] RequiresMCFileNames, bool RequiresDlc = false)
	{
		MCFileName = MCFileName.Replace(".modcomponent", null).ToLowerInvariant();
		RequiresMCFileNames = RequiresMCFileNames.Select((string x) => x.Replace(".modcomponent", null).ToLowerInvariant()).ToArray();
		DepEntry item = new DepEntry
		{
			Mod = MCFileName,
			RequiresDLC = RequiresDlc,
			Requires = RequiresMCFileNames
		};
		UserDepEntries.Add(item);
	}

	private static void ProcessDepEntries()
	{
		Dictionary<string, DepEntry> dictionary = new Dictionary<string, DepEntry>();
		foreach (DepEntry buildDepEntry in BuildDepEntries)
		{
			if (!dictionary.ContainsKey(buildDepEntry.ToString()))
			{
				dictionary.Add(buildDepEntry.ToString(), buildDepEntry);
			}
		}
		foreach (DepEntry globalDepEntry in GlobalDepEntries)
		{
			if (!dictionary.ContainsKey(globalDepEntry.ToString()))
			{
				dictionary.Add(globalDepEntry.ToString(), globalDepEntry);
			}
		}
		foreach (DepEntry userDepEntry in UserDepEntries)
		{
			if (!dictionary.ContainsKey(userDepEntry.ToString()))
			{
				dictionary.Add(userDepEntry.ToString(), userDepEntry);
			}
		}
		foreach (KeyValuePair<string, DepEntry> item in dictionary)
		{
			ErrorEntry errorEntry = CheckDepEntry(item.Value);
			if (!string.IsNullOrEmpty(errorEntry.Mod) && errorEntry.Missing.Count > 0)
			{
				DepErrors.Add(errorEntry);
			}
		}
		Logger.LogDebug($"ProcessDepEntries : {BuildDepEntries.Count} {GlobalDepEntries.Count} {UserDepEntries.Count} | {UserDepEntries.Count} => {DepErrors.Count}");
	}

	private static void ReadFiles()
	{
		string[] files = Directory.GetFiles(MelonEnvironment.ModsDirectory, "*.modcomponent");
		foreach (string text in files)
		{
			string item = Path.GetFileNameWithoutExtension(text).ToLowerInvariant();
			filePaths.Add(text);
			fileNames.Add(item);
		}
		foreach (string filePath in filePaths)
		{
			using FileStream fileStream = new FileStream(filePath, FileMode.Open);
			Path.GetFileName(filePath);
			if (FileUtils.DetectZipFileType(fileStream) != "zip")
			{
				fileStream.Dispose();
				break;
			}
			using ZipArchive zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Read);
			ZipArchiveEntry zipArchiveEntry = null;
			foreach (ZipArchiveEntry entry in zipArchive.Entries)
			{
				if (entry.Name.ToLowerInvariant() == "buildinfo.json")
				{
					zipArchiveEntry = entry;
				}
			}
			if (zipArchiveEntry == null)
			{
				continue;
			}
			using StreamReader streamReader = new StreamReader(zipArchiveEntry.Open());
			string text2 = streamReader.ReadToEnd();
			if (string.IsNullOrEmpty(text2) || !text2.Contains("Requires"))
			{
				continue;
			}
			BuildFileEntry buildFileEntry = JsonConvert.DeserializeObject<BuildFileEntry>(text2);
			if (buildFileEntry != null)
			{
				Logger.LogDebug($"buildinfo FOUND : {buildFileEntry.Name} {buildFileEntry.Requires.Length} {buildFileEntry.RequiresDLC}");
				if (buildFileEntry.Requires.Length != 0 || buildFileEntry.RequiresDLC)
				{
					buildFileEntry.Mod = Path.GetFileNameWithoutExtension(filePath);
					BuildDepEntries.Add(buildFileEntry.AsDepEntry());
				}
			}
		}
	}

	private static void ReadGlobalDepEntries()
	{
		string text = "";
		WebClient webClient = new WebClient();
		try
		{
			webClient.Headers["User-Agent"] = "ModComponent";
			text = webClient.DownloadString(depFileUrl);
		}
		finally
		{
			((IDisposable)webClient)?.Dispose();
		}
		if (!string.IsNullOrEmpty(text))
		{
			GlobalDepEntries = JsonConvert.DeserializeObject<List<DepEntry>>(text);
		}
	}

	private static ErrorEntry? CheckDepEntry(DepEntry entry)
	{
		Logger.LogDebug("Found DepEntry: " + entry.ToString());
		ErrorEntry errorEntry = new ErrorEntry();
		errorEntry.Mod = entry.Mod;
		if (fileNames.Contains(entry.Mod.ToLowerInvariant()))
		{
			Logger.LogDebug("Running DepEntry: " + entry.ToString());
			List<string> list = new List<string>();
			Logger.LogDebug($"Count: {entry.Requires.Length}");
			string[] requires = entry.Requires;
			foreach (string text in requires)
			{
				Logger.LogDebug("Checking for : " + text);
				if (!string.IsNullOrEmpty(text) && !fileNames.Contains(text.ToLowerInvariant()))
				{
					Logger.LogDebug("Missing : " + text);
					list.Add(text);
				}
			}
			errorEntry.Missing = list;
			if (entry.RequiresDLC)
			{
				Logger.LogDebug("Checking for DLC");
				if (!OptionalContentManager.Instance.InstalledContent.ContainsKey("2091330"))
				{
					errorEntry.MissingDlc = true;
				}
			}
		}
		return errorEntry;
	}

	private static void ThrowErrors()
	{
		if (DepErrors.Count == 0)
		{
			return;
		}
		foreach (ErrorEntry depError in DepErrors)
		{
			if (depError.MissingDlc)
			{
				Logger.LogError("MissingDLC: '" + depError.Mod + "' requires DLC");
			}
			foreach (string item in depError.Missing)
			{
				Logger.LogError($"MissingDependency: '{depError.Mod}' requires '{item}'");
			}
		}
	}
}
