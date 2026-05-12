using System;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.API.Components;

[RegisterTypeInIl2Cpp(false)]
public class ModLiquidComponent : ModBaseComponent
{
	public enum LiquidKind
	{
		Water,
		Kerosene
	}

	public LiquidKind LiquidType;

	public float LiquidCapacityLiters;

	public bool RandomizeQuantity;

	public float LiquidLiters;

	private void Awake()
	{
		CopyFieldHandler.UpdateFieldValues<ModLiquidComponent>(this);
	}

	public ModLiquidComponent(IntPtr intPtr)
		: base(intPtr)
	{
	}

	[HideFromIl2Cpp]
	internal override void InitializeComponent(JsonDict jsonDict, string className = "ModLiquidComponent")
	{
		base.InitializeComponent(jsonDict, className);
		JsonDictEntry entry = jsonDict.GetEntry(className);
		LiquidType = entry.GetEnum<LiquidKind>("LiquidType");
		LiquidCapacityLiters = entry.GetFloat("LiquidCapacityLiters");
		RandomizeQuantity = entry.GetBool("RandomizedQuantity");
		LiquidLiters = Mathf.Clamp(entry.GetFloat("LiquidLiters"), 0f, LiquidCapacityLiters);
	}
}
