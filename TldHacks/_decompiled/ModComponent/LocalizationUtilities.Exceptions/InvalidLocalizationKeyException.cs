using System;

namespace LocalizationUtilities.Exceptions;

public sealed class InvalidLocalizationKeyException : Exception
{
	public InvalidLocalizationKeyException()
	{
	}

	public InvalidLocalizationKeyException(string message)
		: base(message)
	{
	}
}
