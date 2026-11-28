using System.Collections.Generic;
using UnityEngine;

namespace FotF.View.Combat
{
    /// <summary>Модификаторы снаряда от особенностей оружия (GDD §7.1).</summary>
    public struct ProjectileMods
    {
        public int Pierce;                   // сколько врагов пробить насквозь
        public float HomingDegreesPerSecond; // самонаводка
        public float ExplosionRadius;        // взрыв по площади при попадании/стене
        public int ChainTargets;             // цепная молния: всего целей
        public float ChainRange;
        public float Knockback;              // отбрасывание попаданием
        public float KnockbackMaxDistance;   // 0 — всегда; иначе только вблизи (обрез)
    }

    /// <summary>Снаряд: летит прямо (или доворачивает к врагу), попадает в IDamageable другой команды, гаснет о стену или по времени.</summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public sealed class ProjectileView : MonoBehaviour
    {
        [SerializeField] LayerMask wallMask;
        [Tooltip("Слои целей для взрыва, цепи и самонаводки")]
        [SerializeField] LayerMask targetMask;

        Rigidbody2D _body;
        HitInfo _hit;
        ProjectileMods _mods;
        float _lifeLeft, _speed;
        Vector3 _origin;
        readonly HashSet<IDamageable> _hitAlready = new HashSet<IDamageable>();
        static readonly List<Collider2D> Near = new List<Collider2D>();

        void Awake() { _body = GetComponent<Rigidbody2D>(); }

        public void Launch(Vector2 direction, float speed, float lifetime, in HitInfo hit) => Launch(direction, speed, lifetime, hit, default);

        public void Launch(Vector2 direction, float speed, float lifetime, in HitInfo hit, in ProjectileMods mods)
        {
            _hit = hit; _mods = mods; _lifeLeft = lifetime; _speed = speed; _origin = transform.position;
            transform.right = direction;
            _body.linearVelocity = direction.normalized * speed;
        }

        void Update()
        {
            _lifeLeft -= Time.deltaTime;
            if (_lifeLeft <= 0f) { Explode(); Destroy(gameObject); return; }
            if (_mods.HomingDegreesPerSecond > 0f) Steer();
        }

        void Steer()
        {
            var target = Nearest(transform.position, 6f, null);
            if (target == null) return;
            Vector2 want = ((Vector2)((Component)target).transform.position - (Vector2)transform.position).normalized;
            Vector2 now = _body.linearVelocity.normalized;
            float angle = Vector2.SignedAngle(now, want);
            float step = Mathf.Clamp(angle, -_mods.HomingDegreesPerSecond * Time.deltaTime, _mods.HomingDegreesPerSecond * Time.deltaTime);
            Vector2 dir = Quaternion.Euler(0, 0, step) * now;
            _body.linearVelocity = dir * _speed;
            transform.right = dir;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (((1 << other.gameObject.layer) & wallMask) != 0) { Explode(); Destroy(gameObject); return; }
            var target = other.GetComponentInParent<IDamageable>();
            if (target == null || target.Team == _hit.Source || !_hitAlready.Add(target)) return;
            bool close = _mods.KnockbackMaxDistance <= 0f || (transform.position - _origin).magnitude <= _mods.KnockbackMaxDistance;
            target.TakeHit(_hit.WithDirection(_body.linearVelocity.normalized).WithKnockback(close ? _mods.Knockback : 0f));
            if (_mods.ChainTargets > 1) Chain(target);
            if (_mods.Pierce > 0) { _mods.Pierce--; return; }
            Explode();
            Destroy(gameObject);
        }

        /// <summary>Цепная молния: прыгает к ближайшим другим врагам в радиусе (посох грозы).</summary>
        void Chain(IDamageable first)
        {
            var hit = new HashSet<IDamageable> { first };
            Vector3 from = ((Component)first).transform.position;
            for (int i = 1; i < _mods.ChainTargets; i++)
            {
                var next = Nearest(from, _mods.ChainRange, hit);
                if (next == null) break;
                hit.Add(next);
                Vector3 to = ((Component)next).transform.position;
                next.TakeHit(_hit.WithDirection((to - from).normalized));
                from = to;
            }
        }

        void Explode()
        {
            if (_mods.ExplosionRadius <= 0f) return;
            float r = _mods.ExplosionRadius; _mods.ExplosionRadius = 0f;
            var filter = new ContactFilter2D { useTriggers = true, useLayerMask = true, layerMask = targetMask };
            Physics2D.OverlapCircle(transform.position, r, filter, Near);
            var done = new HashSet<IDamageable>(_hitAlready);
            foreach (var c in Near)
            {
                var t = c.GetComponentInParent<IDamageable>();
                if (t == null || t.Team == _hit.Source || !done.Add(t)) continue;
                t.TakeHit(_hit.WithDirection((c.transform.position - transform.position).normalized));
            }
        }

        IDamageable Nearest(Vector3 from, float radius, HashSet<IDamageable> skip)
        {
            var filter = new ContactFilter2D { useTriggers = true, useLayerMask = true, layerMask = targetMask };
            Physics2D.OverlapCircle(from, radius, filter, Near);
            IDamageable best = null; float bestD = float.MaxValue;
            foreach (var c in Near)
            {
                var t = c.GetComponentInParent<IDamageable>();
                if (t == null || t.Team == _hit.Source || (skip != null && skip.Contains(t)) || _hitAlready.Contains(t) && skip == null) continue;
                if (t is Enemies.EnemyController e && e.State.IsDead) continue;
                float d = ((Vector2)c.transform.position - (Vector2)from).sqrMagnitude;
                if (d < bestD) { bestD = d; best = t; }
            }
            return best;
        }
    }
}
