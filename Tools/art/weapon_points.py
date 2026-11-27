"""Grip, muzzle and forward angle of a weapon sprite drawn by the prompt convention
"grip at bottom-left, tip/muzzle at top-right" (Art Bible §5.1). Pixels count from the bottom-left (WeaponDefinition).

kind: melee | gun | staff | bow
  grip    = centroid of handle-coloured pixels (wood/leather) in the bottom-left half; fallback 22 % along the axis
  muzzle  = opaque pixel farthest toward the top-right along the forward axis
  forward = melee/staff: grip -> tip; gun: main axis of the non-handle (metal) pixels; bow: 0 (string on the left)
Usage: python -I Tools/art/weapon_points.py KIND sprite.png [annotated_out.png]"""
import math, sys
from PIL import Image, ImageDraw

HANDLE = {(0x73, 0x3e, 0x39), (0x3e, 0x27, 0x31), (0xb8, 0x6f, 0x50), (0xbe, 0x4a, 0x2f), (0xc2, 0x85, 0x69)}


def axis(pts):
    cx = sum(p[0] for p in pts) / len(pts); cy = sum(p[1] for p in pts) / len(pts)
    sxx = sum((p[0] - cx) ** 2 for p in pts); syy = sum((p[1] - cy) ** 2 for p in pts); sxy = sum((p[0] - cx) * (p[1] - cy) for p in pts)
    ang = 0.5 * math.atan2(2 * sxy, sxx - syy)
    return (cx, cy), (math.cos(ang), math.sin(ang))


def points(kind, path):
    im = Image.open(path).convert("RGBA"); w, h = im.size; px = im.load()
    # work in "up" coordinates: u = x, v = h-1-y
    opaque = [(x, h - 1 - y) for y in range(h) for x in range(w) if px[x, y][3]]
    (cx, cy), d = axis(opaque)
    if d[0] + d[1] < 0: d = (-d[0], -d[1])                       # orient toward the top-right
    proj = lambda p: (p[0] - cx) * d[0] + (p[1] - cy) * d[1]
    lo, hi = min(map(proj, opaque)), max(map(proj, opaque))
    tip = max(opaque, key=proj)
    if kind == "bow":
        grip = (round(cx), round(cy)); return grip, (round(cx) + 3, round(cy)), 0.0, (w, h)
    handle = [p for p in opaque if px[p[0], h - 1 - p[1]][:3] in HANDLE and proj(p) < (lo + hi) / 2]
    if handle:
        grip = (round(sum(p[0] for p in handle) / len(handle)), round(sum(p[1] for p in handle) / len(handle)))
    else:
        t = lo + 0.22 * (hi - lo); grip = (round(cx + d[0] * t), round(cy + d[1] * t))
    if kind == "gun":
        metal = [p for p in opaque if px[p[0], h - 1 - p[1]][:3] not in HANDLE]
        _, dm = axis(metal or opaque)
        if dm[0] + dm[1] < 0: dm = (-dm[0], -dm[1])
        fwd = math.degrees(math.atan2(dm[1], dm[0])) % 360
        tip = max(opaque, key=lambda p: (p[0] - grip[0]) * dm[0] + (p[1] - grip[1]) * dm[1])
    else:
        fwd = math.degrees(math.atan2(tip[1] - grip[1], tip[0] - grip[0])) % 360
    return grip, tip, round(fwd, 1), (w, h)


def annotate(path, grip, tip, out, z=8):
    im = Image.open(path).convert("RGBA"); w, h = im.size
    big = Image.new("RGBA", (w * z, h * z), (38, 43, 68, 255)); big.alpha_composite(im.resize((w * z, h * z), Image.NEAREST))
    dr = ImageDraw.Draw(big)
    for (x, y), c in ((grip, (0, 255, 0, 255)), (tip, (255, 0, 255, 255))):
        yy = h - 1 - y; dr.rectangle((x * z, yy * z, x * z + z - 1, yy * z + z - 1), outline=c, width=2)
    big.save(out)


if __name__ == "__main__":
    kind, path = sys.argv[1], sys.argv[2]
    g, t, f, size = points(kind, path)
    print(path, "grip", g, "muzzle", t, "forward", f, "size", size)
    if len(sys.argv) > 3: annotate(path, g, t, sys.argv[3])
