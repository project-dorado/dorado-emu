using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;

namespace Dorado.Tests;

/// <summary>
/// Builds small, self-authored container images for tests. Nothing here is
/// derived from Microsoft content: both builders emit original bytes in the
/// documented public formats (MSCF/MSZIP cabinet and the NX manifest container)
/// so the headless validation suite runs in CI without fetching the
/// git-ignored homebrew corpus.
/// </summary>
internal static class SyntheticPackages
{
    // ---- .ccgame (MSCF cabinet, MSZIP) ----------------------------------

    /// <summary>
    /// Builds a single-folder MSZIP cabinet containing two files named "0"
    /// (an <c>MZ</c> executable payload) and "1" (an <c>XNB</c> payload).
    /// No <c>XCabInfo.resources</c> is emitted, so the reader falls back to
    /// signature-based path inference — which is exactly what we assert.
    /// </summary>
    public static byte[] BuildCcgame()
    {
        byte[] exePayload = BuildMZPayload(1024);
        byte[] xnbPayload = BuildXnbPayload(512);

        byte[] folderStream = Concat(exePayload, xnbPayload);
        byte[] compressed = Deflate(folderStream);

        var files = new (string Name, uint Offset, uint Size)[]
        {
            ("0", 0, (uint)exePayload.Length),
            ("1", (uint)exePayload.Length, (uint)xnbPayload.Length),
        };

        const int headerSize = 36;
        const int folderCount = 1;
        const int folderEntrySize = 8;
        int coffFiles = headerSize + (folderEntrySize * folderCount);

        int fileTableSize = files.Sum(f => 16 + Encoding.ASCII.GetByteCount(f.Name) + 1);
        int coffCabStart = coffFiles + fileTableSize;

        int dataBlockSize = 8 + 2 + compressed.Length; // csum+cbData+cbUncomp header + 'CK' + deflate
        int totalSize = coffCabStart + dataBlockSize;

        var data = new byte[totalSize];

        // CFHEADER
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x00), 0x4643534D); // "MSCF"
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x08), (uint)totalSize);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x10), (uint)coffFiles);
        data[0x18] = 3; // version minor
        data[0x19] = 1; // version major
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x1A), folderCount);
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x1C), (ushort)files.Length);
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x1E), 0); // flags
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x20), 0); // set id
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x22), 0); // cabinet index

        // CFFOLDER (immediately before coffFiles)
        int folderOffset = coffFiles - folderEntrySize;
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(folderOffset), (uint)coffCabStart);
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(folderOffset + 4), 1); // block count
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(folderOffset + 6), 1); // typeCompress = MSZIP

        // CFFILE table
        int cursor = coffFiles;
        foreach (var (name, offset, size) in files)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(cursor), size);
            BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(cursor + 4), offset);
            BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(cursor + 8), 0); // folder index
            BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(cursor + 10), 0); // date
            BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(cursor + 12), 0); // time
            BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(cursor + 14), 0); // attributes
            cursor += 16;
            cursor += Encoding.ASCII.GetBytes(name, data.AsSpan(cursor));
            data[cursor++] = 0;
        }

        // CFDATA (one block)
        int blockOffset = coffCabStart;
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(blockOffset), 0); // csum
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(blockOffset + 4), (ushort)(2 + compressed.Length));
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(blockOffset + 6), (ushort)folderStream.Length);
        data[blockOffset + 8] = (byte)'C';
        data[blockOffset + 9] = (byte)'K';
        compressed.CopyTo(data.AsSpan(blockOffset + 10));

        return data;
    }

    public static byte[] BuildMZPayload(int size)
    {
        var payload = new byte[size];
        payload[0] = (byte)'M';
        payload[1] = (byte)'Z';
        for (int i = 2; i < size; i++)
        {
            payload[i] = (byte)(i % 251);
        }

        return payload;
    }

    public static byte[] BuildXnbPayload(int size)
    {
        var payload = new byte[size];
        Encoding.ASCII.GetBytes("XNB", payload);
        for (int i = 3; i < size; i++)
        {
            payload[i] = (byte)(i * 7 % 251);
        }

        return payload;
    }

    private static byte[] Concat(byte[] a, byte[] b)
    {
        var result = new byte[a.Length + b.Length];
        a.CopyTo(result, 0);
        b.CopyTo(result, a.Length);
        return result;
    }

    private static byte[] Deflate(byte[] input)
    {
        using var output = new MemoryStream();
        using (var deflate = new DeflateStream(output, CompressionLevel.Optimal, leaveOpen: true))
        {
            deflate.Write(input, 0, input.Length);
        }

        return output.ToArray();
    }

    // ---- .zcp (NX manifest container) -----------------------------------

    private const int ManifestOffset = 0x2800;
    private const int RecordHeaderSize = 0x1F0;

    /// <summary>
    /// Builds an NX container with <c>EXEC</c>/<c>TITL</c> manifest records.
    /// The payload is left encrypted-equivalent (the reader reports
    /// <c>IsEncrypted</c> with zero entries), which is the manifest-only path.
    /// </summary>
    public static byte[] BuildZcp(
        string guid = "5dc1e614ed1b4d108259bc7e3e926a86",
        string executable = "Calculator.exe",
        string title = "Calculator",
        string description = "Basic and Scientific calculator.")
    {
        var data = new byte[ManifestOffset + RecordHeaderSize + 0x800];

        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0 * 4), 2);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(4 * 4), 0x1F0);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(6 * 4), ManifestOffset);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(7 * 4), 0x1DEF);
        data[0x32] = (byte)'N';
        data[0x33] = (byte)'X';

        int cursor = ManifestOffset + RecordHeaderSize;

        byte[] execPayload = Encoding.ASCII.GetBytes(
            guid.PadRight(32)[..32] + "\0" + executable + "\0Zune.v3.1");
        cursor = WriteRecord(data, cursor, 0x0409, "EXEC"u8, execPayload);

        byte[] titlPayload = Encoding.Unicode.GetBytes(title + "\0" + description);
        WriteRecord(data, cursor, 0, "TITL"u8, titlPayload);

        return data;
    }

    private static int WriteRecord(byte[] data, int offset, ushort lang, ReadOnlySpan<byte> tag, byte[] payload)
    {
        if (lang != 0)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(offset), lang);
            offset += 2;
        }

        tag.CopyTo(data.AsSpan(offset));
        offset += 4;
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(offset), (uint)payload.Length);
        offset += 4;
        payload.CopyTo(data, offset);
        return offset + payload.Length;
    }
}
