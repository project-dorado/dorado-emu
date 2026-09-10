namespace Dorado.Platform;

/// <summary>Touch phase for a single touch point, mirroring XNA's <c>TouchLocationState</c>.</summary>
public enum TouchState
{
    Invalid = 0,
    Released = 1,
    Pressed = 2,
    Moved = 3,
}

/// <summary>A single touch sample in device (480x272) coordinates.</summary>
public readonly struct TouchPoint
{
    public TouchPoint(int id, float x, float y, TouchState state, float pressure)
    {
        Id = id;
        X = x;
        Y = y;
        State = state;
        Pressure = pressure;
    }

    public int Id { get; }

    public float X { get; }

    public float Y { get; }

    public TouchState State { get; }

    public float Pressure { get; }
}

/// <summary>An immutable input state for one frame.</summary>
public sealed class InputSnapshot
{
    public static readonly InputSnapshot Empty = new();

    public IReadOnlyList<TouchPoint> Touches { get; init; } = Array.Empty<TouchPoint>();

    public bool ButtonA { get; init; }

    public bool ButtonBack { get; init; }

    public bool ButtonStart { get; init; }

    public bool DPadUp { get; init; }

    public bool DPadDown { get; init; }

    public float LeftStickX { get; init; }

    public float LeftStickY { get; init; }

    public IReadOnlySet<int> Keys { get; init; } = new HashSet<int>();
}

/// <summary>Supplies per-frame input; scripted for tests, device-backed on Android.</summary>
public interface IInputSource
{
    InputSnapshot Snapshot(long frame);
}

/// <summary>An input source that reports no input.</summary>
public sealed class EmptyInputSource : IInputSource
{
    public static readonly EmptyInputSource Instance = new();

    public InputSnapshot Snapshot(long frame) => InputSnapshot.Empty;
}
