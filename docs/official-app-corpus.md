# Official app corpus (external) and running official titles

**Status (2026-09-11):** the official apps are available as **plaintext
extracted trees**, and the framework-path titles now **run headlessly**.
`tools/smoke_official.py` executes the 54 framework-path titles from the
decompiled corpus: **39 ran in the latest full run** (41 have passed at least
once; a few titles have background-thread races), including calculator, alarm,
calendar, checkers, solitaire, hearts, spades, notes, twitter, zunereader and
wordmonger. The remaining failures are app-specific and listed below.

## Running a title

```bash
native/zdk-bridge/build.sh                       # libZDK.so (image + font decode, ZDKGL stubs)
export DORADO_ZDK_LIB=$PWD/native/zdk-bridge/libZDK.so
export DORADO_FONT_DIR=../zune-hd-disassembly/assets/fonts   # Zegoe et al. (external)

dorado run "/path/to/decompiled/<app>/gametitle/584E07D1" --frames 60
python3 tools/smoke_official.py                  # all framework-path titles
```

`Dorado.Runtime.ZuneLoadContext` resolves the app-local
`Microsoft.Xna.Zune.dll` P/Invokes (`ZDK`/`MEDIA`) to the bridge library; the
shim resolves its own image-decoder import the same way. GL-path titles (7
titles that use `GlSpriteBatch`/ZDKGL) load but render nothing yet.

## Remaining framework-path failures (latest run)

| App | Category | Detail |
|---|---|---|
| `hexic`, `sudoku`, `splatter-bug` | ZuneGamesLib component model | NullReference inside `ComponentBuilder`/font setup. |
| `chess`, `space-battle-2`, `texasholdem`, `shufflebyalbum`, `decoder-ring` | Background loaders | Titles load content on worker threads that fault. |
| `fan-prediction`, `weather` | Dead network services | Web-service call fails on a worker. |
| `drummachine` | Corpus gap | `Sound\Tick` is absent from the published tree. |
| `msnmoney`, `musicquiz`, `supernova`, `wordmonger` | Timing / collections | Collection-modified and timeout races in the app's own loops. |

## Gap list for official titles

## The corpus

The Internet Archive item
[`zune-hd-official-apps-decompiled`](https://archive.org/details/zune-hd-official-apps-decompiled)
publishes the decrypted `\gametitle` mount of all 61 official apps plus an
`hashes.txt` MD5 manifest. Each ZIP contains:

```
gametitle/
  gameinfo.bin, gameinfo.xml, thumbnail.jpg, thumbnail.png
  584E07D1/
    <App>.exe
    Microsoft.Xna.Zune.dll     (Zune XNA extension assembly)
    ZuneAppLib.dll             (app framework: Application, content helpers)
    Content/…                  (.xnb, PNG, XML)
```

We verified 7,785/7,844 files against the published MD5s. This matches the
ZCSTFS layout `\gametitle\584E07D1\Content` documented in
`docs/container-formats.md` — it *is* the decrypted volume contents.

**This corpus is Microsoft content and must stay external and untracked.**
It is used as a local behavioural/reference corpus only; tests that need it
no-op when it is absent. Dorado never bundles or redistributes it.

### Working with it

```bash
# extracted application directory (decrypted dump)
dorado inspect /path/to/extracted/calculator
dorado unpack  /path/to/extracted/calculator out/
dorado run     /path/to/extracted/calculator --frames 3 --hash
```

`ZunePackageReader.ReadDirectory` accepts either the application directory
(`.../gametitle/584E07D1`) or its parent and builds a `Directory` package with
a lazy file reader, so nothing is duplicated on disk.

## What already works

- Directory packages parse, list, and extract (`ContainerKind.Directory`).
- The loader resolves app-local `ZuneAppLib.dll` / `Microsoft.Xna.Zune.dll` and
  maps `Microsoft.Xna.Framework` / `.Game` references by name, so the
  Zune-specific public key tokens (`83fd262b2676676b`, `e92a8b81eba7ceb7`)
  do not need to match the shims.
- `ZuneLoadContext.LoadAppAssembly` clears the PE `32BITREQ` flag, and resolves
  the `ZDK`/`MEDIA` P/Invokes of `Microsoft.Xna.Zune` to the native bridge.
- The shim now implements the surface the 54 framework-path titles need:
  the game/component model and services, math/primitives, graphics resources,
  textures (`SetData`/`GetData`, render targets, scissor, presentation
  settings), `SpriteFont`/`DrawString`, the full XNB type-reader pipeline
  (value types raw, reference types index-addressed, generic
  List/Array/Dictionary, app-local reflective readers), storage and
  `Guide` selection, media/net stubs, and audio.
- Titles built for Windows keep working on a case-sensitive filesystem:
  `ContentManager` resolves assets case-insensitively, tries asset names that
  already include the root, decodes image assets (PNG/JPEG/BMP) through the
  native ZDK bridge, and the extractor links Windows-style path aliases for
  hard-coded `"Content\\Images\\..."` strings.
- `Microsoft.Xna.Framework.InvariantGlobalization=false` for the CLI/tests so
  titles that call `CultureInfo` run.

## Gap list (updated 2026-09-11)

Everything in the original gap table (primitives, component model, graphics,
audio, content readers, storage, gamer services, media, input) is implemented.
What remains is app-specific:

- ZuneGamesLib component localization (`hexic`, `sudoku`, `splatter-bug`)
  hits a null inside `ComponentBuilder`/font setup.
- Worker-thread loaders fault in `chess`, `space-battle-2`, `texasholdem`,
  `shufflebyalbum`, `decoder-ring`, `fan-prediction` and `weather`.
- `drummachine` needs `Sound\Tick`, which the published tree does not contain.
- `msnmoney`, `musicquiz`, `supernova` and `wordmonger` have timing races.
- GL-path titles need the real `ZDKGL_*` → host-GL bridge (stubs only today).

## Legal

Reading Microsoft assemblies as a local compatibility reference is in scope for
the emulator; **copying their code into the clean-room shim is not**. Follow
`AGENTS.md`: the shim is implemented from public API documentation and
synthesized API indexes, and no Microsoft binary is committed.
