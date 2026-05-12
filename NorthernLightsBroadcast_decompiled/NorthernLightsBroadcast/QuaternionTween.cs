using System;
using UnityEngine;

namespace NorthernLightsBroadcast;

public class QuaternionTween : Tween<Quaternion>
{
	private static readonly Func<ITween<Quaternion>, Quaternion, Quaternion, float, Quaternion> LerpFunc = LerpQuaternion;

	private static Quaternion LerpQuaternion(ITween<Quaternion> t, Quaternion start, Quaternion end, float progress)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return Quaternion.Lerp(start, end, progress);
	}

	public QuaternionTween()
		: base(LerpFunc)
	{
	}
}
