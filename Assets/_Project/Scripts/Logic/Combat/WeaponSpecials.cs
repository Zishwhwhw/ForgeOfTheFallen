using System.Collections.Generic;
using FotF.Data;
using FotF.State;

namespace FotF.Logic.Combat
{
    /// <summary>
    /// Особенности оружия GDD §7.1 (числа — WeaponDefinition.special; пустые — черновик в weapons.json):
    /// перегрев вместо маны, натяжение лука, очередь, взрыв, самонаводка, цепная молния, отбрасывание, пробитие.
    /// </summary>
    public static class WeaponSpecials
    {
        public const string Overheat = "Overheat", Charge = "Charge", Burst = "Burst", Explosion = "Explosion",
            Homing = "Homing", ChainLightning = "ChainLightning", Knockback = "Knockback", Shockwave = "Shockwave";

        /// <summary>Особенности, которые работают только у снарядов.</summary>
        static readonly HashSet<string> ProjectileOnly = new HashSet<string> { Overheat, Charge, Burst, Explosion, Homing, ChainLightning };
        /// <summary>Режимы стрельбы: у гибрида остаётся один — от родителя, давшего тип атаки.</summary>
        static readonly HashSet<string> FireModes = new HashSet<string> { Overheat, Charge, Burst };

        public static bool FitsAttackType(string type, AttackType attack) =>
            attack == AttackType.Projectile || !ProjectileOnly.Contains(type);

        /// <summary>
        /// Особенности гибрида (GDD §6.2 «смешиваются способности»): от обоих родителей, только подходящие к типу атаки,
        /// без повторов; режим стрельбы — один, от родителя типа атаки (primary). Черновое правило 28.11.
        /// </summary>
        public static List<WeaponSpecial> Merge(WeaponInstance primary, WeaponInstance other, AttackType attack)
        {
            var result = new List<WeaponSpecial>();
            bool hasMode = false;
            foreach (var src in new[] { primary, other })
                foreach (var s in src.Specials)
                {
                    if (!FitsAttackType(s.Type, attack) || result.Exists(x => x.Type == s.Type)) continue;
                    if (FireModes.Contains(s.Type)) { if (hasMode) continue; hasMode = true; }
                    result.Add(s);
                }
            return result;
        }

        // ── Перегрев ──
        public static bool CanFire(WeaponInstance w) => !w.Overheated;

        public static void OnFired(WeaponInstance w)
        {
            var s = w.Special(Overheat);
            if (s == null) return;
            w.Heat += s.Get("heatPerShot", 0.06f);
            if (w.Heat >= 1f) { w.Heat = 1f; w.Overheated = true; }
        }

        public static void Tick(WeaponInstance w, float dt)
        {
            var s = w?.Special(Overheat);
            if (s == null || w.Heat <= 0f) return;
            w.Heat -= s.Get("coolPerSecond", 0.5f) * dt;
            if (w.Heat <= 0f) { w.Heat = 0f; w.Overheated = false; }
        }

        // ── Натяжение ──
        /// <summary>Урон по времени натяжения: от minDamage до maxDamage за chargeSeconds.</summary>
        public static int ChargedDamage(WeaponSpecial charge, float heldSeconds, int fallback)
        {
            if (charge == null) return fallback;
            float t = Clamp01(heldSeconds / System.Math.Max(0.01f, charge.Get("chargeSeconds", 1f)));
            float min = charge.Get("minDamage", fallback), max = charge.Get("maxDamage", fallback);
            return (int)System.Math.Round(min + (max - min) * t);
        }

        public static bool FullyCharged(WeaponSpecial charge, float heldSeconds) =>
            charge != null && heldSeconds >= charge.Get("chargeSeconds", 1f);

        // ── Отбрасывание ──
        /// <summary>Сила отбрасывания; у обреза — только вблизи (closeRangeOnly).</summary>
        public static float KnockbackForce(WeaponInstance w, float distance)
        {
            var s = w?.Special(Knockback);
            if (s == null) return 0f;
            if (s.Get("closeRangeOnly", 0f) > 0f && distance > s.Get("closeRange", 2.5f)) return 0f;
            return s.Get("force", 6f);
        }

        static float Clamp01(float v) => v < 0f ? 0f : v > 1f ? 1f : v;
    }
}
