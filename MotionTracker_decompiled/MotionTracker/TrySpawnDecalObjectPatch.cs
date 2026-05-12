using System;
using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace MotionTracker;

[HarmonyPatch(typeof(DynamicDecalsManager), "TrySpawnDecalObject", new Type[] { typeof(DecalProjectorInstance) })]
public class TrySpawnDecalObjectPatch
{
	public static void Postfix(ref DynamicDecalsManager __instance, ref DecalProjectorInstance decalInstance)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if ((int)decalInstance.m_DecalProjectorType == 7)
		{
			Vector3 position = default(Vector3);
			Quaternion rotation = default(Quaternion);
			Vector3 val = default(Vector3);
			__instance.CalculateDecalTransform(decalInstance, (DecalProjectorMaskData)null, ref position, ref rotation, ref val);
			GameObject val2 = new GameObject("DecalContainer");
			val2.transform.position = position;
			val2.transform.rotation = rotation;
			val2.AddComponent<PingComponent>().Initialize(decalInstance.m_ProjectileType);
		}
	}
}
