using System;
using Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using ModComponent.Utils;

namespace ModComponent.API.Components;

[RegisterTypeInIl2Cpp(false)]
public class ModToolComponent : ModBaseEquippableComponent
{
	public CuttingToolType ToolType;

	public float DegradeOnUse = 1f;

	public ToolType Usage;

	public int SkillBonus;

	public float CraftingTimeMultiplier = 1f;

	public float DegradePerHourCrafting;

	public bool BreakDown;

	public float BreakDownTimeMultiplier = 1f;

	public bool ForceLocks;

	public string ForceLockAudio = "";

	public bool IceFishingHole;

	public float IceFishingHoleDegradeOnUse;

	public int IceFishingHoleMinutes;

	public string IceFishingHoleAudio = "";

	public bool CarcassHarvesting;

	public int MinutesPerKgMeat;

	public int MinutesPerKgFrozenMeat;

	public int MinutesPerHide;

	public int MinutesPerGut;

	public float DegradePerHourHarvesting;

	public bool StruggleBonus;

	public float DamageMultiplier = 1f;

	public float FleeChanceMultiplier = 1f;

	public float TapMultiplier = 1f;

	public bool CanPuncture;

	public float BleedoutMultiplier = 1f;

	protected override void Awake()
	{
		CopyFieldHandler.UpdateFieldValues<ModToolComponent>(this);
		base.Awake();
	}

	public ModToolComponent(IntPtr intPtr)
		: base(intPtr)
	{
	}

	[HideFromIl2Cpp]
	internal override void InitializeComponent(JsonDict jsonDict, string className = "ModToolComponent")
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		base.InitializeComponent(jsonDict, className);
		JsonDictEntry entry = jsonDict.GetEntry(className);
		ToolType = entry.GetEnum<CuttingToolType>("ToolType");
		DegradeOnUse = entry.GetFloat("DegradeOnUse");
		Usage = entry.GetEnum<ToolType>("Usage");
		SkillBonus = entry.GetInt("SkillBonus");
		CraftingTimeMultiplier = entry.GetFloat("CraftingTimeMultiplier");
		DegradePerHourCrafting = entry.GetFloat("DegradePerHourCrafting");
		BreakDown = entry.GetBool("BreakDown");
		BreakDownTimeMultiplier = entry.GetFloat("BreakDownTimeMultiplier");
		ForceLocks = entry.GetBool("ForceLocks");
		ForceLockAudio = entry.GetString("ForceLockAudio");
		IceFishingHole = entry.GetBool("IceFishingHole");
		IceFishingHoleDegradeOnUse = entry.GetFloat("IceFishingHoleDegradeOnUse");
		IceFishingHoleMinutes = entry.GetInt("IceFishingHoleMinutes");
		IceFishingHoleAudio = entry.GetString("IceFishingHoleAudio");
		CarcassHarvesting = entry.GetBool("CarcassHarvesting");
		MinutesPerKgMeat = entry.GetInt("MinutesPerKgMeat");
		MinutesPerKgFrozenMeat = entry.GetInt("MinutesPerKgFrozenMeat");
		MinutesPerHide = entry.GetInt("MinutesPerHide");
		MinutesPerGut = entry.GetInt("MinutesPerGut");
		DegradePerHourHarvesting = entry.GetFloat("DegradePerHourHarvesting");
		StruggleBonus = entry.GetBool("StruggleBonus");
		DamageMultiplier = entry.GetFloat("DamageMultiplier");
		FleeChanceMultiplier = entry.GetFloat("FleeChanceMultiplier");
		TapMultiplier = entry.GetFloat("TapMultiplier");
		CanPuncture = entry.GetBool("CanPuncture");
		BleedoutMultiplier = entry.GetFloat("BleedoutMultiplier");
	}
}
