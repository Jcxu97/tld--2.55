using Il2Cpp;

namespace ModComponent.Utils;

internal static class NameUtils
{
	public static LocalizedString CreateLocalizedString(string key)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected O, but got Unknown
		return new LocalizedString
		{
			m_LocalizationID = key
		};
	}

	public static LocalizedString[] CreateLocalizedStrings(params string[] keys)
	{
		LocalizedString[] array = (LocalizedString[])(object)new LocalizedString[keys.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = CreateLocalizedString(keys[i]);
		}
		return array;
	}

	internal static string AddCraftingIconPrefix(string name)
	{
		return "ico_CraftItem__" + name;
	}

	internal static string RemoveCraftingIconPrefix(string iconFileName)
	{
		return iconFileName.Replace("ico_CraftItem__", "");
	}

	internal static string AddGearPrefix(string name)
	{
		return "GEAR_" + name;
	}

	internal static string RemoveGearPrefix(string gearName)
	{
		return gearName.Replace("GEAR_", "");
	}

	public static string? NormalizeName(string name)
	{
		return name?.Replace("(Clone)", "").Trim();
	}
}
