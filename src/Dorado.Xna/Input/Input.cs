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

/// <summary>Controller buttons; values match XNA 3.1 (XInput masks).</summary>
[Flags]
public enum Buttons
{
    None = 0,
    DPadUp = 0x0001,
    DPadDown = 0x0002,
    DPadLeft = 0x0004,
    DPadRight = 0x0008,
    Start = 0x0010,
    Back = 0x0020,
    LeftStick = 0x0040,
    RightStick = 0x0080,
    LeftShoulder = 0x0100,
    RightShoulder = 0x0200,
    BigButton = 0x0800,
    A = 0x1000,
    B = 0x2000,
    X = 0x4000,
    Y = 0x8000,
    LeftTrigger = 0x800000,
    RightTrigger = 0x400000,
    LeftThumbstickUp = 0x10000000,
    LeftThumbstickDown = 0x20000000,
    LeftThumbstickLeft = 0x00200000,
    LeftThumbstickRight = 0x40000000,
    RightThumbstickUp = 0x01000000,
    RightThumbstickDown = 0x02000000,
    RightThumbstickLeft = 0x08000000,
    RightThumbstickRight = 0x04000000,
}

/// <summary>The kind of controller attached to a player slot.</summary>
public enum GamePadType
{
    Unknown = 0,
    GamePad = 1,
    Wheel = 2,
    ArcadeStick = 3,
    FlightStick = 4,
    DancePad = 5,
    Guitar = 6,
    AlternateGuitar = 7,
    DrumKit = 8,
    BigButtonPad = 768,
}

public static class GamePad
{
    public static GamePadState GetState(PlayerIndex playerIndex) => GetState(playerIndex, GamePadDeadZone.None);

    public static GamePadState GetState(PlayerIndex playerIndex, GamePadDeadZone deadZone)
    {
        InputSnapshot snapshot = PlatformHost.Input.Snapshot(PlatformHost.FrameCount);
        return new GamePadState(snapshot);
    }

    public static GamePadCapabilities GetCapabilities(PlayerIndex playerIndex) =>
        new(GamePadType.GamePad, connected: true);
}

public enum GamePadDeadZone
{
    None = 0,
    IndependentAxes = 1,
    Circular = 2,
}

public readonly struct GamePadState
{
    private readonly InputSnapshot? _snapshot;

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
        Triggers = new GamePadTriggers(0f, 0f);
    }

    public GamePadState(
        GamePadThumbSticks thumbSticks,
        GamePadTriggers triggers,
        GamePadButtons buttons,
        GamePadDPad dPad)
    {
        _snapshot = null;
        ThumbSticks = thumbSticks;
        Triggers = triggers;
        Buttons = buttons;
        DPad = dPad;
    }

    public GamePadState(
        Vector2 leftThumbStick,
        Vector2 rightThumbStick,
        float leftTrigger,
        float rightTrigger,
        params Buttons[] buttons)
    {
        _snapshot = null;
        Buttons combined = Combine(buttons);
        ThumbSticks = new GamePadThumbSticks(leftThumbStick, rightThumbStick);
        Triggers = new GamePadTriggers(leftTrigger, rightTrigger);
        Buttons = new GamePadButtons(combined);
        DPad = new GamePadDPad(
            (combined & Input.Buttons.DPadUp) != 0 ? ButtonState.Pressed : ButtonState.Released,
            (combined & Input.Buttons.DPadDown) != 0 ? ButtonState.Pressed : ButtonState.Released,
            (combined & Input.Buttons.DPadLeft) != 0 ? ButtonState.Pressed : ButtonState.Released,
            (combined & Input.Buttons.DPadRight) != 0 ? ButtonState.Pressed : ButtonState.Released);
    }

    public bool IsConnected => true;

    public int PacketNumber => _snapshot is null ? 0 : (int)PlatformHost.FrameCount;

    public GamePadButtons Buttons { get; }

    public GamePadDPad DPad { get; }

    public GamePadThumbSticks ThumbSticks { get; }

    public GamePadTriggers Triggers { get; }

    public bool IsButtonDown(Input.Buttons button)
    {
        return button switch
        {
            Input.Buttons.A => Buttons.A == ButtonState.Pressed,
            Input.Buttons.B => Buttons.B == ButtonState.Pressed,
            Input.Buttons.X => Buttons.X == ButtonState.Pressed,
            Input.Buttons.Y => Buttons.Y == ButtonState.Pressed,
            Input.Buttons.Back => Buttons.Back == ButtonState.Pressed,
            Input.Buttons.Start => Buttons.Start == ButtonState.Pressed,
            Input.Buttons.BigButton => Buttons.BigButton == ButtonState.Pressed,
            Input.Buttons.LeftShoulder => Buttons.LeftShoulder == ButtonState.Pressed,
            Input.Buttons.RightShoulder => Buttons.RightShoulder == ButtonState.Pressed,
            Input.Buttons.LeftStick => Buttons.LeftStick == ButtonState.Pressed,
            Input.Buttons.RightStick => Buttons.RightStick == ButtonState.Pressed,
            Input.Buttons.DPadUp => DPad.Up == ButtonState.Pressed,
            Input.Buttons.DPadDown => DPad.Down == ButtonState.Pressed,
            Input.Buttons.DPadLeft => DPad.Left == ButtonState.Pressed,
            Input.Buttons.DPadRight => DPad.Right == ButtonState.Pressed,
            Input.Buttons.LeftTrigger => Triggers.Left > 0f,
            Input.Buttons.RightTrigger => Triggers.Right > 0f,
            Input.Buttons.LeftThumbstickUp => ThumbSticks.Left.Y > 0f,
            Input.Buttons.LeftThumbstickDown => ThumbSticks.Left.Y < 0f,
            Input.Buttons.LeftThumbstickLeft => ThumbSticks.Left.X < 0f,
            Input.Buttons.LeftThumbstickRight => ThumbSticks.Left.X > 0f,
            Input.Buttons.RightThumbstickUp => ThumbSticks.Right.Y > 0f,
            Input.Buttons.RightThumbstickDown => ThumbSticks.Right.Y < 0f,
            Input.Buttons.RightThumbstickLeft => ThumbSticks.Right.X < 0f,
            Input.Buttons.RightThumbstickRight => ThumbSticks.Right.X > 0f,
            _ => false,
        };
    }

    public bool IsButtonUp(Input.Buttons button) => !IsButtonDown(button);

    private static Input.Buttons Combine(Input.Buttons[]? buttons)
    {
        Input.Buttons combined = Input.Buttons.None;
        foreach (Input.Buttons button in buttons ?? Array.Empty<Input.Buttons>())
        {
            combined |= button;
        }

        return combined;
    }
}

/// <summary>Which inputs a controller supports; Dorado reports a standard pad.</summary>
public readonly struct GamePadCapabilities
{
    private readonly bool _connected;

    internal GamePadCapabilities(GamePadType gamePadType, bool connected)
    {
        GamePadType = gamePadType;
        _connected = connected;
    }

    public bool IsConnected => _connected;

    public GamePadType GamePadType { get; }

    public bool HasAButton => true;

    public bool HasBButton => true;

    public bool HasXButton => true;

    public bool HasYButton => true;

    public bool HasBackButton => true;

    public bool HasStartButton => true;

    public bool HasBigButton => false;

    public bool HasLeftShoulderButton => false;

    public bool HasRightShoulderButton => false;

    public bool HasLeftStick => true;

    public bool HasRightStick => false;

    public bool HasDPadDownButton => true;

    public bool HasDPadLeftButton => true;

    public bool HasDPadRightButton => true;

    public bool HasDPadUpButton => true;

    public bool HasLeftTrigger => false;

    public bool HasRightTrigger => false;

    public bool HasLeftVibrationMotor => false;

    public bool HasRightVibrationMotor => false;

    public bool HasVoiceSupport => false;
}

public readonly struct GamePadButtons
{
    private readonly Buttons _buttons;

    public GamePadButtons(Buttons buttons) => _buttons = buttons;

    internal GamePadButtons(ButtonState a, ButtonState b, ButtonState back, ButtonState start)
    {
        Buttons value = Buttons.None;
        if (a == ButtonState.Pressed)
        {
            value |= Buttons.A;
        }

        if (b == ButtonState.Pressed)
        {
            value |= Buttons.B;
        }

        if (back == ButtonState.Pressed)
        {
            value |= Buttons.Back;
        }

        if (start == ButtonState.Pressed)
        {
            value |= Buttons.Start;
        }

        _buttons = value;
    }

    public ButtonState A => State(Buttons.A);

    public ButtonState B => State(Buttons.B);

    public ButtonState X => State(Buttons.X);

    public ButtonState Y => State(Buttons.Y);

    public ButtonState LeftShoulder => State(Buttons.LeftShoulder);

    public ButtonState RightShoulder => State(Buttons.RightShoulder);

    public ButtonState Back => State(Buttons.Back);

    public ButtonState Start => State(Buttons.Start);

    public ButtonState BigButton => State(Buttons.BigButton);

    public ButtonState LeftStick => State(Buttons.LeftStick);

    public ButtonState RightStick => State(Buttons.RightStick);

    private ButtonState State(Buttons button) =>
        (_buttons & button) == button ? ButtonState.Pressed : ButtonState.Released;
}

public readonly struct GamePadTriggers
{
    public GamePadTriggers(float leftTrigger, float rightTrigger)
    {
        Left = Math.Clamp(leftTrigger, 0f, 1f);
        Right = Math.Clamp(rightTrigger, 0f, 1f);
    }

    public float Left { get; }

    public float Right { get; }

    public readonly bool Equals(GamePadTriggers other) => Left.Equals(other.Left) && Right.Equals(other.Right);

    public override readonly bool Equals(object? obj) => obj is GamePadTriggers other && Equals(other);

    public override readonly int GetHashCode() => HashCode.Combine(Left, Right);

    public override readonly string ToString() => $"{{Left:{Left} Right:{Right}}}";

    public static bool operator ==(GamePadTriggers left, GamePadTriggers right) => left.Equals(right);

    public static bool operator !=(GamePadTriggers left, GamePadTriggers right) => !left.Equals(right);
}

public readonly struct GamePadDPad
{
    public GamePadDPad(ButtonState upValue, ButtonState downValue, ButtonState leftValue, ButtonState rightValue)
    {
        Up = upValue;
        Down = downValue;
        Left = leftValue;
        Right = rightValue;
    }

    public ButtonState Up { get; }

    public ButtonState Down { get; }

    public ButtonState Left { get; }

    public ButtonState Right { get; }
}

public readonly struct GamePadThumbSticks
{
    internal GamePadThumbSticks(Vector2 left)
        : this(left, Vector2.Zero)
    {
    }

    public GamePadThumbSticks(Vector2 leftThumbstick, Vector2 rightThumbstick)
    {
        Left = leftThumbstick;
        Right = rightThumbstick;
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

    public static TouchPanelCapabilities GetCapabilities() => new();

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

    public bool FindById(int id, out TouchLocation touchLocation)
    {
        if (_locations is not null)
        {
            foreach (TouchLocation location in _locations)
            {
                if (location.Id == id)
                {
                    touchLocation = location;
                    return true;
                }
            }
        }

        touchLocation = default;
        return false;
    }

    public IEnumerator<TouchLocation> GetEnumerator() =>
        ((IEnumerable<TouchLocation>)(_locations ?? Array.Empty<TouchLocation>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>The touch capabilities of the fixed Zune HD panel.</summary>
public readonly struct TouchPanelCapabilities
{
    public bool IsConnected => true;

    public bool HasPressure => true;

    public int MaximumTouchCount => 4;
}

public readonly struct TouchLocation
{
    private readonly Vector2 _previous;

    public TouchLocation(int id, TouchLocationState state, Vector2 position, float pressure)
        : this(id, state, position, pressure, state, position, pressure)
    {
    }

    public TouchLocation(
        int id,
        TouchLocationState state,
        Vector2 position,
        float pressure,
        TouchLocationState previousState,
        Vector2 previousPosition,
        float previousPressure)
    {
        Id = id;
        State = state;
        Position = position;
        Pressure = pressure;
        PreviousState = previousState;
        _previous = previousPosition;
        PreviousPressure = previousPressure;
    }

    internal TouchLocation(int id, TouchLocationState state, Vector2 position, Vector2 previous, float pressure)
        : this(id, state, position, pressure, state, previous, pressure)
    {
    }

    public int Id { get; }

    public TouchLocationState State { get; }

    public Vector2 Position { get; }

    public float Pressure { get; }

    public TouchLocationState PreviousState { get; }

    public float PreviousPressure { get; }

    public bool TryGetPreviousLocation(out TouchLocation previousLocation)
    {
        previousLocation = new TouchLocation(
            Id, PreviousState, _previous, PreviousPressure, PreviousState, _previous, PreviousPressure);
        return State != TouchLocationState.Pressed;
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
    public AccelerometerState(Vector3 acceleration)
    {
        Acceleration = acceleration;
        IsConnected = true;
    }

    internal AccelerometerState(Vector3 acceleration, bool isConnected)
    {
        Acceleration = acceleration;
        IsConnected = isConnected;
    }

    public Vector3 Acceleration { get; }

    public bool IsConnected { get; }

    /// <summary>
    /// Builds the rotation matrix that maps the device frame onto gravity.
    /// A zero or degenerate reading yields the identity rotation.
    /// </summary>
    public readonly Matrix GetRotation()
    {
        Vector3 down = Acceleration;
        if (down.LengthSquared() < 1e-8f)
        {
            return Matrix.Identity;
        }

        Vector3 z = Vector3.Normalize(down);
        Vector3 reference = MathF.Abs(z.Y) > 0.999f ? Vector3.UnitX : Vector3.UnitY;
        Vector3 x = Vector3.Normalize(Vector3.Cross(reference, z));
        Vector3 y = Vector3.Cross(z, x);

        return new Matrix(
            x.X, x.Y, x.Z, 0f,
            y.X, y.Y, y.Z, 0f,
            z.X, z.Y, z.Z, 0f,
            0f, 0f, 0f, 1f);
    }
}

/// <summary>The host mouse; on Zune HD there is no mouse, so it mirrors touch.</summary>
public static class Mouse
{
    public static IntPtr WindowHandle { get; set; }

    public static MouseState GetState()
    {
        InputSnapshot snapshot = PlatformHost.Input.Snapshot(PlatformHost.FrameCount);
        if (snapshot.Touches.Count == 0)
        {
            return new MouseState(0, 0, 0, ButtonState.Released, ButtonState.Released, ButtonState.Released);
        }

        TouchPoint point = snapshot.Touches[0];
        ButtonState pressed = point.State is TouchState.Pressed or TouchState.Moved
            ? ButtonState.Pressed
            : ButtonState.Released;
        return new MouseState((int)point.X, (int)point.Y, 0, pressed, ButtonState.Released, ButtonState.Released);
    }

    public static MouseState GetState(PlayerIndex playerIndex) => GetState();

    public static void SetPosition(int x, int y)
    {
    }
}

/// <summary>Mouse buttons and cursor position; a value type matching XNA 3.1.</summary>
public readonly struct MouseState
{
    public MouseState(
        int x,
        int y,
        int scrollWheel,
        ButtonState leftButton,
        ButtonState middleButton,
        ButtonState rightButton)
        : this(
            x,
            y,
            scrollWheel,
            leftButton,
            middleButton,
            rightButton,
            ButtonState.Released,
            ButtonState.Released)
    {
    }

    public MouseState(
        int x,
        int y,
        int scrollWheel,
        ButtonState leftButton,
        ButtonState middleButton,
        ButtonState rightButton,
        ButtonState xButton1,
        ButtonState xButton2)
    {
        X = x;
        Y = y;
        ScrollWheelValue = scrollWheel;
        LeftButton = leftButton;
        MiddleButton = middleButton;
        RightButton = rightButton;
        XButton1 = xButton1;
        XButton2 = xButton2;
    }

    public int X { get; }

    public int Y { get; }

    public int ScrollWheelValue { get; }

    public ButtonState LeftButton { get; }

    public ButtonState MiddleButton { get; }

    public ButtonState RightButton { get; }

    public ButtonState XButton1 { get; }

    public ButtonState XButton2 { get; }
}
