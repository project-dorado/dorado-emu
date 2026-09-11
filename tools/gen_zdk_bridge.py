#!/usr/bin/env python3
"""Generate the no-op/forwarding ZDK bridge stub layer for Dorado.

The Zune XNA extension assembly (``Microsoft.Xna.Zune.dll``, executed as-is from
an app's payload) P/Invokes a native ``ZDK`` library for GLES2, image/font
decoding, system and cloud services. Dorado provides a compatible native
library; this script regenerates the *stub* portion from the external
decompiled corpus so the export surface cannot silently drift.

The two families implemented by hand (``zdk_image.c`` real decode, and the
``zdk_misc.c`` system/font/media/cloud stubs) are skipped here.

Usage:
    python3 tools/gen_zdk_bridge.py
    python3 tools/gen_zdk_bridge.py --check   # fail if regeneration differs
"""

from __future__ import annotations

import argparse
import re
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
WORKSPACE = REPO.parent
FRAMEWORK = WORKSPACE / "Zune HD Apps (Decompiled)" / "_mine" / "_decompiled" / "_framework"
OUT = REPO / "native" / "zdk-bridge" / "zdk_generated.c"

HAND_WRITTEN_FILES = (
    "zdk_image.c",
    "zdk_font.c",
    "zdk_misc.c",
)
FUNCTION_RE = re.compile(r"^(?:uint32_t|void)\s+([A-Za-z0-9_]+)\s*\(", re.MULTILINE)


def hand_written_names() -> set[str]:
    names: set[str] = set()
    for name in HAND_WRITTEN_FILES:
        path = OUT.parent / name
        if path.exists():
            names.update(FUNCTION_RE.findall(path.read_text(errors="ignore")))
    return names

IMPORT_RE = re.compile(r'\[DllImport\("(?P<lib>ZDK|MEDIA)"(?P<rest>[^\]]*)\)\]')
EXTERN_RE = re.compile(
    r"^\s*(?:public|internal)\s+static\s+extern\s+(?P<ret>[\w\[\]<>\.]+)\s+"
    r"(?P<name>[A-Za-z0-9_]+)\s*\((?P<args>[^;]*)\)\s*;"
)

TYPE_MAP = {
    "void": "void",
    "uint": "uint32_t",
    "int": "int32_t",
    "bool": "int32_t",
    "float": "float",
    "double": "double",
    "IntPtr": "void*",
    "Guid": "uint8_t*",
}

PRIMITIVE_POINTERS = {"uint32_t*", "int32_t*", "float*", "double*", "void**"}


def map_type(cs_type: str, out: bool, ref: bool) -> str:
    cs_type = cs_type.strip()
    if cs_type.endswith("[]"):
        base = cs_type[:-2].strip()
        inner = TYPE_MAP.get(base, "uint8_t")
        if inner == "void*":
            return "void**"
        if base == "float":
            return "float*"
        if base == "uint":
            return "uint32_t*"
        if base == "char":
            return "uint16_t*"
        return "uint8_t*"

    if cs_type in ("string", "StringBuilder"):
        return "const uint16_t*" if cs_type == "string" else "uint16_t*"

    mapped = TYPE_MAP.get(cs_type)
    if mapped is None:
        mapped = "void*"
    if out or ref:
        if mapped == "void*":
            return "void**"
        return mapped + "*"
    return mapped


def parse_args(raw: str) -> list[tuple[str, str, bool]]:
    raw = raw.strip()
    if not raw:
        return []
    # Strip inline marshal attributes before splitting on commas.
    raw = re.sub(r"\[MarshalAs\([^\]]*\)\]", "", raw)
    args: list[tuple[str, str, bool]] = []
    for part in split_top_level(raw):
        part = part.strip()
        if not part:
            continue
        is_out = part.startswith("out ")
        is_ref = part.startswith("ref ")
        part = re.sub(r"^(out|ref)\s+", "", part)
        bits = part.rsplit(" ", 1)
        if len(bits) != 2:
            continue
        ctype, cname = bits
        args.append((ctype.strip(), cname.strip(), is_out or is_ref))
    return args


def split_top_level(raw: str) -> list[str]:
    depth = 0
    current = []
    parts = []
    for ch in raw:
        if ch == "," and depth == 0:
            parts.append("".join(current))
            current = []
            continue
        if ch in "<[(":
            depth += 1
        elif ch in ">])":
            depth -= 1
        current.append(ch)
    parts.append("".join(current))
    return parts


def collect() -> dict[str, dict]:
    exports: dict[str, dict] = {}
    files = sorted(FRAMEWORK.glob("Microsoft.Xna.Zune-*/Microsoft.Xna.Zune.decompiled.cs"))
    if not files:
        raise SystemExit(f"corpus not found under {FRAMEWORK}")
    for path in files:
        lines = path.read_text(errors="ignore").splitlines()
        for i, line in enumerate(lines):
            m = IMPORT_RE.search(line)
            if not m:
                continue
            entry = re.search(r'EntryPoint\s*=\s*"([^"]+)"', m.group("rest"))
            # The extern declaration is on the next non-blank, non-attribute line.
            j = i + 1
            while j < len(lines) and not lines[j].strip():
                j += 1
            decl = EXTERN_RE.match(lines[j]) if j < len(lines) else None
            if not decl:
                continue
            export = entry.group(1) if entry else decl.group("name")
            if export in exports:
                continue
            exports[export] = {
                "ret": decl.group("ret"),
                "args": parse_args(decl.group("args")),
                "lib": m.group("lib"),
            }
    return exports


def emit(exports: dict[str, dict], hand_written: set[str]) -> str:
    lines = [
        "/* Generated by tools/gen_zdk_bridge.py — do not edit by hand.",
        " *",
        " * Stub definitions for the native ZDK surface the Zune XNA extension",
        " * P/Invokes. Hand-written files provide the real image decoder and the",
        " * system/font/media/cloud stubs; everything else is a silent no-op so",
        " * titles load and run headlessly.",
        " */",
        "#include <stdint.h>",
        "#include <stddef.h>",
        "",
    ]
    count = 0
    for export in sorted(exports):
        if export in hand_written:
            continue
        entry = exports[export]
        ret = TYPE_MAP.get(entry["ret"], "void*")
        if entry["ret"].endswith("[]"):
            ret = "uint8_t*"
        params = []
        writes = []
        for ctype, name, is_out in entry["args"]:
            mapped = map_type(ctype, is_out, False)
            params.append(f"{mapped} {name}")
            if is_out and mapped in PRIMITIVE_POINTERS:
                writes.append(f"    if ({name}) *{name} = 0;")
            else:
                writes.append(f"    (void){name};")
        signature = ", ".join(params) if params else "void"
        lines.append(f"{ret} {export}({signature})")
        lines.append("{")
        lines.extend(writes)
        if ret != "void":
            lines.append("    return 0;" if ret != "float" else "    return 0.0f;")
        lines.append("}")
        lines.append("")
        count += 1
    lines.insert(8, f"/* {count} generated exports */")
    return "\n".join(lines)


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--check", action="store_true", help="verify the checked-in file is current")
    args = parser.parse_args()

    exports = collect()
    content = emit(exports, hand_written_names())
    if args.check:
        current = OUT.read_text() if OUT.exists() else ""
        if current != content:
            raise SystemExit(f"{OUT} is stale; run tools/gen_zdk_bridge.py")
        print(f"ok: {OUT} is current ({len(exports)} requested, {len(exports) - len(hand_written_names())} generated)")
        return 0

    OUT.parent.mkdir(parents=True, exist_ok=True)
    OUT.write_text(content)
    print(f"wrote {OUT}: {len(exports)} exports, {len(content.splitlines())} lines")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
