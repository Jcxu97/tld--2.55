using System;
using System.Collections.Generic;

namespace GearSpawner;

public static class SpawnTagManager
{
	private static readonly DefaultGearSpawnHandler defaultHandler = new DefaultGearSpawnHandler();

	private static readonly Dictionary<string, GearSpawnHandler> taggedHandlers = new Dictionary<string, GearSpawnHandler>();

	public static void AddFunction(string tag, Func<DifficultyLevel, FirearmAvailability, GearSpawnInfo, float> function)
	{
		AddHandler(tag, new FunctionGearSpawnHandler(function));
	}

	public static void AddHandler(string tag, GearSpawnHandler handler)
	{
		string text = tag.ToLowerInvariant();
		if (text == "none")
		{
			GearSpawnerMod.Logger.Error("The spawn tag 'None' is reserved for GearSpawner internal workings.");
		}
		else if (taggedHandlers.ContainsKey(text))
		{
			GearSpawnerMod.Logger.Error("Spawn tag " + tag + " already registered. Overwriting...");
			taggedHandlers[text] = handler;
		}
		else
		{
			taggedHandlers.Add(text, handler);
		}
	}

	internal static GearSpawnHandler GetHandler(string tag)
	{
		if (!taggedHandlers.TryGetValue(tag, out GearSpawnHandler value))
		{
			return defaultHandler;
		}
		return value;
	}
}
