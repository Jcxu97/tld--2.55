using HarmonyLib;
using Il2Cpp;
using ModComponent.API.Components;
using ModComponent.Mapper;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Patches;

[HarmonyPatch(typeof(ClothingSlot), "CheckForChangeLayer")]
internal static class ClothingSlot_CheckForChangeLayer
{
	private static bool Prefix(ClothingSlot __instance)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		int defaultDrawLayer = DefaultDrawLayers.GetDefaultDrawLayer(__instance.m_ClothingRegion, __instance.m_ClothingLayer);
		ModClothingComponent componentSafe = ((Component?)(object)__instance.m_GearItem).GetComponentSafe<ModClothingComponent>();
		if ((Object)(object)componentSafe == (Object)null)
		{
			if ((Object)(object)__instance.m_GearItem != (Object)null)
			{
				__instance.UpdatePaperDollTextureLayer(defaultDrawLayer);
			}
			return true;
		}
		int num = ((componentSafe.DrawLayer > 0) ? componentSafe.DrawLayer : defaultDrawLayer);
		__instance.UpdatePaperDollTextureLayer(num);
		return false;
	}
}
