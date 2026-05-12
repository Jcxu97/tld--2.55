using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace MotionTracker;

[HarmonyPatch(typeof(GearItem), "ManualUpdate")]
public class GearItemManualUpdatePatch
{
	public static void Postfix(ref GearItem __instance)
	{
		if (!((Object)((Component)__instance).gameObject).name.Contains("Arrow") && !((Object)((Component)__instance).gameObject).name.Contains("Coal") && !PingComponent.IsRawFish(__instance))
		{
			return;
		}
		if (__instance.m_InsideContainer)
		{
			if (Object.op_Implicit((Object)(object)((Component)__instance).gameObject) && Object.op_Implicit((Object)(object)((Component)__instance).gameObject.GetComponent<PingComponent>()))
			{
				PingComponent.ManualDelete(((Component)__instance).gameObject.GetComponent<PingComponent>());
			}
		}
		else if (__instance.m_InPlayerInventory)
		{
			if (Object.op_Implicit((Object)(object)((Component)__instance).gameObject) && Object.op_Implicit((Object)(object)((Component)__instance).gameObject.GetComponent<PingComponent>()))
			{
				PingComponent.ManualDelete(((Component)__instance).gameObject.GetComponent<PingComponent>());
			}
		}
		else if (!Object.op_Implicit((Object)(object)((Component)__instance).gameObject.GetComponent<PingComponent>()))
		{
			if (((Object)((Component)__instance).gameObject).name.Contains("Arrow"))
			{
				((Component)__instance).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.Arrow);
			}
			else if (((Object)((Component)__instance).gameObject).name.Contains("Coal"))
			{
				((Component)__instance).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.Coal);
			}
			else if (PingComponent.IsRawFish(__instance))
			{
				((Component)__instance).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.RawFish);
			}
			((Component)__instance).gameObject.GetComponent<PingComponent>().attachedGearItem = __instance;
		}
	}
}
