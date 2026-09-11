# Zune application container formats

Empirically derived from the public homebrew archive, the ZuneRedux
marketplace archive, the plaintext `runtimeZune.v3.1.zcp` volume, and the
reconstructed `zcstfs.dll` / `keyvault.dll` device modules. Little-endian
throughout unless stated.

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

A `.zcp` is an NX container: an outer manifest/signature region followed by a
**ZCSTFS volume** — the small filesystem mounted by the device's `zcstfs.dll`
at `\gametitle\584E07D1\Content`. This is an STFS derivative, not an opaque
blob; the plaintext runtime volume
(`runtimeZune.v3.1.zcp`, format version 1) can be parsed and extracted
today, while marketplace packages (version 2) encrypt the volume's data
region (see `docs/zcp-decryption.md`).

### NX header (`0x000`–`0x1F0`)

| Offset | Size | Meaning |
|---|---|---|
| 0x00 | 4 | format version: `1` = plaintext, `2` = encrypted |
| 0x04 | 4 | `1` (constant) |
| 0x08 | 4 | `2` (constant) |
| 0x10 | 4 | signature offset (`0x1F0`) |
| 0x18 | 4 | manifest offset (`0x2800`) |
| 0x1C | 4 | signature length (`0x1DE0`–`0x1DEF`) |
| 0x30 | 2 | format word `0x07D2`/`0x07D3`; ASCII `NX` at `0x32` |
| 0x44 | 4 | `1` plaintext / `2` encrypted |
| 0x48 | 4 | `0` / `1` (wrapped content key present) |
| 0x4C | 4 | `0` / `0x100` (AES key size in bits) |
| 0x74 | 20 | SHA-1 fingerprint |
| 0x88 | 4 | manifest records offset (`0x29F0`) |
| 0x94 | 20 | second SHA-1 (zero on the plaintext runtime volume) |
| 0xA8 | 4 | thumbnail offset (`0xAA30`) |
| 0xB4 | 0x24 | volume descriptor (below) |
| 0xD8 | 4 | ZCSTFS hash-region offset |
| 0xE0 | 4 | `0x80000000` (flag) |

The region `0xF0`–`0x1F0` holds a 256-byte wrapped content key when field
`0x48` is set; marketplace packages fill it, the runtime volume leaves it
zeroed.

### Volume descriptor (`0xB4`, 0x24 bytes)

| Offset | Size | Meaning |
|---|---|---|
| +0x00 | 1 | descriptor length (`0x24`) |
| +0x01 | 1 | version |
| +0x02 | 1 | flags |
| +0x03 | 2 | file-table block count |
| +0x05 | 3 | file-table block number |
| +0x08 | 20 | SHA-1 of the top hash table |
| +0x1C | 4 | total block count |
| +0x20 | 4 | free block count |

### ZCSTFS volume

All blocks are `0x4000` bytes.

| Region | Location |
|---|---|
| Hash region | `hashOffset` (header `0xD8`): `total` entries of `0x18` bytes |
| Data blocks | `blockBase = hashOffset + hashBlocks * 0x4000`; block `N` at `blockBase + N * 0x4000` |
| Directory | block `file-table block number` (usually 0) |

- **Hash entries** are `sha1[20] + info:u32`. `info & 0xFFFFFF` is the next
  block in the chain (`0xFFFFFF` terminates), and the top two bits carry the
  allocation state. Entry `i` is the SHA-1 of data block `i` — verified for
  every stored block of the runtime volume.
- **hashBlocks** accumulates the levels needed to index the volume:
  `level0 = ceil(total / 682)`, then `ceil(level0 / 682)`, … with a top-level
  block always present. The runtime volume (260 blocks) uses two hash blocks.
- Only `total - free` blocks are stored, so the file ends after the last
  allocated block.
- **Directory entries** are STFS-style `0x40`-byte records: `name[40]`,
  `flags` (name length in bits 0–5, contiguous bit 6, directory bit 7),
  `valid_data_blocks:u24`, `allocated_data_blocks:u24`, `start_block:u24`,
  `parent_index:u16` (`0xFFFF` = root), `length:u32`, and four FAT-style
  date/time fields.
- File chains are walked through the hash entries, and each block contributes
  `min(0x4000, remaining)` bytes.

### Runtime volume contents (verified)

`runtimeZune.v3.1.zcp` is `0x424000` bytes, 260 blocks (1 free),
`hashOffset = 0x10000`, `blockBase = 0x18000`, and holds ten files:

`Microsoft.Xna.Framework.dll`, `Microsoft.Xna.Framework.Game.dll`,
`mscoree3_5.dll`, `mscorlib.dll`, `runtimehost.dll`, `system.core.dll`,
`system.dll`, `system.sr.dll`, `system.xml.dll`, `system.xml.linq.dll`.

### Manifest records

A record is `[u32 outerLen][u16 lang][4-byte tag][u32 payloadLen][payload]`.
Observed tags:

- `EXEC` — payload: 32-char lowercase GUID (ASCII), NUL, executable name, NUL,
  …, platform string (`Zune.v3.1`).
- `RTVR` — runtime volume record (fields + `Zune.v3.1`).
- `TITL` — payload: UTF-16LE title, NUL, UTF-16LE description.

### Encryption

Marketplace packages AES-ECB encrypt the region from `hashOffset` through the
end of the last stored block with a per-package AES key. The key is not stored
in the clear: it arrives as the 256-byte RSA ciphertext at `0xF0` and is
unwrapped by the device (see `docs/zcp-decryption.md`). `IDrmKeyProvider`
is the seam for user-supplied (already unwrapped) AES keys.
