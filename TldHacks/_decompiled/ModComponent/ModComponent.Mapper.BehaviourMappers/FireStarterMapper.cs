using Il2Cpp;
using ModComponent.API.Behaviours;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Mapper.BehaviourMappers;

internal static class FireStarterMapper
{
	internal static void Configure(ModBaseComponent modComponent)
	{
		ModFireStarterBehaviour componentSafe = ((Component?)(object)modComponent).GetComponentSafe<ModFireStarterBehaviour>();
		if (!((Object)(object)componentSafe == (Object)null))
		{
			FireStarterItem? orCreateComponent = ((Component?)(object)componentSafe).GetOrCreateComponent<FireStarterItem>();
			orCreateComponent.m_SecondsToIgniteTinder = componentSafe.SecondsToIgniteTinder;
			orCreateComponent.m_SecondsToIgniteTorch = componentSafe.SecondsToIgniteTorch;
			orCreateComponent.m_FireStartSkillModifier = componentSafe.SuccessModifier;
			orCreateComponent.m_ConditionDegradeOnUse = ItemMapper.GetDecayPerStep(componentSafe.NumberOfUses, modComponent.MaxHP);
			orCreateComponent.m_ConsumeOnUse = componentSafe.DestroyedOnUse;
			orCreateComponent.m_RequiresSunLight = componentSafe.RequiresSunLight;
			orCreateComponent.m_OnUseSoundEvent = componentSafe.OnUseSoundEvent;
		}
	}
}
