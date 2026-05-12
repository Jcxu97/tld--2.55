using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Mapper.ComponentMappers;

internal static class ClothingMapper
{
	internal static void Configure(ModBaseComponent modComponent)
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		ModClothingComponent modClothingComponent = ((Il2CppObjectBase)modComponent).TryCast<ModClothingComponent>();
		if (!((Object)(object)modClothingComponent == (Object)null))
		{
			ClothingItem? orCreateComponent = ((Component?)(object)modClothingComponent).GetOrCreateComponent<ClothingItem>();
			orCreateComponent.m_DailyHPDecayWhenWornInside = ItemMapper.GetDecayPerStep(modClothingComponent.DaysToDecayWornInside, modClothingComponent.MaxHP);
			orCreateComponent.m_DailyHPDecayWhenWornOutside = ItemMapper.GetDecayPerStep(modClothingComponent.DaysToDecayWornOutside, modClothingComponent.MaxHP);
			orCreateComponent.m_DryBonusWhenNotWorn = 1.5f;
			orCreateComponent.m_DryPercentPerHour = 100f / modClothingComponent.HoursToDryNearFire;
			orCreateComponent.m_DryPercentPerHourNoFire = 100f / modClothingComponent.HoursToDryWithoutFire;
			orCreateComponent.m_FreezePercentPerHour = 100f / modClothingComponent.HoursToFreeze;
			orCreateComponent.m_Region = EnumUtils.TranslateEnumValue<ClothingRegion, ModClothingComponent.BodyRegion>(modClothingComponent.Region);
			orCreateComponent.m_MaxLayer = modClothingComponent.MaxLayer;
			orCreateComponent.m_MinLayer = modClothingComponent.MinLayer;
			orCreateComponent.m_PaperDollTextureName = modClothingComponent.MainTexture;
			orCreateComponent.m_PaperDollBlendmapName = modClothingComponent.BlendTexture;
			orCreateComponent.m_Warmth = modClothingComponent.Warmth;
			orCreateComponent.m_WarmthWhenWet = modClothingComponent.WarmthWhenWet;
			orCreateComponent.m_Waterproofness = modClothingComponent.Waterproofness / 100f;
			orCreateComponent.m_Windproof = modClothingComponent.Windproof;
			orCreateComponent.m_SprintBarReductionPercent = modClothingComponent.SprintBarReduction;
			orCreateComponent.m_Toughness = modClothingComponent.Toughness;
			orCreateComponent.m_FirstPersonPrefabFemale = (string.IsNullOrEmpty(modClothingComponent.FirstPersonPrefabMale) ? ((AssetReferenceFirstPersonClothing)null) : new AssetReferenceFirstPersonClothing(modClothingComponent.FirstPersonPrefabMale));
			orCreateComponent.m_FirstPersonPrefabMale = (string.IsNullOrEmpty(modClothingComponent.FirstPersonPrefabFemale) ? ((AssetReferenceFirstPersonClothing)null) : new AssetReferenceFirstPersonClothing(modClothingComponent.FirstPersonPrefabFemale));
			orCreateComponent.m_PreventAllDamageFromSource = modClothingComponent.PreventAllDamageFromSource;
			ConfigureWolfIntimidation(modClothingComponent);
		}
	}

	private static void ConfigureWolfIntimidation(ModClothingComponent modClothingItem)
	{
		if (modClothingItem.DecreaseAttackChance != 0 || modClothingItem.IncreaseFleeChance != 0)
		{
			WolfIntimidationBuff? orCreateComponent = ((Component?)(object)modClothingItem).GetOrCreateComponent<WolfIntimidationBuff>();
			orCreateComponent.m_DecreaseAttackChancePercentagePoints = modClothingItem.DecreaseAttackChance;
			orCreateComponent.m_IncreaseFleePercentagePoints = modClothingItem.IncreaseFleeChance;
		}
	}
}
