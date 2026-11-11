using UnityEngine;

namespace FrankenArms
{
    public sealed class CombatRoom : MonoBehaviour
    {
        private int remaining;
        private bool started;
        private bool cleared;

        private void Start()
        {
            Invoke(nameof(BeginWave), 0.35f);
        }

        private void BeginWave()
        {
            if (started) return;
            started = true;
            var positions = new[]
            {
                new Vector3(3f, 0f, 0f),
                new Vector3(-2.5f, 2f, 0f),
                new Vector3(-2.5f, -2f, 0f)
            };
            remaining = positions.Length;
            for (var i = 0; i < positions.Length; i++) SpawnEnemy(positions[i]);
            Debug.Log("Combat room started: wave 1/1, enemies 3");
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
    }
}
