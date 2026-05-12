using System;
using ModComponent.API.Behaviours;
using ModComponent.API.Components;
using ModComponent.API.Modifications;
using ModComponent.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace ModComponent.Mapper;

internal static class ComponentJson
{
	public static void InitializeComponents(GameObject prefab)
	{
		if ((Object)(object)prefab == (Object)null)
		{
			throw new ArgumentNullException("prefab");
		}
		if ((Object)(object)prefab.GetModComponent() != (Object)null)
		{
			return;
		}
		string text = NameUtils.RemoveGearPrefix(((Object)prefab).name);
		Logger.LogDebug("Initializing components for " + text);
		try
		{
			string jsonText = JsonHandler.GetJsonText(text);
			if (string.IsNullOrEmpty(jsonText))
			{
				throw new Exception("Json data for " + text + " was null or empty");
			}
			JsonDict jsonDict = JsonConvert.DeserializeObject<JsonDict>(jsonText);
			InitializeComponents(text, prefab, jsonDict);
		}
		catch (Exception value)
		{
			Logger.LogError($"Failed to initialize {text}:: {value}");
		}
	}

	private static void InitializeComponents(string name, GameObject prefab, JsonDict jsonDict)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if ((Object)(object)prefab == (Object)null)
		{
			throw new ArgumentNullException("prefab");
		}
		if (jsonDict == null)
		{
			throw new ArgumentNullException("jsonDict");
		}
		if (!((Object)(object)prefab.GetModComponent() != (Object)null))
		{
			if (jsonDict.ContainsKey("ModBedComponent"))
			{
				prefab.GetOrCreateComponent<ModBedComponent>().InitializeComponent(jsonDict, "ModBedComponent");
			}
			else if (jsonDict.ContainsKey("ModBodyHarvestComponent"))
			{
				prefab.GetOrCreateComponent<ModBodyHarvestComponent>().InitializeComponent(jsonDict, "ModBodyHarvestComponent");
			}
			else if (jsonDict.ContainsKey("ModCharcoalComponent"))
			{
				prefab.GetOrCreateComponent<ModCharcoalComponent>().InitializeComponent(jsonDict, "ModCharcoalComponent");
			}
			else if (jsonDict.ContainsKey("ModClothingComponent"))
			{
				prefab.GetOrCreateComponent<ModClothingComponent>().InitializeComponent(jsonDict, "ModClothingComponent");
			}
			else if (jsonDict.ContainsKey("ModCollectibleComponent"))
			{
				prefab.GetOrCreateComponent<ModCollectibleComponent>().InitializeComponent(jsonDict, "ModCollectibleComponent");
			}
			else if (jsonDict.ContainsKey("ModCookableComponent"))
			{
				prefab.GetOrCreateComponent<ModCookableComponent>().InitializeComponent(jsonDict, "ModCookableComponent");
			}
			else if (jsonDict.ContainsKey("ModCookingPotComponent"))
			{
				prefab.GetOrCreateComponent<ModCookingPotComponent>().InitializeComponent(jsonDict, "ModCookingPotComponent");
			}
			else if (jsonDict.ContainsKey("ModExplosiveComponent"))
			{
				prefab.GetOrCreateComponent<ModExplosiveComponent>().InitializeComponent(jsonDict, "ModExplosiveComponent");
			}
			else if (jsonDict.ContainsKey("ModFirstAidComponent"))
			{
				prefab.GetOrCreateComponent<ModFirstAidComponent>().InitializeComponent(jsonDict, "ModFirstAidComponent");
			}
			else if (jsonDict.ContainsKey("ModFoodComponent"))
			{
				prefab.GetOrCreateComponent<ModFoodComponent>().InitializeComponent(jsonDict, "ModFoodComponent");
			}
			else if (jsonDict.ContainsKey("ModGenericComponent"))
			{
				prefab.GetOrCreateComponent<ModGenericComponent>().InitializeComponent(jsonDict, "ModGenericComponent");
			}
			else if (jsonDict.ContainsKey("ModGenericEquippableComponent"))
			{
				prefab.GetOrCreateComponent<ModGenericEquippableComponent>().InitializeComponent(jsonDict, "ModGenericEquippableComponent");
			}
			else if (jsonDict.ContainsKey("ModLiquidComponent"))
			{
				prefab.GetOrCreateComponent<ModLiquidComponent>().InitializeComponent(jsonDict, "ModLiquidComponent");
			}
			else if (jsonDict.ContainsKey("ModPowderComponent"))
			{
				prefab.GetOrCreateComponent<ModPowderComponent>().InitializeComponent(jsonDict, "ModPowderComponent");
			}
			else if (jsonDict.ContainsKey("ModPurificationComponent"))
			{
				prefab.GetOrCreateComponent<ModPurificationComponent>().InitializeComponent(jsonDict, "ModPurificationComponent");
			}
			else if (jsonDict.ContainsKey("ModRandomItemComponent"))
			{
				prefab.GetOrCreateComponent<ModRandomItemComponent>().InitializeComponent(jsonDict, "ModRandomItemComponent");
			}
			else if (jsonDict.ContainsKey("ModRandomWeightedItemComponent"))
			{
				prefab.GetOrCreateComponent<ModRandomWeightedItemComponent>().InitializeComponent(jsonDict, "ModRandomWeightedItemComponent");
			}
			else if (jsonDict.ContainsKey("ModResearchComponent"))
			{
				prefab.GetOrCreateComponent<ModResearchComponent>().InitializeComponent(jsonDict, "ModResearchComponent");
			}
			else if (jsonDict.ContainsKey("ModToolComponent"))
			{
				prefab.GetOrCreateComponent<ModToolComponent>().InitializeComponent(jsonDict, "ModToolComponent");
			}
			else if (jsonDict.ContainsKey("ModAmmoComponent"))
			{
				prefab.GetOrCreateComponent<ModAmmoComponent>().InitializeComponent(jsonDict, "ModAmmoComponent");
			}
			if (jsonDict.ContainsKey("ModAccelerantBehaviour"))
			{
				prefab.GetOrCreateComponent<ModAccelerantBehaviour>().InitializeBehaviour(jsonDict, "ModAccelerantBehaviour");
			}
			else if (jsonDict.ContainsKey("ModBurnableBehaviour"))
			{
				prefab.GetOrCreateComponent<ModBurnableBehaviour>().InitializeBehaviour(jsonDict, "ModBurnableBehaviour");
			}
			else if (jsonDict.ContainsKey("ModFireStarterBehaviour"))
			{
				prefab.GetOrCreateComponent<ModFireStarterBehaviour>().InitializeBehaviour(jsonDict, "ModFireStarterBehaviour");
			}
			else if (jsonDict.ContainsKey("ModTinderBehaviour"))
			{
				prefab.GetOrCreateComponent<ModTinderBehaviour>().InitializeBehaviour(jsonDict, "ModTinderBehaviour");
			}
			if (jsonDict.ContainsKey("ModCarryingCapacityBehaviour"))
			{
				prefab.GetOrCreateComponent<ModCarryingCapacityBehaviour>().InitializeBehaviour(jsonDict);
			}
			if (jsonDict.ContainsKey("ModEvolveBehaviour"))
			{
				prefab.GetOrCreateComponent<ModEvolveBehaviour>().InitializeBehaviour(jsonDict);
			}
			if (jsonDict.ContainsKey("ModHarvestableBehaviour"))
			{
				prefab.GetOrCreateComponent<ModHarvestableBehaviour>().InitializeBehaviour(jsonDict);
			}
			if (jsonDict.ContainsKey("ModMillableBehaviour"))
			{
				prefab.GetOrCreateComponent<ModMillableBehaviour>().InitializeBehaviour(jsonDict);
			}
			if (jsonDict.ContainsKey("ModRepairableBehaviour"))
			{
				prefab.GetOrCreateComponent<ModRepairableBehaviour>().InitializeBehaviour(jsonDict);
			}
			if (jsonDict.ContainsKey("ModScentBehaviour"))
			{
				prefab.GetOrCreateComponent<ModScentBehaviour>().InitializeBehaviour(jsonDict);
			}
			if (jsonDict.ContainsKey("ModSharpenableBehaviour"))
			{
				prefab.GetOrCreateComponent<ModSharpenableBehaviour>().InitializeBehaviour(jsonDict);
			}
			if (jsonDict.ContainsKey("ModStackableBehaviour"))
			{
				prefab.GetOrCreateComponent<ModStackableBehaviour>().InitializeBehaviour(jsonDict);
			}
			if (jsonDict.ContainsKey("ChangeLayer"))
			{
				prefab.GetOrCreateComponent<ChangeLayer>().InitializeModification(jsonDict);
			}
		}
	}
}
