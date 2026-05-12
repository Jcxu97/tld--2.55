using System;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using ModComponent.Utils;

namespace ModComponent.API.Components;

[RegisterTypeInIl2Cpp(false)]
public class ModPurificationComponent : ModBaseComponent
{
	public float LitersPurify = 1f;

	public float ProgressBarDurationSeconds = 5f;

	public string ProgressBarLocalizationID = "GAMEPLAY_PurifyingWater";

	public string PurifyAudio = "Play_WaterPurification";

	public ModPurificationComponent(IntPtr intPtr)
		: base(intPtr)
	{
	}

	[HideFromIl2Cpp]
	internal override void InitializeComponent(JsonDict jsonDict, string className = "ModPurificationComponent")
	{
		base.InitializeComponent(jsonDict, className);
		JsonDictEntry entry = jsonDict.GetEntry(className);
		LitersPurify = entry.GetFloat("LitersPurify");
		ProgressBarDurationSeconds = entry.GetFloat("ProgressBarDurationSeconds");
		ProgressBarLocalizationID = entry.GetString("ProgressBarLocalizationID");
		PurifyAudio = entry.GetString("PurifyAudio");
	}
}
