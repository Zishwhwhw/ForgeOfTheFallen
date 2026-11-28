using FotF.Data;
using FotF.State;

namespace FotF.Logic.Status
{
    /// <summary>Числа статуса одной стихии (из StatusDefinition).</summary>
    public struct StatusConfig
    {
        public Element Element;
        public Stacking Stacking;
        public int MaxStacks, FreezeAtStacks;
        public float TickDamage, TickSeconds, DurationSeconds, SlowFraction, StunSeconds, FreezeSeconds, BossResist;
    }

    /// <summary>Итог тика статусов для View: урон, замедление, оглушение/заморозка.</summary>
    public struct StatusTick
    {
        public int Damage;
        public float SlowFraction;
        /// <summary>Заморожен или в шоке — не ходит и не бьёт.</summary>
        public bool Disabled;
    }

    /// <summary>
    /// Статусы среза (GDD §7.3): горение — урон во времени, повтор обновляет таймер; холод — замедление,
    /// стаки до заморозки; шок — оглушение; яд — урон во времени, до 5 стаков. Реакций между статусами нет (п.23).
    /// </summary>
    public static class StatusEffects
    {
        /// <summary>Наложить статус. Боссы сопротивляются с вероятностью bossResist.</summary>
        public static bool Apply(EnemyState s, in StatusConfig c, IRandom rng)
        {
            if (s.IsDead) return false;
            if (s.IsBoss && rng.NextFloat() < c.BossResist) return false;
            var st = Find(s, c.Element);
            if (st == null) { st = new StatusInstance { Element = c.Element }; s.Statuses.Add(st); }
            if (st.Frozen) return false;
            st.Stacks = c.Stacking == Stacking.Accumulate ? System.Math.Min(c.MaxStacks, st.Stacks + 1) : 1;
            st.TimeLeft = c.Stacking == Stacking.None ? c.StunSeconds : c.DurationSeconds;
            if (st.TickLeft <= 0f) st.TickLeft = c.TickSeconds;
            if (c.FreezeAtStacks > 0 && st.Stacks >= c.FreezeAtStacks) { st.Frozen = true; st.TimeLeft = c.FreezeSeconds; }
            return true;
        }

        public static StatusTick Tick(EnemyState s, StatusConfig[] configs, float dt)
        {
            var r = new StatusTick();
            for (int i = s.Statuses.Count - 1; i >= 0; i--)
            {
                var st = s.Statuses[i];
                var c = ConfigFor(configs, st.Element);
                st.TimeLeft -= dt;
                if (c.TickDamage > 0f && c.TickSeconds > 0f && !st.Frozen)
                {
                    st.TickLeft -= dt;
                    while (st.TickLeft <= 1e-5f && st.TimeLeft > -1e-5f)
                    {
                        st.TickLeft += c.TickSeconds;
                        r.Damage += (int)System.Math.Round(c.TickDamage * st.Stacks);
                    }
                }
                if (st.TimeLeft <= 1e-5f) { s.Statuses.RemoveAt(i); continue; }
                if (st.Frozen || c.Stacking == Stacking.None) r.Disabled = true;
                if (c.SlowFraction > 0f && !st.Frozen) r.SlowFraction = System.Math.Min(0.9f, r.SlowFraction + c.SlowFraction * st.Stacks);
            }
            return r;
        }

        public static StatusInstance Find(EnemyState s, Element e)
        {
            foreach (var st in s.Statuses) if (st.Element == e) return st;
            return null;
        }

        public static StatusConfig ConfigFor(StatusConfig[] configs, Element e)
        {
            if (configs != null) foreach (var c in configs) if (c.Element == e) return c;
            return default;
        }

        public static bool Exists(StatusConfig[] configs, Element e)
        {
            if (configs == null || e == Element.Physical) return false;
            foreach (var c in configs) if (c.Element == e) return true;
            return false;
        }
    }
}
