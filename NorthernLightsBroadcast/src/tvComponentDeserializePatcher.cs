using System.IO;
using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.PDID;
using UnityEngine;

namespace NorthernLightsBroadcast;

[HarmonyPatch(typeof(GearItem), "Deserialize")]
public class tvComponentDeserializePatcher
{
	public static void Postfix(ref GearItem __instance)
	{
		if (!((Object)__instance).name.Contains("GEAR_TV_LCD") && !((Object)__instance).name.Contains("GEAR_TV_CRT") && !((Object)__instance).name.Contains("GEAR_TV_WALL"))
		{
			return;
		}
		TVManager component = ((Component)__instance).gameObject.GetComponent<TVManager>();
		if (!((Object)(object)component != (Object)null))
		{
			return;
		}
		ObjectGuid component2 = ((Component)__instance).gameObject.GetComponent<ObjectGuid>();
		if ((Object)(object)component2 == (Object)null)
		{
			component.objectGuid = ((Component)component).gameObject.AddComponent<ObjectGuid>();
			component.thisGuid = ((PdidObjectBase)component.objectGuid).GetPDID();
			if (component.thisGuid == null)
			{
				component.objectGuid.MaybeRuntimeRegister();
				component.thisGuid = ((PdidObjectBase)component.objectGuid).GetPDID();
			}
		}
		else
		{
			component.thisGuid = ((PdidObjectBase)component2).GetPDID();
		}
		if (SaveLoad.GetState(component.thisGuid) == TVManager.TVState.Playing)
		{
			string folder = SaveLoad.GetFolder(component.thisGuid);
			if (folder != null && Directory.Exists(folder))
			{
				component.ui.currentFolder = folder;
			}
			string lastPlayed = SaveLoad.GetLastPlayed(component.thisGuid);
			if (lastPlayed != null && File.Exists(lastPlayed))
			{
				component.ui.currentClip = lastPlayed;
			}
			component.ui.Prepare();
		}
		else
		{
			component.SwitchState(SaveLoad.GetState(component.thisGuid));
		}
	}
}
