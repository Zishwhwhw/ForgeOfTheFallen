using System.Collections.Generic;
using FotF.Data;

namespace FotF.State
{
    /// <summary>
    /// Экземпляр оружия в забеге (Architecture §5): обычное — копия статов WeaponDefinition, гибрид — результат сплава.
    /// Все атаки читают статы отсюда, а не из ScriptableObject.
    /// </summary>
    public sealed class WeaponInstance
    {
        public string DefinitionId;        // для обычного — id определения; для гибрида — id визуальной базы
        public string DetailId;            // гибрид: id второго родителя (сменная деталь вида, GDD §6.4)
        public string DisplayName;
        public Rarity Rarity;
        public WeaponFamily Family;
        public AttackType AttackType;
        public List<Element> Elements = new List<Element>();

        public int Damage;
        public float CritChance;
        public float AttacksPerSecond;
        public int ManaCost;

        public int ProjectilesPerShot = 1;
        public float SpreadDegrees, ProjectileSpeed, ProjectileLifetime, StunSeconds;
        public float MeleeRadius, MeleeArcDegrees;
        public bool CutsProjectiles;
        /// <summary>Шанс наложить статус своей стихии (GDD §5.4).</summary>
        public float StatusChance;
        /// <summary>Особенности оружия (перегрев, натяжение, очередь…), у гибрида — от обоих родителей.</summary>
        public List<WeaponSpecial> Specials = new List<WeaponSpecial>();
        /// <summary>Перегрев «Тарахтелки»: 0..1, при 1 — стрелять нельзя до остывания.</summary>
        public float Heat;
        public bool Overheated;

        public WeaponSpecial Special(string type)
        {
            foreach (var s in Specials) if (s.Type == type) return s;
            return null;
        }

        public bool IsHybrid;              // GDD §6.2: пересплава нет
        public bool IsJackpot;             // идеальный сплав
        public string ParentA, ParentB;    // для карточки «родители: A + B» (GDD §5.1)

        /// <summary>Мощность для правила «не слабее лучшего родителя −10%» (урон в секунду с учётом крита x2).</summary>
        public float Power => Damage * System.Math.Max(1, ProjectilesPerShot) * AttacksPerSecond * (1f + CritChance);

        public WeaponInstance Clone()
        {
            var c = (WeaponInstance)MemberwiseClone();
            c.Elements = new List<Element>(Elements);
            c.Specials = new List<WeaponSpecial>(Specials);
            return c;
        }
    }

    /// <summary>Особенность оружия: тип + числа (из WeaponDefinition.special).</summary>
    public sealed class WeaponSpecial
    {
        public string Type;
        public Dictionary<string, float> Values = new Dictionary<string, float>();
        public float Get(string key, float fallback) => Values.TryGetValue(key, out var v) ? v : fallback;
    }
}
