using System;
using UnityEngine;

namespace NorthernLightsBroadcast;

public static class TweenScaleFunctions
{
	private const float halfPi = (float)Math.PI / 2f;

	public static readonly Func<float, float> Linear = LinearFunc;

	public static readonly Func<float, float> QuadraticEaseIn = QuadraticEaseInFunc;

	public static readonly Func<float, float> QuadraticEaseOut = QuadraticEaseOutFunc;

	public static readonly Func<float, float> QuadraticEaseInOut = QuadraticEaseInOutFunc;

	public static readonly Func<float, float> CubicEaseIn = CubicEaseInFunc;

	public static readonly Func<float, float> CubicEaseOut = CubicEaseOutFunc;

	public static readonly Func<float, float> CubicEaseInOut = CubicEaseInOutFunc;

	public static readonly Func<float, float> QuarticEaseIn = QuarticEaseInFunc;

	public static readonly Func<float, float> QuarticEaseOut = QuarticEaseOutFunc;

	public static readonly Func<float, float> QuarticEaseInOut = QuarticEaseInOutFunc;

	public static readonly Func<float, float> QuinticEaseIn = QuinticEaseInFunc;

	public static readonly Func<float, float> QuinticEaseOut = QuinticEaseOutFunc;

	public static readonly Func<float, float> QuinticEaseInOut = QuinticEaseInOutFunc;

	public static readonly Func<float, float> SineEaseIn = SineEaseInFunc;

	public static readonly Func<float, float> SineEaseOut = SineEaseOutFunc;

	public static readonly Func<float, float> SineEaseInOut = SineEaseInOutFunc;

	private static float LinearFunc(float progress)
	{
		return progress;
	}

	private static float QuadraticEaseInFunc(float progress)
	{
		return EaseInPower(progress, 2);
	}

	private static float QuadraticEaseOutFunc(float progress)
	{
		return EaseOutPower(progress, 2);
	}

	private static float QuadraticEaseInOutFunc(float progress)
	{
		return EaseInOutPower(progress, 2);
	}

	private static float CubicEaseInFunc(float progress)
	{
		return EaseInPower(progress, 3);
	}

	private static float CubicEaseOutFunc(float progress)
	{
		return EaseOutPower(progress, 3);
	}

	private static float CubicEaseInOutFunc(float progress)
	{
		return EaseInOutPower(progress, 3);
	}

	private static float QuarticEaseInFunc(float progress)
	{
		return EaseInPower(progress, 4);
	}

	private static float QuarticEaseOutFunc(float progress)
	{
		return EaseOutPower(progress, 4);
	}

	private static float QuarticEaseInOutFunc(float progress)
	{
		return EaseInOutPower(progress, 4);
	}

	private static float QuinticEaseInFunc(float progress)
	{
		return EaseInPower(progress, 5);
	}

	private static float QuinticEaseOutFunc(float progress)
	{
		return EaseOutPower(progress, 5);
	}

	private static float QuinticEaseInOutFunc(float progress)
	{
		return EaseInOutPower(progress, 5);
	}

	private static float SineEaseInFunc(float progress)
	{
		return Mathf.Sin(progress * ((float)Math.PI / 2f) - (float)Math.PI / 2f) + 1f;
	}

	private static float SineEaseOutFunc(float progress)
	{
		return Mathf.Sin(progress * ((float)Math.PI / 2f));
	}

	private static float SineEaseInOutFunc(float progress)
	{
		return (Mathf.Sin(progress * (float)Math.PI - (float)Math.PI / 2f) + 1f) / 2f;
	}

	private static float EaseInPower(float progress, int power)
	{
		return Mathf.Pow(progress, (float)power);
	}

	private static float EaseOutPower(float progress, int power)
	{
		int num = ((power % 2 != 0) ? 1 : (-1));
		return (float)num * (Mathf.Pow(progress - 1f, (float)power) + (float)num);
	}

	private static float EaseInOutPower(float progress, int power)
	{
		progress *= 2f;
		if (progress < 1f)
		{
			return Mathf.Pow(progress, (float)power) / 2f;
		}
		int num = ((power % 2 != 0) ? 1 : (-1));
		return (float)num / 2f * (Mathf.Pow(progress - 2f, (float)power) + (float)(num * 2));
	}
}
