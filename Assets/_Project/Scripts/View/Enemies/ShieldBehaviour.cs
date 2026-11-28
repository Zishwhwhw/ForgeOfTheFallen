using FotF.View.Combat;
using UnityEngine;

namespace FotF.View.Enemies
{
    /// <summary>
    /// Скелет-щитоносец (GDD §7.4): щит спереди блокирует снаряды; ближний бой и взрыв проходят. Медленный (moveSpeed в данных).
    /// Ходит и бьёт как мечник (EnemyBrain).
    /// </summary>
    public sealed class ShieldBehaviour : EnemyBehaviour
    {
        [Tooltip("Половина угла щита, градусы")]
        [SerializeField] float shieldHalfAngle = 70f;
        [SerializeField] SpriteRenderer shieldVisual;
        [SerializeField] DamageNumberView blockTextPrefab;

        Vector2 _facing = Vector2.down;

        public override bool Tick(float dt, out Vector2 wish)
        {
            wish = Vector2.zero;
            var to = ToTarget(out float dist);
            if (dist < float.MaxValue && dist > 0.01f) _facing = to / dist;
            if (shieldVisual) { shieldVisual.transform.localPosition = (Vector3)(_facing * 0.45f) + Vector3.up * 0.5f; shieldVisual.sortingOrder = _facing.y > 0f ? -1 : 2; }
            return false;                                     // движение и удар — обычные
        }

        public override bool Blocks(in HitInfo hit) =>
            hit.FromProjectile && Vector2.Angle(_facing, -hit.Direction) <= shieldHalfAngle;

        public override void OnBlocked(in HitInfo hit)
        {
            if (blockTextPrefab) Instantiate(blockTextPrefab, transform.position + Vector3.up, Quaternion.identity).Show(0, false);
        }

        public override void OnDeath() { if (shieldVisual) shieldVisual.enabled = false; }
    }
}
