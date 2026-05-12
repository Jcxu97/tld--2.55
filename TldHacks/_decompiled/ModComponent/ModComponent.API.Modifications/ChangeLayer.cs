using System;
using Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.API.Modifications;

[RegisterTypeInIl2Cpp(false)]
public class ChangeLayer : MonoBehaviour
{
	public int Layer;

	public bool Recursively;

	public void Start()
	{
		CopyFieldHandler.UpdateFieldValues<ChangeLayer>(this);
		((MonoBehaviour)this).Invoke("SetLayer", 1f);
	}

	internal void SetLayer()
	{
		vp_Layer.Set(((Component)this).gameObject, Layer, Recursively);
		Object.Destroy((Object)(object)this);
	}

	public ChangeLayer(IntPtr intPtr)
		: base(intPtr)
	{
	}

	[HideFromIl2Cpp]
	internal void InitializeModification(JsonDict jsonDict, string inheritanceName = "ChangeLayer")
	{
		JsonDictEntry entry = jsonDict.GetEntry(inheritanceName);
		if (entry != null)
		{
			Recursively = entry.GetBool("Recursively");
			Layer = entry.GetInt("Layer");
		}
	}
}
