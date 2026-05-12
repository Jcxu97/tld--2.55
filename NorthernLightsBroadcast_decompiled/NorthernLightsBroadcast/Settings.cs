using ModSettings;

namespace NorthernLightsBroadcast;

internal static class Settings
{
	public enum LoopSetting
	{
		Off,
		LoopFile,
		LoopFolder
	}

	public static TVSettings options;

	public static void OnLoad()
	{
		options = new TVSettings();
		((ModSettingsBase)options).AddToModSettings("NorthernLightsBroadcast");
	}
}
