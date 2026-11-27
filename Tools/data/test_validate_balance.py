"""Tests for validate_balance.py: run `python3 -m unittest Tools/data/test_validate_balance.py`."""
import copy
import os
import sys
import unittest

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import validate_balance as vb  # noqa: E402


class BalanceValidatorTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.schema = vb.load(vb.SCHEMA)
        cls.docs = {f: vb.load(os.path.join(vb.DATA, f)) for f in os.listdir(vb.DATA) if f.endswith(".json")}

    def errors(self, mutate):
        docs = copy.deepcopy(self.docs)
        mutate(docs)
        return vb.Validator(self.schema, docs).run()

    def weapon(self, docs, wid="weapon.nailgun"):
        return next(w for w in docs["weapons.json"]["items"] if w["id"] == wid)

    def test_repo_data_is_valid(self):
        self.assertEqual(self.errors(lambda d: None), [])

    def test_typo_in_field_name(self):
        errs = self.errors(lambda d: self.weapon(d).update({"damadge": 9}))
        self.assertTrue(any("unknown field 'damadge'" in e for e in errs), errs)

    def test_comment_fields_are_ignored(self):
        self.assertEqual(self.errors(lambda d: self.weapon(d).update({"_why": "test"})), [])

    def test_wrong_enum(self):
        errs = self.errors(lambda d: self.weapon(d).update({"rarity": "Mythic"}))
        self.assertTrue(any("'Mythic' not in Rarity" in e for e in errs), errs)

    def test_out_of_range(self):
        errs = self.errors(lambda d: self.weapon(d).update({"critChance": 1.5}))
        self.assertTrue(any("> max 1" in e for e in errs), errs)

    def test_null_only_where_calibrate(self):
        self.assertEqual(self.errors(lambda d: self.weapon(d).update({"attacksPerSecond": None})), [])
        errs = self.errors(lambda d: self.weapon(d).update({"damage": None}))
        self.assertTrue(any("null is not allowed" in e for e in errs), errs)

    def test_dangling_ref(self):
        def m(d):
            d["heroes.json"]["items"][0]["startWeapon"] = "weapon.laser_sword"
        errs = self.errors(m)
        self.assertTrue(any("unknown weapon id 'weapon.laser_sword'" in e for e in errs), errs)

    def test_dangling_id_inside_free_values(self):
        def m(d):
            cap = next(e for e in d["enemies.json"]["items"] if e["id"] == "enemy.captain_of_the_guard")
            cap["special"]["values"]["summon"] = "enemy.ghost"
        errs = self.errors(m)
        self.assertTrue(any("unknown id 'enemy.ghost'" in e for e in errs), errs)

    def test_duplicate_and_bad_prefix(self):
        def m(d):
            items = d["weapons.json"]["items"]
            items.append(copy.deepcopy(items[0]))
            items.append(dict(copy.deepcopy(items[1]), id="sword.bad"))
        errs = self.errors(m)
        self.assertTrue(any("duplicate id" in e for e in errs), errs)
        self.assertTrue(any("starting with 'weapon.'" in e for e in errs), errs)

    def test_missing_field_and_missing_file(self):
        errs = self.errors(lambda d: self.weapon(d).pop("holdType"))
        self.assertTrue(any("missing field 'holdType'" in e for e in errs), errs)
        errs = self.errors(lambda d: d.pop("bosses.json"))
        self.assertTrue(any("bosses.json: file is missing" in e for e in errs), errs)

    def test_new_file_without_schema_is_reported(self):
        errs = self.errors(lambda d: d.update({"inventory.json": {"kind": "inventory", "version": 1, "items": []}}))
        self.assertTrue(any("inventory.json: file has no kind" in e for e in errs), errs)


if __name__ == "__main__":
    unittest.main()
