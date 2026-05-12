using System;
using Il2Cpp;
using Il2CppSystem.Collections.Generic;
using UnityEngine;

namespace Toolbelts;

internal class TBFunctionalities
{
	internal static string attachBeltText;

	private static GameObject attachBeltButton;

	internal static GearItem beltItem;

	internal static string beltName;

	internal static string detachBeltText;

	private static GameObject detachBeltButton;

	internal static GearItem pantsItem;

	internal static string pantsName;

	internal static string attachCramponsText;

	private static GameObject attachCramponsButton;

	internal static GearItem cramponItem;

	internal static string cramponName;

	internal static string detachCramponsText;

	private static GameObject detachCramponsButton;

	internal static GearItem bootsItem;

	internal static string bootsName;

	internal static string attachScabbardText;

	private static GameObject attachScabbardButton;

	internal static GearItem scabbardItem;

	internal static string scabbardName;

	internal static string detachScabbardText;

	private static GameObject detachScabbardButton;

	internal static GearItem bagItem;

	internal static string bagName;

	internal static void InitializeMTB(ItemDescriptionPage itemDescriptionPage)
	{
		attachBeltText = Localization.Get("GAMEPLAY_TB_AttachBeltLabel");
		detachBeltText = Localization.Get("GAMEPLAY_TB_DetachBeltLabel");
		attachCramponsText = Localization.Get("GAMEPLAY_TB_AttachCramponsLabel");
		detachCramponsText = Localization.Get("GAMEPLAY_TB_DetachCramponsLabel");
		attachScabbardText = Localization.Get("GAMEPLAY_TB_AttachScabbardLabel");
		detachScabbardText = Localization.Get("GAMEPLAY_TB_DetachScabbardLabel");
		GameObject mouseButtonEquip = itemDescriptionPage.m_MouseButtonEquip;
		attachBeltButton = Object.Instantiate<GameObject>(mouseButtonEquip, mouseButtonEquip.transform.parent, true);
		attachBeltButton.transform.Translate(0f, 0f, 0f);
		Utils.GetComponentInChildren<UILabel>(attachBeltButton).text = attachBeltText;
		detachBeltButton = Object.Instantiate<GameObject>(mouseButtonEquip, mouseButtonEquip.transform.parent, true);
		detachBeltButton.transform.Translate(0f, 0f, 0f);
		Utils.GetComponentInChildren<UILabel>(detachBeltButton).text = detachBeltText;
		attachCramponsButton = Object.Instantiate<GameObject>(mouseButtonEquip, mouseButtonEquip.transform.parent, true);
		attachCramponsButton.transform.Translate(0f, 0f, 0f);
		Utils.GetComponentInChildren<UILabel>(attachCramponsButton).text = attachCramponsText;
		detachCramponsButton = Object.Instantiate<GameObject>(mouseButtonEquip, mouseButtonEquip.transform.parent, true);
		detachCramponsButton.transform.Translate(0f, 0f, 0f);
		Utils.GetComponentInChildren<UILabel>(detachCramponsButton).text = detachCramponsText;
		attachScabbardButton = Object.Instantiate<GameObject>(mouseButtonEquip, mouseButtonEquip.transform.parent, true);
		attachScabbardButton.transform.Translate(0f, 0f, 0f);
		Utils.GetComponentInChildren<UILabel>(attachScabbardButton).text = attachScabbardText;
		detachScabbardButton = Object.Instantiate<GameObject>(mouseButtonEquip, mouseButtonEquip.transform.parent, true);
		detachScabbardButton.transform.Translate(0f, 0f, 0f);
		Utils.GetComponentInChildren<UILabel>(detachScabbardButton).text = detachScabbardText;
		AddAction(attachBeltButton, OnAttachBelt);
		AddAction(detachBeltButton, OnDetachBelt);
		AddAction(attachCramponsButton, OnAttachCrampons);
		AddAction(detachCramponsButton, OnDetachCrampons);
		AddAction(attachScabbardButton, OnAttachScabbard);
		AddAction(detachScabbardButton, OnDetachScabbard);
		SetAttachBeltActive(active: false);
		SetDetachBeltActive(active: false);
		SetAttachCramponsActive(active: false);
		SetDetachCramponsActive(active: false);
		SetAttachScabbardActive(active: false);
		SetDetachScabbardActive(active: false);
	}

	private static void AddAction(GameObject button, Action action)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		List<EventDelegate> val = new List<EventDelegate>();
		val.Add(new EventDelegate(Callback.op_Implicit(action)));
		Utils.GetComponentInChildren<UIButton>(button).onClick = val;
	}

	internal static void SetAttachBeltActive(bool active)
	{
		NGUITools.SetActive(attachBeltButton, active);
	}

	internal static void SetDetachBeltActive(bool active)
	{
		NGUITools.SetActive(detachBeltButton, active);
	}

	internal static void SetAttachCramponsActive(bool active)
	{
		NGUITools.SetActive(attachCramponsButton, active);
	}

	internal static void SetDetachCramponsActive(bool active)
	{
		NGUITools.SetActive(detachCramponsButton, active);
	}

	internal static void SetAttachScabbardActive(bool active)
	{
		NGUITools.SetActive(attachScabbardButton, active);
	}

	internal static void SetDetachScabbardActive(bool active)
	{
		NGUITools.SetActive(detachScabbardButton, active);
	}

	private static void OnAttachCrampons()
	{
		GearItem val = cramponItem;
		GearItem bestGearItemWithName = GameManager.GetInventoryComponent().GetBestGearItemWithName("GEAR_Crampons");
		GearItem bestGearItemWithName2 = GameManager.GetInventoryComponent().GetBestGearItemWithName("GEAR_ImprovisedCrampons");
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		if ((Object)(object)bestGearItemWithName == (Object)null && (Object)(object)bestGearItemWithName2 == (Object)null)
		{
			HUDMessage.AddMessage(Localization.Get("GAMEPLAY_TB_NoCrampons"), false, false);
			GameAudioManager.PlayGUIError();
		}
		else if (((Object)val).name == "GEAR_WorkBoots" || ((Object)val).name == "GEAR_BasicBoots" || ((Object)val).name == "GEAR_CombatBoots" || ((Object)val).name == "GEAR_InsulatedBoots" || ((Object)val).name == "GEAR_BasicShoes" || ((Object)val).name == "GEAR_SkiBoots" || ((Object)val).name == "GEAR_LeatherShoes" || ((Object)val).name == "GEAR_DeerSkinBoots" || ((Object)val).name == "GEAR_MuklukBoots" || ((Object)val).name == "GEAR_MinersBoots")
		{
			cramponName = ((Object)val).name;
			if (Object.op_Implicit((Object)(object)bestGearItemWithName))
			{
				GameAudioManager.PlayGuiConfirm();
				InterfaceManager.GetPanel<Panel_GenericProgressBar>().Launch(Localization.Get("GAMEPLAY_TB_AttachingProgressBar"), 1f, 0f, 0f, "PLAY_CRACCESSORIES_LEATHERBELT_EQUIP", (string)null, false, true, OnExitDelegate.op_Implicit((Action<bool, bool, float>)OnAttachCramponsFinished));
				GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)bestGearItemWithName).name, 1, false);
			}
			else if (Object.op_Implicit((Object)(object)bestGearItemWithName2))
			{
				GameAudioManager.PlayGuiConfirm();
				InterfaceManager.GetPanel<Panel_GenericProgressBar>().Launch(Localization.Get("GAMEPLAY_TB_AttachingProgressBar"), 1f, 0f, 0f, "PLAY_CRACCESSORIES_LEATHERBELT_EQUIP", (string)null, false, true, OnExitDelegate.op_Implicit((Action<bool, bool, float>)OnAttachCramponsUFinished));
				GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)bestGearItemWithName2).name, 1, false);
			}
		}
		else
		{
			HUDMessage.AddMessage(Localization.Get("GAMEPLAY_TB_NoCrampons"), false, false);
			GameAudioManager.PlayGUIError();
		}
	}

	private static void OnAttachCramponsFinished(bool success, bool playerCancel, float progress)
	{
		if (cramponName.ToLowerInvariant().Contains("workboots"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.workBoots).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.workBootsCramp, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.workBootsCramp).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (cramponName.ToLowerInvariant().Contains("combatboots"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.combatBoots).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.combatBootsCramp, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.combatBootsCramp).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (cramponName.ToLowerInvariant().Contains("deerskinboots"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.deerBoots).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.deerBootsCramp, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.deerBootsCramp).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (cramponName.ToLowerInvariant().Contains("insulatedboots"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.insulatedBoots).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.insulatedBootsCramp, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.insulatedBootsCramp).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (cramponName.ToLowerInvariant().Contains("leathershoes"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.dressingBoots).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.dressingBootsCramp, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.dressingBootsCramp).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (cramponName.ToLowerInvariant().Contains("mukluk"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.muklukBoots).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.muklukBootsCramp, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.muklukBootsCramp).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (cramponName.ToLowerInvariant().Contains("basicshoes"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.runningBoots).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.runningBootsCramp, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.runningBootsCramp).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (cramponName.ToLowerInvariant().Contains("skiboots"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.skiBoots).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.skiBootsCramp, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.skiBootsCramp).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (cramponName.ToLowerInvariant().Contains("basicboots"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.trailBoots).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.trailBootsCramp, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.trailBootsCramp).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (cramponName.ToLowerInvariant().Contains("minersboots"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.chemicalBoots).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.chemicalBootsCramp, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.chemicalBootsCramp).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
	}

	private static void OnAttachCramponsUFinished(bool success, bool playerCancel, float progress)
	{
		if (cramponName.ToLowerInvariant().Contains("workboots"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.workBoots).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.workBootsImprov, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.workBootsImprov).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (cramponName.ToLowerInvariant().Contains("combatboots"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.combatBoots).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.combatBootsImprov, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.combatBootsImprov).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (cramponName.ToLowerInvariant().Contains("deerskinboots"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.deerBoots).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.deerBootsImprov, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.deerBootsImprov).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (cramponName.ToLowerInvariant().Contains("insulatedboots"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.insulatedBoots).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.insulatedBootsImprov, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.insulatedBootsImprov).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (cramponName.ToLowerInvariant().Contains("leathershoes"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.dressingBoots).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.dressingBootsImprov, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.dressingBootsImprov).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (cramponName.ToLowerInvariant().Contains("mukluk"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.muklukBoots).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.muklukBootsImprov, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.muklukBootsImprov).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (cramponName.ToLowerInvariant().Contains("basicshoes"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.runningBoots).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.runningBootsImprov, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.runningBootsImprov).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (cramponName.ToLowerInvariant().Contains("skiboots"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.skiBoots).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.skiBootsImprov, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.skiBootsImprov).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (cramponName.ToLowerInvariant().Contains("basicboots"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.trailBoots).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.trailBootsImprov, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.trailBootsImprov).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (cramponName.ToLowerInvariant().Contains("minersboots"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.chemicalBoots).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.chemicalBootsImprov, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.chemicalBootsImprov).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
	}

	private static void OnDetachCrampons()
	{
		GearItem val = bootsItem;
		_ = ToolbeltsUtils.crampons;
		_ = ToolbeltsUtils.cramponsimprov;
		if (!((Object)(object)val == (Object)null))
		{
			if (((Object)val).name == "GEAR_WorkNCrampons" || ((Object)val).name == "GEAR_CombatNCrampons" || ((Object)val).name == "GEAR_DeerskinNCrampons" || ((Object)val).name == "GEAR_InsulatedNCrampons" || ((Object)val).name == "GEAR_DressingNCrampons" || ((Object)val).name == "GEAR_MuklukNCrampons" || ((Object)val).name == "GEAR_RunningNCrampons" || ((Object)val).name == "GEAR_SkiNCrampons" || ((Object)val).name == "GEAR_TrailNCrampons" || ((Object)val).name == "GEAR_ChemicalNCrampons")
			{
				bootsName = ((Object)val).name;
				GameAudioManager.PlayGuiConfirm();
				InterfaceManager.GetPanel<Panel_GenericProgressBar>().Launch(Localization.Get("GAMEPLAY_TB_DetachingProgressBar"), 1f, 0f, 0f, "PLAY_CRACCESSORIES_LEATHERBELT_UNQUIP", (string)null, false, true, OnExitDelegate.op_Implicit((Action<bool, bool, float>)OnDetachCramponsFinished));
				GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.crampons, 1, 1f, (InventoryInstantiateFlags)0);
			}
			else if (((Object)val).name == "GEAR_WorkICrampons" || ((Object)val).name == "GEAR_CombatICrampons" || ((Object)val).name == "GEAR_DeerskinICrampons" || ((Object)val).name == "GEAR_InsulatedICrampons" || ((Object)val).name == "GEAR_DressingICrampons" || ((Object)val).name == "GEAR_MuklukICrampons" || ((Object)val).name == "GEAR_RunningICrampons" || ((Object)val).name == "GEAR_SkiICrampons" || ((Object)val).name == "GEAR_TrailICrampons" || ((Object)val).name == "GEAR_ChemicalICrampons")
			{
				bootsName = ((Object)val).name;
				GameAudioManager.PlayGuiConfirm();
				InterfaceManager.GetPanel<Panel_GenericProgressBar>().Launch(Localization.Get("GAMEPLAY_TB_DetachingProgressBar"), 1f, 0f, 0f, "PLAY_CRACCESSORIES_LEATHERBELT_UNQUIP", (string)null, false, true, OnExitDelegate.op_Implicit((Action<bool, bool, float>)OnDetachCramponsFinished));
				GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.cramponsimprov, 1, 1f, (InventoryInstantiateFlags)0);
			}
		}
	}

	private static void OnDetachCramponsFinished(bool success, bool playerCancel, float progress)
	{
		if (bootsName.ToLowerInvariant().Contains("workncrampons"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.workBootsCramp).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.workBoots, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.workBoots).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (bootsName.ToLowerInvariant().Contains("workicrampons"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.workBootsImprov).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.workBoots, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.workBoots).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (bootsName.ToLowerInvariant().Contains("combatncrampons"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.combatBootsCramp).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.combatBoots, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.combatBoots).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (bootsName.ToLowerInvariant().Contains("combaticrampons"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.combatBootsImprov).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.combatBoots, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.combatBoots).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (bootsName.ToLowerInvariant().Contains("deerskinncrampons"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.deerBootsCramp).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.deerBoots, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.deerBoots).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (bootsName.ToLowerInvariant().Contains("deerskinicrampons"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.deerBootsImprov).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.deerBoots, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.deerBoots).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (bootsName.ToLowerInvariant().Contains("insulatedncrampons"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.insulatedBootsCramp).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.insulatedBoots, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.insulatedBoots).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (bootsName.ToLowerInvariant().Contains("insulatedicrampons"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.insulatedBootsImprov).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.insulatedBoots, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.insulatedBoots).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (bootsName.ToLowerInvariant().Contains("dressingncrampons"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.dressingBootsCramp).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.dressingBoots, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.dressingBoots).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (bootsName.ToLowerInvariant().Contains("dressingicrampons"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.dressingBootsImprov).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.dressingBoots, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.dressingBoots).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (bootsName.ToLowerInvariant().Contains("muklukncrampons"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.muklukBootsCramp).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.muklukBoots, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.muklukBoots).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (bootsName.ToLowerInvariant().Contains("muklukicrampons"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.muklukBootsImprov).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.muklukBoots, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.muklukBoots).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (bootsName.ToLowerInvariant().Contains("runningncrampons"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.runningBootsCramp).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.runningBoots, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.runningBoots).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (bootsName.ToLowerInvariant().Contains("runningicrampons"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.runningBootsImprov).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.runningBoots, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.runningBoots).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (bootsName.ToLowerInvariant().Contains("skincrampons"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.skiBootsCramp).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.skiBoots, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.skiBoots).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (bootsName.ToLowerInvariant().Contains("skiicrampons"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.skiBootsImprov).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.skiBoots, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.skiBoots).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (bootsName.ToLowerInvariant().Contains("trailncrampons"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.trailBootsCramp).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.trailBoots, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.trailBoots).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (bootsName.ToLowerInvariant().Contains("trailicrampons"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.trailBootsImprov).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.trailBoots, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.trailBoots).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (bootsName.ToLowerInvariant().Contains("chemicalncrampons"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.chemicalBootsCramp).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.chemicalBoots, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.chemicalBoots).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (bootsName.ToLowerInvariant().Contains("chemicalicrampons"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.chemicalBootsImprov).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.chemicalBoots, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.chemicalBoots).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
	}

	private static void OnAttachBelt()
	{
		GearItem val = beltItem;
		GearItem bestGearItemWithName = GameManager.GetInventoryComponent().GetBestGearItemWithName("GEAR_Toolbelt");
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		if ((Object)(object)bestGearItemWithName == (Object)null)
		{
			HUDMessage.AddMessage(Localization.Get("GAMEPLAY_TB_NoBelt"), false, false);
			GameAudioManager.PlayGUIError();
		}
		else if (((Object)val).name == "GEAR_Jeans" || ((Object)val).name == "GEAR_CargoPants" || ((Object)val).name == "GEAR_CombatPants" || ((Object)val).name == "GEAR_DeerSkinPants" || ((Object)val).name == "GEAR_InsulatedPants" || ((Object)val).name == "GEAR_MinersPants" || ((Object)val).name == "GEAR_WorkPants")
		{
			beltName = ((Object)val).name;
			if (Object.op_Implicit((Object)(object)bestGearItemWithName))
			{
				GameAudioManager.PlayGuiConfirm();
				InterfaceManager.GetPanel<Panel_GenericProgressBar>().Launch(Localization.Get("GAMEPLAY_TB_AttachingProgressBar"), 1f, 0f, 0f, "PLAY_CRACCESSORIES_LEATHERBELT_EQUIP", (string)null, false, true, OnExitDelegate.op_Implicit((Action<bool, bool, float>)OnAttachBeltUFinished));
				GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)bestGearItemWithName).name, 1, false);
			}
		}
		else
		{
			HUDMessage.AddMessage(Localization.Get("GAMEPLAY_TB_NoBelt"), false, false);
			GameAudioManager.PlayGUIError();
		}
	}

	private static void OnAttachBeltUFinished(bool success, bool playerCancel, float progress)
	{
		if (beltName.ToLowerInvariant().Contains("jeans"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.jeans).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.jeansbelt, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.jeansbelt).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (beltName.ToLowerInvariant().Contains("cargo"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.cargo).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.cargobelt, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.cargobelt).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (beltName.ToLowerInvariant().Contains("combat"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.combat).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.combatbelt, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.combatbelt).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (beltName.ToLowerInvariant().Contains("deerskin"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.deerskin).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.deerskinbelt, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.deerskinbelt).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (beltName.ToLowerInvariant().Contains("insulated"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.insulated).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.insulatedbelt, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.insulatedbelt).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (beltName.ToLowerInvariant().Contains("miner"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.miner).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.minerbelt, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.minerbelt).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (beltName.ToLowerInvariant().Contains("work"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.work).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.workbelt, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.workbelt).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
	}

	private static void OnDetachBelt()
	{
		GearItem val = pantsItem;
		if (!((Object)(object)val == (Object)null) && (((Object)val).name == "GEAR_JeansToolbelt" || ((Object)val).name == "GEAR_CargoToolbelt" || ((Object)val).name == "GEAR_MinerToolbelt" || ((Object)val).name == "GEAR_CombatToolbelt" || ((Object)val).name == "GEAR_DeerskinToolbelt" || ((Object)val).name == "GEAR_InsulatedToolbelt" || ((Object)val).name == "GEAR_WorkToolbelt"))
		{
			pantsName = ((Object)val).name;
			GameAudioManager.PlayGuiConfirm();
			InterfaceManager.GetPanel<Panel_GenericProgressBar>().Launch(Localization.Get("GAMEPLAY_TB_DetachingProgressBar"), 1f, 0f, 0f, "PLAY_CRACCESSORIES_LEATHERBELT_UNQUIP", (string)null, false, true, OnExitDelegate.op_Implicit((Action<bool, bool, float>)OnDetachBeltFinished));
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.belt, 1, 1f, (InventoryInstantiateFlags)0);
		}
	}

	private static void OnDetachBeltFinished(bool success, bool playerCancel, float progress)
	{
		if (pantsName.ToLowerInvariant().Contains("jeanstoolbelt"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.jeansbelt).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.jeans, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.jeans).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (pantsName.ToLowerInvariant().Contains("cargotoolbelt"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.cargobelt).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.cargo, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.cargo).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (pantsName.ToLowerInvariant().Contains("minertoolbelt"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.minerbelt).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.miner, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.miner).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (pantsName.ToLowerInvariant().Contains("combattoolbelt"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.combatbelt).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.combat, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.combat).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (pantsName.ToLowerInvariant().Contains("deerskintoolbelt"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.deerskinbelt).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.deerskin, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.deerskin).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (pantsName.ToLowerInvariant().Contains("insulatedtoolbelt"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.insulatedbelt).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.insulated, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.insulated).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
		else if (pantsName.ToLowerInvariant().Contains("worktoolbelt"))
		{
			GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.workbelt).name, 1);
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.work, 1, 1f, (InventoryInstantiateFlags)0);
			GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.work).name, 1).m_CurrentHP = val.m_CurrentHP;
			GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
		}
	}

	private static void OnAttachScabbard()
	{
		GearItem val = scabbardItem;
		GearItem bestGearItemWithName = GameManager.GetInventoryComponent().GetBestGearItemWithName("GEAR_RifleScabbardA");
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		if ((Object)(object)bestGearItemWithName == (Object)null)
		{
			HUDMessage.AddMessage(Localization.Get("GAMEPLAY_TB_NoScabbard"), false, false);
			GameAudioManager.PlayGUIError();
		}
		else if (((Object)val).name == "GEAR_MooseHideBag")
		{
			beltName = ((Object)val).name;
			if (Object.op_Implicit((Object)(object)bestGearItemWithName))
			{
				GameAudioManager.PlayGuiConfirm();
				InterfaceManager.GetPanel<Panel_GenericProgressBar>().Launch(Localization.Get("GAMEPLAY_TB_AttachingProgressBar"), 1f, 0f, 0f, "PLAY_CRACCESSORIES_LEATHERBELT_EQUIP", (string)null, false, true, OnExitDelegate.op_Implicit((Action<bool, bool, float>)OnAttachScabbardFinished));
				GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)bestGearItemWithName).name, 1, false);
			}
		}
		else
		{
			HUDMessage.AddMessage(Localization.Get("GAMEPLAY_TB_NoScabbard"), false, false);
			GameAudioManager.PlayGUIError();
		}
	}

	private static void OnAttachScabbardFinished(bool success, bool playerCancel, float progress)
	{
		GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.bag).name, 1);
		GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.bagscabbard, 1, 1f, (InventoryInstantiateFlags)0);
		GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.bagscabbard).name, 1).m_CurrentHP = val.m_CurrentHP;
		GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
	}

	private static void OnDetachScabbard()
	{
		GearItem val = bagItem;
		if (!((Object)(object)val == (Object)null) && ((Object)val).name == "GEAR_MooseBagPlusScabbard")
		{
			bagName = ((Object)val).name;
			GameAudioManager.PlayGuiConfirm();
			InterfaceManager.GetPanel<Panel_GenericProgressBar>().Launch(Localization.Get("GAMEPLAY_TB_DetachingProgressBar"), 1f, 0f, 0f, "PLAY_CRACCESSORIES_LEATHERBELT_UNQUIP", (string)null, false, true, OnExitDelegate.op_Implicit((Action<bool, bool, float>)OnDetachScabbardFinished));
			GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.scabbard, 1, 1f, (InventoryInstantiateFlags)0);
		}
	}

	private static void OnDetachScabbardFinished(bool success, bool playerCancel, float progress)
	{
		GearItem val = GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.bagscabbard).name, 1);
		GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(ToolbeltsUtils.bag, 1, 1f, (InventoryInstantiateFlags)0);
		GameManager.GetInventoryComponent().GearInInventory(((Object)ToolbeltsUtils.bag).name, 1).m_CurrentHP = val.m_CurrentHP;
		GameManager.GetInventoryComponent().RemoveGearFromInventory(((Object)val).name, 1, false);
	}
}
