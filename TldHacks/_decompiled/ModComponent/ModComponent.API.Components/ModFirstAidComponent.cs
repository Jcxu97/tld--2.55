using System;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using ModComponent.Utils;

namespace ModComponent.API.Components;

[RegisterTypeInIl2Cpp(false)]
public class ModFirstAidComponent : ModBaseComponent
{
	public enum FirstAidKind
	{
		Antibiotics,
		Bandage,
		Disinfectant,
		PainKiller
	}

	public string ProgressBarMessage = "";

	public string RemedyText = "";

	public int InstantHealing;

	public FirstAidKind FirstAidType;

	public int TimeToUseSeconds = 3;

	public int UnitsPerUse = 1;

	public string UseAudio = "";

	private void Awake()
	{
		CopyFieldHandler.UpdateFieldValues<ModFirstAidComponent>(this);
	}

	public ModFirstAidComponent(IntPtr intPtr)
		: base(intPtr)
	{
	}

	[HideFromIl2Cpp]
	internal override void InitializeComponent(JsonDict jsonDict, string className = "ModFirstAidComponent")
	{
		base.InitializeComponent(jsonDict, className);
		JsonDictEntry entry = jsonDict.GetEntry(className);
		ProgressBarMessage = entry.GetString("ProgressBarMessage");
		RemedyText = entry.GetString("RemedyText");
		InstantHealing = entry.GetInt("InstantHealing");
		FirstAidType = entry.GetEnum<FirstAidKind>("FirstAidType");
		TimeToUseSeconds = entry.GetInt("TimeToUseSeconds");
		UnitsPerUse = entry.GetInt("UnitsPerUse");
		UseAudio = entry.GetString("UseAudio");
	}
}
