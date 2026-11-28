using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using FotF.Data;
using FotF.EditorTools.Data;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace FotF.Tests.Data
{
    /// <summary>
    /// ARCHITECTURE §8.3: схема ↔ классы SO, каждый id из JSON имеет SO, значения SO совпадают с JSON
    /// (то есть импорт «FotF/Данные/Импорт баланса» прогнан после правки JSON).
    /// </summary>
    public class BalanceDataTests
    {
        static JObject Schema => BalanceImporter.LoadSchema();

        static JObject LoadFile(JObject kindDef) =>
            JObject.Parse(File.ReadAllText(Path.Combine(BalanceImporter.BalanceDir, (string)kindDef["file"])));

        [Test]
        public void SchemaEnums_MatchCSharpEnums()
        {
            foreach (var p in ((JObject)Schema["enums"]).Properties())
            {
                var t = typeof(WeaponDefinition).Assembly.GetType("FotF.Data." + p.Name) ?? typeof(Rarity).Assembly.GetType("FotF.Data." + p.Name);
                Assert.IsNotNull(t, $"нет enum FotF.Data.{p.Name}");
                CollectionAssert.AreEqual(p.Value.Select(v => (string)v).ToArray(), Enum.GetNames(t), p.Name);
            }
        }

        [Test]
        public void EverySchemaField_ExistsInSoClass_WithMatchingType()
        {
            var problems = new StringBuilder();
            foreach (var kind in ((JObject)Schema["kinds"]).Properties())
            {
                var def = (JObject)kind.Value;
                var type = BalanceImporter.SoType((string)def["so"]);
                if (type == null) { problems.AppendLine($"{kind.Name}: нет класса {def["so"]}"); continue; }
                foreach (var f in ((JObject)def["fields"]).Properties())
                {
                    var fi = type.GetField(f.Name, BindingFlags.Public | BindingFlags.Instance);
                    if (fi == null) { problems.AppendLine($"{type.Name}.{f.Name}: нет поля"); continue; }
                    if (!TypeMatches((JObject)f.Value, fi.FieldType)) problems.AppendLine($"{type.Name}.{f.Name}: тип {fi.FieldType.Name} не подходит к {f.Value["type"]}");
                }
            }
            Assert.IsEmpty(problems.ToString(), problems.ToString());
        }

        static bool TypeMatches(JObject f, Type t)
        {
            switch ((string)f["type"])
            {
                case "id": case "string": return t == typeof(string);
                case "int": return t == typeof(int);
                case "number": return t == typeof(float);
                case "bool": return t == typeof(bool);
                case "enum": return t.IsEnum && t.Name == (string)f["enum"];
                case "ref": return typeof(ScriptableObject).IsAssignableFrom(t);
                case "list": return t.IsArray;
                case "map": return t == typeof(ValueMap) || (t.IsClass && !t.IsArray && t != typeof(string));
                case "object": return t.IsClass && !t.IsArray && t != typeof(string);
                default: return false;
            }
        }

        [Test]
        public void EveryJsonId_HasSoAsset()
        {
            var missing = new StringBuilder();
            foreach (var kind in ((JObject)Schema["kinds"]).Properties())
            {
                var def = (JObject)kind.Value;
                var index = BalanceImporter.IndexById(BalanceImporter.SoType((string)def["so"]));
                foreach (var item in (JArray)LoadFile(def)["items"])
                    if (!index.ContainsKey((string)item["id"])) missing.AppendLine($"{item["id"]} ({def["so"]})");
            }
            Assert.IsEmpty(missing.ToString(), "нет SO — запустите FotF/Данные/Импорт баланса:\n" + missing);
        }

        [Test]
        public void SoValues_MatchJson()
        {
            var diffs = new StringBuilder();
            foreach (var kind in ((JObject)Schema["kinds"]).Properties())
            {
                var def = (JObject)kind.Value;
                var index = BalanceImporter.IndexById(BalanceImporter.SoType((string)def["so"]));
                foreach (JObject item in (JArray)LoadFile(def)["items"])
                    if (index.TryGetValue((string)item["id"], out var so))
                        Compare(BalanceImporter.StripComments(item), so, (string)item["id"], diffs);
            }
            Assert.IsEmpty(diffs.ToString(), "SO расходятся с JSON — запустите FotF/Данные/Импорт баланса:\n" + diffs);
        }

        /// <summary>null в JSON = «калибровать»/«нет» — не сравнивается.</summary>
        static void Compare(JToken j, object value, string path, StringBuilder diffs)
        {
            if (j.Type == JTokenType.Null) return;
            switch (value)
            {
                case ScriptableObject so when j.Type == JTokenType.String:
                    if (BalanceImporter.IdOf(so) != (string)j) diffs.AppendLine($"{path}: ссылка {BalanceImporter.IdOf(so)} ≠ {j}");
                    return;
                case null:
                    diffs.AppendLine($"{path}: в SO пусто, в JSON {j.ToString(Newtonsoft.Json.Formatting.None)}");
                    return;
                case ValueMap map:
                    foreach (var p in ((JObject)j).Properties())
                    {
                        var want = BalanceImporter.ValueMapConverter.ToEntry(p.Name, p.Value);
                        var got = map.Find(p.Name);
                        if (got == null || got.kind != want.kind || Math.Abs(got.number - want.number) > 1e-4f || (got.text ?? "") != (want.text ?? ""))
                            diffs.AppendLine($"{path}.{p.Name}: {p.Value} не совпадает");
                    }
                    if (map.entries.Length != ((JObject)j).Count) diffs.AppendLine($"{path}: лишние ключи в SO");
                    return;
            }
            var t = value.GetType();
            if (j is JObject o)
            {
                foreach (var p in o.Properties())
                {
                    var fi = t.GetField(p.Name, BindingFlags.Public | BindingFlags.Instance);
                    if (fi == null) { diffs.AppendLine($"{path}.{p.Name}: нет поля в {t.Name}"); continue; }
                    Compare(p.Value, fi.GetValue(value), path + "." + p.Name, diffs);
                }
            }
            else if (j is JArray a)
            {
                var list = ((IEnumerable)value).Cast<object>().ToList();
                if (list.Count != a.Count) { diffs.AppendLine($"{path}: {list.Count} элементов ≠ {a.Count}"); return; }
                for (int i = 0; i < a.Count; i++) Compare(a[i], list[i], $"{path}[{i}]", diffs);
            }
            else if (t.IsEnum) { if (value.ToString() != (string)j) diffs.AppendLine($"{path}: {value} ≠ {j}"); }
            else if (value is float f) { if (Math.Abs(f - (float)j) > 1e-4f) diffs.AppendLine($"{path}: {f} ≠ {j}"); }
            else if (value is int n) { if (n != (int)j) diffs.AppendLine($"{path}: {n} ≠ {j}"); }
            else if (value is bool b) { if (b != (bool)j) diffs.AppendLine($"{path}: {b} ≠ {j}"); }
            else if (value is string s) { if (s != (string)j) diffs.AppendLine($"{path}: «{s}» ≠ «{j}»"); }
        }
    }
}
