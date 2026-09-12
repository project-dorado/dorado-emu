# Dorado

**A Zune HD application emulator.** Dorado loads and runs Zune HD `.zcp` / `.ccgame`
packages — XNA Game Studio 3.1 managed apps — and is designed to be embedded in
the `dorado-hd` Android client.

> Part of [project-dorado](https://github.com/project-dorado). Dorado is an
> independent preservation/emulation project. It bundles no Microsoft binaries,
> firmware, fonts, or DRM keys.

## What it is

A Zune HD `.zcp` is an NX container holding an XNA 3.1 / .NET Compact Framework
3.5 managed application. Dorado parses the container, then executes the app's IL
against a clean-room `Microsoft.Xna.Framework` 3.1 compatibility shim, mapping
XNA graphics/audio/input onto the host platform.

```
 .zcp (NX, AES-ECB DRM)  ─┐
 .ccgame (MSCF cabinet)  ─┼─▶ Dorado.Containers ─▶ managed .exe + .xnb content
                          │                                   │
                          │                                   ▼
                          │                     Dorado.Runtime (CF 3.5 → .NET identity remap)
                          │                                   │
                          │                                   ▼
                          └─────────────────▶ Microsoft.Xna.Framework 3.1 shim (Dorado.Xna)
                                                                 │
                                     ┌───────────────────────────┴───────────────────────────┐
                                     ▼                                                       ▼
                        Dorado.Platform.Desktop (SDL/OpenGL)          Dorado.Platform.Android (EGL/GLES3/AAudio)
```

## Status

**M0 + M1 complete, plus ZCSTFS volume parsing and the official-app runtime.**
Container parsing (`.ccgame`/`.zcp`), the ZCSTFS mini filesystem, a working
desktop XNA 3.1 runtime that renders golden frames, and the clean-room surface
the official titles bind against are implemented and covered by **79 tests**.
The decompiled official-app corpus runs headlessly: **50 of 54 framework-path
titles** in the latest full smoke run (calculator, alarm, calendar, checkers,
solitaire, hearts, spades, notes, twitter, Zune Reader, WordMonger, Hexic and
more); the four remaining are two upstream corpus gaps and two dead-network
clients (see `docs/official-app-corpus.md`). A native `ZDK` compatibility bridge provides
image decode (stb_image), TrueType font rasterization (stb_truetype) and a
stubbed GLES2 surface for the app-local `Microsoft.Xna.Zune.dll`.

| Milestone | Scope | Status |
|---|---|---|
| M0 | `.ccgame` (CAB/XCab) + `.zcp` (NX) parsers, CLI, tests | ✅ done |
| M1 | Desktop XNA 3.1 shim; run homebrew prebuilt apps; golden frames | ✅ done |
| M1.5 | ZCSTFS volume reader (runtime volume verified block-by-block), AES key seam | ✅ done |
| M3 | Official-app runtime: XNA surface, XNB pipeline, ZDK bridge, corpus smoke | 🟡 50/54 titles |
| M2 | Android host (EGL/GLES3/AAudio) + `dorado-hd` module | ⬜ |
| M4 | User-supplied DRM key bridge (key file shipped; device keypack needed) | 🟡 seam |
| M5 | Optional full-system Tegra APX 2600 + WinCE 6.0 core | ⬜ |

### M1 results

- **Runtime.** `Dorado.Runtime` extracts a package, then loads its managed
  assembly in a custom `AssemblyLoadContext` that maps the Compact Framework and
  XNA 3.1 assembly identities onto the running framework and the shims. The real
  `ZunePong.exe` / `ZuneHDDemo.exe` entry points execute.
- **Shims.** `Microsoft.Xna.Framework` (core) and `Microsoft.Xna.Framework.Game`
  are clean-room, identity-matched assemblies (`3.1.0.0`) covering the exact
  member surface the two M1 titles use: Game loop, SpriteBatch, GraphicsDevice,
  ContentManager + XNB, Color/Vector/Rectangle/BoundingBox, Viewport,
  RenderTarget2D, and Keyboard/GamePad/Touch/Accelerometer.
- **Backend.** A deterministic headless software rasterizer
  (`Dorado.Platform.Desktop`) with premultiplied blitting, rotation/scale/flip,
  render targets, and a self-contained PNG writer.
- **Verified.** `XNA Pong` and `Etch-A-Sketch` render deterministic 480x272
  frames; golden hashes are asserted in `M1RuntimeTests`, and a scripted touch
  stream draws strokes (proving input plumbing).

### M0 results

- `.ccgame` — MSCF cabinet reader with a managed MSZIP inflater (preset-dictionary
  capable) and full `XCabInfo.resources` decoding, including the `Files` mapping
  table. Validated on single- and multi-folder homebrew packages.
- `.zcp` — NX header + `EXEC`/`RTVR`/`TITL` manifest parsing; encrypted payloads
  are detected and gated behind `IDrmKeyProvider`.

### M1.5 results

- **ZCSTFS volume reader.** `Dorado.Containers` parses the STFS-derived mini
  filesystem inside `.zcp`: volume descriptor, `0x18`-byte SHA-1/chain hash
  entries, `0x4000`-byte blocks, and `0x40`-byte directory records. The
  plaintext runtime volume extracts all ten XNA runtime assemblies, and every
  stored block's SHA-1 is verified against its hash entry.
- **Decryption seam.** Marketplace volumes are AES-ECB; `--key-file` supplies
  unwrapped AES keys for owned content (see `docs/zcp-decryption.md` for the
  RSA-2048/keypack mechanism the device uses).
- CLI, unit tests, CI, and corpus tooling.

## Container formats

See [`docs/container-formats.md`](docs/container-formats.md) for the byte-level
specification of both container types.

- **`.ccgame`** — MSCF cabinet (MSZIP), numbered payload files plus an
  `XCabInfo.resources` .NET resource set that maps them (`Files` string[,],
  `StartupAssembly`, `GameGuid`, `GameTitle`, `Platform`, `RuntimeProfile`).
  Unencrypted.
- **`.zcp`** — NX container: header, Authenticode PKCS#7 blob, plaintext
  `EXEC`/`TITL` manifest, then a **ZCSTFS volume** (an STFS-derived mini
  filesystem). Format version 1 (the XNA runtime volume) is plaintext; version
  2 (marketplace) AES-ECB encrypts the volume with a per-package key that
  arrives RSA-2048-wrapped. See `docs/container-formats.md` and
  `docs/zcp-decryption.md`.

## Building

```bash
dotnet build Dorado.sln -c Release
dotnet test  Dorado.sln

# fetch a local test corpus (git-ignored; see corpus/LICENSE-MANIFEST.md)
tools/corpus-fetch.sh
```

## Quick start

```bash
CLI="dotnet run --project src/Dorado.Cli --"

$CLI inspect "corpus/XNA Pong.ccgame"          # container metadata + files
$CLI refs    path/to/App.exe                    # assembly/member references
$CLI unpack  "corpus/XNA Pong.ccgame" out/      # extract an unencrypted package
$CLI run     "corpus/XNA Pong.ccgame" --frames 20 --hash
$CLI run     "corpus/Etch-A-Sketch.ccgame" --frames 20 --out frame.png

$CLI inspect path/to/runtimeZune.v3.1.zcp       # ZCSTFS volume, no key needed
$CLI unpack  path/to/runtimeZune.v3.1.zcp out/  # extract the ten runtime assemblies
$CLI inspect path/to/extracted/GametitleApp     # an extracted application directory
$CLI unpack  "Alarm Clock.zcp" out/ --key-file keys.json   # owned key (docs/zcp-decryption.md)
```

Running official decompiled titles needs the native ZDK bridge and (optionally)
the external Zegoe font corpus:

```bash
native/zdk-bridge/build.sh
export DORADO_ZDK_LIB=$PWD/native/zdk-bridge/libZDK.so
export DORADO_FONT_DIR=../zune-hd-disassembly/assets/fonts
$CLI run "<decompiled-app>/gametitle/584E07D1" --frames 60
python3 tools/smoke_official.py      # 54 framework-path titles
```

Requires the .NET 8 SDK. The Android host (`net8.0-android`) additionally
requires the `android` workload; it is added in M2.

## Licensing

MIT (see [`LICENSE`](LICENSE)). Zune, Zegoe, XNA and the Zune HD are Microsoft
trademarks. Dorado is an independent homage and ships no Microsoft content.
