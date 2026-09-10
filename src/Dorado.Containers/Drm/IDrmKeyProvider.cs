namespace Dorado.Containers.Drm;

/// <summary>
/// Supplies DRM content keys for encrypted <c>.zcp</c> packages. Dorado ships no
/// keys; implementations read user-supplied material for content the user owns.
/// </summary>
public interface IDrmKeyProvider
{
    /// <summary>
    /// Returns the AES content key for a package, or <see langword="null"/> if this
    /// provider has no key for it. Never throws for a missing key.
    /// </summary>
    /// <param name="packageGuid">
    /// Lower-case hex GUID from the package manifest (for example
    /// <c>5dc1e614ed1b4d108259bc7e3e926a86</c>), or an empty span if unknown.
    /// </param>
    /// <param name="containerHeader">The raw container header bytes, for key derivation.</param>
    byte[]? TryGetContentKey(ReadOnlySpan<byte> packageGuid, ReadOnlySpan<byte> containerHeader);
}
