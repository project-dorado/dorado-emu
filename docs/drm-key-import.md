# DRM key import (user-supplied content keys)

Marketplace `.zcp` volumes are AES-ECB encrypted with a per-package content
key. Dorado does **not** ship, derive, or break keys; it provides the
`IDrmKeyProvider` seam plus a JSON sidecar loader so a user may supply the
unwrapped key for content they lawfully own. See
[`zcp-decryption.md`](zcp-decryption.md) for the full mechanism.

## Scope

- Dorado is an offline emulator. It never contacts Microsoft services.
- Homebrew `.ccgame` and plaintext `.zcp` volumes need no key.
- Encrypted marketplace packages remain unreadable until a key is supplied.
- Key material is read from a user-chosen file at runtime and is never logged,
  bundled, committed, or transmitted.

## Provider contract

```csharp
public interface IDrmKeyProvider
{
    // Return the 16/24/32-byte AES key for a package, or null if unavailable.
    byte[]? TryGetContentKey(ReadOnlySpan<byte> packageGuid, ReadOnlySpan<byte> containerHeader);
}
```

`packageGuid` is the 32-character lower-case hex GUID from the `EXEC` record.
`containerHeader` exposes the first `0x1F0` bytes for implementations that need
to derive or select a key.

## Key file format

```json
{
  "keys": [
    { "guid": "b0219e77eee74d9baf5b7934c594fc65", "key": "<hex AES key>" },
    { "key": "<hex AES key used when the GUID is absent or unknown>" }
  ]
}
```

Use it from the CLI:

```bash
dorado unpack "Alarm Clock.zcp" out/ --key-file keys.json
dorado inspect "Alarm Clock.zcp" --key-file keys.json
```

`JsonDrmKeyProvider` accepts 16-, 24-, and 32-byte hex keys and matches GUIDs
case-insensitively, with optional braces and dashes.

## Verification

Each ZCSTFS hash entry is the SHA-1 of its data block, and the volume
descriptor stores the SHA-1 of the top hash block, so a wrong key fails
immediately with an `InvalidDataException` rather than producing corrupt files.

## Extraction from a device (out of scope)

Extraction from a physical, user-owned Zune HD is not implemented here.
Community tooling (`CUB3D/zuneslayer`, Project Lyra) can read the provisioned
keypack or the decrypted `\gametitle` mount on firmware v4.5. Do not
redistribute extracted keys or Microsoft content.
