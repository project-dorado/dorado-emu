using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework;

/// <summary>A component that can be initialized by the game.</summary>
public interface IGameComponent
{
    void Initialize();
}

/// <summary>A component that updates once per frame.</summary>
public interface IUpdateable
{
    bool Enabled { get; }

    int UpdateOrder { get; }

    event EventHandler? EnabledChanged;

    event EventHandler? UpdateOrderChanged;

    void Update(GameTime gameTime);
}

/// <summary>A component that draws once per frame.</summary>
public interface IDrawable
{
    int DrawOrder { get; }

    bool Visible { get; }

    event EventHandler? DrawOrderChanged;

    event EventHandler? VisibleChanged;

    void Draw(GameTime gameTime);
}

/// <summary>Carries the component involved in a collection change.</summary>
public class GameComponentCollectionEventArgs : EventArgs
{
    public GameComponentCollectionEventArgs(IGameComponent gameComponent)
    {
        GameComponent = gameComponent ?? throw new ArgumentNullException(nameof(gameComponent));
    }

    public IGameComponent GameComponent { get; }
}

/// <summary>The ordered set of components owned by a <see cref="Game"/>.</summary>
public sealed class GameComponentCollection : Collection<IGameComponent>
{
    public event EventHandler<GameComponentCollectionEventArgs>? ComponentAdded;

    public event EventHandler<GameComponentCollectionEventArgs>? ComponentRemoved;

    protected override void InsertItem(int index, IGameComponent item)
    {
        ArgumentNullException.ThrowIfNull(item);
        base.InsertItem(index, item);
        ComponentAdded?.Invoke(this, new GameComponentCollectionEventArgs(item));
    }

    protected override void RemoveItem(int index)
    {
        IGameComponent item = this[index];
        base.RemoveItem(index);
        ComponentRemoved?.Invoke(this, new GameComponentCollectionEventArgs(item));
    }

    protected override void SetItem(int index, IGameComponent item)
    {
        ArgumentNullException.ThrowIfNull(item);
        IGameComponent previous = this[index];
        base.SetItem(index, item);
        ComponentRemoved?.Invoke(this, new GameComponentCollectionEventArgs(previous));
        ComponentAdded?.Invoke(this, new GameComponentCollectionEventArgs(item));
    }

    protected override void ClearItems()
    {
        IGameComponent[] removed = this.ToArray();
        base.ClearItems();
        foreach (IGameComponent item in removed)
        {
            ComponentRemoved?.Invoke(this, new GameComponentCollectionEventArgs(item));
        }
    }
}

/// <summary>A type-keyed service registry; the backing store for <see cref="Game.Services"/>.</summary>
public class GameServiceContainer : IServiceProvider
{
    private readonly Dictionary<Type, object> _services = new();

    public void AddService(Type type, object provider)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(provider);
        _services[type] = provider;
    }

    public void RemoveService(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        _services.Remove(type);
    }

    public object? GetService(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return _services.TryGetValue(type, out object? provider) ? provider : null;
    }

    public object? this[Type type]
    {
        get => GetService(type);
        set
        {
            ArgumentNullException.ThrowIfNull(type);
            if (value is null)
            {
                RemoveService(type);
            }
            else
            {
                AddService(type, value);
            }
        }
    }
}

/// <summary>A component with an update order and enabled state.</summary>
public class GameComponent : IGameComponent, IUpdateable, IDisposable
{
    private bool _enabled = true;
    private int _updateOrder;

    public GameComponent(Game game)
    {
        Game = game ?? throw new ArgumentNullException(nameof(game));
    }

    ~GameComponent()
    {
        Dispose(false);
    }

    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (_enabled == value)
            {
                return;
            }

            _enabled = value;
            OnEnabledChanged(this, EventArgs.Empty);
        }
    }

    public Game Game { get; }

    public int UpdateOrder
    {
        get => _updateOrder;
        set
        {
            if (_updateOrder == value)
            {
                return;
            }

            _updateOrder = value;
            OnUpdateOrderChanged(this, EventArgs.Empty);
        }
    }

    public event EventHandler? Disposed;

    public event EventHandler? EnabledChanged;

    public event EventHandler? UpdateOrderChanged;

    public virtual void Initialize()
    {
    }

    public virtual void Update(GameTime gameTime)
    {
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
        Disposed?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void Dispose(bool disposing)
    {
    }

    protected virtual void OnUpdateOrderChanged(object sender, EventArgs args) =>
        UpdateOrderChanged?.Invoke(sender, args);

    protected virtual void OnEnabledChanged(object sender, EventArgs args) =>
        EnabledChanged?.Invoke(sender, args);
}

/// <summary>A component that also draws.</summary>
public class DrawableGameComponent : GameComponent, IDrawable
{
    private int _drawOrder;
    private bool _visible = true;

    public DrawableGameComponent(Game game)
        : base(game)
    {
        GraphicsDevice = game.GraphicsDevice;
    }

    public int DrawOrder
    {
        get => _drawOrder;
        set
        {
            if (_drawOrder == value)
            {
                return;
            }

            _drawOrder = value;
            OnDrawOrderChanged(this, EventArgs.Empty);
        }
    }

    public GraphicsDevice GraphicsDevice { get; }

    public bool Visible
    {
        get => _visible;
        set
        {
            if (_visible == value)
            {
                return;
            }

            _visible = value;
            OnVisibleChanged(this, EventArgs.Empty);
        }
    }

    public event EventHandler? DrawOrderChanged;

    public event EventHandler? VisibleChanged;

    public override void Initialize()
    {
        base.Initialize();
        LoadContent();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            UnloadContent();
        }

        base.Dispose(disposing);
    }

    public virtual void Draw(GameTime gameTime)
    {
    }

    [Obsolete("The LoadGraphicsContent method is obsolete and will be removed in the future. Use the LoadContent method instead.")]
    protected virtual void LoadGraphicsContent(bool loadAllContent) =>
        LoadContent();

    [Obsolete("The UnloadGraphicsContent method is obsolete and will be removed in the future. Use the UnloadContent method instead.")]
    protected virtual void UnloadGraphicsContent(bool unloadAllContent) =>
        UnloadContent();

    protected virtual void LoadContent()
    {
    }

    protected virtual void UnloadContent()
    {
    }

    protected virtual void OnDrawOrderChanged(object sender, EventArgs args) =>
        DrawOrderChanged?.Invoke(sender, args);

    protected virtual void OnVisibleChanged(object sender, EventArgs args) =>
        VisibleChanged?.Invoke(sender, args);
}

/// <summary>Creates and presents the graphics device for a game.</summary>
public interface IGraphicsDeviceManager
{
    void CreateDevice();

    bool BeginDraw();

    void EndDraw();
}
