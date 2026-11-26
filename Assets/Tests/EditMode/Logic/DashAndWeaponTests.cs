using FotF.Logic;
using FotF.Logic.Combat;
using FotF.State;
using NUnit.Framework;

namespace FotF.Tests.Logic
{
    public class DashTests
    {
        static readonly DashConfig One = new DashConfig(1, 1f, 0.2f, 0.25f);

        [Test]
        public void Dash_UsesCharge_GivesIFrames_ThenRecharges()
        {
            var s = new HeroState { Hp = 100, MaxHp = 100 }; Dash.Init(s, One);
            Assert.IsTrue(Dash.TryStart(s, One));
            Assert.IsTrue(s.IsDashing); Assert.IsTrue(s.IsInvulnerable); Assert.AreEqual(0, s.DashCharges);
            Assert.IsFalse(Dash.TryStart(s, One), "нет заряда");
            Dash.Tick(s, One, 0.3f);
            Assert.IsFalse(s.IsDashing); Assert.IsFalse(s.IsInvulnerable);
            Dash.Tick(s, One, 0.69f);
            Assert.AreEqual(0, s.DashCharges, "кулдаун ~1 с ещё не прошёл");
            Dash.Tick(s, One, 0.02f);
            Assert.AreEqual(1, s.DashCharges);
        }

        [Test]
        public void TwoCharges_RechargeOneAtATime()
        {
            var two = new DashConfig(2, 1f, 0.1f, 0.1f);
            var s = new HeroState { Hp = 100, MaxHp = 100 }; Dash.Init(s, two);
            Dash.TryStart(s, two); Dash.Tick(s, two, 0.15f); Dash.TryStart(s, two);
            Assert.AreEqual(0, s.DashCharges);
            Dash.Tick(s, two, 0.9f); Assert.AreEqual(1, s.DashCharges);
            Dash.Tick(s, two, 1.0f); Assert.AreEqual(2, s.DashCharges);
        }
    }

    public class WeaponAttackTests
    {
        sealed class FixedRandom : IRandom
        {
            readonly float _v; public FixedRandom(float v) { _v = v; }
            public float NextFloat() => _v; public int NextInt(int a, int b) => a;
        }

        static HeroState Hero(int mana)
        {
            var s = new HeroState { Mana = mana, MaxMana = 150, Hp = 100, MaxHp = 100 };
            s.Weapons[0].WeaponId = "weapon.nailgun";
            return s;
        }

        [Test]
        public void Attack_RespectsCooldown()
        {
            var s = Hero(10); var w = new WeaponStats(8, 4f, 1, 0f);
            Assert.AreEqual(WeaponAttack.Result.Fired, WeaponAttack.TryAttack(s, s.ActiveWeapon, w));
            Assert.AreEqual(WeaponAttack.Result.Cooldown, WeaponAttack.TryAttack(s, s.ActiveWeapon, w));
            WeaponAttack.Tick(s.ActiveWeapon, 0.25f);
            Assert.AreEqual(WeaponAttack.Result.Fired, WeaponAttack.TryAttack(s, s.ActiveWeapon, w));
        }

        [Test]
        public void Attack_SpendsMana_AndFailsWithoutIt()
        {
            var s = Hero(1); var w = new WeaponStats(8, 100f, 1, 0f);
            Assert.AreEqual(WeaponAttack.Result.Fired, WeaponAttack.TryAttack(s, s.ActiveWeapon, w));
            Assert.AreEqual(0, s.Mana);
            WeaponAttack.Tick(s.ActiveWeapon, 1f);
            Assert.AreEqual(WeaponAttack.Result.NoMana, WeaponAttack.TryAttack(s, s.ActiveWeapon, w));
        }

        [Test]
        public void ZeroManaWeapon_WorksAtZeroMana()
        {
            var s = Hero(0); var sword = new WeaponStats(18, 2f, 0, 0.05f);
            Assert.AreEqual(WeaponAttack.Result.Fired, WeaponAttack.TryAttack(s, s.ActiveWeapon, sword));
        }

        [Test]
        public void Crit_DoublesDamage()
        {
            Assert.AreEqual(36, WeaponAttack.RollDamage(18, 0.05f, new FixedRandom(0.01f)).Amount);
            Assert.AreEqual(18, WeaponAttack.RollDamage(18, 0.05f, new FixedRandom(0.5f)).Amount);
        }

        [Test]
        public void Swap_OnlyToNonEmptySlot()
        {
            var s = Hero(0);
            WeaponAttack.Swap(s); Assert.AreEqual(0, s.ActiveSlot);
            s.Weapons[1].WeaponId = "weapon.eternal_sword";
            WeaponAttack.Swap(s); Assert.AreEqual(1, s.ActiveSlot);
        }
    }
}

