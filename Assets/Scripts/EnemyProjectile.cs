using UnityEngine;

namespace FrankenArms
{
    public sealed class EnemyProjectile : MonoBehaviour
    {
        private Vector2 velocity;
        private float lifetime;
        private StatusEffectType effect;
        private bool budgetAcquired;

        internal void Launch(Vector2 direction, float speed, StatusEffectType appliedEffect = StatusEffectType.None)
        {
            budgetAcquired = ProjectileBudget.TryAcquire();
            if (!budgetAcquired)
            {
                Destroy(gameObject);
                return;
            }
            velocity = direction.normalized * speed;
            lifetime = 2.5f;
            effect = appliedEffect;
        }

        private void OnDestroy()
        {
            if (budgetAcquired) ProjectileBudget.Release();
        }

        private void Update()
        {
            transform.position += (Vector3)(velocity * Time.deltaTime);
            lifetime -= Time.deltaTime;
            if (lifetime <= 0f) Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponent<PlayerHealth>();
            if (player == null) return;
            player.TakeDamage(1);
            if (effect != StatusEffectType.None)
                player.GetComponent<StatusEffectController>()?.Apply(effect, effect == StatusEffectType.Frozen ? 0.6f : 3f);
            ScreenShake.Pulse(0.08f, 0.025f);
            Destroy(gameObject);
        }
    }
}
