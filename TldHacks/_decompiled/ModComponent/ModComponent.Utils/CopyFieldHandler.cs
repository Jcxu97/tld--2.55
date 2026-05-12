using System.Reflection;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem;
using Il2CppSystem.Reflection;
using UnityEngine;

namespace ModComponent.Utils;

internal static class CopyFieldHandler
{
	public static void UpdateFieldValues<T>(T componentToUpdate) where T : Component
	{
		GameObject val = AssetBundleUtils.LoadAsset<GameObject>(NameUtils.NormalizeName(((Object)(object)componentToUpdate).name));
		if ((Object)(object)val == (Object)null)
		{
			Logger.Log("While copying fields for '{0}', the prefab was null.");
			return;
		}
		T component = val.GetComponent<T>();
		if ((Object)(object)component != (Object)null)
		{
			CopyFieldsMono(componentToUpdate, component);
		}
	}

	internal static void CopyFieldsMono<T>(T copyTo, T copyFrom)
	{
		FieldInfo[] fields = typeof(T).GetFields();
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			if (!fieldInfo.IsInitOnly && !fieldInfo.IsLiteral)
			{
				fieldInfo.SetValue(copyTo, fieldInfo.GetValue(copyFrom));
			}
		}
		if (fields.Length == 0)
		{
			Logger.LogWarning("There were no fields to copy!");
		}
	}

	internal static void CopyFieldsIl2Cpp<T>(T copyTo, T copyFrom) where T : Object
	{
		FieldInfo[] array = Il2CppArrayBase<FieldInfo>.op_Implicit((Il2CppArrayBase<FieldInfo>)(object)Il2CppType.Of<T>().GetFields());
		FieldInfo[] array2 = array;
		foreach (FieldInfo val in array2)
		{
			if (!val.IsInitOnly && !val.IsLiteral)
			{
				val.SetValue((Object)(object)copyTo, val.GetValue((Object)(object)copyFrom));
			}
		}
		if (array.Length == 0)
		{
			Logger.LogWarning("There were no fields to copy!");
		}
	}
}
