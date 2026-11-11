using UnityEngine;

namespace FrankenArms
{
    public sealed class Projectile : MonoBehaviour
    {
        private Vector2 velocity;
        private float lifetime;

        internal void Launch(Vector2 direction, float speed)
        {
            velocity = direction.normalized * speed;
            lifetime = 1.5f;
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
            enemy.TakeDamage(1);
            Destroy(gameObject);
        }
    }
}
