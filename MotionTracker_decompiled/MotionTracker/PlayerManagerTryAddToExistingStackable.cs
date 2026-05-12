using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace MotionTracker;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
public class PlayerManagerTryAddToExistingStackable
{
	public static bool Prefix(GearItem gearToAdd, float normalizedCondition, int numUnits, GearItem existingGearItem)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		PingComponent component = ((Component)gearToAdd).gameObject.GetComponent<PingComponent>();
		if (Object.op_Implicit((Object)(object)component) && component.animalType == PingManager.AnimalType.BeachLoot)
		{
			string[] obj = new string[7]
			{
				"PlayerManager.TryAddToExistingStackable event: See Spawned Beach Loot (",
				((Object)gearToAdd).name,
				":",
				((Object)gearToAdd).GetInstanceID().ToString(),
				") at [",
				null,
				null
			};
			Vector3 position = ((Component)gearToAdd).transform.position;
			obj[5] = ((object)(Vector3)(ref position)).ToString();
			obj[6] = "] and existing BeachLoot PingComponent.";
			MyLogger.LogMessage(string.Concat(obj), 803, "Prefix", "D:\\Users\\okclm\\Documents\\My Coding\\The Long Dark\\Mods\\MotionTracker-1.2.0\\Harmony.cs");
			PingComponent.ManualDelete(component);
		}
		if (((Object)((Component)gearToAdd).gameObject).name.Contains("Arrow") || ((Object)((Component)gearToAdd).gameObject).name.Contains("Coal") || PingComponent.IsRawFish(gearToAdd))
		{
			if ((Object)(object)((Component)gearToAdd).gameObject.GetComponent<PingComponent>() != (Object)null)
			{
				PingComponent.ManualDelete(((Component)gearToAdd).gameObject.GetComponent<PingComponent>());
			}
			return true;
		}
		return true;
	}
}
