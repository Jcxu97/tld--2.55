using System.Collections.Generic;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace ModComponent.AssetLoader;

internal static class AtlasManager
{
	private static Dictionary<string, UIAtlas> knownSpriteAtlases = new Dictionary<string, UIAtlas>();

	public static void LoadUiAtlas(Object asset)
	{
		GameObject val = ((Il2CppObjectBase)asset).TryCast<GameObject>();
		if ((Object)(object)val == (Object)null)
		{
			Logger.Log("Asset called '" + asset.name + "' is not a GameObject as expected.");
			return;
		}
		UIAtlas component = val.GetComponent<UIAtlas>();
		if ((Object)(object)component == (Object)null)
		{
			Logger.Log("Asset called '" + asset.name + "' does not contain a UIAtlast as expected.");
			return;
		}
		Logger.Log("Processing asset '" + asset.name + "' as UIAtlas.");
		string[] array = Il2CppArrayBase<string>.op_Implicit(component.GetListOfSprites().ToArray());
		foreach (string text in array)
		{
			if (knownSpriteAtlases.ContainsKey(text))
			{
				Logger.Log($"Replacing definition of sprite '{text}' from atlas '{((Object)knownSpriteAtlases[text]).name}' to '{((Object)component).name}'.");
				knownSpriteAtlases[text] = component;
			}
			else
			{
				knownSpriteAtlases.Add(text, component);
			}
		}
	}

	internal static UIAtlas GetSpriteAtlas(string spriteName)
	{
		knownSpriteAtlases.TryGetValue(spriteName, out UIAtlas value);
		return value;
	}
}
