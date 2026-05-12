using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Collections.Generic;
using Il2CppTLD.Gameplay;
using UnityEngine;

namespace MotionTracker;

[HarmonyPatch(typeof(BeachcombingSpawner), "UpdateBigItems")]
public class BeachcombingSpawnerUpdateBigItemsPatch
{
	private static float timer = 0f;

	private static float triggerTime = 5f;

	public static void Postfix(ref BeachcombingSpawner __instance)
	{
		timer += Time.deltaTime;
		if (!(timer > triggerTime))
		{
			return;
		}
		int num = 0;
		Enumerator<BeachcombingBigItemLocation> enumerator = __instance.m_BigItemLocations.GetEnumerator();
		while (enumerator.MoveNext())
		{
			BeachcombingBigItemLocation current = enumerator.Current;
			GearItem[] array = Il2CppArrayBase<GearItem>.op_Implicit(((Component)current).GetComponentsInChildren<GearItem>());
			int num2 = 0;
			GearItem[] array2 = array;
			foreach (GearItem val in array2)
			{
				PingComponent component = ((Component)val).gameObject.GetComponent<PingComponent>();
				if (Object.op_Implicit((Object)(object)component))
				{
					if (component.animalType != PingManager.AnimalType.BeachLoot)
					{
						PingComponent.ManualDelete(component);
						((Component)val).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.BeachLoot);
					}
				}
				else if (((Component)val).gameObject.activeInHierarchy && ((Component)current).gameObject.activeInHierarchy)
				{
					((Component)val).gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.BeachLoot);
				}
				num2++;
			}
			num++;
		}
		timer = 0f;
	}
}
