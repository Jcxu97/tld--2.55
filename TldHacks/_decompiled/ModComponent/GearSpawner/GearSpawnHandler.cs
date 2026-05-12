using System;

namespace GearSpawner;

public abstract class GearSpawnHandler
{
	public abstract bool ShouldSpawn(DifficultyLevel difficultyLevel, FirearmAvailability firearmAvailability, GearSpawnInfo gearSpawnInfo);

	public static bool RollChance(float percent)
	{
		if (!(percent <= 0f))
		{
			if (percent >= 100f)
			{
				return true;
			}
			return RandomFloat() < percent;
		}
		return false;
		static float RandomFloat()
		{
			return new Random().Next(0, 100);
		}
	}
}
