namespace NLBIni;

internal static class Assert
{
	internal static bool StringHasNoBlankSpaces(string s)
	{
		return !s.Contains(" ");
	}
}
