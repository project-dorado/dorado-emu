using Dorado.Platform;

namespace Dorado.Platform.Desktop.Software;

/// <summary>
/// A deterministic, headless software rasterizer for the XNA SpriteBatch subset.
/// Renders straight into an RGBA framebuffer so golden frames are reproducible.
/// </summary>
public sealed class SoftwareGraphicsBackend : IGraphicsBackend
{
    private readonly object _gate = new();
    private SoftwareTexture? _target;

    public SoftwareGraphicsBackend(int width = 480, int height = 272)
    {
        Width = width;
        Height = height;
        Backbuffer = new SoftwareTexture(width, height);
    }

    public int Width { get; }

    public int Height { get; }

    internal SoftwareTexture Backbuffer { get; }

    /// <summary>Frames rendered since construction.</summary>
    public int FrameCount { get; private set; }

    public ITexture CreateTexture(int width, int height, ReadOnlySpan<byte> rgba, bool premultiplied)
    {
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Texture dimensions must be positive.");
        }

        var texture = new SoftwareTexture(width, height);
        if (!rgba.IsEmpty)
        {
            for (int i = 0; i < texture.Pixels.Length; i++)
            {
                byte r = rgba[(i * 4) + 0];
                byte g = rgba[(i * 4) + 1];
                byte b = rgba[(i * 4) + 2];
                byte a = rgba[(i * 4) + 3];
                if (!premultiplied)
                {
                    r = (byte)(r * a / 255);
                    g = (byte)(g * a / 255);
                    b = (byte)(b * a / 255);
                }

                texture.Pixels[i] = new Rgba32(r, g, b, a);
            }
        }

        return texture;
    }

    public void SetRenderTarget(ITexture? target) => _target = (SoftwareTexture?)target;

    public void Clear(Rgba32 color)
    {
        lock (_gate)
        {
            SoftwareTexture target = _target ?? Backbuffer;
            Array.Fill(target.Pixels, color);
        }
    }

    public void DrawSprite(in SpriteDrawRequest request)
    {
        if (request.Texture is not SoftwareTexture texture)
        {
            return;
        }

        lock (_gate)
        {
            SoftwareTexture target = _target ?? Backbuffer;
            Rasterize(target, texture, in request);
        }
    }

    public void Present()
    {
        lock (_gate)
        {
            FrameCount++;
        }
    }

    /// <summary>Snapshots the backbuffer as a packed ARGB array (row-major).</summary>
    public uint[] SnapshotBackbuffer()
    {
        lock (_gate)
        {
            var result = new uint[Backbuffer.Pixels.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Backbuffer.Pixels[i].ToPacked();
            }

            return result;
        }
    }

    public void SavePng(string path)
    {
        lock (_gate)
        {
            using var stream = File.Create(path);
            PngWriter.Write(stream, Backbuffer.Width, Backbuffer.Height, Backbuffer.Pixels);
        }
    }

    private static void Rasterize(SoftwareTexture target, SoftwareTexture texture, in SpriteDrawRequest r)
    {
        int srcW = r.HasSource ? r.SourceWidth : texture.Width;
        int srcH = r.HasSource ? r.SourceHeight : texture.Height;
        if (srcW <= 0 || srcH <= 0 || r.ScaleX == 0f || r.ScaleY == 0f)
        {
            return;
        }

        float cos = MathF.Cos(r.Rotation);
        float sin = MathF.Sin(r.Rotation);

        // Destination AABB from the four transformed source corners.
        float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
        ReadOnlySpan<(float X, float Y)> corners = stackalloc[]
        {
            (0f, 0f), (srcW, 0f), (0f, srcH), (srcW, srcH),
        };
        foreach ((float cx, float cy) in corners)
        {
            float lx = r.ScaleX * (cx - r.OriginX);
            float ly = r.ScaleY * (cy - r.OriginY);
            float wx = r.X + (cos * lx) - (sin * ly);
            float wy = r.Y + (sin * lx) + (cos * ly);
            minX = MathF.Min(minX, wx);
            minY = MathF.Min(minY, wy);
            maxX = MathF.Max(maxX, wx);
            maxY = MathF.Max(maxY, wy);
        }

        int x0 = Math.Max(0, (int)MathF.Floor(minX));
        int y0 = Math.Max(0, (int)MathF.Floor(minY));
        int x1 = Math.Min(target.Width - 1, (int)MathF.Ceiling(maxX));
        int y1 = Math.Min(target.Height - 1, (int)MathF.Ceiling(maxY));

        for (int y = y0; y <= y1; y++)
        {
            for (int x = x0; x <= x1; x++)
            {
                float dx = x + 0.5f - r.X;
                float dy = y + 0.5f - r.Y;
                float ux = (cos * dx) + (sin * dy);
                float uy = (-sin * dx) + (cos * dy);

                float sx = (ux / r.ScaleX) + r.OriginX;
                float sy = (uy / r.ScaleY) + r.OriginY;
                if (r.FlipX)
                {
                    sx = srcW - sx;
                }

                if (r.FlipY)
                {
                    sy = srcH - sy;
                }

                if (sx < 0f || sy < 0f || sx >= srcW || sy >= srcH)
                {
                    continue;
                }

                int texelX = (r.HasSource ? r.SourceX : 0) + (int)sx;
                int texelY = (r.HasSource ? r.SourceY : 0) + (int)sy;
                if (texelX < 0 || texelY < 0 || texelX >= texture.Width || texelY >= texture.Height)
                {
                    continue;
                }

                Rgba32 src = texture.Pixels[(texelY * texture.Width) + texelX];
                Blend(target, (y * target.Width) + x, src, r.Tint);
            }
        }
    }

    private static void Blend(SoftwareTexture target, int index, Rgba32 src, Rgba32 tint)
    {
        int sr = src.R * tint.R / 255;
        int sg = src.G * tint.G / 255;
        int sb = src.B * tint.B / 255;
        int sa = src.A * tint.A / 255;

        if (sa == 0 && sr == 0 && sg == 0 && sb == 0)
        {
            return;
        }

        Rgba32 dst = target.Pixels[index];
        int inv = 255 - sa;
        byte r = (byte)Math.Min(255, sr + (dst.R * inv / 255));
        byte g = (byte)Math.Min(255, sg + (dst.G * inv / 255));
        byte b = (byte)Math.Min(255, sb + (dst.B * inv / 255));
        byte a = (byte)Math.Min(255, sa + (dst.A * inv / 255));
        target.Pixels[index] = new Rgba32(r, g, b, a);
    }
}
