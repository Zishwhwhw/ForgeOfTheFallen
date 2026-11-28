using FotF.Data;
using UnityEngine;

namespace FotF.View.Combat
{
    public enum Team { Player, Enemy }

    /// <summary>Попадание, которое View передаёт цели. Число урона уже посчитано в Logic.</summary>
    public readonly struct HitInfo
    {
        public readonly int Amount;
        public readonly bool IsCrit;
        public readonly Vector2 Direction;
        public readonly float StunSeconds;
        public readonly Team Source;
        /// <summary>Стихии удара: цель накладывает их статусы с шансом StatusChance (GDD §5.4, §7.3).</summary>
        public readonly Element[] Elements;
        public readonly float StatusChance;
        /// <summary>Скорость отбрасывания; 0 — обычная у цели.</summary>
        public readonly float Knockback;
        /// <summary>Удар снарядом (не ближний и не взрыв).</summary>
        public readonly bool FromProjectile;

        public HitInfo(int amount, bool isCrit, Vector2 direction, float stunSeconds, Team source)
            : this(amount, isCrit, direction, stunSeconds, source, null, 0f, 0f) { }

        public HitInfo(int amount, bool isCrit, Vector2 direction, float stunSeconds, Team source, Element[] elements, float statusChance, float knockback, bool fromProjectile = false)
        {
            Amount = amount; IsCrit = isCrit; Direction = direction; StunSeconds = stunSeconds; Source = source;
            Elements = elements; StatusChance = statusChance; Knockback = knockback; FromProjectile = fromProjectile;
        }

        public HitInfo WithDirection(Vector2 dir) => new HitInfo(Amount, IsCrit, dir, StunSeconds, Source, Elements, StatusChance, Knockback, FromProjectile);
        public HitInfo WithKnockback(float k) => new HitInfo(Amount, IsCrit, Direction, StunSeconds, Source, Elements, StatusChance, k, FromProjectile);
        public HitInfo AsProjectile(bool yes) => new HitInfo(Amount, IsCrit, Direction, StunSeconds, Source, Elements, StatusChance, Knockback, yes);
        public HitInfo WithAmount(int amount) => new HitInfo(amount, IsCrit, Direction, StunSeconds, Source, Elements, StatusChance, Knockback, FromProjectile);
    }

    public interface IDamageable
    {
        Team Team { get; }
        void TakeHit(in HitInfo hit);
    }
}
