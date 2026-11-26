using FotF.Logic.Combat;
using FotF.State;
using NUnit.Framework;

namespace FotF.Tests.Logic
{
    public class VitalsTests
    {
        static readonly VitalsConfig Knight = new VitalsConfig(120, 60, 150, 3f, 30f);
        static HeroState NewHero() { var s = new HeroState(); Vitals.Init(s, Knight); return s; }

        [Test]
        public void Init_FillsAllBars()
        {
            var s = NewHero();
            Assert.AreEqual((120, 60, 150), (s.Hp, s.Armor, s.Mana));
        }

        [Test]
        public void Damage_GoesToArmorFirst()
        {
            var s = NewHero();
            var r = Vitals.ApplyDamage(s, 20, Knight);
            Assert.AreEqual(40, s.Armor); Assert.AreEqual(120, s.Hp); Assert.AreEqual(20, r.ToArmor);
        }

        [Test]
        public void HitBiggerThanArmor_BreaksArmor_OverflowDoesNotReachHp()
        {
            var s = NewHero();
            Vitals.ApplyDamage(s, 50, Knight);                 // armor 10 left
            var r = Vitals.ApplyDamage(s, 30, Knight);
            Assert.AreEqual(0, s.Armor); Assert.AreEqual(120, s.Hp);
            Assert.IsTrue(r.ArmorBroken); Assert.AreEqual(0, r.ToHp);
        }

        [Test]
        public void NoArmor_DamageGoesToHp_AndCanKill()
        {
            var s = NewHero(); s.Armor = 0;
            Vitals.ApplyDamage(s, 100, Knight);
            Assert.AreEqual(20, s.Hp);
            var r = Vitals.ApplyDamage(s, 30, Knight);
            Assert.AreEqual(0, s.Hp); Assert.IsTrue(r.Killed); Assert.IsTrue(s.IsDead);
        }

        [Test]
        public void Armor_RegensOnlyAfterDelayWithoutDamage()
        {
            var s = NewHero(); Vitals.ApplyDamage(s, 60, Knight);
            Vitals.Tick(s, Knight, 2.9f);
            Assert.AreEqual(0, s.Armor, "до 3 с броня не растёт");
            Vitals.Tick(s, Knight, 0.1f + 1f);                 // 3 с прошло, затем 1 с восстановления
            Assert.AreEqual(30, s.Armor);
            Vitals.Tick(s, Knight, 10f);
            Assert.AreEqual(60, s.Armor, "не выше максимума");
        }

        [Test]
        public void DamageResetsArmorRegenDelay()
        {
            var s = NewHero(); Vitals.ApplyDamage(s, 60, Knight);
            Vitals.Tick(s, Knight, 2.5f);
            Vitals.ApplyDamage(s, 1, Knight);                  // armor 0 -> hit HP, delay restarts
            Vitals.Tick(s, Knight, 2.5f);
            Assert.AreEqual(0, s.Armor);
        }

        [Test]
        public void Hp_NeverRegenerates()
        {
            var s = NewHero(); s.Armor = 0; Vitals.ApplyDamage(s, 50, Knight);
            Vitals.Tick(s, Knight, 60f);
            Assert.AreEqual(70, s.Hp);
        }

        [Test]
        public void Invulnerable_BlocksDamage()
        {
            var s = NewHero(); s.InvulnerableLeft = 0.2f;
            var r = Vitals.ApplyDamage(s, 50, Knight);
            Assert.IsTrue(r.Blocked); Assert.AreEqual(60, s.Armor);
        }

        [Test]
        public void Mana_SpendAndCap()
        {
            var s = NewHero();
            Assert.IsTrue(Vitals.TrySpendMana(s, 150));
            Assert.IsFalse(Vitals.TrySpendMana(s, 1));
            Assert.IsTrue(Vitals.TrySpendMana(s, 0), "0 маны — всегда можно");
            Vitals.AddMana(s, 500);
            Assert.AreEqual(150, s.Mana);
        }
    }
}
