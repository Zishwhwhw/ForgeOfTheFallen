using FotF.Data;
using FotF.State;

namespace FotF.View.Combat
{
    /// <summary>WeaponDefinition (данные) → WeaponInstance (состояние забега).</summary>
    public static class WeaponFactory
    {
        public static WeaponInstance Create(WeaponDefinition d)
        {
            if (d == null) return null;
            var w = new WeaponInstance
            {
                DefinitionId = d.id, DisplayName = d.name.Replace("weapon_", "").Replace('_', ' '),
                Rarity = d.rarity, Family = d.family, AttackType = d.attackType,
                Damage = d.damage, CritChance = d.critChance, AttacksPerSecond = d.attacksPerSecond, ManaCost = d.manaCost,
                ProjectilesPerShot = d.projectilesPerShot, SpreadDegrees = d.spreadDegrees, ProjectileSpeed = d.projectileSpeed,
                ProjectileLifetime = d.projectileLifetime, StunSeconds = d.stunSeconds,
                MeleeRadius = d.meleeRadius, MeleeArcDegrees = d.meleeArcDegrees, CutsProjectiles = d.cutsProjectiles,
            };
            w.Elements.Add(d.element);
            w.StatusChance = d.statusChance;
            if (d.special != null && d.special.IsSet)
            {
                var s = new WeaponSpecial { Type = d.special.type };
                foreach (var e in d.special.values.entries)
                    if (e.kind == ValueMap.Kind.Number || e.kind == ValueMap.Kind.Bool) s.Values[e.key] = e.number;
                w.Specials.Add(s);
            }
            if (!string.IsNullOrEmpty(d.displayName)) w.DisplayName = d.displayName;
            return w;
        }
    }
}
