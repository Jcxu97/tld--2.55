using System;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace GearSpawner;

internal static class GearSpawnReader
{
	private const string NUMBER = "-?\\d+(?:\\.\\d+)?";

	private const string VECTOR = "-?\\d+(?:\\.\\d+)?\\s*,\\s*-?\\d+(?:\\.\\d+)?\\s*,\\s*-?\\d+(?:\\.\\d+)?";

	private static readonly Regex LOOTTABLE_ENTRY_REGEX = new Regex("^item\\s*=\\s*(\\w+)\\W+w\\s*=\\s*(-?\\d+(?:\\.\\d+)?)$", RegexOptions.Compiled);

	private static readonly Regex LOOTTABLE_REGEX = new Regex("^loottable\\s*=\\s*(\\w+)$", RegexOptions.Compiled);

	private static readonly Regex SCENE_REGEX = new Regex("^scene\\s*=\\s*(\\w+)$", RegexOptions.Compiled);

	private static readonly Regex TAG_REGEX = new Regex("^tag\\s*=\\s*(\\w+)$", RegexOptions.Compiled);

	private static readonly Regex SPAWN_REGEX = new Regex("^item\\s*=\\s*(\\w+)(?:\\W+p\\s*=\\s*(-?\\d+(?:\\.\\d+)?\\s*,\\s*-?\\d+(?:\\.\\d+)?\\s*,\\s*-?\\d+(?:\\.\\d+)?))?(?:\\W+r\\s*=\\s*(-?\\d+(?:\\.\\d+)?\\s*,\\s*-?\\d+(?:\\.\\d+)?\\s*,\\s*-?\\d+(?:\\.\\d+)?))?(?:\\W+\\s*c\\s*=\\s*(-?\\d+(?:\\.\\d+)?))?$", RegexOptions.Compiled);

	private static float ParseFloat(string value, float defaultValue, string line)
	{
		if (string.IsNullOrEmpty(value))
		{
			return defaultValue;
		}
		try
		{
			return float.Parse(value, CultureInfo.InvariantCulture);
		}
		catch (Exception)
		{
			throw new ArgumentException($"Could not parse '{value}' as numeric value in line {line}.");
		}
	}

	private static int ParseInt(string value, int defaultValue, string line)
	{
		if (string.IsNullOrEmpty(value))
		{
			return defaultValue;
		}
		try
		{
			return int.Parse(value);
		}
		catch (Exception)
		{
			throw new ArgumentException($"Could not parse '{value}' as numeric value in line {line}.");
		}
	}

	private static Vector3 ParseVector(string value, string line)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(value))
		{
			return Vector3.zero;
		}
		string[] array = value.Split(',');
		if (array.Length != 3)
		{
			throw new ArgumentException($"A vector requires 3 components, but found {array.Length} in line '{line}'.");
		}
		return new Vector3(ParseFloat(array[0].Trim(), 0f, line), ParseFloat(array[1].Trim(), 0f, line), ParseFloat(array[2].Trim(), 0f, line));
	}

	internal static void ProcessLines(string[] lines)
	{
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		string text = null;
		string text2 = null;
		string text3 = "none";
		foreach (string text4 in lines)
		{
			string text5 = text4.Trim();
			if (text5.Length == 0 || text5.StartsWith("#"))
			{
				continue;
			}
			Match match = SCENE_REGEX.Match(text5);
			if (match.Success)
			{
				text = match.Groups[1].Value;
				text2 = null;
				continue;
			}
			match = TAG_REGEX.Match(text5);
			if (match.Success)
			{
				text3 = match.Groups[1].Value;
				GearSpawnerMod.Logger.Msg("Tag found while reading spawn file. '" + text3 + "'");
				continue;
			}
			match = SPAWN_REGEX.Match(text5);
			if (match.Success)
			{
				if (string.IsNullOrEmpty(text))
				{
					throw new InvalidFormatException("No scene name defined before line '" + text4 + "'. Did you forget a 'scene = <SceneName>'?");
				}
				GearSpawnInfo gearSpawnInfo = default(GearSpawnInfo);
				gearSpawnInfo.PrefabName = match.Groups[1].Value;
				gearSpawnInfo.SpawnChance = ParseFloat(match.Groups[4].Value, 100f, text4);
				gearSpawnInfo.Position = ParseVector(match.Groups[2].Value, text4);
				gearSpawnInfo.Rotation = Quaternion.Euler(ParseVector(match.Groups[3].Value, text4));
				gearSpawnInfo.Tag = text3;
				GearSpawnInfo gearSpawnInfo2 = gearSpawnInfo;
				GearSpawnManager.AddGearSpawnInfo(text, gearSpawnInfo2);
				continue;
			}
			match = LOOTTABLE_REGEX.Match(text5);
			if (match.Success)
			{
				text2 = match.Groups[1].Value;
				text = null;
				continue;
			}
			match = LOOTTABLE_ENTRY_REGEX.Match(text5);
			if (match.Success)
			{
				if (string.IsNullOrEmpty(text2))
				{
					throw new InvalidFormatException("No loottable name defined before line '" + text4 + "'. Did you forget a 'loottable = <LootTableName>'?");
				}
				LootTableEntry lootTableEntry = default(LootTableEntry);
				lootTableEntry.PrefabName = match.Groups[1].Value;
				lootTableEntry.Weight = ParseInt(match.Groups[2].Value, 0, text4);
				LootTableEntry entry = lootTableEntry;
				LootTableManager.AddLootTableEntry(text2, entry);
				continue;
			}
			throw new InvalidFormatException("Unrecognized line '" + text4 + "'.");
		}
	}
}
