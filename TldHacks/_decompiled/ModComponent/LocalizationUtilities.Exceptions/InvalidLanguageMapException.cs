using System;

namespace LocalizationUtilities.Exceptions;

public sealed class InvalidLanguageMapException : Exception
{
	public InvalidLanguageMapException()
	{
	}

	public InvalidLanguageMapException(string message)
		: base(message)
	{
	}
}
