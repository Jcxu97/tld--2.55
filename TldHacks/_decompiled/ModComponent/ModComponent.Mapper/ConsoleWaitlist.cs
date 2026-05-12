using System.Collections.Generic;
using HarmonyLib;
using Il2Cpp;
using Il2CppSystem.Collections.Generic;
using Il2CppTLD.Gear;

namespace ModComponent.Mapper;

internal static class ConsoleWaitlist
{
	[HarmonyPatch(typeof(ConsoleManager), "Initialize")]
	internal static class UpdateConsoleCommands
	{
		private static void Postfix()
		{
			TryUpdateWaitlist();
		}
	}

	internal readonly struct ModConsoleName
	{
		public readonly string displayName;

		public readonly string prefabName;

		public readonly GearType gearType;

		public ModConsoleName(string displayName, string prefabName, GearType gearType)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			this.displayName = displayName;
			this.prefabName = prefabName;
			this.gearType = gearType;
		}
	}

	private static List<ModConsoleName> commandWaitlist = new List<ModConsoleName>(0);

	public static void AddToWaitlist(string displayName, string prefabName, GearType gearType)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		commandWaitlist.Add(new ModConsoleName(displayName, prefabName, gearType));
	}

	public static bool IsConsoleManagerInitialized()
	{
		return ConsoleManager.m_Initialized;
	}

	public static void TryUpdateWaitlist()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (commandWaitlist.Count <= 0 || !IsConsoleManagerInitialized())
		{
			return;
		}
		foreach (ModConsoleName item in commandWaitlist)
		{
			RegisterConsoleGearName(item.displayName, item.prefabName, item.gearType);
		}
		commandWaitlist.Clear();
		Logger.Log("Console Commands added. The waitlist is empty.");
	}

	internal static void MaybeRegisterConsoleGearName(string displayName, string prefabName, GearType gearType)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (IsConsoleManagerInitialized())
		{
			RegisterConsoleGearName(displayName, prefabName, gearType);
		}
		else
		{
			AddToWaitlist(displayName, prefabName, gearType);
		}
	}

	private static void RegisterConsoleGearName(string displayName, string prefabName, GearType gearType)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Invalid comparison between Unknown and I4
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Invalid comparison between Unknown and I4
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Invalid comparison between Unknown and I4
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Invalid comparison between Unknown and I4
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Invalid comparison between Unknown and I4
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Invalid comparison between Unknown and I4
		ConsoleManager.m_SearchStringToGearNames.Add(displayName.ToLower(), prefabName);
		ConsoleManager.m_SearchStringToGearNames.Add(prefabName.ToLower(), prefabName);
		string text = "ModComponent";
		if ((int)gearType <= 2)
		{
			if ((int)gearType != 1)
			{
				if ((int)gearType == 2)
				{
					text = "ClothingItem";
				}
			}
			else
			{
				text = "FoodItem";
			}
		}
		else if ((int)gearType != 8)
		{
			if ((int)gearType != 16)
			{
				if ((int)gearType == 32)
				{
					text = "FireStarterItem";
				}
			}
			else
			{
				text = "FirstAidItem";
			}
		}
		else
		{
			text = "ToolsItem";
		}
		List<string> val = new List<string>();
		if (!ConsoleManager.m_ComponentNameToGearNames.ContainsKey(text))
		{
			ConsoleManager.m_ComponentNameToGearNames.Add(text, val);
		}
		ConsoleManager.m_ComponentNameToGearNames[text].Add(prefabName);
		Enumerator<uConsoleCommandParameterSet> enumerator = uConsoleAutoComplete.m_CommandParameterSets.GetEnumerator();
		while (enumerator.MoveNext())
		{
			uConsoleCommandParameterSet current = enumerator.Current;
			if (current.m_Commands.Contains("add") && current.m_Commands.Contains("gear_add"))
			{
				current.m_AllowedParameters.Add(displayName.ToLower());
			}
		}
	}
}
