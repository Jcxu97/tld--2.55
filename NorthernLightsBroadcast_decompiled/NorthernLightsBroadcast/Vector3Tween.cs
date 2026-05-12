using System;
using UnityEngine;

namespace NorthernLightsBroadcast;

public class Vector3Tween : Tween<Vector3>
{
	private static readonly Func<ITween<Vector3>, Vector3, Vector3, float, Vector3> LerpFunc = LerpVector3;

	private static Vector3 LerpVector3(ITween<Vector3> t, Vector3 start, Vector3 end, float progress)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Lerp(start, end, progress);
	}

	public Vector3Tween()
		: base(LerpFunc)
	{
	}
}
