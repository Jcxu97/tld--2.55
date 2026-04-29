using System;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(FrameTimeProbe.ModMain), "FrameTimeProbe", "1.0.0", "user")]
[assembly: MelonGame("Hinterland", "TheLongDark")]

namespace FrameTimeProbe;

// 每 5 秒汇总一次最近 300 帧的 frame time 分布(p50/p95/p99/max)+ spike 计数
// 在 Latest.log 打印 "[FTP] fps=X p50=Yms p95=Zms p99=Wms max=Mms spikes16+=N spikes33+=M"
// spike 定义:frame time > 16.67ms(60fps 掉线)和 > 33ms(30fps 掉线)
public class ModMain : MelonMod
{
    private const int WINDOW = 300;              // 5s @ 60fps
    private readonly float[] _times = new float[WINDOW];
    private int _idx = 0;
    private int _count = 0;
    private float _elapsed = 0f;
    private MelonLogger.Instance _log;

    public override void OnInitializeMelon()
    {
        _log = LoggerInstance;
        _log.Msg("FrameTimeProbe v1.0 — 每 5s 汇总 frame time 分布到 log");
    }

    public override void OnUpdate()
    {
        float dt = Time.unscaledDeltaTime;  // unscaled 忽略 timeScale(SpeedMultiplier 影响不了)
        _times[_idx] = dt;
        _idx = (_idx + 1) % WINDOW;
        if (_count < WINDOW) _count++;
        _elapsed += dt;

        if (_elapsed >= 5f)
        {
            _elapsed = 0f;
            Report();
        }
    }

    private void Report()
    {
        int n = _count;
        if (n < 30) return;
        var copy = new float[n];
        Array.Copy(_times, copy, n);
        Array.Sort(copy);

        float p50 = copy[(int)(n * 0.5f)];
        float p95 = copy[(int)(n * 0.95f)];
        float p99 = copy[(int)(n * 0.99f)];
        float max = copy[n - 1];
        float avg = 0f;
        int s16 = 0, s33 = 0;
        for (int i = 0; i < n; i++)
        {
            avg += copy[i];
            if (copy[i] > 0.01667f) s16++;
            if (copy[i] > 0.03333f) s33++;
        }
        avg /= n;
        float avgFps = 1f / avg;

        _log.Msg($"[FTP] fps={avgFps:F0}  p50={p50 * 1000f:F1}ms  p95={p95 * 1000f:F1}ms  p99={p99 * 1000f:F1}ms  max={max * 1000f:F1}ms  >16ms={s16}/{n}  >33ms={s33}/{n}  | fixedDt={Time.fixedDeltaTime * 1000f:F1}ms({1f / Time.fixedDeltaTime:F0}Hz) timeScale={Time.timeScale:F2} vsync={QualitySettings.vSyncCount} targetFR={Application.targetFrameRate}");
    }
}
