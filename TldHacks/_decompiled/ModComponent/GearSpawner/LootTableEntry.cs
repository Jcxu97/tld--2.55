using System;
using UnityEngine;

namespace GearSpawner;

public readonly record struct LootTableEntry(string PrefabName, int Weight)
{
	internal LootTableEntry Normalize()
	{
		LootTableEntry result = default(LootTableEntry);
		result.PrefabName = NormalizePrefabName(PrefabName);
		result.Weight = Mathf.Clamp(Weight, 0, int.MaxValue);
		return result;
	}

	private static string NormalizePrefabName(string prefabName)
	{
		if (!prefabName.StartsWith("GEAR_", StringComparison.InvariantCulture))
		{
			return "GEAR_" + prefabName;
		}
		return prefabName;
	}
}
