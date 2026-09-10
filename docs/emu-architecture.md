# Dorado emulator architecture

## Layers

1. **`Dorado.Containers`** — pure parsing. `.ccgame` (MSCF/MSZIP cabinet with an
   `XCabInfo.resources` manifest) and `.zcp` (NX container with an
   `EXEC`/`TITL` manifest and an optionally-encrypted payload). Produces a
   `ZunePackage` with metadata and file entries. No platform dependencies.

2. **`Dorado.Runtime`** — loads the managed application. A custom
   `AssemblyLoadContext` remaps .NET Compact Framework 3.5 assembly identities
   (`mscorlib`, `System`, `System.Core`, version `3.5.0.0`, token
   `7cec85d7bea7798e`) and `Microsoft.Xna.Framework` `3.1.0.0` onto the shim.

3. **`Dorado.Xna`** — a clean-room `Microsoft.Xna.Framework` 3.1 compatibility
   shim. The assembly is named `Microsoft.Xna.Framework` with
   `AssemblyVersion 3.1.0.0` so the app's existing references bind to it. It
   talks to a platform backend through `Dorado.Platform.Abstractions`.

4. **Platform backends**
   - `Dorado.Platform.Desktop` — SDL2/OpenGL, used for development and
     deterministic golden-frame tests.
   - `Dorado.Platform.Android` — EGL/GLES3, AAudio and `<android/sensor.h>` via
     NDK P/Invoke. No Mono.Android binding dependency: Java owns the surface and
     pushes input over JNI; managed code drives the graphics/audio NDK APIs.

## Android embedding (M2)

Primary: embed Mono (`libmonosgen-2.0.so`) plus `libdorado.so` in an AAR. A
Kotlin `SurfaceView` hands its `ANativeWindow` to `nativeStart(...)`; the managed
side creates an EGL context and runs the app. Input is forwarded over JNI.
Fallback (if embedding proves fragile): a companion `.NET for Android` host app
launched by intent / rendered into a virtual display.

## Assembly identity remap

Zune apps reference:
- `mscorlib, Version=3.5.0.0, PublicKeyToken=7cec85d7bea7798e`
- `System, Version=3.5.0.0, ...`
- `System.Core, Version=3.5.0.0, ...`
- `Microsoft.Xna.Framework, Version=3.1.0.0`

`Dorado.Runtime` resolves these to the .NET 8 facades and the shim. Missing CF
APIs are filled with small shims. The exact assembly list is verified from the
`RuntimeProfile` entry in `XCabInfo.resources`.
