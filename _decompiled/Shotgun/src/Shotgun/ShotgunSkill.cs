using System;
using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace Shotgun;

internal static class ShotgunSkill
{
	internal delegate(int points, int tier) GetPointsAndTiersDelegate(string skillName);

	internal delegate void IncreaseSkillPointsDelegate(string skillName, int points);

	internal const string SkillName = "Shotgun";

	internal static readonly float[] TierMultipliers = new float[5] { 1f, 1.15f, 1.3f, 1.5f, 1.75f };

	internal const float BaseReloadCooldown = 2.625f;

	internal const float BaseSwayMaxFatigue = 1.3125f;

	internal const float BaseSwayIncrease = 0.175f;

	internal const float BaseSwayDecrease = 0.0857f;

	private static Type mFSkillType;

	internal static GetPointsAndTiersDelegate GetPointsAndTiers { get; private set; }

	internal static IncreaseSkillPointsDelegate IncreaseSkillPoints { get; private set; }

	internal static bool IsAvailable => mFSkillType != null && GetPointsAndTiers != null;

	internal static void Init()
	{
		Type type = AccessTools.TypeByName("FiligranisSkills.FiligranisSkills, FiligranisSkills");
		if (type == null)
		{
			MelonLogger.Warning("FiligranisSkills not found, Shotgun skill will not be available");
			return;
		}
		mFSkillType = type;
		MethodInfo methodInfo = AccessTools.FirstMethod(mFSkillType, (Func<MethodInfo, bool>)((MethodInfo mi) => mi.Name == "GetPointsAndTiers"));
		if (methodInfo == null)
		{
			MelonLogger.Error("GetPointsAndTiers method not found in " + mFSkillType.Name);
		}
		else
		{
			GetPointsAndTiers = methodInfo.CreateDelegate<GetPointsAndTiersDelegate>();
		}
		methodInfo = AccessTools.FirstMethod(mFSkillType, (Func<MethodInfo, bool>)((MethodInfo mi) => mi.Name == "IncreaseSkillPoints"));
		if (methodInfo == null)
		{
			MelonLogger.Error("IncreaseSkillPoints method not found in " + mFSkillType.Name);
		}
		else
		{
			IncreaseSkillPoints = methodInfo.CreateDelegate<IncreaseSkillPointsDelegate>();
		}
		TryRegisterSkill();
	}

	private static void TryRegisterSkill()
	{
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Expected O, but got Unknown
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Expected O, but got Unknown
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Expected O, but got Unknown
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Expected O, but got Unknown
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Expected O, but got Unknown
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Expected O, but got Unknown
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Expected O, but got Unknown
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Expected O, but got Unknown
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Expected O, but got Unknown
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Expected O, but got Unknown
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Expected O, but got Unknown
		if (!(mFSkillType == null))
		{
			MethodInfo methodInfo = AccessTools.Method(mFSkillType, "RegisterSkill", (Type[])null, (Type[])null);
			if (methodInfo == null)
			{
				MelonLogger.Error("RegisterSkill method not found");
				return;
			}
			Type type = AccessTools.TypeByName("FiligranisSkills.SkillDefinition, FiligranisSkills");
			ConstructorInfo constructorInfo = AccessTools.Constructor(type, new Type[9]
			{
				typeof(string),
				typeof(LocalizedString),
				typeof(string),
				typeof(string),
				typeof(string),
				typeof(LocalizedString[]),
				typeof(LocalizedString[]),
				typeof(int[]),
				typeof(Texture2D)
			}, false);
			object obj = constructorInfo.Invoke(new object[9]
			{
				"Shotgun",
				(object)new LocalizedString
				{
					m_LocalizationID = "Shotgun"
				},
				"ico_skill_large_Rifle",
				"ico_skill_large_Rifle",
				"ico_skill_large_Rifle",
				new LocalizedString[5]
				{
					new LocalizedString
					{
						m_LocalizationID = "Novice: You've never handled a shotgun before. The recoil catches you off guard, your aim wavers constantly, and reloading is painfully slow."
					},
					new LocalizedString
					{
						m_LocalizationID = "Beginner: You're getting used to the kick. Your hands are steadier and you fumble less with shells, but there's still a long way to go."
					},
					new LocalizedString
					{
						m_LocalizationID = "Intermediate: The shotgun feels more natural now. You brace properly against the recoil and can keep your sights on target longer."
					},
					new LocalizedString
					{
						m_LocalizationID = "Advanced: Confidence with the shotgun is showing. You handle recoil smoothly, reload with practiced efficiency, and hold steady aim under pressure."
					},
					new LocalizedString
					{
						m_LocalizationID = "Expert: The shotgun is an extension of yourself. Recoil is absorbed instinctively, reloading is fluid, and you can hold aim with remarkable control."
					}
				},
				new LocalizedString[5]
				{
					new LocalizedString
					{
						m_LocalizationID = "Base Shotgun Skill."
					},
					new LocalizedString
					{
						m_LocalizationID = "Recoil, sway, and reload improved by 15%."
					},
					new LocalizedString
					{
						m_LocalizationID = "Recoil, sway, and reload improved by 30%."
					},
					new LocalizedString
					{
						m_LocalizationID = "Recoil, sway, and reload improved by 50%."
					},
					new LocalizedString
					{
						m_LocalizationID = "Recoil, sway, and reload improved by 75%."
					}
				},
				new int[5] { 0, 25, 50, 75, 100 },
				null
			});
			methodInfo.Invoke(null, new object[1] { obj });
			MelonLogger.Msg("Shotgun skill registered with FiligranisSkills");
		}
	}

	internal static int GetCurrentTier()
	{
		if (!IsAvailable)
		{
			return 0;
		}
		return GetPointsAndTiers("Shotgun").tier;
	}

	internal static float GetTierMultiplier()
	{
		int currentTier = GetCurrentTier();
		return TierMultipliers[Math.Clamp(currentTier, 0, TierMultipliers.Length - 1)];
	}

	internal static float GetBadStat(float baseValue, float multiplier)
	{
		return baseValue / multiplier;
	}

	internal static float GetGoodStat(float baseValue, float multiplier)
	{
		return baseValue * multiplier;
	}

	internal static float GetReloadCooldown()
	{
		return GetBadStat(2.625f, GetTierMultiplier());
	}

	internal static float GetSwayMaxFatigue()
	{
		return GetBadStat(1.3125f, GetTierMultiplier());
	}

	internal static float GetSwayIncrease()
	{
		return GetBadStat(0.175f, GetTierMultiplier());
	}

	internal static float GetSwayDecrease()
	{
		return GetGoodStat(0.0857f, GetTierMultiplier());
	}

	internal static float GetRecoilMultiplier()
	{
		return TierMultipliers[4] / GetTierMultiplier();
	}

	internal static void ApplyToGunItem(GunItem gunItem)
	{
		gunItem.m_ReloadCoolDownSeconds = GetReloadCooldown();
		gunItem.m_SwayValueMaxFatigue = GetSwayMaxFatigue();
		gunItem.m_SwayIncreasePerSecond = GetSwayIncrease();
		gunItem.m_SwayDecreasePerSecond = GetSwayDecrease();
	}

	internal static void MaybeIncreaseSkillOnFire()
	{
		if (IncreaseSkillPoints != null)
		{
			double num = new System.Random().NextDouble();
			if (num < 0.33)
			{
				IncreaseSkillPoints("Shotgun", 1);
			}
		}
	}
}
