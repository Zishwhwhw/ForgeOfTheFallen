using FotF.Data;
using FotF.Logic.Status;

namespace FotF.View.Combat
{
    /// <summary>StatusDefinition (данные) → StatusConfig (Logic).</summary>
    public static class StatusConfigs
    {
        public static StatusConfig[] From(StatusCatalog catalog)
        {
            if (catalog == null) return null;
            var list = new StatusConfig[catalog.statuses.Length];
            for (int i = 0; i < list.Length; i++)
            {
                var s = catalog.statuses[i];
                if (!s) continue;
                list[i] = new StatusConfig
                {
                    Element = s.element, Stacking = s.stacking, MaxStacks = s.maxStacks, FreezeAtStacks = s.freezeAtStacks,
                    TickDamage = s.tickDamage, TickSeconds = s.tickSeconds, DurationSeconds = s.durationSeconds,
                    SlowFraction = s.slowFraction, StunSeconds = s.stunSeconds, FreezeSeconds = s.freezeSeconds, BossResist = s.bossResist,
                };
            }
            return list;
        }
    }
}
