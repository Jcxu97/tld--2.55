using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Mapper.ComponentMappers;

internal static class FirstAidMapper
{
	internal static void Configure(ModBaseComponent modComponent)
	{
		ModFirstAidComponent modFirstAidComponent = ((Il2CppObjectBase)modComponent).TryCast<ModFirstAidComponent>();
		if (!((Object)(object)modFirstAidComponent == (Object)null))
		{
			FirstAidItem orCreateComponent = ((Component?)(object)modFirstAidComponent).GetOrCreateComponent<FirstAidItem>();
			orCreateComponent.m_AppliesSutures = false;
			orCreateComponent.m_StabalizesSprains = false;
			switch (modFirstAidComponent.FirstAidType)
			{
			case ModFirstAidComponent.FirstAidKind.Antibiotics:
				orCreateComponent.m_ProvidesAntibiotics = true;
				break;
			case ModFirstAidComponent.FirstAidKind.Bandage:
				orCreateComponent.m_AppliesBandage = true;
				break;
			case ModFirstAidComponent.FirstAidKind.Disinfectant:
				orCreateComponent.m_CleansWounds = true;
				break;
			case ModFirstAidComponent.FirstAidKind.PainKiller:
				orCreateComponent.m_KillsPain = true;
				break;
			}
			orCreateComponent.m_HPIncrease = modFirstAidComponent.InstantHealing;
			orCreateComponent.m_TimeToUseSeconds = modFirstAidComponent.TimeToUseSeconds;
			orCreateComponent.m_UnitsPerUse = modFirstAidComponent.UnitsPerUse;
			orCreateComponent.m_UseAudio = modFirstAidComponent.UseAudio;
		}
	}
}
