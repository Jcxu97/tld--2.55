using HarmonyLib;
using Il2Cpp;

namespace ModComponent.Mapper;

[HarmonyPatch(typeof(AfflictionButton), "SetCauseAndEffect")]
internal static class AfflictionButtonSetCauseAndEffectPatch
{
	public static void Prefix(ref string causeStr, AfflictionType affType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		string cause = BuffCauseTracker.GetCause(affType);
		if (!string.IsNullOrEmpty(cause))
		{
			causeStr = cause;
		}
	}
}
