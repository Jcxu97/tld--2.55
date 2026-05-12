using System;
using UnityEngine;

namespace MotionTracker;

public class ColorTween : Tween<Color>
{
	private static readonly Func<ITween<Color>, Color, Color, float, Color> LerpFunc = LerpColor;

	private static Color LerpColor(ITween<Color> t, Color start, Color end, float progress)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return Color.Lerp(start, end, progress);
	}

	public ColorTween()
		: base(LerpFunc)
	{
	}
}
