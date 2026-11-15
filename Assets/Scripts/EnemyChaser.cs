using UnityEngine;

namespace FrankenArms
{
    /// <summary>
    /// Lightweight melee archetype used by selected room layouts. It keeps
    /// pressure on the player while leaving the normal target stationary.
    /// </summary>
    public sealed class EnemyChaser : MonoBehaviour
    {
        [SerializeField] private float speed = 1.35f;
        private Rigidbody2D body;
        private Transform target;
        private StatusEffectController status;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            target = FindAnyObjectByType<PlayerMovement>()?.transform;
            status = GetComponent<StatusEffectController>();
        }

        private void FixedUpdate()
        {
            if (status == null) status = GetComponent<StatusEffectController>();
            if (body == null || target == null) return;
            var direction = (Vector2)(target.position - transform.position);
            if (direction.sqrMagnitude < 0.16f) return;
            var speedMultiplier = status == null ? 1f : status.MoveSpeedMultiplier;
            body.MovePosition(body.position + direction.normalized * speed * speedMultiplier * Time.fixedDeltaTime);
        }
    }
}
