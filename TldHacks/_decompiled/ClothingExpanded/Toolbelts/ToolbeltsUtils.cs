using Il2Cpp;
using Il2CppSystem;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Toolbelts;

internal static class ToolbeltsUtils
{
	public static Panel_Inventory inventory;

	public static GearItem belt = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_Toolbelt")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem crampons = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_Crampons")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem cramponsimprov = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_ImprovisedCrampons")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem bag = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_MooseHideBag")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem scabbard = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_RifleScabbardA")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem bagscabbard = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_MooseBagPlusScabbard")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem jeans = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_Jeans")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem jeansbelt = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_JeansToolbelt")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem cargo = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_CargoPants")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem cargobelt = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_CargoToolbelt")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem miner = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_MinersPants")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem minerbelt = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_MinerToolbelt")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem combat = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_CombatPants")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem combatbelt = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_CombatToolbelt")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem deerskin = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_DeerskinPants")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem deerskinbelt = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_DeerskinToolbelt")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem insulated = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_InsulatedPants")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem insulatedbelt = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_InsulatedToolbelt")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem work = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_WorkPants")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem workbelt = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_WorkToolbelt")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem workBoots = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_WorkBoots")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem workBootsCramp = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_WorkNCrampons")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem workBootsImprov = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_WorkICrampons")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem skiBoots = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_SkiBoots")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem skiBootsCramp = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_SkiNCrampons")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem skiBootsImprov = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_SkiICrampons")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem combatBoots = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_CombatBoots")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem combatBootsCramp = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_CombatNCrampons")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem combatBootsImprov = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_CombatICrampons")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem deerBoots = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_DeerskinBoots")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem deerBootsCramp = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_DeerskinNCrampons")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem deerBootsImprov = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_DeerskinICrampons")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem dressingBoots = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_LeatherShoes")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem dressingBootsCramp = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_DressingNCrampons")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem dressingBootsImprov = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_DressingICrampons")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem insulatedBoots = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_InsulatedBoots")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem insulatedBootsCramp = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_InsulatedNCrampons")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem insulatedBootsImprov = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_InsulatedICrampons")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem muklukBoots = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_MuklukBoots")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem muklukBootsCramp = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_MuklukNCrampons")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem muklukBootsImprov = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_MuklukICrampons")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem trailBoots = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_BasicBoots")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem trailBootsCramp = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_TrailNCrampons")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem trailBootsImprov = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_TrailICrampons")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem runningBoots = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_BasicShoes")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem runningBootsCramp = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_RunningNCrampons")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem runningBootsImprov = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_RunningICrampons")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem chemicalBoots = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_MinersBoots")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem chemicalBootsCramp = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_ChemicalNCrampons")).WaitForCompletion().GetComponent<GearItem>();

	public static GearItem chemicalBootsImprov = Addressables.LoadAssetAsync<GameObject>(Object.op_Implicit("GEAR_ChemicalICrampons")).WaitForCompletion().GetComponent<GearItem>();

	public static GameObject GetPlayer()
	{
		return GameManager.GetPlayerObject();
	}

	public static bool IsPants(string gearItemName)
	{
		string[] array = new string[7] { "GEAR_Jeans", "GEAR_CargoPants", "GEAR_MinersPants", "GEAR_CombatPants", "GEAR_DeerSkinPants", "GEAR_InsulatedPants", "GEAR_WorkPants" };
		for (int i = 0; i < array.Length; i++)
		{
			if (gearItemName == array[i])
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsBoots(string gearItemName)
	{
		string[] array = new string[10] { "GEAR_WorkBoots", "GEAR_InsulatedBoots", "GEAR_BasicBoots", "GEAR_DeerSkinBoots", "GEAR_SkiBoots", "GEAR_CombatBoots", "GEAR_LeatherShoes", "GEAR_MuklukBoots", "GEAR_BasicShoes", "GEAR_MinersBoots" };
		for (int i = 0; i < array.Length; i++)
		{
			if (gearItemName == array[i])
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsPantsBelt(string gearItemName)
	{
		string[] array = new string[7] { "GEAR_JeansToolbelt", "GEAR_CargoToolbelt", "GEAR_MinerToolbelt", "GEAR_CombatToolbelt", "GEAR_DeerskinToolbelt", "GEAR_InsulatedToolbelt", "GEAR_WorkToolbelt" };
		for (int i = 0; i < array.Length; i++)
		{
			if (gearItemName == array[i])
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsBootsCrampons(string gearItemName)
	{
		string[] array = new string[20]
		{
			"GEAR_WorkNCrampons", "GEAR_WorkICrampons", "GEAR_InsulatedNCrampons", "GEAR_InsulatedICrampons", "GEAR_TrailNCrampons", "GEAR_TrailICrampons", "GEAR_DeerskinNCrampons", "GEAR_DeerskinICrampons", "GEAR_SkiNCrampons", "GEAR_SkiICrampons",
			"GEAR_CombatNCrampons", "GEAR_CombatICrampons", "GEAR_DressingNCrampons", "GEAR_DressingICrampons", "GEAR_MuklukNCrampons", "GEAR_MuklukICrampons", "GEAR_RunningNCrampons", "GEAR_RunningICrampons", "GEAR_ChemicalNCrampons", "GEAR_ChemicalICrampons"
		};
		for (int i = 0; i < array.Length; i++)
		{
			if (gearItemName == array[i])
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsScenePlayable()
	{
		if (!string.IsNullOrEmpty(GameManager.m_ActiveScene) && !GameManager.m_ActiveScene.Contains("MainMenu") && !(GameManager.m_ActiveScene == "Boot"))
		{
			return !(GameManager.m_ActiveScene == "Empty");
		}
		return false;
	}

	public static bool IsScenePlayable(string scene)
	{
		if (!string.IsNullOrEmpty(scene) && !scene.Contains("MainMenu") && !(scene == "Boot"))
		{
			return !(scene == "Empty");
		}
		return false;
	}

	public static bool IsMainMenu(string scene)
	{
		if (!string.IsNullOrEmpty(scene))
		{
			return scene.Contains("MainMenu");
		}
		return false;
	}
}
