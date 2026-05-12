using System;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppSystem;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ModComponent.Utils;

internal static class AssetBundleUtils
{
	public static Object LoadAsset(AssetBundle assetBundle, string name)
	{
		return LoadAsset(assetBundle, name, Il2CppType.Of<Object>());
	}

	public static T? LoadAsset<T>(AssetBundle assetBundle, string name) where T : Object
	{
		Object obj = LoadAsset(assetBundle, name, Il2CppType.Of<T>());
		if (obj == null)
		{
			return default(T);
		}
		return ((Il2CppObjectBase)obj).TryCast<T>();
	}

	public static Object LoadAsset(AssetBundle assetBundle, string name, Type type)
	{
		if ((Object)(object)assetBundle == (Object)null)
		{
			throw new NullReferenceException("The asset bundle cannot be null.");
		}
		if (name == null)
		{
			throw new NullReferenceException("The input asset name cannot be null.");
		}
		if (name.Length == 0)
		{
			throw new ArgumentException("The input asset name cannot be empty.");
		}
		if (type == (Type)null)
		{
			throw new NullReferenceException("The input type cannot be null.");
		}
		return assetBundle.LoadAsset_Internal(name, type);
	}

	public static T LoadAsset<T>(string assetName)
	{
		return Addressables.LoadAssetAsync<T>(Object.op_Implicit(assetName)).WaitForCompletion();
	}
}
