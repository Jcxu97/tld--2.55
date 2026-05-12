using System.Collections.Generic;
using Il2Cpp;

namespace ModComponent.Mapper;

internal static class BuffCauseTracker
{
	private static Dictionary<AfflictionType, string> causes = new Dictionary<AfflictionType, string>();

	public static void SetCause(AfflictionType buff, string cause)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (causes.ContainsKey(buff))
		{
			causes[buff] = cause;
		}
		else
		{
			causes.Add(buff, cause);
		}
	}

	public static string GetCause(AfflictionType buff)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		causes.TryGetValue(buff, out string value);
		return value;
	}
}
