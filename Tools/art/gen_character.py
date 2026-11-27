"""PixelLab characters: create-character-pro from a concept crop, then template animations.

Usage:
  python -I Tools/art/gen_character.py create JOBS.json [name ...]   # create (10-25 gen each), download rotations
  python -I Tools/art/gen_character.py animate JOBS.json [name ...]  # add the job's "animations" (template, 1 gen/direction)
JOBS.json: {"out": "ArtSource/raw/pixellab/chars",
            "jobs": [{"name": "e_archer", "size": 48, "concept": "crop.png", "reference": "ref.png",
                      "description": "...", "animations": [{"name": "idle", "template": "breathing-idle"}],
                      "directions": ["east"]}]}
Character ids are kept in ArtSource/characters.json. At most 2 PixelLab jobs at once (429 otherwise).
Pro jobs are billed even if cancelled — never cancel them mid-way."""
import io, json, os, sys, threading, time, zipfile
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import pl
from PIL import Image

REG = "ArtSource/characters.json"
LOCK = threading.Lock()


def registry():
    return json.load(open(REG, encoding="utf-8")) if os.path.exists(REG) else {}


def save_registry(r):
    with open(REG, "w", encoding="utf-8") as f: json.dump(r, f, ensure_ascii=False, indent=1)


def b64(path, max_side):
    im = Image.open(path).convert("RGBA")
    return pl.b64img(im, max_side=max_side)


def download(cid, dest):
    raw = pl.req("GET", f"/characters/{cid}/zip", raw=True, timeout=300)
    os.makedirs(dest, exist_ok=True)
    zipfile.ZipFile(io.BytesIO(raw)).extractall(dest)


def create(job, out):
    body = {"description": job["description"], "image_size": {"width": job["size"], "height": job["size"]},
            "method": "create_from_concept", "view": job.get("view", "low top-down"),
            "concept_image": b64(job["concept"], 1024), "no_background": True,
            "style_description": job.get("style_description"),
            "target_palette": {"colors": pl.ENDESGA32, "name": "Endesga 32"}}
    if job.get("reference"): body["reference_image"] = b64(job["reference"], 168)
    if job.get("template"): body["template_id"] = job["template"]
    r = pl.req("POST", "/create-character-pro", body, timeout=300)
    cid = r.get("character_id")
    j = pl.wait_job(r["background_job_id"], every=10) if r.get("background_job_id") else {"status": "completed"}
    cid = cid or (j.get("last_response") or {}).get("character_id")
    with LOCK:
        reg = registry(); reg[job["name"]] = {"character_id": cid, "status": j.get("status"), "usage": j.get("usage") or r.get("usage"),
                                             "size": job["size"], "description": job["description"]}; save_registry(reg)
    if cid and j.get("status") == "completed":
        download(cid, os.path.join(out, job["name"]))
    return f"{job['name']}: {j.get('status')} id={cid} usage={j.get('usage') or r.get('usage')}"


def animate(job, out):
    cid = registry().get(job["name"], {}).get("character_id")
    if not cid: return f"{job['name']}: no character"
    done = []
    for a in job.get("animations", []):
        body = {"character_id": cid, "animation_name": a["name"], "mode": "template", "template_animation_id": a["template"],
                "directions": a.get("directions", job.get("directions", ["east"]))}
        r = pl.req("POST", "/animate-character", body, timeout=300)
        for jid in r.get("background_job_ids", []): pl.wait_job(jid, every=8)
        done.append(a["name"])
    download(cid, os.path.join(out, job["name"]))
    return f"{job['name']}: animations {done}"


def main():
    mode, spec = sys.argv[1], json.load(open(sys.argv[2], encoding="utf-8"))
    only = set(sys.argv[3:])
    jobs = [j for j in spec["jobs"] if not only or j["name"] in only]
    fn = create if mode == "create" else animate
    sem = threading.Semaphore(2); threads = []
    def worker(j):
        with sem:
            try: msg = fn(j, spec["out"])
            except SystemExit as e: msg = f"{j['name']}: FAILED {e}"
            except Exception as e: msg = f"{j['name']}: ERROR {type(e).__name__} {e}"
            with LOCK: print(msg, flush=True)
    for j in jobs:
        t = threading.Thread(target=worker, args=(j,)); t.start(); threads.append(t); time.sleep(2)
    for t in threads: t.join()
    print("balance", pl.balance())


if __name__ == "__main__":
    main()
