using System;
using System.Collections.Generic;
using System.Diagnostics;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppSystem;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GearSpawner;

[HarmonyPatch]
internal static class GearSpawnManager
{
	private static readonly Dictionary<string, List<GearSpawnInfo>> gearSpawnInfos = new Dictionary<string, List<GearSpawnInfo>>();

	internal static void AddGearSpawnInfo(string sceneName, GearSpawnInfo gearSpawnInfo)
	{
		string normalizedSceneName = GetNormalizedSceneName(sceneName);
		if (!gearSpawnInfos.TryGetValue(normalizedSceneName, out List<GearSpawnInfo> value))
		{
			value = new List<GearSpawnInfo>();
			gearSpawnInfos.Add(normalizedSceneName, value);
		}
		value.Add(gearSpawnInfo.NormalizePrefabName());
	}

	private static string GetNormalizedGearName(string gearName)
	{
		if (!gearName.StartsWith("GEAR_"))
		{
			return "GEAR_" + gearName;
		}
		return gearName;
	}

	private static string GetNormalizedSceneName(string sceneName)
	{
		return sceneName.ToLowerInvariant();
	}

	private static IEnumerable<GearSpawnInfo>? GetSpawnInfos(string sceneName)
	{
		if (gearSpawnInfos.TryGetValue(sceneName, out List<GearSpawnInfo> value))
		{
			GearSpawnerMod.Logger.Msg($"Found {value.Count} spawn entries for '{sceneName}'");
			return value;
		}
		GearSpawnerMod.Logger.Msg("Could not find any spawn entries for '" + sceneName + "'");
		return null;
	}

	internal static void PrepareScene()
	{
		if (!IsNonGameScene())
		{
			string activeScene = GameManager.m_ActiveScene;
			SpawnManager.InvokeStartSpawningEvent(activeScene);
			GearSpawnerMod.Logger.Msg("Spawning items for scene '" + activeScene + "' ...");
			Stopwatch stopwatch = Stopwatch.StartNew();
			IReadOnlyList<GearItem> items = SpawnGearForScene(GetNormalizedSceneName(activeScene));
			stopwatch.Stop();
			GearSpawnerMod.Logger.Msg($"Spawned '{GameModes.GetDifficultyLevel()}' items for scene '{activeScene}' in {stopwatch.ElapsedMilliseconds} ms");
			SpawnManager.InvokeFinishSpawningEvent(items);
		}
	}

	internal static bool IsNonGameScene()
	{
		string activeScene = GameManager.m_ActiveScene;
		if (activeScene != null && (activeScene == null || activeScene.Length != 0))
		{
			switch (activeScene)
			{
			case "MainMenu":
			case "Boot":
			case "Empty":
				break;
			default:
				return false;
			}
		}
		return true;
	}

	private static IReadOnlyList<GearItem> SpawnGearForScene(string sceneName)
	{
		IEnumerable<GearSpawnInfo> spawnInfos = GetSpawnInfos(sceneName);
		if (spawnInfos == null)
		{
			return Array.Empty<GearItem>();
		}
		DifficultyLevel difficultyLevel = GameModes.GetDifficultyLevel();
		FirearmAvailability firearmAvailability = GameModes.GetFirearmAvailability();
		List<GearItem> list = new List<GearItem>();
		foreach (GearSpawnInfo item in spawnInfos)
		{
			if (ShouldSpawn(difficultyLevel, firearmAvailability, item))
			{
				GameObject val = SpawnGear(sceneName, item);
				if ((Object)(object)val != (Object)null)
				{
					list.Add(val.GetComponent<GearItem>());
				}
			}
		}
		return list;
	}

	private static bool ShouldSpawn(DifficultyLevel difficultyLevel, FirearmAvailability firearmAvailability, GearSpawnInfo gearSpawnInfo)
	{
		if (!Settings.instance.alwaysSpawnItems)
		{
			return SpawnTagManager.GetHandler(gearSpawnInfo.Tag).ShouldSpawn(difficultyLevel, firearmAvailability, gearSpawnInfo);
		}
		return true;
	}

	private static GameObject? SpawnGear(string sceneName, GearSpawnInfo gearSpawnInfo)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit(gearSpawnInfo.PrefabName)).WaitForCompletion();
		if ((Object)(object)val == (Object)null)
		{
			GearSpawnerMod.Logger.Warning($"Could not find prefab '{gearSpawnInfo.PrefabName}' to spawn in scene '{sceneName}'.");
			return null;
		}
		GameObject obj = ((Il2CppObjectBase)Object.Instantiate<GameObject>(val, gearSpawnInfo.Position, gearSpawnInfo.Rotation)).Cast<GameObject>();
		((Object)obj).name = ((Object)val).name;
		EnableObjectForXPMode(obj);
		return obj;
	}

	private static void EnableObjectForXPMode(GameObject gameObject)
	{
		DisableObjectForXPMode component = gameObject.GetComponent<DisableObjectForXPMode>();
		if ((Object)(object)component != (Object)null)
		{
			Object.Destroy((Object)(object)component);
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(QualitySettingsManager), "ApplyCurrentQualitySettings")]
	internal static void GameManager_ApplyCurrentQualitySettings()
	{
		PrepareScene();
	}
}
