using System.Runtime.InteropServices;

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

    internal static SoundEffect FromRaw(byte[] pcm, byte[] format, int durationMs, int loopStart, int loopLength) =>
        new(pcm, format, 0, format.Length, durationMs, loopStart, loopLength);

    public static float MasterVolume { get; set; } = 1f;

    public static float DistanceScale { get; set; } = 1f;

    public static float DopplerScale { get; set; } = 1f;

    public static float SpeedOfSound { get; set; } = 343.5f;

    public byte[] Pcm { get; }

    public TimeSpan Duration { get; }

    public int LoopStart { get; }

    public int LoopLength { get; }

    public int Channels { get; }

    public int SampleRate { get; }

    public string Name { get; set; } = string.Empty;

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

    public bool IsDisposed { get; private set; }

    public bool IsLooped { get; set; }

    public float Volume { get; set; } = 1f;

    public float Pitch { get; set; }

    public float Pan { get; set; }

    public SoundState State { get; private set; } = SoundState.Stopped;

    public void Play()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        State = SoundState.Playing;
    }

    public void Stop() => Stop(immediate: true);

    public void Stop(bool immediate)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        State = SoundState.Stopped;
    }

    public void Pause()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        State = SoundState.Paused;
    }

    public void Resume()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        State = SoundState.Playing;
    }

    public void Apply3D(AudioListener listener, AudioEmitter emitter)
    {
    }

    public void Apply3D(AudioListener[] listeners, AudioEmitter emitter)
    {
    }

    public void Dispose() => IsDisposed = true;
}

public enum SoundState
{
    Playing = 0,
    Paused = 1,
    Stopped = 2,
}

/// <summary>A 3-D audio emitter used by <see cref="SoundEffectInstance.Apply3D(AudioListener, AudioEmitter)"/>.</summary>
public class AudioEmitter
{
    public AudioEmitter()
    {
        Position = Vector3.Zero;
        Velocity = Vector3.Zero;
        Forward = Vector3.Forward;
        Up = Vector3.Up;
        DopplerScale = 1f;
    }

    public Vector3 Position { get; set; }

    public Vector3 Velocity { get; set; }

    public Vector3 Forward { get; set; }

    public Vector3 Up { get; set; }

    public float DopplerScale { get; set; }
}

/// <summary>A 3-D audio listener used by <see cref="SoundEffectInstance.Apply3D(AudioListener, AudioEmitter)"/>.</summary>
public class AudioListener
{
    public AudioListener()
    {
        Position = Vector3.Zero;
        Velocity = Vector3.Zero;
        Forward = Vector3.Forward;
        Up = Vector3.Up;
    }

    public Vector3 Position { get; set; }

    public Vector3 Velocity { get; set; }

    public Vector3 Forward { get; set; }

    public Vector3 Up { get; set; }
}

/// <summary>Raised when too many instances of a sound are playing.</summary>
[Serializable]
public sealed class InstancePlayLimitException : ExternalException
{
    public InstancePlayLimitException()
        : base("Too many sound instances are playing.")
    {
    }

    public InstancePlayLimitException(string message)
        : base(message)
    {
    }

    public InstancePlayLimitException(string message, Exception inner)
        : base(message, inner)
    {
    }
}

/// <summary>Raised when no usable audio device exists.</summary>
[Serializable]
public sealed class NoAudioHardwareException : ExternalException
{
    public NoAudioHardwareException()
        : base("No audio hardware is available.")
    {
    }

    public NoAudioHardwareException(string message)
        : base(message)
    {
    }

    public NoAudioHardwareException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
