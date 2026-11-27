"""Validate the balance JSON files (ARCHITECTURE §8) against Tools/data/balance_schema.json.

Usage:
  python3 Tools/data/validate_balance.py            # check, exit 1 on any error
  python3 Tools/data/validate_balance.py --format   # also rewrite files in canonical form (2-space indent, UTF-8)

Checks: every kind file exists; kind/version header; unique ids with the right prefix; no unknown fields
(typos), fields starting with '_' are comments; types, min/max, enum values; null only for fields marked
"calibrate"; refs point to existing ids; any other string that looks like an id of a known kind must exist.
"""
import json
import os
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
DATA = os.path.join(ROOT, "Assets", "_Project", "Data", "Balance")
SCHEMA = os.path.join(ROOT, "Tools", "data", "balance_schema.json")


def load(path):
    with open(path, encoding="utf-8") as f:
        return json.load(f)


class Validator:
    def __init__(self, schema, docs):
        self.schema, self.docs, self.errors = schema, docs, []
        self.ids = {}  # kind -> set of ids

    def err(self, where, msg):
        self.errors.append(f"{where}: {msg}")

    def run(self):
        kinds = self.schema["kinds"]
        for kind, spec in kinds.items():
            doc = self.docs.get(spec["file"])
            if doc is None:
                self.err(spec["file"], "file is missing")
                continue
            if doc.get("kind") != kind:
                self.err(spec["file"], f"kind must be '{kind}', got {doc.get('kind')!r}")
            if not isinstance(doc.get("version"), int) or doc["version"] < 1:
                self.err(spec["file"], "version must be an int >= 1")
            items = doc.get("items")
            if not isinstance(items, list):
                self.err(spec["file"], "items must be a list")
                continue
            seen = self.ids.setdefault(kind, set())
            for i, it in enumerate(items):
                iid = it.get("id") if isinstance(it, dict) else None
                where = f"{spec['file']}[{iid or i}]"
                if not isinstance(iid, str) or not iid.startswith(kind + "."):
                    self.err(where, f"id must be a string starting with '{kind}.'")
                elif iid in seen:
                    self.err(where, "duplicate id")
                else:
                    seen.add(iid)
        for fname in self.docs:
            if fname not in {s["file"] for s in kinds.values()}:
                self.err(fname, "file has no kind in the schema")
        all_ids = set().union(*self.ids.values()) if self.ids else set()
        for kind, spec in kinds.items():
            doc = self.docs.get(spec["file"])
            if not doc or not isinstance(doc.get("items"), list):
                continue
            for i, it in enumerate(doc["items"]):
                where = f"{spec['file']}[{it.get('id', i) if isinstance(it, dict) else i}]"
                self.check_object(where, it, spec["fields"])
                self.scan_strings(where, it, all_ids)
        return self.errors

    def check_object(self, where, obj, fields):
        if not isinstance(obj, dict):
            self.err(where, "must be an object")
            return
        for k in obj:
            if not k.startswith("_") and k not in fields:
                self.err(where, f"unknown field '{k}'")
        for k, f in fields.items():
            if k not in obj:
                if not f.get("optional"):
                    self.err(where, f"missing field '{k}'")
                continue
            self.check_value(f"{where}.{k}", obj[k], f)

    def check_value(self, where, v, f):
        t = f["type"]
        if v is None:
            if not (f.get("calibrate") or f.get("nullable")):
                self.err(where, "null is not allowed here")
            return
        if t in ("int", "number"):
            ok = isinstance(v, int) if t == "int" else isinstance(v, (int, float))
            if not ok or isinstance(v, bool):
                self.err(where, f"must be {t}")
                return
            if "min" in f and v < f["min"]:
                self.err(where, f"{v} < min {f['min']}")
            if "max" in f and v > f["max"]:
                self.err(where, f"{v} > max {f['max']}")
        elif t == "bool":
            if not isinstance(v, bool):
                self.err(where, "must be true/false")
        elif t in ("string", "id"):
            if not isinstance(v, str) or not v:
                self.err(where, "must be a non-empty string")
        elif t == "enum":
            allowed = self.schema["enums"][f["enum"]]
            if v not in allowed:
                self.err(where, f"'{v}' not in {f['enum']} {allowed}")
        elif t == "ref":
            if v not in self.ids.get(f["to"], set()):
                self.err(where, f"unknown {f['to']} id '{v}'")
        elif t == "list":
            if not isinstance(v, list):
                self.err(where, "must be a list")
                return
            for i, x in enumerate(v):
                self.check_value(f"{where}[{i}]", x, f["items"])
        elif t == "object":
            self.check_object(where, v, f["fields"])
        elif t == "map":
            if not isinstance(v, dict):
                self.err(where, "must be an object")

    def scan_strings(self, where, v, all_ids, key=""):
        """Any string shaped like '<kind>.<name>' (not a *Key / id field) must be an existing id."""
        if isinstance(v, dict):
            for k, x in v.items():
                if not k.startswith("_"):
                    self.scan_strings(f"{where}.{k}", x, all_ids, k)
        elif isinstance(v, list):
            for i, x in enumerate(v):
                self.scan_strings(f"{where}[{i}]", x, all_ids, key)
        elif isinstance(v, str) and key not in ("id", "displayName") and not key.endswith("Key"):
            prefix = v.split(".", 1)[0]
            if "." in v and " " not in v and prefix in self.ids and v not in all_ids:
                self.err(where, f"refers to unknown id '{v}'")


def main(argv):
    schema = load(SCHEMA)
    docs = {f: load(os.path.join(DATA, f)) for f in sorted(os.listdir(DATA)) if f.endswith(".json")}
    errors = Validator(schema, docs).run()
    for e in errors:
        print("ERROR", e)
    if "--format" in argv and not errors:
        for f, d in docs.items():
            with open(os.path.join(DATA, f), "w", encoding="utf-8") as fh:
                json.dump(d, fh, ensure_ascii=False, indent=2)
                fh.write("\n")
    total = sum(len(d.get("items", [])) for d in docs.values())
    print(f"{'FAIL' if errors else 'OK'}: {len(docs)} files, {total} items, {len(errors)} errors")
    return 1 if errors else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
