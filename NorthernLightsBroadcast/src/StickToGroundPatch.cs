using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace NorthernLightsBroadcast;

[HarmonyPatch(typeof(StickToGround), "Awake")]
public class StickToGroundPatch
{
	public static void Postfix(StickToGround __instance)
	{
		if (!((Object)((Component)__instance).gameObject).name.Contains("OBJ_TelevisionB_LOD0") && !((Object)((Component)__instance).gameObject).name.Contains("OBJ_Television_LOD0"))
		{
			return;
		}
		GameObject gameObject = ((Component)__instance).gameObject;
		SaveLoad.LoadTheTVs();
		if ((Object)(object)gameObject != (Object)null && (Object)(object)gameObject.GetComponent<TVManager>() == (Object)null)
		{
			gameObject.AddComponent<TVManager>();
			if (((Object)((Component)__instance).gameObject).name.Contains("OBJ_TelevisionB_LOD0"))
			{
				((Renderer)gameObject.GetComponent<MeshRenderer>()).sharedMaterial = NorthernLightsBroadcastMain.TelevisionB_Material_Cutout;
			}
		}
	}
}
