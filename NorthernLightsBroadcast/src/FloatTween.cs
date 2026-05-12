using System;

namespace NorthernLightsBroadcast;

public class FloatTween : Tween<float>
{
	private static readonly Func<ITween<float>, float, float, float, float> LerpFunc = LerpFloat;

	private static float LerpFloat(ITween<float> t, float start, float end, float progress)
	{
		return start + (end - start) * progress;
	}

	public FloatTween()
		: base(LerpFunc)
	{
	}
}
