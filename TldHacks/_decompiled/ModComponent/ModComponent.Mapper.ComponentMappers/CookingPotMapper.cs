using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppTLD.IntBackedUnit;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Mapper.ComponentMappers;

internal static class CookingPotMapper
{
	internal static void Configure(ModBaseComponent modComponent)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		ModCookingPotComponent modCookingPotComponent = ((Il2CppObjectBase)modComponent).TryCast<ModCookingPotComponent>();
		if (!((Object)(object)modCookingPotComponent == (Object)null))
		{
			CookingPotItem orCreateComponent = ((Component?)(object)modComponent).GetOrCreateComponent<CookingPotItem>();
			orCreateComponent.m_WaterCapacity = ItemLiquidVolume.FromLiters(modCookingPotComponent.Capacity);
			orCreateComponent.m_CanCookGrub = modCookingPotComponent.CanCookGrub;
			orCreateComponent.m_CanCookLiquid = modCookingPotComponent.CanCookLiquid;
			orCreateComponent.m_CanCookMeat = modCookingPotComponent.CanCookMeat;
			orCreateComponent.m_CanOnlyWarmUpFood = false;
			CookingPotItem item = ModUtils.GetItem<CookingPotItem>(modCookingPotComponent.Template, ((Object)modComponent).name);
			orCreateComponent.m_BoilingTimeMultiplier = item.m_BoilingTimeMultiplier;
			orCreateComponent.m_BoilWaterPotMaterialsList = item.m_BoilWaterPotMaterialsList;
			orCreateComponent.m_BoilWaterReadyMaterialsList = item.m_BoilWaterReadyMaterialsList;
			orCreateComponent.m_ConditionPercentDamageFromBoilingDry = item.m_ConditionPercentDamageFromBoilingDry;
			orCreateComponent.m_ConditionPercentDamageFromBurningFood = item.m_ConditionPercentDamageFromBurningFood;
			orCreateComponent.m_CookedCalorieMultiplier = item.m_CookedCalorieMultiplier;
			orCreateComponent.m_CookingTimeMultiplier = item.m_CookingTimeMultiplier;
			orCreateComponent.m_GrubMeshType = item.m_GrubMeshType;
			orCreateComponent.m_LampOilMultiplier = item.m_LampOilMultiplier;
			orCreateComponent.m_MeltSnowMaterialsList = item.m_MeltSnowMaterialsList;
			orCreateComponent.m_NearFireWarmUpCookingTimeMultiplier = item.m_NearFireWarmUpCookingTimeMultiplier;
			orCreateComponent.m_NearFireWarmUpReadyTimeMultiplier = item.m_NearFireWarmUpReadyTimeMultiplier;
			orCreateComponent.m_ParticlesItemCooking = item.m_ParticlesItemCooking;
			orCreateComponent.m_ParticlesItemReady = item.m_ParticlesItemReady;
			orCreateComponent.m_ParticlesItemRuined = item.m_ParticlesItemRuined;
			orCreateComponent.m_ParticlesSnowMelting = item.m_ParticlesSnowMelting;
			orCreateComponent.m_ParticlesWaterBoiling = item.m_ParticlesWaterBoiling;
			orCreateComponent.m_ParticlesWaterReady = item.m_ParticlesWaterReady;
			orCreateComponent.m_ParticlesWaterRuined = item.m_ParticlesWaterRuined;
			orCreateComponent.m_ReadyTimeMultiplier = item.m_ReadyTimeMultiplier;
			orCreateComponent.m_RuinedFoodMaterialsList = item.m_RuinedFoodMaterialsList;
			orCreateComponent.m_SnowMesh = modCookingPotComponent.SnowMesh;
			orCreateComponent.m_WaterMesh = modCookingPotComponent.WaterMesh;
			GameObject val = Object.Instantiate<GameObject>(((Component)item.m_GrubMeshFilter).gameObject, ((Component)orCreateComponent).transform);
			orCreateComponent.m_GrubMeshFilter = val.GetComponent<MeshFilter>();
			orCreateComponent.m_GrubMeshRenderer = val.GetComponent<MeshRenderer>();
			((Component?)(object)modComponent).GetOrCreateComponent<GearItem>().GearItemData.m_IsPlaceable = true;
		}
	}
}
