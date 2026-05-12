using System;
using System.Collections.Generic;

namespace ModComponent.Mapper;

internal static class JsonHandler
{
	private static readonly Dictionary<string, string> itemJsons = new Dictionary<string, string>();

	public static void RegisterJsonText(string itemName, string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			Logger.Log("JSON data empty for " + itemName);
		}
		else if (itemJsons.ContainsKey(itemName))
		{
			Logger.Log("Overwriting data for " + itemName);
			itemJsons[itemName] = text;
		}
		else
		{
			itemJsons.Add(itemName, text);
			Logger.LogDebug("JSON added for " + itemName);
		}
	}

	public static string GetJsonText(string itemName)
	{
		Logger.LogDebug("Get JSON data for " + itemName + " " + itemName.ToLower());
		try
		{
			if (!itemJsons.TryGetValue(itemName.ToLowerInvariant(), out string value))
			{
				throw new Exception("Could not find json file for " + itemName);
			}
			return value;
		}
		catch (Exception ex)
		{
			Logger.Log($"Exception {itemName} {itemName.ToLowerInvariant()} :: {ex.ToString()}");
			return null;
		}
	}
}
