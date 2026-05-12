using System;
using System.Security.Cryptography;
using UnityEngine;

namespace ModComponent.Utils;

internal static class RandomUtils
{
	private static readonly RNGCryptoServiceProvider cryptoRandom = new RNGCryptoServiceProvider();

	public static int Range(int min, int max, bool maxInclusive)
	{
		if (maxInclusive)
		{
			return Range(min, max + 1);
		}
		return Range(min, max);
	}

	public static int Range(int min, int max)
	{
		if (min > max)
		{
			int num = max;
			int num2 = min;
			min = num;
			max = num2;
		}
		if (min == max || min == max - 1)
		{
			return min;
		}
		int num3 = (int)Math.Floor(RandomFloat() * (float)(max - min) + (float)min);
		if (num3 >= max)
		{
			num3 = max - 1;
		}
		return num3;
	}

	public static float Range(float min, float max)
	{
		return RandomFloat() * (max - min) + min;
	}

	public static bool RollChance(float percent)
	{
		if (percent <= 0f)
		{
			return false;
		}
		if (percent >= 100f)
		{
			return true;
		}
		return RandomFloat() < Mathf.Clamp01(percent / 100f);
	}

	public static int RandomInt()
	{
		byte[] array = new byte[4];
		cryptoRandom.GetBytes(array);
		return BitConverter.ToInt32(array, 0);
	}

	public static long RandomLong()
	{
		byte[] array = new byte[8];
		cryptoRandom.GetBytes(array);
		return BitConverter.ToInt64(array, 0);
	}

	public static uint RandomUInt()
	{
		byte[] array = new byte[4];
		cryptoRandom.GetBytes(array);
		return BitConverter.ToUInt32(array, 0);
	}

	public static ulong RandomULong()
	{
		byte[] array = new byte[8];
		cryptoRandom.GetBytes(array);
		return BitConverter.ToUInt64(array, 0);
	}

	public static float RandomFloat()
	{
		return (float)((double)RandomULong() / 1.8446744073709552E+19);
	}

	public static double RandomDouble()
	{
		return (double)RandomULong() / 1.8446744073709552E+19;
	}
}
