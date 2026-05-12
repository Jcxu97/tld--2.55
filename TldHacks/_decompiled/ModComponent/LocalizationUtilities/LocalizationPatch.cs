using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HarmonyLib;
using Il2Cpp;

namespace LocalizationUtilities;

[HarmonyPatch]
internal static class LocalizationPatch
{
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Localization), "LoadStringTableForLanguage")]
	private static void AddCustomLocalizationsToStringTable()
	{
		StringTable s_CurrentLanguageStringTable = Localization.s_CurrentLanguageStringTable;
		foreach (LocalizationSet localization in LocalizationManager.Localizations)
		{
			AddOrUpdate(s_CurrentLanguageStringTable, localization);
		}
	}

	private static void AddOrUpdate(StringTable stringTable, LocalizationSet set)
	{
		string[] languagesArray = stringTable.GetLanguagesArray();
		foreach (LocalizationEntry entry in set.Entries)
		{
			Entry orAddEntryFromKey = stringTable.GetOrAddEntryFromKey(entry.LocalizationID);
			for (int i = 0; i < languagesArray.Length; i++)
			{
				string text = languagesArray[i];
				string pattern = "\\[(.*?)\\]";
				Match match = Regex.Match(text, pattern);
				if (match.Success)
				{
					text = match.Groups[1].Value;
				}
				string value2;
				if (entry.Map.TryGetValue(text, out string value) && !string.IsNullOrWhiteSpace(value))
				{
					orAddEntryFromKey.m_Languages[i] = value;
				}
				else if (set.DefaultToEnglish && entry.Map.TryGetValue("English", out value2))
				{
					orAddEntryFromKey.m_Languages[i] = value2;
				}
			}
		}
	}

	private static string[] GetLanguagesArray(this StringTable stringTable)
	{
		return ((IEnumerable<string>)stringTable.GetLanguages().ToArray()).ToArray();
	}

	private static Entry GetOrAddEntryFromKey(this StringTable stringTable, string localizationID)
	{
		return stringTable.GetEntryFromKey(localizationID) ?? stringTable.AddEntryForKey(localizationID);
	}
}
