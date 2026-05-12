using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ModComponent.Utils;

internal class JsonDictEntry : Dictionary<string, object>
{
	internal string? GetString(string key, string _default = null)
	{
		if (ContainsKey(key))
		{
			return base[key].ToString();
		}
		return _default;
	}

	internal float GetFloat(string key, float _default = 0f)
	{
		if (ContainsKey(key) && float.TryParse(GetString(key), out var result))
		{
			return result;
		}
		return _default;
	}

	internal int GetInt(string key, int _default = 0)
	{
		if (ContainsKey(key))
		{
			return int.Parse(GetString(key), CultureInfo.InvariantCulture);
		}
		return _default;
	}

	internal T GetEnum<T>(string key) where T : Enum
	{
		return EnumUtils.ParseEnum<T>(GetString(key));
	}

	internal bool GetBool(string key, bool _default = false)
	{
		if (ContainsKey(key))
		{
			return bool.Parse(base[key].ToString());
		}
		return _default;
	}

	internal Vector3 GetVector3(string key, Vector3 _default = default(Vector3))
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (ContainsKey(key))
		{
			object obj = base[key];
			float[] array = ((IEnumerable<JToken>)((obj is JArray) ? obj : null)).Select((JToken t) => t.ToObject<float>()).ToArray();
			if (array.Length == 3)
			{
				return new Vector3(array[0], array[1], array[2]);
			}
		}
		return _default;
	}

	internal T[] GetArray<T>(string key)
	{
		if (ContainsKey(key))
		{
			object obj = base[key];
			return ((IEnumerable<JToken>)((obj is JArray) ? obj : null)).Select((JToken t) => t.ToObject<T>()).ToArray();
		}
		return Array.Empty<T>();
	}
}
