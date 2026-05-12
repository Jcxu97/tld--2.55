using Il2Cpp;
using ModComponent.API.Behaviours;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Mapper.BehaviourMappers;

internal static class TinderMapper
{
	internal static void Configure(ModBaseComponent modComponent)
	{
		Configure(((Component?)(object)modComponent).GetGameObject());
	}

	public static void Configure(GameObject prefab)
	{
		ModTinderBehaviour componentSafe = prefab.GetComponentSafe<ModTinderBehaviour>();
		if (!((Object)(object)componentSafe == (Object)null))
		{
			FuelSourceItem? orCreateComponent = ((Component?)(object)componentSafe).GetOrCreateComponent<FuelSourceItem>();
			orCreateComponent.m_BurnDurationHours = 0.02f;
			orCreateComponent.m_FireAgeMinutesBeforeAdding = 0f;
			orCreateComponent.m_FireStartSkillModifier = componentSafe.SuccessModifier;
			orCreateComponent.m_HeatIncrease = 5f;
			orCreateComponent.m_HeatInnerRadius = 2.5f;
			orCreateComponent.m_HeatOuterRadius = 6f;
			orCreateComponent.m_FireStartDurationModifier = componentSafe.DurationOffset;
			orCreateComponent.m_IsWet = false;
			orCreateComponent.m_IsTinder = true;
			orCreateComponent.m_IsBurntInFireTracked = false;
		}
	}
}
