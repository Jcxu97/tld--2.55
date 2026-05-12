using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace NorthernLightsBroadcast;

[HarmonyPatch(typeof(PlayerManager), "AddItemToPlayerInventory")]
public class tvTurnOffOnStow
{
	public static void Prefix(ref PlayerManager __instance, ref GearItem gi, ref bool trackItemLooted, ref bool enableNotificationFlag)
	{
		if (((Object)gi).name.Contains("GEAR_TV_LCD") || ((Object)gi).name.Contains("GEAR_TV_CRT") || ((Object)gi).name.Contains("GEAR_TV_WALL"))
		{
			TVManager component = ((Component)gi).gameObject.GetComponent<TVManager>();
			if (component.currentState != 0)
			{
				component.SavePlaytime();
				component.SwitchState(TVManager.TVState.Off);
				SaveLoad.SetState(component.thisGuid, component.currentState);
			}
		}
	}
}
