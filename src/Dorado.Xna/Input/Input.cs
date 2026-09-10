using System.Collections;
using Dorado.Platform;

namespace Microsoft.Xna.Framework.Input;

/// <summary>Key codes (Windows virtual-key values, matching XNA).</summary>
public enum Keys
{
    None = 0,
    Back = 8,
    Tab = 9,
    Enter = 13,
    Shift = 16,
    Ctrl = 17,
    Alt = 18,
    Pause = 19,
    CapsLock = 20,
    Escape = 27,
    Space = 32,
    PageUp = 33,
    PageDown = 34,
    End = 35,
    Home = 36,
    Left = 37,
    Up = 38,
    Right = 39,
    Down = 40,
    Select = 41,
    Print = 42,
    Insert = 45,
    Delete = 46,
    D0 = 48,
    D1 = 49,
    D2 = 50,
    D3 = 51,
    D4 = 52,
    D5 = 53,
    D6 = 54,
    D7 = 55,
    D8 = 56,
    D9 = 57,
    A = 65,
    B = 66,
    C = 67,
    D = 68,
    E = 69,
    F = 70,
    G = 71,
    H = 72,
    I = 73,
    J = 74,
    K = 75,
    L = 76,
    M = 77,
    N = 78,
    O = 79,
    P = 80,
    Q = 81,
    R = 82,
    S = 83,
    T = 84,
    U = 85,
    V = 86,
    W = 87,
    X = 88,
    Y = 89,
    Z = 90,
    LeftShift = 160,
    RightShift = 161,
    LeftControl = 162,
    RightControl = 163,
    LeftAlt = 164,
    RightAlt = 165,
}

public static class Keyboard
{
    public static KeyboardState GetState() => new(PlatformHost.Input.Snapshot(PlatformHost.FrameCount).Keys);

    public static KeyboardState GetState(PlayerIndex playerIndex) => GetState();
}

public readonly struct KeyboardState
{
    private readonly IReadOnlySet<int> _keys;

    internal KeyboardState(IReadOnlySet<int> keys) => _keys = keys;

    public bool IsKeyDown(Keys key) => _keys is not null && _keys.Contains((int)key);

    public bool IsKeyUp(Keys key) => !IsKeyDown(key);

    public Keys[] GetPressedKeys() =>
        _keys is null ? Array.Empty<Keys>() : _keys.Select(k => (Keys)k).ToArray();
}

public enum ButtonState
{
    Released = 0,
    Pressed = 1,
}

public static class GamePad
{
    public static GamePadState GetState(PlayerIndex playerIndex) => GetState(playerIndex, GamePadDeadZone.None);

    public static GamePadState GetState(PlayerIndex playerIndex, GamePadDeadZone deadZone)
    {
        InputSnapshot snapshot = PlatformHost.Input.Snapshot(PlatformHost.FrameCount);
        return new GamePadState(snapshot);
    }
}

public enum GamePadDeadZone
{
    None = 0,
    IndependentAxes = 1,
    Circular = 2,
}

public readonly struct GamePadState
{
    private readonly InputSnapshot _snapshot;

    internal GamePadState(InputSnapshot snapshot)
    {
        _snapshot = snapshot;
        Buttons = new GamePadButtons(
            snapshot.ButtonA ? ButtonState.Pressed : ButtonState.Released,
            ButtonState.Released,
            snapshot.ButtonBack ? ButtonState.Pressed : ButtonState.Released,
            snapshot.ButtonStart ? ButtonState.Pressed : ButtonState.Released);
        DPad = new GamePadDPad(
            snapshot.DPadUp ? ButtonState.Pressed : ButtonState.Released,
            snapshot.DPadDown ? ButtonState.Pressed : ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released);
        ThumbSticks = new GamePadThumbSticks(new Vector2(snapshot.LeftStickX, snapshot.LeftStickY));
    }

    public bool IsConnected => true;

    public GamePadButtons Buttons { get; }

    public GamePadDPad DPad { get; }

    public GamePadThumbSticks ThumbSticks { get; }
}

public readonly struct GamePadButtons
{
    internal GamePadButtons(ButtonState a, ButtonState b, ButtonState back, ButtonState start)
    {
        A = a;
        B = b;
        Back = back;
        Start = start;
    }

    public ButtonState A { get; }

    public ButtonState B { get; }

    public ButtonState X => ButtonState.Released;

    public ButtonState Y => ButtonState.Released;

    public ButtonState Back { get; }

    public ButtonState Start { get; }
}

public readonly struct GamePadDPad
{
    internal GamePadDPad(ButtonState up, ButtonState down, ButtonState left, ButtonState right)
    {
        Up = up;
        Down = down;
        Left = left;
        Right = right;
    }

    public ButtonState Up { get; }

    public ButtonState Down { get; }

    public ButtonState Left { get; }

    public ButtonState Right { get; }
}

public readonly struct GamePadThumbSticks
{
    internal GamePadThumbSticks(Vector2 left)
    {
        Left = left;
        Right = Vector2.Zero;
    }

    public Vector2 Left { get; }

    public Vector2 Right { get; }
}

public enum TouchLocationState
{
    Invalid = 0,
    Released = 1,
    Pressed = 2,
    Moved = 3,
}

public static class TouchPanel
{
    public const int DisplayWidth = 480;
    public const int DisplayHeight = 272;

    private static readonly Dictionary<int, Vector2> PreviousPositions = new();

    public static TouchCollection GetState()
    {
        InputSnapshot snapshot = PlatformHost.Input.Snapshot(PlatformHost.FrameCount);
        var locations = new TouchLocation[snapshot.Touches.Count];
        var present = new HashSet<int>();

        for (int i = 0; i < snapshot.Touches.Count; i++)
        {
            TouchPoint point = snapshot.Touches[i];
            var position = new Vector2(point.X, point.Y);
            var state = (TouchLocationState)point.State;

            // XNA keeps per-id history: the previous location is the same position on
            // a fresh press, otherwise the position reported on the previous frame.
            Vector2 previous = PreviousPositions.TryGetValue(point.Id, out Vector2 stored) && state != TouchLocationState.Pressed
                ? stored
                : position;

            locations[i] = new TouchLocation(point.Id, state, position, previous, point.Pressure);
            PreviousPositions[point.Id] = position;
            present.Add(point.Id);
        }

        if (PreviousPositions.Count > present.Count)
        {
            foreach (int stale in PreviousPositions.Keys.Where(id => !present.Contains(id)).ToArray())
            {
                PreviousPositions.Remove(stale);
            }
        }

        return new TouchCollection(locations);
    }
}

public struct TouchCollection : IReadOnlyList<TouchLocation>
{
    private readonly TouchLocation[]? _locations;

    internal TouchCollection(TouchLocation[] locations) => _locations = locations;

    public int Count => _locations?.Length ?? 0;

    public TouchLocation this[int index] => (_locations ?? throw new InvalidOperationException())[index];

    public IEnumerator<TouchLocation> GetEnumerator() =>
        ((IEnumerable<TouchLocation>)(_locations ?? Array.Empty<TouchLocation>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public readonly struct TouchLocation
{
    private readonly Vector2 _previous;

    internal TouchLocation(int id, TouchLocationState state, Vector2 position, Vector2 previous, float pressure)
    {
        Id = id;
        State = state;
        Position = position;
        Pressure = pressure;
        _previous = previous;
    }

    public int Id { get; }

    public TouchLocationState State { get; }

    public Vector2 Position { get; }

    public float Pressure { get; }

    public bool TryGetPreviousLocation(out TouchLocation previousLocation)
    {
        previousLocation = new TouchLocation(Id, State, _previous, _previous, Pressure);
        return true;
    }
}

public static class Accelerometer
{
    public static AccelerometerState GetState()
    {
        InputSnapshot snapshot = PlatformHost.Input.Snapshot(PlatformHost.FrameCount);
        return new AccelerometerState(new Vector3(snapshot.LeftStickX, snapshot.LeftStickY, 0f));
    }
}

public readonly struct AccelerometerState
{
    internal AccelerometerState(Vector3 acceleration) => Acceleration = acceleration;

    public Vector3 Acceleration { get; }
}
