using System.Collections.Generic;
using System.Linq;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ModComponent.API.Behaviours;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Mapper.BehaviourMappers;

internal static class SharpenableMapper
{
	internal static void Configure(ModBaseComponent modComponent)
	{
		Configure(((Component?)(object)modComponent).GetGameObject());
	}

	public static void Configure(GameObject prefab)
	{
		ModSharpenableBehaviour componentSafe = prefab.GetComponentSafe<ModSharpenableBehaviour>();
		if (!((Object)(object)componentSafe == (Object)null))
		{
			Sharpenable? orCreateComponent = ((Component?)(object)componentSafe).GetOrCreateComponent<Sharpenable>();
			orCreateComponent.m_ConditionIncreaseMax = componentSafe.ConditionMax;
			orCreateComponent.m_ConditionIncreaseMin = componentSafe.ConditionMin;
			orCreateComponent.m_DurationMinutesMax = componentSafe.MinutesMax;
			orCreateComponent.m_DurationMinutesMin = componentSafe.MinutesMin;
			orCreateComponent.m_SharpenToolChoices = Il2CppReferenceArray<ToolsItem>.op_Implicit(ModUtils.GetItems<ToolsItem>(componentSafe.Tools, ((Object)prefab).name + ": Tools"));
			orCreateComponent.m_RequiresToolToSharpen = ((IEnumerable<ToolsItem>)orCreateComponent.m_SharpenToolChoices).Count() > 0;
			orCreateComponent.m_SharpenAudio = componentSafe.Audio;
		}
	}
}
