using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Collections.Generic;
using Il2CppTLD.Gameplay;
using Il2CppTLD.IntBackedUnit;
using ModComponent.API;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Mapper.ComponentMappers;

internal static class FoodMapper
{
	internal static void Configure(ModBaseComponent modComponent)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_034b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_037f: Unknown result type (might be due to invalid IL or missing references)
		ModFoodComponent modFoodComponent = ((Il2CppObjectBase)modComponent).TryCast<ModFoodComponent>();
		if ((Object)(object)modFoodComponent == (Object)null)
		{
			return;
		}
		FoodItem orCreateComponent = ((Component?)(object)modFoodComponent).GetOrCreateComponent<FoodItem>();
		GearItem orCreateComponent2 = ((Component?)(object)modFoodComponent).GetOrCreateComponent<GearItem>();
		orCreateComponent.m_Nutrients = new List<Nutrient>();
		if (modFoodComponent.VitaminC > 0)
		{
			Nutrient val = new Nutrient();
			val.m_Amount = modFoodComponent.VitaminC;
			val.m_Nutrient = new AssetReferenceNutrientDefinition("13a8bda1e12982e428b7551cc01b01df");
			orCreateComponent.m_Nutrients.Add(val);
		}
		orCreateComponent.m_CaloriesTotal = modFoodComponent.Calories;
		orCreateComponent.m_CaloriesRemaining = modFoodComponent.Calories;
		orCreateComponent.m_ReduceThirst = modFoodComponent.ThirstEffect;
		orCreateComponent.m_ChanceFoodPoisoning = Mathf.Clamp01((float)modFoodComponent.FoodPoisoning / 100f);
		orCreateComponent.m_ChanceFoodPoisoningLowCondition = Mathf.Clamp01((float)modFoodComponent.FoodPoisoningLowCondition / 100f);
		orCreateComponent.m_ChanceFoodPoisoningRuined = Mathf.Clamp01((float)modFoodComponent.FoodPoisoningLowCondition / 100f);
		orCreateComponent.m_DailyHPDecayInside = ItemMapper.GetDecayPerStep(modFoodComponent.DaysToDecayIndoors, modFoodComponent.MaxHP);
		orCreateComponent.m_DailyHPDecayOutside = ItemMapper.GetDecayPerStep(modFoodComponent.DaysToDecayOutdoors, modFoodComponent.MaxHP);
		orCreateComponent.m_TimeToEatSeconds = Mathf.Clamp(1, modFoodComponent.EatingTime, 10);
		orCreateComponent.m_TimeToOpenAndEatSeconds = Mathf.Clamp(1, modFoodComponent.EatingTime, 10) + 5;
		orCreateComponent.m_EatingAudio = modFoodComponent.EatingAudio;
		orCreateComponent.m_OpenAndEatingAudio = modFoodComponent.EatingPackagedAudio;
		orCreateComponent.m_Packaged = !string.IsNullOrEmpty(orCreateComponent.m_OpenAndEatingAudio);
		orCreateComponent.m_IsDrink = modFoodComponent.Drink;
		orCreateComponent.m_IsFish = modFoodComponent.Fish;
		orCreateComponent.m_IsMeat = modFoodComponent.Meat;
		orCreateComponent.m_IsRawMeat = modFoodComponent.Raw;
		orCreateComponent.m_IsNatural = modFoodComponent.Natural;
		orCreateComponent.m_MustConsumeAll = false;
		orCreateComponent.m_ParasiteRiskPercentIncrease = Il2CppStructArray<float>.op_Implicit(ModUtils.NotNull(modFoodComponent.ParasiteRiskIncrements));
		orCreateComponent.m_PercentHeatLossPerMinuteIndoors = 1f;
		orCreateComponent.m_PercentHeatLossPerMinuteOutdoors = 2f;
		if (modFoodComponent.Opening)
		{
			orCreateComponent.m_GearRequiredToOpen = true;
			orCreateComponent.m_OpenedWithCanOpener = modFoodComponent.OpeningWithCanOpener;
			orCreateComponent.m_OpenedWithHatchet = modFoodComponent.OpeningWithHatchet;
			orCreateComponent.m_OpenedWithKnife = modFoodComponent.OpeningWithKnife;
			if (modFoodComponent.OpeningWithSmashing)
			{
				SmashableItem? orCreateComponent3 = ((Component?)(object)modFoodComponent).GetOrCreateComponent<SmashableItem>();
				orCreateComponent3.m_MinPercentLoss = 10;
				orCreateComponent3.m_MaxPercentLoss = 30;
				orCreateComponent3.m_TimeToSmash = 6;
				orCreateComponent3.m_SmashAudio = "Play_EatingSmashCan";
			}
			if (modFoodComponent.Canned)
			{
				orCreateComponent.m_GearPrefabHarvestAfterFinishEatingNormal = AssetBundleUtils.LoadAsset<GameObject>("GEAR_RecycledCan");
			}
		}
		if (modFoodComponent.AffectRest)
		{
			FatigueBuff? orCreateComponent4 = ((Component?)(object)modFoodComponent).GetOrCreateComponent<FatigueBuff>();
			orCreateComponent4.m_InitialPercentDecrease = modFoodComponent.InstantRestChange;
			orCreateComponent4.m_RateOfIncreaseScale = 0.5f;
			orCreateComponent4.m_DurationHours = (float)modFoodComponent.RestFactorMinutes / 60f;
		}
		if (modFoodComponent.AffectCold)
		{
			FreezingBuff? orCreateComponent5 = ((Component?)(object)modFoodComponent).GetOrCreateComponent<FreezingBuff>();
			orCreateComponent5.m_InitialPercentDecrease = modFoodComponent.InstantColdChange;
			orCreateComponent5.m_RateOfIncreaseScale = 0.5f;
			orCreateComponent5.m_DurationHours = (float)modFoodComponent.ColdFactorMinutes / 60f;
		}
		if (modFoodComponent.AffectCondition)
		{
			ConditionRestBuff? orCreateComponent6 = ((Component?)(object)modFoodComponent).GetOrCreateComponent<ConditionRestBuff>();
			orCreateComponent6.m_ConditionRestBonus = modFoodComponent.ConditionRestBonus;
			orCreateComponent6.m_NumHoursRestAffected = modFoodComponent.ConditionRestMinutes / 60f;
		}
		if (modFoodComponent.ContainsAlcohol)
		{
			AlcoholComponent? orCreateComponent7 = ((Component?)(object)modFoodComponent).GetOrCreateComponent<AlcoholComponent>();
			orCreateComponent7.AmountTotal = modFoodComponent.WeightKG * modFoodComponent.AlcoholPercentage * 0.01f;
			orCreateComponent7.AmountRemaining = orCreateComponent7.AmountTotal;
			orCreateComponent7.UptakeSeconds = modFoodComponent.AlcoholUptakeMinutes * 60f;
		}
		if (modFoodComponent.Fish)
		{
			FoodWeight orCreateComponent8 = ((Component?)(object)modFoodComponent).GetOrCreateComponent<FoodWeight>();
			orCreateComponent8.m_CaloriesPerKG = orCreateComponent.m_CaloriesTotal / (float)orCreateComponent2.WeightKG.m_Units;
			orCreateComponent8.m_MaxWeight = orCreateComponent2.WeightKG * 1.2f;
			orCreateComponent8.m_MinWeight = orCreateComponent2.WeightKG * 0.8f;
			orCreateComponent2.m_FoodWeight = orCreateComponent8;
			orCreateComponent2.WeightKG = ItemWeight.Zero;
		}
		orCreateComponent2.m_FoodItem = orCreateComponent;
		((Component?)(object)modFoodComponent).GetOrCreateComponent<HoverIconsToShow>().m_HoverIcons = Il2CppStructArray<HoverIcons>.op_Implicit((HoverIcons[])(object)new HoverIcons[1] { (HoverIcons)1 });
	}
}
