using System;
using Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.API.Behaviours;

[RegisterTypeInIl2Cpp(false)]
public class ModScentBehaviour : MonoBehaviour
{
	public ScentRangeCategory scentCategory;

	public ModScentBehaviour(IntPtr intPtr)
		: base(intPtr)
	{
	}

	[HideFromIl2Cpp]
	internal void InitializeBehaviour(JsonDict jsonDict, string className = "ModScentBehaviour")
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		JsonDictEntry entry = jsonDict.GetEntry(className);
		scentCategory = entry.GetEnum<ScentRangeCategory>("ScentCategory");
	}
}
