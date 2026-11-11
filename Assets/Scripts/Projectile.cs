using UnityEngine;

namespace FrankenArms
{
    public sealed class Projectile : MonoBehaviour
    {
        private Vector2 velocity;
        private float lifetime;
        private int damage;

        internal void Launch(Vector2 direction, float speed, int damageAmount)
        {
            velocity = direction.normalized * speed;
            lifetime = 1.5f;
            damage = damageAmount;
        }

        private void Update()
        {
            transform.position += (Vector3)(velocity * Time.deltaTime);
            lifetime -= Time.deltaTime;
            if (lifetime <= 0f) Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var enemy = other.GetComponent<EnemyTarget>();
            if (enemy == null) return;
            enemy.TakeDamage(damage);
            ScreenShake.Pulse(0.08f, 0.04f);
            var burst = new GameObject("Impact Burst");
            burst.transform.position = transform.position;
            var burstRenderer = burst.AddComponent<SpriteRenderer>();
            burstRenderer.sprite = RuntimeSpriteFactory.CreateProjectile();
            burstRenderer.sortingOrder = 14;
            burst.AddComponent<ImpactBurst>().Initialize(new Color(1f, 0.35f, 0.15f, 1f), 0.12f, 0.65f);
            Destroy(gameObject);
        }
    }
}
