using System.Buffers.Binary;
using System.IO.Compression;
using Dorado.Platform;

namespace Dorado.Platform.Desktop.Software;

/// <summary>Minimal RGBA8 PNG encoder (no external image dependency).</summary>
internal static class PngWriter
{
    private static readonly byte[] Signature = { 137, 80, 78, 71, 13, 10, 26, 10 };

    public static void Write(Stream stream, int width, int height, ReadOnlySpan<Rgba32> pixels)
    {
        stream.Write(Signature);

        Span<byte> ihdr = stackalloc byte[13];
        BinaryPrimitives.WriteInt32BigEndian(ihdr, width);
        BinaryPrimitives.WriteInt32BigEndian(ihdr[4..], height);
        ihdr[8] = 8;  // bit depth
        ihdr[9] = 6;  // colour type RGBA
        ihdr[10] = 0; // compression
        ihdr[11] = 0; // filter
        ihdr[12] = 0; // interlace
        WriteChunk(stream, "IHDR"u8, ihdr);

        int stride = (width * 4) + 1;
        var raw = new byte[stride * height];
        for (int y = 0; y < height; y++)
        {
            int rowStart = y * stride;
            raw[rowStart] = 0; // filter: none
            for (int x = 0; x < width; x++)
            {
                Rgba32 pixel = pixels[(y * width) + x];
                int o = rowStart + 1 + (x * 4);
                raw[o] = pixel.R;
                raw[o + 1] = pixel.G;
                raw[o + 2] = pixel.B;
                raw[o + 3] = pixel.A;
            }
        }

        byte[] compressed;
        using (var ms = new MemoryStream())
        {
            using (var zlib = new ZLibStream(ms, CompressionLevel.Optimal, leaveOpen: true))
            {
                zlib.Write(raw);
            }

            compressed = ms.ToArray();
        }

        WriteChunk(stream, "IDAT"u8, compressed);
        WriteChunk(stream, "IEND"u8, ReadOnlySpan<byte>.Empty);
    }

    private static void WriteChunk(Stream stream, ReadOnlySpan<byte> type, ReadOnlySpan<byte> data)
    {
        Span<byte> length = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(length, data.Length);
        stream.Write(length);
        stream.Write(type);
        stream.Write(data);

        uint crc = Crc32.Compute(type, data);
        Span<byte> crcBytes = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crcBytes, crc);
        stream.Write(crcBytes);
    }
}

internal static class Crc32
{
    private static readonly uint[] Table = CreateTable();

    public static uint Compute(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b)
    {
        uint crc = 0xFFFFFFFFu;
        crc = Update(crc, a);
        crc = Update(crc, b);
        return crc ^ 0xFFFFFFFFu;
    }

    private static uint Update(uint crc, ReadOnlySpan<byte> data)
    {
        foreach (byte value in data)
        {
            crc = Table[(crc ^ value) & 0xFF] ^ (crc >> 8);
        }

        return crc;
    }

    private static uint[] CreateTable()
    {
        var table = new uint[256];
        for (uint n = 0; n < 256; n++)
        {
            uint c = n;
            for (int k = 0; k < 8; k++)
            {
                c = (c & 1) != 0 ? 0xEDB88320u ^ (c >> 1) : c >> 1;
            }

            table[n] = c;
        }

        return table;
    }
}
