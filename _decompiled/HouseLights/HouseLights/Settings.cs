namespace HouseLights;

internal static class Settings
{
	public static HouseLightsSettings options;

	public static void OnLoad()
	{
		options = new HouseLightsSettings();
		options.AddToModSettings("House Lights Settings");
	}
}
