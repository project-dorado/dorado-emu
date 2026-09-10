using Dorado.Platform;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework;

/// <summary>The XNA game host: owns the content manager, graphics device and run loop.</summary>
public class Game : IDisposable
{
    private bool _exitRequested;

    public Game()
    {
        Content = new ContentManager();
        Window = new GameWindow();
        GraphicsDevice = new GraphicsDevice(PlatformHost.Graphics);
    }

    public ContentManager Content { get; }

    public GameWindow Window { get; }

    public GraphicsDevice GraphicsDevice { get; }

    public bool IsFixedTimeStep { get; set; } = true;

    public TimeSpan TargetElapsedTime { get; set; } = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 60);

    public bool IsMouseVisible { get; set; }

    public bool IsRunning { get; private set; }

    public void Run()
    {
        if (IsRunning)
        {
            throw new InvalidOperationException("The game is already running.");
        }

        IsRunning = true;
        try
        {
            Initialize();
            LoadContent();

            TimeSpan total = TimeSpan.Zero;
            long frame = 0;
            while (!_exitRequested && !PlatformHost.ExitRequested)
            {
                if (!PlatformHost.RunForever && frame >= PlatformHost.FrameLimit)
                {
                    break;
                }

                TimeSpan elapsed = PlatformHost.FixedTimeStep ? PlatformHost.FrameTime : PlatformHost.FrameTime;
                total += elapsed;
                var gameTime = new GameTime(total, elapsed, total, elapsed);

                PlatformHost.OnFrame(frame);
                Update(gameTime);
                if (BeginDraw())
                {
                    Draw(gameTime);
                    EndDraw();
                }

                PlatformHost.Graphics.Present();
                PlatformHost.OnFrameRendered?.Invoke((int)frame);
                frame++;
            }

            UnloadContent();
        }
        finally
        {
            IsRunning = false;
        }
    }

    public void Exit() => _exitRequested = true;

    public void SuppressDraw()
    {
    }

    public void Dispose()
    {
        UnloadContent();
        GC.SuppressFinalize(this);
    }

    protected virtual void Initialize()
    {
    }

    protected virtual void LoadContent()
    {
    }

    protected virtual void UnloadContent()
    {
    }

    protected virtual void Update(GameTime gameTime)
    {
    }

    protected virtual void Draw(GameTime gameTime)
    {
    }

    protected virtual bool BeginDraw() => true;

    protected virtual void EndDraw()
    {
    }

    protected virtual void OnExiting(object sender, EventArgs args)
    {
    }
}

/// <summary>Timing information for a single frame.</summary>
public class GameTime
{
    public GameTime(TimeSpan totalGameTime, TimeSpan elapsedGameTime, TimeSpan totalRealTime, TimeSpan elapsedRealTime)
    {
        TotalGameTime = totalGameTime;
        ElapsedGameTime = elapsedGameTime;
        TotalRealTime = totalRealTime;
        ElapsedRealTime = elapsedRealTime;
    }

    public TimeSpan TotalGameTime { get; }

    public TimeSpan ElapsedGameTime { get; }

    public TimeSpan TotalRealTime { get; }

    public TimeSpan ElapsedRealTime { get; }

    public bool IsRunningSlowly { get; set; }
}

/// <summary>The host window.</summary>
public class GameWindow
{
    public string Title { get; set; } = string.Empty;

    public bool AllowUserResizing { get; set; }

    public Rectangle ClientBounds => new(0, 0, 480, 272);

    public IntPtr Handle => IntPtr.Zero;
}

/// <summary>Owns swap-chain settings; the device itself is created by <see cref="Game"/>.</summary>
public class GraphicsDeviceManager : IDisposable
{
    private readonly Game _game;

    public GraphicsDeviceManager(Game game)
    {
        _game = game ?? throw new ArgumentNullException(nameof(game));
    }

    public GraphicsDevice GraphicsDevice => _game.GraphicsDevice;

    public bool SynchronizeWithVerticalRetrace { get; set; } = true;

    public bool IsFullScreen { get; set; }

    public int PreferredBackBufferWidth { get; set; } = 480;

    public int PreferredBackBufferHeight { get; set; } = 272;

    public void ApplyChanges()
    {
    }

    public void ToggleFullScreen()
    {
    }

    public void Dispose()
    {
    }
}
