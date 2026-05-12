using System;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.API.Behaviours;

[RegisterTypeInIl2Cpp(false)]
public class ModRepairableBehaviour : MonoBehaviour
{
	public string Audio = string.Empty;

	public int Minutes;

	public int Condition;

	public string[] RequiredTools = Array.Empty<string>();

	public string[] MaterialNames = Array.Empty<string>();

	public int[] MaterialCounts = Array.Empty<int>();

	public ModRepairableBehaviour(IntPtr intPtr)
		: base(intPtr)
	{
	}

	[HideFromIl2Cpp]
	internal void InitializeBehaviour(JsonDict jsonDict, string className = "ModRepairableBehaviour")
	{
		JsonDictEntry entry = jsonDict.GetEntry(className);
		Audio = entry.GetString("Audio");
		Minutes = entry.GetInt("Minutes");
		Condition = entry.GetInt("Condition");
		RequiredTools = entry.GetArray<string>("RequiredTools");
		MaterialNames = entry.GetArray<string>("MaterialNames");
		MaterialCounts = entry.GetArray<int>("MaterialCounts");
	}
}
