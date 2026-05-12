using Il2Cpp;
using ModComponent.API.Behaviours;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Mapper.BehaviourMappers;

internal static class BurnableMapper
{
	internal static void Configure(ModBaseComponent modComponent)
	{
		Configure(((Component?)(object)modComponent).GetGameObject());
	}

	public static void Configure(GameObject prefab)
	{
		ModBurnableBehaviour componentSafe = prefab.GetComponentSafe<ModBurnableBehaviour>();
		if (!((Object)(object)componentSafe == (Object)null))
		{
			FuelSourceItem? orCreateComponent = ((Component?)(object)componentSafe).GetOrCreateComponent<FuelSourceItem>();
			orCreateComponent.m_BurnDurationHours = (float)componentSafe.BurningMinutes / 60f;
			orCreateComponent.m_FireAgeMinutesBeforeAdding = componentSafe.BurningMinutesBeforeAllowedToAdd;
			orCreateComponent.m_FireStartSkillModifier = componentSafe.SuccessModifier;
			orCreateComponent.m_HeatIncrease = componentSafe.TempIncrease;
			orCreateComponent.m_HeatInnerRadius = 2.5f;
			orCreateComponent.m_HeatOuterRadius = 6f;
			orCreateComponent.m_FireStartDurationModifier = componentSafe.DurationOffset;
			orCreateComponent.m_IsWet = false;
			orCreateComponent.m_IsTinder = false;
			orCreateComponent.m_IsBurntInFireTracked = false;
		}
	}
}
