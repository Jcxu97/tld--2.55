using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Mapper.ComponentMappers;

internal static class ResearchMapper
{
	internal static void Configure(ModBaseComponent modComponent)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		ModResearchComponent modResearchComponent = ((Il2CppObjectBase)modComponent).TryCast<ModResearchComponent>();
		if (!((Object)(object)modResearchComponent == (Object)null))
		{
			ResearchItem? orCreateComponent = ((Component?)(object)modResearchComponent).GetOrCreateComponent<ResearchItem>();
			orCreateComponent.m_ReadAudio = modResearchComponent.ReadAudio;
			orCreateComponent.m_SkillPoints = modResearchComponent.SkillPoints;
			orCreateComponent.m_NoBenefitAtSkillLevel = modResearchComponent.NoBenefitAtSkillLevel;
			orCreateComponent.m_SkillType = modResearchComponent.SkillType;
			orCreateComponent.m_TimeRequirementHours = modResearchComponent.TimeRequirementHours;
		}
	}
}
