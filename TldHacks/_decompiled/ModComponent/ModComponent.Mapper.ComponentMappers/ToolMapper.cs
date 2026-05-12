using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Mapper.ComponentMappers;

internal static class ToolMapper
{
	internal static void Configure(ModBaseComponent modComponent)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		ModToolComponent modToolComponent = ((Il2CppObjectBase)modComponent).TryCast<ModToolComponent>();
		if (!((Object)(object)modToolComponent == (Object)null))
		{
			ToolsItem orCreateComponent = ((Component?)(object)modToolComponent).GetOrCreateComponent<ToolsItem>();
			GearItem? orCreateComponent2 = ((Component?)(object)modToolComponent).GetOrCreateComponent<GearItem>();
			orCreateComponent.m_ToolType = modToolComponent.Usage;
			orCreateComponent.m_CuttingToolType = modToolComponent.ToolType;
			orCreateComponent.m_CraftingAndRepairSkillModifier = modToolComponent.SkillBonus;
			orCreateComponent.m_CraftingAndRepairTimeModifier = modToolComponent.CraftingTimeMultiplier;
			orCreateComponent.m_DegradePerHourCrafting = modToolComponent.DegradePerHourCrafting;
			orCreateComponent.m_CanOnlyCraftAndRepairClothes = true;
			orCreateComponent.m_AppearInStoryOnly = false;
			ConfigureBodyHarvest(modToolComponent);
			ConfigureBreakDown(modToolComponent);
			ConfigureDegradeOnUse(modToolComponent);
			ConfigureForceLock(modToolComponent);
			ConfigureIceFishingHole(modToolComponent);
			ConfigureStruggleBonus(modToolComponent);
			orCreateComponent2.m_ToolsItem = orCreateComponent;
		}
	}

	private static void ConfigureBodyHarvest(ModToolComponent modToolComponent)
	{
		if (modToolComponent.CarcassHarvesting)
		{
			BodyHarvestItem? orCreateComponent = ((Component?)(object)modToolComponent).GetOrCreateComponent<BodyHarvestItem>();
			orCreateComponent.m_HarvestMeatMinutesPerKG = modToolComponent.MinutesPerKgMeat;
			orCreateComponent.m_HarvestFrozenMeatMinutesPerKG = modToolComponent.MinutesPerKgFrozenMeat;
			orCreateComponent.m_HarvestGutMinutesPerUnit = modToolComponent.MinutesPerGut;
			orCreateComponent.m_HarvestHideMinutesPerUnit = modToolComponent.MinutesPerHide;
			orCreateComponent.m_HPDecreasePerHourUse = modToolComponent.DegradePerHourHarvesting;
		}
	}

	private static void ConfigureBreakDown(ModToolComponent modToolComponent)
	{
		((Component?)(object)modToolComponent).GetOrCreateComponent<BreakDownItem>().m_BreakDownTimeModifier = modToolComponent.BreakDownTimeMultiplier;
		string templateToolName = GetTemplateToolName(modToolComponent);
		if (templateToolName != null)
		{
			AlternativeToolManager.AddToList(modToolComponent, templateToolName);
		}
	}

	private static void ConfigureDegradeOnUse(ModToolComponent modToolComponent)
	{
		DegradeOnUse? orCreateComponent = ((Component?)(object)modToolComponent).GetOrCreateComponent<DegradeOnUse>();
		orCreateComponent.m_DegradeHP = Mathf.Max(orCreateComponent.m_DegradeHP, modToolComponent.DegradeOnUse);
	}

	private static void ConfigureForceLock(ModToolComponent modToolComponent)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		if (modToolComponent.ForceLocks)
		{
			ForceLockItem? orCreateComponent = ((Component?)(object)modToolComponent).GetOrCreateComponent<ForceLockItem>();
			orCreateComponent.m_ForceLockAudio = ModUtils.DefaultIfEmpty(modToolComponent.ForceLockAudio, "PLAY_LOCKERPRYOPEN1");
			orCreateComponent.m_LocalizedProgressText = new LocalizedString
			{
				m_LocalizationID = "GAMEPLAY_Forcing"
			};
			AlternativeToolManager.AddToList(modToolComponent, "GEAR_Prybar");
		}
	}

	private static void ConfigureIceFishingHole(ModToolComponent modToolComponent)
	{
		if (modToolComponent.IceFishingHole)
		{
			IceFishingHoleClearItem? orCreateComponent = ((Component?)(object)modToolComponent).GetOrCreateComponent<IceFishingHoleClearItem>();
			orCreateComponent.m_BreakIceAudio = ModUtils.DefaultIfEmpty(modToolComponent.IceFishingHoleAudio, "Play_IceBreakingChopping");
			orCreateComponent.m_HPDecreaseToClear = modToolComponent.IceFishingHoleDegradeOnUse;
			orCreateComponent.m_NumGameMinutesToClear = modToolComponent.IceFishingHoleMinutes;
		}
	}

	private static void ConfigureStruggleBonus(ModToolComponent modToolComponent)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		if (modToolComponent.StruggleBonus)
		{
			StruggleBonus? orCreateComponent = ((Component?)(object)modToolComponent).GetOrCreateComponent<StruggleBonus>();
			orCreateComponent.m_BleedoutMinutesScale = modToolComponent.BleedoutMultiplier;
			orCreateComponent.m_CanPuncture = modToolComponent.CanPuncture;
			orCreateComponent.m_DamageScalePercent = modToolComponent.DamageMultiplier;
			orCreateComponent.m_FleeChanceScale = modToolComponent.FleeChanceMultiplier;
			orCreateComponent.m_TapIncrementScale = modToolComponent.TapMultiplier;
			orCreateComponent.m_StruggleWeaponType = GetStruggleWeaponType(modToolComponent);
		}
	}

	private static StruggleWeaponType GetStruggleWeaponType(ModToolComponent modToolComponent)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected I4, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		CuttingToolType toolType = modToolComponent.ToolType;
		return (StruggleWeaponType)((toolType - 2) switch
		{
			0 => 2, 
			1 => 6, 
			2 => 1, 
			_ => 0, 
		});
	}

	private static string? GetTemplateToolName(ModToolComponent modToolComponent)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected I4, but got Unknown
		CuttingToolType toolType = modToolComponent.ToolType;
		return (toolType - 1) switch
		{
			0 => "GEAR_Hacksaw", 
			1 => "GEAR_Hatchet", 
			2 => "GEAR_Hammer", 
			3 => "GEAR_Knife", 
			_ => null, 
		};
	}
}
