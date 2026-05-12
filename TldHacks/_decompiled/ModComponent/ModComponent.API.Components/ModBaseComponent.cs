using System;
using Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.API.Components;

[RegisterTypeInIl2Cpp(false)]
public abstract class ModBaseComponent : MonoBehaviour
{
	public enum ItemCategory
	{
		Auto,
		Clothing,
		FirstAid,
		Firestarting,
		Food,
		Material,
		Tool
	}

	public string ConsoleName = "";

	public string DisplayNameLocalizationId = "";

	public string DescriptionLocalizatonId = "";

	public ItemCategory InventoryCategory;

	public string InventoryActionLocalizationId = "";

	public string PickUpAudio = "";

	public string StowAudio = "Play_InventoryStow";

	public string PutBackAudio = "";

	public string WornOutAudio = "";

	public float WeightKG;

	public float MaxHP;

	public float DaysToDecay;

	public GearStartCondition InitialCondition;

	public bool InspectOnPickup;

	public float InspectDistance = 0.4f;

	public Vector3 InspectScale = Vector3.one;

	public Vector3 InspectAngles = Vector3.zero;

	public Vector3 InspectOffset = Vector3.zero;

	public GameObject? InspectModel;

	public GameObject? NormalModel;

	[HideFromIl2Cpp]
	public string GetEffectiveConsoleName()
	{
		if (string.IsNullOrEmpty(ConsoleName))
		{
			return ((Object)this).name.Replace("GEAR_", "");
		}
		return ConsoleName;
	}

	public ModBaseComponent(IntPtr intPtr)
		: base(intPtr)
	{
	}//IL_0064: Unknown result type (might be due to invalid IL or missing references)
	//IL_0069: Unknown result type (might be due to invalid IL or missing references)
	//IL_006f: Unknown result type (might be due to invalid IL or missing references)
	//IL_0074: Unknown result type (might be due to invalid IL or missing references)
	//IL_007a: Unknown result type (might be due to invalid IL or missing references)
	//IL_007f: Unknown result type (might be due to invalid IL or missing references)


	[HideFromIl2Cpp]
	internal virtual void InitializeComponent(JsonDict jsonDict, string inheritanceName)
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		JsonDictEntry entry = jsonDict.GetEntry(inheritanceName);
		ConsoleName = NameUtils.RemoveGearPrefix(((Object)((Component)this).gameObject).name);
		DisplayNameLocalizationId = entry.GetString("DisplayNameLocalizationId");
		DescriptionLocalizatonId = entry.GetString("DescriptionLocalizatonId");
		InventoryActionLocalizationId = entry.GetString("InventoryActionLocalizationId");
		WeightKG = entry.GetFloat("WeightKG");
		DaysToDecay = entry.GetFloat("DaysToDecay");
		MaxHP = entry.GetFloat("MaxHP");
		InitialCondition = entry.GetEnum<GearStartCondition>("InitialCondition");
		InventoryCategory = entry.GetEnum<ItemCategory>("InventoryCategory");
		PickUpAudio = entry.GetString("PickUpAudio");
		PutBackAudio = entry.GetString("PutBackAudio");
		StowAudio = entry.GetString("StowAudio");
		WornOutAudio = entry.GetString("WornOutAudio");
		InspectOnPickup = entry.GetBool("InspectOnPickup");
		InspectDistance = entry.GetFloat("InspectDistance", InspectDistance);
		InspectAngles = entry.GetVector3("InspectAngles");
		InspectOffset = entry.GetVector3("InspectOffset");
		InspectScale = entry.GetVector3("InspectScale");
		NormalModel = ((Component)this).gameObject.GetChild(entry.GetString("NormalModel"));
		InspectModel = ((Component)this).gameObject.GetChild(entry.GetString("InspectModel"));
		Validate();
	}

	internal virtual void Validate()
	{
	}
}
