using System;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ModComponent.API.Behaviours;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Mapper.BehaviourMappers;

internal static class MillableMapper
{
	internal static void Configure(ModBaseComponent modComponent)
	{
		Configure(((Component?)(object)modComponent).GetGameObject());
	}

	internal static void Configure(GameObject prefab)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		ModMillableBehaviour componentSafe = prefab.GetComponentSafe<ModMillableBehaviour>();
		if (!((Object)(object)componentSafe == (Object)null))
		{
			Millable? orCreateComponent = ((Component?)(object)componentSafe).GetOrCreateComponent<Millable>();
			orCreateComponent.m_CanRestoreFromWornOut = componentSafe.CanRestoreFromWornOut;
			orCreateComponent.m_RecoveryDurationMinutes = componentSafe.RecoveryDurationMinutes;
			orCreateComponent.m_RepairDurationMinutes = componentSafe.RepairDurationMinutes;
			orCreateComponent.m_Skill = componentSafe.Skill;
			if (componentSafe.RepairRequiredGear.Length != componentSafe.RepairRequiredGearUnits.Length)
			{
				throw new ArgumentException("RepairRequiredGear and RepairRequiredGearUnits do not have the same length on gear item '" + ((Object)componentSafe).name + "'.");
			}
			orCreateComponent.m_RepairRequiredGear = Il2CppReferenceArray<GearItem>.op_Implicit(ModUtils.GetItems<GearItem>(componentSafe.RepairRequiredGear, ((Object)componentSafe).name));
			orCreateComponent.m_RepairRequiredGearUnits = Il2CppStructArray<int>.op_Implicit(componentSafe.RepairRequiredGearUnits);
			if (componentSafe.RestoreRequiredGear.Length != componentSafe.RestoreRequiredGearUnits.Length)
			{
				throw new ArgumentException("RestoreRequiredGear and RestoreRequiredGearUnits do not have the same length on gear item '" + ((Object)componentSafe).name + "'.");
			}
			orCreateComponent.m_RestoreRequiredGear = Il2CppReferenceArray<GearItem>.op_Implicit(ModUtils.GetItems<GearItem>(componentSafe.RestoreRequiredGear, ((Object)componentSafe).name));
			orCreateComponent.m_RestoreRequiredGearUnits = Il2CppStructArray<int>.op_Implicit(componentSafe.RestoreRequiredGearUnits);
		}
	}
}
