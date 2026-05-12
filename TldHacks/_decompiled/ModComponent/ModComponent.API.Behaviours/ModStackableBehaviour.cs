using System;
using Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.API.Behaviours;

[RegisterTypeInIl2Cpp(false)]
public class ModStackableBehaviour : MonoBehaviour
{
	public string SingleUnitTextID = "";

	public string MultipleUnitTextID = "";

	public string StackSprite = "";

	public int UnitsPerItem = 1;

	public float ChanceFull = 100f;

	public string[] ShareStackWithGear = Array.Empty<string>();

	public string? InstantiateStackItem;

	public float StackConditionDifferenceConstraint = 0.01f;

	private void Awake()
	{
		CopyFieldHandler.UpdateFieldValues<ModStackableBehaviour>(this);
		GearItem component = ((Component)this).GetComponent<GearItem>();
		StackableItem val = ((component != null) ? ((Component)component).GetComponent<StackableItem>() : null);
		if ((Object)(object)val != (Object)null && (Object)(object)component != (Object)null && !component.m_BeenInspected)
		{
			val.m_Units = ((val.m_DefaultUnitsInItem == 1 || RandomUtils.RollChance(ChanceFull)) ? val.m_DefaultUnitsInItem : RandomUtils.Range(1, val.m_DefaultUnitsInItem));
		}
	}

	public ModStackableBehaviour(IntPtr intPtr)
		: base(intPtr)
	{
	}

	[HideFromIl2Cpp]
	internal void InitializeBehaviour(JsonDict jsonDict, string className = "ModStackableBehaviour")
	{
		JsonDictEntry entry = jsonDict.GetEntry(className);
		MultipleUnitTextID = entry.GetString("MultipleUnitTextId");
		StackSprite = entry.GetString("StackSprite");
		SingleUnitTextID = entry.GetString("SingleUnitTextId");
		UnitsPerItem = entry.GetInt("UnitsPerItem", UnitsPerItem);
		ChanceFull = entry.GetFloat("ChanceFull", ChanceFull);
		ShareStackWithGear = entry.GetArray<string>("ShareStackWithGear");
		InstantiateStackItem = entry.GetString("InstantiateStackItem");
		StackConditionDifferenceConstraint = entry.GetFloat("StackConditionDifferenceConstraint", StackConditionDifferenceConstraint);
	}
}
