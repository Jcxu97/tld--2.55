using System;
using MelonLoader;
using UnityEngine;

namespace Shotgun;

internal static class SetupTest
{
	public static bool Validate(ConfigurationReferences refs)
	{
		if (!ValidateTopLevel(refs))
		{
			return false;
		}
		return true;
	}

	internal static bool ValidateTopLevel(ConfigurationReferences refs)
	{
		if (!ValidateFPSCamera(refs))
		{
			return false;
		}
		if (!ValidateFPSPlayer(refs))
		{
			return false;
		}
		return true;
	}

	internal static bool ValidateFPSCamera(ConfigurationReferences refs)
	{
		ConfigurationReferences refs2 = refs;
		if (!TestAndReport(() => (UnityEngine.Object)(object)refs2.FPSCamera != (UnityEngine.Object)null, "Null FPSCamera"))
		{
			return false;
		}
		return true;
	}

	internal static bool ValidateFPSPlayer(ConfigurationReferences refs)
	{
		ConfigurationReferences refs2 = refs;
		if (!TestAndReport(() => (UnityEngine.Object)(object)refs2.FPSPlayer != (UnityEngine.Object)null, "Null FPSPlayer"))
		{
			return false;
		}
		return true;
	}

	internal static bool TestAndReport(Func<bool> test, string error)
	{
		if (!test())
		{
			MelonLogger.Msg("[TEST FAILURE]: " + error);
			return false;
		}
		return true;
	}
}
