using Dorado.Containers.Drm;

namespace Dorado.Containers;

/// <summary>Detects and reads Zune application packages.</summary>
public static class ZunePackageReader
{
    /// <summary>Reads a package from a file path.</summary>
    public static ZunePackage Read(string path, IDrmKeyProvider? drm = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        byte[] data = File.ReadAllBytes(path);
        return Read(data, drm);
    }

    /// <summary>Reads a package from bytes, detecting the container type.</summary>
    public static ZunePackage Read(byte[] data, IDrmKeyProvider? drm = null)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (CcgameReader.LooksLikeCab(data))
        {
            return CcgameReader.Read(data);
        }

        if (ZcpReader.LooksLikeNx(data))
        {
            return ZcpReader.Read(data, drm);
        }

        throw new InvalidDataException(
            "Unrecognised package: neither an MSCF cabinet (.ccgame) nor an NX container (.zcp).");
    }
}
