using System;
using Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using Il2CppTLD.Gear;
using Il2CppTLD.IntBackedUnit;
using MelonLoader;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.API.Components;

[RegisterTypeInIl2Cpp(false)]
public class ModPowderComponent : ModBaseComponent
{
	public PowderType ModPowderType;

	public float CapacityKG;

	public float ChanceFull = 100f;

	private void Awake()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		CopyFieldHandler.UpdateFieldValues<ModPowderComponent>(this);
		PowderItem component = ((Component)this).GetComponent<PowderItem>();
		GearItem component2 = ((Component)this).GetComponent<GearItem>();
		if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)component2) && !component2.m_BeenInspected && ChanceFull != 100f && !RandomUtils.RollChance(ChanceFull))
		{
			component.m_Weight = new ItemWeight((long)((float)component.m_WeightLimit.m_Units * RandomUtils.Range(0.125f, 1f)));
		}
	}

	public ModPowderComponent(IntPtr intPtr)
		: base(intPtr)
	{
	}

	[HideFromIl2Cpp]
	internal override void InitializeComponent(JsonDict jsonDict, string className = "ModPowderComponent")
	{
		base.InitializeComponent(jsonDict, className);
		JsonDictEntry entry = jsonDict.GetEntry(className);
		ModPowderType = ScriptableObject.CreateInstance<PowderType>();
		CapacityKG = entry.GetFloat("CapacityKG");
		ChanceFull = entry.GetFloat("ChanceFull");
	}
}
