"""Stitch browser-pane screenshot tiles back into one image.

Usage: python stitch_tiles.py OUT W H x0,y0,tile.jpg [x0,y0,tile.jpg ...]
Each tile shows the source image at 1 image px = 1 screenshot px, offset by (x0, y0).
"""
import sys
from PIL import Image

out, w, h = sys.argv[1], int(sys.argv[2]), int(sys.argv[3])
canvas = Image.new("RGB", (w, h))
for spec in sys.argv[4:]:
    x0, y0, path = spec.split(",", 2)
    x0, y0 = int(x0), int(y0)
    tile = Image.open(path).convert("RGB")
    cw, ch = min(tile.width, w - x0), min(tile.height, h - y0)
    canvas.paste(tile.crop((0, 0, cw, ch)), (x0, y0))
canvas.save(out, quality=95)
print("saved", out, canvas.size)
