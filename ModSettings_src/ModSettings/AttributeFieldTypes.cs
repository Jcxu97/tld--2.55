using System;
using System.Collections.Generic;
using HarmonyLib;

namespace ModSettings;

internal static class AttributeFieldTypes
{
	private static readonly HashSet<Type> integerTypes = new HashSet<Type>
	{
		typeof(byte),
		typeof(sbyte),
		typeof(short),
		typeof(ushort),
		typeof(int),
		typeof(uint),
		typeof(long)
	};

	private static readonly HashSet<Type> floatTypes = new HashSet<Type>
	{
		typeof(float),
		typeof(double),
		typeof(decimal)
	};

	private static readonly ICollection<Type> choiceTypes = new HashSet<Type>(integerTypes) { typeof(bool) };

	private static readonly ICollection<Type> sliderTypes = Union(integerTypes, floatTypes);

	private static readonly ICollection<Type> supportedTypes = Union(choiceTypes, sliderTypes);

	private static ICollection<Type> Union(ICollection<Type> left, ICollection<Type> right)
	{
		HashSet<Type> hashSet = new HashSet<Type>(left);
		hashSet.UnionWith(right);
		return hashSet;
	}

	internal static bool IsIntegerType(Type type)
	{
		return integerTypes.Contains(type);
	}

	internal static bool IsFloatType(Type type)
	{
		return floatTypes.Contains(type);
	}

	internal static bool IsChoiceType(Type type)
	{
		return choiceTypes.Contains(type);
	}

	internal static bool IsSliderType(Type type)
	{
		return sliderTypes.Contains(type);
	}

	internal static bool IsSupportedType(Type type)
	{
		if (!type.IsEnum)
		{
			return supportedTypes.Contains(type);
		}
		return true;
	}

	internal static long MaxValue(Type numericType)
	{
		if (numericType == typeof(bool))
		{
			return 1L;
		}
		return Convert.ToInt64(AccessTools.Field(numericType, "MaxValue").GetValue(null));
	}

	internal static long MinValue(Type numericType)
	{
		if (numericType == typeof(bool))
		{
			return 0L;
		}
		return Convert.ToInt64(AccessTools.Field(numericType, "MinValue").GetValue(null));
	}
}
