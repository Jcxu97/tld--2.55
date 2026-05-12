using HarmonyLib;
using Il2Cpp;
using ModComponent.AssetLoader;
using UnityEngine;

namespace ModComponent.Patches;

[HarmonyPatch(typeof(UISprite), "SetAtlasSprite")]
internal static class UISprite_set_spriteName
{
	internal static void Postfix(UISprite __instance)
	{
		UIAtlas requiredAtlas = AtlasUtils.GetRequiredAtlas(__instance, __instance.mSpriteName);
		if (!((Object)(object)__instance.atlas == (Object)(object)requiredAtlas))
		{
			AtlasUtils.SaveOriginalAtlas(__instance);
			__instance.atlas = requiredAtlas;
		}
	}
}
