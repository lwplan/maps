#!/usr/bin/env python3
"""Print a tiny ASCII preview for each path texture.

This avoids needing image libraries; it decodes PNGs directly and samples
luminance so you can infer the layout (straight, bend, T, cross, etc.).
Usage:
    python scripts/preview_paths.py UnityProject/Assets/TileW/materials/path*.png
"""
from __future__ import annotations

import struct
import sys
import zlib
from pathlib import Path
from typing import Iterable, List, Sequence, Tuple

Pixel = Tuple[int, int, int, int]
Image = List[List[Pixel]]


def decode_png(path: Path) -> Image:
    data = path.read_bytes()
    if not data.startswith(b"\x89PNG\r\n\x1a\n"):
        raise ValueError(f"{path} is not a PNG file")

    pos = 8
    idat: List[bytes] = []
    width = height = None

    while pos < len(data):
        length = int.from_bytes(data[pos : pos + 4], "big")
        pos += 4
        ctype = data[pos : pos + 4]
        pos += 4
        chunk = data[pos : pos + length]
        pos += length
        pos += 4  # skip CRC

        if ctype == b"IHDR":
            width, height, bit_depth, color_type, _, _, _ = struct.unpack(
                ">IIBBBBB", chunk
            )
            if color_type != 6 or bit_depth != 8:
                raise ValueError(
                    f"Unsupported color type/bit depth in {path}: {color_type}/{bit_depth}"
                )
        elif ctype == b"IDAT":
            idat.append(chunk)
        elif ctype == b"IEND":
            break

    raw = zlib.decompress(b"".join(idat))
    bpp = 4
    rowbytes = width * bpp
    rows: List[bytearray] = []
    idx = 0

    for y in range(height):
        filter_type = raw[idx]
        idx += 1
        row = bytearray(raw[idx : idx + rowbytes])
        idx += rowbytes

        if filter_type == 1:  # Sub
            for i in range(bpp, rowbytes):
                row[i] = (row[i] + row[i - bpp]) & 0xFF
        elif filter_type == 2:  # Up
            prev = rows[y - 1] if y else [0] * rowbytes
            for i in range(rowbytes):
                row[i] = (row[i] + prev[i]) & 0xFF
        elif filter_type == 3:  # Average
            prev = rows[y - 1] if y else [0] * rowbytes
            for i in range(rowbytes):
                left = row[i - bpp] if i >= bpp else 0
                row[i] = (row[i] + ((left + prev[i]) >> 1)) & 0xFF
        elif filter_type == 4:  # Paeth
            prev = rows[y - 1] if y else [0] * rowbytes
            for i in range(rowbytes):
                left = row[i - bpp] if i >= bpp else 0
                up = prev[i]
                upleft = prev[i - bpp] if i >= bpp else 0

                p = left + up - upleft
                pa = abs(p - left)
                pb = abs(p - up)
                pc = abs(p - upleft)
                if pa <= pb and pa <= pc:
                    pr = left
                elif pb <= pc:
                    pr = up
                else:
                    pr = upleft
                row[i] = (row[i] + pr) & 0xFF
        rows.append(row)

    return [
        [tuple(row[i : i + 4]) for i in range(0, len(row), 4)] for row in rows
    ]


def luminance(pixel: Pixel) -> float:
    r, g, b, _ = pixel
    return 0.2126 * r + 0.7152 * g + 0.0722 * b


def ascii_preview(img: Image, size: int = 9) -> str:
    h = len(img)
    w = len(img[0])
    block_h = h // size
    block_w = w // size
    chars = "@%#*+=-:. "
    lines: List[str] = []

    for gy in range(size):
        row_chars: List[str] = []
        for gx in range(size):
            vals: List[float] = []
            for y in range(gy * block_h, (gy + 1) * block_h):
                for x in range(gx * block_w, (gx + 1) * block_w):
                    vals.append(luminance(img[y][x]))
            mean = sum(vals) / len(vals)
            row_chars.append(chars[int(mean / 255 * (len(chars) - 1))])
        lines.append("".join(row_chars))
    return "\n".join(lines)


def preview(paths: Iterable[Path], size: int = 9) -> None:
    for path in paths:
        try:
            img = decode_png(path)
        except Exception as exc:  # noqa: BLE001 - display any parsing issues
            print(f"\n{path}: [error] {exc}")
            continue
        print(f"\n{path.name}\n{ascii_preview(img, size=size)}")


def main(args: Sequence[str]) -> int:
    if not args:
        print("Provide one or more PNG files to preview.")
        return 1
    preview(Path(p) for p in args)
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
