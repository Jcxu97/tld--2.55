using Il2Cpp;
using Il2CppTLD.IntBackedUnit;
using ModComponent.API.Behaviours;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Mapper.BehaviourMappers;

internal static class CarryingCapacityMapper
{
	internal static void Configure(ModBaseComponent modComponent)
	{
		Configure(((Component?)(object)modComponent).GetGameObject());
	}

	public static void Configure(GameObject prefab)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		ModCarryingCapacityBehaviour componentSafe = prefab.GetComponentSafe<ModCarryingCapacityBehaviour>();
		if (!((Object)(object)componentSafe == (Object)null))
		{
			CarryingCapacityBuff? orCreateComponent = ((Component?)(object)componentSafe).GetOrCreateComponent<CarryingCapacityBuff>();
			orCreateComponent.m_IsWorn = (Object)(object)((Component?)(object)componentSafe).GetComponentSafe<ModClothingComponent>() != (Object)null || (Object)(object)((Component?)(object)componentSafe).GetComponentSafe<ClothingItem>() != (Object)null;
			orCreateComponent.m_CarryingCapacityBuffValues = new BuffValues
			{
				m_MaxCarryCapacityBuff = ItemWeight.FromKilograms(componentSafe.MaxCarryCapacityKGBuff)
			};
		}
	}
}
