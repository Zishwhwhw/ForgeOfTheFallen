using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FotF.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace FotF.EditorTools.Data
{
    /// <summary>
    /// Импорт баланса JSON → ScriptableObject (ARCHITECTURE §8.1). Источник истины — Assets/_Project/Data/Balance/*.json.
    /// SO находится по полю id; нет такого — создаётся Data/&lt;Папка&gt;/&lt;id с _&gt;.asset (арт ставится в Inspector).
    /// null в JSON = «калибровать»: у чисел остаётся значение SO; у объектов/списков (special, parents) — «нет».
    /// Поля с '_' — комментарии. Ссылки между сущностями — по id. Спрайты и префабы не трогаются.
    /// </summary>
    public static class BalanceImporter
    {
        public const string BalanceDir = "Assets/_Project/Data/Balance";
        const string DataDir = "Assets/_Project/Data";

        /// <summary>Папка новых ассетов по виду (существующие ассеты находятся по id где угодно в Data).</summary>
        static readonly Dictionary<string, string> Folders = new Dictionary<string, string>
        {
            ["weapon"] = "Weapons", ["hero"] = "Heroes", ["enemy"] = "Enemies", ["boss"] = "Bosses",
            ["passive"] = "Passives", ["status"] = "Statuses", ["curse"] = "Curses", ["item"] = "Items",
            ["constants"] = "Constants", ["fusion"] = "Fusion", ["encounter"] = "Rooms", ["gamefeel"] = "GameFeel",
        };

        public sealed class Report
        {
            public int Created, Updated;
            public readonly List<string> Errors = new List<string>();
            public readonly List<string> Warnings = new List<string>();
            public override string ToString() =>
                $"Импорт баланса: обновлено {Updated}, создано {Created}, ошибок {Errors.Count}, предупреждений {Warnings.Count}";
        }

        public static string SchemaPath => Path.Combine(Directory.GetParent(Application.dataPath)!.FullName, "Tools", "data", "balance_schema.json");

        [MenuItem("FotF/Данные/Импорт баланса")]
        static void ImportMenu() => Log(Import());

        public static void Log(Report r)
        {
            foreach (var w in r.Warnings) Debug.LogWarning("[Баланс] " + w);
            foreach (var e in r.Errors) Debug.LogError("[Баланс] " + e);
            if (r.Errors.Count == 0) Debug.Log("[Баланс] " + r);
        }

        /// <summary>Тип SO по имени из схемы ("so").</summary>
        public static Type SoType(string soName) => typeof(WeaponDefinition).Assembly.GetType("FotF.Data." + soName);

        public static JObject LoadSchema() => JObject.Parse(File.ReadAllText(SchemaPath));

        /// <summary>Все SO баланса по id (ключ — id, только ассеты в Data).</summary>
        public static Dictionary<string, ScriptableObject> IndexById(Type type)
        {
            var map = new Dictionary<string, ScriptableObject>();
            foreach (var guid in AssetDatabase.FindAssets("t:" + type.Name, new[] { DataDir }))
            {
                var so = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), type) as ScriptableObject;
                var id = so ? IdOf(so) : null;
                if (!string.IsNullOrEmpty(id) && !map.ContainsKey(id)) map[id] = so;
            }
            return map;
        }

        public static string IdOf(ScriptableObject so) => so.GetType().GetField("id")?.GetValue(so) as string;

        public static Report Import()
        {
            var report = new Report();
            JObject schema;
            try { schema = LoadSchema(); }
            catch (Exception e) { report.Errors.Add($"схема {SchemaPath}: {e.Message}"); return report; }

            // Проход 1: найти или создать SO для каждого id всех видов (чтобы ссылки разрешались в любом порядке).
            var byId = new Dictionary<string, ScriptableObject>();
            var items = new List<(string kind, JObject item, ScriptableObject so)>();
            foreach (var kindProp in ((JObject)schema["kinds"]).Properties())
            {
                string kind = kindProp.Name;
                var def = (JObject)kindProp.Value;
                var type = SoType((string)def["so"]);
                if (type == null) { report.Errors.Add($"{kind}: нет класса SO FotF.Data.{def["so"]}"); continue; }
                string file = Path.Combine(BalanceDir, (string)def["file"]);
                if (!File.Exists(file)) { report.Errors.Add($"{kind}: нет файла {file}"); continue; }

                JObject root;
                try { root = JObject.Parse(File.ReadAllText(file)); }
                catch (Exception e) { report.Errors.Add($"{file}: {e.Message}"); continue; }

                var existing = IndexById(type);
                var seen = new HashSet<string>();
                foreach (JObject item in (JArray)root["items"])
                {
                    string id = (string)item["id"];
                    if (string.IsNullOrEmpty(id)) { report.Errors.Add($"{file}: запись без id"); continue; }
                    seen.Add(id);
                    if (!existing.TryGetValue(id, out var so)) so = AdoptUnnamed(type) ?? Create(type, kind, id, report);
                    if (so == null) continue;
                    byId[id] = so;
                    items.Add((kind, item, so));
                }
                foreach (var id in existing.Keys.Where(k => !seen.Contains(k)))
                    report.Warnings.Add($"{kind}: SO с id {id} есть, а в {Path.GetFileName(file)} его нет (SO не удалён)");
            }

            // Проход 2: заполнить поля.
            var serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                MissingMemberHandling = MissingMemberHandling.Error,
                Converters = { new StringEnumConverter(), new ValueMapConverter(), new IdRefConverter(byId) },
            });
            foreach (var (kind, item, so) in items)
            {
                try
                {
                    var clean = (JObject)StripComments(item);
                    ResetNulledObjects(clean, so);
                    using (var reader = clean.CreateReader()) serializer.Populate(reader, so);
                    EditorUtility.SetDirty(so);
                    report.Updated++;
                }
                catch (Exception e) { report.Errors.Add($"{kind} {item["id"]}: {e.Message}"); }
            }

            AddWeaponsToCatalogs(byId.Values.OfType<WeaponDefinition>(), report);
            AssetDatabase.SaveAssets();
            return report;
        }

        /// <summary>Одиночный SO без id (например, старый fusion_rules) получает id из JSON, а не дублируется.</summary>
        static ScriptableObject AdoptUnnamed(Type type)
        {
            foreach (var guid in AssetDatabase.FindAssets("t:" + type.Name, new[] { DataDir }))
            {
                var so = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), type) as ScriptableObject;
                if (so && string.IsNullOrEmpty(IdOf(so))) return so;
            }
            return null;
        }

        static ScriptableObject Create(Type type, string kind, string id, Report report)
        {
            string folder = DataDir + "/" + (Folders.TryGetValue(kind, out var f) ? f : kind);
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder(DataDir, Path.GetFileName(folder));
            var so = ScriptableObject.CreateInstance(type);
            type.GetField("id")?.SetValue(so, id);
            string path = $"{folder}/{id.Replace('.', '_')}.asset";
            if (File.Exists(path)) { report.Errors.Add($"{id}: файл {path} уже есть, но его id другой"); return null; }
            AssetDatabase.CreateAsset(so, path);
            report.Created++;
            return so;
        }

        /// <summary>Убрать комментарии дизайнера: поля, начинающиеся с '_', на любой глубине.</summary>
        public static JToken StripComments(JToken t)
        {
            switch (t)
            {
                case JObject o:
                    var r = new JObject();
                    foreach (var p in o.Properties()) if (!p.Name.StartsWith("_")) r.Add(p.Name, StripComments(p.Value));
                    return r;
                case JArray a: return new JArray(a.Select(StripComments));
                default: return t.DeepClone();
            }
        }

        /// <summary>
        /// null у объекта/списка (special, parents, minions…) значит «нет», а не «калибровать»: сбрасываем в пустое.
        /// У чисел null — «калибровать», значение SO остаётся (NullValueHandling.Ignore).
        /// </summary>
        static void ResetNulledObjects(JObject item, ScriptableObject so)
        {
            foreach (var p in item.Properties())
            {
                if (p.Value.Type != JTokenType.Null) continue;
                var f = so.GetType().GetField(p.Name, BindingFlags.Public | BindingFlags.Instance);
                if (f == null || f.FieldType.IsValueType || f.FieldType == typeof(string)) continue;
                if (f.FieldType.IsArray) f.SetValue(so, Array.CreateInstance(f.FieldType.GetElementType()!, 0));
                else if (typeof(UnityEngine.Object).IsAssignableFrom(f.FieldType)) f.SetValue(so, null);
                else f.SetValue(so, Activator.CreateInstance(f.FieldType));
            }
        }

        static void AddWeaponsToCatalogs(IEnumerable<WeaponDefinition> weapons, Report report)
        {
            var all = weapons.ToList();
            foreach (var guid in AssetDatabase.FindAssets("t:WeaponCatalog", new[] { DataDir }))
            {
                var cat = AssetDatabase.LoadAssetAtPath<WeaponCatalog>(AssetDatabase.GUIDToAssetPath(guid));
                var missing = all.Where(w => !cat.weapons.Contains(w)).ToArray();
                if (missing.Length == 0) continue;
                cat.weapons = cat.weapons.Where(w => w != null).Concat(missing).ToArray();
                EditorUtility.SetDirty(cat);
            }
        }

        /// <summary>Ссылка на сущность по id ("weapon.eternal_sword" → WeaponDefinition).</summary>
        sealed class IdRefConverter : JsonConverter
        {
            readonly Dictionary<string, ScriptableObject> _byId;
            public IdRefConverter(Dictionary<string, ScriptableObject> byId) { _byId = byId; }
            public override bool CanWrite => false;
            public override bool CanConvert(Type t) => typeof(ScriptableObject).IsAssignableFrom(t);

            public override object ReadJson(JsonReader reader, Type type, object existing, JsonSerializer s)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                if (reader.TokenType != JsonToken.String) throw new JsonSerializationException($"ссылка на {type.Name} должна быть строкой-id");
                string id = (string)reader.Value;
                if (!_byId.TryGetValue(id, out var so)) throw new JsonSerializationException($"нет сущности с id {id}");
                if (!type.IsInstanceOfType(so)) throw new JsonSerializationException($"{id} — это {so.GetType().Name}, нужен {type.Name}");
                return so;
            }

            public override void WriteJson(JsonWriter w, object v, JsonSerializer s) => throw new NotSupportedException();
        }

        /// <summary>Поле-«map» схемы → ValueMap (скаляры; вложенное сохраняется текстом JSON).</summary>
        public sealed class ValueMapConverter : JsonConverter
        {
            public override bool CanWrite => false;
            public override bool CanConvert(Type t) => t == typeof(ValueMap);

            public override object ReadJson(JsonReader reader, Type type, object existing, JsonSerializer s)
            {
                var map = new ValueMap();
                if (reader.TokenType == JsonToken.Null) return map;
                var o = JObject.Load(reader);
                map.entries = o.Properties().Select(p => ToEntry(p.Name, p.Value)).ToArray();
                return map;
            }

            public static ValueMap.Entry ToEntry(string key, JToken v)
            {
                var e = new ValueMap.Entry { key = key };
                switch (v.Type)
                {
                    case JTokenType.Integer:
                    case JTokenType.Float: e.kind = ValueMap.Kind.Number; e.number = (float)v; break;
                    case JTokenType.Boolean: e.kind = ValueMap.Kind.Bool; e.number = (bool)v ? 1f : 0f; break;
                    case JTokenType.String: e.kind = ValueMap.Kind.Text; e.text = (string)v; break;
                    case JTokenType.Null: e.kind = ValueMap.Kind.Null; break;
                    default: e.kind = ValueMap.Kind.Text; e.text = v.ToString(Formatting.None); break;
                }
                return e;
            }

            public override void WriteJson(JsonWriter w, object v, JsonSerializer s) => throw new NotSupportedException();
        }
    }

    /// <summary>Авто-импорт при изменении JSON баланса (ARCHITECTURE §8.1).</summary>
    sealed class BalanceJsonPostprocessor : AssetPostprocessor
    {
        static bool _queued;

        static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            if (_queued || !imported.Any(p => p.StartsWith(BalanceImporter.BalanceDir) && p.EndsWith(".json"))) return;
            _queued = true;
            EditorApplication.delayCall += () =>
            {
                _queued = false;
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;
                BalanceImporter.Log(BalanceImporter.Import());
            };
        }
    }
}
