using System;
using FotF.Data;
using FotF.State;

namespace FotF.Logic.Fusion
{
    public enum Dominance { A, B, Middle }

    /// <summary>Числа сплава. В GDD заданы только шанс джекпота (2–3%) и порог −10%; веса — калибровать (вопрос владельцу).</summary>
    public readonly struct FusionRules
    {
        /// <summary>Доля статов доминирующего родителя при доминанте A или B (калибровать).</summary>
        public readonly float DominantWeight;
        /// <summary>Шанс идеального сплава (GDD §6.2: 2–3%).</summary>
        public readonly float JackpotChance;
        /// <summary>Гибрид не слабее лучшего родителя минус 10% (GDD п.152).</summary>
        public readonly float FloorOfBestParent;
        public FusionRules(float dominantWeight, float jackpotChance, float floorOfBestParent)
        { DominantWeight = dominantWeight; JackpotChance = jackpotChance; FloorOfBestParent = floorOfBestParent; }
        public static FusionRules Default => new FusionRules(0.7f, 0.025f, 0.9f);
    }

    public readonly struct FusionResult
    {
        public readonly WeaponInstance Weapon; public readonly Dominance Dominance; public readonly bool Jackpot;
        public FusionResult(WeaponInstance w, Dominance d, bool j) { Weapon = w; Dominance = d; Jackpot = j; }
    }

    /// <summary>
    /// Сплав двух оружий (GDD §6.2, Architecture §5). Детерминирован от IRandom.
    /// Читаемые правила (п.152): стихии обоих родителей переходят всегда; тип атаки — от более редкого;
    /// гибрид не слабее лучшего родителя −10%; случайна только доминанта (и джекпот).
    /// </summary>
    public static class FusionService
    {
        public static bool CanFuse(WeaponInstance a, WeaponInstance b) => a != null && b != null && !a.IsHybrid && !b.IsHybrid;

        public static FusionResult Fuse(WeaponInstance a, WeaponInstance b, IRandom rng, in FusionRules rules)
        {
            if (!CanFuse(a, b)) throw new InvalidOperationException("Пересплава нет: гибрид — финальная форма (GDD §6.2)");

            bool jackpot = rng.NextFloat() < rules.JackpotChance;
            var dominance = (Dominance)rng.NextInt(0, 3);
            float wa = dominance == Dominance.A ? rules.DominantWeight : dominance == Dominance.B ? 1f - rules.DominantWeight : 0.5f;

            // визуал и тип атаки — от более редкого; при равной редкости — от первого положенного (A)
            var rare = b.Rarity > a.Rarity ? b : a;
            var other = ReferenceEquals(rare, a) ? b : a;

            var h = new WeaponInstance
            {
                DefinitionId = rare.DefinitionId, DetailId = other.DefinitionId,
                DisplayName = $"{rare.DisplayName} + {other.DisplayName}",
                Rarity = Rarity.Franken, Family = rare.Family, AttackType = rare.AttackType,
                IsHybrid = true, IsJackpot = jackpot, ParentA = a.DefinitionId, ParentB = b.DefinitionId,
            };
            foreach (var e in a.Elements) if (!h.Elements.Contains(e)) h.Elements.Add(e);
            foreach (var e in b.Elements) if (!h.Elements.Contains(e)) h.Elements.Add(e);
            if (h.Elements.Count > 1) h.Elements.Remove(Element.Physical);

            if (jackpot)
            {
                // «100% статов обоих» — трактовка до решения владельца: урон и крит складываются, темп — лучший, мана — меньшая
                h.Damage = a.Damage + b.Damage;
                h.CritChance = Math.Min(1f, a.CritChance + b.CritChance);
                h.AttacksPerSecond = Math.Max(a.AttacksPerSecond, b.AttacksPerSecond);
                h.ManaCost = Math.Min(a.ManaCost, b.ManaCost);
            }
            else
            {
                h.Damage = (int)Math.Round(Mix(a.Damage, b.Damage, wa));
                h.CritChance = Mix(a.CritChance, b.CritChance, wa);
                h.AttacksPerSecond = Mix(a.AttacksPerSecond, b.AttacksPerSecond, wa);
                h.ManaCost = (int)Math.Round(Mix(a.ManaCost, b.ManaCost, wa));
            }
            // параметры формы атаки — от родителя, давшего тип атаки
            h.ProjectilesPerShot = rare.ProjectilesPerShot; h.SpreadDegrees = rare.SpreadDegrees;
            h.ProjectileSpeed = rare.ProjectileSpeed; h.ProjectileLifetime = rare.ProjectileLifetime;
            h.MeleeRadius = rare.MeleeRadius; h.MeleeArcDegrees = rare.MeleeArcDegrees;
            // способности обоих переходят (GDD §6.2: «смешиваются способности»)
            h.CutsProjectiles = a.CutsProjectiles || b.CutsProjectiles;
            h.StunSeconds = Math.Max(a.StunSeconds, b.StunSeconds);
            h.StatusChance = Math.Max(a.StatusChance, b.StatusChance);
            h.Specials = Combat.WeaponSpecials.Merge(rare, other, h.AttackType);
            if (h.AttackType == AttackType.Melee) h.ManaCost = 0;        // GDD §5.3: ближнее без снаряда — 0 маны

            // гибрид не слабее лучшего родителя −10%
            float floor = Math.Max(a.Power, b.Power) * rules.FloorOfBestParent;
            if (h.Power < floor && h.Power > 0f) h.Damage = (int)Math.Ceiling(h.Damage * floor / h.Power);
            return new FusionResult(h, dominance, jackpot);
        }

        static float Mix(float a, float b, float wa) => a * wa + b * (1f - wa);
    }
}
