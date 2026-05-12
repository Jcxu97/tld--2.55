using System;

namespace LocalizationUtilities.Exceptions;

public sealed class InvalidEntryListException : Exception
{
	public InvalidEntryListException()
	{
	}

	public InvalidEntryListException(string message)
		: base(message)
	{
	}
}
