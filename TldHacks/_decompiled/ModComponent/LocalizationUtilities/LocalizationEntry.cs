using System.Collections.Generic;
using LocalizationUtilities.Exceptions;

namespace LocalizationUtilities;

public sealed record LocalizationEntry(string LocalizationID, Dictionary<string, string> Map)
{
	public LocalizationEntry(string localizationID)
		: this(localizationID, new Dictionary<string, string>())
	{
	}

	public void Validate()
	{
		if (string.IsNullOrEmpty(LocalizationID))
		{
			throw new InvalidLocalizationKeyException("Localization ID cannot be null or empty.");
		}
		if (Map == null)
		{
			throw new InvalidLanguageMapException("Map cannot be null.");
		}
		if (Map.Count == 0)
		{
			throw new InvalidLanguageMapException("Map cannot have no contents.");
		}
		foreach (KeyValuePair<string, string> item in Map)
		{
			if (string.IsNullOrEmpty(item.Key))
			{
				throw new InvalidLanguageMapException("Localization language cannot be null or empty.");
			}
			if (item.Value == null)
			{
				throw new InvalidLanguageMapException("Localized text cannot be null.");
			}
		}
	}

	public override string ToString()
	{
		return LocalizationID;
	}
}
