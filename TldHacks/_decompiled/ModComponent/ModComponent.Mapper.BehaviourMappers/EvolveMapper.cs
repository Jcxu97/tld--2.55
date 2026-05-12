using Il2Cpp;
using ModComponent.API.Behaviours;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Mapper.BehaviourMappers;

internal static class EvolveMapper
{
	internal static void Configure(ModBaseComponent modComponent)
	{
		Configure(((Component?)(object)modComponent).GetGameObject());
	}

	internal static void Configure(GameObject prefab)
	{
		ModEvolveBehaviour componentSafe = prefab.GetComponentSafe<ModEvolveBehaviour>();
		if (!((Object)(object)componentSafe == (Object)null))
		{
			EvolveItem? orCreateComponent = ((Component?)(object)componentSafe).GetOrCreateComponent<EvolveItem>();
			orCreateComponent.m_ForceNoAutoEvolve = false;
			orCreateComponent.m_GearItemToBecome = GetTargetItem(componentSafe.TargetItemName, ((Object)componentSafe).name);
			orCreateComponent.m_RequireIndoors = componentSafe.IndoorsOnly;
			orCreateComponent.m_StartEvolvePercent = 0;
			orCreateComponent.m_TimeToEvolveGameDays = Mathf.Clamp((float)componentSafe.EvolveHours / 24f, 0.01f, 1000f);
		}
	}

	private static GearItem GetTargetItem(string targetItemName, string reference)
	{
		GameObject val = AssetBundleUtils.LoadAsset<GameObject>(targetItemName);
		if ((Object)(object)val != (Object)null && (Object)(object)val.GetModComponent() != (Object)null)
		{
			ItemMapper.Map(val);
		}
		return ModUtils.GetItem<GearItem>(targetItemName, reference);
	}
}
