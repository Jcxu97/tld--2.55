using Il2Cpp;
using ModComponent.API.Behaviours;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Mapper.BehaviourMappers;

internal static class AccelerantMapper
{
	internal static void Configure(ModBaseComponent modComponent)
	{
		Configure(((Component?)(object)modComponent).GetGameObject());
	}

	public static void Configure(GameObject prefab)
	{
		ModAccelerantBehaviour componentSafe = prefab.GetComponentSafe<ModAccelerantBehaviour>();
		if (!((Object)(object)componentSafe == (Object)null))
		{
			FireStarterItem? orCreateComponent = ((Component?)(object)componentSafe).GetOrCreateComponent<FireStarterItem>();
			orCreateComponent.m_IsAccelerant = true;
			orCreateComponent.m_FireStartDurationModifier = componentSafe.DurationOffset;
			orCreateComponent.m_FireStartSkillModifier = componentSafe.SuccessModifier;
			orCreateComponent.m_ConsumeOnUse = componentSafe.DestroyedOnUse;
		}
	}
}
