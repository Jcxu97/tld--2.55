using Il2Cpp;
using Il2CppTLD.Gameplay;
using Il2CppTLD.Gameplay.Tunable;

namespace GearSpawner;

internal static class GameModes
{
	public static DifficultyLevel GetDifficultyLevel()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected I4, but got Unknown
		if (GameManager.IsStoryMode())
		{
			return DifficultyLevel.Storymode;
		}
		ExperienceModeType currentExperienceModeType = ExperienceModeManager.GetCurrentExperienceModeType();
		return (int)currentExperienceModeType switch
		{
			0 => DifficultyLevel.Pilgram, 
			1 => DifficultyLevel.Voyager, 
			2 => DifficultyLevel.Stalker, 
			9 => DifficultyLevel.Interloper, 
			10 => GetCustomDifficultyLevel(), 
			14 => DifficultyLevel.Challenge, 
			15 => DifficultyLevel.Challenge, 
			5 => DifficultyLevel.Challenge, 
			8 => DifficultyLevel.Challenge, 
			7 => DifficultyLevel.Challenge, 
			17 => DifficultyLevel.Challenge, 
			4 => DifficultyLevel.Challenge, 
			6 => DifficultyLevel.Challenge, 
			_ => DifficultyLevel.Other, 
		};
	}

	private static DifficultyLevel GetCustomDifficultyLevel()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected I4, but got Unknown
		CustomTunableLMHV baseResourceAvailability = ((ExperienceMode)GameManager.GetCustomMode()).m_BaseResourceAvailability;
		return (int)baseResourceAvailability switch
		{
			3 => DifficultyLevel.Pilgram, 
			2 => DifficultyLevel.Voyager, 
			1 => DifficultyLevel.Stalker, 
			0 => DifficultyLevel.Interloper, 
			_ => DifficultyLevel.Other, 
		};
	}

	public static FirearmAvailability GetFirearmAvailability()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Invalid comparison between Unknown and I4
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Invalid comparison between Unknown and I4
		if (GameManager.IsStoryMode())
		{
			Episode currentEpisode = SaveGameSystem.m_CurrentEpisode;
			if ((int)currentEpisode > 1)
			{
				return FirearmAvailability.All;
			}
			return FirearmAvailability.Rifle;
		}
		ExperienceModeType currentExperienceModeType = ExperienceModeManager.GetCurrentExperienceModeType();
		if ((int)currentExperienceModeType != 9)
		{
			if ((int)currentExperienceModeType == 10)
			{
				return GetCustomFirearmAvailability();
			}
			return FirearmAvailability.All;
		}
		return FirearmAvailability.None;
	}

	private static FirearmAvailability GetCustomFirearmAvailability()
	{
		FirearmAvailability firearmAvailability = FirearmAvailability.None;
		if (GameManager.GetCustomMode().m_RevolversInWorld)
		{
			firearmAvailability |= FirearmAvailability.Revolver;
		}
		if (GameManager.GetCustomMode().m_RiflesInWorld)
		{
			firearmAvailability |= FirearmAvailability.Rifle;
		}
		return firearmAvailability;
	}
}
