using System;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.API.Components;

[RegisterTypeInIl2Cpp(false)]
public class ModCookingPotComponent : ModBaseComponent
{
	public bool CanCookLiquid;

	public bool CanCookGrub;

	public bool CanCookMeat;

	public float Capacity;

	public string Template = "";

	public Mesh? SnowMesh;

	public Mesh? WaterMesh;

	private void Awake()
	{
		CopyFieldHandler.UpdateFieldValues<ModCookingPotComponent>(this);
	}

	public ModCookingPotComponent(IntPtr intPtr)
		: base(intPtr)
	{
	}

	[HideFromIl2Cpp]
	internal override void InitializeComponent(JsonDict jsonDict, string className = "ModCookingPotComponent")
	{
		base.InitializeComponent(jsonDict, className);
		JsonDictEntry entry = jsonDict.GetEntry(className);
		CanCookLiquid = entry.GetBool("CanCookLiquid");
		CanCookGrub = entry.GetBool("CanCookGrub");
		CanCookMeat = entry.GetBool("CanCookMeat");
		Capacity = entry.GetFloat("Capacity");
		Template = entry.GetString("Template");
		SnowMesh = null;
		WaterMesh = null;
	}
}
