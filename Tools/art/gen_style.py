"""Batch PixelLab generate-with-style-v2 (Pro): one generation per job -> many candidates + a contact sheet.

Usage: python -I Tools/art/gen_style.py JOBS.json [only_name ...]
JOBS.json: {"out": "ArtSource/raw/pixellab/<batch>", "style_description": "...",
            "jobs": [{"name": "w03_sabre", "canvas": 28, "style": ["path.png", ...], "description": "..."}]}
Each style image is centred on a canvas x canvas square (its size picks the output size and candidate count).
At most 2 jobs run at once (PixelLab returns 429 above that). Key: env PIXELLAB_KEY (see pl.py)."""
import base64, io, json, os, sys, threading, time
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import pl
from PIL import Image


def fit(path, canvas):
    im = Image.open(path).convert("RGBA")
    bb = im.getbbox()
    if bb: im = im.crop(bb)
    if max(im.size) > canvas:
        k = canvas / max(im.size)
        im = im.resize((max(1, round(im.width * k)), max(1, round(im.height * k))), Image.NEAREST)
    c = Image.new("RGBA", (canvas, canvas))
    c.alpha_composite(im, ((canvas - im.width) // 2, (canvas - im.height) // 2))
    return c


def images_from(resp):
    """Find base64 images anywhere in a job's last_response."""
    out = []
    def walk(x):
        if isinstance(x, dict):
            if "base64" in x and isinstance(x["base64"], str): out.append(x["base64"]); return
            for v in x.values(): walk(v)
        elif isinstance(x, list):
            for v in x: walk(v)
    walk(resp)
    return out


def run(job, spec, out_root, lock):
    d = os.path.join(out_root, job["name"]); os.makedirs(d, exist_ok=True)
    styles = [fit(p, job["canvas"]) for p in job["style"]]
    body = {"description": job["description"],
            "style_images": [{"image": pl.b64img(s), "width": s.width, "height": s.height} for s in styles],
            "style_description": job.get("style_description", spec.get("style_description")),
            "no_background": True,
            "target_palette": {"colors": pl.ENDESGA32, "name": "Endesga 32"}}
    r = pl.req("POST", "/generate-with-style-v2", body)
    j = pl.wait_job(r["background_job_id"], every=6)
    with open(os.path.join(d, "job.json"), "w", encoding="utf-8") as f:
        json.dump({"request": {k: v for k, v in body.items() if k != "style_images"}, "status": j.get("status"),
                   "usage": j.get("usage"), "keys": list((j.get("last_response") or {}).keys())}, f, ensure_ascii=False, indent=1)
    imgs = images_from(j.get("last_response"))
    tiles = []
    for i, b in enumerate(imgs):
        im = Image.open(io.BytesIO(base64.b64decode(b))).convert("RGBA")
        im.save(os.path.join(d, f"{job['name']}_{i:02d}.png")); tiles.append(im)
    if tiles:
        cols = 8; s = tiles[0].width; z = max(1, 64 // s)
        sheet = Image.new("RGBA", (cols * (s * z + 4), ((len(tiles) + cols - 1) // cols) * (s * z + 4)), (38, 43, 68, 255))
        for i, t in enumerate(tiles):
            sheet.alpha_composite(t.resize((s * z, s * z), Image.NEAREST), ((i % cols) * (s * z + 4), (i // cols) * (s * z + 4)))
        sheet.save(os.path.join(d, "_sheet.png"))
    with lock:
        print(f"{job['name']}: {j.get('status')} {len(tiles)} images usage={j.get('usage')}", flush=True)


def main():
    spec = json.load(open(sys.argv[1], encoding="utf-8"))
    only = set(sys.argv[2:])
    jobs = [j for j in spec["jobs"] if not only or j["name"] in only]
    lock = threading.Lock(); sem = threading.Semaphore(2); threads = []
    def worker(j):
        with sem:
            try: run(j, spec, spec["out"], lock)
            except SystemExit as e:
                with lock: print(f"{j['name']}: FAILED {e}", flush=True)
            time.sleep(1)
    for j in jobs:
        t = threading.Thread(target=worker, args=(j,)); t.start(); threads.append(t)
    for t in threads: t.join()
    print("balance", pl.balance())


if __name__ == "__main__":
    main()
