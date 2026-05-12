using System;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.API.Behaviours;

[RegisterTypeInIl2Cpp(false)]
public class ModHarvestableBehaviour : MonoBehaviour
{
	public string Audio = string.Empty;

	public int Minutes;

	public string[] YieldNames = Array.Empty<string>();

	public int[] YieldCounts = Array.Empty<int>();

	public string[] RequiredToolNames = Array.Empty<string>();

	public ModHarvestableBehaviour(IntPtr intPtr)
		: base(intPtr)
	{
	}

	[HideFromIl2Cpp]
	internal void InitializeBehaviour(JsonDict jsonDict, string className = "ModHarvestableBehaviour")
	{
		JsonDictEntry entry = jsonDict.GetEntry(className);
		Audio = entry.GetString("Audio");
		Minutes = entry.GetInt("Minutes");
		YieldCounts = entry.GetArray<int>("YieldCounts");
		YieldNames = entry.GetArray<string>("YieldNames");
		RequiredToolNames = entry.GetArray<string>("RequiredToolNames");
	}
}
