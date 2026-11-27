"""Bake 32 RotSprite angles for every weapon (Art Bible §5.1) into Assets/_Project/Art/Sprites/Weapons/Rotated/.
Usage: python -I Tools/art/bake_weapon_rotations.py   (reads grip/forward from the weapon .asset files)
Then in Unity: FotF/Art/Assign Weapon Rotations — sets the grip pivot on each frame and assigns WeaponDefinition.rotatedSprites."""
import os, re, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from rotsprite import bake_frames

DATA = "Assets/_Project/Data/Weapons"
ART = "Assets/_Project/Art/Sprites/Weapons"
OUT = os.path.join(ART, "Rotated")
STEPS = 32

def field(text, name):
    m = re.search(rf"^\s*{name}: (.+)$", text, re.M)
    return m.group(1).strip() if m else None

os.makedirs(OUT, exist_ok=True)
for f in sorted(os.listdir(DATA)):
    if not f.endswith(".asset"): continue
    text = open(os.path.join(DATA, f), encoding="utf-8").read()
    name = os.path.splitext(f)[0]
    src = os.path.join(ART, name + ".png")
    grip = re.search(r"gripPixel: \{x: ([\d.\-]+), y: ([\d.\-]+)\}", text)
    if not os.path.exists(src) or not grip: print("skip", name); continue
    fwd = float(field(text, "spriteForwardAngle") or 0)
    n = bake_frames(src, (float(grip.group(1)), float(grip.group(2))), fwd, STEPS, os.path.join(OUT, name), name + "_rot")
    print(f"{name}: {STEPS} x {n}px")
