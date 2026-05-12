using System;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.API.Components;

[RegisterTypeInIl2Cpp(false)]
public class ModBedComponent : ModBaseComponent
{
	public float ConditionGainPerHour;

	public float AdditionalConditionGainPerHour;

	public float WarmthBonusCelsius;

	public float DegradePerHour;

	public float BearAttackModifier;

	public float WolfAttackModifier;

	public string OpenAudio = "";

	public string CloseAudio = "";

	public GameObject? PackedMesh;

	public GameObject? UsableMesh;

	private void Awake()
	{
		CopyFieldHandler.UpdateFieldValues<ModBedComponent>(this);
	}

	public ModBedComponent(IntPtr intPtr)
		: base(intPtr)
	{
	}

	[HideFromIl2Cpp]
	internal override void InitializeComponent(JsonDict jsonDict, string className = "ModBedComponent")
	{
		base.InitializeComponent(jsonDict, className);
		JsonDictEntry entry = jsonDict.GetEntry(className);
		ConditionGainPerHour = entry.GetFloat("ConditionGainPerHour");
		AdditionalConditionGainPerHour = entry.GetFloat("AdditionalConditionGainPerHour");
		WarmthBonusCelsius = entry.GetFloat("WarmthBonusCelsius");
		DegradePerHour = entry.GetFloat("DegradePerHour");
		BearAttackModifier = entry.GetFloat("BearAttackModifier");
		WolfAttackModifier = entry.GetFloat("WolfAttackModifier");
		OpenAudio = entry.GetString("OpenAudio");
		CloseAudio = entry.GetString("CloseAudio");
		PackedMesh = ((Component)this).gameObject.GetChild(entry.GetString("PackedMesh"));
		UsableMesh = ((Component)this).gameObject.GetChild(entry.GetString("UsableMesh"));
	}
}
