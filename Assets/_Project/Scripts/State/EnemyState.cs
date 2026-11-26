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
