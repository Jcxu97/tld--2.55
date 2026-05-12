using System;

namespace GearSpawner;

public sealed class InvalidFormatException : Exception
{
	internal InvalidFormatException()
	{
	}

	internal InvalidFormatException(string message)
		: base(message)
	{
	}
}
