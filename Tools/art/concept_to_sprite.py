"""Concept crop -> game sprite: cut the background, shrink to the target size, snap to Endesga 32, hard alpha, 1px outline.
For small sprites (icons, pickups, projectiles) drawn on the approved concept sheets — no PixelLab generations spent.

Usage: python -I Tools/art/concept_to_sprite.py JOBS.json
JOBS.json: {"items": [{"sheet": "Research/concepts/sheets/15_passive_icons.jpg", "box": [x0, y0, x1, y1],
                       "size": 16, "out": "Assets/_Project/Art/Sprites/Icons/icon_passive_01.png", "canvas": 16}]}
size  = longest side of the sprite in pixels; canvas = square canvas (multiple of 8), sprite centred."""
import json, sys
from PIL import Image

E32 = ["#be4a2f","#d77643","#ead4aa","#e4a672","#b86f50","#733e39","#3e2731","#a22633","#e43b44","#f77622","#feae34","#fee761",
       "#63c74d","#3e8948","#265c42","#193c3e","#124e89","#0099db","#2ce8f5","#ffffff","#c0cbdc","#8b9bb4","#5a6988","#3a4466",
       "#262b44","#181425","#ff0044","#68386c","#b55088","#f6757a","#e8b796","#c28569"]
PAL = [tuple(int(h[i:i + 2], 16) for i in (1, 3, 5)) for h in E32]


def nearest(c):
    return min(PAL, key=lambda p: (p[0] - c[0]) ** 2 * 2 + (p[1] - c[1]) ** 2 * 4 + (p[2] - c[2]) ** 2 * 3)


def cut_background(im, tol=34):
    """Flood-fill from the border: pixels close to the border colour become transparent (concept sheets use a flat navy)."""
    im = im.convert("RGBA"); w, h = im.size; px = im.load()
    bg = px[0, 0][:3]
    seen = set(); stack = [(x, y) for x in range(w) for y in (0, h - 1)] + [(x, y) for y in range(h) for x in (0, w - 1)]
    while stack:
        x, y = stack.pop()
        if (x, y) in seen or not (0 <= x < w and 0 <= y < h): continue
        seen.add((x, y)); r, g, b, a = px[x, y]
        if abs(r - bg[0]) + abs(g - bg[1]) + abs(b - bg[2]) > tol: continue
        px[x, y] = (0, 0, 0, 0)
        stack += [(x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)]
    return im


def shrink(im, size):
    bb = im.getbbox(); im = im.crop(bb) if bb else im
    k = size / max(im.size)
    w, h = max(1, round(im.width * k)), max(1, round(im.height * k))
    small = im.resize((w, h), Image.BOX)
    px = small.load()
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            px[x, y] = (*nearest((r, g, b)), 255) if a >= 110 else (0, 0, 0, 0)
    return small


def largest_blob(im):
    """Keep only the biggest 4-connected opaque piece (drops crumbs of neighbouring items)."""
    w, h = im.size; px = im.load(); seen = set(); best = []
    for y in range(h):
        for x in range(w):
            if (x, y) in seen or not px[x, y][3]: continue
            comp, stack = [], [(x, y)]
            while stack:
                p = stack.pop()
                if p in seen or not (0 <= p[0] < w and 0 <= p[1] < h) or not px[p][3]: continue
                seen.add(p); comp.append(p)
                stack += [(p[0] + 1, p[1]), (p[0] - 1, p[1]), (p[0], p[1] + 1), (p[0], p[1] - 1)]
            if len(comp) > len(best): best = comp
    keep = set(best)
    for y in range(h):
        for x in range(w):
            if (x, y) not in keep: px[x, y] = (0, 0, 0, 0)
    return im


def outline(im):
    """Darkest palette colour around the silhouette where it has no outline yet (Art Bible: 1px dark outline)."""
    w, h = im.size; src = im.load(); out = im.copy(); dst = out.load(); dark = (*PAL[25], 255)
    for y in range(h):
        for x in range(w):
            if src[x, y][3]: continue
            if any(0 <= x + dx < w and 0 <= y + dy < h and src[x + dx, y + dy][3] for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1))):
                dst[x, y] = dark
    return out


def convert(item):
    im = Image.open(item["sheet"]).crop(tuple(item["box"]))
    im = cut_background(im, item.get("tol", 34))
    if item.get("circle"):  # keep only a centred disc (drops sheet numbers next to round badges)
        from PIL import ImageDraw
        m = Image.new("L", im.size, 0); w, h = im.size; r = min(w, h) / 2 * item["circle"]
        ImageDraw.Draw(m).ellipse((w / 2 - r, h / 2 - r, w / 2 + r, h / 2 + r), fill=255)
        im.putalpha(Image.composite(im.getchannel("A"), m, m))
    sprite = shrink(im, item["size"] - (2 if item.get("outline", True) else 0))
    if item.get("largest"): sprite = largest_blob(sprite)
    canvas = item.get("canvas", item["size"])
    c = Image.new("RGBA", (canvas, canvas))
    c.alpha_composite(sprite, ((canvas - sprite.width) // 2, (canvas - sprite.height) // 2))
    if item.get("outline", True): c = outline(c)
    c.save(item["out"])
    return item["out"]


if __name__ == "__main__":
    import os
    spec = json.load(open(sys.argv[1], encoding="utf-8"))
    for it in spec["items"]:
        os.makedirs(os.path.dirname(it["out"]), exist_ok=True)
        print(convert(it))
