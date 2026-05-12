using System;
using System.Reflection;
using MelonLoader;

namespace ModComponent;

internal static class Logger
{
	private static bool reflectionSuccessful;

	private static MethodInfo? method_Internal_Error;

	private static MethodInfo? method_RunErrorCallbacks;

	static Logger()
	{
		GetHiddenLogMethods();
		LogDebug($"Reflection Successful: {reflectionSuccessful}");
	}

	internal static void Log(string message)
	{
		((MelonBase)Implementation.instance).LoggerInstance.Msg(message);
	}

	internal static void LogWarning(string message)
	{
		((MelonBase)Implementation.instance).LoggerInstance.Warning(message);
	}

	internal static void LogError(string message)
	{
		((MelonBase)Implementation.instance).LoggerInstance.Error(message);
	}

	internal static void LogItemPackError(string namesection, string message)
	{
		if (!reflectionSuccessful)
		{
			LogError(message);
			return;
		}
		string text = string.Format(message);
		method_Internal_Error?.Invoke(null, new object[2] { namesection, text });
		method_RunErrorCallbacks?.Invoke(null, new object[2] { namesection, text });
	}

	internal static void LogBlue(string message)
	{
		((MelonBase)Implementation.instance).LoggerInstance.Msg(ConsoleColor.Blue, message);
	}

	internal static void LogGreen(string message)
	{
		((MelonBase)Implementation.instance).LoggerInstance.Msg(ConsoleColor.Green, message);
	}

	internal static void LogDebug(string message)
	{
		if (Settings.instance.showDebugOutput)
		{
			Log(message);
		}
	}

	internal static void LogNotDebug(string message)
	{
		Log(message);
	}

	private static void GetHiddenLogMethods()
	{
		MethodInfo[] methods = typeof(MelonLogger).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (MethodInfo methodInfo in methods)
		{
			if (methodInfo.Name == "Internal_Error")
			{
				method_Internal_Error = methodInfo;
			}
			else if (methodInfo.Name == "RunErrorCallbacks")
			{
				method_RunErrorCallbacks = methodInfo;
			}
		}
		reflectionSuccessful = method_Internal_Error != null && method_RunErrorCallbacks != null;
	}
}
