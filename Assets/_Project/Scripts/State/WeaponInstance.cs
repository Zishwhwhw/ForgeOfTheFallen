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

        public bool IsHybrid;              // GDD §6.2: пересплава нет
        public bool IsJackpot;             // идеальный сплав
        public string ParentA, ParentB;    // для карточки «родители: A + B» (GDD §5.1)

        /// <summary>Мощность для правила «не слабее лучшего родителя −10%» (урон в секунду с учётом крита x2).</summary>
        public float Power => Damage * System.Math.Max(1, ProjectilesPerShot) * AttacksPerSecond * (1f + CritChance);

        public WeaponInstance Clone()
        {
            var c = (WeaponInstance)MemberwiseClone();
            c.Elements = new List<Element>(Elements);
            return c;
        }
    }
}
