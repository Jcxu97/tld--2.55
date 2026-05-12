using System;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ModComponent.API.Behaviours;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Mapper.BehaviourMappers;

internal static class RepairableMapper
{
	internal static void Configure(ModBaseComponent modComponent)
	{
		Configure(((Component?)(object)modComponent).GetGameObject());
	}

	internal static void Configure(GameObject prefab)
	{
		ModRepairableBehaviour componentSafe = prefab.GetComponentSafe<ModRepairableBehaviour>();
		if (!((Object)(object)componentSafe == (Object)null))
		{
			Repairable? orCreateComponent = ((Component?)(object)componentSafe).GetOrCreateComponent<Repairable>();
			orCreateComponent.m_RepairAudio = componentSafe.Audio;
			orCreateComponent.m_DurationMinutes = componentSafe.Minutes;
			orCreateComponent.m_ConditionIncrease = componentSafe.Condition;
			if (componentSafe.MaterialNames.Length != componentSafe.MaterialCounts.Length)
			{
				throw new ArgumentException("MaterialNames and MaterialCounts do not have the same length on gear item '" + ((Object)componentSafe).name + "'.");
			}
			orCreateComponent.m_RequiredGear = Il2CppReferenceArray<GearItem>.op_Implicit(ModUtils.GetItems<GearItem>(componentSafe.MaterialNames, ((Object)componentSafe).name));
			orCreateComponent.m_RequiredGearUnits = Il2CppStructArray<int>.op_Implicit(componentSafe.MaterialCounts);
			orCreateComponent.m_RepairToolChoices = Il2CppReferenceArray<ToolsItem>.op_Implicit(ModUtils.GetItems<ToolsItem>(componentSafe.RequiredTools, ((Object)componentSafe).name));
			orCreateComponent.m_RequiresToolToRepair = ((Il2CppArrayBase<ToolsItem>)(object)orCreateComponent.m_RepairToolChoices).Length > 0;
		}
	}
}
