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
        public HitInfo(int amount, bool isCrit, Vector2 direction, float stunSeconds, Team source)
        { Amount = amount; IsCrit = isCrit; Direction = direction; StunSeconds = stunSeconds; Source = source; }
    }

    public interface IDamageable
    {
        Team Team { get; }
        void TakeHit(in HitInfo hit);
    }
}
