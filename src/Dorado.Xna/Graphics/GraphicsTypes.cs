using System.Collections;
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

/// <summary>A display mode supported by an adapter; a value type matching XNA 3.1.</summary>
public struct DisplayMode : IEquatable<DisplayMode>
{
    internal DisplayMode(int width, int height, SurfaceFormat format, int refreshRate = 60)
    {
        Width = width;
        Height = height;
        Format = format;
        RefreshRate = refreshRate;
    }

    public int Width { get; }

    public int Height { get; }

    public SurfaceFormat Format { get; }

    public int RefreshRate { get; }

    public float AspectRatio => Height == 0 ? 0f : (float)Width / Height;

    public Rectangle TitleSafeArea => new(0, 0, Width, Height);

    public readonly bool Equals(DisplayMode other) =>
        Width == other.Width && Height == other.Height && Format == other.Format && RefreshRate == other.RefreshRate;

    public override readonly bool Equals(object? obj) => obj is DisplayMode other && Equals(other);

    public override readonly int GetHashCode() => HashCode.Combine(Width, Height, Format, RefreshRate);

    public override readonly string ToString() =>
        $"{{Width:{Width} Height:{Height} Format:{Format} RefreshRate:{RefreshRate}}}";

    public static bool operator ==(DisplayMode left, DisplayMode right) => left.Equals(right);

    public static bool operator !=(DisplayMode left, DisplayMode right) => !left.Equals(right);
}

/// <summary>The display modes an adapter supports, queryable by surface format.</summary>
public struct DisplayModeCollection : IEnumerable<DisplayMode>, IEquatable<DisplayModeCollection>
{
    private readonly DisplayMode[]? _modes;

    internal DisplayModeCollection(DisplayMode[] modes) => _modes = modes;

    public IEnumerable<DisplayMode> this[SurfaceFormat format] =>
        (_modes ?? Array.Empty<DisplayMode>()).Where(mode => mode.Format == format);

    public IEnumerator<DisplayMode> GetEnumerator() =>
        ((IEnumerable<DisplayMode>)(_modes ?? Array.Empty<DisplayMode>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public readonly bool Equals(DisplayModeCollection other)
    {
        DisplayMode[] left = _modes ?? Array.Empty<DisplayMode>();
        DisplayMode[] right = other._modes ?? Array.Empty<DisplayMode>();
        return left.SequenceEqual(right);
    }

    public override readonly bool Equals(object? obj) => obj is DisplayModeCollection other && Equals(other);

    public override readonly int GetHashCode()
    {
        HashCode hash = new();
        foreach (DisplayMode mode in _modes ?? Array.Empty<DisplayMode>())
        {
            hash.Add(mode);
        }

        return hash.ToHashCode();
    }

    public static bool operator ==(DisplayModeCollection left, DisplayModeCollection right) => left.Equals(right);

    public static bool operator !=(DisplayModeCollection left, DisplayModeCollection right) => !left.Equals(right);
}

/// <summary>A graphics adapter; Dorado exposes a single synthetic adapter.</summary>
public class GraphicsAdapter
{
    private static readonly DisplayMode[] Modes =
    {
        new(480, 272, SurfaceFormat.Color),
    };

    private static readonly GraphicsAdapter Default = new();

    public static GraphicsAdapter DefaultAdapter => Default;

    public static ReadOnlyCollection<GraphicsAdapter> Adapters { get; } =
        new(new[] { Default });

    public virtual DisplayMode CurrentDisplayMode => new(480, 272, SurfaceFormat.Color);

    public virtual DisplayModeCollection SupportedDisplayModes => new(Modes);

    public virtual bool IsWideScreen => true;

    public virtual string Description => "Dorado Software Adapter";

    public virtual string DeviceName => "Dorado";
}

/// <summary>How a texture is intended to be used; Dorado treats all flags as hints.</summary>
[Flags]
public enum TextureUsage
{
    None = 0,
    AutoGenerateMipMap = 0x400,
    Linear = 0x40000000,
    Tiled = int.MinValue,
}

/// <summary>How a render target's contents survive a device reset.</summary>
public enum RenderTargetUsage
{
    DiscardContents = 0,
    PreserveContents = 1,
    PlatformContents = 2,
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

/// <summary>Base class for GPU textures.</summary>
public abstract class Texture : GraphicsResource
{
    public int LevelCount { get; internal set; } = 1;
}

/// <summary>A GPU texture.</summary>
public class Texture2D : Texture
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
        : this(graphicsDevice, width, height, 1, TextureUsage.None, SurfaceFormat.Color)
    {
    }

    public Texture2D(
        GraphicsDevice graphicsDevice,
        int width,
        int height,
        int numberLevels,
        TextureUsage usage,
        SurfaceFormat format)
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
        LevelCount = Math.Max(1, numberLevels);
        TextureUsage = usage;
        Format = format;
    }

    public int Width { get; }

    public int Height { get; }

    public SurfaceFormat Format { get; } = SurfaceFormat.Color;

    public TextureUsage TextureUsage { get; } = TextureUsage.None;

    internal ITexture Backend => _backend;

    internal byte[]? PixelSnapshot => _pixels;

    public void SetData<T>(T[] data)
        where T : struct =>
        SetData(0, null, data, 0, data?.Length ?? 0, SetDataOptions.None);

    public void SetData<T>(T[] data, int startIndex, int elementCount, SetDataOptions options)
        where T : struct =>
        SetData(0, null, data, startIndex, elementCount, options);

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

    public void GetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount)
        where T : struct
    {
        ArgumentNullException.ThrowIfNull(data);
        if (startIndex < 0 || elementCount < 0 || startIndex + elementCount > data.Length)
        {
            throw new ArgumentException("The destination range is outside the array.", nameof(data));
        }

        byte[] rgba = _pixels ??
            throw new NotSupportedException("Pixel readback is only available for textures written with SetData.");

        Rectangle region = rect ?? new Rectangle(0, 0, Width, Height);
        if (region.X < 0 || region.Y < 0 || region.Right > Width || region.Bottom > Height)
        {
            throw new ArgumentOutOfRangeException(nameof(rect), "The region is outside the texture.");
        }

        if (typeof(T) == typeof(Color))
        {
            var colors = (Color[])(object)data;
            int copied = 0;
            for (int row = 0; row < region.Height && copied < elementCount; row++)
            {
                for (int column = 0; column < region.Width && copied < elementCount; column++)
                {
                    int source = (((region.Y + row) * Width) + region.X + column) * 4;
                    colors[startIndex + copied] = new Color(
                        rgba[source], rgba[source + 1], rgba[source + 2], rgba[source + 3]);
                    copied++;
                }
            }

            return;
        }

        if (typeof(T) == typeof(byte))
        {
            var bytes = (byte[])(object)data;
            int copied = 0;
            for (int row = 0; row < region.Height && copied < elementCount; row++)
            {
                int source = ((((region.Y + row) * Width) + region.X) * 4);
                int count = Math.Min(region.Width * 4, elementCount - copied);
                if (source + count > rgba.Length)
                {
                    break;
                }

                Array.Copy(rgba, source, bytes, startIndex + copied, count);
                copied += count;
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

/// <summary>A surface a device can render into.</summary>
public class RenderTarget : GraphicsResource
{
    internal RenderTarget(GraphicsDevice graphicsDevice, int width, int height, RenderTargetUsage usage)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Render target dimensions must be positive.");
        }

        GraphicsDevice = graphicsDevice;
        Width = width;
        Height = height;
        RenderTargetUsage = usage;
    }

    public int Width { get; }

    public int Height { get; }

    public RenderTargetUsage RenderTargetUsage { get; }

    public bool IsContentLost => false;

    public event EventHandler? ContentLost;

    protected virtual void OnContentLost() => ContentLost?.Invoke(this, EventArgs.Empty);
}

/// <summary>A render-target texture; retrieve the sampled surface with <see cref="GetTexture"/>.</summary>
public class RenderTarget2D : RenderTarget
{
    private readonly Texture2D _surface;

    public RenderTarget2D(
        GraphicsDevice graphicsDevice,
        int width,
        int height,
        int numberLevels,
        SurfaceFormat format)
        : this(graphicsDevice, width, height, numberLevels, format, RenderTargetUsage.DiscardContents)
    {
    }

    public RenderTarget2D(
        GraphicsDevice graphicsDevice,
        int width,
        int height,
        int numberLevels,
        SurfaceFormat format,
        RenderTargetUsage usage)
        : base(graphicsDevice, width, height, usage)
    {
        Format = format;
        _surface = graphicsDevice.CreateTexture(width, height, ReadOnlySpan<byte>.Empty, premultiplied: true);
        _surface.LevelCount = Math.Max(1, numberLevels);
        _surface.GraphicsDevice = graphicsDevice;
    }

    public RenderTarget2D(
        GraphicsDevice graphicsDevice,
        int width,
        int height,
        int numberLevels,
        SurfaceFormat format,
        MultiSampleType multiSampleType,
        int multiSampleQuality)
        : this(graphicsDevice, width, height, numberLevels, format, RenderTargetUsage.DiscardContents)
    {
        _ = multiSampleType;
        _ = multiSampleQuality;
    }

    public RenderTarget2D(
        GraphicsDevice graphicsDevice,
        int width,
        int height,
        int numberLevels,
        SurfaceFormat format,
        MultiSampleType multiSampleType,
        int multiSampleQuality,
        RenderTargetUsage usage)
        : this(graphicsDevice, width, height, numberLevels, format, usage)
    {
        _ = multiSampleType;
        _ = multiSampleQuality;
    }

    public SurfaceFormat Format { get; }

    internal ITexture Backend => _surface.Backend;

    public Texture2D GetTexture() => _surface;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _surface.Dispose();
        }

        base.Dispose(disposing);
    }
}

/// <summary>A texture that receives a copy of the back buffer.</summary>
public class ResolveTexture2D : Texture2D
{
    public ResolveTexture2D(GraphicsDevice graphicsDevice, int width, int height, int numberLevels, SurfaceFormat format)
        : base(graphicsDevice, width, height, numberLevels, TextureUsage.None, format)
    {
    }

    public bool IsContentLost => false;

    public event EventHandler? ContentLost;

    protected virtual void OnContentLost() => ContentLost?.Invoke(this, EventArgs.Empty);
}

/// <summary>Raised when a resource cannot be allocated on the device.</summary>
[Serializable]
public sealed class OutOfVideoMemoryException : System.Runtime.InteropServices.ExternalException
{
    public OutOfVideoMemoryException()
        : base("The video memory allocation failed.")
    {
    }

    public OutOfVideoMemoryException(string message)
        : base(message)
    {
    }

    public OutOfVideoMemoryException(string message, Exception inner)
        : base(message, inner)
    {
    }
}

/// <summary>The graphics device; a thin façade over an <see cref="IGraphicsBackend"/>.</summary>
public class GraphicsDevice : IDisposable
{
    private readonly IGraphicsBackend _backend;
    private Rectangle _scissorRectangle;
    private RenderTarget2D? _renderTarget;

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

    public void SetRenderTarget(RenderTarget2D? renderTarget)
    {
        _renderTarget = renderTarget;
        _backend.SetRenderTarget(renderTarget?.Backend);
    }

    public void SetRenderTarget(int renderTargetIndex, RenderTarget2D? renderTarget)
    {
        _ = renderTargetIndex;
        SetRenderTarget(renderTarget);
    }

    public RenderTarget? GetRenderTarget(int renderTargetIndex)
    {
        _ = renderTargetIndex;
        return _renderTarget;
    }

    /// <summary>
    /// Copies the back buffer into <paramref name="resolveTexture"/>. The
    /// software backend keeps no device back buffer, so Dorado leaves the
    /// resolve surface untouched; titles that sample it render blank rather
    /// than crash.
    /// </summary>
    public void ResolveBackBuffer(ResolveTexture2D resolveTexture)
    {
        ArgumentNullException.ThrowIfNull(resolveTexture);
    }

    public DisplayMode DisplayMode =>
        new(_backend.Width, _backend.Height, PresentationParameters.BackBufferFormat);

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
