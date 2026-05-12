using HarmonyLib;
using Il2Cpp;

namespace NorthernLightsBroadcast;

[HarmonyPatch(typeof(SaveGameSystem), "SaveSceneData")]
public class SaveCandles
{
	public static void Postfix(ref SlotData slot)
	{
		SaveLoad.SaveTheTVs();
	}
}
