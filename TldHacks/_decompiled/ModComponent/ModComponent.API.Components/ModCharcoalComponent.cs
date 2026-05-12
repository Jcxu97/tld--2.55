using System;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using ModComponent.Utils;

namespace ModComponent.API.Components;

[RegisterTypeInIl2Cpp(false)]
public class ModCharcoalComponent : ModBaseComponent
{
	public float SurveyGameMinutes = 15f;

	public float SurveyRealSeconds = 3f;

	public float SurveySkillExtendedHours = 1f;

	public string SurveyLoopAudio = "Play_MapCharcoalWriting";

	public ModCharcoalComponent(IntPtr intPtr)
		: base(intPtr)
	{
	}

	[HideFromIl2Cpp]
	internal override void InitializeComponent(JsonDict jsonDict, string className = "ModCharcoalComponent")
	{
		base.InitializeComponent(jsonDict, className);
		JsonDictEntry entry = jsonDict.GetEntry(className);
		SurveyGameMinutes = entry.GetFloat("SurveyGameMinutes");
		SurveyRealSeconds = entry.GetFloat("SurveyRealSeconds");
		SurveySkillExtendedHours = entry.GetFloat("SurveySkillExtendedHours");
		SurveyLoopAudio = entry.GetString("SurveyLoopAudio");
	}
}
