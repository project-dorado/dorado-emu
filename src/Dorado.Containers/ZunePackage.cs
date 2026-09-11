using System.Diagnostics.CodeAnalysis;

namespace Dorado.Containers;

/// <summary>
/// A parsed Zune application package. For unencrypted packages <see cref="Entries"/>
/// contains the decompressed payload files; for encrypted <c>.zcp</c> packages it is
/// empty and <see cref="IsEncrypted"/> is <see langword="true"/>.
/// </summary>
public sealed class ZunePackage
{
    private readonly Dictionary<string, byte[]> _entries;
    private readonly Func<string, byte[]?>? _entryReader;

    internal ZunePackage(
        ContainerKind kind,
        PackageMetadata metadata,
        IEnumerable<PackageFile> files,
        IEnumerable<KeyValuePair<string, byte[]>>? entries = null,
        bool isEncrypted = false,
        Func<string, byte[]?>? entryReader = null,
        ZcstfsVolume? volume = null)
    {
        Kind = kind;
        Metadata = metadata;
        Files = files.ToArray();
        _entries = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        if (entries is not null)
        {
            foreach (var (path, data) in entries)
            {
                _entries[path] = data;
            }
        }

        _entryReader = entryReader;
        IsEncrypted = isEncrypted;
        Volume = volume;
    }

    /// <summary>Container type.</summary>
    public ContainerKind Kind { get; }

    /// <summary>Manifest metadata.</summary>
    public PackageMetadata Metadata { get; }

    /// <summary>Files described by the package, whether or not their bytes are available.</summary>
    public IReadOnlyList<PackageFile> Files { get; }

    /// <summary>True when the payload is DRM-encrypted and therefore not readable.</summary>
    public bool IsEncrypted { get; }

    /// <summary>The parsed ZCSTFS volume for <c>.zcp</c> packages, when present.</summary>
    public ZcstfsVolume? Volume { get; }

    /// <summary>True when payload bytes can be produced for the declared files.</summary>
    public bool CanReadEntries => !IsEncrypted && (_entryReader is not null || _entries.Count > 0);

    /// <summary>Number of payload files whose bytes are available (or readable on demand).</summary>
    public int EntryCount => _entryReader is not null ? Files.Count : _entries.Count;

    /// <summary>Gets a payload file by logical path.</summary>
    public bool TryGetEntry(string path, [NotNullWhen(true)] out byte[]? data)
    {
        if (_entries.TryGetValue(path, out var value))
        {
            data = value;
            return true;
        }

        if (_entryReader is not null)
        {
            value = _entryReader(path);
            if (value is not null)
            {
                _entries[path] = value;
                data = value;
                return true;
            }
        }

        data = null;
        return false;
    }

    /// <summary>Gets a payload file by logical path or throws.</summary>
    public byte[] GetEntry(string path) =>
        TryGetEntry(path, out byte[]? value)
            ? value
            : throw new KeyNotFoundException($"Package has no entry '{path}'.");
}
