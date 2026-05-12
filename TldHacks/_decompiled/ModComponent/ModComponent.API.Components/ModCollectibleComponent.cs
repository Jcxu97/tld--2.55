using System;
using Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using ModComponent.Utils;

namespace ModComponent.API.Components;

[RegisterTypeInIl2Cpp(false)]
public class ModCollectibleComponent : ModBaseComponent
{
	public string HudMessageLocalizationId = "";

	public string NarrativeTextLocalizationId = "";

	public Alignment TextAlignment;

	public ModCollectibleComponent(IntPtr intPtr)
		: base(intPtr)
	{
	}

	[HideFromIl2Cpp]
	internal override void InitializeComponent(JsonDict jsonDict, string className = "ModCollectibleComponent")
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		base.InitializeComponent(jsonDict, className);
		JsonDictEntry entry = jsonDict.GetEntry(className);
		HudMessageLocalizationId = entry.GetString("HudMessageLocalizationId");
		NarrativeTextLocalizationId = entry.GetString("NarrativeTextLocalizationId");
		TextAlignment = entry.GetEnum<Alignment>("TextAlignment");
	}
}
