using System;
using UnityEngine;

namespace FotF.Data
{
    /// <summary>
    /// Свободный набор «ключ → значение» из баланса (поля-«map» схемы: special.values, passive.values,
    /// цены, золото…). Unity не сериализует словари, поэтому — массив записей. Заполняется импортёром баланса.
    /// </summary>
    [Serializable]
    public sealed class ValueMap
    {
        public enum Kind { Number, Text, Bool, Null }

        [Serializable]
        public sealed class Entry
        {
            public string key;
            public Kind kind;
            public float number;
            [Tooltip("Строка или id сущности (например, enemy.skeleton_swordsman)")]
            public string text;
        }

        public Entry[] entries = Array.Empty<Entry>();

        public Entry Find(string key)
        {
            foreach (var e in entries) if (e != null && e.key == key) return e;
            return null;
        }

        public bool Has(string key) => Find(key) != null;

        public float Number(string key, float fallback = 0f)
        {
            var e = Find(key);
            return e != null && (e.kind == Kind.Number || e.kind == Kind.Bool) ? e.number : fallback;
        }

        public int Int(string key, int fallback = 0) => Mathf.RoundToInt(Number(key, fallback));

        public bool Bool(string key, bool fallback = false)
        {
            var e = Find(key);
            return e != null && e.kind == Kind.Bool ? e.number != 0f : fallback;
        }

        public string Text(string key, string fallback = null)
        {
            var e = Find(key);
            return e != null && e.kind == Kind.Text ? e.text : fallback;
        }
    }

    /// <summary>Нестандартная механика (перегрев, натяжение, деление слизня…): тип + параметры (ARCHITECTURE §8.2).</summary>
    [Serializable]
    public sealed class SpecialMechanic
    {
        [Tooltip("Пусто — механики нет")]
        public string type;
        public ValueMap values = new ValueMap();

        public bool IsSet => !string.IsNullOrEmpty(type);
    }

    /// <summary>Эффект пассивки/проклятия: имя эффекта + параметры.</summary>
    [Serializable]
    public sealed class EffectSpec
    {
        public string effect;
        public ValueMap values = new ValueMap();
    }
}
