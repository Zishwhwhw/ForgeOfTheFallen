using UnityEngine;

namespace FrankenArms
{
    public sealed class CombatRoom : MonoBehaviour
    {
        private int remaining;
        private bool started;
        private bool cleared;
        private RoomDoor[] doors;
        private int roomIndex = 1;
        private RunSummaryUI summary;

        internal int RoomIndex { get { return roomIndex; } }
        internal bool IsCleared { get { return cleared; } }

        private void Start()
        {
            Invoke(nameof(BeginWave), 0.35f);
        }

        private void BeginWave()
        {
            if (started) return;
            started = true;
            var positions = GetWavePositions();
            remaining = positions.Length;
            CreateDoors();
            for (var i = 0; i < positions.Length; i++) SpawnEnemy(positions[i]);
            Debug.Log(string.Format("Combat room {0} started: enemies {1}", roomIndex, positions.Length));
        }

        private Vector3[] GetWavePositions()
        {
            if (roomIndex % 3 == 2)
                return new[] { new Vector3(3f, 0f), new Vector3(-2.5f, 2f), new Vector3(-2.5f, -2f), new Vector3(0f, 3f) };
            if (roomIndex % 3 == 0)
                return new[] { new Vector3(3f, 2.5f), new Vector3(3f, -2.5f), new Vector3(-3f, 0f), new Vector3(0f, 2.5f), new Vector3(0f, -2.5f) };
            return new[] { new Vector3(3f, 0f), new Vector3(-2.5f, 2f), new Vector3(-2.5f, -2f) };
        }

        internal void StartNextRoom()
        {
            if (!cleared) return;
            roomIndex++;
            cleared = false;
            started = false;
            if (doors != null)
                for (var i = 0; i < doors.Length; i++) doors[i].Lock();
            BeginWave();
            Debug.Log("Entered room " + roomIndex);
        }

        private void SpawnEnemy(Vector3 position)
        {
            var enemyObject = new GameObject("Enemy Target");
            enemyObject.transform.position = position;
            var renderer = enemyObject.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeSpriteFactory.CreateSquare(24);
            renderer.color = new Color(0.95f, 0.25f, 0.3f, 1f);
            renderer.sortingOrder = 9;
            var collider = enemyObject.AddComponent<CircleCollider2D>();
            collider.radius = 0.38f;
            enemyObject.AddComponent<EnemyTarget>().Initialize(this);
            enemyObject.AddComponent<EnemyContactDamage>();
        }

        internal void EnemyDefeated()
        {
            remaining--;
            if (remaining > 0 || cleared) return;
            cleared = true;
            if (doors != null)
                for (var i = 0; i < doors.Length; i++) doors[i].Open();
            if (summary == null)
            {
                summary = gameObject.AddComponent<RunSummaryUI>();
                summary.Initialize(this);
            }
            Debug.Log("Combat room cleared: reward spawned");
            var reward = new GameObject("Room Reward");
            reward.transform.position = Vector3.zero;
            var renderer = reward.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeSpriteFactory.CreateSquare(16);
            renderer.color = new Color(1f, 0.85f, 0.2f, 1f);
            renderer.sortingOrder = 11;
            var collider = reward.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.35f;
            reward.AddComponent<CoinPickup>();
        }

        private void CreateDoors()
        {
            doors = new[]
            {
                CreateDoor(new Vector3(-7.5f, 0f, 0f), false),
                CreateDoor(new Vector3(7.5f, 0f, 0f), true)
            };
        }

        private RoomDoor CreateDoor(Vector3 position, bool isExit)
        {
            var doorObject = new GameObject("Room Door");
            doorObject.transform.position = position;
            doorObject.transform.localScale = new Vector3(0.5f, 2.2f, 1f);
            var renderer = doorObject.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeSpriteFactory.CreateSquare(16);
            renderer.sortingOrder = 8;
            var door = doorObject.AddComponent<RoomDoor>();
            door.Lock();
            if (isExit)
            {
                var exit = doorObject.AddComponent<RoomExit>();
                exit.Initialize(this);
                door.SetExit(exit);
            }
            return door;
        }
    }
}
