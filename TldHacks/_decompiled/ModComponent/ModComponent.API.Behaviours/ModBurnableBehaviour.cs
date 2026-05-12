using System;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using ModComponent.Utils;

namespace ModComponent.API.Behaviours;

[RegisterTypeInIl2Cpp(false)]
public class ModBurnableBehaviour : ModFireMakingBaseBehaviour
{
	public int BurningMinutes;

	public float BurningMinutesBeforeAllowedToAdd;

	public float TempIncrease;

	public ModBurnableBehaviour(IntPtr intPtr)
		: base(intPtr)
	{
	}

	[HideFromIl2Cpp]
	internal override void InitializeBehaviour(JsonDict jsonDict, string className = "ModBurnableBehaviour")
	{
		base.InitializeBehaviour(jsonDict, className);
		JsonDictEntry entry = jsonDict.GetEntry(className);
		BurningMinutes = entry.GetInt("BurningMinutes");
		BurningMinutesBeforeAllowedToAdd = entry.GetFloat("BurningMinutesBeforeAllowedToAdd");
		TempIncrease = entry.GetFloat("TempIncrease");
	}
}
