using System;
using System.Collections.Generic;
using HarmonyLib;
using Il2CppTLD.Gear;

namespace CraftingRevisions;

[HarmonyPatch]
public static class BlueprintManager
{
	private static HashSet<string> jsonUserBlueprints = new HashSet<string>();

	public static void AddBlueprintFromJson(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			throw new ArgumentException("Blueprint text contains no information", "text");
		}
		jsonUserBlueprints.Add(text);
	}

	internal static void ValidateJsonBlueprint(string json)
	{
		ModUserBlueprintData.ParseFromJson(json).Validate();
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(BlueprintManager), "LoadAllUserBlueprints")]
	private static void BlueprintManager_LoadAllUserBlueprints_Postfix(BlueprintManager __instance)
	{
		foreach (string jsonUserBlueprint in jsonUserBlueprints)
		{
			try
			{
				ModUserBlueprintData modUserBlueprintData = ModUserBlueprintData.ParseFromJson(jsonUserBlueprint);
				try
				{
					if (modUserBlueprintData.Validate())
					{
						BlueprintData blueprintData = modUserBlueprintData.GetBlueprintData();
						__instance.m_AllBlueprints.Add(blueprintData);
					}
				}
				catch (Exception ex)
				{
					Logger.LogError("Blueprint Exception: " + modUserBlueprintData.Name + ex);
				}
			}
			catch (Exception ex2)
			{
				Logger.LogError("Blueprint Parse Exception:" + ex2);
			}
		}
	}
}
