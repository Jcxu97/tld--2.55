using System;
using Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace ModSettings;

internal class DescriptionHolder : MonoBehaviour
{
	internal string Text
	{
		[HideFromIl2Cpp]
		get;
		[HideFromIl2Cpp]
		private set;
	} = "";


	static DescriptionHolder()
	{
		ClassInjector.RegisterTypeInIl2Cpp<DescriptionHolder>();
	}

	public DescriptionHolder(IntPtr ptr)
		: base(ptr)
	{
	}

	[HideFromIl2Cpp]
	internal void SetDescription(string description, bool localize)
	{
		if (localize)
		{
			Text = description;
		}
		else
		{
			Text = Localization.Get(description);
		}
	}
}
