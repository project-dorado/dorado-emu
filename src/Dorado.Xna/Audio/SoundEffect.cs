namespace Microsoft.Xna.Framework.Audio;

/// <summary>A decoded PCM sound effect. Playback is a no-op until the audio backend lands (M2).</summary>
public sealed class SoundEffect : IDisposable
{
    internal SoundEffect(byte[] pcm, byte[] container, int formatStart, int formatSize, int durationMs, int loopStart, int loopLength)
    {
        Pcm = pcm;
        Duration = TimeSpan.FromMilliseconds(durationMs);
        LoopStart = loopStart;
        LoopLength = loopLength;
        if (formatSize >= 8 && formatStart + 8 <= container.Length)
        {
            Channels = BitConverter.ToInt16(container, formatStart + 2);
            SampleRate = BitConverter.ToInt32(container, formatStart + 4);
        }
    }

    public byte[] Pcm { get; }

    public TimeSpan Duration { get; }

    public int LoopStart { get; }

    public int LoopLength { get; }

    public int Channels { get; }

    public int SampleRate { get; }

    public string? Name { get; set; }

    public bool IsDisposed { get; private set; }

    public bool Play() => Play(1f, 0f, 0f);

    public bool Play(float volume, float pitch, float pan)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        return true;
    }

    public SoundEffectInstance CreateInstance()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        return new SoundEffectInstance(this);
    }

    public void Dispose() => IsDisposed = true;
}

/// <summary>A playable instance of a <see cref="SoundEffect"/>.</summary>
public sealed class SoundEffectInstance : IDisposable
{
    internal SoundEffectInstance(SoundEffect effect)
    {
        Effect = effect;
    }

    public SoundEffect Effect { get; }

    public bool IsLooped { get; set; }

    public float Volume { get; set; } = 1f;

    public float Pitch { get; set; }

    public float Pan { get; set; }

    public SoundState State { get; private set; } = SoundState.Stopped;

    public void Play()
    {
        State = SoundState.Playing;
    }

    public void Pause() => State = SoundState.Paused;

    public void Resume() => State = SoundState.Playing;

    public void Stop() => State = SoundState.Stopped;

    public void Dispose()
    {
    }
}

public enum SoundState
{
    Playing = 0,
    Paused = 1,
    Stopped = 2,
}
