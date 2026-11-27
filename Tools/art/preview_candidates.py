"""Numbered preview of PixelLab candidates: python -I Tools/art/preview_candidates.py DIR OUT.png [max] [zoom]"""
import glob, os, sys
from PIL import Image, ImageDraw

d, out = sys.argv[1], sys.argv[2]
limit = int(sys.argv[3]) if len(sys.argv) > 3 else 64
files = sorted(f for f in glob.glob(os.path.join(d, "*.png")) if not os.path.basename(f).startswith("_"))[:limit]
ims = [Image.open(f).convert("RGBA") for f in files]
s = max(i.width for i in ims)
z = int(sys.argv[4]) if len(sys.argv) > 4 else max(1, 96 // s)
cell = s * z + 14; cols = 8
sheet = Image.new("RGBA", (cols * cell, ((len(ims) + cols - 1) // cols) * cell), (38, 43, 68, 255))
dr = ImageDraw.Draw(sheet)
for i, im in enumerate(ims):
    x, y = (i % cols) * cell, (i // cols) * cell
    sheet.alpha_composite(im.resize((im.width * z, im.height * z), Image.NEAREST), (x + 7, y + 12))
    dr.text((x + 2, y), str(i), fill=(255, 255, 0, 255))
sheet.save(out)
print(len(ims), "->", out)
