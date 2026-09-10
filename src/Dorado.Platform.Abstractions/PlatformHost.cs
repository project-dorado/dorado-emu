namespace Dorado.Platform;

/// <summary>
/// Ambient hooks the XNA shim uses to reach the host. The runner sets these before
/// launching an application; a single application runs per process at a time.
/// </summary>
public static class PlatformHost
{
    private static IGraphicsBackend? _graphics;
    private static bool _exitRequested;

    public static IGraphicsBackend Graphics
    {
        get => _graphics ?? throw new InvalidOperationException("No graphics backend has been assigned to PlatformHost.");
        set => _graphics = value;
    }

    public static bool HasGraphics => _graphics is not null;

    public static IInputSource Input { get; set; } = EmptyInputSource.Instance;

    /// <summary>When true the game loop advances by <see cref="FrameTime"/> rather than the wall clock.</summary>
    public static bool FixedTimeStep { get; set; } = true;

    public static TimeSpan FrameTime { get; set; } = TimeSpan.FromSeconds(1.0 / 60.0);

    /// <summary>Maximum frames to run before the loop stops (ignored when <see cref="RunForever"/>).</summary>
    public static int FrameLimit { get; set; } = 60;

    public static bool RunForever { get; set; }

    /// <summary>Invoked after each presented frame with the zero-based frame index.</summary>
    public static Action<int>? OnFrameRendered { get; set; }

    public static long FrameCount { get; private set; }

    public static bool ExitRequested => _exitRequested;

    public static void RequestExit() => _exitRequested = true;

    public static void ResetExit() => _exitRequested = false;

    public static void OnFrame(long frame) => FrameCount = frame + 1;
}
