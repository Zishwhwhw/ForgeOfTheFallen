using FotF.State;

namespace FotF.Logic.Combat
{
    public readonly struct DashConfig
    {
        public readonly int Charges;
        public readonly float Cooldown, Duration, Invulnerability;
        public DashConfig(int charges, float cooldown, float duration, float invulnerability)
        { Charges = charges; Cooldown = cooldown; Duration = duration; Invulnerability = invulnerability; }
    }

    /// <summary>Рывок (GDD §4.1): заряды, перезарядка по одному заряду, i-frames.</summary>
    public static class Dash
    {
        public static void Init(HeroState s, in DashConfig c)
        {
            s.MaxDashCharges = s.DashCharges = c.Charges;
            s.DashRechargeLeft = 0f; s.DashTimeLeft = 0f; s.InvulnerableLeft = 0f;
        }

        public static bool TryStart(HeroState s, in DashConfig c)
        {
            if (s.IsDead || s.IsDashing || s.DashCharges <= 0) return false;
            s.DashCharges--;
            if (s.DashRechargeLeft <= 0f) s.DashRechargeLeft = c.Cooldown;
            s.DashTimeLeft = c.Duration;
            if (c.Invulnerability > s.InvulnerableLeft) s.InvulnerableLeft = c.Invulnerability;
            return true;
        }

        public static void Tick(HeroState s, in DashConfig c, float dt)
        {
            if (dt <= 0f) return;
            if (s.DashTimeLeft > 0f) s.DashTimeLeft = s.DashTimeLeft > dt ? s.DashTimeLeft - dt : 0f;
            if (s.InvulnerableLeft > 0f) s.InvulnerableLeft = s.InvulnerableLeft > dt ? s.InvulnerableLeft - dt : 0f;
            if (s.DashCharges >= s.MaxDashCharges) { s.DashRechargeLeft = 0f; return; }
            s.DashRechargeLeft -= dt;
            while (s.DashRechargeLeft <= 0f && s.DashCharges < s.MaxDashCharges)
            {
                s.DashCharges++;
                s.DashRechargeLeft += s.DashCharges < s.MaxDashCharges ? c.Cooldown : 0f;
                if (s.DashCharges >= s.MaxDashCharges) { s.DashRechargeLeft = 0f; break; }
            }
        }
    }
}
