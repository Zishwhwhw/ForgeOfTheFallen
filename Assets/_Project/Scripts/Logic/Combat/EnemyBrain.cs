using FotF.State;

namespace FotF.Logic.Combat
{
    public readonly struct EnemyConfig
    {
        public readonly int MaxHp;
        public readonly float AttackRange, AggroRange, Windup, Recover;
        public EnemyConfig(int maxHp, float attackRange, float aggroRange, float windup, float recover)
        { MaxHp = maxHp; AttackRange = attackRange; AggroRange = aggroRange; Windup = windup; Recover = recover; }
    }

    /// <summary>Что View должен сделать в этом кадре.</summary>
    public enum EnemyIntent { Stand, MoveToTarget, Strike }

    /// <summary>
    /// Поведение ближнего врага (Скелет-мечник, GDD §7.4): подходит → замах (телеграф, GDD п.86) → удар → пауза.
    /// Оглушение прерывает замах. Чистая логика, без Unity.
    /// </summary>
    public static class EnemyBrain
    {
        public static void Init(EnemyState s, in EnemyConfig c) { s.MaxHp = s.Hp = c.MaxHp; s.Phase = EnemyPhase.Idle; s.PhaseTimeLeft = 0f; }

        public static EnemyIntent Tick(EnemyState s, in EnemyConfig c, float dt, float distanceToTarget)
        {
            if (s.IsDead) return EnemyIntent.Stand;
            if (s.PhaseTimeLeft > 0f) s.PhaseTimeLeft -= dt;
            switch (s.Phase)
            {
                case EnemyPhase.Idle:
                    if (distanceToTarget <= c.AggroRange) s.Phase = EnemyPhase.Chase;
                    return EnemyIntent.Stand;
                case EnemyPhase.Chase:
                    if (distanceToTarget <= c.AttackRange) { s.Phase = EnemyPhase.Windup; s.PhaseTimeLeft = c.Windup; return EnemyIntent.Stand; }
                    return EnemyIntent.MoveToTarget;
                case EnemyPhase.Windup:
                    if (s.PhaseTimeLeft > 0f) return EnemyIntent.Stand;
                    s.Phase = EnemyPhase.Recover; s.PhaseTimeLeft = c.Recover;
                    return EnemyIntent.Strike;                       // удар ровно один раз — на выходе из замаха
                case EnemyPhase.Recover:
                case EnemyPhase.Stunned:
                    if (s.PhaseTimeLeft <= 0f) s.Phase = EnemyPhase.Chase;
                    return EnemyIntent.Stand;
                default:
                    return EnemyIntent.Stand;
            }
        }

        /// <summary>Урон врагу. Возвращает true, если враг умер этим ударом.</summary>
        public static bool ApplyDamage(EnemyState s, int amount)
        {
            if (s.IsDead || amount <= 0) return false;
            s.Hp -= amount;
            if (s.Hp > 0) return false;
            s.Hp = 0; s.Phase = EnemyPhase.Dead; s.PhaseTimeLeft = 0f;
            return true;
        }

        /// <summary>Оглушение (Гвоздомёт: 0.5 с) — сбивает замах.</summary>
        public static void Stun(EnemyState s, float seconds)
        {
            if (s.IsDead || seconds <= 0f) return;
            s.Phase = EnemyPhase.Stunned;
            if (seconds > s.PhaseTimeLeft) s.PhaseTimeLeft = seconds;
        }
    }
}
