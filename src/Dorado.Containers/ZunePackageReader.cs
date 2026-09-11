using Dorado.Containers.Drm;

namespace Dorado.Containers;

/// <summary>Detects and reads Zune application packages.</summary>
public static class ZunePackageReader
{
    /// <summary>Reads a package from a file path or an extracted application directory.</summary>
    public static ZunePackage Read(string path, IDrmKeyProvider? drm = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (Directory.Exists(path))
        {
            return ReadDirectory(path);
        }

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

    /// <summary>
    /// Reads an extracted application tree, such as a decrypted <c>\gametitle\584E07D1</c>
    /// directory or the output of <c>dorado unpack</c>. This is how a user runs
    /// content they extracted from their own device.
    /// </summary>
    public static ZunePackage ReadDirectory(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        string root = FindApplicationRoot(path);
        string[] files = Directory.GetFiles(root, "*", SearchOption.AllDirectories);
        string? executable = files
            .Where(file => file.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(file => string.Equals(Path.GetFileName(file), "default.exe", StringComparison.OrdinalIgnoreCase))
            .ThenBy(file => file, StringComparer.OrdinalIgnoreCase)
            .Select(file => Path.GetRelativePath(root, file))
            .FirstOrDefault();

        var packageFiles = files
            .Select(file => Path.GetRelativePath(root, file).Replace(Path.DirectorySeparatorChar, '/'))
            .Select(relative => new PackageFile(
                relative,
                new FileInfo(Path.Combine(root, relative)).Length,
                relative))
            .ToList();

        string? title = ReadGameInfoValue(root, "Title");
        var metadata = new PackageMetadata
        {
            Title = title ?? Path.GetFileName(root),
            Executable = executable?.Replace(Path.DirectorySeparatorChar, '/'),
            StartupAssembly = executable?.Replace(Path.DirectorySeparatorChar, '/'),
            Platform = "Zune.v3.1",
        };

        return new ZunePackage(
            ContainerKind.Directory,
            metadata,
            packageFiles,
            entryReader: relative =>
            {
                string candidate = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
                return candidate.StartsWith(Path.GetFullPath(root), StringComparison.Ordinal) && File.Exists(candidate)
                    ? File.ReadAllBytes(candidate)
                    : null;
            });
    }

    /// <summary>
    /// Locates the directory that holds the application's executable and
    /// <c>Content</c> folder. Device extractions nest the payload under
    /// <c>gametitle/&lt;content-id&gt;</c>; anything else is used as-is.
    /// </summary>
    private static string FindApplicationRoot(string path)
    {
        string full = Path.GetFullPath(path);
        if (HasExecutable(full))
        {
            return full;
        }

        string? gametitle = Path.Combine(full, "gametitle");
        if (Directory.Exists(gametitle))
        {
            foreach (string candidate in Directory.GetDirectories(gametitle))
            {
                if (HasExecutable(candidate))
                {
                    return candidate;
                }
            }
        }

        foreach (string candidate in Directory.GetDirectories(full))
        {
            if (HasExecutable(candidate))
            {
                return candidate;
            }
        }

        return full;
    }

    private static bool HasExecutable(string directory) =>
        Directory.Exists(directory) &&
        Directory.EnumerateFiles(directory, "*.exe", SearchOption.TopDirectoryOnly).Any();

    private static string? ReadGameInfoValue(string root, string element)
    {
        string? gameInfo = Directory
            .EnumerateFiles(root, "gameinfo.xml", SearchOption.AllDirectories)
            .FirstOrDefault();
        if (gameInfo is null)
        {
            string parent = Path.GetDirectoryName(root) ?? root;
            gameInfo = Directory
                .EnumerateFiles(parent, "gameinfo.xml", SearchOption.TopDirectoryOnly)
                .FirstOrDefault();
        }

        if (gameInfo is null)
        {
            return null;
        }

        try
        {
            var document = System.Xml.Linq.XDocument.Load(gameInfo);
            return document.Descendants(element).FirstOrDefault()?.Value;
        }
        catch (System.Xml.XmlException)
        {
            return null;
        }
    }
}
