# DRM key import (M4 — user-supplied key path)

Marketplace `.zcp` payloads are AES-ECB encrypted with a content key that is not
public. Dorado does **not** ship, derive, or break keys. It provides a seam —
`IDrmKeyProvider` — so a user may supply keys for content they lawfully own,
typically extracted from their own device. The **seam ships in M0**
(`src/Dorado.Containers/Drm/IDrmKeyProvider.cs`, with a `NullDrmKeyProvider`
default wired into `ZcpReader`); M4 is the *key-file importer* on top of it.

## Threat-model / scope

- Dorado is an offline emulator. It never contacts Microsoft services.
- Homebrew `.ccgame` and any unencrypted `.zcp` need no key.
- Encrypted marketplace packages remain unreadable until a key is supplied.
- Key material is read from a user-chosen file at runtime and is never logged,
  bundled, committed, or transmitted.

## Provider contract (implemented seam)

```csharp
public interface IDrmKeyProvider
{
    // Return the 16/24/32-byte AES key for a package, or null if unavailable.
    byte[]? TryGetContentKey(ReadOnlySpan<byte> packageGuid, ReadOnlySpan<byte> containerHeader);
}
```

## Key file format (planned)

A simple JSON or raw-bytes sidecar referenced from the `dorado-hd` UI:

```json
{ "keys": [ { "guid": "5dc1e614ed1b4d108259bc7e3e926a86", "key": "<hex>" } ] }
```

Extraction from a physical, user-owned Zune HD is out of scope for this
repository; community tooling (e.g. Project Lyra / zuneslayer) exists and is
documented separately. Do not redistribute extracted keys.
