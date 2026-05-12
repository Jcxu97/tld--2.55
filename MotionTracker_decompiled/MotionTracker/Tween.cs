using System;
using Il2CppInterop.Runtime.Attributes;
using UnityEngine;

namespace MotionTracker;

public class Tween<T> : ITween<T>, ITween where T : struct
{
	private readonly Func<ITween<T>, T, T, float, T> lerpFunc;

	private float currentTime;

	private float duration;

	private Func<float, float> scaleFunc;

	private Action<ITween<T>> progressCallback;

	private Action<ITween<T>> completionCallback;

	private TweenState state;

	private T start;

	private T end;

	private T value;

	private ITween continueWith;

	public object Key { get; set; }

	public float CurrentTime => currentTime;

	public float Duration => duration;

	public float Delay { get; set; }

	public TweenState State => state;

	public T StartValue => start;

	public T EndValue => end;

	public T CurrentValue => value;

	public Func<float> TimeFunc { get; set; }

	public GameObject GameObject { get; set; }

	public Renderer Renderer { get; set; }

	public bool ForceUpdate { get; set; }

	public float CurrentProgress { get; private set; }

	public Tween(Func<ITween<T>, T, T, float, T> lerpFunc)
	{
		this.lerpFunc = lerpFunc;
		state = TweenState.Stopped;
		TimeFunc = TweenManager.DefaultTimeFunc;
	}

	[HideFromIl2Cpp]
	public Tween<T> Setup(T start, T end, float duration, Func<float, float> scaleFunc, Action<ITween<T>> progress, Action<ITween<T>> completion = null)
	{
		scaleFunc = scaleFunc ?? TweenScaleFunctions.Linear;
		currentTime = 0f;
		this.duration = duration;
		this.scaleFunc = scaleFunc;
		progressCallback = progress;
		completionCallback = completion;
		this.start = start;
		this.end = end;
		return this;
	}

	[HideFromIl2Cpp]
	public void Start()
	{
		if (state == TweenState.Running)
		{
			return;
		}
		if (duration <= 0f && Delay <= 0f)
		{
			value = end;
			if (progressCallback != null)
			{
				progressCallback(this);
			}
			if (completionCallback != null)
			{
				completionCallback(this);
			}
		}
		else
		{
			state = TweenState.Running;
			UpdateValue();
		}
	}

	[HideFromIl2Cpp]
	public void Pause()
	{
		if (state == TweenState.Running)
		{
			state = TweenState.Paused;
		}
	}

	[HideFromIl2Cpp]
	public void Resume()
	{
		if (state == TweenState.Paused)
		{
			state = TweenState.Running;
		}
	}

	[HideFromIl2Cpp]
	public void Stop(TweenStopBehavior stopBehavior)
	{
		if (state == TweenState.Stopped)
		{
			return;
		}
		state = TweenState.Stopped;
		if (stopBehavior == TweenStopBehavior.Complete)
		{
			currentTime = duration;
			UpdateValue();
			if (completionCallback != null)
			{
				completionCallback(this);
				completionCallback = null;
			}
			if (continueWith != null)
			{
				continueWith.Start();
				TweenManager.AddTween(continueWith);
				continueWith = null;
			}
		}
	}

	[HideFromIl2Cpp]
	public bool Update(float elapsedTime)
	{
		if (state == TweenState.Running)
		{
			if (Delay > 0f)
			{
				currentTime += elapsedTime;
				if (currentTime <= Delay)
				{
					return false;
				}
				currentTime -= Delay;
				Delay = 0f;
			}
			else
			{
				currentTime += elapsedTime;
			}
			if (currentTime >= duration)
			{
				Stop(TweenStopBehavior.Complete);
				return true;
			}
			UpdateValue();
			return false;
		}
		return state == TweenState.Stopped;
	}

	[HideFromIl2Cpp]
	public Tween<TNewTween> ContinueWith<TNewTween>(Tween<TNewTween> tween) where TNewTween : struct
	{
		tween.Key = Key;
		tween.GameObject = GameObject;
		tween.Renderer = Renderer;
		tween.ForceUpdate = ForceUpdate;
		continueWith = tween;
		return tween;
	}

	[HideFromIl2Cpp]
	private void UpdateValue()
	{
		if ((Object)(object)Renderer == (Object)null || Renderer.isVisible || ForceUpdate)
		{
			CurrentProgress = scaleFunc(currentTime / duration);
			value = lerpFunc(this, start, end, CurrentProgress);
			if (progressCallback != null)
			{
				progressCallback(this);
			}
		}
	}
}
