using System;
using UnityEngine;

namespace MotionTracker;

public class Vector2Tween : Tween<Vector2>
{
	private static readonly Func<ITween<Vector2>, Vector2, Vector2, float, Vector2> LerpFunc = LerpVector2;

	private static Vector2 LerpVector2(ITween<Vector2> t, Vector2 start, Vector2 end, float progress)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return Vector2.Lerp(start, end, progress);
	}

	public Vector2Tween()
		: base(LerpFunc)
	{
	}
}
