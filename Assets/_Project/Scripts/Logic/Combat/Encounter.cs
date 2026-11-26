using FotF.State;

namespace FotF.Logic.Combat
{
    /// <summary>Что View должен сделать после шага встречи.</summary>
    public enum EncounterEvent { None, LockDoors, SpawnWave, Cleared }

    /// <summary>
    /// Бой в комнате (GDD §3.1): вошёл → двери запираются → волны по очереди → награда → двери открываются.
    /// Волна считается зачищенной, когда живых не осталось.
    /// </summary>
    public static class Encounter
    {
        public static EncounterEvent Start(EncounterState s, int waveCount)
        {
            if (s.Phase != EncounterPhase.NotStarted) return EncounterEvent.None;
            if (waveCount <= 0) { s.Phase = EncounterPhase.Cleared; return EncounterEvent.Cleared; }
            s.Phase = EncounterPhase.BetweenWaves; s.WaveIndex = -1; s.DelayLeft = 0f;
            return EncounterEvent.LockDoors;
        }

        /// <summary>Сообщить о заспавненных врагах текущей волны.</summary>
        public static void OnWaveSpawned(EncounterState s, int count) { s.Alive += count; s.Phase = EncounterPhase.Fighting; }

        public static void OnEnemyDied(EncounterState s) { if (s.Alive > 0) s.Alive--; }

        public static EncounterEvent Tick(EncounterState s, int waveCount, float delayBetweenWaves, float dt)
        {
            switch (s.Phase)
            {
                case EncounterPhase.Fighting:
                    if (s.Alive > 0) return EncounterEvent.None;
                    if (s.WaveIndex + 1 >= waveCount) { s.Phase = EncounterPhase.Cleared; return EncounterEvent.Cleared; }
                    s.Phase = EncounterPhase.BetweenWaves; s.DelayLeft = delayBetweenWaves;
                    return EncounterEvent.None;
                case EncounterPhase.BetweenWaves:
                    s.DelayLeft -= dt;
                    if (s.DelayLeft > 0f) return EncounterEvent.None;
                    s.WaveIndex++;
                    return EncounterEvent.SpawnWave;
                default:
                    return EncounterEvent.None;
            }
        }
    }
}
