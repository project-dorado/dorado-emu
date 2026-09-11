# ZDK compatibility bridge (native)

Official Zune HD applications execute the app-local `Microsoft.Xna.Zune.dll`,
which P/Invokes a native `ZDK` library (GLES2 through `ZDKGL_*`, image decode
through `ZDKImage_*`, plus system/font/media/cloud services). Dorado builds a
compatible `libZDK.so` so those titles can load and run headlessly.

## Layout

| File | Role |
|---|---|
| `zdk_generated.c` | Stub definitions generated from the external decompiled corpus by `tools/gen_zdk_bridge.py` (never edit by hand). |
| `zdk_image.c` | Real PNG/JPEG/BMP decode via vendored `stb_image.h` (public domain). |
| `zdk_misc.c` | System/font/media/cloud stubs; safe zero-filled outputs. |
| `vendor/stb_image.h` | stb_image v2.30 (public domain / MIT), vendored for the decoder. |

## Build

```bash
native/zdk-bridge/build.sh          # produces libZDK.so
DORADO_ZDK_LIB=$PWD/native/zdk-bridge/libZDK.so \
  dotnet run --project src/Dorado.Cli -- run <app-dir> --frames 60
```

`Dorado.Runtime` resolves `ZDK`/`MEDIA` P/Invokes from `DORADO_ZDK_LIB`, then
from `libZDK.so` beside the host or the extracted title. When the library is
absent, framework-path titles that do not touch ZDK still run; corpus smoke
tests skip.

## Scope

- `ZDKGL_*` currently returns success/no-op values so GL-path titles load.
  Real rendering (`ZDKGL_*` over host GL/GLES) is a follow-up.
- Regenerate stubs with `python3 tools/gen_zdk_bridge.py`; verify with
  `--check`.
