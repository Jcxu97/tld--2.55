using System;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.API.Behaviours;

[RegisterTypeInIl2Cpp(false)]
public class ModSharpenableBehaviour : MonoBehaviour
{
	public string[] Tools = Array.Empty<string>();

	public int MinutesMin;

	public int MinutesMax;

	public float ConditionMin;

	public float ConditionMax;

	public string Audio = "";

	public ModSharpenableBehaviour(IntPtr intPtr)
		: base(intPtr)
	{
	}

	[HideFromIl2Cpp]
	internal void InitializeBehaviour(JsonDict jsonDict, string className = "ModSharpenableBehaviour")
	{
		JsonDictEntry entry = jsonDict.GetEntry(className);
		Audio = entry.GetString("Audio");
		MinutesMin = entry.GetInt("MinutesMin");
		MinutesMax = entry.GetInt("MinutesMax");
		ConditionMin = entry.GetFloat("ConditionMin");
		ConditionMax = entry.GetFloat("ConditionMax");
		Tools = entry.GetArray<string>("Tools");
	}
}
