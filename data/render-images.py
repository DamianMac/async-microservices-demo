#!/usr/bin/env python3
"""Render images/*.svg to images/rendered/*.png using headless Chrome.

Usage: python3 render-images.py [--scale 2]

Chrome is found at its usual macOS/Linux locations, or set CHROME to its path.
"""
import argparse
import os
import shutil
import subprocess
import sys
import tempfile
import time
from concurrent.futures import ThreadPoolExecutor
from pathlib import Path
from xml.etree import ElementTree

HERE = Path(__file__).resolve().parent
SRC = HERE / "images"
OUT = SRC / "rendered"

CHROME_CANDIDATES = [
    "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
    "/Applications/Chromium.app/Contents/MacOS/Chromium",
    "google-chrome",
    "chromium",
    "chromium-browser",
]


def find_chrome():
    for candidate in [os.environ.get("CHROME")] + CHROME_CANDIDATES:
        if candidate and (Path(candidate).exists() or shutil.which(candidate)):
            return candidate
    sys.exit("Chrome not found; set CHROME to its path")


def svg_size(path):
    root = ElementTree.parse(path).getroot()
    return int(float(root.get("width"))), int(float(root.get("height")))


def render(chrome, svg, scale):
    width, height = svg_size(svg)
    png = OUT / (svg.stem + ".png")
    png.unlink(missing_ok=True)
    with tempfile.TemporaryDirectory() as profile:
        # Headless Chrome on macOS often writes the screenshot and then never
        # exits, so wait for the file rather than for the process.
        proc = subprocess.Popen(
            [
                chrome,
                "--headless",
                "--disable-gpu",
                "--hide-scrollbars",
                f"--user-data-dir={profile}",
                f"--window-size={width},{height}",
                f"--force-device-scale-factor={scale}",
                f"--screenshot={png}",
                svg.as_uri(),
            ],
            stdout=subprocess.DEVNULL,
            stderr=subprocess.DEVNULL,
        )
        try:
            deadline = time.monotonic() + 30
            while not (png.exists() and png.stat().st_size > 0):
                if proc.poll() is not None or time.monotonic() > deadline:
                    raise RuntimeError(f"Chrome failed to render {svg.name}")
                time.sleep(0.2)
            time.sleep(0.3)  # let the write finish
        finally:
            proc.terminate()
            try:
                proc.wait(timeout=5)
            except subprocess.TimeoutExpired:
                proc.kill()
    return png


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--scale", type=float, default=1, help="pixel density, e.g. 2 for retina")
    args = parser.parse_args()

    chrome = find_chrome()
    OUT.mkdir(exist_ok=True)
    svgs = sorted(SRC.glob("*.svg"))

    with ThreadPoolExecutor(max_workers=4) as pool:
        for png in pool.map(lambda svg: render(chrome, svg, args.scale), svgs):
            print(png.relative_to(HERE))

    print(f"Rendered {len(svgs)} images to {OUT.relative_to(HERE)}")


if __name__ == "__main__":
    main()
