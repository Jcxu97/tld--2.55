using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace MotionTracker;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
public class RadialObjectSpawnerRemoveFromSpawnsPatch
{
	public static void Postfix(ref RadialObjectSpawner __instance, GameObject go)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		string[] obj = new string[11]
		{
			"RadialObjectSpawner RemoveFromSpawns event: Harvesting RadialObjectSpawner (",
			((Object)__instance).name,
			":",
			((Object)__instance).GetInstanceID().ToString(),
			" loot (",
			((Object)go).name,
			":",
			((Object)go).GetInstanceID().ToString(),
			") at [",
			null,
			null
		};
		Vector3 position = go.transform.position;
		obj[9] = ((object)(Vector3)(ref position)).ToString();
		obj[10] = "] during.";
		MyLogger.LogMessage(string.Concat(obj), 541, "Postfix", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Harmony.cs");
		if (Object.op_Implicit((Object)(object)go.gameObject.GetComponent<PingComponent>()))
		{
			MyLogger.LogMessage("RadialObjectSpawner RemoveFromSpawns event: Harvested object (" + ((Object)go).name + ":" + ((Object)go).GetInstanceID() + ") PingComponent exists for beach loot.  Delete PingComponent to remove from radar.", 547, "Postfix", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Harmony.cs");
			PingComponent.ManualDelete(go.gameObject.GetComponent<PingComponent>());
		}
	}
}
