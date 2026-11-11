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
            Destroy(gameObject);
        }
    }
}
