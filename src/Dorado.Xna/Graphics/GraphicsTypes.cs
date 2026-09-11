using System.Collections.ObjectModel;
using Dorado.Platform;

namespace Microsoft.Xna.Framework.Graphics;

/// <summary>A device type; Dorado always runs on a hardware-equivalent backend.</summary>
public enum DeviceType
{
    Default = 0,
    Hardware = 1,
    Reference = 2,
    Null = 3,
}

/// <summary>Depth/stencil buffer formats.</summary>
public enum DepthFormat
{
    None = 0,
    Depth16 = 1,
    Depth24 = 2,
    Depth24Stencil8 = 3,
}

/// <summary>Shader model requirements; the software backend accepts any profile.</summary>
public enum ShaderProfile
{
    ShaderModel1_1 = 1,
    ShaderModel2_0 = 2,
    ShaderModel3_0 = 3,
}

/// <summary>Swap-chain presentation interval.</summary>
public enum PresentInterval
{
    Default = 0,
    Immediate = 1,
    One = 2,
    Two = 3,
    Three = 4,
    Four = 5,
}

/// <summary>A display mode supported by an adapter.</summary>
public class DisplayMode
{
    public DisplayMode(int width, int height, SurfaceFormat format)
    {
        Width = width;
        Height = height;
        Format = format;
    }

    public int Width { get; }

    public int Height { get; }

    public SurfaceFormat Format { get; }

    public float AspectRatio => Height == 0 ? 0f : (float)Width / Height;
}

/// <summary>A graphics adapter; Dorado exposes a single synthetic adapter.</summary>
public class GraphicsAdapter
{
    private static readonly GraphicsAdapter Default = new();

    public static GraphicsAdapter DefaultAdapter => Default;

    public static ReadOnlyCollection<GraphicsAdapter> Adapters { get; } =
        new(new[] { Default });

    public virtual DisplayMode CurrentDisplayMode => new(480, 272, SurfaceFormat.Color);

    public virtual bool IsWideScreen => true;

    public virtual string Description => "Dorado Software Adapter";

    public virtual string DeviceName => "Dorado";
}

/// <summary>Swap-chain and presentation settings.</summary>
public class PresentationParameters
{
    public int BackBufferWidth { get; set; } = 480;

    public int BackBufferHeight { get; set; } = 272;

    public SurfaceFormat BackBufferFormat { get; set; } = SurfaceFormat.Color;

    public int BackBufferCount { get; set; } = 1;

    public DepthFormat DepthStencilFormat { get; set; } = DepthFormat.Depth24;

    public bool EnableAutoDepthStencil { get; set; }

    public bool IsFullScreen { get; set; }

    public IntPtr DeviceWindowHandle { get; set; }

    public int MultiSampleCount { get; set; }

    public PresentInterval PresentationInterval { get; set; } = PresentInterval.Default;
}

/// <summary>Exposes the device a game is rendered with.</summary>
public interface IGraphicsDeviceService
{
    GraphicsDevice GraphicsDevice { get; }

    event EventHandler? DeviceCreated;

    event EventHandler? DeviceDisposing;

    event EventHandler? DeviceResetting;

    event EventHandler? DeviceReset;
}

/// <summary>Pixel formats; numeric values follow XNA 3.1.</summary>
public enum SurfaceFormat
{
    Unknown = 0,
    Color = 1,
    Bgr565 = 2,
    Bgra5551 = 3,
    Bgra4444 = 4,
    Dxt1 = 5,
    Dxt3 = 6,
    Dxt5 = 7,
    NormalizedByte2 = 8,
    NormalizedByte4 = 9,
    Rgba1010102 = 10,
    Rg32 = 11,
    Rgba64 = 12,
    Alpha8 = 13,
    Single = 14,
    Vector2 = 15,
    Vector4 = 16,
    HalfSingle = 17,
    HalfVector2 = 18,
    HalfVector4 = 19,
    HdrBlendable = 20,
}

public enum SpriteBlendMode
{
    None = 0,
    AlphaBlend = 1,
    Additive = 2,
}

public enum SpriteSortMode
{
    Immediate = 0,
    Deferred = 1,
    Texture = 2,
    BackToFront = 3,
    FrontToBack = 4,
}

public enum SaveStateMode
{
    None = 0,
    SaveState = 1,
}

[Flags]
public enum SpriteEffects
{
    None = 0,
    FlipHorizontally = 1,
    FlipVertically = 2,
}

/// <summary>Base class for resources that belong to a graphics device.</summary>
public class GraphicsResource : IDisposable
{
    public GraphicsDevice? GraphicsDevice { get; internal set; }

    public string Name { get; set; } = string.Empty;

    public object? Tag { get; set; }

    public bool IsDisposed { get; protected set; }

    public event EventHandler? Disposing;

    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        IsDisposed = true;
        Disposing?.Invoke(this, EventArgs.Empty);
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
    }
}

/// <summary>A GPU texture.</summary>
public class Texture2D : GraphicsResource
{
    private ITexture _backend;
    private byte[]? _pixels;

    internal Texture2D(ITexture backend, int width, int height)
    {
        _backend = backend;
        Width = width;
        Height = height;
        GraphicsDevice = GraphicsDevice.Active;
    }

    public Texture2D(GraphicsDevice graphicsDevice, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Texture dimensions must be positive.");
        }

        GraphicsDevice = graphicsDevice;
        _backend = graphicsDevice.CreateBackendTexture(width, height);
        Width = width;
        Height = height;
    }

    public int Width { get; }

    public int Height { get; }

    internal ITexture Backend => _backend;

    internal byte[]? PixelSnapshot => _pixels;

    public void SetData<T>(T[] data)
        where T : struct
    {
        ArgumentNullException.ThrowIfNull(data);
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        byte[] rgba = ToRgbaBytes(data);
        if (rgba.Length < Width * Height * 4)
        {
            throw new ArgumentException("Data is smaller than the texture.", nameof(data));
        }

        GraphicsDevice device = GraphicsDevice ??
            throw new InvalidOperationException("The texture is not bound to a graphics device.");
        _backend = device.Backend.CreateTexture(Width, Height, rgba, premultiplied: true);
        _pixels = rgba;
    }

    public void GetData<T>(T[] data)
        where T : struct
    {
        ArgumentNullException.ThrowIfNull(data);
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        byte[] rgba = _pixels ??
            throw new NotSupportedException("Pixel readback is only available for textures written with SetData.");

        int pixels = Width * Height;
        if (typeof(T) == typeof(byte))
        {
            int count = Math.Min(data.Length, rgba.Length);
            Array.Copy(rgba, data, count);
            return;
        }

        if (typeof(T) == typeof(Color))
        {
            var colors = (Color[])(object)data;
            int count = Math.Min(colors.Length, pixels);
            for (int i = 0; i < count; i++)
            {
                colors[i] = new Color(rgba[(i * 4) + 0], rgba[(i * 4) + 1], rgba[(i * 4) + 2], rgba[(i * 4) + 3]);
            }

            return;
        }

        throw new NotSupportedException($"GetData<{typeof(T).Name}> is not supported.");
    }

    internal static Texture2D FromPixels(int width, int height, byte[] rgba, bool premultiplied)
    {
        GraphicsDevice device = GraphicsDevice.Active ??
            throw new InvalidOperationException("No graphics device has been created.");
        ITexture backend = device.Backend.CreateTexture(width, height, rgba, premultiplied);
        var texture = new Texture2D(backend, width, height);
        if (premultiplied)
        {
            texture._pixels = rgba;
        }
        else
        {
            texture._pixels = Premultiply(rgba);
        }

        return texture;
    }

    private static byte[] ToRgbaBytes<T>(T[] data)
        where T : struct
    {
        if (data is byte[] bytes)
        {
            return bytes;
        }

        if (data is Color[] colors)
        {
            var rgba = new byte[colors.Length * 4];
            for (int i = 0; i < colors.Length; i++)
            {
                rgba[(i * 4) + 0] = colors[i].R;
                rgba[(i * 4) + 1] = colors[i].G;
                rgba[(i * 4) + 2] = colors[i].B;
                rgba[(i * 4) + 3] = colors[i].A;
            }

            return rgba;
        }

        throw new NotSupportedException($"SetData<{typeof(T).Name}> is not supported.");
    }

    private static byte[] Premultiply(byte[] rgba)
    {
        var result = new byte[rgba.Length];
        for (int i = 0; i + 3 < rgba.Length; i += 4)
        {
            byte a = rgba[i + 3];
            result[i + 0] = (byte)(rgba[i + 0] * a / 255);
            result[i + 1] = (byte)(rgba[i + 1] * a / 255);
            result[i + 2] = (byte)(rgba[i + 2] * a / 255);
            result[i + 3] = a;
        }

        return result;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}

/// <summary>A viewport rectangle.</summary>
public struct Viewport
{
    public Viewport(int x, int y, int width, int height)
        : this()
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        MinDepth = 0f;
        MaxDepth = 1f;
    }

    public int X { get; set; }

    public int Y { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public float MinDepth { get; set; }

    public float MaxDepth { get; set; }
}

/// <summary>A render-target texture.</summary>
public class RenderTarget2D : Texture2D
{
    private readonly GraphicsDevice _device;

    public RenderTarget2D(GraphicsDevice graphicsDevice, int width, int height, int numberLevels, SurfaceFormat format)
        : base(graphicsDevice.CreateBackendTexture(width, height), width, height)
    {
        _device = graphicsDevice;
        Format = format;
        GraphicsDevice = graphicsDevice;
    }

    public SurfaceFormat Format { get; }

    public Texture2D GetTexture() => this;
}

/// <summary>The graphics device; a thin façade over an <see cref="IGraphicsBackend"/>.</summary>
public class GraphicsDevice : IDisposable
{
    private readonly IGraphicsBackend _backend;

    public GraphicsDevice(IGraphicsBackend backend)
    {
        _backend = backend;
        Viewport = new Viewport(0, 0, backend.Width, backend.Height);
        Active = this;
    }

    /// <summary>The most recently created device; Dorado runs one title per process.</summary>
    public static GraphicsDevice? Active { get; private set; }

    public Viewport Viewport { get; set; }

    public int DisplayWidth => _backend.Width;

    public int DisplayHeight => _backend.Height;

    public void Clear(Color color) =>
        _backend.Clear(new Rgba32(color.R, color.G, color.B, color.A));

    public void SetRenderTarget(RenderTarget2D? renderTarget) =>
        _backend.SetRenderTarget(renderTarget?.Backend);

    public void SetRenderTarget(int renderTargetIndex, RenderTarget2D? renderTarget) =>
        _backend.SetRenderTarget(renderTarget?.Backend);

    public void Present() => _backend.Present();

    internal IGraphicsBackend Backend => _backend;

    internal ITexture CreateBackendTexture(int width, int height) =>
        _backend.CreateTexture(width, height, ReadOnlySpan<byte>.Empty, premultiplied: true);

    internal Texture2D CreateTexture(int width, int height, ReadOnlySpan<byte> rgba, bool premultiplied)
    {
        ITexture backend = _backend.CreateTexture(width, height, rgba, premultiplied);
        return new Texture2D(backend, width, height) { GraphicsDevice = this };
    }

    public void Dispose()
    {
    }
}
