using System;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.API.Behaviours;

[RegisterTypeInIl2Cpp(false)]
public class ModCarryingCapacityBehaviour : MonoBehaviour
{
	public float MaxCarryCapacityKGBuff;

	public ModCarryingCapacityBehaviour(IntPtr intPtr)
		: base(intPtr)
	{
	}

	[HideFromIl2Cpp]
	internal void InitializeBehaviour(JsonDict jsonDict, string className = "ModCarryingCapacityBehaviour")
	{
		JsonDictEntry entry = jsonDict.GetEntry(className);
		MaxCarryCapacityKGBuff = entry.GetFloat("MaxCarryCapacityKGBuff");
	}
}
