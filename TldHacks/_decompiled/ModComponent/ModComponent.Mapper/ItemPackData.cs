using System.Collections.Generic;
using ModComponent.AssetLoader;

namespace ModComponent.Mapper;

internal class ItemPackData
{
	private string name;

	private string zipFileName;

	private string zipFilePath;

	private bool loadedCorrectly = true;

	public ItemPackData(string zipFilePath)
	{
		name = ParseNameFromZipFilePath(zipFilePath);
		zipFileName = ModAssetBundleManager.GetAssetName(zipFilePath, removeFileExtension: false);
		this.zipFilePath = zipFilePath;
	}

	public ItemPackData(string zipFilePath, string zipFileName)
	{
		name = ParseNameFromZipFilePath(zipFileName);
		this.zipFileName = zipFileName;
		this.zipFilePath = zipFilePath;
	}

	public string GetName()
	{
		return string.Copy(name);
	}

	public string GetZipFileName()
	{
		return string.Copy(zipFileName);
	}

	public string GetZipFilePath()
	{
		return string.Copy(zipFilePath);
	}

	public bool GetLoadedCorrectly()
	{
		return loadedCorrectly;
	}

	public void SetLoadedIncorrectly()
	{
		if (loadedCorrectly)
		{
			loadedCorrectly = false;
		}
	}

	public static string ParseNameFromZipFilePath(string zipFilePath)
	{
		return MergeWithSpaces(SplitCamelCase(ModAssetBundleManager.GetAssetName(zipFilePath.Trim()).Split('-', '_', ' ')));
	}

	public static string[] SplitCamelCase(string line)
	{
		if (string.IsNullOrEmpty(line))
		{
			return new string[0];
		}
		List<string> list = new List<string>();
		string text = "";
		for (int i = 0; i < line.Length; i++)
		{
			if (char.IsUpper(line[i]) && i != 0)
			{
				list.Add(string.Copy(text));
				text = "";
			}
			text += line[i];
		}
		list.Add(string.Copy(text));
		return list.ToArray();
	}

	public static string[] SplitCamelCase(string[] lines)
	{
		string[] array = new string[0];
		foreach (string line in lines)
		{
			array = Combine(array, SplitCamelCase(line));
		}
		return array;
	}

	public static T[] Combine<T>(T[] array1, T[] array2)
	{
		T[] array3 = new T[array1.Length + array2.Length];
		for (int i = 0; i < array1.Length; i++)
		{
			array3[i] = array1[i];
		}
		for (int j = 0; j < array2.Length; j++)
		{
			array3[array1.Length + j] = array2[j];
		}
		return array3;
	}

	public static string MergeWithSpaces(string[] words)
	{
		string text = "";
		bool flag = true;
		for (int i = 0; i < words.Length; i++)
		{
			if (!string.IsNullOrEmpty(words[i]))
			{
				if (flag)
				{
					text += words[i];
					flag = false;
				}
				else
				{
					text = text + " " + words[i];
				}
			}
		}
		return text;
	}
}
