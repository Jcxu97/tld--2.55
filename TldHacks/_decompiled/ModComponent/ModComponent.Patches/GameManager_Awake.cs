using HarmonyLib;
using Il2Cpp;
using ModComponent.Mapper;

namespace ModComponent.Patches;

[HarmonyPatch(typeof(GameManager), "Awake")]
internal static class GameManager_Awake
{
	private static void Postfix()
	{
		AlternativeToolManager.ProcessList();
	}
}
