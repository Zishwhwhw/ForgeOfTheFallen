using System;
using System.Collections.Generic;
using FotF.Data;
using FotF.Logic.Combat;
using FotF.State;
using FotF.View.Animation;
using FotF.View.Combat;
using FotF.View.Heroes;
using UnityEngine;

namespace FotF.View.Enemies
{
    /// <summary>
    /// Враг ближнего боя на сцене. Решения — в EnemyBrain (Logic); здесь движение, телеграф, удар, отклик.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class EnemyController : MonoBehaviour, IDamageable
    {
        [SerializeField] EnemyDefinition definition;
        [SerializeField] DirectionalSpriteAnimator body;
        [SerializeField] SpriteFlash flash;
        [Tooltip("Телеграф замаха: свечение оружия / дуга (GDD п.86). Включается на время замаха")]
        [SerializeField] SpriteRenderer telegraph;
        [SerializeField] DamageNumberView damageNumberPrefab;
        [SerializeField] Vector3 numberOffset = new Vector3(0f, 1f, 0f);
        [SerializeField] Collider2D bodyCollider;
        [SerializeField] EnemyDeathView deathView;
        [SerializeField] float knockbackSpeed = 3f;
        [SerializeField] float knockbackSeconds = 0.08f;

        public EnemyDefinition Definition => definition;
        public EnemyState State { get; } = new EnemyState();
        public Team Team => Team.Enemy;
        public event Action<EnemyController> Died;

        Rigidbody2D _rb;
        HeroController _target;
        EnemyConfig _cfg;
        Vector2 _strikeDir = Vector2.down, _knockback;
        float _knockbackLeft;
        readonly List<Collider2D> _hits = new List<Collider2D>();

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _cfg = new EnemyConfig(definition.maxHp, definition.attackRange, definition.aggroRange, definition.windupSeconds, definition.recoverSeconds);
            EnemyBrain.Init(State, _cfg);
            if (telegraph) telegraph.enabled = false;
        }

        public void SetTarget(HeroController hero) => _target = hero;

        void Update()
        {
            if (State.IsDead) return;
            if (_target == null) _target = FindFirstObjectByType<HeroController>();
            Vector2 to = _target ? (Vector2)_target.transform.position - (Vector2)transform.position : Vector2.zero;
            float dist = _target && !_target.State.IsDead ? to.magnitude : float.MaxValue;

            var before = State.Phase;
            var intent = EnemyBrain.Tick(State, _cfg, Time.deltaTime, dist);
            if (before != EnemyPhase.Windup && State.Phase == EnemyPhase.Windup && to.sqrMagnitude > 0.0001f) _strikeDir = to.normalized; // направление фиксируется в начале замаха
            if (intent == EnemyIntent.Strike) Strike();

            bool winding = State.Phase == EnemyPhase.Windup;
            if (telegraph)
            {
                telegraph.enabled = winding;
                if (winding) telegraph.transform.right = _strikeDir;
            }
            if (body)
            {
                body.SetFacing(winding || State.Phase == EnemyPhase.Recover ? _strikeDir : (intent == EnemyIntent.MoveToTarget ? to : Vector2.zero));
                body.Play(winding ? "windup" : State.Phase == EnemyPhase.Recover ? "attack" : intent == EnemyIntent.MoveToTarget ? "walk" : "idle");
            }
            _desired = intent == EnemyIntent.MoveToTarget && dist > 0.01f ? to / dist * definition.moveSpeed : Vector2.zero;
        }

        Vector2 _desired;
        void FixedUpdate()
        {
            if (State.IsDead) { _rb.linearVelocity = Vector2.zero; return; }
            if (_knockbackLeft > 0f) { _knockbackLeft -= Time.fixedDeltaTime; _rb.linearVelocity = _knockback; return; }
            _rb.linearVelocity = _desired;
        }

        void Strike()
        {
            var filter = new ContactFilter2D { useTriggers = false, useLayerMask = true, layerMask = 1 << LayerMask.NameToLayer("Hero") };
            Physics2D.OverlapCircle(transform.position, definition.strikeRadius, filter, _hits);
            foreach (var c in _hits)
            {
                var hero = c.GetComponentInParent<HeroController>();
                if (hero == null) continue;
                Vector2 to = (Vector2)hero.transform.position - (Vector2)transform.position;
                if (Vector2.Angle(_strikeDir, to) > definition.strikeArcDegrees * 0.5f) continue;
                hero.TakeHit(new HitInfo(definition.attackDamage, false, to.normalized, 0f, Team.Enemy));
                break;
            }
        }

        public void TakeHit(in HitInfo hit)
        {
            if (State.IsDead) return;
            bool died = EnemyBrain.ApplyDamage(State, hit.Amount);
            if (damageNumberPrefab)
                Instantiate(damageNumberPrefab, transform.position + numberOffset + (Vector3)(UnityEngine.Random.insideUnitCircle * 0.2f), Quaternion.identity).Show(hit.Amount, hit.IsCrit);
            if (flash) flash.Flash();
            if (died) { Die(); return; }
            if (hit.StunSeconds > 0f) EnemyBrain.Stun(State, hit.StunSeconds);
            _knockback = hit.Direction * knockbackSpeed; _knockbackLeft = knockbackSeconds;
        }

        void Die()
        {
            if (telegraph) telegraph.enabled = false;
            if (bodyCollider) bodyCollider.enabled = false;
            _rb.linearVelocity = Vector2.zero; _rb.simulated = false;
            if (deathView) deathView.Play();          // GDD §10.2: мерцание → сплющивание → труп
            else if (body) body.Play("death", true);
            HitStop.Request(deathView && deathView.Feel ? deathView.Feel.killMicroPauseSeconds : 0.025f); // микропауза на убийстве
            Died?.Invoke(this);                       // труп остаётся до конца этажа (GDD §10.2)
        }

        void OnDrawGizmosSelected()
        {
            if (!definition) return;
            Gizmos.color = new Color(1f, 0f, 0.27f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, definition.strikeRadius);
        }
    }
}
