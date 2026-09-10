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

**M0 — Foundation.** Container parsing is implemented; the XNA shim and runtime
are in progress. There is no working Zune emulator anywhere in the world
(GametechWiki: *"THERE ARE CURRENTLY NO EMULATORS FOR THIS DEVICE"*) — Dorado is
deliberately early.

| Milestone | Scope | Status |
|---|---|---|
| M0 | `.ccgame` (CAB/XCab) + `.zcp` (NX) parsers, CLI, tests | ✅ done |
| M1 | Desktop XNA 3.1 shim; run homebrew prebuilt apps; golden frames | ⬜ next |
| M2 | Android host (EGL/GLES3/AAudio) + `dorado-hd` module | ⬜ |
| M3 | Corpus coverage, XNB types, `.ccgame`→`.zcp` writer | ⬜ |
| M4 | User-supplied DRM key bridge | ⬜ |
| M5 | Optional full-system Tegra APX 2600 + WinCE 6.0 core | ⬜ |

### M0 results

- `.ccgame` — MSCF cabinet reader with a managed MSZIP inflater (preset-dictionary
  capable) and full `XCabInfo.resources` decoding, including the `Files` mapping
  table. Validated on single- and multi-folder homebrew packages.
- `.zcp` — NX header + `EXEC`/`TITL` manifest parsing; encrypted payloads are
  detected and gated behind `IDrmKeyProvider`.
- CLI, unit tests, CI, and corpus tooling.

## Container formats

See [`docs/container-formats.md`](docs/container-formats.md) for the byte-level
specification of both container types.

- **`.ccgame`** — MSCF cabinet (MSZIP), numbered payload files plus an
  `XCabInfo.resources` .NET resource set that maps them (`Files` string[,],
  `StartupAssembly`, `GameGuid`, `GameTitle`, `Platform`, `RuntimeProfile`).
  Unencrypted.
- **`.zcp`** — NX container: header, Authenticode PKCS#7 blob, plaintext
  `EXEC`/`TITL` manifest, then an AES-ECB encrypted payload. Marketplace DRM keys
  are not public.

## Building

```bash
dotnet build Dorado.sln -c Release
dotnet test  Dorado.sln
dotnet run --project src/Dorado.Cli -- inspect <package>

# fetch a local test corpus (git-ignored; see corpus/LICENSE-MANIFEST.md)
tools/corpus-fetch.sh
```

Requires the .NET 8 SDK. The Android host (`net8.0-android`) additionally
requires the `android` workload; it is added in M2.

## Licensing

MIT (see [`LICENSE`](LICENSE)). Zune, Zegoe, XNA and the Zune HD are Microsoft
trademarks. Dorado is an independent homage and ships no Microsoft content.
