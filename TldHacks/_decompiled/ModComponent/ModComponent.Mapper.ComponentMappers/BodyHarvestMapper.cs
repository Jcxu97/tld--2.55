using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppTLD.IntBackedUnit;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Mapper.ComponentMappers;

internal static class BodyHarvestMapper
{
	internal static void Configure(ModBaseComponent modComponent)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		ModBodyHarvestComponent modBodyHarvestComponent = ((Il2CppObjectBase)modComponent).TryCast<ModBodyHarvestComponent>();
		if (!((Object)(object)modBodyHarvestComponent == (Object)null))
		{
			BodyHarvest orCreateComponent = ((Component?)(object)modBodyHarvestComponent).GetOrCreateComponent<BodyHarvest>();
			GearItem? componentSafe = ((Component?)(object)modBodyHarvestComponent).GetComponentSafe<GearItem>();
			((Component?)(object)modBodyHarvestComponent).GetComponentSafe<Inspect>();
			componentSafe.m_BodyHarvest = orCreateComponent;
			orCreateComponent.m_AllowDecay = false;
			orCreateComponent.m_HarvestAudio = modBodyHarvestComponent.HarvestAudio;
			orCreateComponent.m_LocalizedDisplayName = NameUtils.CreateLocalizedString(modBodyHarvestComponent.DisplayNameLocalizationId);
			orCreateComponent.m_GutAvailableUnits = modBodyHarvestComponent.GutQuantity;
			orCreateComponent.m_GutPrefab = AssetBundleUtils.LoadAsset<GameObject>(modBodyHarvestComponent.GutPrefab);
			orCreateComponent.m_GutWeightKgPerUnit = ItemWeight.FromKilograms(modBodyHarvestComponent.GutWeightKgPerUnit);
			orCreateComponent.m_HideAvailableUnits = modBodyHarvestComponent.HideQuantity;
			orCreateComponent.m_HidePrefab = AssetBundleUtils.LoadAsset<GameObject>(modBodyHarvestComponent.HidePrefab);
			orCreateComponent.m_HideWeightKgPerUnit = ItemWeight.FromKilograms(modBodyHarvestComponent.HideWeightKgPerUnit);
			orCreateComponent.m_MeatAvailableMax = ItemWeight.FromKilograms(modBodyHarvestComponent.MeatAvailableMaxKG);
			orCreateComponent.m_MeatAvailableMin = ItemWeight.FromKilograms(modBodyHarvestComponent.MeatAvailableMinKG);
			orCreateComponent.m_MeatAvailableKG = ItemWeight.FromKilograms(Random.Range(modBodyHarvestComponent.MeatAvailableMinKG, modBodyHarvestComponent.MeatAvailableMaxKG));
			orCreateComponent.m_MeatPrefab = AssetBundleUtils.LoadAsset<GameObject>(modBodyHarvestComponent.MeatPrefab);
			orCreateComponent.m_CanQuarter = modBodyHarvestComponent.CanQuarter;
			orCreateComponent.m_QuarterAudio = modBodyHarvestComponent.QuarterAudio;
			orCreateComponent.m_QuarterBagMeatCapacity = ItemWeight.FromKilograms(modBodyHarvestComponent.QuarterBagMeatCapacityKG);
			orCreateComponent.m_QuarterBagWasteMultiplier = modBodyHarvestComponent.QuarterBagWasteMultiplier;
			orCreateComponent.m_QuarterDurationMinutes = modBodyHarvestComponent.QuarterDurationMinutes;
			orCreateComponent.m_QuarterObjectPrefab = AssetBundleUtils.LoadAsset<GameObject>(modBodyHarvestComponent.QuarterObjectPrefab);
			orCreateComponent.m_QuarterPrefabSpawnRadius = modBodyHarvestComponent.QuarterPrefabSpawnRadius;
			orCreateComponent.m_StartRavaged = false;
			orCreateComponent.m_Ravaged = false;
			orCreateComponent.m_StartFrozen = false;
			orCreateComponent.m_PercentFrozen = 0f;
			orCreateComponent.m_StartConditionMax = 100f;
			orCreateComponent.m_StartConditionMin = 95f;
			orCreateComponent.m_GutLabelOverride = NameUtils.CreateLocalizedString(modBodyHarvestComponent.GutLabelOverride);
			orCreateComponent.m_HideLabelOverride = NameUtils.CreateLocalizedString(modBodyHarvestComponent.HideLabelOverride);
			orCreateComponent.m_MeatLabelOverride = NameUtils.CreateLocalizedString(modBodyHarvestComponent.MeatLabelOverride);
		}
	}
}
