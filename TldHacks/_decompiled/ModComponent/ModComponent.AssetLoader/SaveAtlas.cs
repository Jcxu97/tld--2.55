using System;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace ModComponent.AssetLoader;

[RegisterTypeInIl2Cpp(false)]
internal class SaveAtlas : MonoBehaviour
{
	public UIAtlas? original;

	public SaveAtlas(IntPtr intPtr)
		: base(intPtr)
	{
	}
}
