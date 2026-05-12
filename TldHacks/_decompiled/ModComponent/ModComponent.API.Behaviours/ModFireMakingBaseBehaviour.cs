using System;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.API.Behaviours;

[RegisterTypeInIl2Cpp(false)]
public abstract class ModFireMakingBaseBehaviour : MonoBehaviour
{
	public float SuccessModifier;

	public float DurationOffset;

	public ModFireMakingBaseBehaviour(IntPtr intPtr)
		: base(intPtr)
	{
	}

	[HideFromIl2Cpp]
	internal virtual void InitializeBehaviour(JsonDict jsonDict, string className)
	{
	}
}
