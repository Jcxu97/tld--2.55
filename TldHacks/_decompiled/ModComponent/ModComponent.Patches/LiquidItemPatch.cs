using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.Gear;
using Il2CppTLD.IntBackedUnit;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent.Patches;

internal class LiquidItemPatch
{
	[HarmonyPatch(typeof(LiquidItem), "Awake")]
	internal static class LiquidItem_Awake
	{
		private static void Postfix(LiquidItem __instance)
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			if ((Settings.instance.randomPlasticWaterBottles || (!(((Object)__instance).name == "GEAR_Water500ml") && !(((Object)__instance).name == "GEAR_Water1000ml"))) && (float)__instance.m_Minimum.m_Units == 0f && (Object)(object)__instance.m_LiquidType == (Object)(object)LiquidType.GetPotableWater())
			{
				__instance.m_Liquid = ItemLiquidVolume.FromLiters(RandomUtils.Range((float)__instance.GetCapacityLitres().m_Units / 8f, __instance.GetCapacityLitres().m_Units));
			}
		}
	}
}
