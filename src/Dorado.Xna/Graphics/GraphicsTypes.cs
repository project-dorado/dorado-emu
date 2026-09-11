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

/// <summary>Multisample settings; Dorado ignores sampling.</summary>
public enum MultiSampleType
{
    None = 0,
    NonMaskable2x = 1,
    NonMaskable4x = 2,
    TwoSamples = 3,
    FourSamples = 4,
}

/// <summary>Presentation options; Dorado presents a single frame per tick.</summary>
[Flags]
public enum PresentOptions
{
    None = 0,
    DiscardDepthStencil = 1,
    DeviceClip = 2,
    Immediate = 4,
}

/// <summary>Swap effect used when presenting.</summary>
public enum SwapEffect
{
    Discard = 0,
    Sequential = 1,
    Copy = 2,
    Flip = 3,
}

/// <summary>Buffers cleared by <see cref="GraphicsDevice.Clear(ClearOptions, Color, float, int)"/>.</summary>
[Flags]
public enum ClearOptions
{
    Target = 1,
    DepthBuffer = 2,
    Stencil = 4,
}

/// <summary>Device health.</summary>
public enum GraphicsDeviceStatus
{
    Normal = 0,
    Lost = 1,
    NotReset = 2,
}

/// <summary>Hints for dynamic texture uploads; Dorado always overwrites.</summary>
public enum SetDataOptions
{
    None = 0,
    Discard = 1,
    NoOverwrite = 2,
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
public class PresentationParameters : IDisposable
{
    public const int DefaultPresentRate = 60;

    public int BackBufferWidth { get; set; } = 480;

    public int BackBufferHeight { get; set; } = 272;

    public SurfaceFormat BackBufferFormat { get; set; } = SurfaceFormat.Color;

    public int BackBufferCount { get; set; } = 1;

    public DepthFormat AutoDepthStencilFormat { get; set; } = DepthFormat.Depth24;

    public DepthFormat DepthStencilFormat { get; set; } = DepthFormat.Depth24;

    public bool EnableAutoDepthStencil { get; set; }

    public bool IsFullScreen { get; set; }

    public IntPtr DeviceWindowHandle { get; set; }

    public int FullScreenRefreshRateInHz { get; set; } = DefaultPresentRate;

    public MultiSampleType MultiSampleType { get; set; } = MultiSampleType.None;

    public int MultiSampleQuality { get; set; }

    public PresentOptions PresentOptions { get; set; } = PresentOptions.None;

    public PresentInterval PresentationInterval { get; set; } = PresentInterval.Default;

    public SwapEffect SwapEffect { get; set; } = SwapEffect.Discard;

    public void Clear()
    {
    }

    public PresentationParameters Clone() => (PresentationParameters)MemberwiseClone();

    public override string ToString() =>
        $"{{BackBufferWidth:{BackBufferWidth} BackBufferHeight:{BackBufferHeight} " +
        $"BackBufferFormat:{BackBufferFormat} IsFullScreen:{IsFullScreen}}}";

    public override bool Equals(object? obj) => obj is PresentationParameters other &&
        BackBufferWidth == other.BackBufferWidth &&
        BackBufferHeight == other.BackBufferHeight &&
        BackBufferFormat == other.BackBufferFormat &&
        BackBufferCount == other.BackBufferCount &&
        DepthStencilFormat == other.DepthStencilFormat &&
        EnableAutoDepthStencil == other.EnableAutoDepthStencil &&
        IsFullScreen == other.IsFullScreen &&
        DeviceWindowHandle == other.DeviceWindowHandle &&
        PresentationInterval == other.PresentationInterval;

    public override int GetHashCode() => HashCode.Combine(
        BackBufferWidth, BackBufferHeight, BackBufferFormat, BackBufferCount, IsFullScreen);

    public static bool operator ==(PresentationParameters? left, PresentationParameters? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(PresentationParameters? left, PresentationParameters? right) => !(left == right);

    public void Dispose() => GC.SuppressFinalize(this);
}

/// <summary>Arguments for a device resource creation event.</summary>
public sealed class ResourceCreatedEventArgs : EventArgs
{
    public ResourceCreatedEventArgs(object resource)
    {
        Resource = resource;
    }

    public object Resource { get; }
}

/// <summary>Arguments for a device resource destruction event.</summary>
public sealed class ResourceDestroyedEventArgs : EventArgs
{
    public ResourceDestroyedEventArgs(string name, object tag)
    {
        Name = name;
        Tag = tag;
    }

    public string Name { get; }

    public object Tag { get; }
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
        where T : struct =>
        SetData(0, null, data, 0, data?.Length ?? 0, SetDataOptions.None);

    public void SetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount, SetDataOptions options)
        where T : struct
    {
        ArgumentNullException.ThrowIfNull(data);
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        if (elementCount <= 0)
        {
            return;
        }

        byte[] source = ToRgbaBytes(data, startIndex, elementCount);
        Rectangle region = rect ?? new Rectangle(0, 0, Width, Height);
        if (region.Width <= 0 || region.Height <= 0)
        {
            return;
        }

        byte[] pixels = _pixels is { Length: > 0 } existing && existing.Length >= Width * Height * 4
            ? existing
            : new byte[Width * Height * 4];

        int sourceStride = region.Width * 4;
        for (int row = 0; row < region.Height; row++)
        {
            int destinationY = region.Y + row;
            if (destinationY < 0 || destinationY >= Height)
            {
                continue;
            }

            int copyPixels = Math.Min(region.Width, Width - region.X);
            int sourceOffset = row * sourceStride;
            if (copyPixels <= 0 || sourceOffset + (copyPixels * 4) > source.Length)
            {
                break;
            }

            Array.Copy(
                source,
                sourceOffset,
                pixels,
                (((destinationY * Width) + region.X) * 4),
                copyPixels * 4);
        }

        Upload(pixels, premultiplied: false);
        _pixels = pixels;
    }

    private void Upload(byte[] rgba, bool premultiplied)
    {
        GraphicsDevice device = GraphicsDevice ??
            throw new InvalidOperationException("The texture is not bound to a graphics device.");
        _backend = device.Backend.CreateTexture(Width, Height, rgba, premultiplied);
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

    private static byte[] ToRgbaBytes<T>(T[] data, int startIndex, int elementCount)
        where T : struct
    {
        if (startIndex < 0 || startIndex > data.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(startIndex));
        }

        if (data is byte[] bytes)
        {
            int count = Math.Min(elementCount, bytes.Length - startIndex);
            var slice = new byte[count];
            Array.Copy(bytes, startIndex, slice, 0, count);
            return slice;
        }

        if (data is uint[] packed)
        {
            int count = Math.Min(elementCount, packed.Length - startIndex);
            var rgba = new byte[count * 4];
            for (int i = 0; i < count; i++)
            {
                uint value = packed[startIndex + i];
                rgba[(i * 4) + 0] = (byte)(value >> 16);
                rgba[(i * 4) + 1] = (byte)(value >> 8);
                rgba[(i * 4) + 2] = (byte)value;
                rgba[(i * 4) + 3] = (byte)(value >> 24);
            }

            return rgba;
        }

        if (data is Color[] colors)
        {
            int count = Math.Min(elementCount, colors.Length - startIndex);
            var rgba = new byte[count * 4];
            for (int i = 0; i < count; i++)
            {
                Color color = colors[startIndex + i];
                rgba[(i * 4) + 0] = color.R;
                rgba[(i * 4) + 1] = color.G;
                rgba[(i * 4) + 2] = color.B;
                rgba[(i * 4) + 3] = color.A;
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
    private Rectangle _scissorRectangle;

    public GraphicsDevice(IGraphicsBackend backend)
    {
        _backend = backend;
        Viewport = new Viewport(0, 0, backend.Width, backend.Height);
        _scissorRectangle = new Rectangle(0, 0, backend.Width, backend.Height);
        PresentationParameters = new PresentationParameters
        {
            BackBufferWidth = backend.Width,
            BackBufferHeight = backend.Height,
        };
        Active = this;
    }

    /// <summary>The most recently created device; Dorado runs one title per process.</summary>
    public static GraphicsDevice? Active { get; private set; }

    public Viewport Viewport { get; set; }

    public Rectangle ScissorRectangle
    {
        get => _scissorRectangle;
        set => _scissorRectangle = value;
    }

    public PresentationParameters PresentationParameters { get; }

    public GraphicsDeviceStatus GraphicsDeviceStatus => GraphicsDeviceStatus.Normal;

    public bool IsDisposed { get; private set; }

    public int DisplayWidth => _backend.Width;

    public int DisplayHeight => _backend.Height;

    public event EventHandler? DeviceLost;

    public event EventHandler? DeviceReset;

    public event EventHandler? DeviceResetting;

    public event EventHandler? Disposing;

    public void Clear(Color color) =>
        _backend.Clear(new Rgba32(color.R, color.G, color.B, color.A));

    public void Clear(ClearOptions options, Color color, float depth, int stencil) =>
        Clear(color);

    public void Clear(ClearOptions options, Color color, float depth, int stencil, Rectangle[] regions) =>
        Clear(color);

    public void Clear(ClearOptions options, Vector4 color, float depth, int stencil) =>
        Clear(new Color(color));

    public void Clear(ClearOptions options, Vector4 color, float depth, int stencil, Rectangle[] regions) =>
        Clear(new Color(color));

    public void SetRenderTarget(RenderTarget2D? renderTarget) =>
        _backend.SetRenderTarget(renderTarget?.Backend);

    public void SetRenderTarget(int renderTargetIndex, RenderTarget2D? renderTarget) =>
        _backend.SetRenderTarget(renderTarget?.Backend);

    public void Reset()
    {
        DeviceResetting?.Invoke(this, EventArgs.Empty);
        DeviceReset?.Invoke(this, EventArgs.Empty);
    }

    public void Reset(PresentationParameters presentationParameters)
    {
        ArgumentNullException.ThrowIfNull(presentationParameters);
        Reset();
    }

    public void EvictManagedResources()
    {
    }

    public void Present() => _backend.Present();

    public void Present(IntPtr overrideWindowHandle) => Present();

    public void Present(Rectangle? sourceRectangle, Rectangle? destinationRectangle, IntPtr overrideWindowHandle) =>
        Present();

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
        if (IsDisposed)
        {
            return;
        }

        IsDisposed = true;
        Disposing?.Invoke(this, EventArgs.Empty);
        GC.SuppressFinalize(this);
    }

    private void NotifyDeviceLost() => DeviceLost?.Invoke(this, EventArgs.Empty);
}
