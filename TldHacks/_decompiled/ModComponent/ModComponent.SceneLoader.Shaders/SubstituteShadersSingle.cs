using System;
using MelonLoader;
using UnityEngine;

namespace ModComponent.SceneLoader.Shaders;

[RegisterTypeInIl2Cpp(false)]
internal sealed class SubstituteShadersSingle : MonoBehaviour
{
	public SubstituteShadersSingle(IntPtr intPtr)
		: base(intPtr)
	{
	}

	private void Awake()
	{
		ShaderSubstitutionManager.ReplaceDummyShaders(((Component)this).gameObject, recursive: false);
	}
}
