using System.Collections.Generic;
using FotF.Data;
using FotF.Logic.Combat;
using FotF.State;
using NUnit.Framework;

namespace FotF.Tests.Logic
{
    /// <summary>Особенности оружия GDD §7.1 и их переход в гибрид.</summary>
    public class WeaponSpecialsTests
    {
        static WeaponSpecial S(string type, params (string, float)[] v)
        {
            var s = new WeaponSpecial { Type = type };
            foreach (var (k, x) in v) s.Values[k] = x;
            return s;
        }

        static WeaponInstance W(AttackType a, params WeaponSpecial[] specials) =>
            new WeaponInstance { AttackType = a, Specials = new List<WeaponSpecial>(specials) };

        [Test]
        public void Overheat_LocksAfterEnoughShots_ThenCools()
        {
            var w = W(AttackType.Projectile, S(WeaponSpecials.Overheat, ("heatPerShot", 0.25f), ("coolPerSecond", 1f)));
            for (int i = 0; i < 4; i++) { Assert.IsTrue(WeaponSpecials.CanFire(w)); WeaponSpecials.OnFired(w); }
            Assert.IsFalse(WeaponSpecials.CanFire(w), "4 × 0.25 = перегрев");
            WeaponSpecials.Tick(w, 0.5f);
            Assert.IsFalse(WeaponSpecials.CanFire(w), "остывает до нуля");
            WeaponSpecials.Tick(w, 0.6f);
            Assert.IsTrue(WeaponSpecials.CanFire(w));
        }

        [Test]
        public void Bow_Charge_10To30_PierceAtFull()
        {
            var c = S(WeaponSpecials.Charge, ("minDamage", 10), ("maxDamage", 30), ("chargeSeconds", 1f));
            Assert.AreEqual(10, WeaponSpecials.ChargedDamage(c, 0f, 10));
            Assert.AreEqual(20, WeaponSpecials.ChargedDamage(c, 0.5f, 10));
            Assert.AreEqual(30, WeaponSpecials.ChargedDamage(c, 2f, 10));
            Assert.IsFalse(WeaponSpecials.FullyCharged(c, 0.9f));
            Assert.IsTrue(WeaponSpecials.FullyCharged(c, 1f));
        }

        [Test]
        public void SawedOff_Knockback_OnlyClose()
        {
            var w = W(AttackType.Projectile, S(WeaponSpecials.Knockback, ("closeRangeOnly", 1), ("closeRange", 2.5f), ("force", 6)));
            Assert.AreEqual(6f, WeaponSpecials.KnockbackForce(w, 1f));
            Assert.AreEqual(0f, WeaponSpecials.KnockbackForce(w, 4f));
        }

        [Test]
        public void Hybrid_MeleeDropsProjectileSpecials_KeepsKnockback()
        {
            var hammer = W(AttackType.Melee, S(WeaponSpecials.Knockback, ("force", 8)));
            var ember = W(AttackType.Projectile, S(WeaponSpecials.Explosion, ("radius", 1.5f)));
            var merged = WeaponSpecials.Merge(hammer, ember, AttackType.Melee);
            Assert.AreEqual(1, merged.Count);
            Assert.AreEqual(WeaponSpecials.Knockback, merged[0].Type);
        }

        [Test]
        public void Hybrid_OneFireMode_FromPrimary()
        {
            var crossbow = W(AttackType.Projectile, S(WeaponSpecials.Burst, ("shots", 3)));
            var rattler = W(AttackType.Projectile, S(WeaponSpecials.Overheat), S(WeaponSpecials.Homing));
            var merged = WeaponSpecials.Merge(crossbow, rattler, AttackType.Projectile);
            CollectionAssert.AreEquivalent(new[] { WeaponSpecials.Burst, WeaponSpecials.Homing }, merged.ConvertAll(s => s.Type));
        }
    }
}
