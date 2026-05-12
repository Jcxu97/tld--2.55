using System;
using System.Collections.Generic;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace ModComponent.SceneLoader.Shaders;

internal static class ShaderSubstitutionManager
{
	internal static List<Material> fixedMaterials = new List<Material>();

	public static void ReplaceDummyShaders(GameObject obj, bool recursive)
	{
		if ((Object)(object)obj == (Object)null)
		{
			return;
		}
		if (recursive)
		{
			foreach (Renderer componentsInChild in obj.GetComponentsInChildren<Renderer>(true))
			{
				ReplaceDummyShaders(componentsInChild);
			}
			return;
		}
		foreach (Renderer component in obj.GetComponents<Renderer>())
		{
			ReplaceDummyShaders(component);
		}
	}

	public static void ReplaceDummyShaders(Renderer renderer)
	{
		if ((Object)(object)renderer == (Object)null)
		{
			return;
		}
		try
		{
			foreach (Material item in (Il2CppArrayBase<Material>)(object)renderer.sharedMaterials)
			{
				FixMaterial(item);
			}
		}
		catch (Exception ex)
		{
			Logger.LogError(ex.ToString());
		}
	}

	public static void ReplaceDummyShaders(Terrain terrain)
	{
		if ((Object)(object)terrain == (Object)null)
		{
			return;
		}
		try
		{
			TerrainData terrainData = terrain.terrainData;
			if ((Object)(object)terrainData == (Object)null)
			{
				return;
			}
			foreach (TreePrototype item in (Il2CppArrayBase<TreePrototype>)(object)terrainData.treePrototypes)
			{
				if (item != null)
				{
					ReplaceDummyShaders(item.m_Prefab, recursive: true);
				}
			}
			foreach (DetailPrototype item2 in (Il2CppArrayBase<DetailPrototype>)(object)terrainData.detailPrototypes)
			{
				if (item2 != null)
				{
					ReplaceDummyShaders(item2.m_Prototype, recursive: true);
				}
			}
			terrainData.RefreshPrototypes();
		}
		catch (Exception ex)
		{
			Logger.LogError(ex.ToString());
		}
	}

	private static void FixMaterial(Material material)
	{
		if (!((Object)(object)material == (Object)null) && !fixedMaterials.Contains(material))
		{
			fixedMaterials.Add(material);
			if (((Object)material.shader).name.StartsWith("Dummy"))
			{
				material.shader = Shader.Find(((Object)material.shader).name.Replace("Dummy", "")) ?? material.shader;
			}
			else if (ShaderList.DummyShaderReplacements.ContainsKey(((Object)material.shader).name))
			{
				material.shader = Shader.Find(ShaderList.DummyShaderReplacements[((Object)material.shader).name]);
			}
		}
	}

	public static void AddDummyShaderReplacement(string dummyName, string inGameName)
	{
		ShaderList.DummyShaderReplacements.Add(dummyName, inGameName);
	}
}
