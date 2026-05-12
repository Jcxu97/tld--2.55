using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace Toolbelts;

internal class Patches
{
	[HarmonyPatch(typeof(Panel_Inventory), "Initialize")]
	internal class ToolbeltsInitialization
	{
		private static void Postfix(Panel_Inventory __instance)
		{
			ToolbeltsUtils.inventory = __instance;
			TBFunctionalities.InitializeMTB(__instance.m_ItemDescriptionPage);
		}
	}

	[HarmonyPatch(typeof(ItemDescriptionPage), "UpdateGearItemDescription")]
	internal class UpdateInventoryButton
	{
		private static void Postfix(ItemDescriptionPage __instance, GearItem gi)
		{
			Panel_Inventory panel = InterfaceManager.GetPanel<Panel_Inventory>();
			if (!((Object)(object)__instance != (Object)(object)((panel != null) ? panel.m_ItemDescriptionPage : null)))
			{
				TBFunctionalities.beltItem = ((gi != null) ? ((Component)gi).GetComponent<GearItem>() : null);
				TBFunctionalities.pantsItem = ((gi != null) ? ((Component)gi).GetComponent<GearItem>() : null);
				TBFunctionalities.cramponItem = ((gi != null) ? ((Component)gi).GetComponent<GearItem>() : null);
				TBFunctionalities.bootsItem = ((gi != null) ? ((Component)gi).GetComponent<GearItem>() : null);
				TBFunctionalities.scabbardItem = ((gi != null) ? ((Component)gi).GetComponent<GearItem>() : null);
				TBFunctionalities.bagItem = ((gi != null) ? ((Component)gi).GetComponent<GearItem>() : null);
				if ((Object)(object)gi != (Object)null && ToolbeltsUtils.IsPants(((Object)gi).name))
				{
					TBFunctionalities.SetAttachBeltActive(active: true);
				}
				else
				{
					TBFunctionalities.SetAttachBeltActive(active: false);
				}
				if ((Object)(object)gi != (Object)null && ToolbeltsUtils.IsPantsBelt(((Object)gi).name))
				{
					TBFunctionalities.SetDetachBeltActive(active: true);
				}
				else
				{
					TBFunctionalities.SetDetachBeltActive(active: false);
				}
				if ((Object)(object)gi != (Object)null && ToolbeltsUtils.IsBoots(((Object)gi).name))
				{
					TBFunctionalities.SetAttachCramponsActive(active: true);
				}
				else
				{
					TBFunctionalities.SetAttachCramponsActive(active: false);
				}
				if ((Object)(object)gi != (Object)null && ToolbeltsUtils.IsBootsCrampons(((Object)gi).name))
				{
					TBFunctionalities.SetDetachCramponsActive(active: true);
				}
				else
				{
					TBFunctionalities.SetDetachCramponsActive(active: false);
				}
				if (((Object)gi).name == "GEAR_MooseHideBag")
				{
					TBFunctionalities.SetAttachScabbardActive(active: true);
				}
				else
				{
					TBFunctionalities.SetAttachScabbardActive(active: false);
				}
				if (((Object)gi).name == "GEAR_MooseBagPlusScabbard")
				{
					TBFunctionalities.SetDetachScabbardActive(active: true);
				}
				else
				{
					TBFunctionalities.SetDetachScabbardActive(active: false);
				}
			}
		}
	}
}
