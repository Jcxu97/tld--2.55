using System;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppTLD.IntBackedUnit;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Mapper.ComponentMappers;

internal static class CookableMapper
{
	internal static void Configure(ModBaseComponent modComponent)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		ModCookableComponent modCookableComponent = ((Il2CppObjectBase)modComponent).TryCast<ModCookableComponent>();
		if ((Object)(object)modCookableComponent == (Object)null || !modCookableComponent.Cooking)
		{
			return;
		}
		GearItem? orCreateComponent = ((Component?)(object)modCookableComponent).GetOrCreateComponent<GearItem>();
		Cookable orCreateComponent2 = ((Component?)(object)modCookableComponent).GetOrCreateComponent<Cookable>();
		orCreateComponent2.m_CookableType = modCookableComponent.Type;
		orCreateComponent2.m_CookTimeMinutes = modCookableComponent.CookingMinutes;
		orCreateComponent2.m_ReadyTimeMinutes = modCookableComponent.BurntMinutes;
		orCreateComponent2.m_NumUnitsRequired = modCookableComponent.CookingUnitsRequired;
		orCreateComponent2.m_PotableWaterRequired = ItemLiquidVolume.FromLiters(modCookableComponent.CookingWaterRequired);
		orCreateComponent2.m_WarmUpNearFireRange = 1.5f;
		orCreateComponent2.m_CookEvent = ModUtils.MakeAudioEvent(ModUtils.DefaultIfEmpty(modCookableComponent.CookingAudio, GetDefaultCookAudio(modCookableComponent)));
		orCreateComponent2.m_PutInPotEvent = ModUtils.MakeAudioEvent(ModUtils.DefaultIfEmpty(modCookableComponent.StartCookingAudio, GetDefaultStartCookingAudio(modCookableComponent)));
		orCreateComponent.GearItemData.m_CookingSlotPlacementAudio = orCreateComponent2.m_PutInPotEvent;
		Cookable component = AssetBundleUtils.LoadAsset<GameObject>(((int)orCreateComponent2.m_CookableType == 0) ? "GEAR_RawMeatDeer" : "GEAR_PinnacleCanPeaches").GetComponent<Cookable>();
		orCreateComponent2.m_MeshPotStyle = ((component != null) ? component.m_MeshPotStyle : null);
		orCreateComponent2.m_MeshCanStyle = ((component != null) ? component.m_MeshCanStyle : null);
		orCreateComponent2.m_MeshFryingPanStyle = ((component != null) ? component.m_MeshFryingPanStyle : null);
		orCreateComponent2.m_LiquidMeshRenderer = ((component != null) ? component.m_LiquidMeshRenderer : null);
		if ((Object)(object)modCookableComponent.CookingResult == (Object)null)
		{
			FoodItem componentSafe = ((Component?)(object)modCookableComponent).GetComponentSafe<FoodItem>();
			if ((Object)(object)componentSafe != (Object)null)
			{
				componentSafe.m_HeatedWhenCooked = true;
			}
			return;
		}
		GearItem component2 = modCookableComponent.CookingResult.GetComponent<GearItem>();
		if ((Object)(object)component2 == (Object)null)
		{
			AutoMapper.MapModComponent(modCookableComponent.CookingResult);
			component2 = modCookableComponent.CookingResult.GetComponent<GearItem>();
		}
		orCreateComponent2.m_CookedPrefab = component2 ?? throw new ArgumentException("CookingResult does not map to GearItem for prefab " + ((Object)modCookableComponent).name);
	}

	private static string GetDefaultCookAudio(ModCookableComponent modCookableComponent)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		CookableType type = modCookableComponent.Type;
		if ((int)type != 0)
		{
			if ((int)type == 1)
			{
				return "Play_BoilingLiquidThickHeavy";
			}
			return "Play_BoilingLiquidLight";
		}
		return "Play_FryingHeavy";
	}

	private static string GetDefaultStartCookingAudio(ModCookableComponent modCookableComponent)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		CookableType type = modCookableComponent.Type;
		if ((int)type != 0)
		{
			if ((int)type == 1)
			{
				return "PLAY_PUTINPOTSLOP";
			}
			return "PLAY_PUTINPOTWATER";
		}
		return "Play_AddMeatPan";
	}
}
