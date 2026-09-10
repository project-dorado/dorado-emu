using System.Text;
using Dorado.Containers.Ccgame;
using Dorado.Containers.Internal;

namespace Dorado.Containers;

/// <summary>Reads XNA deployment cabinets (<c>.ccgame</c>).</summary>
public static class CcgameReader
{
    private const string XcabInfoEntry = "XCabInfo.resources";

    /// <summary>Returns true when the bytes begin with the MSCF cabinet signature.</summary>
    public static bool LooksLikeCab(ReadOnlySpan<byte> data) =>
        data.Length >= 4 && data[0] == (byte)'M' && data[1] == (byte)'S' && data[2] == (byte)'C' && data[3] == (byte)'F';

    /// <summary>Reads a <c>.ccgame</c> cabinet from a file path.</summary>
    public static ZunePackage Read(string path)
    {
        using var stream = File.OpenRead(path);
        return Read(stream);
    }

    /// <summary>Reads a <c>.ccgame</c> cabinet from a stream.</summary>
    public static ZunePackage Read(Stream stream)
    {
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return Read(buffer.ToArray());
    }

    /// <summary>Reads a <c>.ccgame</c> cabinet from bytes.</summary>
    public static ZunePackage Read(byte[] data)
    {
        CabArchive archive = CabArchive.Parse(data);
        IReadOnlyList<KeyValuePair<string, byte[]>> raw = archive.ReadAllFiles();

        XcabInfo? info = null;
        foreach (var (name, payload) in raw)
        {
            if (name.Equals(XcabInfoEntry, StringComparison.OrdinalIgnoreCase))
            {
                info = XcabInfo.Parse(payload);
                break;
            }
        }

        var files = new List<PackageFile>();
        var entries = new List<KeyValuePair<string, byte[]>>();
        foreach (var (name, payload) in raw)
        {
            if (name.Equals(XcabInfoEntry, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string path = info is not null && info.EntryToPath.TryGetValue(name, out var mapped)
                ? mapped
                : FallbackPath(name, payload);

            files.Add(new PackageFile(path, payload.Length, name));
            entries.Add(new KeyValuePair<string, byte[]>(path, payload));
        }

        var metadata = new PackageMetadata
        {
            Title = info?.Title,
            Description = info?.Description,
            Copyright = info?.Copyright,
            Executable = info?.StartupAssembly,
            StartupAssembly = info?.StartupAssembly,
            Platform = info?.Platform,
            RuntimeProfile = info?.RuntimeProfile,
            GameGuid = info?.GameGuid,
            CcgameVersion = info?.CcgameVersion,
        };

        return new ZunePackage(ContainerKind.Ccgame, metadata, files, entries);
    }

    private static string FallbackPath(string containerName, byte[] payload)
    {
        if (payload.Length >= 2 && payload[0] == (byte)'M' && payload[1] == (byte)'Z')
        {
            return $"{containerName}.exe";
        }

        if (payload.Length >= 3 && Encoding.ASCII.GetString(payload, 0, 3) == "XNB")
        {
            return $"{containerName}.xnb";
        }

        return containerName;
    }
}
