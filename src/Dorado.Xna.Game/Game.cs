using Dorado.Platform;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework;

/// <summary>The XNA game host: owns services, components, the content manager and the run loop.</summary>
public class Game : IDisposable
{
    private readonly GameServiceContainer _services = new();
    private readonly GameComponentCollection _components = new();
    private readonly HashSet<IGameComponent> _initializedComponents = new();
    private bool _exitRequested;
    private bool _suppressDraw;
    private bool _initialized;
    private bool _disposed;
    private TimeSpan _totalTime;
    private long _frame;

    public Game()
    {
        Content = new ContentManager(Services);
        Window = new GameWindow();
        Window.Title = GetType().Assembly.GetName().Name ?? "Dorado";
        GraphicsDevice = new GraphicsDevice(PlatformHost.Graphics);
        _components.ComponentAdded += OnComponentAdded;
        _components.ComponentRemoved += OnComponentRemoved;
    }

    public GameComponentCollection Components => _components;

    public ContentManager Content { get; set; }

    public GraphicsDevice GraphicsDevice { get; }

    public TimeSpan InactiveSleepTime { get; set; } = TimeSpan.FromMilliseconds(20);

    public bool IsActive => IsRunning;

    public bool IsFixedTimeStep { get; set; } = true;

    public bool IsMouseVisible { get; set; }

    public bool IsRunning { get; private set; }

    public GameServiceContainer Services => _services;

    public TimeSpan TargetElapsedTime { get; set; } = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 60);

    public GameWindow Window { get; }

    public event EventHandler? Activated;

    public event EventHandler? Deactivated;

    public event EventHandler? Disposed;

    public event EventHandler? Exiting;

    public void Run()
    {
        if (IsRunning)
        {
            throw new InvalidOperationException("The game is already running.");
        }

        IsRunning = true;
        try
        {
            EnsureInitialized();
            BeginRun();
            Activated?.Invoke(this, EventArgs.Empty);
            while (!_exitRequested && !PlatformHost.ExitRequested)
            {
                if (!PlatformHost.RunForever && _frame >= PlatformHost.FrameLimit)
                {
                    break;
                }

                Tick();
            }

            EndRun();
            UnloadContent();
        }
        finally
        {
            Deactivated?.Invoke(this, EventArgs.Empty);
            IsRunning = false;
        }
    }

    public void Tick()
    {
        EnsureInitialized();

        TimeSpan elapsed = PlatformHost.FixedTimeStep ? PlatformHost.FrameTime : PlatformHost.FrameTime;
        _totalTime += elapsed;
        var gameTime = new GameTime(_totalTime, elapsed, _totalTime, elapsed);

        PlatformHost.OnFrame(_frame);
        Update(gameTime);

        bool suppressDraw = _suppressDraw;
        _suppressDraw = false;
        if (!suppressDraw && BeginDraw())
        {
            Draw(gameTime);
            EndDraw();
        }

        PlatformHost.Graphics.Present();
        PlatformHost.OnFrameRendered?.Invoke((int)_frame);
        _frame++;
    }

    public void SuppressDraw() => _suppressDraw = true;

    public void Exit()
    {
        OnExiting(this, EventArgs.Empty);
        Exiting?.Invoke(this, EventArgs.Empty);
        _exitRequested = true;
    }

    protected virtual void BeginRun()
    {
    }

    protected virtual void EndRun()
    {
    }

    protected virtual void Update(GameTime gameTime)
    {
        foreach (IUpdateable updateable in Components.OfType<IUpdateable>().OrderBy(c => c.UpdateOrder).ToArray())
        {
            if (updateable.Enabled)
            {
                updateable.Update(gameTime);
            }
        }
    }

    protected virtual bool BeginDraw() => true;

    protected virtual void Draw(GameTime gameTime)
    {
        foreach (IDrawable drawable in Components.OfType<IDrawable>().OrderBy(c => c.DrawOrder).ToArray())
        {
            if (drawable.Visible)
            {
                drawable.Draw(gameTime);
            }
        }
    }

    protected virtual void EndDraw()
    {
    }

    protected virtual void Initialize()
    {
    }

    public void ResetElapsedTime() => _totalTime = TimeSpan.Zero;

    protected virtual void OnActivated(object sender, EventArgs args)
    {
    }

    protected virtual void OnDeactivated(object sender, EventArgs args)
    {
    }

    protected virtual void OnExiting(object sender, EventArgs args)
    {
    }

    [Obsolete("The LoadGraphicsContent method is obsolete and will be removed in the future. Use the LoadContent method instead.")]
    protected virtual void LoadGraphicsContent(bool loadAllContent) => LoadContent();

    [Obsolete("The UnloadGraphicsContent method is obsolete and will be removed in the future. Use the UnloadContent method instead.")]
    protected virtual void UnloadGraphicsContent(bool unloadAllContent) => UnloadContent();

    protected virtual void LoadContent()
    {
    }

    protected virtual void UnloadContent()
    {
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Dispose(true);
        GC.SuppressFinalize(this);
        Disposed?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        foreach (GameComponent component in Components.OfType<GameComponent>().ToArray())
        {
            component.Dispose();
        }

        Content?.Dispose();
    }

    protected virtual bool ShowMissingRequirementMessage(Exception exception) => false;

    private void EnsureInitialized()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        Initialize();
        foreach (IGameComponent component in Components.ToArray())
        {
            InitializeComponent(component);
        }

        LoadContent();
    }

    private void InitializeComponent(IGameComponent component)
    {
        if (_initializedComponents.Add(component))
        {
            component.Initialize();
        }
    }

    private void OnComponentAdded(object? sender, GameComponentCollectionEventArgs args)
    {
        if (_initialized)
        {
            InitializeComponent(args.GameComponent);
        }
    }

    private void OnComponentRemoved(object? sender, GameComponentCollectionEventArgs args)
    {
        _initializedComponents.Remove(args.GameComponent);
    }
}

/// <summary>Timing information for a single frame.</summary>
public class GameTime
{
    public GameTime()
    {
    }

    public GameTime(TimeSpan totalRealTime, TimeSpan elapsedRealTime, TimeSpan totalGameTime, TimeSpan elapsedGameTime)
        : this(totalRealTime, elapsedRealTime, totalGameTime, elapsedGameTime, false)
    {
    }

    public GameTime(
        TimeSpan totalRealTime,
        TimeSpan elapsedRealTime,
        TimeSpan totalGameTime,
        TimeSpan elapsedGameTime,
        bool isRunningSlowly)
    {
        TotalRealTime = totalRealTime;
        ElapsedRealTime = elapsedRealTime;
        TotalGameTime = totalGameTime;
        ElapsedGameTime = elapsedGameTime;
        IsRunningSlowly = isRunningSlowly;
    }

    public TimeSpan ElapsedGameTime { get; }

    public TimeSpan ElapsedRealTime { get; }

    public bool IsRunningSlowly { get; }

    public TimeSpan TotalGameTime { get; }

    public TimeSpan TotalRealTime { get; }
}

/// <summary>The host window. Dorado's window is a fixed 480x272 surface.</summary>
public class GameWindow
{
    public virtual bool AllowUserResizing { get; set; }

    public virtual Rectangle ClientBounds => new(0, 0, 480, 272);

    public virtual IntPtr Handle => IntPtr.Zero;

    public virtual string ScreenDeviceName => "Dorado";

    public string Title
    {
        get => _title;
        set
        {
            if (_title != value)
            {
                _title = value;
                SetTitle(value);
            }
        }
    }

    public event EventHandler? ClientSizeChanged;

    public event EventHandler? ScreenDeviceNameChanged;

    public virtual void BeginScreenDeviceChange(bool willBeFullScreen)
    {
    }

    public virtual void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
    {
    }

    public void EndScreenDeviceChange(string screenDeviceName) =>
        EndScreenDeviceChange(screenDeviceName, ClientBounds.Width, ClientBounds.Height);

    protected virtual void SetTitle(string title)
    {
    }

    protected void OnActivated() => Activated?.Invoke(this, EventArgs.Empty);

    protected void OnDeactivated() => Deactivated?.Invoke(this, EventArgs.Empty);

    protected void OnPaint() => Paint?.Invoke(this, EventArgs.Empty);

    protected void OnScreenDeviceNameChanged() => ScreenDeviceNameChanged?.Invoke(this, EventArgs.Empty);

    protected void OnClientSizeChanged() => ClientSizeChanged?.Invoke(this, EventArgs.Empty);

    /// <summary>Raised by <see cref="OnActivated"/>; matched to the XNA window host surface.</summary>
    public event EventHandler? Activated;

    /// <summary>Raised by <see cref="OnDeactivated"/>; matched to the XNA window host surface.</summary>
    public event EventHandler? Deactivated;

    /// <summary>Raised by <see cref="OnPaint"/>; matched to the XNA window host surface.</summary>
    public event EventHandler? Paint;

    private string _title = string.Empty;
}

/// <summary>Owns swap-chain settings and exposes the game's device as a service.</summary>
public class GraphicsDeviceManager : IGraphicsDeviceService, IDisposable, IGraphicsDeviceManager
{
    public static readonly int DefaultBackBufferHeight = 272;

    public static readonly int DefaultBackBufferWidth = 480;

    public static readonly SurfaceFormat[] ValidAdapterFormats =
    {
        SurfaceFormat.Color,
    };

    public static readonly SurfaceFormat[] ValidBackBufferFormats =
    {
        SurfaceFormat.Color,
    };

    public static readonly DeviceType[] ValidDeviceTypes =
    {
        DeviceType.Default,
        DeviceType.Hardware,
    };

    private readonly Game _game;
    private bool _disposed;

    public GraphicsDeviceManager(Game game)
    {
        _game = game ?? throw new ArgumentNullException(nameof(game));
        game.Services.AddService(typeof(IGraphicsDeviceService), this);
        game.Services.AddService(typeof(IGraphicsDeviceManager), this);
    }

    public GraphicsDevice GraphicsDevice => _game.GraphicsDevice;

    public bool IsFullScreen { get; set; }

    public bool PreferMultiSampling { get; set; }

    public SurfaceFormat PreferredBackBufferFormat { get; set; } = SurfaceFormat.Color;

    public int PreferredBackBufferHeight { get; set; } = DefaultBackBufferHeight;

    public int PreferredBackBufferWidth { get; set; } = DefaultBackBufferWidth;

    public DepthFormat PreferredDepthStencilFormat { get; set; } = DepthFormat.Depth24;

    public ShaderProfile MinimumPixelShaderProfile { get; set; } = ShaderProfile.ShaderModel2_0;

    public ShaderProfile MinimumVertexShaderProfile { get; set; } = ShaderProfile.ShaderModel2_0;

    public bool SynchronizeWithVerticalRetrace { get; set; } = true;

    public event EventHandler? DeviceCreated;

    public event EventHandler? DeviceDisposing;

    public event EventHandler? DeviceReset;

    public event EventHandler? DeviceResetting;

    public event EventHandler? Disposed;

    public event EventHandler<PreparingDeviceSettingsEventArgs>? PreparingDeviceSettings;

    public void ApplyChanges()
    {
        OnDeviceResetting(this, EventArgs.Empty);
        OnPreparingDeviceSettings(this, new PreparingDeviceSettingsEventArgs(new GraphicsDeviceInformation()));
        OnDeviceReset(this, EventArgs.Empty);
    }

    public void ToggleFullScreen() => IsFullScreen = !IsFullScreen;

    void IGraphicsDeviceManager.CreateDevice() => OnDeviceCreated(this, EventArgs.Empty);

    bool IGraphicsDeviceManager.BeginDraw() => true;

    void IGraphicsDeviceManager.EndDraw()
    {
    }

    void IDisposable.Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Dispose(true);
        GC.SuppressFinalize(this);
        Disposed?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnDeviceCreated(object sender, EventArgs args) =>
        DeviceCreated?.Invoke(sender, args);

    protected virtual void OnDeviceDisposing(object sender, EventArgs args) =>
        DeviceDisposing?.Invoke(sender, args);

    protected virtual void OnDeviceReset(object sender, EventArgs args) =>
        DeviceReset?.Invoke(sender, args);

    protected virtual void OnDeviceResetting(object sender, EventArgs args) =>
        DeviceResetting?.Invoke(sender, args);

    protected virtual void OnPreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs args) =>
        PreparingDeviceSettings?.Invoke(sender, args);

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            OnDeviceDisposing(this, EventArgs.Empty);
        }
    }
}

/// <summary>The device configuration chosen for the game.</summary>
public class GraphicsDeviceInformation
{
    public GraphicsAdapter Adapter { get; set; } = GraphicsAdapter.DefaultAdapter;

    public DeviceType DeviceType { get; set; } = DeviceType.Hardware;

    public PresentationParameters PresentationParameters { get; set; } = new();

    public GraphicsDeviceInformation Clone() => new()
    {
        Adapter = Adapter,
        DeviceType = DeviceType,
        PresentationParameters = PresentationParameters,
    };

    public override bool Equals(object? obj) =>
        obj is GraphicsDeviceInformation other &&
        ReferenceEquals(Adapter, other.Adapter) &&
        DeviceType == other.DeviceType &&
        ReferenceEquals(PresentationParameters, other.PresentationParameters);

    public override int GetHashCode() => HashCode.Combine(Adapter, DeviceType, PresentationParameters);
}

/// <summary>Raised just before the device is (re)created so callers can adjust settings.</summary>
public class PreparingDeviceSettingsEventArgs : EventArgs
{
    public PreparingDeviceSettingsEventArgs(GraphicsDeviceInformation graphicsDeviceInformation)
    {
        GraphicsDeviceInformation = graphicsDeviceInformation ??
            throw new ArgumentNullException(nameof(graphicsDeviceInformation));
    }

    public GraphicsDeviceInformation GraphicsDeviceInformation { get; }
}

/// <summary>Raised when no supported graphics device configuration exists.</summary>
public class NoSuitableGraphicsDeviceException : ApplicationException
{
    public NoSuitableGraphicsDeviceException(string message)
        : base(message)
    {
    }

    public NoSuitableGraphicsDeviceException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
