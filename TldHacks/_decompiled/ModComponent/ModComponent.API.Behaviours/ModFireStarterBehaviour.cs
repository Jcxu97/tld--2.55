using System;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using ModComponent.Utils;

namespace ModComponent.API.Behaviours;

[RegisterTypeInIl2Cpp(false)]
public class ModFireStarterBehaviour : ModFireMakingBaseBehaviour
{
	public float SecondsToIgniteTinder;

	public float SecondsToIgniteTorch;

	public float NumberOfUses;

	public bool RequiresSunLight;

	public string OnUseSoundEvent = string.Empty;

	public bool RuinedAfterUse;

	public bool DestroyedOnUse;

	public ModFireStarterBehaviour(IntPtr intPtr)
		: base(intPtr)
	{
	}

	[HideFromIl2Cpp]
	internal override void InitializeBehaviour(JsonDict jsonDict, string className = "ModFireStarterBehaviour")
	{
		base.InitializeBehaviour(jsonDict, className);
		JsonDictEntry entry = jsonDict.GetEntry(className);
		DestroyedOnUse = entry.GetBool("DestroyedOnUse");
		NumberOfUses = entry.GetFloat("NumberOfUses");
		OnUseSoundEvent = entry.GetString("OnUseSoundEvent");
		RequiresSunLight = entry.GetBool("RequiresSunLight");
		RuinedAfterUse = entry.GetBool("RuinedAfterUse");
		SecondsToIgniteTinder = entry.GetFloat("SecondsToIgniteTinder");
		SecondsToIgniteTorch = entry.GetFloat("SecondsToIgniteTorch");
	}
}
