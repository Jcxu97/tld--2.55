using System;
using MelonLoader;
using UnityEngine;

namespace ModComponent.SceneLoader.Shaders;

[RegisterTypeInIl2Cpp(false)]
internal sealed class SubstituteShadersRecursive : MonoBehaviour
{
	public SubstituteShadersRecursive(IntPtr intPtr)
		: base(intPtr)
	{
	}

	private void Awake()
	{
		ShaderSubstitutionManager.ReplaceDummyShaders(((Component)this).gameObject, recursive: true);
	}
}
