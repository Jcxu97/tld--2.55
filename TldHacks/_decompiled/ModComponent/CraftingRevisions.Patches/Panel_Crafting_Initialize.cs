using HarmonyLib;
using Il2Cpp;

namespace CraftingRevisions.Patches;

[HarmonyPatch(typeof(Panel_Crafting), "Initialize")]
internal class Panel_Crafting_Initialize
{
	private static void Postfix()
	{
		InterfaceManager.m_Instance.m_BlueprintManager.LoadAllUserBlueprints();
	}
}
