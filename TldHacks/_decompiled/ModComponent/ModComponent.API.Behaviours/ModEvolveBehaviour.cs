using System;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.API.Behaviours;

[RegisterTypeInIl2Cpp(false)]
public class ModEvolveBehaviour : MonoBehaviour
{
	public string TargetItemName = string.Empty;

	public bool IndoorsOnly;

	public int EvolveHours = 1;

	public ModEvolveBehaviour(IntPtr intPtr)
		: base(intPtr)
	{
	}

	[HideFromIl2Cpp]
	internal void InitializeBehaviour(JsonDict jsonDict, string className = "ModEvolveBehaviour")
	{
		JsonDictEntry entry = jsonDict.GetEntry(className);
		TargetItemName = entry.GetString("TargetItemName");
		EvolveHours = entry.GetInt("EvolveHours");
		IndoorsOnly = entry.GetBool("IndoorsOnly");
	}
}
