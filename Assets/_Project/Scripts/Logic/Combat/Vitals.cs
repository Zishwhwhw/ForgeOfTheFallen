using System;
using FotF.State;

namespace FotF.Logic.Combat
{
    /// <summary>Параметры живучести (переводятся из HeroDefinition во View).</summary>
    public readonly struct VitalsConfig
    {
        public readonly int MaxHp, MaxArmor, MaxMana;
        public readonly float ArmorRegenDelay, ArmorRegenPerSecond;
        public VitalsConfig(int maxHp, int maxArmor, int maxMana, float armorRegenDelay, float armorRegenPerSecond)
        { MaxHp = maxHp; MaxArmor = maxArmor; MaxMana = maxMana; ArmorRegenDelay = armorRegenDelay; ArmorRegenPerSecond = armorRegenPerSecond; }
    }

    public readonly struct DamageResult
    {
        public readonly int ToArmor, ToHp;
        public readonly bool Blocked, ArmorBroken, Killed;
        public DamageResult(int toArmor, int toHp, bool blocked, bool armorBroken, bool killed)
        { ToArmor = toArmor; ToHp = toHp; Blocked = blocked; ArmorBroken = armorBroken; Killed = killed; }
    }

    /// <summary>
    /// HP, броня, мана героя (GDD §4.1).
    /// Броня — щит-полоса: пока она есть, удар идёт только в неё; удар больше остатка ломает броню, излишек в HP не проходит.
    /// Броня восстанавливается после ArmorRegenDelay секунд без урона. HP не регенерирует.
    /// </summary>
    public static class Vitals
    {
        public static void Init(HeroState s, in VitalsConfig c)
        {
            s.MaxHp = s.Hp = c.MaxHp;
            s.MaxArmor = s.Armor = c.MaxArmor;
            s.MaxMana = s.Mana = c.MaxMana;
            s.ArmorRegenDelayLeft = 0f;
            s.ArmorRegenAccumulator = 0f;
        }

        public static DamageResult ApplyDamage(HeroState s, int amount, in VitalsConfig c)
        {
            if (amount <= 0 || s.IsDead) return default;
            if (s.IsInvulnerable) return new DamageResult(0, 0, true, false, false);
            s.ArmorRegenDelayLeft = c.ArmorRegenDelay;
            s.ArmorRegenAccumulator = 0f;
            if (s.Armor > 0)
            {
                int toArmor = Math.Min(s.Armor, amount);
                bool broken = amount >= s.Armor;
                s.Armor -= toArmor;
                return new DamageResult(toArmor, 0, false, broken, false);
            }
            int toHp = Math.Min(s.Hp, amount);
            s.Hp -= toHp;
            return new DamageResult(0, toHp, false, false, s.Hp <= 0);
        }

        public static void Tick(HeroState s, in VitalsConfig c, float dt)
        {
            if (s.IsDead || dt <= 0f) return;
            if (s.ArmorRegenDelayLeft > 0f)
            {
                s.ArmorRegenDelayLeft -= dt;
                if (s.ArmorRegenDelayLeft > 0f) return;
                dt = -s.ArmorRegenDelayLeft;          // остаток кадра уже идёт на восстановление
                s.ArmorRegenDelayLeft = 0f;
            }
            if (s.Armor >= s.MaxArmor) { s.ArmorRegenAccumulator = 0f; return; }
            s.ArmorRegenAccumulator += c.ArmorRegenPerSecond * dt;
            int whole = (int)s.ArmorRegenAccumulator;
            if (whole <= 0) return;
            s.ArmorRegenAccumulator -= whole;
            s.Armor = Math.Min(s.MaxArmor, s.Armor + whole);
        }

        public static bool TrySpendMana(HeroState s, int cost)
        {
            if (cost <= 0) return true;
            if (s.Mana < cost) return false;
            s.Mana -= cost;
            return true;
        }

        public static void AddMana(HeroState s, int amount) => s.Mana = Math.Min(s.MaxMana, s.Mana + Math.Max(0, amount));

        /// <summary>Лечение зельем/сердцем: HP не выше максимума, мёртвого не лечит.</summary>
        public static void Heal(HeroState s, int amount)
        {
            if (s.Hp <= 0) return;
            s.Hp = Math.Min(s.MaxHp, s.Hp + Math.Max(0, amount));
        }
    }
}
