namespace Dorado.Platform;

/// <summary>An 8-bit-per-channel colour, stored straight (non-premultiplied).</summary>
public readonly struct Rgba32 : IEquatable<Rgba32>
{
    public Rgba32(byte r, byte g, byte b, byte a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public byte R { get; }

    public byte G { get; }

    public byte B { get; }

    public byte A { get; }

    public static Rgba32 FromPacked(uint argb) =>
        new((byte)(argb >> 16), (byte)(argb >> 8), (byte)argb, (byte)(argb >> 24));

    public uint ToPacked() => (uint)((A << 24) | (R << 16) | (G << 8) | B);

    public bool Equals(Rgba32 other) => R == other.R && G == other.G && B == other.B && A == other.A;

    public override bool Equals(object? obj) => obj is Rgba32 other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(R, G, B, A);

    public override string ToString() => $"#{R:X2}{G:X2}{B:X2}{A:X2}";
}

/// <summary>An opaque GPU/host texture handle.</summary>
public interface ITexture
{
    int Width { get; }

    int Height { get; }
}

/// <summary>A single 2-D sprite draw, already resolved by the XNA shim.</summary>
public struct SpriteDrawRequest
{
    public ITexture Texture;

    public bool HasSource;
    public int SourceX;
    public int SourceY;
    public int SourceWidth;
    public int SourceHeight;

    public float X;
    public float Y;
    public float OriginX;
    public float OriginY;
    public float ScaleX;
    public float ScaleY;
    public float Rotation;

    public Rgba32 Tint;
    public bool FlipX;
    public bool FlipY;
    public float LayerDepth;
}

/// <summary>The rendering surface backend the XNA shim draws through.</summary>
public interface IGraphicsBackend
{
    int Width { get; }

    int Height { get; }

    void Clear(Rgba32 color);

    /// <summary>
    /// Creates a texture. When <paramref name="premultiplied"/> is true the pixel
    /// data already has alpha folded into RGB (the XNA content pipeline default).
    /// </summary>
    ITexture CreateTexture(int width, int height, ReadOnlySpan<byte> rgba, bool premultiplied);

    void SetRenderTarget(ITexture? target);

    void DrawSprite(in SpriteDrawRequest request);

    /// <summary>Flushes the current frame to the host.</summary>
    void Present();
}
