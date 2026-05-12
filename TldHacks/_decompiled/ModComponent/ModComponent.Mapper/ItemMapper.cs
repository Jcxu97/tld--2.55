using System;
using System.Collections.Generic;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem;
using Il2CppTLD.Gear;
using Il2CppTLD.IntBackedUnit;
using ModComponent.API.Behaviours;
using ModComponent.API.Components;
using ModComponent.Mapper.BehaviourMappers;
using ModComponent.Mapper.ComponentMappers;
using ModComponent.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ModComponent.Mapper;

internal static class ItemMapper
{
	private static readonly List<ModBaseComponent> mappedItems = new List<ModBaseComponent>();

	public static void Map(string prefabName)
	{
		GameObject obj = AssetBundleUtils.LoadAsset<GameObject>(prefabName);
		if ((Object)(object)obj == (Object)null)
		{
			throw new ArgumentException("Prefab " + prefabName + " not found");
		}
		GameObject obj2 = ((Il2CppObjectBase)obj).TryCast<GameObject>();
		if ((Object)(object)obj2 == (Object)null)
		{
			throw new ArgumentException("Prefab " + prefabName + " is not a GameObject");
		}
		Map(obj2);
	}

	public static void Map(GameObject prefab)
	{
		if ((Object)(object)prefab == (Object)null)
		{
			throw new ArgumentException("The prefab was NULL.");
		}
		ModBaseComponent modComponent = prefab.GetModComponent();
		if ((Object)(object)modComponent == (Object)null)
		{
			throw new ArgumentException("Prefab " + ((Object)prefab).name + " does not contain a ModComponent.");
		}
		if ((Object)(object)prefab.GetComponent<GearItem>() == (Object)null)
		{
			ConfigureBehaviours(modComponent);
			ConfigureGearItem(modComponent);
			InspectMapper.Configure(modComponent);
			EquippableMapper.Configure(modComponent);
			LiquidMapper.Configure(modComponent);
			PowderMapper.Configure(modComponent);
			FoodMapper.Configure(modComponent);
			CookableMapper.Configure(modComponent);
			CookingPotMapper.Configure(modComponent);
			ClothingMapper.Configure(modComponent);
			CollectibleMapper.Configure(modComponent);
			CharcoalMapper.Configure(modComponent);
			PurificationMapper.Configure(modComponent);
			ResearchMapper.Configure(modComponent);
			FirstAidMapper.Configure(modComponent);
			ToolMapper.Configure(modComponent);
			GenericEquippableMapper.Configure(modComponent);
			BedMapper.Configure(modComponent);
			BodyHarvestMapper.Configure(modComponent);
			AmmoMapper.Configure(modComponent);
			mappedItems.Add(modComponent);
			PostProcess(modComponent);
		}
	}

	internal static void ConfigureBehaviours(ModBaseComponent modComponent)
	{
		AccelerantMapper.Configure(modComponent);
		BurnableMapper.Configure(modComponent);
		FireStarterMapper.Configure(modComponent);
		TinderMapper.Configure(modComponent);
		CarryingCapacityMapper.Configure(modComponent);
		EvolveMapper.Configure(modComponent);
		HarvestableMapper.Configure(modComponent);
		MillableMapper.Configure(modComponent);
		RepairableMapper.Configure(modComponent);
		ScentMapper.Configure(modComponent);
		SharpenableMapper.Configure(modComponent);
		StackableMapper.Configure(modComponent);
	}

	internal static void ConfigureBehaviours(GameObject prefab)
	{
		if ((Object)(object)prefab == (Object)null)
		{
			throw new ArgumentException("The prefab was NULL.");
		}
		ModBaseComponent? modComponent = prefab.GetModComponent();
		if ((Object)(object)modComponent == (Object)null)
		{
			throw new ArgumentException("Prefab " + ((Object)prefab).name + " does not contain a ModComponent.");
		}
		ConfigureBehaviours(modComponent);
	}

	internal static float GetDecayPerStep(float steps, float maxHP)
	{
		if (!(steps > 0f))
		{
			return 0f;
		}
		return maxHP / steps;
	}

	private static void ConfigureGearItem(ModBaseComponent modComponent)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Expected O, but got Unknown
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		GearItem orCreateComponent = ((Component?)(object)modComponent).GetOrCreateComponent<GearItem>();
		HoverIconsToShow orCreateComponent2 = ((Component?)(object)modComponent).GetOrCreateComponent<HoverIconsToShow>();
		if ((Object)(object)orCreateComponent.GearItemData == (Object)null)
		{
			GearItemData gearItemData = ScriptableObject.CreateInstance<GearItemData>();
			orCreateComponent.m_GearItemData = gearItemData;
		}
		GearItemInventoryIconSimpleData val = ScriptableObject.CreateInstance<GearItemInventoryIconSimpleData>();
		((Object)val).name = ((Object)modComponent).name.Replace("GEAR_", "ico_GearItem__");
		val.m_Icon = Addressables.LoadAssetAsync<Texture2D>(Object.op_Implicit(((Object)val).name)).WaitForCompletion();
		orCreateComponent.GearItemData.m_PrefabReference = new AssetReferenceGearItem(((Object)modComponent).name);
		orCreateComponent.GearItemData.m_CoverFlowBlendTexture = new AssetReferenceTexture2D(((Object)val).name);
		orCreateComponent.GearItemData.m_CoverFlowDamageTexture = new AssetReferenceTexture2D(((Object)val).name);
		orCreateComponent.GearItemData.m_CoverFlowMainTexture = new AssetReferenceTexture2D(((Object)val).name);
		orCreateComponent.GearItemData.m_CoverFlowOpenedTexture = new AssetReferenceTexture2D(((Object)val).name);
		orCreateComponent.GearItemData.m_IconData = (GearItemInventoryIconData)(object)val;
		orCreateComponent.GearItemData.m_Type = GetGearType(modComponent);
		orCreateComponent.GearItemData.m_BaseWeight = ItemWeight.FromKilograms(modComponent.WeightKG);
		orCreateComponent.GearItemData.m_MaxHP = modComponent.MaxHP;
		orCreateComponent.GearItemData.m_DailyHPDecay = GetDecayPerStep(modComponent.DaysToDecay, modComponent.MaxHP);
		orCreateComponent.m_StartCondition = modComponent.InitialCondition;
		orCreateComponent.GearItemData.m_LocalizedName = NameUtils.CreateLocalizedString(modComponent.DisplayNameLocalizationId);
		orCreateComponent.GearItemData.m_LocalizedDescription = NameUtils.CreateLocalizedString(modComponent.DescriptionLocalizatonId);
		orCreateComponent.GearItemData.m_PickupAudio = ModUtils.MakeAudioEvent(modComponent.PickUpAudio);
		orCreateComponent.GearItemData.m_StowAudio = ModUtils.MakeAudioEvent(modComponent.StowAudio);
		orCreateComponent.GearItemData.m_PutBackAudio = ModUtils.MakeAudioEvent(modComponent.PutBackAudio);
		orCreateComponent.GearItemData.m_WornOutAudio = ModUtils.MakeAudioEvent(modComponent.WornOutAudio);
		orCreateComponent.GearItemData.m_CookingSlotPlacementAudio = ModUtils.MakeAudioEvent(null);
		orCreateComponent.GearItemData.m_ConditionType = GetConditionTableType(modComponent);
		orCreateComponent.GearItemData.m_ScentIntensity = ScentMapper.GetScentIntensity(modComponent);
		orCreateComponent.GearItemData.m_IsPlaceable = false;
		orCreateComponent2.m_HoverIcons = GetHoverIconsToShow(modComponent);
		orCreateComponent.Awake();
	}

	private static Il2CppStructArray<HoverIcons> GetHoverIconsToShow(ModBaseComponent modComponent)
	{
		Il2CppStructArray<HoverIcons> val = new Il2CppStructArray<HoverIcons>(1L);
		switch (modComponent.InventoryCategory)
		{
		case ModBaseComponent.ItemCategory.Tool:
			((Il2CppArrayBase<HoverIcons>)(object)val)[0] = (HoverIcons)3;
			break;
		case ModBaseComponent.ItemCategory.Firestarting:
			((Il2CppArrayBase<HoverIcons>)(object)val)[0] = (HoverIcons)0;
			break;
		case ModBaseComponent.ItemCategory.FirstAid:
			((Il2CppArrayBase<HoverIcons>)(object)val)[0] = (HoverIcons)5;
			break;
		case ModBaseComponent.ItemCategory.Food:
			((Il2CppArrayBase<HoverIcons>)(object)val)[0] = (HoverIcons)1;
			break;
		case ModBaseComponent.ItemCategory.Clothing:
			((Il2CppArrayBase<HoverIcons>)(object)val)[0] = (HoverIcons)4;
			break;
		case ModBaseComponent.ItemCategory.Material:
			((Il2CppArrayBase<HoverIcons>)(object)val)[0] = (HoverIcons)2;
			break;
		case ModBaseComponent.ItemCategory.Auto:
			return new Il2CppStructArray<HoverIcons>(0L);
		default:
			return new Il2CppStructArray<HoverIcons>(0L);
		}
		return val;
	}

	private static ConditionTableType GetConditionTableType(ModBaseComponent modComponent)
	{
		ModFoodComponent modFoodComponent = ((Il2CppObjectBase)modComponent).TryCast<ModFoodComponent>();
		if ((Object)(object)modFoodComponent != (Object)null)
		{
			if (!modFoodComponent.Canned)
			{
				if (!modFoodComponent.Meat)
				{
					if (!modFoodComponent.Natural && !modFoodComponent.Drink)
					{
						return (ConditionTableType)1;
					}
					return (ConditionTableType)(-1);
				}
				return (ConditionTableType)0;
			}
			return (ConditionTableType)2;
		}
		if (!((Object)(object)((Il2CppObjectBase)modComponent).TryCast<ModAmmoComponent>() != (Object)null))
		{
			return (ConditionTableType)(-1);
		}
		return (ConditionTableType)4;
	}

	private static GearType GetGearType(ModBaseComponent modComponent)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (modComponent.InventoryCategory != 0)
		{
			return EnumUtils.TranslateEnumValue<GearType, ModBaseComponent.ItemCategory>(modComponent.InventoryCategory);
		}
		if (!(modComponent is ModToolComponent))
		{
			if (!(modComponent is ModFoodComponent) && !(modComponent is ModCookableComponent))
			{
				ModLiquidComponent obj = modComponent as ModLiquidComponent;
				if (obj == null || obj.LiquidType != 0)
				{
					if (!(modComponent is ModClothingComponent))
					{
						if (!((Object)(object)((Component?)(object)modComponent).GetComponentSafe<ModFireMakingBaseBehaviour>() != (Object)null) && !((Object)(object)((Component?)(object)modComponent).GetComponentSafe<ModBurnableBehaviour>() != (Object)null))
						{
							return (GearType)64;
						}
						return (GearType)32;
					}
					return (GearType)2;
				}
			}
			return (GearType)1;
		}
		return (GearType)8;
	}

	private static void PostProcess(ModBaseComponent modComponent)
	{
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		((Component)modComponent).gameObject.layer = vp_Layer.Gear;
		GearItem component = ((Component)modComponent).GetComponent<GearItem>();
		component.m_SkinnedMeshRenderers = Il2CppReferenceArray<SkinnedMeshRenderer>.op_Implicit(ModUtils.NotNull(Il2CppArrayBase<SkinnedMeshRenderer>.op_Implicit((Il2CppArrayBase<SkinnedMeshRenderer>)(object)component.m_SkinnedMeshRenderers)));
		MeshRenderer componentInChildren = AssetBundleUtils.LoadAsset<GameObject>("GEAR_CoffeeCup").GetComponentInChildren<MeshRenderer>();
		foreach (MeshRenderer item in (Il2CppArrayBase<MeshRenderer>)(object)component.m_MeshRenderers)
		{
			foreach (Material item2 in (Il2CppArrayBase<Material>)(object)((Renderer)item).materials)
			{
				if (((Object)item2.shader).name == "Standard")
				{
					item2.shader = ((Renderer)componentInChildren).material.shader;
					item2.shaderKeywords = ((Renderer)componentInChildren).material.shaderKeywords;
				}
			}
		}
		ConsoleWaitlist.MaybeRegisterConsoleGearName(modComponent.GetEffectiveConsoleName(), ((Object)modComponent).name, component.GearItemData.m_Type);
	}
}
