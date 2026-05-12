using System;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ModComponent.API.Behaviours;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Mapper.BehaviourMappers;

internal static class HarvestableMapper
{
	internal static void Configure(ModBaseComponent modComponent)
	{
		Configure(((Component?)(object)modComponent).GetGameObject());
	}

	internal static void Configure(GameObject prefab)
	{
		ModHarvestableBehaviour componentSafe = prefab.GetComponentSafe<ModHarvestableBehaviour>();
		if (!((Object)(object)componentSafe == (Object)null))
		{
			Harvest? orCreateComponent = ((Component?)(object)componentSafe).GetOrCreateComponent<Harvest>();
			((HarvestBase)orCreateComponent).m_Audio = componentSafe.Audio;
			((HarvestBase)orCreateComponent).m_DurationMinutes = componentSafe.Minutes;
			if (componentSafe.YieldNames.Length != componentSafe.YieldCounts.Length)
			{
				throw new ArgumentException("YieldNames and YieldCounts do not have the same length on gear item '" + ((Object)componentSafe).name + "'.");
			}
			orCreateComponent.m_YieldGear = Il2CppReferenceArray<GearItem>.op_Implicit(ModUtils.GetItems<GearItem>(componentSafe.YieldNames, ((Object)componentSafe).name));
			orCreateComponent.m_YieldGearUnits = Il2CppStructArray<int>.op_Implicit(componentSafe.YieldCounts);
			((HarvestBase)orCreateComponent).m_AppliedSkillType = (SkillType)(-1);
			((HarvestBase)orCreateComponent).m_RequiredTools = Il2CppReferenceArray<ToolsItem>.op_Implicit(ModUtils.GetItems<ToolsItem>(componentSafe.RequiredToolNames, ((Object)componentSafe).name));
		}
	}
}
