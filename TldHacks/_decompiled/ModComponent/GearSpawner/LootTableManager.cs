using System;
using System.Collections.Generic;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppSystem.Collections.Generic;
using Il2CppTLD.Gameplay;
using MelonLoader;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GearSpawner;

[HarmonyPatch]
internal static class LootTableManager
{
	private static Dictionary<string, List<LootTableEntry>> lootTableEntries = new Dictionary<string, List<LootTableEntry>>();

	private static List<int> processedLootTables = new List<int>();

	internal static void AddLootTableEntry(string lootTable, LootTableEntry entry)
	{
		string normalizedLootTableName = GetNormalizedLootTableName(lootTable);
		if (!lootTableEntries.TryGetValue(normalizedLootTableName, out List<LootTableEntry> value))
		{
			value = new List<LootTableEntry>();
			lootTableEntries.Add(normalizedLootTableName, value);
		}
		value.Add(entry.Normalize());
	}

	internal static void ConfigureLootTableData(LootTableData lootTableData, string type = "container")
	{
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Expected O, but got Unknown
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Expected O, but got Unknown
		if ((Object)(object)lootTableData == (Object)null)
		{
			return;
		}
		int instanceID = ((Object)lootTableData).GetInstanceID();
		if (processedLootTables.Contains(instanceID))
		{
			return;
		}
		processedLootTables.Add(instanceID);
		if (!lootTableEntries.TryGetValue(((Object)lootTableData).name.ToLowerInvariant(), out List<LootTableEntry> value))
		{
			return;
		}
		new List<RandomTableDataEntry<AssetReferenceGearItem>>();
		List<string> list = new List<string>();
		Enumerator<RandomTableDataEntry<AssetReferenceGearItem>> enumerator = ((RandomTableData<AssetReferenceGearItem>)(object)lootTableData).m_BaseEntries.GetEnumerator();
		while (enumerator.MoveNext())
		{
			RandomTableDataEntry<AssetReferenceGearItem> current = enumerator.Current;
			list.Add(((AssetReference)current.m_Item).AssetGUID);
		}
		int num = 0;
		foreach (LootTableEntry item in value)
		{
			if (!list.Contains(item.PrefabName))
			{
				RandomTableDataEntry<AssetReferenceGearItem> val = new RandomTableDataEntry<AssetReferenceGearItem>();
				val.m_Item = new AssetReferenceGearItem(item.PrefabName);
				val.m_Weight = item.Weight;
				((RandomTableData<AssetReferenceGearItem>)(object)lootTableData).m_BaseEntries.Add(val);
				((RandomTableData<AssetReferenceGearItem>)(object)lootTableData).m_FilteredExtendedItems.Add(val.m_Item);
				((RandomTableData<AssetReferenceGearItem>)(object)lootTableData).m_ExistingOperations.Add(new IKeyEvaluator(((Il2CppObjectBase)val.m_Item).Pointer), ((AssetReferenceT<GameObject>)(object)val.m_Item).LoadAssetAsync());
				num++;
			}
		}
		if (num > 0)
		{
			MelonLogger.Msg("Processed " + num + " items for " + ((Object)lootTableData).name.ToLowerInvariant() + "(" + type + ")");
		}
	}

	private static string GetNormalizedLootTableName(string lootTable)
	{
		if (lootTable.StartsWith("Loot", StringComparison.InvariantCultureIgnoreCase))
		{
			return lootTable.ToLowerInvariant();
		}
		if (lootTable.StartsWith("Cargo", StringComparison.InvariantCultureIgnoreCase))
		{
			return "loot" + lootTable.ToLowerInvariant();
		}
		return "loottable" + lootTable.ToLowerInvariant();
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Container), "PopulateWithRandomGear")]
	private static void Container_PopulateWithRandomGear(Container __instance)
	{
		if ((Object)(object)__instance.m_LootTable != (Object)null)
		{
			ConfigureLootTableData(__instance.m_LootTable);
		}
		if ((Object)(object)__instance.m_LootTableData != (Object)null)
		{
			ConfigureLootTableData(__instance.m_LootTableData);
		}
		if ((Object)(object)__instance.m_LockedLootTableData != (Object)null)
		{
			ConfigureLootTableData(__instance.m_LockedLootTableData);
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(IceFishingHole), "Awake")]
	private static void IceFishingHole_Awake(IceFishingHole __instance)
	{
		if ((Object)(object)__instance.m_LootTable != (Object)null)
		{
			ConfigureLootTableData((LootTableData)(object)__instance.m_LootTable, "FishingHole");
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(RadialObjectSpawner), "Awake")]
	private static void RadialObjectSpawner_Awake(RadialObjectSpawner __instance)
	{
		if ((Object)(object)__instance.m_LootTableData != (Object)null)
		{
			ConfigureLootTableData(__instance.m_LootTableData, "RadialSpawner");
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(IceFishingHole), "InstantiateFish", new Type[] { typeof(AssetReference) })]
	private static void IceFishingHole_InstantiateFish(IceFishingHole __instance, AssetReference fishReference, ref bool __runOriginal, ref GearItem __result)
	{
		GameObject val = Addressables.LoadAssetAsync<GameObject>(fishReference.RuntimeKey).WaitForCompletion();
		if ((Object)(object)val != (Object)null)
		{
			GameObject val2 = Object.Instantiate<GameObject>(val);
			if ((Object)(object)val2.GetComponent<GearItem>() != (Object)null && (Object)(object)val2.GetComponent<HarvestFish>() == (Object)null)
			{
				GearItem component = val2.GetComponent<GearItem>();
				__result = component;
				__runOriginal = false;
			}
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SaveGameSystem), "LoadSceneData", new Type[]
	{
		typeof(string),
		typeof(string)
	})]
	private static void SaveGameSystem_LoadSceneData(SaveGameSystem __instance, string name, string sceneSaveName)
	{
		processedLootTables.Clear();
	}
}
