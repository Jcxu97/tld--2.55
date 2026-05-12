using System.Collections.Generic;
using Il2CppSystem.Collections.Generic;

namespace ModComponent.Utils;

internal static class ConversionUtils
{
	internal static List<T> Convert<T>(List<T> list)
	{
		List<T> list2 = new List<T>(list.Count);
		Enumerator<T> enumerator = list.GetEnumerator();
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			list2.Add(current);
		}
		return list2;
	}

	internal static List<T> Convert<T>(List<T> list)
	{
		List<T> val = new List<T>(list.Count);
		foreach (T item in list)
		{
			val.Add(item);
		}
		return val;
	}

	internal static List<T> Convert<T>(T[] array)
	{
		List<T> val = new List<T>(array.Length);
		foreach (T val2 in array)
		{
			val.Add(val2);
		}
		return val;
	}

	internal static List<T> Convert<T>(IEnumerable<T> enumerable)
	{
		List<T> obj = new List<T>(enumerable);
		List<T> list = new List<T>(obj.Count);
		Enumerator<T> enumerator = obj.GetEnumerator();
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			list.Add(current);
		}
		return list;
	}

	internal static T[] ToArray<T>(List<T> list)
	{
		return list.ToArray();
	}

	internal static T[] ToArray<T>(List<T> list)
	{
		T[] array = new T[list.Count];
		for (int i = 0; i < list.Count; i++)
		{
			array[i] = list[i];
		}
		return array;
	}

	internal static T[] ToArray<T>(IEnumerable<T> enumerable)
	{
		return ToArray<T>(new List<T>(enumerable));
	}
}
