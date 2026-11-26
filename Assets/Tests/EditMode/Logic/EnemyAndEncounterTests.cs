using FotF.Logic.Combat;
using FotF.State;
using NUnit.Framework;

namespace FotF.Tests.Logic
{
    public class EnemyBrainTests
    {
        static readonly EnemyConfig Skeleton = new EnemyConfig(40, 1.1f, 12f, 0.5f, 0.6f);
        static EnemyState New() { var s = new EnemyState(); EnemyBrain.Init(s, Skeleton); return s; }

        [Test]
        public void ApproachesThenWindsUpThenStrikesOnce()
        {
            var s = New();
            Assert.AreEqual(EnemyIntent.Stand, EnemyBrain.Tick(s, Skeleton, 0.1f, 5f));          // aggro
            Assert.AreEqual(EnemyIntent.MoveToTarget, EnemyBrain.Tick(s, Skeleton, 0.1f, 5f));
            Assert.AreEqual(EnemyIntent.Stand, EnemyBrain.Tick(s, Skeleton, 0.1f, 1f));           // в радиусе → замах
            Assert.AreEqual(EnemyPhase.Windup, s.Phase);
            Assert.AreEqual(EnemyIntent.Stand, EnemyBrain.Tick(s, Skeleton, 0.45f, 1f), "замах 0.5 с ещё идёт");
            Assert.AreEqual(EnemyIntent.Strike, EnemyBrain.Tick(s, Skeleton, 0.1f, 1f));
            Assert.AreEqual(EnemyIntent.Stand, EnemyBrain.Tick(s, Skeleton, 0.1f, 1f), "один удар на замах");
            Assert.AreEqual(EnemyPhase.Recover, s.Phase);
        }

        [Test]
        public void StrikeHappensEvenIfHeroDashedAway_TelegraphIsCommitted()
        {
            var s = New(); EnemyBrain.Tick(s, Skeleton, 0.1f, 5f); EnemyBrain.Tick(s, Skeleton, 0.1f, 1f);
            Assert.AreEqual(EnemyIntent.Strike, EnemyBrain.Tick(s, Skeleton, 0.6f, 8f), "удар идёт в точку замаха; увернуться — задача игрока");
        }

        [Test]
        public void StunCancelsWindup()
        {
            var s = New(); EnemyBrain.Tick(s, Skeleton, 0.1f, 5f); EnemyBrain.Tick(s, Skeleton, 0.1f, 1f);
            EnemyBrain.Stun(s, 0.5f);
            Assert.AreEqual(EnemyIntent.Stand, EnemyBrain.Tick(s, Skeleton, 0.6f, 1f));
            Assert.AreEqual(EnemyPhase.Chase, s.Phase, "после оглушения снова преследует");
        }

        [Test]
        public void DiesAtZeroHp_AndDoesNothingAfter()
        {
            var s = New();
            Assert.IsFalse(EnemyBrain.ApplyDamage(s, 18)); Assert.IsFalse(EnemyBrain.ApplyDamage(s, 18));
            Assert.IsTrue(EnemyBrain.ApplyDamage(s, 18)); Assert.IsTrue(s.IsDead);
            Assert.IsFalse(EnemyBrain.ApplyDamage(s, 18), "второй раз не умирает");
            Assert.AreEqual(EnemyIntent.Stand, EnemyBrain.Tick(s, Skeleton, 1f, 0.5f));
        }
    }

    public class EncounterTests
    {
        [Test]
        public void Start_LocksDoors_ThenWavesInOrder_ThenCleared()
        {
            var s = new EncounterState();
            Assert.AreEqual(EncounterEvent.LockDoors, Encounter.Start(s, 2));
            Assert.AreEqual(EncounterEvent.SpawnWave, Encounter.Tick(s, 2, 0.8f, 0.01f));
            Assert.AreEqual(0, s.WaveIndex); Encounter.OnWaveSpawned(s, 3);
            Assert.AreEqual(EncounterEvent.None, Encounter.Tick(s, 2, 0.8f, 1f));
            Encounter.OnEnemyDied(s); Encounter.OnEnemyDied(s); Encounter.OnEnemyDied(s);
            Assert.AreEqual(EncounterEvent.None, Encounter.Tick(s, 2, 0.8f, 0.1f), "пауза между волнами");
            Assert.AreEqual(EncounterEvent.None, Encounter.Tick(s, 2, 0.8f, 0.5f));
            Assert.AreEqual(EncounterEvent.SpawnWave, Encounter.Tick(s, 2, 0.8f, 0.4f));
            Assert.AreEqual(1, s.WaveIndex); Encounter.OnWaveSpawned(s, 2);
            Encounter.OnEnemyDied(s); Encounter.OnEnemyDied(s);
            Assert.AreEqual(EncounterEvent.Cleared, Encounter.Tick(s, 2, 0.8f, 0.1f));
            Assert.AreEqual(EncounterEvent.None, Encounter.Tick(s, 2, 0.8f, 0.1f), "Cleared сообщается один раз");
        }

        [Test]
        public void StartTwice_DoesNothing()
        {
            var s = new EncounterState(); Encounter.Start(s, 1);
            Assert.AreEqual(EncounterEvent.None, Encounter.Start(s, 1));
        }
    }
}
