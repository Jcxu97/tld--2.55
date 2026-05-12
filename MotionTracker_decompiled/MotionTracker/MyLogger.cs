using System.Runtime.CompilerServices;

namespace MotionTracker;

public class MyLogger
{
	public static void LogMessage(string message, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string? caller = null, [CallerFilePath] string? filepath = null)
	{
	}
}
