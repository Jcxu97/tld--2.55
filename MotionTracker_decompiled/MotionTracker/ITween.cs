using System;

namespace MotionTracker;

public interface ITween
{
	object Key { get; }

	TweenState State { get; }

	Func<float> TimeFunc { get; set; }

	void Start();

	void Pause();

	void Resume();

	void Stop(TweenStopBehavior stopBehavior);

	bool Update(float elapsedTime);
}
public interface ITween<T> : ITween where T : struct
{
	T CurrentValue { get; }

	float CurrentProgress { get; }

	Tween<T> Setup(T start, T end, float duration, Func<float, float> scaleFunc, Action<ITween<T>> progress, Action<ITween<T>> completion = null);
}
