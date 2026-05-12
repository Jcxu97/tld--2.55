using HarmonyLib;
using Il2Cpp;
using Il2CppSystem.Collections.Generic;
using UnityEngine;

namespace MotionTracker;

[HarmonyPatch(typeof(RadialObjectSpawner), "ReleaseSpawnedObjectsToPool")]
public class RadialObjectSpawnerReleaseSpawnedObjectsToPoolPatch
{
	private static float timer;

	private static float triggerTime;

	public static void Prefix(ref RadialObjectSpawner __instance)
	{
		timer += Time.deltaTime;
		if (!(timer > triggerTime))
		{
			return;
		}
		int num = 0;
		Enumerator<GameObject> enumerator = __instance.m_Spawns.GetEnumerator();
		while (enumerator.MoveNext())
		{
			GameObject current = enumerator.Current;
			if (Object.op_Implicit((Object)(object)current.gameObject.GetComponent<PingComponent>()))
			{
				PingComponent.ManualDelete(current.gameObject.GetComponent<PingComponent>());
			}
			num++;
		}
		timer = 0f;
	}
}
