using System.Resources;
using System.Runtime.Serialization.Formatters.Binary;
using Dorado.Containers.Internal;

namespace Dorado.Containers.Ccgame;

/// <summary>
/// The <c>XCabInfo.resources</c> manifest embedded in an XNA <c>.ccgame</c> cabinet.
/// </summary>
internal sealed class XcabInfo
{
    private static readonly string[] KnownKeys =
    [
        "CcgameVersion",
        "Files",
        "GameCopyright",
        "GameDescription",
        "GameGuid",
        "GameThumbnail",
        "GameTitle",
        "Platform",
        "RuntimeProfile",
        "StartupAssembly",
    ];

    /// <summary>Maps a container entry name (for example <c>1</c>) to its logical path.</summary>
    public IReadOnlyDictionary<string, string> EntryToPath { get; private init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public string? StartupAssembly { get; private init; }

    public string? Title { get; private init; }

    public string? Description { get; private init; }

    public string? Copyright { get; private init; }

    public string? Platform { get; private init; }

    public string? RuntimeProfile { get; private init; }

    public string? GameGuid { get; private init; }

    public string? CcgameVersion { get; private init; }

    public static XcabInfo Parse(byte[] blob)
    {
        using var stream = new MemoryStream(blob, writable: false);
        using var reader = new ResourceReader(stream);

        object? filesValue = null;
        if (TryRead(reader, "Files") is { } filesEntry)
        {
            filesValue = ResourceBlob.Deserialize(filesEntry.Data);
            IReadOnlyDictionary<string, string> probe = BuildMap(filesValue);
            Debug($"Files type={filesEntry.TypeName} len={filesEntry.Data.Length} -> {probe.Count} mapping(s)");
            if (Environment.GetEnvironmentVariable("DORADO_DEBUG") is not null)
            {
                foreach (var (key, value) in probe)
                {
                    Console.Error.WriteLine($"    files[{key}] = {value}");
                }
            }
        }
        else
        {
            Debug("Files resource missing");
        }

        return new XcabInfo
        {
            EntryToPath = BuildMap(filesValue),
            StartupAssembly = TryReadString(reader, "StartupAssembly"),
            Title = TryReadString(reader, "GameTitle"),
            Description = TryReadString(reader, "GameDescription"),
            Copyright = TryReadString(reader, "GameCopyright"),
            Platform = TryReadString(reader, "Platform"),
            RuntimeProfile = TryReadString(reader, "RuntimeProfile"),
            GameGuid = TryReadString(reader, "GameGuid"),
            CcgameVersion = TryReadString(reader, "CcgameVersion"),
        };
    }

    private static void Debug(string message)
    {
        if (Environment.GetEnvironmentVariable("DORADO_DEBUG") is not null)
        {
            Console.Error.WriteLine($"[xcab] {message}");
        }
    }

    private static (string TypeName, byte[] Data)? TryRead(ResourceReader reader, string key)
    {
        try
        {
            reader.GetResourceData(key, out string typeName, out byte[] data);
            return (typeName, data);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private static string? TryReadString(ResourceReader reader, string key)
    {
        if (TryRead(reader, key) is not { } entry)
        {
            return null;
        }

        if (ResourceBlob.TryDecodePrimitiveString(entry.Data, out string primitive) && IsPlausible(primitive))
        {
            return primitive;
        }

#pragma warning disable SYSLIB0011 // Legacy resource values may be BinaryFormatter-encoded.
        try
        {
            using var ms = new MemoryStream(entry.Data, writable: false);
            object? value = new BinaryFormatter().Deserialize(ms);
            return value?.ToString();
        }
        catch (Exception ex) when (ex is System.Runtime.Serialization.SerializationException or IOException)
        {
            return null;
        }
#pragma warning restore SYSLIB0011
    }

    private static bool IsPlausible(string value) =>
        !value.Any(char.IsControl);

    /// <summary>
    /// The <c>Files</c> value is a two-row <c>string[,]</c> table: row 0 holds the
    /// container entry names (for example <c>0</c>) and row 1 the logical paths.
    /// Other shapes are handled defensively.
    /// </summary>
    private static IReadOnlyDictionary<string, string> BuildMap(object? value)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        switch (value)
        {
            case string[,] table:
            {
                int rows = table.GetLength(0);
                int cols = table.GetLength(1);
                if (rows == 2)
                {
                    for (int i = 0; i < cols; i++)
                    {
                        map[table[0, i]] = table[1, i];
                    }
                }
                else if (cols == 2)
                {
                    for (int i = 0; i < rows; i++)
                    {
                        map[table[i, 0]] = table[i, 1];
                    }
                }
                else
                {
                    int index = 0;
                    foreach (string item in table)
                    {
                        map[index.ToString(System.Globalization.CultureInfo.InvariantCulture)] = item;
                        index++;
                    }
                }

                break;
            }

            case string[] flat:
                for (int i = 0; i < flat.Length; i++)
                {
                    map[i.ToString(System.Globalization.CultureInfo.InvariantCulture)] = flat[i];
                }

                break;

            case Array array:
            {
                int index = 0;
                foreach (object? item in array)
                {
                    if (item is string str)
                    {
                        map[index.ToString(System.Globalization.CultureInfo.InvariantCulture)] = str;
                    }

                    index++;
                }

                break;
            }
        }

        return map;
    }
}
