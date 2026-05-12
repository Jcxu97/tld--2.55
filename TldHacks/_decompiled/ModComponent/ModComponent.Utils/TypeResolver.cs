using System;
using System.Reflection;
using Il2CppInterop.Runtime;
using Il2CppSystem;
using UnityEngine;

namespace ModComponent.Utils;

internal class TypeResolver
{
	public static Type? Resolve(string name, bool throwErrorOnFailure)
	{
		Type type = Type.GetType(name, throwOnError: false);
		if (type != null)
		{
			return type;
		}
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		for (int i = 0; i < assemblies.Length; i++)
		{
			type = assemblies[i].GetType(name, throwOnError: false);
			if (type != null)
			{
				return type;
			}
		}
		if (throwErrorOnFailure)
		{
			throw new ArgumentException("Could not resolve type '" + name + "'. Are you missing an assembly?");
		}
		return null;
	}

	public static Type? ResolveIl2Cpp(string name, bool throwErrorOnFailure)
	{
		Type type = Type.GetType(name, throwOnError: false);
		if (type != null)
		{
			return Il2CppType.From(type, false);
		}
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		for (int i = 0; i < assemblies.Length; i++)
		{
			type = assemblies[i].GetType(name, throwOnError: false);
			if (type != null)
			{
				return Il2CppType.From(type, false);
			}
		}
		if (throwErrorOnFailure)
		{
			throw new ArgumentException("Could not resolve type '" + name + "'. Are you missing an assembly?");
		}
		return null;
	}

	public static bool InheritsFromMonobehaviour(Type type)
	{
		if (type == (Type)null)
		{
			return false;
		}
		return type.IsSubclassOf(Il2CppType.Of<MonoBehaviour>());
	}
}
