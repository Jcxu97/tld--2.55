using System;
using System.Collections.Generic;
using Il2CppInterop.Runtime.Attributes;
using UnityEngine;

namespace MotionTracker;

public class TweenManager : MonoBehaviour
{
	private static GameObject root;

	private static readonly List<ITween> tweens = new List<ITween>();

	private static GameObject toDestroy;

	public static TweenStopBehavior AddKeyStopBehavior = TweenStopBehavior.DoNotModify;

	public static Func<float> DefaultTimeFunc = TimeFuncDeltaTime;

	public static readonly Func<float> TimeFuncDeltaTimeFunc = TimeFuncDeltaTime;

	public static readonly Func<float> TimeFuncUnscaledDeltaTimeFunc = TimeFuncUnscaledDeltaTime;

	public static bool ClearTweensOnLevelLoad { get; set; }

	public TweenManager(IntPtr intPtr)
		: base(intPtr)
	{
	}

	[HideFromIl2Cpp]
	private static void EnsureCreated()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected O, but got Unknown
		if (!((Object)(object)root == (Object)null) || !Application.isPlaying)
		{
			return;
		}
		root = GameObject.Find("ModTemplate.UtilsTween");
		if ((Object)(object)root == (Object)null || (Object)(object)root.GetComponent<TweenManager>() == (Object)null)
		{
			if ((Object)(object)root != (Object)null)
			{
				toDestroy = root;
			}
			root = new GameObject
			{
				name = "ModTemplate.UtilsTween",
				hideFlags = (HideFlags)61
			};
			((Object)root.AddComponent<TweenManager>()).hideFlags = (HideFlags)61;
		}
		if (Application.isPlaying)
		{
			Object.DontDestroyOnLoad((Object)(object)root);
		}
	}

	[HideFromIl2Cpp]
	private void Start()
	{
		if ((Object)(object)toDestroy != (Object)null)
		{
			Object.Destroy((Object)(object)toDestroy);
			toDestroy = null;
		}
	}

	[HideFromIl2Cpp]
	public static void SceneManagerSceneLoaded()
	{
		if (ClearTweensOnLevelLoad)
		{
			tweens.Clear();
		}
	}

	[HideFromIl2Cpp]
	private void Update()
	{
		for (int num = tweens.Count - 1; num >= 0; num--)
		{
			ITween tween = tweens[num];
			if (tween.Update(tween.TimeFunc()) && num < tweens.Count && tweens[num] == tween)
			{
				tweens.RemoveAt(num);
			}
		}
	}

	[HideFromIl2Cpp]
	public static void OwnUpdate()
	{
		for (int num = tweens.Count - 1; num >= 0; num--)
		{
			ITween tween = tweens[num];
			if (tween.Update(tween.TimeFunc()) && num < tweens.Count && tweens[num] == tween)
			{
				tweens.RemoveAt(num);
			}
		}
	}

	[HideFromIl2Cpp]
	public static FloatTween Tween(object key, float start, float end, float duration, Func<float, float> scaleFunc, Action<ITween<float>> progress, Action<ITween<float>> completion = null)
	{
		FloatTween floatTween = new FloatTween();
		floatTween.Key = key;
		floatTween.Setup(start, end, duration, scaleFunc, progress, completion);
		floatTween.Start();
		AddTween(floatTween);
		return floatTween;
	}

	[HideFromIl2Cpp]
	public static Vector2Tween Tween(object key, Vector2 start, Vector2 end, float duration, Func<float, float> scaleFunc, Action<ITween<Vector2>> progress, Action<ITween<Vector2>> completion = null)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		Vector2Tween vector2Tween = new Vector2Tween();
		vector2Tween.Key = key;
		vector2Tween.Setup(start, end, duration, scaleFunc, progress, completion);
		vector2Tween.Start();
		AddTween(vector2Tween);
		return vector2Tween;
	}

	[HideFromIl2Cpp]
	public static Vector3Tween Tween(object key, Vector3 start, Vector3 end, float duration, Func<float, float> scaleFunc, Action<ITween<Vector3>> progress, Action<ITween<Vector3>> completion = null)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		Vector3Tween vector3Tween = new Vector3Tween();
		vector3Tween.Key = key;
		vector3Tween.Setup(start, end, duration, scaleFunc, progress, completion);
		vector3Tween.Start();
		AddTween(vector3Tween);
		return vector3Tween;
	}

	[HideFromIl2Cpp]
	public static Vector4Tween Tween(object key, Vector4 start, Vector4 end, float duration, Func<float, float> scaleFunc, Action<ITween<Vector4>> progress, Action<ITween<Vector4>> completion = null)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		Vector4Tween vector4Tween = new Vector4Tween();
		vector4Tween.Key = key;
		vector4Tween.Setup(start, end, duration, scaleFunc, progress, completion);
		vector4Tween.Start();
		AddTween(vector4Tween);
		return vector4Tween;
	}

	[HideFromIl2Cpp]
	public static ColorTween Tween(object key, Color start, Color end, float duration, Func<float, float> scaleFunc, Action<ITween<Color>> progress, Action<ITween<Color>> completion = null)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		ColorTween colorTween = new ColorTween();
		colorTween.Key = key;
		colorTween.Setup(start, end, duration, scaleFunc, progress, completion);
		colorTween.Start();
		AddTween(colorTween);
		return colorTween;
	}

	[HideFromIl2Cpp]
	public static QuaternionTween Tween(object key, Quaternion start, Quaternion end, float duration, Func<float, float> scaleFunc, Action<ITween<Quaternion>> progress, Action<ITween<Quaternion>> completion = null)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		QuaternionTween quaternionTween = new QuaternionTween();
		quaternionTween.Key = key;
		quaternionTween.Setup(start, end, duration, scaleFunc, progress, completion);
		quaternionTween.Start();
		AddTween(quaternionTween);
		return quaternionTween;
	}

	[HideFromIl2Cpp]
	public static void AddTween(ITween tween)
	{
		EnsureCreated();
		if (tween.Key != null)
		{
			RemoveTweenKey(tween.Key, AddKeyStopBehavior);
		}
		tweens.Add(tween);
	}

	[HideFromIl2Cpp]
	public static bool RemoveTween(ITween tween, TweenStopBehavior stopBehavior)
	{
		tween.Stop(stopBehavior);
		return tweens.Remove(tween);
	}

	[HideFromIl2Cpp]
	public static bool RemoveTweenKey(object key, TweenStopBehavior stopBehavior)
	{
		if (key == null)
		{
			return false;
		}
		bool result = false;
		for (int num = tweens.Count - 1; num >= 0; num--)
		{
			ITween tween = tweens[num];
			if (key.Equals(tween.Key))
			{
				tween.Stop(stopBehavior);
				tweens.RemoveAt(num);
				result = true;
			}
		}
		return result;
	}

	[HideFromIl2Cpp]
	public static void Clear()
	{
		tweens.Clear();
	}

	[HideFromIl2Cpp]
	private static float TimeFuncDeltaTime()
	{
		return Time.deltaTime;
	}

	[HideFromIl2Cpp]
	private static float TimeFuncUnscaledDeltaTime()
	{
		return Time.unscaledDeltaTime;
	}
}
