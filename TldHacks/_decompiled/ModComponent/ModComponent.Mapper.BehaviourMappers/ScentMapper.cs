using Il2Cpp;
using ModComponent.API.Behaviours;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Mapper.BehaviourMappers;

internal static class ScentMapper
{
	internal static void Configure(ModBaseComponent modComponent)
	{
		Configure(((Component?)(object)modComponent).GetGameObject());
	}

	internal static void Configure(GameObject prefab)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		ModScentBehaviour componentSafe = prefab.GetComponentSafe<ModScentBehaviour>();
		if (!((Object)(object)componentSafe == (Object)null))
		{
			((Component?)(object)componentSafe).GetOrCreateComponent<Scent>().m_ScentCategory = componentSafe.scentCategory;
		}
	}

	internal static float GetScentIntensity(ModBaseComponent modComponent)
	{
		return GetScentIntensity(((Component?)(object)modComponent).GetGameObject());
	}

	internal static float GetScentIntensity(GameObject prefab)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected I4, but got Unknown
		Scent componentSafe = prefab.GetComponentSafe<Scent>();
		if ((Object)(object)componentSafe == (Object)null)
		{
			return 0f;
		}
		ScentRangeCategory scentCategory = componentSafe.m_ScentCategory;
		return (int)scentCategory switch
		{
			2 => 5f, 
			3 => 5f, 
			4 => 20f, 
			5 => 50f, 
			0 => 15f, 
			1 => 15f, 
			_ => 0f, 
		};
	}
}
