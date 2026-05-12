using System;
using Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using ModComponent.Utils;

namespace ModComponent.API.Components;

[RegisterTypeInIl2Cpp(false)]
public class ModResearchComponent : ModBaseComponent
{
	public SkillType SkillType = (SkillType)(-1);

	public int TimeRequirementHours = 5;

	public int SkillPoints = 10;

	public int NoBenefitAtSkillLevel = 4;

	public string ReadAudio = "Play_ResearchBook";

	public ModResearchComponent(IntPtr intPtr)
		: base(intPtr)
	{
	}//IL_0002: Unknown result type (might be due to invalid IL or missing references)


	[HideFromIl2Cpp]
	internal override void InitializeComponent(JsonDict jsonDict, string className = "ModResearchComponent")
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		base.InitializeComponent(jsonDict, className);
		JsonDictEntry entry = jsonDict.GetEntry(className);
		SkillType = entry.GetEnum<SkillType>("SkillType");
		TimeRequirementHours = entry.GetInt("TimeRequirementHours");
		SkillPoints = entry.GetInt("SkillPoints");
		NoBenefitAtSkillLevel = entry.GetInt("NoBenefitAtSkillLevel");
		ReadAudio = entry.GetString("ReadAudio");
	}
}
