using System;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using ModComponent.Utils;

namespace ModComponent.API.Behaviours;

[RegisterTypeInIl2Cpp(false)]
public class ModTinderBehaviour : ModFireMakingBaseBehaviour
{
	public ModTinderBehaviour(IntPtr intPtr)
		: base(intPtr)
	{
	}

	[HideFromIl2Cpp]
	internal override void InitializeBehaviour(JsonDict jsonDict, string className = "ModTinderBehaviour")
	{
		base.InitializeBehaviour(jsonDict, className);
		jsonDict.GetEntry(className);
	}
}
