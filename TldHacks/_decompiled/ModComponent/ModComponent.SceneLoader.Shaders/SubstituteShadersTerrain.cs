using System;
using MelonLoader;
using UnityEngine;

namespace ModComponent.SceneLoader.Shaders;

[RegisterTypeInIl2Cpp(false)]
internal sealed class SubstituteShadersTerrain : MonoBehaviour
{
	public SubstituteShadersTerrain(IntPtr intPtr)
		: base(intPtr)
	{
	}

	private void Awake()
	{
		Terrain component = ((Component)this).GetComponent<Terrain>();
		if ((Object)(object)component != (Object)null)
		{
			ShaderSubstitutionManager.ReplaceDummyShaders(component);
		}
	}
}
