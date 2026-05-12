using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.IO;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HouseLights;

internal class HouseLightsUtils
{
	public static Shader vanillaShader = Shader.Find("Shader Forge/TLD_StandardDiffuse");

	internal static object InvokePrivMethod(object inst, string name, params object[] arguments)
	{
		MethodInfo method = inst.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
		if (!method.Equals(null))
		{
			return method.Invoke(inst, arguments);
		}
		return null;
	}

	internal static void SetPrivObj(object inst, string name, object value, Type type)
	{
		FieldInfo field = inst.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
		if (!field.Equals(null) && field.FieldType.Equals(type))
		{
			field.SetValue(inst, value);
		}
	}

	internal static void SetPrivFloat(object inst, string name, float value)
	{
		SetPrivObj(inst, name, value, typeof(float));
	}

	internal static List<GameObject> GetRootObjects()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			Scene sceneAt = SceneManager.GetSceneAt(i);
			GameObject[] array = Il2CppArrayBase<GameObject>.op_Implicit((Il2CppArrayBase<GameObject>)(object)((Scene)(ref sceneAt)).GetRootGameObjects());
			GameObject[] array2 = array;
			foreach (GameObject item in array2)
			{
				list.Add(item);
			}
		}
		return list;
	}

	internal static void GetChildrenWithName(GameObject obj, string name, List<GameObject> result)
	{
		if (obj.transform.childCount <= 0)
		{
			return;
		}
		for (int i = 0; i < obj.transform.childCount; i++)
		{
			GameObject gameObject = ((Component)obj.transform.GetChild(i)).gameObject;
			if (((Object)gameObject).name.ToLower().Contains(name))
			{
				result.Add(gameObject);
			}
			GetChildrenWithName(gameObject, name, result);
		}
	}

	public static GameObject InstantiateSwitch(Vector3 pos, Vector3 rot, int variant)
	{
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = (GameObject)(variant switch
		{
			1 => Object.Instantiate<GameObject>(HouseLights.HLbundle.LoadAsset<GameObject>("OBJ_SwitchHLB")), 
			2 => Object.Instantiate<GameObject>(HouseLights.HLbundle.LoadAsset<GameObject>("OBJ_SwitchHLC")), 
			3 => Object.Instantiate<GameObject>(HouseLights.HLbundle.LoadAsset<GameObject>("OBJ_SwitchHLD")), 
			_ => Object.Instantiate<GameObject>(HouseLights.HLbundle.LoadAsset<GameObject>("OBJ_SwitchHL")), 
		});
		MeshRenderer[] array = Il2CppArrayBase<MeshRenderer>.op_Implicit(val.GetComponentsInChildren<MeshRenderer>());
		MeshRenderer[] array2 = array;
		foreach (MeshRenderer val2 in array2)
		{
			Texture mainTexture = ((Renderer)val2).material.mainTexture;
			Material val3 = new Material(vanillaShader);
			val3.mainTexture = mainTexture;
			((Renderer)val2).material = val3;
		}
		val.transform.eulerAngles = rot;
		val.transform.position = pos;
		val.transform.localScale = new Vector3(1f, 1f, 1f);
		if (Settings.options.Debug)
		{
			MelonLogger.Msg("Instantiate new House Lights switch.");
		}
		return val;
	}

	internal static bool IsMenu()
	{
		if (GameManager.m_ActiveScene != null && (GameManager.m_ActiveScene.Contains("MainMenu") || InterfaceManager.IsMainMenuEnabled()))
		{
			return true;
		}
		return false;
	}

	public static AssetBundle LoadFromStream(string name)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
		MemoryStream memoryStream = new MemoryStream((int)stream.Length);
		stream.CopyTo(memoryStream);
		MemoryStream val = new MemoryStream(Il2CppStructArray<byte>.op_Implicit(memoryStream.ToArray()));
		return AssetBundle.LoadFromStream((Stream)(object)val);
	}
}
