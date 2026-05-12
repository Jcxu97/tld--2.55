using System.Collections.Generic;
using HarmonyLib;
using Il2Cpp;
using Il2CppSystem.Collections.Generic;
using Il2CppTLD.Gameplay;
using UnityEngine;

namespace MotionTracker;

[HarmonyPatch(typeof(BeachcombingSpawner), "Update")]
public class BeachcombingSpawnerUpdatePatch
{
	private static float timer = 0f;

	private static float triggerTime = 5f;

	private static bool doOnce = false;

	public static void Postfix(ref BeachcombingSpawner __instance)
	{
		timer += Time.deltaTime;
		if (!(timer > triggerTime))
		{
			return;
		}
		if (doOnce)
		{
			MyLogger.LogMessage("BeachcombingSpawner Update event: Begin root objects identification.", 152, "Postfix", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Harmony.cs");
			foreach (GameObject rootObject in SpawnUtils.GetRootObjects())
			{
				List<GameObject> result = new List<GameObject>();
				SpawnUtils.GetChildren(rootObject, result);
			}
			MyLogger.LogMessage("BeachcombingSpawner Update event: End root objects identification.", 163, "Postfix", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Harmony.cs");
			doOnce = false;
		}
		if (((Object)__instance).name.Contains("Tide"))
		{
			int num = 0;
			Enumerator<RadialObjectSpawner> enumerator2 = __instance.m_ChildSpawners.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				RadialObjectSpawner current2 = enumerator2.Current;
				int num2 = 0;
				Enumerator<GameObject> enumerator3 = current2.m_Spawns.GetEnumerator();
				while (enumerator3.MoveNext())
				{
					GameObject current3 = enumerator3.Current;
					if (!Object.op_Implicit((Object)(object)current3.gameObject.GetComponent<PingComponent>()))
					{
						current3.gameObject.AddComponent<PingComponent>().Initialize(PingManager.AnimalType.BeachLoot);
					}
					num2++;
				}
				num++;
			}
		}
		timer = 0f;
	}
}
