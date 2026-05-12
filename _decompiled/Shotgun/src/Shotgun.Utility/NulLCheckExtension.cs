using System;
using System.Reflection;
using MelonLoader;
using UnityEngine;

namespace Shotgun.Utility;

internal static class NulLCheckExtension
{
	internal static void ReportIfPropertyNull<T>(this T script, string propertyName) where T : MonoBehaviour
	{
		try
		{
			PropertyInfo property = ((object)script).GetType().GetProperty(propertyName);
			if (property == null)
			{
				MelonLogger.Error("'" + propertyName + "' not found.");
			}
			else if (property.GetValue(script, null) == null)
			{
				MelonLogger.Msg("'" + propertyName + "' is null");
			}
		}
		catch (Exception)
		{
			MelonLogger.Error("Error while trying to access '" + propertyName + "'");
		}
	}

	internal static void ReportNullProperties<T>(this T script, string optionalDescription = "") where T : MonoBehaviour
	{
		NullChecks.ReportNulls(script, optionalDescription);
	}
}
