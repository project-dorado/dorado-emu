namespace Dorado.Containers.Drm;

/// <summary>A provider that never returns a key, used by default.</summary>
public sealed class NullDrmKeyProvider : IDrmKeyProvider
{
    /// <summary>Shared instance.</summary>
    public static readonly NullDrmKeyProvider Instance = new();

    /// <inheritdoc />
    public byte[]? TryGetContentKey(ReadOnlySpan<byte> packageGuid, ReadOnlySpan<byte> containerHeader) => null;
}
