using System;
using CraftingRevisions;
using GearSpawner;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using ModComponent.Utils;
using UnityEngine;

namespace ModComponent;

internal class Implementation : MelonMod
{
	[HarmonyPatch(typeof(GameManager), "Awake")]
	internal static class MC_Initialize
	{
		internal static void Prefix()
		{
			if (runDeps)
			{
				try
				{
					DependencyChecker.RunChecks();
				}
				catch (Exception value)
				{
					Logger.LogWarning($"DependencyChecker Failed:\n{value}");
				}
				runDeps = false;
			}
		}
	}

	[HarmonyPatch(typeof(Panel_Crafting), "Initialize")]
	internal static class MC_MapPrefabs
	{
		internal static void Prefix()
		{
			Logger.LogNotDebug("Panel_Crafting_Initialize");
			if (!isReady)
			{
				AssetBundleProcessor.MapPrefabs();
				isReady = true;
			}
		}
	}

	[HarmonyPatch(typeof(GearItem), "SetDamageBlendValue")]
	internal static class gisetdmg
	{
		internal static void Prefix(GearItem __instance, float blendVal, ref bool __runOriginal)
		{
			if ((Object)(object)((Component?)(object)__instance).GetModComponent() != (Object)null)
			{
				__runOriginal = false;
			}
		}
	}

	internal static bool runDeps = true;

	internal static bool isReady = false;

	internal static Implementation instance;

	public Implementation()
	{
		instance = this;
	}

	public override void OnInitializeMelon()
	{
		Logger.LogDebug("Debug Compilation");
		Logger.LogNotDebug("Release Compilation");
		GearSpawner.Settings.instance.AddToModSettings("Gear Spawn Settings");
		CraftingRevisions.Settings.instance.AddToModSettings("Crafting Revisions");
		Settings.instance.AddToModSettings("ModComponent");
		AssetBundleProcessor.Initialize();
	}

	public override void OnApplicationQuit()
	{
		AssetBundleProcessor.CleanupTempFolder();
	}
}
