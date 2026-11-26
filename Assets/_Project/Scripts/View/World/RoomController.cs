using System;
using FotF.Data;
using FotF.Logic.Combat;
using FotF.State;
using FotF.View.Enemies;
using FotF.View.Heroes;
using UnityEngine;

namespace FotF.View.World
{
    /// <summary>
    /// Боевая комната (GDD §3.1): герой вошёл в зону → двери запираются → волны из EncounterDefinition →
    /// сундук → двери открываются. Логика волн — Encounter (Logic); здесь спавн готовых префабов.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class RoomController : MonoBehaviour
    {
        [SerializeField] EncounterDefinition encounter;
        [Tooltip("Префабы врагов; выбирается тот, чей EnemyDefinition совпадает с группой волны")]
        [SerializeField] EnemyController[] enemyPrefabs = Array.Empty<EnemyController>();
        [SerializeField] Transform[] spawnPoints = Array.Empty<Transform>();
        [SerializeField] DoorView[] doors = Array.Empty<DoorView>();
        [SerializeField] ChestView chestPrefab;
        [SerializeField] Transform chestPoint;

        public EncounterState State { get; } = new EncounterState();
        public event Action<string> Announce;

        HeroController _hero;
        int _spawnCursor;

        void OnTriggerEnter2D(Collider2D other)
        {
            var hero = other.GetComponentInParent<HeroController>();
            if (hero == null || State.Phase != EncounterPhase.NotStarted) return;
            _hero = hero;
            Handle(Encounter.Start(State, encounter ? encounter.waves.Length : 0));
        }

        void Update()
        {
            if (encounter == null || State.Phase == EncounterPhase.NotStarted || State.Phase == EncounterPhase.Cleared) return;
            Handle(Encounter.Tick(State, encounter.waves.Length, encounter.delayBetweenWaves, Time.deltaTime));
        }

        void Handle(EncounterEvent e)
        {
            switch (e)
            {
                case EncounterEvent.LockDoors:
                    foreach (var d in doors) d.SetLocked(true);
                    Announce?.Invoke("Двери заперты!");
                    break;
                case EncounterEvent.SpawnWave:
                    SpawnWave(encounter.waves[State.WaveIndex]);
                    Announce?.Invoke($"Волна {State.WaveIndex + 1} / {encounter.waves.Length}");
                    break;
                case EncounterEvent.Cleared:
                    foreach (var d in doors) d.SetLocked(false);
                    if (chestPrefab) Instantiate(chestPrefab, chestPoint ? chestPoint.position : transform.position, Quaternion.identity);
                    Announce?.Invoke("Комната зачищена");
                    break;
            }
        }

        void SpawnWave(EncounterDefinition.Wave wave)
        {
            int spawned = 0;
            foreach (var g in wave.groups)
            {
                var prefab = Array.Find(enemyPrefabs, p => p.Definition == g.enemy);
                if (prefab == null) { Debug.LogError($"[Room] нет префаба для {g.enemy?.id}", this); continue; }
                for (int i = 0; i < g.count; i++)
                {
                    var point = spawnPoints.Length > 0 ? spawnPoints[_spawnCursor++ % spawnPoints.Length].position : transform.position;
                    var e = Instantiate(prefab, point + (Vector3)(UnityEngine.Random.insideUnitCircle * 0.4f), Quaternion.identity);
                    e.SetTarget(_hero);
                    e.Died += _ => Encounter.OnEnemyDied(State);
                    spawned++;
                }
            }
            Encounter.OnWaveSpawned(State, spawned);
        }
    }
}
