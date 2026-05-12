using System;
using System.Collections.Generic;
using System.Reflection;
using MelonLoader;
using UnityEngine;

namespace Shotgun.Utility;

internal class NullChecks
{
	internal static Dictionary<Type, List<string>> TypeToPropertyNameDict = new Dictionary<Type, List<string>>();

	internal static void ReportNulls<T>(T t, string optionalDescription = "") where T : MonoBehaviour
	{
		Type type = ((object)t).GetType();
		if (!TypeToPropertyNameDict.TryGetValue(type, out List<string> value))
		{
			PropertyInfo[] properties = type.GetProperties();
			value = new List<string>();
			PropertyInfo[] array = properties;
			foreach (PropertyInfo propertyInfo in array)
			{
				value.Add(propertyInfo.Name);
			}
		}
		MelonLogger.Msg("");
		MelonLogger.Msg("");
		MelonLogger.Msg("");
		MelonLogger.Msg("NullCheck on " + ((UnityEngine.Object)(object)t).name);
		if (optionalDescription.Length > 0)
		{
			MelonLogger.Msg(optionalDescription);
		}
		MelonLogger.Msg("");
		foreach (string item in value)
		{
			t.ReportIfPropertyNull(item);
		}
		MelonLogger.Msg("");
	}
}
