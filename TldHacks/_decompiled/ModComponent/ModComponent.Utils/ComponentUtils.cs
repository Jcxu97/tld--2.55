using System.Diagnostics.CodeAnalysis;
using ModComponent.API.Components;
using UnityEngine;

namespace ModComponent.Utils;

internal static class ComponentUtils
{
	[return: NotNullIfNotNull("component")]
	public static T? GetComponentSafe<T>(this Component? component) where T : Component
	{
		if (!((Object)(object)component == (Object)null))
		{
			return component.GetGameObject().GetComponentSafe<T>();
		}
		return default(T);
	}

	[return: NotNullIfNotNull("gameObject")]
	public static T? GetComponentSafe<T>(this GameObject? gameObject) where T : Component
	{
		if (!((Object)(object)gameObject == (Object)null))
		{
			return gameObject.GetComponent<T>();
		}
		return default(T);
	}

	[return: NotNullIfNotNull("component")]
	public static T? GetOrCreateComponent<T>(this Component? component) where T : Component
	{
		if (!((Object)(object)component == (Object)null))
		{
			return component.GetGameObject().GetOrCreateComponent<T>();
		}
		return default(T);
	}

	[return: NotNullIfNotNull("gameObject")]
	public static T? GetOrCreateComponent<T>(this GameObject? gameObject) where T : Component
	{
		if ((Object)(object)gameObject == (Object)null)
		{
			return default(T);
		}
		T val = gameObject.GetComponentSafe<T>();
		if ((Object)(object)val == (Object)null)
		{
			val = gameObject.AddComponent<T>();
		}
		return val;
	}

	[return: NotNullIfNotNull("component")]
	internal static ModBaseEquippableComponent? GetEquippableModComponent(this Component? component)
	{
		return component.GetComponentSafe<ModBaseEquippableComponent>();
	}

	[return: NotNullIfNotNull("gameObject")]
	internal static ModBaseEquippableComponent? GetEquippableModComponent(this GameObject? gameObject)
	{
		return gameObject.GetComponentSafe<ModBaseEquippableComponent>();
	}

	[return: NotNullIfNotNull("component")]
	internal static ModBaseComponent? GetModComponent(this Component? component)
	{
		return component.GetComponentSafe<ModBaseComponent>();
	}

	[return: NotNullIfNotNull("gameObject")]
	internal static ModBaseComponent? GetModComponent(this GameObject? gameObject)
	{
		return gameObject.GetComponentSafe<ModBaseComponent>();
	}

	[return: NotNullIfNotNull("component")]
	internal static GameObject? GetGameObject(this Component? component)
	{
		try
		{
			return ((Object)(object)component == (Object)null) ? null : component.gameObject;
		}
		catch
		{
		}
		return null;
	}
}
