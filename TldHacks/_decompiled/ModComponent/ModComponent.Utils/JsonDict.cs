using System.Collections.Generic;

namespace ModComponent.Utils;

internal class JsonDict : Dictionary<string, JsonDictEntry>
{
	internal JsonDictEntry? GetEntry(string key)
	{
		if (ContainsKey(key))
		{
			return base[key];
		}
		return new JsonDictEntry();
	}
}
