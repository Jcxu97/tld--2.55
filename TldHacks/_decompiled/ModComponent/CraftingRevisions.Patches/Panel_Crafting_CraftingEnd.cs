using HarmonyLib;
using Il2Cpp;

namespace CraftingRevisions.Patches;

[HarmonyPatch(typeof(Panel_Crafting), "OnCraftingSuccess")]
internal static class Panel_Crafting_CraftingEnd
{
	private static void Prefix()
	{
		WatchHandleCraftingSuccess.isExecuting = true;
	}

	private static void Postfix()
	{
		WatchHandleCraftingSuccess.isExecuting = false;
	}
}
