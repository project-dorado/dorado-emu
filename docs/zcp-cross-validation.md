# `.zcp` parser cross-validation (Dorado.Containers ↔ device modules)

**Updated:** 2026-09-11

The two independent sources of truth for the marketplace NX container are:

- **`dorado-hd/docs/zcp-inventory.md`** — reverse-engineered inventory of the
  official `.zcp` packages (offsets, signature chain, manifest shape, DRM).
- **`dorado-emu/src/Dorado.Containers/Zcp/`** — the parser used by the
  emulator and the IPC bridge (`ZcpReader.cs`, `ZcstfsReader.cs`).

This document records the agreement between them and the executable evidence.
Where the inventory is a document and the reader is code, the reader's
constants are asserted by `dorado-emu/tests/Dorado.Tests/ZcpReaderTests.cs` and
`ZcstfsReaderTests.cs` against synthetic packages built at exactly the
documented offsets and, when present, the external plaintext runtime volume.

## Field agreement

| Field | Inventory / device evidence | Reader | Agrees |
|---|---|---|---|
| NX magic | `4e 58` ("NX") at `0x32` | `NxMagicOffset = 0x32` | ✅ |
| Format version | `1` plaintext (`runtimeZune`), `2` marketplace | `header[0]` drives `IsEncrypted` | ✅ |
| Signature offset | `hdr[4]` = `0x1F0` | signature not parsed (metadata path) | ✅ |
| Manifest offset | `hdr[6]` = `0x2800` | `manifestOffset` from the header | ✅ |
| `EXEC` record | lang `0x0409`, GUID, `<App>.exe`, `Zune.v3.1` | `FindTag(..., "EXEC")` | ✅ |
| `TITL` record | UTF-16LE title, NUL, description | `FindTag(..., "TITL")` | ✅ |
| Volume descriptor | `0x24` bytes at `0xB4` | parsed (`ZcstfsReader`) | ✅ |
| Hash/table offsets | hash region at `0xD8`; blocks `0x4000` | `ZcstfsReader` (`BlockBase`, `HashEntrySize`) | ✅ |
| Encryption | AES-ECB per package; RSA-2048-wrapped key at `0xF0` | `DecryptEcb` behind `IDrmKeyProvider` | ✅ |
| Key seam | — | `IDrmKeyProvider` (`docs/drm-key-import.md`) | ✅ |

## Executable evidence

- `ZcpReaderTests` builds NX containers at the documented offsets and asserts
  title, executable, platform, GUID, description and encryption state.
- `ZcstfsReaderTests` builds synthetic ZCSTFS volumes (plain and AES-256
  encrypted) and asserts file-tree extraction and keyed decryption.
- `ZcstfsReaderTests.ExtractsRuntimeVolumeFixture` parses the external
  plaintext `runtimeZune.v3.1.zcp`, asserts the ten runtime assemblies are
  valid `MZ`/`PE` images, and verifies **every stored block's SHA-1 against its
  hash entry** (259/259).
- The device-side parser (`zcstfs.dll`, reconstructed from `NK.bin`) was
  decompiled with Ghidra; its constants (`0x18` entry stride, `0x4000` blocks,
  `0x2AA` entries per hash level, AES-256 key schedule with 14 rounds) match
  the reader.

## Disagreements / gaps

- The earlier inventory concluded the payload was "AES-ECB encrypted … keys
  unrecoverable". The mechanism is now known precisely (RSA-2048 wrapping +
  device keypack); the keys remain unavailable offline, so the practical
  verdict is unchanged. `zcp-inventory.md` should cite this document.
- Multi-level hash layouts (volumes above 682 blocks) follow the driver's
  level-count construction and are implemented, but could only be validated on
  single-level plaintext volumes (only the runtime volume is unencrypted).
- The parser intentionally does not verify the Authenticode PKCS#7 signature.
