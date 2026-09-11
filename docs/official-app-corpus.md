# Official app corpus (external) and running official titles

**Status:** the official apps are now available as **plaintext extracted trees**,
and Dorado can inspect/run extracted application directories. Running the
official titles end-to-end still needs a larger XNA surface — the gap list is
below.

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
- `ZuneLoadContext.LoadAppAssembly` clears the PE `32BITREQ` flag: Zune Compact
  Framework assemblies are IL-only but carry it, and modern .NET refuses to
  load them unchanged.
- The runtime volume supplies the XNA 3.1 framework assemblies; the emulator's
  shim (`Dorado.Xna`) covers the homebrew surface and passes M1 golden frames.

## Gap list for official titles

The first failure after the loader fixes is a missing shim type; the required
surface (from `ZuneAppLib.dll`, `Microsoft.Xna.Zune.dll`, and the app
assemblies) is:

| Area | Missing members |
|---|---|
| Primitives | `Matrix`, `Quaternion`, `Plane`/`PlaneIntersectionType`, `Vector4`, `BoundingSphere`, `BoundingFrustum`, `MathHelper`, `Point` (added) |
| Game framework | `GameComponent`, `DrawableGameComponent`, `GameWindow`, `GameServiceContainer`, `IGraphicsDeviceManager`, `Game.Services` |
| Graphics | `SpriteFont`, `SpriteBlendMode`, `SpriteSortMode`, `SaveStateMode`, `SetDataOptions`, `GraphicsResource`, `OutOfVideoMemoryException` |
| Audio | `SoundEffect`, `SoundEffectInstance`, `SoundState`, `InstancePlayLimitException` |
| Content | `ContentReader`, `ContentTypeReader<T>`; `Microsoft.Xna.Zune`'s own readers (`GlTextureReader`, `GlFontReader`, `GlModelReader`, `GlSpriteFontReader`) run against these and decode the Zune `.xnb` formats |
| Storage | `StorageDevice`, `StorageContainer` (save games) |
| GamerServices | `Guide.BeginShowStorageDeviceSelector` / `EndShowStorageDeviceSelector` |
| Media | `Media.Song`, `Media.MediaPlayer` (background playback) |
| Input | `Accelerometer`, `TouchPanel` extensions used by `ZuneAppLib` |

Recommended order: primitives/math → game framework (`GameComponent` tree,
`Game.Services`) → content reader plumbing (this is what loads real content) →
audio/storage/gamer services stubs → media.

## Legal

Reading Microsoft assemblies as a local compatibility reference is in scope for
the emulator; **copying their code into the clean-room shim is not**. Follow
`AGENTS.md`: the shim is implemented from public API documentation and
synthesized API indexes, and no Microsoft binary is committed.
