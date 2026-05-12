namespace GearSpawner;

public abstract class ProbabilisticGearSpawnHandler : GearSpawnHandler
{
	public abstract float GetProbability(DifficultyLevel difficultyLevel, FirearmAvailability firearmAvailability, GearSpawnInfo gearSpawnInfo);

	public sealed override bool ShouldSpawn(DifficultyLevel difficultyLevel, FirearmAvailability firearmAvailability, GearSpawnInfo gearSpawnInfo)
	{
		return GearSpawnHandler.RollChance(GetProbability(difficultyLevel, firearmAvailability, gearSpawnInfo));
	}
}
