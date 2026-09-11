#!/usr/bin/env python3
"""Run the official Zune HD framework-path corpus through the emulator CLI.

Reads the render-stack classification produced by
``dorado-hd/tools/app_render_stack.py`` and executes each framework-path title
headlessly for a few frames, reporting pass/fail per app. The corpus and the
native ZDK bridge live outside the repository and are never uploaded.

Usage:
    python3 tools/smoke_official.py                  # 3 frames per title
    python3 tools/smoke_official.py --frames 60
    python3 tools/smoke_official.py --filter hexic
    python3 tools/smoke_official.py --json out.json
"""

from __future__ import annotations

import argparse
import glob
import json
import os
import subprocess
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
WORKSPACE = REPO.parent
DEFAULT_CORPUS = WORKSPACE / "Zune HD Apps (Decompiled)"
DEFAULT_ZDK = REPO / "native" / "zdk-bridge" / "libZDK.so"
DEFAULT_FONTS = WORKSPACE / "zune-hd-disassembly" / "assets" / "fonts"
DEFAULT_CLI = REPO / "src" / "Dorado.Cli"


def load_framework_apps(mine: Path) -> list[dict]:
    stack_path = mine / "_render-stack.json"
    if not stack_path.exists():
        raise SystemExit(f"render-stack index not found: {stack_path} (run app_render_stack.py)")
    stack = json.loads(stack_path.read_text())
    return [app for app in stack.get("apps", []) if app.get("stack") == "framework"]


def corpus_dir_for(mine: Path, slug: str) -> str:
    index_path = mine / "_index" / f"{slug}.json"
    if index_path.exists():
        try:
            return json.loads(index_path.read_text()).get("corpusDir") or slug
        except json.JSONDecodeError:
            pass
    return slug


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--corpus", type=Path, default=DEFAULT_CORPUS)
    parser.add_argument("--zdk", type=Path, default=DEFAULT_ZDK)
    parser.add_argument("--fonts", type=Path, default=DEFAULT_FONTS)
    parser.add_argument("--cli", type=Path, default=DEFAULT_CLI)
    parser.add_argument("--frames", type=int, default=3)
    parser.add_argument("--timeout", type=int, default=90)
    parser.add_argument("--filter", default="")
    parser.add_argument("--json", type=Path)
    args = parser.parse_args()

    mine = args.corpus / "_mine"
    apps = load_framework_apps(mine)
    if args.filter:
        apps = [a for a in apps if args.filter in a["slug"]]
    if not apps:
        raise SystemExit("no framework-path apps selected")

    env = dict(os.environ)
    if args.zdk.exists():
        env["DORADO_ZDK_LIB"] = str(args.zdk)
    if args.fonts.is_dir():
        env["DORADO_FONT_DIR"] = str(args.fonts)

    cli = ["dotnet", "run", "--project", str(args.cli), "-c", "Release", "--no-build", "--", "run"]
    results: list[dict] = []
    counts = {"ok": 0, "missing": 0, "fail": 0}
    for app in apps:
        slug = app["slug"]
        directories = [d for d in glob.glob(str(args.corpus / corpus_dir_for(mine, slug) / "gametitle" / "*")) if os.path.isdir(d)]
        if not directories:
            counts["missing"] += 1
            results.append({"slug": slug, "status": "missing"})
            continue

        try:
            proc = subprocess.run(
                cli + [directories[0], "--frames", str(args.frames)],
                env=env,
                capture_output=True,
                text=True,
                timeout=args.timeout,
            )
            stdout = (proc.stdout or "").strip().splitlines()
            stderr = (proc.stderr or "").strip().splitlines()
            if proc.returncode == 0 and stdout and stdout[-1].startswith("ran "):
                counts["ok"] += 1
                results.append({"slug": slug, "status": "ok", "detail": stdout[-1]})
            else:
                counts["fail"] += 1
                detail = stderr[-1] if stderr else (stdout[-1] if stdout else f"rc={proc.returncode}")
                results.append({"slug": slug, "status": "fail", "detail": detail[:300]})
        except subprocess.TimeoutExpired:
            counts["fail"] += 1
            results.append({"slug": slug, "status": "fail", "detail": "timeout"})

    for result in results:
        if result["status"] == "ok":
            print(f"ok       {result['slug']:<24} {result.get('detail', '')}")
        else:
            print(f"{result['status']:<8} {result['slug']:<24} {result.get('detail', '')}")

    print(
        f"\n{counts['ok']}/{len(results)} framework-path titles ran "
        f"({counts['fail']} failed, {counts['missing']} missing from corpus)"
    )
    if args.json:
        args.json.write_text(json.dumps({"results": results, "counts": counts}, indent=2) + "\n")
        print(f"json: {args.json}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
