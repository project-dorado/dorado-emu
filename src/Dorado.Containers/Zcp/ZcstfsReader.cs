using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Dorado.Containers.Drm;

namespace Dorado.Containers;

/// <summary>
/// Reads ZCSTFS volumes embedded in NX containers (<c>.zcp</c>). This is the
/// mini filesystem mounted by the device's <c>zcstfs.dll</c>; the immutable
/// runtime volume (<c>runtimeZune.v3.1.zcp</c>) ships unencrypted while
/// marketplace packages wrap their data area in AES-ECB.
/// </summary>
/// <remarks>
/// Layout notes (empirically derived from the plaintext runtime volume and
/// cross-checked against the reconstructed <c>zcstfs.dll</c>):
/// <list type="bullet">
/// <item>the NX header stores the hash-region offset at <c>0xD8</c> and the
/// volume descriptor at <c>0xB4</c>;</item>
/// <item>the hash region is an array of <c>0x18</c>-byte entries
/// (<c>sha1[20] + info:u32</c>, one per data block) that doubles as the block
/// chain map (<c>info &amp; 0xFFFFFF</c> is the next block, <c>0xFFFFFF</c>
/// terminates);</item>
/// <item>hash blocks and data blocks are both <c>0x4000</c> bytes; every
/// <c>682</c> blocks needs an additional hash block, summed per level;</item>
/// <item>the data area begins at <c>blockBase = dataOffset + hashBlocks * 0x4000</c>
/// and block <c>N</c> is stored at <c>blockBase + N * 0x4000</c>;</item>
/// <item>directories are STFS-style <c>0x40</c>-byte entries
/// (<c>name[40] + flags + u24 counts + u24 start + u16 parent + u32 length + dates</c>).</item>
/// </list>
/// </remarks>
public static class ZcstfsReader
{
    /// <summary>Data block size (16 KiB).</summary>
    public const int BlockSize = 0x4000;

    /// <summary>Hash entry stride inside the hash region.</summary>
    public const int HashEntrySize = 0x18;

    /// <summary>Number of data blocks covered by one hash block.</summary>
    public const int EntriesPerHashBlock = 682;

    /// <summary>Directory entry size.</summary>
    public const int DirectoryEntrySize = 0x40;

    /// <summary>Sentinel marking the end of a block chain.</summary>
    public const int EndOfChain = 0xFFFFFF;

    private const int NxMagicOffset = 0x32;
    private const int VolumeDescriptorOffset = 0xB4;
    private const int VolumeDescriptorSize = 0x24;
    private const int HashRegionOffset = 0xD8;
    private const int MaxBlocks = 1 << 22;
    private const int MaxFiles = 1 << 16;

    /// <summary>Returns true when the bytes begin with an NX container header.</summary>
    public static bool LooksLikeNx(ReadOnlySpan<byte> data) =>
        data.Length > NxMagicOffset + 1 && data[NxMagicOffset] == (byte)'N' && data[NxMagicOffset + 1] == (byte)'X';

    /// <summary>
    /// Attempts to open the ZCSTFS volume inside <paramref name="data"/>. Returns
    /// <see langword="false"/> when the container is not a ZCSTFS volume; throws
    /// only for structurally corrupt input that claims to be one.
    /// </summary>
    /// <param name="data">Raw <c>.zcp</c> bytes.</param>
    /// <param name="drm">Optional key provider; required for marketplace packages.</param>
    /// <param name="packageGuid">Lower-case hex GUID used to look up a key.</param>
    public static bool TryRead(byte[] data, IDrmKeyProvider? drm, string? packageGuid, out ZcstfsVolume? volume)
    {
        ArgumentNullException.ThrowIfNull(data);
        volume = null;

        if (!LooksLikeNx(data) || data.Length < VolumeDescriptorOffset + VolumeDescriptorSize)
        {
            return false;
        }

        int version = (int)BinaryPrimitives.ReadUInt32LittleEndian(data);
        int descriptorLength = data[VolumeDescriptorOffset];
        if (descriptorLength != VolumeDescriptorSize)
        {
            return false;
        }

        int fileTableBlockCount = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(VolumeDescriptorOffset + 3));
        int fileTableBlockNumber = ReadUInt24(data, VolumeDescriptorOffset + 5);
        byte[] topHash = data.AsSpan(VolumeDescriptorOffset + 8, 20).ToArray();
        int totalBlocks = (int)BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(VolumeDescriptorOffset + 0x1C));
        int freeBlocks = (int)BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(VolumeDescriptorOffset + 0x20));
        int dataOffset = (int)BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(HashRegionOffset));

        if (totalBlocks <= 0 || totalBlocks > MaxBlocks || freeBlocks < 0 || freeBlocks > totalBlocks)
        {
            throw new InvalidDataException($"ZCSTFS volume has an implausible block count ({totalBlocks}).");
        }

        if (fileTableBlockCount <= 0 || fileTableBlockCount > totalBlocks || fileTableBlockNumber >= totalBlocks)
        {
            throw new InvalidDataException("ZCSTFS volume descriptor has an invalid file table location.");
        }

        long hashRegionSize = (long)CountHashBlocks(totalBlocks) * BlockSize;
        long blockBase = dataOffset + hashRegionSize;
        int storedBlocks = totalBlocks - freeBlocks;
        long required = blockBase + ((long)storedBlocks * BlockSize);
        if (dataOffset < VolumeDescriptorOffset + VolumeDescriptorSize || required > data.Length)
        {
            throw new InvalidDataException(
                $"ZCSTFS volume does not fit the container (data={dataOffset}, base={blockBase}, blocks={totalBlocks}, size={data.Length}).");
        }

        bool encrypted = version >= 2;
        byte[]? key = null;
        if (encrypted && drm is not null)
        {
            byte[] guid = string.IsNullOrEmpty(packageGuid) ? [] : Encoding.ASCII.GetBytes(packageGuid);
            byte[]? candidate = drm.TryGetContentKey(guid, data.AsSpan(0, 0x1F0));
            if (candidate is not null && (candidate.Length is 16 or 24 or 32))
            {
                key = candidate;
            }
        }

        if (encrypted && key is null)
        {
            volume = new ZcstfsVolume(
                version, isEncrypted: true, data, dataOffset, blockBase, totalBlocks, freeBlocks,
                fileTableBlockNumber, fileTableBlockCount, topHash, [], []);
            return true;
        }

        // Work on a private copy: the payload region is decrypted in place when a
        // key is available, and the caller owns its original buffer.
        byte[] buffer = (byte[])data.Clone();
        if (key is not null)
        {
            long encryptedEnd = Math.Min(buffer.Length, blockBase + ((long)storedBlocks * BlockSize));
            long encryptedLength = encryptedEnd - dataOffset;
            if (encryptedLength <= 0)
            {
                throw new InvalidDataException("ZCSTFS volume has an empty encrypted region.");
            }

            long aligned = encryptedLength & ~0xF;
            DecryptEcb(buffer, dataOffset, checked((int)aligned), key);
        }

        List<HashEntry> entries = ReadHashEntries(buffer, dataOffset, totalBlocks);
        List<Node> nodes = new();
        ReadDirectoryChain(buffer, blockBase, entries, totalBlocks, fileTableBlockNumber, fileTableBlockCount, nodes);
        List<ZcstfsFile> files = BuildTree(nodes, entries, totalBlocks);

        volume = new ZcstfsVolume(
            version, isEncrypted: false, buffer, dataOffset, blockBase, totalBlocks, freeBlocks,
            fileTableBlockNumber, fileTableBlockCount, topHash, entries, files);
        return true;
    }

    /// <summary>Number of 0x4000 hash blocks required to index <paramref name="totalBlocks"/> blocks.</summary>
    public static int CountHashBlocks(int totalBlocks)
    {
        int level0 = Math.Max(1, (totalBlocks + EntriesPerHashBlock - 1) / EntriesPerHashBlock);
        int count = level0;
        int level = level0;
        while (level > 1)
        {
            level = (level + EntriesPerHashBlock - 1) / EntriesPerHashBlock;
            count += level;
        }

        // The top-level hash table is always present, even when a single
        // level-0 block could cover the whole volume.
        if (count == level0)
        {
            count++;
        }

        return count;
    }

    private static List<HashEntry> ReadHashEntries(byte[] data, int offset, int count)
    {
        var entries = new List<HashEntry>(count);
        for (int i = 0; i < count; i++)
        {
            int entryOffset = offset + (i * HashEntrySize);
            byte[] sha1 = data.AsSpan(entryOffset, 20).ToArray();
            uint info = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(entryOffset + 20));
            entries.Add(new HashEntry(sha1, info));
        }

        return entries;
    }

    private static void ReadDirectoryChain(
        byte[] data,
        long blockBase,
        List<HashEntry> entries,
        int totalBlocks,
        int firstBlock,
        int blockCount,
        List<Node> nodes)
    {
        int block = firstBlock;
        for (int i = 0; i < blockCount && block != EndOfChain; i++)
        {
            if (block < 0 || block >= totalBlocks)
            {
                throw new InvalidDataException("ZCSTFS directory chain references an out-of-range block.");
            }

            long offset = blockBase + ((long)block * BlockSize);
            if (offset < 0 || offset + BlockSize > data.Length)
            {
                throw new InvalidDataException("ZCSTFS directory block falls outside the container.");
            }

            ParseDirectoryBlock(data.AsSpan((int)offset, BlockSize), nodes);
            block = entries[block].NextBlock;
        }

        if (nodes.Count > MaxFiles)
        {
            throw new InvalidDataException("ZCSTFS directory contains an implausible number of entries.");
        }
    }

    private static void ParseDirectoryBlock(ReadOnlySpan<byte> block, List<Node> nodes)
    {
        for (int i = 0; i < block.Length / DirectoryEntrySize; i++)
        {
            ReadOnlySpan<byte> entry = block.Slice(i * DirectoryEntrySize, DirectoryEntrySize);
            if (entry[0] == 0)
            {
                continue;
            }

            int nameLength = entry[40] & 0x3F;
            if (nameLength is <= 0 or > 40)
            {
                throw new InvalidDataException("ZCSTFS directory entry has an invalid name length.");
            }

            string name = Encoding.ASCII.GetString(entry[..nameLength]);
            if (name.Length == 0 || name.IndexOfAny(['/', '\\', '\0']) >= 0)
            {
                throw new InvalidDataException("ZCSTFS directory entry has an unsafe name.");
            }

            bool isDirectory = (entry[40] & 0x80) != 0;
            int startBlock = ReadUInt24(entry, 47);
            int parentIndex = BinaryPrimitives.ReadUInt16LittleEndian(entry[50..]);
            uint length = BinaryPrimitives.ReadUInt32LittleEndian(entry[52..]);

            nodes.Add(new Node(name, isDirectory, startBlock, parentIndex, length));
        }
    }

    private static List<ZcstfsFile> BuildTree(List<Node> nodes, List<HashEntry> entries, int totalBlocks)
    {
        var files = new List<ZcstfsFile>();
        for (int i = 0; i < nodes.Count; i++)
        {
            Node node = nodes[i];
            if (node.IsDirectory)
            {
                continue;
            }

            string path = BuildPath(nodes, i);
            List<int> chain = WalkChain(entries, totalBlocks, node.StartBlock, node.Length);
            files.Add(new ZcstfsFile(path, node.Length, node.StartBlock, chain));
        }

        return files;
    }

    private static string BuildPath(List<Node> nodes, int index)
    {
        var segments = new List<string>();
        int current = index;
        int guard = 0;
        while (current >= 0 && current < nodes.Count && guard++ <= MaxFiles)
        {
            Node node = nodes[current];
            segments.Add(node.Name);
            int parent = node.ParentIndex;
            if (parent == 0xFFFF)
            {
                break;
            }

            if (parent >= nodes.Count || parent == current)
            {
                throw new InvalidDataException("ZCSTFS directory entry has an invalid parent reference.");
            }

            current = parent;
        }

        segments.Reverse();
        return string.Join('/', segments);
    }

    private static List<int> WalkChain(List<HashEntry> entries, int totalBlocks, int startBlock, uint length)
    {
        var chain = new List<int>();
        long remaining = length;
        int block = startBlock;
        int guard = 0;
        while (remaining > 0 && block != EndOfChain)
        {
            if (block < 0 || block >= totalBlocks || guard++ > totalBlocks)
            {
                throw new InvalidDataException("ZCSTFS file block chain is corrupt.");
            }

            chain.Add(block);
            remaining -= Math.Min(BlockSize, remaining);
            block = entries[block].NextBlock;
        }

        if (remaining != 0)
        {
            throw new InvalidDataException("ZCSTFS file block chain ended before the declared length.");
        }

        return chain;
    }

    /// <summary>Decrypts an AES-ECB region (no padding) in place.</summary>
    public static void DecryptEcb(byte[] buffer, int offset, int length, ReadOnlySpan<byte> key)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        if (key.Length is not (16 or 24 or 32))
        {
            throw new ArgumentException("AES key must be 16, 24 or 32 bytes.", nameof(key));
        }

        if (length <= 0 || offset < 0 || offset + length > buffer.Length || (length & 0xF) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        using Aes aes = Aes.Create();
        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.None;
        aes.Key = key.ToArray();
        using ICryptoTransform decryptor = aes.CreateDecryptor();
        _ = decryptor.TransformBlock(buffer, offset, length, buffer, offset);
    }

    private static int ReadUInt24(ReadOnlySpan<byte> data, int offset) =>
        data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16);

    private static int ReadUInt24(byte[] data, int offset) =>
        data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16);

    /// <summary>One 0x18-byte hash/chain entry.</summary>
    public readonly record struct HashEntry(byte[] Sha1, uint Info)
    {
        /// <summary>Allocation state stored in the top two bits of <see cref="Info"/>.</summary>
        public int State => (int)((Info >> 30) & 0x3);

        /// <summary>Index of the next block in the chain, or <see cref="EndOfChain"/>.</summary>
        public int NextBlock => (int)(Info & 0xFFFFFF);
    }

    private sealed record Node(string Name, bool IsDirectory, int StartBlock, int ParentIndex, uint Length);

    /// <summary>A file inside a ZCSTFS volume.</summary>
    /// <param name="Path">Slash-separated logical path.</param>
    /// <param name="Length">Declared file length in bytes.</param>
    /// <param name="StartBlock">First data block index.</param>
    /// <param name="Blocks">Ordered block chain.</param>
    public sealed record ZcstfsFile(string Path, uint Length, int StartBlock, IReadOnlyList<int> Blocks);
}

/// <summary>A parsed ZCSTFS volume.</summary>
public sealed class ZcstfsVolume
{
    internal ZcstfsVolume(
        int version,
        bool isEncrypted,
        byte[] data,
        int dataOffset,
        long blockBase,
        int totalBlockCount,
        int freeBlockCount,
        int fileTableBlockNumber,
        int fileTableBlockCount,
        byte[] topHashTableHash,
        IReadOnlyList<ZcstfsReader.HashEntry> hashEntries,
        IReadOnlyList<ZcstfsReader.ZcstfsFile> files)
    {
        Version = version;
        IsEncrypted = isEncrypted;
        Data = data;
        DataOffset = dataOffset;
        BlockBase = blockBase;
        TotalBlockCount = totalBlockCount;
        FreeBlockCount = freeBlockCount;
        FileTableBlockNumber = fileTableBlockNumber;
        FileTableBlockCount = fileTableBlockCount;
        TopHashTableHash = topHashTableHash;
        HashEntries = hashEntries;
        Files = files;
    }

    /// <summary>NX format version (1 = plaintext runtime volume, 2 = marketplace).</summary>
    public int Version { get; }

    /// <summary>True when the data area is still encrypted (no usable key).</summary>
    public bool IsEncrypted { get; }

    /// <summary>Container bytes with the data area decrypted in place when a key was supplied.</summary>
    public byte[] Data { get; }

    /// <summary>Offset of the hash region.</summary>
    public int DataOffset { get; }

    /// <summary>Offset of data block zero.</summary>
    public long BlockBase { get; }

    /// <summary>Total number of 0x4000-byte data blocks.</summary>
    public int TotalBlockCount { get; }

    /// <summary>Number of free blocks reported by the volume descriptor.</summary>
    public int FreeBlockCount { get; }

    /// <summary>First file-table (directory) block.</summary>
    public int FileTableBlockNumber { get; }

    /// <summary>Number of directory blocks.</summary>
    public int FileTableBlockCount { get; }

    /// <summary>SHA-1 of the top-level hash table (header field, plaintext).</summary>
    public byte[] TopHashTableHash { get; }

    /// <summary>Per-block hash/chain entries.</summary>
    public IReadOnlyList<ZcstfsReader.HashEntry> HashEntries { get; }

    /// <summary>Files discovered in the directory tree.</summary>
    public IReadOnlyList<ZcstfsReader.ZcstfsFile> Files { get; }

    /// <summary>Returns the offset of <paramref name="block"/> inside <see cref="Data"/>.</summary>
    public long BlockOffset(int block) => BlockBase + ((long)block * ZcstfsReader.BlockSize);

    /// <summary>Reads a file's bytes from the volume.</summary>
    public byte[] ReadFile(ZcstfsReader.ZcstfsFile file)
    {
        ArgumentNullException.ThrowIfNull(file);
        var result = new byte[file.Length];
        int written = 0;
        foreach (int block in file.Blocks)
        {
            int take = Math.Min(ZcstfsReader.BlockSize, result.Length - written);
            if (take <= 0)
            {
                break;
            }

            long offset = BlockOffset(block);
            if (offset < 0 || offset + take > Data.Length)
            {
                throw new InvalidDataException("ZCSTFS file block falls outside the container.");
            }

            Array.Copy(Data, (int)offset, result, written, take);
            written += take;
        }

        return result;
    }
}
