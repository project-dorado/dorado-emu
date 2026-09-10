# Zune application container formats

Empirically derived from the public homebrew archive and the ZuneRedux
marketplace archive. Little-endian throughout unless stated.

## 1. `.ccgame` — XNA deployment cabinet

A `.ccgame` is a standard Microsoft Cabinet (`MSCF`) using **MSZIP**
compression (`typeCompress = 1`), produced by the XNA Game Studio deployer.

### Cabinet header (`CFHEADER`, 36 bytes)

| Offset | Size | Field |
|---|---|---|
| 0x00 | 4 | `MSCF` signature |
| 0x04 | 4 | reserved1 |
| 0x08 | 4 | cabinet size |
| 0x0C | 4 | reserved2 |
| 0x10 | 4 | `coffFiles` — file-table offset |
| 0x14 | 4 | reserved3 |
| 0x18 | 1 | version minor |
| 0x19 | 1 | version major |
| 0x1A | 2 | folder count |
| 0x1C | 2 | file count |
| 0x1E | 2 | flags |
| 0x20 | 2 | set id |
| 0x22 | 2 | cabinet index |

The folder table (`CFFOLDER`, 8 bytes each) sits **immediately before**
`coffFiles`: `coffFolderTable = coffFiles - 8 * folderCount`. XNA cabinets omit
the optional prev/next strings even when the corresponding flag bits are set,
so anchoring on `coffFiles` is the reliable option.

`CFFOLDER`: `coffCabStart` (u32, first `CFDATA` offset), `cCFData` (u16),
`typeCompress` (u16).

`CFFILE` (at `coffFiles`, 16 bytes + null-terminated name unless the
reserve/header-name flag is set): `cbFile` (u32), `uoffFolderStart` (u32),
`iFolder` (u16), date (u16), time (u16), attributes (u16), name.

`CFDATA` (at each folder's `coffCabStart`): `csum` (u32), `cbData` (u16),
`cbUncomp` (u16), data. Each folder's blocks concatenate into one uncompressed
stream; `uoffFolderStart` is the offset of a file within that stream.

### MSZIP

Each `CFDATA` block starts with `CK` (0x43 0x4B) followed by a raw DEFLATE
segment. Decompress with a **persistent** inflater per folder so the 32 KiB
sliding dictionary carries across blocks.

### Payload

Files are named `"0"`, `"1"`, … and `"XCabInfo.resources"`:

- `"0"` (and, in multi-folder packages, further `MZ` entries) — the managed XNA
  application assembly.
- `"n"` — content, typically `.xnb` (magic `XNB`).
- `"XCabInfo.resources"` — a .NET `ResourceReader` set (magic
  `CE CA EF BE`) with these keys:

  | Key | Type | Meaning |
  |---|---|---|
  | `Files` | `string[,]` | maps container entry index → real path |
  | `StartupAssembly` | `string` | managed assembly to launch |
  | `GameGuid` | `Guid` | package identity |
  | `GameTitle` | `string` | display title |
  | `GameDescription` | `string` | description |
  | `GameCopyright` | `string` | copyright |
  | `GameThumbnail` | `System.Drawing.Bitmap` | icon (not read) |
  | `Platform` | `string` | `Zune` |
  | `RuntimeProfile` | `string` | runtime profile name |

## 2. `.zcp` — marketplace NX container

| Region | Offset | Contents |
|---|---|---|
| NX header | `0x000`–`0x1F0` | u32 fields; magic `4E 58` ("NX") at `0x32`, preceded by a version word (`07 D2`/`07 D3`) |
| Authenticode | `0x1F0` (`hdr[4]`), length `hdr[7]` | PKCS#7 SignedData (`Microsoft Zune Publisher`) |
| Manifest | `0x2800` (`hdr[6]`) | records |
| Payload | after manifest | AES-ECB encrypted; marketplace DRM |

Header `u32[8]` (observed): `2, 1, 2, 0, 0x1F0, 0, 0x2800, 0x1DEF`.
`hdr[4]` = signature offset, `hdr[6]` = manifest offset, `hdr[7]` = signature
length.

### Manifest records

A record is `[u32 outerLen][u16 lang][4-byte tag][u32 payloadLen][payload]`.
Observed tags:

- `EXEC` — payload: 32-char lowercase GUID (ASCII), NUL, executable name, NUL,
  …, platform string (`Zune.v3.1`).
- `TITL` — payload: UTF-16LE title, NUL, UTF-16LE description.

### Payload

The payload after the manifest is a block cipher in ECB mode (identical 16-byte
ciphertext blocks for identical plaintext). Decryption requires the marketplace
DRM key, which is not public. `IDrmKeyProvider` is the seam for a user-supplied
key (see `docs/drm-key-import.md`).
