using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Mapper.ComponentMappers;

internal static class BedMapper
{
	internal static void Configure(ModBaseComponent modComponent)
	{
		ModBedComponent modBedComponent = ((Il2CppObjectBase)modComponent).TryCast<ModBedComponent>();
		if (!((Object)(object)modBedComponent == (Object)null))
		{
			Bed? orCreateComponent = ((Component?)(object)modBedComponent).GetOrCreateComponent<Bed>();
			orCreateComponent.m_ConditionPercentGainPerHour = modBedComponent.ConditionGainPerHour;
			orCreateComponent.m_UinterruptedRestPercentGainPerHour = modBedComponent.AdditionalConditionGainPerHour;
			orCreateComponent.m_WarmthBonusCelsius = modBedComponent.WarmthBonusCelsius;
			orCreateComponent.m_PercentChanceReduceBearAttackWhenSleeping = modBedComponent.BearAttackModifier;
			orCreateComponent.m_PercentChanceReduceWolfAttackWhenSleeping = modBedComponent.WolfAttackModifier;
			orCreateComponent.m_OpenAudio = ModUtils.DefaultIfEmpty(modBedComponent.OpenAudio, "PLAY_SNDGENSLEEPINGBAGCLOSE");
			orCreateComponent.m_CloseAudio = ModUtils.DefaultIfEmpty(modBedComponent.CloseAudio, "PLAY_SNDGENSLEEPINGBAGOPEN");
			orCreateComponent.m_BedRollMesh = modBedComponent.PackedMesh ?? ((Component)modBedComponent).gameObject;
			orCreateComponent.m_BedRollMesh.layer = vp_Layer.Gear;
			orCreateComponent.m_BedRollPlacedMesh = modBedComponent.UsableMesh ?? ((Component)modBedComponent).gameObject;
			orCreateComponent.m_BedRollPlacedMesh.layer = vp_Layer.Gear;
			orCreateComponent.SetState((BedRollState)0);
			DegradeOnUse? orCreateComponent2 = ((Component?)(object)modBedComponent).GetOrCreateComponent<DegradeOnUse>();
			orCreateComponent2.m_DegradeHP = Mathf.Max(orCreateComponent2.m_DegradeHP, modBedComponent.DegradePerHour);
			((Component?)(object)modBedComponent).GetOrCreateComponent<GearItem>().GearItemData.m_IsPlaceable = true;
		}
	}
}
