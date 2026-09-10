#!/usr/bin/env bash
# Fetch a small local test corpus into corpus/ (git-ignored).
#
# Nothing here is committed or redistributed. See corpus/LICENSE-MANIFEST.md.
# Only fetch packages whose license you have verified for your use.
set -euo pipefail

cd "$(dirname "$0")/.."
mkdir -p corpus

BASE="https://archive.org/download/zune-hd-homebrew-gen-4"
ITEMS=(
  "Games/XNA Pong/XNA Pong [Source].zip"
  "Games/XNA Pong/XNA Pong.ccgame"
  "Applications/Etch-A-Sketch/Etch-A-Sketch [Source].zip"
  "Applications/Flashlight/Flashlight.ccgame"
  "Applications/Alarm.ccgame"
  "Applications/Android in XNA/Android in XNA v1.0.ccgame"
)

urlencode() { python3 -c 'import urllib.parse,sys; print(urllib.parse.quote(sys.argv[1]))' "$1"; }

for item in "${ITEMS[@]}"; do
  out="corpus/$(basename "$item")"
  if [[ -f "$out" ]]; then
    echo "skip  $out"
    continue
  fi
  echo "fetch $item"
  curl -fL --retry 3 -A "dorado-emu-corpus/0.1" "$BASE/$(urlencode "$item")" -o "$out"
done

# Optional: marketplace .zcp samples for manifest-only parsing. Encrypted
# payloads are never used. Point ZCP_DIR at a local copy if you have one.
if [[ -n "${ZCP_DIR:-}" && -d "$ZCP_DIR" ]]; then
  echo "copying .zcp samples from $ZCP_DIR (manifest inspection only)"
  find "$ZCP_DIR" -maxdepth 1 -name '*.zcp' -exec cp -n {} corpus/ \;
fi

echo "done -> corpus/"
