using System.IO;
using ModSettings;

namespace Shotgun;

internal class Settings : JsonModSettings
{
	internal static Settings Instance;

	[Name("Custom Ammo HUD X Offset")]
	[Slider(10f, 1920f, 192)]
	[Description("X offset of custom ammo HUD, measured in pixels from the right of the screen relative to a screen size of 1920x1080. Scales with your actual resolution.")]
	public int CustomAmmoHUDXOffset = 60;

	[Name("Custom Ammo HUD Y Offset")]
	[Slider(10f, 1080f, 108)]
	[Description("Y offset of custom ammo HUD, measured in pixels from the bottom of the screen relative to a screen size of 1920x1080. Scales with your actual resolution.")]
	public int CustomAmmoHUDYOffset = 80;

	[Name("Custom Ammo HUD Icon Height")]
	[Slider(30f, 120f, 19)]
	[Description("Y offset of custom ammo HUD, measured in pixels from the bottom of the screen relative to a screen size of 1920x1080. Scales with your actual resolution.")]
	public int CustomAmmoHUDIconHeight = 50;

	[Name("Custom Ammo HUD Spacing Ratio")]
	[Slider(0.25f, 3f, 12)]
	[Description("Spacing-to-Width ratio for Custom Ammo HUD icons. Higher values will result in larger spacing between ammo icons.")]
	public float CustomAmmoHUDIconSpacingRatio = 1f;

	[Name("Enable April Fools Effects")]
	[Description("When enabled, the shotgun fires pink sparkles and animals spin when hit. Fun for April 1st!")]
	public bool EnableAprilFools = false;

	internal Settings()
		: base(Path.Combine("Shotgun", "Shotgun"))
	{
		Initialize();
	}

	protected void Initialize()
	{
		Instance = this;
		((ModSettingsBase)this).AddToModSettings("Shotgun");
		((ModSettingsBase)this).RefreshGUI();
	}

	protected override void OnConfirm()
	{
		base.OnConfirm();
		CustomAmmoHUD.Current.UpdateIconPanelPosition();
	}
}
