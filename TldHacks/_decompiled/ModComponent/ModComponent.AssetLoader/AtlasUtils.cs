using Il2Cpp;
using UnityEngine;

namespace ModComponent.AssetLoader;

internal static class AtlasUtils
{
	internal static UIAtlas? GetRequiredAtlas(UISprite sprite, string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return sprite.atlas;
		}
		UIAtlas spriteAtlas = AtlasManager.GetSpriteAtlas(value);
		if ((Object)(object)spriteAtlas != (Object)null)
		{
			return spriteAtlas;
		}
		SaveAtlas component = ((Component)sprite).gameObject.GetComponent<SaveAtlas>();
		if ((Object)(object)component != (Object)null)
		{
			return component.original;
		}
		return sprite.atlas;
	}

	internal static void SaveOriginalAtlas(UISprite sprite)
	{
		if ((Object)(object)((Component)sprite).gameObject.GetComponent<SaveAtlas>() == (Object)null)
		{
			((Component)sprite).gameObject.AddComponent<SaveAtlas>().original = sprite.atlas;
		}
	}
}
