using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;
using Dorado.Containers;

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

    // ---- .zcp (ZCSTFS volume) -------------------------------------------

    /// <summary>Layout shared by the synthetic volume builder and its tests.</summary>
    internal static class Zcstfs
    {
        public const int DataOffset = 0x1000;
        public const int TotalBlocks = 4;
        public const int BlockBase = DataOffset + (2 * ZcstfsReader.BlockSize);

        public const string FirstPath = "Content/a.xnb";
        public const int FirstLength = 0x5000;
        public const string SecondPath = "b.exe";
        public const int SecondLength = 0x40;
    }

    /// <summary>
    /// Builds a minimal but valid ZCSTFS volume in an NX container. The volume
    /// has a root directory plus one subdirectory, a two-block file
    /// (<c>Content/a.xnb</c>) and a single-block file (<c>b.exe</c>). When
    /// <paramref name="encrypt"/> is set the data area is AES-256-ECB encrypted
    /// with <paramref name="key"/>.
    /// </summary>
    public static byte[] BuildZcstfs(bool encrypt = false, byte[]? key = null)
    {
        int size = Zcstfs.BlockBase + (Zcstfs.TotalBlocks * ZcstfsReader.BlockSize);
        var data = new byte[size];

        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0 * 4), encrypt ? 2u : 1u);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(1 * 4), 1u);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(2 * 4), 2u);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(4 * 4), 0x1F0u);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(6 * 4), 0x2800u);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(7 * 4), 0x1DE0u);
        data[0x32] = (byte)'N';
        data[0x33] = (byte)'X';

        if (encrypt)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x44), 2u);
            BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x48), 1u);
            BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x4C), 0x100u);
        }

        // Volume descriptor at 0xB4.
        const int vd = 0xB4;
        data[vd] = 0x24;
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(vd + 3), 1); // file table block count
        // file table block number (0) at vd + 5
        // top hash (20 bytes) at vd + 8 (left zero: only used as an oracle)
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(vd + 0x1C), Zcstfs.TotalBlocks);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(vd + 0x20), 0);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0xD8), Zcstfs.DataOffset);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0xE0), 0x80000000);

        // Data blocks.
        byte[] first = BuildPattern(Zcstfs.FirstLength, 0x11);
        byte[] second = BuildPattern(Zcstfs.SecondLength, 0x77);
        int firstBlock0 = Zcstfs.BlockBase + (1 * ZcstfsReader.BlockSize);
        int firstBlock1 = Zcstfs.BlockBase + (2 * ZcstfsReader.BlockSize);
        int secondBlock = Zcstfs.BlockBase + (3 * ZcstfsReader.BlockSize);
        first.AsSpan(0, ZcstfsReader.BlockSize).CopyTo(data.AsSpan(firstBlock0));
        first.AsSpan(ZcstfsReader.BlockSize).CopyTo(data.AsSpan(firstBlock1));
        second.CopyTo(data.AsSpan(secondBlock));

        // Directory block 0.
        int directory = Zcstfs.BlockBase;
        WriteDirectoryEntry(data.AsSpan(directory), "Content", isDirectory: true, startBlock: 0, parent: 0xFFFF, length: 0);
        WriteDirectoryEntry(
            data.AsSpan(directory + ZcstfsReader.DirectoryEntrySize),
            "a.xnb",
            isDirectory: false,
            startBlock: 1,
            parent: 0,
            length: Zcstfs.FirstLength,
            blocks: 2);
        WriteDirectoryEntry(
            data.AsSpan(directory + (2 * ZcstfsReader.DirectoryEntrySize)),
            "b.exe",
            isDirectory: false,
            startBlock: 3,
            parent: 0xFFFF,
            length: Zcstfs.SecondLength,
            blocks: 1);

        // Hash/chain entries.
        int[] chain = [ZcstfsReader.EndOfChain, 2, ZcstfsReader.EndOfChain, ZcstfsReader.EndOfChain];
        for (int i = 0; i < Zcstfs.TotalBlocks; i++)
        {
            int entry = Zcstfs.DataOffset + (i * ZcstfsReader.HashEntrySize);
            byte[] block = data.AsSpan(Zcstfs.BlockBase + (i * ZcstfsReader.BlockSize), ZcstfsReader.BlockSize).ToArray();
            System.Security.Cryptography.SHA1.HashData(block, data.AsSpan(entry, 20));
            uint info = 0xC0000000u | (uint)chain[i];
            BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(entry + 20), info);
        }

        if (encrypt)
        {
            key ??= new byte[32];
            int encryptedLength = Zcstfs.BlockBase + (Zcstfs.TotalBlocks * ZcstfsReader.BlockSize) - Zcstfs.DataOffset;
            EncryptEcb(data, Zcstfs.DataOffset, encryptedLength, key);
        }

        return data;
    }

    private static void WriteDirectoryEntry(
        Span<byte> entry,
        string name,
        bool isDirectory,
        int startBlock,
        int parent,
        uint length,
        int blocks = 0)
    {
        entry[..ZcstfsReader.DirectoryEntrySize].Clear();
        Encoding.ASCII.GetBytes(name, entry);
        entry[40] = (byte)(name.Length | (isDirectory ? 0x80 : 0));
        entry[41] = (byte)blocks;
        entry[44] = (byte)blocks;
        entry[47] = (byte)startBlock;
        entry[48] = (byte)(startBlock >> 8);
        entry[49] = (byte)(startBlock >> 16);
        BinaryPrimitives.WriteUInt16LittleEndian(entry[50..], (ushort)parent);
        BinaryPrimitives.WriteUInt32LittleEndian(entry[52..], length);
    }

    private static byte[] BuildPattern(int length, byte seed)
    {
        var payload = new byte[length];
        for (int i = 0; i < length; i++)
        {
            payload[i] = (byte)(seed + (i % 251));
        }

        return payload;
    }

    /// <summary>AES-ECB encryption (no padding) used to build encrypted fixtures.</summary>
    public static void EncryptEcb(byte[] buffer, int offset, int length, byte[] key)
    {
        using var aes = System.Security.Cryptography.Aes.Create();
        aes.Mode = System.Security.Cryptography.CipherMode.ECB;
        aes.Padding = System.Security.Cryptography.PaddingMode.None;
        aes.Key = key;
        using var encryptor = aes.CreateEncryptor();
        _ = encryptor.TransformBlock(buffer, offset, length, buffer, offset);
    }
}
