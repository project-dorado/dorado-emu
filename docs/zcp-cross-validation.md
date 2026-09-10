# `.zcp` parser cross-validation (Dorado.Containers ↔ Dorado-HD inventory)

**Date:** 2026-09-10

The two independent sources of truth for the marketplace NX container are:

- **`dorado-hd/docs/zcp-inventory.md`** — reverse-engineered inventory of the
  official `.zcp` packages (offsets, signature chain, manifest shape, DRM).
- **`dorado-emu/src/Dorado.Containers/Zcp/ZcpReader.cs`** — the parser used by
  the emulator and the IPC bridge.

This document records the agreement between them and points at the executable
evidence. Where the inventory is a document and the reader is code, the reader's
constants are asserted by `dorado-emu/tests/Dorado.Tests/ZcpReaderTests.cs`
against synthetic packages built at exactly the documented offsets.

## Field agreement

| Field | Inventory (`zcp-inventory.md`) | Reader (`ZcpReader.cs`) | Agrees |
|---|---|---|---|
| NX magic | `4e 58` ("NX") at `0x32` | `NxMagicOffset = 0x32` | ✅ |
| Header window | `0x000`–`0x1F0` | header read up to the record table | ✅ |
| Signature offset | `hdr[4]` = `0x1F0` | signature not parsed (not needed to read metadata) | ✅ (unused) |
| Signature length | `hdr[7]` ≈ `0x1DE0`–`0x1DEF` | not parsed | ✅ (unused) |
| Manifest offset | `hdr[6]` = `0x2800` | `manifestOffset` from the header; default `0x2800` | ✅ |
| Record header size | manifest records start at `+0x1F0` | `RecordHeaderSize = 0x1F0` | ✅ |
| `EXEC` record | lang `0x0409`, 32-char GUID, `<App>.exe`, `Zune.v3.1` | `FindTag(..., "EXEC")`; GUID + exe + platform parsed | ✅ |
| `TITL` record | UTF-16LE title, NUL, description | `FindTag(..., "TITL")`; UTF-16LE split on NUL | ✅ |
| Payload | AES-ECB encrypted; keys unrecoverable | `IsEncrypted = true`, `EntryCount = 0` without a key provider | ✅ |
| Key seam | — | `IDrmKeyProvider` (`docs/drm-key-import.md`) | ✅ |

## Executable evidence

`ZcpReaderTests` builds NX containers at the documented offsets and asserts:

- `ParsesManifestMetadata` — title, executable, startup assembly, platform
  (`Zune.v3.1`), 32-char GameGuid, description, `IsEncrypted = true`,
  `EntryCount = 0`.
- `DetectsNxMagic` — `LooksLikeNx` true for the documented magic, false for
  arbitrary bytes.
- `RejectsNonNxData` — a malformed header throws rather than mis-parsing.

The same synthetic builders (`SyntheticPackages.BuildZcp`) are used by
`EmulatorRpcServerTests.Inspect_SyntheticZcp_ReportsManifestMetadata`, which
proves the **IPC bridge** reports the same metadata over JSON-RPC — so the
parser, the emulator, and the desktop IPC client all agree on the NX shape.

## Disagreements / gaps

None found for the manifest path. The parser intentionally does not verify the
Authenticode PKCS#7 signature or attempt payload decryption; both are outside its
scope (the inventory documents them, and `IDrmKeyProvider` is the seam for
user-supplied keys). No code change is required to reconcile the two sources.
