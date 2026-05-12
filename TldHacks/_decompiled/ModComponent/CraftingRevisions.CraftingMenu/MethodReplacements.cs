using Il2Cpp;
using Il2CppTLD.Gear;
using UnityEngine;

namespace CraftingRevisions.CraftingMenu;

internal static class MethodReplacements
{
	public static bool ItemPassesFilter(Panel_Crafting __instance, BlueprintData bpi)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected I4, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Invalid comparison between Unknown and I4
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		switch ((ModCraftingCategory)(int)__instance.m_CurrentCategory)
		{
		case ModCraftingCategory.All:
			return true;
		case ModCraftingCategory.FireStarting:
			if ((int)bpi.m_CraftingResultType == 0)
			{
				return bpi.m_CraftedResultGear.IsGearType((GearType)32);
			}
			return false;
		case ModCraftingCategory.FirstAid:
			if ((int)bpi.m_CraftingResultType == 0)
			{
				return bpi.m_CraftedResultGear.IsGearType((GearType)16);
			}
			return false;
		case ModCraftingCategory.Clothing:
			if ((int)bpi.m_CraftingResultType == 0)
			{
				return bpi.m_CraftedResultGear.IsGearType((GearType)2);
			}
			return false;
		case ModCraftingCategory.Tools:
			if ((int)bpi.m_CraftingResultType == 0)
			{
				if (!bpi.m_CraftedResultGear.IsGearType((GearType)8))
				{
					return bpi.m_CraftedResultGear.IsGearType((GearType)64);
				}
				return true;
			}
			return false;
		case ModCraftingCategory.Decoration:
			return (int)bpi.m_CraftingResultType == 1;
		case ModCraftingCategory.Materials:
			if ((int)bpi.m_CraftingResultType == 0)
			{
				return bpi.m_CraftedResultGear.IsGearType((GearType)4);
			}
			return false;
		case ModCraftingCategory.Food:
			if ((int)bpi.m_CraftingResultType == 0)
			{
				return bpi.m_CraftedResultGear.IsGearType((GearType)1);
			}
			return false;
		default:
			return false;
		}
	}

	public static void HandleInput(Panel_Crafting __instance)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Invalid comparison between Unknown and I4
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		if (InputManager.GetEscapePressed((MonoBehaviour)(object)__instance))
		{
			__instance.OnBackButton();
			return;
		}
		float menuMovementVertical;
		if (Utils.IsGamepadActive())
		{
			if (InputManager.GetOpenActionsPanelPressed((MonoBehaviour)(object)__instance))
			{
				__instance.OnBackButton();
				return;
			}
			if (InputManager.GetAltFirePressed((MonoBehaviour)(object)__instance))
			{
				return;
			}
			if (InputManager.GetFirePressed((MonoBehaviour)(object)__instance))
			{
				InterfaceManager.IsUsingSurvivalTabs();
				return;
			}
			if ((int)__instance.m_CurrentNavArea == 0)
			{
				if (InputManager.GetContinuePressed((MonoBehaviour)(object)__instance))
				{
					__instance.SetNavigationArea((NavArea)1);
					return;
				}
			}
			else
			{
				if (InputManager.GetInventoryExaminePressed((MonoBehaviour)(object)__instance))
				{
					__instance.OnBeginCrafting();
					return;
				}
				if (InputManager.GetInventoryFilterLeftPressed((MonoBehaviour)(object)__instance))
				{
					__instance.m_RequirementContainer.OnPrevious();
					return;
				}
				if (InputManager.GetInventoryFilterRightPressed((MonoBehaviour)(object)__instance))
				{
					__instance.m_RequirementContainer.OnNext();
					return;
				}
				if (InputManager.GetInventoryDropPressed((MonoBehaviour)(object)__instance))
				{
					__instance.m_RequirementContainer.HandleNavigation();
					return;
				}
				if (InputManager.GetInventorySortPressed((MonoBehaviour)(object)__instance))
				{
					Filter val = (Filter)(__instance.m_CurrentFilter + 1);
					if ((int)val >= 7)
					{
						val = (Filter)0;
					}
					__instance.SetFilter(val);
					return;
				}
			}
			menuMovementVertical = Utils.GetMenuMovementVertical((MonoBehaviour)(object)__instance, true, true);
		}
		else
		{
			InputManager.GetAxisScrollWheel((MonoBehaviour)(object)__instance);
			_ = __instance.m_Blueprints.Count;
			_ = __instance.m_FilteredBlueprints.Count;
			menuMovementVertical = Utils.GetMenuMovementVertical((MonoBehaviour)(object)__instance, true, true);
		}
		Utils.IsZero(menuMovementVertical, 0.0001f);
	}
}
