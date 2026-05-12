using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Mapper.ComponentMappers;

internal static class CharcoalMapper
{
	internal static void Configure(ModBaseComponent modComponent)
	{
		ModCharcoalComponent modCharcoalComponent = ((Il2CppObjectBase)modComponent).TryCast<ModCharcoalComponent>();
		if (!((Object)(object)modCharcoalComponent == (Object)null))
		{
			CharcoalItem? orCreateComponent = ((Component?)(object)modCharcoalComponent).GetOrCreateComponent<CharcoalItem>();
			orCreateComponent.m_SurveyGameMinutes = modCharcoalComponent.SurveyGameMinutes;
			orCreateComponent.m_SurveyLoopAudio = modCharcoalComponent.SurveyLoopAudio;
			orCreateComponent.m_SurveyRealSeconds = modCharcoalComponent.SurveyRealSeconds;
			orCreateComponent.m_SurveySkillExtendedHours = modCharcoalComponent.SurveySkillExtendedHours;
		}
	}
}
