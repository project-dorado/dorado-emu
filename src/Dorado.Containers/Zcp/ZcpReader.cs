using System.Buffers.Binary;
using System.Text;
using Dorado.Containers.Drm;

namespace Dorado.Containers;

/// <summary>Reads marketplace NX containers (<c>.zcp</c>).</summary>
public static class ZcpReader
{
    private const int NxMagicOffset = 0x32;
    private const int RecordHeaderSize = 0x1F0;
    private const int ManifestScanWindow = 0x2000;

    /// <summary>Returns true when the bytes are an NX container.</summary>
    public static bool LooksLikeNx(ReadOnlySpan<byte> data) =>
        data.Length > NxMagicOffset + 1 && data[NxMagicOffset] == (byte)'N' && data[NxMagicOffset + 1] == (byte)'X';

    /// <summary>Reads a <c>.zcp</c> from a file path.</summary>
    public static ZunePackage Read(string path, IDrmKeyProvider? drm = null)
    {
        using var stream = File.OpenRead(path);
        return Read(stream, drm);
    }

    /// <summary>Reads a <c>.zcp</c> from a stream.</summary>
    public static ZunePackage Read(Stream stream, IDrmKeyProvider? drm = null)
    {
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return Read(buffer.ToArray(), drm);
    }

    /// <summary>Reads a <c>.zcp</c> from bytes.</summary>
    public static ZunePackage Read(byte[] data, IDrmKeyProvider? drm = null)
    {
        if (!LooksLikeNx(data))
        {
            throw new InvalidDataException("Not a .zcp container (missing 'NX' magic at 0x32).");
        }

        if (data.Length < 8 * sizeof(uint) + RecordHeaderSize)
        {
            throw new InvalidDataException("NX container is truncated.");
        }

        var header = new uint[8];
        for (int i = 0; i < header.Length; i++)
        {
            header[i] = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(i * sizeof(uint)));
        }

        int manifestOffset = checked((int)header[6]);
        int recordStart = manifestOffset + RecordHeaderSize;
        int limit = Math.Min(data.Length, manifestOffset + ManifestScanWindow);

        var exec = FindTag(data, recordStart, limit, "EXEC"u8);
        var titl = FindTag(data, recordStart, limit, "TITL"u8);

        string? executable = null;
        string? platform = null;
        string? guid = null;
        if (exec >= 0)
        {
            var payload = ReadPayload(data, exec);
            var ascii = Encoding.ASCII.GetString(payload);
            int guidEnd = ascii.IndexOf('\0');
            if (guidEnd == 32)
            {
                guid = ascii[..32];
                int exeStart = guidEnd + 1;
                int exeEnd = ascii.IndexOf('\0', exeStart);
                if (exeEnd > exeStart)
                {
                    executable = ascii[exeStart..exeEnd];
                }
            }

            int zuneIndex = ascii.IndexOf("Zune.v", StringComparison.Ordinal);
            if (zuneIndex >= 0)
            {
                int zuneEnd = zuneIndex;
                while (zuneEnd < ascii.Length && (char.IsLetterOrDigit(ascii[zuneEnd]) || ascii[zuneEnd] is '.' or '_'))
                {
                    zuneEnd++;
                }

                platform = ascii[zuneIndex..zuneEnd];
            }
        }

        string? title = null;
        string? description = null;
        if (titl >= 0)
        {
            var payload = ReadPayload(data, titl);
            (title, description) = SplitTitleDescription(payload);
        }

        var metadata = new PackageMetadata
        {
            Title = title,
            Description = description,
            Executable = executable,
            StartupAssembly = executable,
            Platform = platform,
            GameGuid = guid,
        };

        var drmGuid = guid is null ? ReadOnlySpan<byte>.Empty : Encoding.ASCII.GetBytes(guid);
        byte[]? key = drm?.TryGetContentKey(drmGuid, data.AsSpan(0, RecordHeaderSize));

        // Payload decryption (ZCSTFS) is future work (M3/M4). Without a key the
        // encrypted payload is unreadable; expose manifest metadata only.
        var files = executable is null
            ? Array.Empty<PackageFile>()
            : new[] { new PackageFile(executable, -1, executable) };

        return new ZunePackage(ContainerKind.Zcp, metadata, files, null, isEncrypted: key is null);
    }

    private static int FindTag(byte[] data, int start, int limit, ReadOnlySpan<byte> tag)
    {
        if (start < 0 || limit <= start || limit > data.Length)
        {
            return -1;
        }

        int index = data.AsSpan(start, limit - start).IndexOf(tag);
        return index < 0 ? -1 : start + index;
    }

    private static ReadOnlySpan<byte> ReadPayload(byte[] data, int tagOffset)
    {
        int lengthOffset = tagOffset + 4;
        if (lengthOffset + 4 > data.Length)
        {
            return ReadOnlySpan<byte>.Empty;
        }

        uint length = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(lengthOffset));
        int start = lengthOffset + 4;
        if (length == 0 || start + length > data.Length)
        {
            return ReadOnlySpan<byte>.Empty;
        }

        return data.AsSpan(start, (int)length);
    }

    private static (string? Title, string? Description) SplitTitleDescription(ReadOnlySpan<byte> payload)
    {
        var text = Encoding.Unicode.GetString(payload);
        int nul = text.IndexOf('\0');
        if (nul < 0)
        {
            return (text.Length > 0 ? text : null, null);
        }

        string title = text[..nul];
        string rest = text[(nul + 1)..].TrimStart('\0');
        int nextNul = rest.IndexOf('\0');
        string description = nextNul < 0 ? rest : rest[..nextNul];
        return (title.Length > 0 ? title : null, description.Length > 0 ? description : null);
    }
}
