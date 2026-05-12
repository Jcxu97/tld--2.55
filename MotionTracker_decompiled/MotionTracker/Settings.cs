using UnityEngine;

namespace MotionTracker;

internal static class Settings
{
	public enum DisplayStyle
	{
		AlwaysOn,
		Toggle
	}

	public static MotionTrackerSettings options;

	public static Vector3 animalScale;

	public static Vector3 spraypaintScale;

	public static Vector3 gearScale;

	public static Color animalColor;

	public static Color spraypaintColor;

	public static Color gearColor;

	public static bool toggleBool;

	public static void OnLoad()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		options = new MotionTrackerSettings();
		options.AddToModSettings("野生动物雷达追踪v1.3");
		animalScale = new Vector3(options.animalScale, options.animalScale, options.animalScale);
		gearScale = new Vector3(options.gearScale, options.gearScale, options.gearScale);
		spraypaintScale = new Vector3(options.spraypaintScale, options.spraypaintScale, options.spraypaintScale);
		animalColor = new Color(1f, 1f, 1f, options.animalOpacity);
		gearColor = new Color(1f, 1f, 1f, options.gearOpacity);
		spraypaintColor = new Color(0.62f, 0.29f, 0f, options.spraypaintOpacity);
	}
}
