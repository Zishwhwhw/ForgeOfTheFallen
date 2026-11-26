using FotF.State;

namespace FotF.Logic.Combat
{
    public readonly struct WeaponStats
    {
        public readonly int Damage, ManaCost;
        public readonly float AttacksPerSecond, CritChance;
        public WeaponStats(int damage, float attacksPerSecond, int manaCost, float critChance)
        { Damage = damage; AttacksPerSecond = attacksPerSecond; ManaCost = manaCost; CritChance = critChance; }
    }

    public readonly struct HitRoll
    {
        public readonly int Amount; public readonly bool IsCrit;
        public HitRoll(int amount, bool isCrit) { Amount = amount; IsCrit = isCrit; }
    }

    /// <summary>Атака оружием: перезарядка, мана (GDD §5.3), крит x2 (GDD §4.1).</summary>
    public static class WeaponAttack
    {
        public const float CritMultiplier = 2f;

        public enum Result { Fired, Cooldown, NoMana, NoWeapon }

        public static Result TryAttack(HeroState hero, WeaponSlotState slot, in WeaponStats w)
        {
            if (slot == null || slot.IsEmpty || hero.IsDead) return Result.NoWeapon;
            if (slot.CooldownLeft > 0f) return Result.Cooldown;
            if (!Vitals.TrySpendMana(hero, w.ManaCost)) return Result.NoMana;
            slot.CooldownLeft = w.AttacksPerSecond > 0f ? 1f / w.AttacksPerSecond : 0f;
            return Result.Fired;
        }

        public static void Tick(WeaponSlotState slot, float dt)
        {
            if (slot.CooldownLeft > 0f) slot.CooldownLeft = slot.CooldownLeft > dt ? slot.CooldownLeft - dt : 0f;
        }

        public static HitRoll RollDamage(int baseDamage, float critChance, IRandom rng)
        {
            bool crit = critChance > 0f && rng.NextFloat() < critChance;
            return new HitRoll(crit ? (int)(baseDamage * CritMultiplier) : baseDamage, crit);
        }

        public static void Swap(HeroState hero)
        {
            int other = 1 - hero.ActiveSlot;
            if (!hero.Weapons[other].IsEmpty) hero.ActiveSlot = other;
        }
    }
}
