using System;
using UnityEngine;

namespace GearSpawner;

public class DefaultGearSpawnHandler : ProbabilisticGearSpawnHandler
{
	public override float GetProbability(DifficultyLevel difficultyLevel, FirearmAvailability firearmAvailability, GearSpawnInfo gearSpawnInfo)
	{
		return GetAdjustedProbability(difficultyLevel, gearSpawnInfo.SpawnChance);
	}

	private static float GetAdjustedProbability(DifficultyLevel difficultyLevel, float baseProbability)
	{
		float num = difficultyLevel switch
		{
			DifficultyLevel.Pilgram => Math.Max(0f, Settings.instance.pilgramSpawnProbabilityMultiplier), 
			DifficultyLevel.Voyager => Math.Max(0f, Settings.instance.voyagerSpawnProbabilityMultiplier), 
			DifficultyLevel.Stalker => Math.Max(0f, Settings.instance.stalkerSpawnProbabilityMultiplier), 
			DifficultyLevel.Interloper => Math.Max(0f, Settings.instance.interloperSpawnProbabilityMultiplier), 
			DifficultyLevel.Challenge => Math.Max(0f, Settings.instance.challengeSpawnProbabilityMultiplier), 
			_ => 1f, 
		};
		if (num == 0f)
		{
			return 0f;
		}
		float num2 = Mathf.Clamp(baseProbability, 0f, 100f);
		if (num2 == 100f)
		{
			return 100f;
		}
		return Mathf.Clamp(num * num2, 0f, 100f);
	}
}
