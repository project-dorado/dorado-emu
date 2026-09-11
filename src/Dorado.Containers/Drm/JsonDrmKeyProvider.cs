using System.Text.Json;

namespace Dorado.Containers.Drm;

/// <summary>
/// Reads AES content keys from a user-supplied JSON sidecar:
/// <code>{ "keys": [ { "guid": "5dc1e614ed1b4d108259bc7e3e926a86", "key": "&lt;hex&gt;" } ] }</code>
/// A key with an empty (or missing) <c>guid</c> acts as a default. Dorado never
/// ships, derives, or transmits keys; users supply keys for content they own.
/// </summary>
public sealed class JsonDrmKeyProvider : IDrmKeyProvider
{
    private readonly Dictionary<string, byte[]> _keys;
    private readonly byte[]? _fallback;

    private JsonDrmKeyProvider(Dictionary<string, byte[]> keys, byte[]? fallback)
    {
        _keys = keys;
        _fallback = fallback;
    }

    /// <summary>Loads keys from a JSON file.</summary>
    public static JsonDrmKeyProvider FromFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return Parse(File.ReadAllText(path));
    }

    /// <summary>Parses a key sidecar document.</summary>
    public static JsonDrmKeyProvider Parse(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        using JsonDocument document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("keys", out JsonElement keys) || keys.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidDataException("Key file must contain a 'keys' array.");
        }

        var parsed = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        byte[]? fallback = null;
        foreach (JsonElement entry in keys.EnumerateArray())
        {
            if (!entry.TryGetProperty("key", out JsonElement keyElement) || keyElement.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            byte[] key = Convert.FromHexString(keyElement.GetString()!);
            if (key.Length is not (16 or 24 or 32))
            {
                throw new InvalidDataException("AES content keys must be 16, 24 or 32 bytes (hex-encoded).");
            }

            string? guid = entry.TryGetProperty("guid", out JsonElement guidElement) && guidElement.ValueKind == JsonValueKind.String
                ? guidElement.GetString()
                : null;
            if (string.IsNullOrWhiteSpace(guid))
            {
                fallback = key;
            }
            else
            {
                parsed[NormalizeGuid(guid)] = key;
            }
        }

        return new JsonDrmKeyProvider(parsed, fallback);
    }

    /// <inheritdoc />
    public byte[]? TryGetContentKey(ReadOnlySpan<byte> packageGuid, ReadOnlySpan<byte> containerHeader)
    {
        if (packageGuid.Length > 0)
        {
            string guid = NormalizeGuid(System.Text.Encoding.ASCII.GetString(packageGuid));
            if (_keys.TryGetValue(guid, out byte[]? key))
            {
                return key;
            }
        }

        return _fallback;
    }

    private static string NormalizeGuid(string value) =>
        value.Trim().Trim('{', '}').Replace("-", string.Empty, StringComparison.Ordinal).ToLowerInvariant();
}
