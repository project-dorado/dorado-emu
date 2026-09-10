using System.Text;
using Dorado.Platform;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content;

/// <summary>Minimal reader for XNA 3.1 XNB files (uncompressed).</summary>
internal static class XnbReader
{
    private const byte FlagCompressedLzx = 0x80;
    private const byte FlagCompressedLz4 = 0x40;

    public static object Read(byte[] data)
    {
        if (data.Length < 10 || data[0] != 'X' || data[1] != 'N' || data[2] != 'B')
        {
            throw new InvalidDataException("Not an XNB file.");
        }

        byte flags = data[5];
        if ((flags & (FlagCompressedLzx | FlagCompressedLz4)) != 0)
        {
            throw new NotSupportedException("Compressed XNB content is not supported yet.");
        }

        int pos = 10;
        int readerCount = Read7Bit(data, ref pos);

        string? primaryReader = null;
        for (int i = 0; i < readerCount; i++)
        {
            int nameLength = Read7Bit(data, ref pos);
            string name = Encoding.ASCII.GetString(data, pos, nameLength);
            pos += nameLength;
            pos += 4; // reader version
            primaryReader ??= name;
        }

        _ = Read7Bit(data, ref pos); // shared resource count
        _ = Read7Bit(data, ref pos); // primary asset type-reader index

        return primaryReader switch
        {
            "Microsoft.Xna.Framework.Content.Texture2DReader" => ReadTexture2D(data, ref pos),
            "Microsoft.Xna.Framework.Content.SoundEffectReader" => ReadSoundEffect(data, ref pos),
            _ => throw new NotSupportedException($"Unsupported XNB content reader '{primaryReader}'."),
        };
    }

    private static Texture2D ReadTexture2D(byte[] data, ref int pos)
    {
        int surfaceFormat = ReadInt32(data, ref pos);
        int width = ReadInt32(data, ref pos);
        int height = ReadInt32(data, ref pos);
        int mipCount = ReadInt32(data, ref pos);

        int dataSize = ReadInt32(data, ref pos);
        int start = pos;
        pos += dataSize;

        byte[] pixels = DecodePixels(data, start, dataSize, width, height, surfaceFormat, mipCount);
        return new Texture2D(PlatformHost.Graphics.CreateTexture(width, height, pixels, premultiplied: true), width, height);
    }

    private static byte[] DecodePixels(byte[] data, int start, int dataSize, int width, int height, int surfaceFormat, int mipCount)
    {
        if (width <= 0 || height <= 0)
        {
            throw new InvalidDataException("Texture has invalid dimensions.");
        }

        int pixels = width * height;
        if (dataSize == pixels * 4)
        {
            var rgba = new byte[dataSize];
            Array.Copy(data, start, rgba, 0, dataSize);
            return rgba;
        }

        if (dataSize == pixels * 2)
        {
            var rgba = new byte[pixels * 4];
            for (int i = 0; i < pixels; i++)
            {
                int packed = data[start + (i * 2)] | (data[start + (i * 2) + 1] << 8);
                byte r = (byte)((packed >> 11) & 0x1F);
                byte g = (byte)((packed >> 5) & 0x3F);
                byte b = (byte)(packed & 0x1F);
                rgba[i * 4] = (byte)((r << 3) | (r >> 2));
                rgba[(i * 4) + 1] = (byte)((g << 2) | (g >> 4));
                rgba[(i * 4) + 2] = (byte)((b << 3) | (b >> 2));
                rgba[(i * 4) + 3] = 255;
            }

            return rgba;
        }

        throw new NotSupportedException(
            $"Unsupported texture encoding: {dataSize} bytes for {width}x{height} (surface format {surfaceFormat}, {mipCount} mips).");
    }

    private static object ReadSoundEffect(byte[] data, ref int pos)
    {
        int formatSize = ReadInt32(data, ref pos);
        int formatStart = pos;
        pos += formatSize;
        int loopStart = ReadInt32(data, ref pos);
        int loopLength = ReadInt32(data, ref pos);
        int duration = ReadInt32(data, ref pos);
        int dataSize = ReadInt32(data, ref pos);
        var pcm = new byte[dataSize];
        Array.Copy(data, pos, pcm, 0, dataSize);
        pos += dataSize;

        return new Audio.SoundEffect(pcm, data, formatStart, formatSize, duration, loopStart, loopLength);
    }

    private static int ReadInt32(byte[] data, ref int pos)
    {
        int value = BitConverter.ToInt32(data, pos);
        pos += 4;
        return value;
    }

    private static int Read7Bit(byte[] data, ref int pos)
    {
        int result = 0;
        int shift = 0;
        byte b;
        do
        {
            b = data[pos++];
            result |= (b & 0x7F) << shift;
            shift += 7;
        }
        while ((b & 0x80) != 0);

        return result;
    }
}
