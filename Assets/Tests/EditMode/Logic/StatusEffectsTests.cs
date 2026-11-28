using FotF.Data;
using FotF.Logic;
using FotF.Logic.Status;
using FotF.State;
using NUnit.Framework;

namespace FotF.Tests.Logic
{
    /// <summary>Статусы GDD §7.3 (числа — черновик из statuses.json).</summary>
    public class StatusEffectsTests
    {
        static readonly StatusConfig Burn = new StatusConfig { Element = Element.Fire, Stacking = Stacking.RefreshTimer, MaxStacks = 1, TickDamage = 3, TickSeconds = 0.5f, DurationSeconds = 3, BossResist = 0.3f };
        static readonly StatusConfig Cold = new StatusConfig { Element = Element.Ice, Stacking = Stacking.Accumulate, MaxStacks = 3, FreezeAtStacks = 3, SlowFraction = 0.15f, DurationSeconds = 3, FreezeSeconds = 1.5f };
        static readonly StatusConfig Shock = new StatusConfig { Element = Element.Lightning, Stacking = Stacking.None, MaxStacks = 1, StunSeconds = 0.6f };
        static readonly StatusConfig Poison = new StatusConfig { Element = Element.Poison, Stacking = Stacking.Accumulate, MaxStacks = 5, TickDamage = 2, TickSeconds = 0.5f, DurationSeconds = 4 };
        static readonly StatusConfig[] All = { Burn, Cold, Shock, Poison };
        static readonly IRandom Rng = new SeededRandom(1);

        [Test]
        public void Burn_DealsDamageOverTime_RefreshesNotStacks()
        {
            var s = new EnemyState();
            StatusEffects.Apply(s, Burn, Rng); StatusEffects.Apply(s, Burn, Rng);
            Assert.AreEqual(1, s.Statuses[0].Stacks);
            int dmg = 0; for (int i = 0; i < 40; i++) dmg += StatusEffects.Tick(s, All, 0.1f).Damage;
            Assert.That(dmg, Is.InRange(15, 18), "3 урона каждые 0.5 с в течение 3 с");
            Assert.IsEmpty(s.Statuses);
        }

        [Test]
        public void Cold_Slows_ThenFreezesAtThreeStacks()
        {
            var s = new EnemyState();
            StatusEffects.Apply(s, Cold, Rng);
            Assert.AreEqual(0.15f, StatusEffects.Tick(s, All, 0.01f).SlowFraction, 1e-4f);
            StatusEffects.Apply(s, Cold, Rng); StatusEffects.Apply(s, Cold, Rng);
            Assert.IsTrue(StatusEffects.Tick(s, All, 0.01f).Disabled, "заморозка");
            for (int i = 0; i < 16; i++) StatusEffects.Tick(s, All, 0.1f);
            Assert.IsEmpty(s.Statuses, "заморозка 1.5 с прошла");
        }

        [Test]
        public void Shock_Stuns_ForStunSeconds()
        {
            var s = new EnemyState();
            StatusEffects.Apply(s, Shock, Rng);
            Assert.IsTrue(StatusEffects.Tick(s, All, 0.3f).Disabled);
            Assert.IsFalse(StatusEffects.Tick(s, All, 0.4f).Disabled);
        }

        [Test]
        public void Poison_StacksUpToFive()
        {
            var s = new EnemyState();
            for (int i = 0; i < 8; i++) StatusEffects.Apply(s, Poison, Rng);
            Assert.AreEqual(5, s.Statuses[0].Stacks);
            Assert.AreEqual(10, StatusEffects.Tick(s, All, 0.5f).Damage, "2 × 5 стаков");
        }

        [Test]
        public void Boss_Resists_DeadNever()
        {
            var cfg = Cold; cfg.BossResist = 1f;
            Assert.IsFalse(StatusEffects.Apply(new EnemyState { IsBoss = true }, cfg, Rng));
            Assert.IsFalse(StatusEffects.Apply(new EnemyState { Phase = EnemyPhase.Dead }, Burn, Rng));
        }

        [Test]
        public void DifferentStatuses_Coexist_NoReactions()
        {
            var s = new EnemyState();
            StatusEffects.Apply(s, Burn, Rng); StatusEffects.Apply(s, Poison, Rng);
            Assert.AreEqual(2, s.Statuses.Count);
            Assert.AreEqual(5, StatusEffects.Tick(s, All, 0.5f).Damage, "горение 3 + яд 2, без взаимодействия");
        }
    }
}
