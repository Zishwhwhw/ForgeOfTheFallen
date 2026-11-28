using FotF.Data;
using FotF.View.Combat;
using UnityEngine;

namespace FotF.View.Enemies
{
    /// <summary>
    /// Стрелок (скелет-лучник, GDD §7.4): держит дистанцию, перед выстрелом — прицельная линия (телеграф п.86),
    /// направление фиксируется в конце замаха. Химера «лучник + культист»: попадание лечит и даёт щит врагам рядом с целью.
    /// Числа: special.values (keepDistance, shootRange, projectileSpeed, aimLockSeconds; healOnHit, shieldOnHit, healRadius).
    /// </summary>
    public sealed class RangedBehaviour : EnemyBehaviour
    {
        [SerializeField] ProjectileView projectilePrefab;
        [Tooltip("Прицельная линия: спрайт, растянутый по X")]
        [SerializeField] SpriteRenderer aimLine;
        [Tooltip("Химера лучник + культист")]
        [SerializeField] bool healsAlliesOnHit;

        enum Phase { Move, Aim, Recover }
        Phase _phase;
        float _t, _cooldown = 1f;
        Vector2 _dir = Vector2.down;
        float _phaseOffset;

        protected override void Awake() { base.Awake(); _phaseOffset = Random.value * 10f; }

        public override bool Tick(float dt, out Vector2 wish)
        {
            wish = Vector2.zero;
            var to = ToTarget(out float dist);
            if (dist == float.MaxValue) { SetLine(false); Face(Vector2.down, "idle"); return true; }
            float keep = Special("keepDistance", 5f), range = Special("shootRange", 8f);
            _cooldown -= dt;
            switch (_phase)
            {
                case Phase.Move:
                    var dir = to / dist;
                    if (dist < keep - 1f) wish = -dir * Owner.MoveSpeed;                   // отходит
                    else if (dist > range) wish = dir * Owner.MoveSpeed;                  // подходит на выстрел
                    else wish = new Vector2(-dir.y, dir.x) * (Owner.MoveSpeed * 0.35f) * Mathf.Sign(Mathf.Sin(Time.time * 0.7f + _phaseOffset)); // смещается вбок
                    Face(wish.sqrMagnitude > 0.01f ? wish : to, wish.sqrMagnitude > 0.01f ? "walk" : "idle");
                    if (dist <= range && _cooldown <= 0f) { _phase = Phase.Aim; _t = 0f; }
                    break;
                case Phase.Aim:
                    _t += dt;
                    float windup = Mathf.Max(0.4f, Owner.Definition.windupSeconds);
                    if (_t < windup - Special("aimLockSeconds", 0.2f)) _dir = to / dist;   // линия следит, в конце — фиксируется
                    SetLine(true);
                    Face(_dir, "windup");
                    if (_t >= windup) { Shoot(); SetLine(false); _phase = Phase.Recover; _t = 0f; }
                    break;
                case Phase.Recover:
                    _t += dt;
                    Face(_dir, "attack");
                    if (_t >= Owner.Definition.recoverSeconds) { _phase = Phase.Move; _cooldown = 0.6f; }
                    break;
            }
            return true;
        }

        void SetLine(bool on)
        {
            if (!aimLine) return;
            aimLine.enabled = on;
            if (on) aimLine.transform.right = _dir;
        }

        void Shoot()
        {
            if (!projectilePrefab) return;
            var p = Instantiate(projectilePrefab, transform.position + Vector3.up * 0.6f, Quaternion.identity);
            var mods = new ProjectileMods();
            if (healsAlliesOnHit) mods.OnImpact = HealAround;
            p.Launch(_dir, Special("projectileSpeed", 9f), 2f, new HitInfo(Owner.Definition.attackDamage, false, _dir, 0f, Team.Enemy), mods);
        }

        void HealAround(Vector3 at)
        {
            float r = Special("healRadius", 3f);
            int heal = (int)Special("healOnHit", 8f), shield = (int)Special("shieldOnHit", 6f);
            foreach (var e in Owner.transform.parent ? Owner.transform.parent.GetComponentsInChildren<EnemyController>() : new EnemyController[0])
                if (!e.State.IsDead && ((Vector2)(e.transform.position - at)).sqrMagnitude <= r * r)
                {
                    e.Heal(heal);
                    e.State.Shield = Mathf.Max(e.State.Shield, shield);
                }
        }

        public override void OnHit(in HitInfo hit) { }
        public override void OnDisabled() { SetLine(false); if (_phase == Phase.Aim) { _phase = Phase.Move; _cooldown = 0.5f; } }
        public override void OnDeath() => SetLine(false);
    }
}
