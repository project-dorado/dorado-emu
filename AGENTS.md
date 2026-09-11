# AGENTS.md — dorado-emu repository rules

Dorado is a **Zune HD application emulator**: it parses `.zcp` / `.ccgame`
containers and runs the contained XNA 3.1 managed apps against a clean-room
compatibility shim. It is consumed by the `dorado-hd` Android client.

## Invariants

1. **No Microsoft content, ever.** Never commit or bundle `.zcp` / `.ccgame`
   payloads, firmware, fonts, DRM keys, or decompiled Microsoft assemblies.
   Corpus fixtures live in the git-ignored `corpus/` directory only.
2. **Clean-room shim.** `Dorado.Xna` implements `Microsoft.Xna.Framework` from
   public API documentation. Do not copy or decompile Microsoft binaries.
3. **The container layer is pure.** `Dorado.Containers` has no platform or UI
   dependency and must stay testable on desktop.
4. **Parse defensively.** Containers are untrusted input: bounds-check every
   read, never trust sizes/offsets, and keep `unpack` path-traversal-safe.
5. **Tests accompany behavior.** Parser and inflate changes require unit tests;
   corpus-dependent tests must no-op gracefully when the fixture is absent so
   CI stays green without copyrighted content.

## Layout

- `src/Dorado.Containers` — `.ccgame` (MSCF/MSZIP) and `.zcp` (NX) parsers,
  XNA `XCabInfo.resources` reader, `IDrmKeyProvider` seam.
- `src/Dorado.Runtime` — CF 3.5 → modern .NET assembly-identity remap and app loading.
- `src/Dorado.Xna` — `Microsoft.Xna.Framework` 3.1 shim (assembly identity `3.1.0.0`).
- `src/Dorado.Platform.*` — graphic/audio/input backends.
- `src/Dorado.Cli` — `dorado inspect|refs|unpack|run` (+ the JSON-RPC IPC bridge used by the desktop).
- `docs/` — container formats, architecture, DRM key import, mini-app parity, ZCP cross-validation.

## Build / test

```bash
dotnet build Dorado.sln -c Release
dotnet test  Dorado.sln
```

Zero-warning policy (`TreatWarningsAsErrors`). `tools/corpus-fetch.sh` fetches
local fixtures; see `corpus/LICENSE-MANIFEST.md` before using any package.
