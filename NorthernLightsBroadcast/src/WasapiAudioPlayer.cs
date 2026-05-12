using System;
using MelonLoader;
using NAudio.Wave;

namespace NorthernLightsBroadcast;

public class VolumeSampleProvider : ISampleProvider
{
    private readonly ISampleProvider _source;
    public WaveFormat WaveFormat => _source.WaveFormat;
    public float Volume { get; set; } = 1.0f;

    public VolumeSampleProvider(ISampleProvider source) { _source = source; }

    public int Read(float[] buffer, int offset, int count)
    {
        int read = _source.Read(buffer, offset, count);
        float vol = Volume;
        if (vol != 1.0f)
        {
            for (int i = 0; i < read; i++)
                buffer[offset + i] *= vol;
        }
        return read;
    }
}

public class WasapiAudioPlayer : IDisposable
{
    private WasapiOut _wasapiOut;
    private MediaFoundationReader _reader;
    private VolumeSampleProvider _volumeProvider;
    private bool _disposed;
    private float _volume = 1.0f;

    public bool IsPlaying => _wasapiOut?.PlaybackState == PlaybackState.Playing;
    public bool IsPaused => _wasapiOut?.PlaybackState == PlaybackState.Paused;
    public bool IsReady => _wasapiOut != null && _reader != null;
    public double CurrentTime => _reader?.CurrentTime.TotalSeconds ?? 0;
    public double TotalTime => _reader?.TotalTime.TotalSeconds ?? 0;

    public float Volume
    {
        get => _volume;
        set
        {
            _volume = Math.Clamp(value, 0f, 1f);
            if (_volumeProvider != null)
                _volumeProvider.Volume = _volume;
        }
    }

    public bool Open(string filePath)
    {
        try
        {
            Close();
            _reader = new MediaFoundationReader(filePath);
            var sampleProvider = _reader.ToSampleProvider();
            _volumeProvider = new VolumeSampleProvider(sampleProvider) { Volume = _volume };
            _wasapiOut = new WasapiOut();
            _wasapiOut.Init(_volumeProvider);
            MelonLogger.Msg($"[NLB] WasapiOut opened: {_reader.WaveFormat.SampleRate}Hz, {_reader.WaveFormat.Channels}ch, {_reader.TotalTime.TotalSeconds:F1}s");
            return true;
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"[NLB] WasapiOut open failed: {ex.Message}");
            Close();
            return false;
        }
    }

    public void Play()
    {
        if (_wasapiOut == null) return;
        _wasapiOut.Play();
    }

    public void Pause()
    {
        if (_wasapiOut == null) return;
        _wasapiOut.Pause();
    }

    public void Resume()
    {
        if (_wasapiOut == null) return;
        _wasapiOut.Play();
    }

    public void Stop()
    {
        if (_wasapiOut == null) return;
        _wasapiOut.Stop();
    }

    public void Seek(double seconds)
    {
        if (_reader == null) return;
        try
        {
            var target = TimeSpan.FromSeconds(seconds);
            if (target < TimeSpan.Zero) target = TimeSpan.Zero;
            if (target > _reader.TotalTime) target = _reader.TotalTime;
            _reader.CurrentTime = target;
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"[NLB] WasapiOut seek failed: {ex.Message}");
        }
    }

    public void Close()
    {
        try
        {
            _wasapiOut?.Stop();
            _wasapiOut?.Dispose();
        }
        catch { }
        try
        {
            _reader?.Dispose();
        }
        catch { }
        _wasapiOut = null;
        _reader = null;
        _volumeProvider = null;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Close();
    }
}
