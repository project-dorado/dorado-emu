# Corpus license manifest

Dorado never commits or redistributes Zune binaries, firmware, DRM keys, or
Microsoft-signed packages. Test content is fetched locally by
`tools/corpus-fetch.sh` into this directory (git-ignored) and each entry is
recorded here with its origin and license. Verify a package's license before
adding it to any automated CI job.

| Package | Origin | License / status | Redistributable? |
|---|---|---|---|
| XNA Pong `[Source].zip` | `archive.org/details/zune-hd-homebrew-gen-4` | Homebrew, source included | Per author — verify |
| Etch-A-Sketch `[Source].zip` | `archive.org/details/zune-hd-homebrew-gen-4` | Homebrew, source included | Per author — verify |
| Flashlight `.ccgame` | `archive.org/details/zune-hd-homebrew-gen-4` | Homebrew | Per author — verify |
| Alarm `.ccgame` | `archive.org/details/zune-hd-homebrew-gen-4` | Homebrew | Per author — verify |
| Android in XNA `[Source].zip` | `archive.org/details/zune-hd-homebrew-gen-4` | Homebrew, source included | Per author — verify |
| Doom `[HD]` `[Deploy Kit].zip` | `archive.org/details/zune-hd-homebrew-gen-4` | GPL (id Software / PrBoom lineage) | **No — local only** |
| `*.zcp` (marketplace) | `archive.org/details/zune-hd-official-apps`, ZuneRedux | Encrypted, Microsoft DRM | **No** |

> **Rule:** if a package is not clearly redistributable, it stays local and is
> never committed, attached to a release, or fetched in CI.
