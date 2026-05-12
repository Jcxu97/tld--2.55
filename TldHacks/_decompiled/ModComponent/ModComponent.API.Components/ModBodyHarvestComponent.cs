using System;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using ModComponent.Utils;

namespace ModComponent.API.Components;

[RegisterTypeInIl2Cpp(false)]
public class ModBodyHarvestComponent : ModBaseComponent
{
	public bool CanCarry;

	public string HarvestAudio = "";

	public string GutPrefab = "";

	public int GutQuantity;

	public float GutWeightKgPerUnit;

	public string HidePrefab = "";

	public int HideQuantity;

	public float HideWeightKgPerUnit;

	public string MeatPrefab = "";

	public float MeatAvailableMinKG;

	public float MeatAvailableMaxKG;

	public bool CanQuarter;

	public string QuarterAudio = "";

	public float QuarterBagMeatCapacityKG;

	public float QuarterBagWasteMultiplier;

	public float QuarterDurationMinutes;

	public string QuarterObjectPrefab = "";

	public float QuarterPrefabSpawnAngle;

	public float QuarterPrefabSpawnRadius;

	public string? GutLabelOverride;

	public string? HideLabelOverride;

	public string? MeatLabelOverride;

	private void Awake()
	{
		CopyFieldHandler.UpdateFieldValues<ModBodyHarvestComponent>(this);
	}

	public ModBodyHarvestComponent(IntPtr intPtr)
		: base(intPtr)
	{
	}

	[HideFromIl2Cpp]
	internal override void InitializeComponent(JsonDict jsonDict, string className = "ModBodyHarvestComponent")
	{
		base.InitializeComponent(jsonDict, className);
		JsonDictEntry entry = jsonDict.GetEntry(className);
		CanCarry = entry.GetBool("CanCarry");
		HarvestAudio = entry.GetString("HarvestAudio");
		GutPrefab = entry.GetString("GutPrefab");
		GutQuantity = entry.GetInt("GutQuantity");
		GutWeightKgPerUnit = entry.GetFloat("GutWeightKgPerUnit");
		HidePrefab = entry.GetString("HidePrefab");
		HideQuantity = entry.GetInt("HideQuantity");
		HideWeightKgPerUnit = entry.GetFloat("HideWeightKgPerUnit");
		MeatPrefab = entry.GetString("MeatPrefab");
		MeatAvailableMinKG = entry.GetFloat("MeatAvailableMinKG");
		MeatAvailableMaxKG = entry.GetFloat("MeatAvailableMaxKG");
		CanQuarter = false;
		QuarterAudio = "";
		QuarterBagMeatCapacityKG = 0f;
		QuarterBagWasteMultiplier = 1f;
		QuarterDurationMinutes = 1f;
		QuarterObjectPrefab = "";
		QuarterPrefabSpawnAngle = 0f;
		QuarterPrefabSpawnRadius = 1f;
		GutLabelOverride = entry.GetString("GutLabelOverride");
		HideLabelOverride = entry.GetString("HideLabelOverride");
		MeatLabelOverride = entry.GetString("MeatLabelOverride");
	}
}
