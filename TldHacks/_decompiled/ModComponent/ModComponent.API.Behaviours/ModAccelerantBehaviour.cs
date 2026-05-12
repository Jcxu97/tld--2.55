using System;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using ModComponent.Utils;

namespace ModComponent.API.Behaviours;

[RegisterTypeInIl2Cpp(false)]
public class ModAccelerantBehaviour : ModFireMakingBaseBehaviour
{
	public bool DestroyedOnUse;

	public ModAccelerantBehaviour(IntPtr intPtr)
		: base(intPtr)
	{
	}

	[HideFromIl2Cpp]
	internal override void InitializeBehaviour(JsonDict jsonDict, string className = "ModAccelerantBehaviour")
	{
		base.InitializeBehaviour(jsonDict, className);
		JsonDictEntry entry = jsonDict.GetEntry(className);
		DestroyedOnUse = entry.GetBool("DestroyedOnUse");
	}
}
