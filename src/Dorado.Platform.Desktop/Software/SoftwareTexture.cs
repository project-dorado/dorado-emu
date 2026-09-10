using Dorado.Platform;

namespace Dorado.Platform.Desktop.Software;

internal sealed class SoftwareTexture : ITexture
{
    public SoftwareTexture(int width, int height)
    {
        Width = width;
        Height = height;
        Pixels = new Rgba32[width * height];
    }

    public int Width { get; }

    public int Height { get; }

    public Rgba32[] Pixels { get; }
}
