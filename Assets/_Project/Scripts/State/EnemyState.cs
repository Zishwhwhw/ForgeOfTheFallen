namespace FotF.State
{
    public enum EnemyPhase { Idle, Chase, Windup, Strike, Recover, Stunned, Dead }

    /// <summary>Runtime-состояние врага.</summary>
    public sealed class EnemyState
    {
        public int Hp, MaxHp;
        public EnemyPhase Phase = EnemyPhase.Idle;
        /// <summary>Секунд до конца текущей фазы (замах, восстановление, оглушение).</summary>
        public float PhaseTimeLeft;
        public bool IsDead => Phase == EnemyPhase.Dead;
        /// <summary>Статусы на враге (GDD §7.3).</summary>
        public readonly System.Collections.Generic.List<StatusInstance> Statuses = new System.Collections.Generic.List<StatusInstance>();
        /// <summary>Босс/мини-босс: сопротивление статусам (StatusDefinition.bossResist).</summary>
        public bool IsBoss;
    }

    /// <summary>Наложенный статус: стихия, стаки, остаток времени.</summary>
    public sealed class StatusInstance
    {
        public FotF.Data.Element Element;
        public int Stacks;
        public float TimeLeft, TickLeft;
        public bool Frozen;
    }

    public enum EncounterPhase { NotStarted, Fighting, BetweenWaves, Cleared }

    /// <summary>Runtime-состояние боевой встречи комнаты.</summary>
    public sealed class EncounterState
    {
        public EncounterPhase Phase = EncounterPhase.NotStarted;
        public int WaveIndex = -1;
        public int Alive;
        public float DelayLeft;
    }
}
