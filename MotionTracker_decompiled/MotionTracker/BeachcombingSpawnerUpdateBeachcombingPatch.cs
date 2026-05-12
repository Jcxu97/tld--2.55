using HarmonyLib;
using Il2CppTLD.Gameplay;
using UnityEngine;

namespace MotionTracker;

[HarmonyPatch(typeof(BeachcombingSpawner), "UpdateBeachcombing")]
public class BeachcombingSpawnerUpdateBeachcombingPatch
{
	public static void Postfix(ref BeachcombingSpawner __instance)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		MyLogger.LogMessage("!!BeachcombingSpawner UpdateBeachcombing event: (" + ((Object)__instance).name + ":" + ((Object)__instance).GetInstanceID() + ") at [" + ((Component)__instance).transform.position.x + "," + ((Component)__instance).transform.position.y + "," + ((Component)__instance).transform.position.z + "].", 223, "Postfix", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Harmony.cs");
	}
}
