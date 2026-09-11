# Mini-app parity inventory (Dorado-HD ↔ official Zune HD apps ↔ emulator)

**Date:** 2026-09-10

Cross-reference of the 29 mini-apps implemented natively in
`dorado-hd/app/src/main/java/com/heretek/dorado_hd/ui/apps/` against:

- the official Zune HD application packages in `/Zune HD Apps/` (`.zcp`), and
- the runnable homebrew corpus in `dorado-emu/corpus/` (`.ccgame`).

**Status legend:** ✅ native Compose parity · 🟡 native, simplified vs device ·
🧪 mock shell (device app was a network/social client with no local behavior) ·
🎮 executable in `dorado-emu` (XNA runtime).

## Utilities

| Dorado-HD id | Label | Official `.zcp` | Emulator | Status |
|---|---|---|---|---|
| `calculator` | calculator | `Calculator.zcp` | — | ✅ |
| `notes` | notes | `Notes.zcp` | — | ✅ |
| `stopwatch` | stopwatch | `Stopwatch.zcp` | — | ✅ |
| `alarm` | alarm clock | `Alarm Clock.zcp` | `Alarm.ccgame` | ✅ 🎮 |
| `calendar` | calendar | `Calendar.zcp` | — | ✅ |
| `level` | level | `Level.zcp` | `Flashlight.ccgame` (sensor sibling) | ✅ 🎮 |

## Music

| Dorado-HD id | Label | Official `.zcp` | Emulator | Status |
|---|---|---|---|---|
| `metronome` | metronome | `Metronome.zcp` | — | ✅ |
| `piano` | piano | `Piano.zcp` | — | ✅ |
| `drummachine` | drum machine | `Drum Machine.zcp` | — | ✅ |
| `chordfinder` | chord finder | `Chord Finder.zcp` | — | ✅ |
| `musicquiz` | music quiz | `Music Quiz.zcp` | — | ✅ |
| `shufflebyalbum` | shuffle by album | `Shuffle By Album.zcp` | — | ✅ |

## Games

| Dorado-HD id | Label | Official `.zcp` | Emulator | Status |
|---|---|---|---|---|
| `solitaire` | solitaire | `Solitaire.zcp` | — | ✅ |
| `sudoku` | sudoku | `Sudoku.zcp` | — | ✅ |
| `hexic` | hexic | `Hexic.zcp` | — | ✅ |
| `reversi` | reversi | `Reversi.zcp` | — | ✅ |
| `hearts` | hearts | `Hearts.zcp` | — | ✅ |
| `spades` | spades | `Spades.zcp` | — | ✅ |
| `checkers` | checkers | `Checkers.zcp` | — | ✅ |
| `chess` | chess | `Chess.zcp` | — | ✅ |
| `texasholdem` | texas hold 'em | `Texas Hold Em.zcp` | — | ✅ |

> Homebrew game goldens live in `dorado-emu/corpus/`: `XNA Pong.ccgame` and
> `Etch-A-Sketch.ccgame` render deterministic 480×272 frames and are the
> renderer-regression fixtures (`M1RuntimeTests`, `EmulatorRpcServerTests`).

## Network / social (mock shells)

These device apps were thin clients to live Microsoft services (all shut down).
Dorado-HD ships local mock surfaces that preserve the navigation and layout; no
live backend is reproduced.

| Dorado-HD id | Label | Official `.zcp` | Status |
|---|---|---|---|
| `weather` | weather | — (zune.net service) | 🧪 |
| `twitter` | twitter | `Twitter.zcp` | 🧪 |
| `facebook` | facebook | `Facebook.zcp` | 🧪 |
| `email` | email | `Email.zcp` | 🧪 |
| `messenger` | messenger | — (Live Messenger) | 🧪 |
| `msnmoney` | msn money | `MSN Money.zcp` | 🧪 |
| `zunereader` | zune reader | — | 🧪 |
| `zunesocial` | social | — | 🧪 |

## Coverage summary

- **29 / 29** mini-apps have a native or mock surface (100% of the app registry).
- **21 / 29** are faithful native interactions with a direct official `.zcp`
  counterpart (utilities, music, games).
- **8 / 29** are network/social shells whose original backends are dead.
- **2** homebrew XNA titles run headlessly in the emulator and serve as the
  renderer's golden-frame regression fixtures.

## Remaining parity work

- Official `.zcp` payloads are AES-ECB encrypted with a per-package key that the
  device unwraps (RSA-2048 + provisioned keypack; see `docs/zcp-decryption.md`).
  Dorado parses the plaintext runtime volume and accepts user-supplied keys via
  `IDrmKeyProvider`; marketplace logic stays out of reach without an owned key.
- The 29 native apps are not yet executed through `dorado-emu`; the emulator is
  the runtime for **user-supplied** `.ccgame`/`.zcp` packages, not a replacement
  for the built-in Compose apps.
