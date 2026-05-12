using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Il2Cpp;
using Il2CppAK;
using Il2CppAK.Wwise;
using ModComponent.Mapper;
using UnityEngine;

namespace ModComponent.Utils;

internal static class ModUtils
{
	private static readonly Dictionary<string, uint> eventIds = new Dictionary<string, uint>();

	public static bool AlmostZero(float value)
	{
		return Mathf.Abs(value) < 0.001f;
	}

	public static void CopyFields<T>(T copyTo, T copyFrom)
	{
		FieldInfo[] fields = typeof(T).GetFields();
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			fieldInfo.SetValue(copyTo, fieldInfo.GetValue(copyFrom));
		}
		if (fields.Length == 0)
		{
			Logger.LogError("There were no fields to copy!");
		}
	}

	public static string DefaultIfEmpty(string value, string defaultValue)
	{
		if (!string.IsNullOrEmpty(value))
		{
			return value;
		}
		return defaultValue;
	}

	public static bool IsNonGameScene()
	{
		if (!string.IsNullOrEmpty(GameManager.m_ActiveScene) && !GameManager.m_ActiveScene.StartsWith("MainMenu") && !(GameManager.m_ActiveScene == "Boot"))
		{
			return GameManager.m_ActiveScene == "Empty";
		}
		return true;
	}

	public static T[] NotNull<T>(T[] array)
	{
		return array ?? Array.Empty<T>();
	}

	public static void PlayAudio(string audioName)
	{
		if (audioName != null)
		{
			GameAudioManager.PlaySound(audioName, InterfaceManager.GetSoundEmitter());
		}
	}

	internal static Delegate? CreateDelegate(Type delegateType, object target, string methodName)
	{
		MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (!(method == null))
		{
			return Delegate.CreateDelegate(delegateType, target, method);
		}
		return null;
	}

	public static GameObject? GetChild(this GameObject gameObject, string childName)
	{
		if (!string.IsNullOrEmpty(childName) && !((Object)(object)gameObject == (Object)null) && !((Object)(object)gameObject.transform == (Object)null))
		{
			return ((Component?)(object)gameObject.transform.FindChild(childName)).GetGameObject();
		}
		return null;
	}

	public static GameObject? GetParent(this GameObject gameObject)
	{
		if (!((Object)(object)gameObject == (Object)null) && !((Object)(object)gameObject.transform == (Object)null) && !((Object)(object)gameObject.transform.parent == (Object)null))
		{
			return ((Component)gameObject.transform.parent).gameObject;
		}
		return null;
	}

	public static GameObject? GetSibling(this GameObject gameObject, string siblingName)
	{
		GameObject parent = gameObject.GetParent();
		if (!((Object)(object)parent == (Object)null))
		{
			return parent.GetChild(siblingName);
		}
		return null;
	}

	public static GameObject? GetInChildren(GameObject parent, string childName)
	{
		if (string.IsNullOrEmpty(childName))
		{
			return null;
		}
		Transform transform = parent.transform;
		for (int i = 0; i < transform.childCount; i++)
		{
			GameObject gameObject = ((Component)transform.GetChild(i)).gameObject;
			if (((Object)gameObject).name == childName)
			{
				return gameObject;
			}
			if (gameObject.transform.childCount > 0)
			{
				GameObject inChildren = GetInChildren(gameObject, childName);
				if ((Object)(object)inChildren != (Object)null)
				{
					return inChildren;
				}
			}
		}
		return null;
	}

	internal static T GetItem<T>(string name, string? reference = null) where T : Component
	{
		GameObject val = AssetBundleUtils.LoadAsset<GameObject>(name);
		if ((Object)(object)val == (Object)null)
		{
			throw new ArgumentException("Could not load '" + name + "'" + ((reference != null) ? (" referenced by '" + reference + "'") : "") + ".");
		}
		bool num = AssetBundleProcessor.IsModComponentPrefab(name);
		bool flag = (Object)(object)val.GetComponentSafe<T>() != (Object)null;
		string prefabBundlePath = AssetBundleProcessor.GetPrefabBundlePath(name);
		if (num && !flag && prefabBundlePath != null)
		{
			Logger.LogDebug("Mapping dependency " + name);
			AutoMapper.AutoMapPrefab(Path.GetFileNameWithoutExtension(prefabBundlePath), name);
		}
		T componentSafe = val.GetComponentSafe<T>();
		if ((Object)(object)componentSafe == (Object)null)
		{
			throw new ArgumentException("'" + name + "'" + ((reference != null) ? (" referenced by '" + reference + "'") : "") + " is not a '" + typeof(T).Name + "'.");
		}
		return componentSafe;
	}

	internal static T[] GetItems<T>(string[] names, string? reference = null) where T : Component
	{
		T[] array = new T[names.Length];
		for (int i = 0; i < names.Length; i++)
		{
			array[i] = GetItem<T>(names[i], reference);
		}
		return array;
	}

	internal static T? GetMatchingItem<T>(string name, string? reference = null) where T : Component
	{
		try
		{
			return GetItem<T>(name, reference);
		}
		catch (ArgumentException ex)
		{
			Logger.LogError(ex.Message);
			return default(T);
		}
	}

	internal static T[] GetMatchingItems<T>(string[] names, string? reference = null) where T : Component
	{
		names = NotNull(names);
		List<T> list = new List<T>();
		for (int i = 0; i < names.Length; i++)
		{
			T matchingItem = GetMatchingItem<T>(names[i], reference);
			if ((Object)(object)matchingItem != (Object)null)
			{
				list.Add(matchingItem);
			}
		}
		return list.ToArray();
	}

	internal static Event? MakeAudioEvent(string? eventName)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Expected O, but got Unknown
		if (string.IsNullOrEmpty(eventName) || GetAKEventIdFromString(eventName) == 0)
		{
			Event val = new Event
			{
				WwiseObjectReference = ScriptableObject.CreateInstance<WwiseEventReference>()
			};
			((WwiseObjectReference)val.WwiseObjectReference).objectName = "NULL_WWISEEVENT";
			((WwiseObjectReference)val.WwiseObjectReference).id = GetAKEventIdFromString("NULL_WWISEEVENT");
			return val;
		}
		Event val2 = new Event
		{
			WwiseObjectReference = ScriptableObject.CreateInstance<WwiseEventReference>()
		};
		((WwiseObjectReference)val2.WwiseObjectReference).objectName = eventName;
		((WwiseObjectReference)val2.WwiseObjectReference).id = GetAKEventIdFromString(eventName);
		return val2;
	}

	private static uint GetAKEventIdFromString(string eventName)
	{
		if (eventIds.Count == 0)
		{
			PropertyInfo[] properties = typeof(EVENTS).GetProperties(BindingFlags.Static | BindingFlags.Public);
			foreach (PropertyInfo obj in properties)
			{
				string key = obj.Name.ToLowerInvariant();
				uint value = (uint)obj.GetValue(null);
				eventIds.Add(key, value);
			}
		}
		eventIds.TryGetValue(eventName.ToLowerInvariant(), out var value2);
		return value2;
	}
}
