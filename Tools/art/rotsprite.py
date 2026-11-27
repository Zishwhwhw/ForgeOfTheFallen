"""RotSprite-style rotation for pixel art: Scale2x x3 (8x) -> nearest rotate -> 8x downsample (centre sample)."""
from PIL import Image
def scale2x(im):
    w,h=im.size; s=im.load(); o=Image.new('RGBA',(w*2,h*2)); d=o.load()
    g=lambda x,y: s[min(max(x,0),w-1),min(max(y,0),h-1)]
    for y in range(h):
        for x in range(w):
            P=g(x,y); A=g(x,y-1); B=g(x+1,y); C=g(x-1,y); D=g(x,y+1)
            e0=C if (C==A and C!=D and A!=B) else P
            e1=A if (A==B and A!=C and B!=D) else P
            e2=C if (D==C and D!=B and C!=A) else P
            e3=B if (B==D and B!=A and D!=C) else P
            d[2*x,2*y]=e0; d[2*x+1,2*y]=e1; d[2*x,2*y+1]=e2; d[2*x+1,2*y+1]=e3
    return o
def rotsprite(im, angle, center):
    big=im
    for _ in range(3): big=scale2x(big)
    big=big.rotate(angle, resample=Image.NEAREST, center=(center[0]*8+4, center[1]*8+4))
    w,h=im.size; o=Image.new('RGBA',(w,h)); d=o.load(); b=big.load()
    for y in range(h):
        for x in range(w):
            p=b[x*8+4,y*8+4]; d[x,y]=p if p[3]>=128 else (0,0,0,0)
            if p[3]>=128: d[x,y]=(p[0],p[1],p[2],255)
    return o


if __name__ == "__main__":
    # Bake rotated copies of a weapon sprite around its grip pixel:
    #   python3 Tools/art/rotsprite.py weapon.png GX GY FORWARD_DEG STEPS OUT_DIR
    # Writes OUT_DIR/<name>_<angle>.png for STEPS evenly spaced aim angles (e.g. 32 -> every 11.25 deg).
    import os, sys
    src, gx, gy, fwd, steps, out = sys.argv[1], int(sys.argv[2]), int(sys.argv[3]), float(sys.argv[4]), int(sys.argv[5]), sys.argv[6]
    im = Image.open(src).convert("RGBA"); n = 2 * max(im.size) + 8
    canvas = Image.new("RGBA", (n, n)); canvas.alpha_composite(im, (n // 2 - gx, n // 2 - gy))
    os.makedirs(out, exist_ok=True); name = os.path.splitext(os.path.basename(src))[0]
    for i in range(steps):
        aim = i * 360.0 / steps
        rotsprite(canvas, aim - fwd, (n // 2, n // 2)).save(os.path.join(out, f"{name}_{int(round(aim * 100)):05d}.png"))
    print(f"{steps} angles -> {out} (grip at canvas centre {n // 2},{n // 2})")


def bake_frames(src, grip_unity, fwd, steps, out_dir, name):
    """`steps` RotSprite frames as separate PNGs out_dir/name_00.png… (aim i*360/steps, CCW from +x),
    grip at each frame's centre pixel. grip_unity counts from the bottom-left (WeaponDefinition.gripPixel).
    Frame side is a multiple of 8 (Art Bible §2)."""
    import os
    im = Image.open(src).convert("RGBA")
    gx, gy = int(grip_unity[0]), im.height - 1 - int(grip_unity[1])
    n = 2 * max(im.size) + 8
    n += (-n) % 8
    canvas = Image.new("RGBA", (n, n)); canvas.alpha_composite(im, (n // 2 - gx, n // 2 - gy))
    os.makedirs(out_dir, exist_ok=True)
    for i in range(steps):
        rotsprite(canvas, i * 360.0 / steps - fwd, (n // 2, n // 2)).save(os.path.join(out_dir, f"{name}_{i:02d}.png"))
    return n
