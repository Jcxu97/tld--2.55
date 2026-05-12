using System;
using System.Collections.Generic;
using System.IO;
using Il2CppSystem;
using MelonLoader;
using UnityEngine;

namespace BlueprintCleaner;

public class Main : MelonMod
{
	public static bool vanillaDisplay = false;

	public static List<string> blueprintsRemoved = LoadListFromJson();

	public override void OnInitializeMelon()
	{
		Debug.Log(Object.op_Implicit($"[{((MelonBase)this).Info.Name}] Version {((MelonBase)this).Info.Version} loaded!"));
		Settings.OnLoad();
	}

	public static void SaveListToJson(List<string> blueprintsList)
	{
		string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mods", "RemovedBlueprints_dont_touch_this_file.json");
		string contents = "[" + string.Join(",", blueprintsList.ConvertAll((string item) => "\"" + item + "\"")) + "]";
		File.WriteAllText(path, contents);
	}

	public static List<string> LoadListFromJson()
	{
		string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mods", "RemovedBlueprints_dont_touch_this_file.json");
		if (File.Exists(path))
		{
			string text = File.ReadAllText(path);
			List<string> list = new List<string>();
			text = text.Trim('[', ']');
			if (!string.IsNullOrEmpty(text))
			{
				string[] array = text.Split(',');
				foreach (string text2 in array)
				{
					list.Add(text2.Trim('"'));
				}
			}
			return list;
		}
		return new List<string>();
	}
}
