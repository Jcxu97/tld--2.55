using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Il2Cpp;

namespace GearSpawner;

public static class SpawnManager
{
	public static event Action<string>? OnStartSpawning;

	public static event Action<IReadOnlyList<GearItem>>? OnFinishSpawning;

	public static void ParseSpawnInformation(string text)
	{
		GearSpawnReader.ProcessLines(Regex.Split(text, "\r\n|\r|\n"));
	}

	public static void AddLootTableEntry(string lootTable, LootTableEntry entry)
	{
		LootTableManager.AddLootTableEntry(lootTable, entry);
	}

	public static void AddGearSpawnInfo(string sceneName, GearSpawnInfo gearSpawnInfo)
	{
		GearSpawnManager.AddGearSpawnInfo(sceneName, gearSpawnInfo);
	}

	internal static void InvokeFinishSpawningEvent(IReadOnlyList<GearItem> items)
	{
		SpawnManager.OnFinishSpawning?.Invoke(items);
	}

	internal static void InvokeStartSpawningEvent(string sceneName)
	{
		SpawnManager.OnStartSpawning?.Invoke(sceneName);
	}
}
