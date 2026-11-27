"""Check weapon sprites against Art Bible §2 and §5.1.

Usage:
  python3 Tools/art/check_weapon.py <png> <S|M|L> [--grip X,Y] [--fix OUT.png]

Checks: canvas allowed for the size class and a multiple of 8, weapon length (opaque bbox; bbox diagonal
for diagonal sprites) inside the class range, every opaque pixel is Endesga 32, alpha only
0/255, not a 2x upscale, the grip pixel (if given) lands on an opaque pixel.
--fix writes a copy snapped to Endesga 32 with hard alpha (it does not resize). Exit code 1 on any failure.
"""
import argparse
import sys

from PIL import Image

ENDESGA32 = ["be4a2f", "d77643", "ead4aa", "e4a672", "b86f50", "733e39", "3e2731", "a22633",
             "e43b44", "f77622", "feae34", "fee761", "63c74d", "3e8948", "265c42", "193c3e",
             "124e89", "0099db", "2ce8f5", "ffffff", "c0cbdc", "8b9bb4", "5a6988", "3a4466",
             "262b44", "181425", "ff0044", "68386c", "b55088", "f6757a", "e8b796", "c28569"]
PALETTE = [tuple(int(h[i:i + 2], 16) for i in (0, 2, 4)) for h in ENDESGA32]

# class: (min length, max length, allowed canvases)
CLASSES = {
    "S": (12, 16, {(16, 16)}),
    "M": (24, 36, {(32, 16), (32, 24), (16, 32), (32, 32)}),
    "L": (40, 48, {(48, 16), (48, 24), (48, 48)}),
}


def nearest(rgb):
    return min(PALETTE, key=lambda p: (p[0] - rgb[0]) ** 2 + (p[1] - rgb[1]) ** 2 + (p[2] - rgb[2]) ** 2)


def is_2x_upscale(im):
    w, h = im.size
    if w % 2 or h % 2:
        return False
    small = im.resize((w // 2, h // 2), Image.NEAREST).resize((w, h), Image.NEAREST)
    return list(small.getdata()) == list(im.getdata())


def check(path, cls, grip=None):
    im = Image.open(path).convert("RGBA")
    w, h = im.size
    lo, hi, canvases = CLASSES[cls]
    errors = []
    if w % 8 or h % 8:
        errors.append(f"canvas {w}x{h} is not a multiple of 8")
    if (w, h) not in canvases:
        errors.append(f"canvas {w}x{h} not allowed for class {cls}: {sorted(canvases)}")
    px = im.load()
    bad_alpha = bad_color = 0
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            if a not in (0, 255):
                bad_alpha += 1
            elif a == 255 and (r, g, b) not in PALETTE:
                bad_color += 1
    if bad_alpha:
        errors.append(f"{bad_alpha} pixels with partial alpha")
    if bad_color:
        errors.append(f"{bad_color} opaque pixels outside Endesga 32")
    bbox = im.getchannel("A").point(lambda a: 255 if a == 255 else 0).getbbox()
    if not bbox:
        errors.append("sprite is empty")
    else:
        bw, bh = bbox[2] - bbox[0], bbox[3] - bbox[1]
        # diagonal sprites (spriteForwardAngle ~45/225): length is the bbox diagonal
        length = round((bw ** 2 + bh ** 2) ** 0.5) if min(bw, bh) >= 0.6 * max(bw, bh) else max(bw, bh)
        if not lo <= length <= hi:
            errors.append(f"length {length}px outside class {cls} range {lo}-{hi}px")
    if is_2x_upscale(im):
        errors.append("image is a 2x nearest upscale (pixels are not 1:1)")
    if grip:
        gx, gy = grip
        if not (0 <= gx < w and 0 <= gy < h) or px[gx, gy][3] != 255:
            errors.append(f"grip pixel {gx},{gy} is not on an opaque pixel")
    return errors


def fix(path, out):
    im = Image.open(path).convert("RGBA")
    px = im.load()
    for y in range(im.height):
        for x in range(im.width):
            r, g, b, a = px[x, y]
            px[x, y] = (0, 0, 0, 0) if a < 128 else (*nearest((r, g, b)), 255)
    im.save(out)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("png")
    ap.add_argument("size_class", choices=sorted(CLASSES))
    ap.add_argument("--grip", help="X,Y in sprite pixels, origin top-left")
    ap.add_argument("--fix", metavar="OUT")
    a = ap.parse_args()
    src = a.png
    if a.fix:
        fix(a.png, a.fix)
        src = a.fix
    grip = tuple(int(v) for v in a.grip.split(",")) if a.grip else None
    errors = check(src, a.size_class, grip)
    for e in errors:
        print("FAIL", src, "-", e)
    if not errors:
        print("OK", src)
    sys.exit(1 if errors else 0)


if __name__ == "__main__":
    main()
