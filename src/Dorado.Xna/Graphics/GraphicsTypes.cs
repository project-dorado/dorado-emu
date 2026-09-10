using Dorado.Platform;

namespace Microsoft.Xna.Framework.Graphics;

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

/// <summary>A GPU texture.</summary>
public class Texture2D : IDisposable
{
    internal Texture2D(ITexture backend, int width, int height)
    {
        Backend = backend;
        Width = width;
        Height = height;
    }

    public int Width { get; }

    public int Height { get; }

    public GraphicsDevice? GraphicsDevice { get; internal set; }

    internal ITexture Backend { get; }

    public void Dispose()
    {
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
    }

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
