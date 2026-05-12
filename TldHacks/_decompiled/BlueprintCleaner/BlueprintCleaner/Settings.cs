namespace BlueprintCleaner;

internal static class Settings
{
	internal static BlueprintCleanerSettings settings;

	public static void OnLoad()
	{
		settings = new BlueprintCleanerSettings();
		settings.AddToModSettings("蓝图堆叠管理器v1.1.1");
	}
}
