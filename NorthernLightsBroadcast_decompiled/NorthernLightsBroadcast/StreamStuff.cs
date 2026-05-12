using System;
using System.Collections.Generic;
using System.Net;
using MelonLoader;

namespace NorthernLightsBroadcast;

public static class StreamStuff
{
	public static bool gotList = false;

	public static string indexFileContent;

	public static int globalChance = 50;

	public static List<string> fileURL = new List<string>();

	public static Dictionary<string, int> playbackChance = new Dictionary<string, int>();

	public static Dictionary<string, int> playbackMaxCount = new Dictionary<string, int>();

	public static string indexFileURL = "https://digitalzombie.de/NorthernLightsBroadcast/index";

	private static string _fileURL = "https://digitalzombie.de/NorthernLightsBroadcast/lines.txt";

	public static void GetText()
	{
		string[] array = new WebClient().DownloadString(_fileURL).Split(new string[1] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			MelonLogger.Msg(array[i]);
		}
	}

	public static void GetIndexList()
	{
		try
		{
			indexFileContent = new WebClient().DownloadString(indexFileURL);
			string[] array = indexFileContent.Split("#");
			foreach (string text in array)
			{
				if (!text.Contains("___") && text.Contains("|"))
				{
					string[] array2 = new string[3];
					array2 = text.Split("|");
					fileURL.Add(array2[0]);
					playbackChance.Add(array2[0], int.Parse(array2[1]));
					playbackMaxCount.Add(array2[0], int.Parse(array2[2]));
				}
			}
			indexFileContent = "";
			if (fileURL.Count > 0)
			{
				gotList = true;
			}
		}
		catch
		{
			gotList = false;
		}
	}
}
