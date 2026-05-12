using System;
using MelonLoader;

namespace CraftingRevisions;

internal static class Logger
{
	internal static void Log(string message)
	{
		MelonLogger.Msg(message);
	}

	internal static void LogWarning(string message)
	{
		MelonLogger.Warning(message);
	}

	internal static void LogError(string message)
	{
		MelonLogger.Error(message);
	}

	internal static void LogBlue(string message)
	{
		MelonLogger.Msg(ConsoleColor.Blue, message);
	}

	internal static void LogGreen(string message)
	{
		MelonLogger.Msg(ConsoleColor.Green, message);
	}

	internal static void LogDebug(string message)
	{
		Log(message);
	}

	internal static void LogNotDebug(string message)
	{
		Log(message);
	}
}
