using System;
using Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.API.Components;

[RegisterTypeInIl2Cpp(false)]
public class ModCookableComponent : ModBaseComponent
{
	public bool Cooking;

	public CookableType Type;

	public int CookingMinutes = 1;

	public int BurntMinutes = 1;

	public float CookingWaterRequired;

	public int CookingUnitsRequired = 1;

	public GameObject? CookingResult;

	public string CookingAudio = "";

	public string StartCookingAudio = "";

	private void Awake()
	{
		CopyFieldHandler.UpdateFieldValues<ModCookableComponent>(this);
	}

	public ModCookableComponent(IntPtr intPtr)
		: base(intPtr)
	{
	}

	[HideFromIl2Cpp]
	internal override void InitializeComponent(JsonDict jsonDict, string className = "ModCookableComponent")
	{
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		base.InitializeComponent(jsonDict, className);
		JsonDictEntry entry = jsonDict.GetEntry(className);
		BurntMinutes = entry.GetInt("BurntMinutes");
		Cooking = entry.GetBool("Cooking");
		CookingAudio = entry.GetString("CookingAudio");
		StartCookingAudio = entry.GetString("StartCookingAudio");
		CookingMinutes = entry.GetInt("CookingMinutes");
		if (string.IsNullOrEmpty(entry.GetString(className, "CookingResult")))
		{
			CookingResult = null;
		}
		else
		{
			string assetName = entry.GetString("CookingResult").ToString();
			CookingResult = AssetBundleUtils.LoadAsset<GameObject>(assetName);
		}
		CookingUnitsRequired = entry.GetInt("CookingUnitsRequired");
		CookingWaterRequired = entry.GetFloat("CookingWaterRequired");
		Type = entry.GetEnum<CookableType>("Type");
	}
}
