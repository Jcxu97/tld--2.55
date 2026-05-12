using System;
using UnityEngine;

namespace NorthernLightsBroadcast;

public class Vector4Tween : Tween<Vector4>
{
	private static readonly Func<ITween<Vector4>, Vector4, Vector4, float, Vector4> LerpFunc = LerpVector4;

	private static Vector4 LerpVector4(ITween<Vector4> t, Vector4 start, Vector4 end, float progress)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return Vector4.Lerp(start, end, progress);
	}

	public Vector4Tween()
		: base(LerpFunc)
	{
	}
}
