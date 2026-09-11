using System.Text;

namespace Microsoft.Xna.Framework.Content;

/// <summary>Reads an XNA 3.1 XNB asset through the content type-reader pipeline.</summary>
internal static class XnbReader
{
    private const byte FlagCompressedLzx = 0x80;
    private const byte FlagCompressedLz4 = 0x40;

    public static object? Read(byte[] data, ContentManager? contentManager = null, string assetName = "")
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

        using var stream = new MemoryStream(data, writable: false);
        stream.Position = 10;

        int readerCount = Read7Bit(stream);
        var names = new List<string>(readerCount);
        for (int i = 0; i < readerCount; i++)
        {
            int nameLength = Read7Bit(stream);
            var name = new byte[nameLength];
            ReadExactly(stream, name);
            names.Add(Encoding.ASCII.GetString(name));
            stream.Position += 4; // reader version
        }

        _ = Read7Bit(stream); // shared resource count
        int primaryIndex = Read7Bit(stream);

        var manager = new ContentTypeReaderManager();
        var readers = new List<ContentTypeReader> { null! };
        foreach (string name in names)
        {
            readers.Add(ContentTypeReaderManager.Create(name, manager));
        }

        var reader = new ContentReader(stream, manager, readers, contentManager) { AssetName = assetName };
        if (primaryIndex == 0)
        {
            return null;
        }

        return readers[primaryIndex].Read(reader, null);
    }

    private static int Read7Bit(Stream stream)
    {
        int result = 0;
        int shift = 0;
        byte b;
        do
        {
            int value = stream.ReadByte();
            if (value < 0)
            {
                throw new EndOfStreamException("Unexpected end of XNB stream.");
            }

            b = (byte)value;
            result |= (b & 0x7F) << shift;
            shift += 7;
        }
        while ((b & 0x80) != 0);

        return result;
    }

    private static void ReadExactly(Stream stream, byte[] buffer)
    {
        int offset = 0;
        while (offset < buffer.Length)
        {
            int read = stream.Read(buffer, offset, buffer.Length - offset);
            if (read <= 0)
            {
                throw new EndOfStreamException("Unexpected end of XNB stream.");
            }

            offset += read;
        }
    }
}
