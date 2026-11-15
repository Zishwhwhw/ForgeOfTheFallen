using UnityEngine;

namespace FrankenArms
{
    /// <summary>
    /// First GDD pet prototype. The dog follows the hero, cannot die, and
    /// periodically bites a nearby enemy to create a small priority shift.
    /// </summary>
    public sealed class PetDog : MonoBehaviour
    {
        [SerializeField] private float followSpeed = 4.2f;
        [SerializeField] private float attackRange = 1.35f;
        [SerializeField] private float attackInterval = 1.25f;
        private Transform player;
        private float attackTimer;

        private void Awake()
        {
            player = FindAnyObjectByType<PlayerMovement>()?.transform;
            attackTimer = 0.5f;
        }

        private void Update()
        {
            if (player == null) return;
            var desired = player.position + new Vector3(-0.75f, -0.75f, 0f);
            transform.position = Vector3.MoveTowards(transform.position, desired, followSpeed * Time.deltaTime);
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                BiteNearestEnemy();
                attackTimer = attackInterval;
            }
        }

        private void BiteNearestEnemy()
        {
            var enemies = FindObjectsByType<EnemyTarget>();
            EnemyTarget nearest = null;
            var nearestDistance = attackRange * attackRange;
            for (var i = 0; i < enemies.Length; i++)
            {
                var distance = ((Vector2)(enemies[i].transform.position - transform.position)).sqrMagnitude;
                if (distance >= nearestDistance) continue;
                nearestDistance = distance;
                nearest = enemies[i];
            }
            if (nearest == null) return;
            nearest.TakeDamage(1);
            nearest.GetComponent<StatusEffectController>()?.Apply(StatusEffectType.Marked, 4f);
            ScreenShake.Pulse(0.05f, 0.02f);
        }
    }
}
