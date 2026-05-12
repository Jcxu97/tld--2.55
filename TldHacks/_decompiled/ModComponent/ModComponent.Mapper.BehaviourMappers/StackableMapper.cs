using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ModComponent.API.Behaviours;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Mapper.BehaviourMappers;

internal static class StackableMapper
{
	public static void Configure(ModBaseComponent modComponent)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		ModStackableBehaviour componentSafe = ((Component?)(object)modComponent).GetComponentSafe<ModStackableBehaviour>();
		if (!((Object)(object)componentSafe == (Object)null))
		{
			StackableItem orCreateComponent = ((Component?)(object)componentSafe).GetOrCreateComponent<StackableItem>();
			orCreateComponent.m_LocalizedMultipleUnitText = new LocalizedString
			{
				m_LocalizationID = componentSafe.MultipleUnitTextID
			};
			orCreateComponent.m_LocalizedSingleUnitText = (string.IsNullOrWhiteSpace(componentSafe.SingleUnitTextID) ? NameUtils.CreateLocalizedString(modComponent.DisplayNameLocalizationId) : NameUtils.CreateLocalizedString(componentSafe.SingleUnitTextID));
			orCreateComponent.m_StackSpriteName = componentSafe.StackSprite;
			orCreateComponent.m_Units = componentSafe.UnitsPerItem;
			orCreateComponent.m_DefaultUnitsInItem = componentSafe.UnitsPerItem;
			orCreateComponent.m_ShareStackWithGear = new Il2CppReferenceArray<StackableItem>(0L);
			if (componentSafe.ShareStackWithGear.Length != 0)
			{
				orCreateComponent.m_ShareStackWithGear = Il2CppReferenceArray<StackableItem>.op_Implicit(ModUtils.GetItems<StackableItem>(componentSafe.ShareStackWithGear, ((Object)componentSafe).name));
			}
			if (!string.IsNullOrEmpty(componentSafe.InstantiateStackItem))
			{
				orCreateComponent.m_InstantiateStackItem = AssetBundleUtils.LoadAsset<GameObject>(componentSafe.InstantiateStackItem).GetComponent<GearItem>();
			}
			orCreateComponent.m_StackConditionDifferenceConstraint = componentSafe.StackConditionDifferenceConstraint;
		}
	}
}
