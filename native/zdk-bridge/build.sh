#!/usr/bin/env bash
# Builds the native ZDK compatibility library used by official Zune HD titles.
# The library is loaded at runtime by Dorado.Runtime (DORADO_ZDK_LIB or next to
# the running host); it is never committed.
set -euo pipefail
cd "$(dirname "$0")"

CC="${CC:-cc}"
"$CC" -O2 -fPIC -shared -std=c11 -Wall -Wextra -Wno-unused-parameter \
    -o libZDK.so zdk_generated.c zdk_image.c zdk_misc.c -lm

echo "built $(pwd)/libZDK.so"
