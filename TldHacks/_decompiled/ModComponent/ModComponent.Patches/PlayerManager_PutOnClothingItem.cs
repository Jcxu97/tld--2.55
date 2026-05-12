using HarmonyLib;
using Il2Cpp;
using ModComponent.API.Components;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Patches;

[HarmonyPatch(typeof(PlayerManager), "PutOnClothingItem")]
internal static class PlayerManager_PutOnClothingItem
{
	private static void Prefix(PlayerManager __instance, GearItem gi, ClothingLayer layerToPutOn)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)((gi != null) ? gi.m_ClothingItem : null) == (Object)null) && (int)layerToPutOn != 4)
		{
			ClothingRegion region = gi.m_ClothingItem.m_Region;
			GearItem clothingInSlot = __instance.GetClothingInSlot(region, layerToPutOn);
			if (Object.op_Implicit((Object)(object)clothingInSlot))
			{
				__instance.TakeOffClothingItem(clothingInSlot);
			}
		}
	}

	private static void Postfix(GearItem gi)
	{
		((Component?)(object)gi).GetComponentSafe<ModClothingComponent>()?.OnPutOn?.Invoke();
	}
}
