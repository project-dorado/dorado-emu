using System.Buffers.Binary;
using System.Text;

namespace Dorado.Containers.Internal;

internal sealed class CabFolder
{
    public uint CabStart { get; init; }

    public int BlockCount { get; init; }

    public ushort Compression { get; init; }
}

internal sealed class CabFileEntry
{
    public string? Name { get; init; }

    public uint Size { get; init; }

    public uint FolderOffset { get; init; }

    public int Folder { get; init; }
}

/// <summary>
/// Minimal Microsoft Cabinet (MSCF) reader supporting stored and MSZIP folders.
/// XNA cabinets (<c>.ccgame</c>) omit the optional prev/next strings even when the
/// flag bits are set, so the folder table is anchored at <c>coffFiles</c>.
/// </summary>
internal sealed class CabArchive
{
    private const uint Mscf = 0x4643534D; // "MSCF"
    private const int HeaderSize = 36;
    private const int FolderEntrySize = 8;
    private const int FileEntrySize = 16;
    private const int WindowSize = 32768;

    private readonly byte[] _data;
    private readonly int _reserveDataSize;

    private CabArchive(byte[] data, int reserveDataSize, CabFolder[] folders, CabFileEntry[] files)
    {
        _data = data;
        _reserveDataSize = reserveDataSize;
        Folders = folders;
        Files = files;
    }

    public IReadOnlyList<CabFolder> Folders { get; }

    public IReadOnlyList<CabFileEntry> Files { get; }

    public static CabArchive Parse(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (data.Length < HeaderSize || BinaryPrimitives.ReadUInt32LittleEndian(data) != Mscf)
        {
            throw new InvalidDataException("Not an MSCF cabinet.");
        }

        uint coffFiles = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(0x10));
        int folderCount = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(0x1A));
        int fileCount = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(0x1C));
        ushort flags = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(0x1E));

        int offset = HeaderSize;
        int reserveDataSize = 0;
        if ((flags & 0x0004) != 0)
        {
            if (offset + 4 > data.Length)
            {
                throw new InvalidDataException("Truncated cabinet reserve header.");
            }

            int cbCFHeader = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(offset));
            reserveDataSize = data[offset + 3];
            offset += 4 + cbCFHeader;
        }

        long folderTableOffset = (long)coffFiles - (FolderEntrySize * folderCount);
        if (folderTableOffset < offset || folderTableOffset + (FolderEntrySize * folderCount) > data.Length)
        {
            throw new InvalidDataException("Cabinet folder table is out of range.");
        }

        var folders = new CabFolder[folderCount];
        for (int i = 0; i < folderCount; i++)
        {
            int p = (int)folderTableOffset + (FolderEntrySize * i);
            folders[i] = new CabFolder
            {
                CabStart = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(p)),
                BlockCount = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(p + 4)),
                Compression = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(p + 6)),
            };
        }

        if (coffFiles == 0xFFFFFFFF || (long)coffFiles + (FileEntrySize * (long)fileCount) > data.Length)
        {
            throw new InvalidDataException("Cabinet file table is out of range.");
        }

        bool namesInData = (flags & 0x0004) != 0;
        int cursor = (int)coffFiles;
        var files = new CabFileEntry[fileCount];
        for (int i = 0; i < fileCount; i++)
        {
            if (cursor + FileEntrySize > data.Length)
            {
                throw new InvalidDataException("Truncated cabinet file entry.");
            }

            uint size = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(cursor));
            uint folderOffset = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(cursor + 4));
            int folder = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(cursor + 8));
            cursor += FileEntrySize;

            string? name = null;
            if (!namesInData)
            {
                int end = Array.IndexOf(data, (byte)0, cursor);
                if (end < 0)
                {
                    throw new InvalidDataException("Unterminated cabinet file name.");
                }

                name = Encoding.ASCII.GetString(data, cursor, end - cursor);
                cursor = end + 1;
            }

            files[i] = new CabFileEntry
            {
                Name = name,
                Size = size,
                FolderOffset = folderOffset,
                Folder = folder,
            };
        }

        return new CabArchive(data, reserveDataSize, folders, files);
    }

    /// <summary>Reads and decompresses a folder's stream.</summary>
    public byte[] ReadFolder(int index)
    {
        CabFolder folder = Folders[index];
        using var output = new MemoryStream();
        byte[] window = Array.Empty<byte>();
        int p = checked((int)folder.CabStart);

        for (int block = 0; block < folder.BlockCount; block++)
        {
            if (p + 8 > _data.Length)
            {
                throw new InvalidDataException("Truncated cabinet data block.");
            }

            int cbData = BinaryPrimitives.ReadUInt16LittleEndian(_data.AsSpan(p + 4));
            int cbUncomp = BinaryPrimitives.ReadUInt16LittleEndian(_data.AsSpan(p + 6));
            int dataStart = p + 8;
            if (dataStart + cbData + _reserveDataSize > _data.Length)
            {
                throw new InvalidDataException("Cabinet data block is out of range.");
            }

            switch (folder.Compression)
            {
                case 0:
                    output.Write(_data, dataStart, cbData);
                    break;

                case 1:
                {
                    if (cbData < 2 || _data[dataStart] != (byte)'C' || _data[dataStart + 1] != (byte)'K')
                    {
                        throw new InvalidDataException("MSZIP block is missing its 'CK' signature.");
                    }

                    int payloadLength = cbData - 2;
                    var payload = new byte[payloadLength];
                    Array.Copy(_data, dataStart + 2, payload, 0, payloadLength);
                    byte[] inflated = Puff.Inflate(payload, payloadLength, window, cbUncomp);
                    output.Write(inflated, 0, inflated.Length);
                    break;
                }

                default:
                    throw new NotSupportedException(
                        $"Cabinet compression type {folder.Compression} (Quantum/LZX) is not supported.");

            }

            window = Tail(output.GetBuffer(), checked((int)output.Length), WindowSize);
            p = dataStart + cbData + _reserveDataSize;
        }

        return output.ToArray();
    }

    /// <summary>Extracts every named file, preserving cabinet order.</summary>
    public IReadOnlyList<KeyValuePair<string, byte[]>> ReadAllFiles()
    {
        var streams = new byte[Folders.Count][];
        var result = new List<KeyValuePair<string, byte[]>>(Files.Count);
        for (int i = 0; i < Files.Count; i++)
        {
            CabFileEntry file = Files[i];
            if (file.Folder < 0 || file.Folder >= Folders.Count)
            {
                throw new InvalidDataException("Cabinet file references an invalid folder.");
            }

            byte[] folder = streams[file.Folder] ??= ReadFolder(file.Folder);
            if (file.FolderOffset + file.Size > folder.Length)
            {
                throw new InvalidDataException($"Cabinet entry '{file.Name}' is out of range.");
            }

            var payload = new byte[file.Size];
            Array.Copy(folder, file.FolderOffset, payload, 0, file.Size);
            string name = file.Name ?? i.ToString(System.Globalization.CultureInfo.InvariantCulture);
            result.Add(new KeyValuePair<string, byte[]>(name, payload));
        }

        return result;
    }

    private static byte[] Tail(byte[] buffer, int length, int max)
    {
        int count = Math.Min(max, length);
        var window = new byte[count];
        Array.Copy(buffer, length - count, window, 0, count);
        return window;
    }
}
