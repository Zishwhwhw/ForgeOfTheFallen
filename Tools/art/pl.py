"""Minimal PixelLab v2 client. Key: env PIXELLAB_KEY or macOS Keychain item 'PIXELLAB_KEY'. Never write the key to files."""
import base64, io, json, os, subprocess, sys, time, urllib.request, urllib.error

API = "https://api.pixellab.ai/v2"
ENDESGA32 = ["#be4a2f","#d77643","#ead4aa","#e4a672","#b86f50","#733e39","#3e2731","#a22633",
             "#e43b44","#f77622","#feae34","#fee761","#63c74d","#3e8948","#265c42","#193c3e",
             "#124e89","#0099db","#2ce8f5","#ffffff","#c0cbdc","#8b9bb4","#5a6988","#3a4466",
             "#262b44","#181425","#ff0044","#68386c","#b55088","#f6757a","#e8b796","#c28569"]

def key():
    k = os.environ.get("PIXELLAB_KEY")
    if not k and sys.platform == "win32":
        import winreg  # переменная пользователя, заданная через SetEnvironmentVariable(..., 'User')
        try:
            with winreg.OpenKey(winreg.HKEY_CURRENT_USER, "Environment") as reg:
                k = winreg.QueryValueEx(reg, "PIXELLAB_KEY")[0]
        except OSError:
            sys.exit("PIXELLAB_KEY не задан: [Environment]::SetEnvironmentVariable('PIXELLAB_KEY', '<ключ>', 'User')")
    if not k:
        k = subprocess.run(["security", "find-generic-password", "-a", os.environ["USER"], "-s", "PIXELLAB_KEY", "-w"],
                           capture_output=True, text=True, check=True).stdout.strip()
    return k

def req(method, path, body=None, raw=False, timeout=120):
    data = json.dumps(body).encode() if body is not None else None
    r = urllib.request.Request(API + path, data=data, method=method,
                               headers={"Authorization": "Bearer " + key(), "Content-Type": "application/json"})
    try:
        with urllib.request.urlopen(r, timeout=timeout) as resp:
            b = resp.read()
            return b if raw else json.loads(b)
    except urllib.error.HTTPError as e:
        sys.exit(f"HTTP {e.code} {path}: {e.read().decode()[:1500]}")

def b64img(path_or_pil, max_side=None):
    from PIL import Image
    im = path_or_pil if not isinstance(path_or_pil, str) else Image.open(path_or_pil)
    im = im.convert("RGBA")
    if max_side and max(im.size) > max_side:
        im.thumbnail((max_side, max_side), Image.LANCZOS)
    buf = io.BytesIO(); im.save(buf, "PNG")
    return {"type": "base64", "base64": base64.b64encode(buf.getvalue()).decode(), "format": "png"}

def wait_job(job_id, every=10, limit=1800):
    t = 0
    while t < limit:
        j = req("GET", f"/background-jobs/{job_id}")
        st = j.get("status")
        if st in ("completed", "failed", "cancelled", "error"):
            return j
        time.sleep(every); t += every
    sys.exit(f"job {job_id} timeout")

def balance():
    return req("GET", "/balance")["subscription"]["generations"]
