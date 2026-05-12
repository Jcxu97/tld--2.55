using System;
using MelonLoader;
using UnityEngine;

namespace ModComponent.API.Modifications;

[RegisterTypeInIl2Cpp(false)]
public class AddTag : MonoBehaviour
{
	public string Tag = "";

	public void Awake()
	{
		((Component)this).gameObject.tag = Tag;
	}

	public AddTag(IntPtr intPtr)
		: base(intPtr)
	{
	}
}
