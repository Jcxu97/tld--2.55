using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NorthernLightsBroadcast;

public static class FileStuff
{
	public static string settingsFile = Application.dataPath + "/../Mods/NorthernLightsBroadcast_ResumeAtFrame.ini";

	public static Dictionary<string, double> clipFrames = new Dictionary<string, double>();

	public static FileStream frameFile;

	public static List<string> videoFileExtensions = new List<string>
	{
		".asf", ".avi", ".dv", ".m4v", ".mov", ".mp4", ".mpg", ".mpeg", ".ogv", ".vp8",
		".webm", ".wmv"
	};

	public static void OpenFrameFile()
	{
		if (!File.Exists(settingsFile))
		{
			frameFile = File.Create(settingsFile);
			frameFile.Close();
		}
		string[] array = File.ReadAllLines(settingsFile);
		foreach (string obj in array)
		{
			string[] array2 = new string[2];
			array2 = obj.Split("|");
			clipFrames.Add(array2[0], (long)float.Parse(array2[1]));
		}
	}

	public static void AddFrameValueToFile(string clipName, double frameValue)
	{
		double num = frameValue;
		if (num < 0.0)
		{
			num = 0.0;
		}
		if (clipFrames.ContainsKey(clipName))
		{
			clipFrames[clipName] = num;
		}
		else
		{
			clipFrames.Add(clipName, num);
		}
	}

	public static double GetFrameValueFromFile(string clipName)
	{
		if (clipFrames.ContainsKey(clipName))
		{
			return clipFrames[clipName];
		}
		AddFrameValueToFile(clipName, 0.0);
		return 0.0;
	}

	public static void SaveFrameFile()
	{
		using StreamWriter streamWriter = new StreamWriter(settingsFile);
		foreach (KeyValuePair<string, double> clipFrame in clipFrames)
		{
			double num = clipFrame.Value;
			if (num < 0.0)
			{
				num = 0.0;
			}
			streamWriter.WriteLine(clipFrame.Key + "|" + num);
		}
		streamWriter.Close();
	}

	public static string[] GetFilesInPath(string path)
	{
		string[] files = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
		List<string> list = new List<string>();
		string[] array = files;
		foreach (string text in array)
		{
			if (videoFileExtensions.Contains(Path.GetExtension(text)))
			{
				list.Add(text);
			}
		}
		return list.ToArray();
	}

	public static string[] GetFoldersInPath(string path)
	{
		return Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
	}
}
