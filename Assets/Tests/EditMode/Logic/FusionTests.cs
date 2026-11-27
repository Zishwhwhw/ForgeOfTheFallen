using FotF.Data;
using FotF.Logic;
using FotF.Logic.Fusion;
using FotF.State;
using NUnit.Framework;

namespace FotF.Tests.Logic
{
    public class FusionTests
    {
        sealed class Seq : IRandom
        {
            readonly float _f; readonly int _i;
            public Seq(float f, int i) { _f = f; _i = i; }
            public float NextFloat() => _f; public int NextInt(int a, int b) => _i;
        }

        static WeaponInstance Sword() => new WeaponInstance { DefinitionId = "weapon.eternal_sword", DisplayName = "Вечный меч", Rarity = Rarity.Common, Family = WeaponFamily.Sword,
            AttackType = AttackType.Melee, Damage = 18, CritChance = 0.05f, AttacksPerSecond = 2.5f, ManaCost = 0, MeleeRadius = 1.6f, MeleeArcDegrees = 130f, CutsProjectiles = true, Elements = { Element.Physical } };
        static WeaponInstance Nail() => new WeaponInstance { DefinitionId = "weapon.nailgun", DisplayName = "Гвоздомёт", Rarity = Rarity.Common, Family = WeaponFamily.Pistol,
            AttackType = AttackType.Projectile, Damage = 8, CritChance = 0.05f, AttacksPerSecond = 5f, ManaCost = 1, ProjectileSpeed = 16f, StunSeconds = 0.5f, Elements = { Element.Physical } };
        static WeaponInstance EmberStaff() => new WeaponInstance { DefinitionId = "weapon.ember_staff", DisplayName = "Посох углей", Rarity = Rarity.Rare, Family = WeaponFamily.Staff,
            AttackType = AttackType.Projectile, Damage = 16, CritChance = 0.05f, AttacksPerSecond = 1.5f, ManaCost = 4, Elements = { Element.Fire } };

        [Test]
        public void Result_IsFranken_Hybrid_WithParents()
        {
            var r = FusionService.Fuse(Sword(), Nail(), new Seq(0.5f, 2), FusionRules.Default);
            Assert.AreEqual(Rarity.Franken, r.Weapon.Rarity); Assert.IsTrue(r.Weapon.IsHybrid);
            Assert.AreEqual(("weapon.eternal_sword", "weapon.nailgun"), (r.Weapon.ParentA, r.Weapon.ParentB));
        }

        [Test]
        public void Elements_OfBothParents_AlwaysPass()
        {
            var r = FusionService.Fuse(Nail(), EmberStaff(), new Seq(0.5f, 0), FusionRules.Default);
            CollectionAssert.Contains(r.Weapon.Elements, Element.Fire);
        }

        [Test]
        public void AttackTypeAndVisual_FromRarerParent()
        {
            var r = FusionService.Fuse(Sword(), EmberStaff(), new Seq(0.5f, 0), FusionRules.Default); // доминанта A (меч), но посох реже
            Assert.AreEqual(AttackType.Projectile, r.Weapon.AttackType);
            Assert.AreEqual("weapon.ember_staff", r.Weapon.DefinitionId);
            Assert.AreEqual("weapon.eternal_sword", r.Weapon.DetailId);
        }

        [Test]
        public void EqualRarity_FirstPlacedGivesType()
        {
            Assert.AreEqual(AttackType.Melee, FusionService.Fuse(Sword(), Nail(), new Seq(0.5f, 2), FusionRules.Default).Weapon.AttackType);
            Assert.AreEqual(AttackType.Projectile, FusionService.Fuse(Nail(), Sword(), new Seq(0.5f, 2), FusionRules.Default).Weapon.AttackType);
        }

        [Test]
        public void Dominance_ShiftsStats_TowardDominantParent()
        {
            var a = FusionService.Fuse(Sword(), Nail(), new Seq(0.5f, 0), FusionRules.Default).Weapon;  // A
            var b = FusionService.Fuse(Sword(), Nail(), new Seq(0.5f, 1), FusionRules.Default).Weapon;  // B
            Assert.Greater(b.AttacksPerSecond, a.AttacksPerSecond, "при доминанте гвоздомёта темп выше");
        }

        [Test]
        public void Hybrid_NotWeakerThanBestParentMinus10Percent()
        {
            var rules = FusionRules.Default;
            foreach (int d in new[] { 0, 1, 2 })
            {
                var r = FusionService.Fuse(Sword(), EmberStaff(), new Seq(0.5f, d), rules).Weapon;
                float best = System.Math.Max(Sword().Power, EmberStaff().Power);
                Assert.GreaterOrEqual(r.Power, best * rules.FloorOfBestParent - 0.01f, $"доминанта {d}");
            }
        }

        [Test]
        public void Jackpot_WhenRollBelowChance()
        {
            var r = FusionService.Fuse(Sword(), Nail(), new Seq(0.01f, 2), FusionRules.Default);
            Assert.IsTrue(r.Jackpot); Assert.AreEqual(26, r.Weapon.Damage);
            Assert.IsFalse(FusionService.Fuse(Sword(), Nail(), new Seq(0.5f, 2), FusionRules.Default).Jackpot);
        }

        [Test]
        public void JackpotRate_IsAboutTwoToThreePercent()
        {
            var rng = new SeededRandom(42); int hits = 0; const int n = 20000;
            for (int i = 0; i < n; i++) if (FusionService.Fuse(Sword(), Nail(), rng, FusionRules.Default).Jackpot) hits++;
            Assert.That(hits / (float)n, Is.InRange(0.02f, 0.03f));
        }

        [Test]
        public void NoRefusion_OfHybrids()
        {
            var h = FusionService.Fuse(Sword(), Nail(), new Seq(0.5f, 2), FusionRules.Default).Weapon;
            Assert.IsFalse(FusionService.CanFuse(h, Nail()));
            Assert.Throws<System.InvalidOperationException>(() => FusionService.Fuse(h, Nail(), new Seq(0.5f, 2), FusionRules.Default));
        }

        [Test]
        public void MeleeHybrid_CostsNoMana_AndKeepsAbilities()
        {
            var r = FusionService.Fuse(Sword(), Nail(), new Seq(0.5f, 1), FusionRules.Default).Weapon;
            Assert.AreEqual(0, r.ManaCost); Assert.IsTrue(r.CutsProjectiles); Assert.AreEqual(0.5f, r.StunSeconds);
        }
    }
}
