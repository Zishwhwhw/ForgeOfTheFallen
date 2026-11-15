using UnityEngine;

namespace FrankenArms
{
    /// <summary>
    /// Ranged room threat with a readable cadence. Its windup gives the player
    /// time to move laterally or spend a Dash instead of taking blind damage.
    /// </summary>
    public sealed class EnemyShooter : MonoBehaviour
    {
        [SerializeField] private float fireInterval = 1.45f;
        [SerializeField] private float projectileSpeed = 4.6f;
        private Transform target;
        private float fireTimer;

        private void Awake()
        {
            target = FindAnyObjectByType<PlayerMovement>()?.transform;
            fireTimer = 0.75f;
        }

        private void Update()
        {
            if (target == null) return;
            fireTimer -= Time.deltaTime;
            if (fireTimer > 0f) return;
            FireAtTarget();
            fireTimer = fireInterval;
        }

        private void FireAtTarget()
        {
            var direction = (Vector2)(target.position - transform.position);
            if (direction.sqrMagnitude < 0.01f) return;

            var projectileObject = new GameObject("Enemy Bolt");
            projectileObject.transform.position = transform.position + (Vector3)(direction.normalized * 0.5f);
            var renderer = projectileObject.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeSpriteFactory.CreateProjectile();
            renderer.color = new Color(1f, 0.25f, 0.2f, 1f);
            renderer.sortingOrder = 12;
            var body = projectileObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var collider = projectileObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.12f;
            projectileObject.AddComponent<EnemyProjectile>().Launch(direction, projectileSpeed, StatusEffectType.Poison);
        }
    }
}
