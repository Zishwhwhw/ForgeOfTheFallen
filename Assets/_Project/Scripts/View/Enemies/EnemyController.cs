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
        [Tooltip("Статусы стихий (GDD §7.3)")]
        [SerializeField] StatusCatalog statuses;
        [Header("Дроп (GDD §7.9): золото и сферы маны")]
        [SerializeField] World.LootPickupView coinPrefab;
        [SerializeField] World.LootPickupView manaOrbPrefab;
        [SerializeField] BalanceConstants constants;
        [SerializeField] float knockbackSpeed = 3f;
        [SerializeField] float knockbackSeconds = 0.08f;
        [Tooltip("Особое поведение (лучник, щитоносец, слизень…). Пусто — ближний бой по EnemyBrain")]
        [SerializeField] EnemyBehaviour behaviour;

        public EnemyDefinition Definition => definition;
        public EnemyState State { get; } = new EnemyState();
        public Team Team => Team.Enemy;
        public event Action<EnemyController> Died;
        /// <summary>Враг породил другого (деление слизня, призыв): комната должна его учесть.</summary>
        public event Action<EnemyController> Spawned;

        public HeroController Target => _target;
        public DirectionalSpriteAnimator Body => body;
        public SpriteRenderer Telegraph => telegraph;
        public Rigidbody2D Rigidbody => _rb;
        /// <summary>Скорость с учётом замедления статусом.</summary>
        public float MoveSpeed => definition.moveSpeed * (1f - _slow);
        public EnemyBehaviour Behaviour => behaviour;

        /// <summary>Переопределить HP (осколки слизня, химеры).</summary>
        public void OverrideMaxHp(int hp) { State.MaxHp = State.Hp = Mathf.Max(1, hp); }

        public void Heal(int amount) { if (!State.IsDead) State.Hp = Mathf.Min(State.MaxHp, State.Hp + Mathf.Max(0, amount)); }

        public EnemyController SpawnChild(EnemyController prefab, Vector3 at)
        {
            var child = Instantiate(prefab, at, Quaternion.identity, transform.parent);
            child.SetTarget(_target);
            Spawned?.Invoke(child);
            return child;
        }

        /// <summary>Удар по герою (ближний или особый).</summary>
        public void HitHero(HeroController hero, int damage, Vector2 dir, Element[] elements = null, float statusChance = 0f)
        {
            if (hero) hero.TakeHit(new HitInfo(damage, false, dir, 0f, Team.Enemy, elements, statusChance, 0f));
        }

        Rigidbody2D _rb;
        HeroController _target;
        EnemyConfig _cfg;
        Vector2 _strikeDir = Vector2.down, _knockback;
        float _knockbackLeft;
        readonly List<Collider2D> _hits = new List<Collider2D>();

        FotF.Logic.Status.StatusConfig[] _statusConfigs;
        readonly FotF.Logic.SeededRandom _statusRng = new FotF.Logic.SeededRandom(System.Environment.TickCount);
        bool _disabled; float _slow; Color _baseColor = Color.white; SpriteRenderer _bodyRenderer;

        void Awake()
        {
            _statusConfigs = StatusConfigs.From(statuses);
            if (body) { _bodyRenderer = body.GetComponent<SpriteRenderer>(); if (_bodyRenderer) _baseColor = _bodyRenderer.color; }
            _rb = GetComponent<Rigidbody2D>();
            _cfg = new EnemyConfig(definition.maxHp, definition.attackRange, definition.aggroRange, definition.windupSeconds, definition.recoverSeconds);
            EnemyBrain.Init(State, _cfg);
            State.IsBoss = definition.tier == EnemyTier.MiniBoss;   // мини-босс сопротивляется статусам
            if (telegraph) telegraph.enabled = false;
        }

        public void SetTarget(HeroController hero) => _target = hero;

        void Update()
        {
            if (State.IsDead) return;
            TickStatuses(Time.deltaTime);
            if (State.IsDead) return;
            if (_disabled) { if (telegraph) telegraph.enabled = false; _desired = Vector2.zero; if (behaviour) behaviour.OnDisabled(); return; }
            if (_target == null) _target = FindAnyObjectByType<HeroController>();
            if (behaviour && behaviour.Tick(Time.deltaTime, out var wish)) { _desired = wish; return; }
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
            _desired = intent == EnemyIntent.MoveToTarget && dist > 0.01f ? to / dist * definition.moveSpeed * (1f - _slow) : Vector2.zero;
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
                float poison = definition.special != null ? definition.special.values.Number("poisonChance", 0f) : 0f;   // крыса: яд 20%
                hero.TakeHit(new HitInfo(definition.attackDamage, false, to.normalized, 0f, Team.Enemy,
                    poison > 0f ? new[] { Element.Poison } : null, poison, 0f));
                break;
            }
        }

        public void TakeHit(in HitInfo hit)
        {
            if (State.IsDead) return;
            if (behaviour && behaviour.Blocks(hit)) { behaviour.OnBlocked(hit); return; }
            int amount = hit.Amount;
            if (State.Shield > 0) { int absorbed = Mathf.Min(State.Shield, amount); State.Shield -= absorbed; amount -= absorbed; }
            if (behaviour) behaviour.OnHit(hit);
            bool died = amount > 0 && EnemyBrain.ApplyDamage(State, amount);
            if (damageNumberPrefab)
                Instantiate(damageNumberPrefab, transform.position + numberOffset + (Vector3)(UnityEngine.Random.insideUnitCircle * 0.2f), Quaternion.identity).Show(amount, hit.IsCrit);
            if (flash) flash.Flash();
            if (died) { Die(); return; }
            ApplyStatuses(hit);
            if (hit.StunSeconds > 0f) EnemyBrain.Stun(State, hit.StunSeconds);
            _knockback = hit.Direction * (hit.Knockback > 0f ? hit.Knockback : knockbackSpeed); _knockbackLeft = knockbackSeconds;
        }

        void Die()
        {
            if (telegraph) telegraph.enabled = false;
            if (bodyCollider) bodyCollider.enabled = false;
            _rb.linearVelocity = Vector2.zero; _rb.simulated = false;
            if (deathView) deathView.Play();          // GDD §10.2: мерцание → сплющивание → труп
            else if (body) body.Play("death", true);
            HitStop.Request(deathView && deathView.Feel ? deathView.Feel.killMicroPauseSeconds : 0.025f); // микропауза на убийстве
            if (behaviour) behaviour.OnDeath();
            DropLoot();
            Died?.Invoke(this);                       // труп остаётся до конца этажа (GDD §10.2)
        }

        void ApplyStatuses(in HitInfo hit)
        {
            if (hit.Elements == null || hit.StatusChance <= 0f || _statusConfigs == null) return;
            foreach (var e in hit.Elements)
                if (FotF.Logic.Status.StatusEffects.Exists(_statusConfigs, e) && _statusRng.NextFloat() < hit.StatusChance)
                    FotF.Logic.Status.StatusEffects.Apply(State, FotF.Logic.Status.StatusEffects.ConfigFor(_statusConfigs, e), _statusRng);
        }

        void TickStatuses(float dt)
        {
            if (State.Statuses.Count == 0) { if (_disabled || _slow > 0f) { _disabled = false; _slow = 0f; Tint(); } return; }
            var t = FotF.Logic.Status.StatusEffects.Tick(State, _statusConfigs, dt);
            _disabled = t.Disabled; _slow = t.SlowFraction;
            if (t.Damage > 0)
            {
                if (damageNumberPrefab) Instantiate(damageNumberPrefab, transform.position + numberOffset, Quaternion.identity).Show(t.Damage, false);
                if (EnemyBrain.ApplyDamage(State, t.Damage)) { Die(); return; }
            }
            Tint();
        }

        /// <summary>Цвет статуса на теле (до иконок статусов): огонь — оранжевый, холод — голубой, шок — жёлтый, яд — зелёный.</summary>
        void Tint()
        {
            if (!_bodyRenderer || State.IsDead) return;
            Color c = _baseColor;
            foreach (var st in State.Statuses)
                c = st.Element switch
                {
                    Element.Fire => new Color32(0xfe, 0xae, 0x34, 0xff),
                    Element.Ice => st.Frozen ? new Color32(0x2c, 0xe8, 0xf5, 0xff) : new Color32(0x9e, 0xd8, 0xff, 0xff),
                    Element.Lightning => new Color32(0xfe, 0xe7, 0x61, 0xff),
                    Element.Poison => new Color32(0x63, 0xc7, 0x4d, 0xff),
                    _ => c,
                };
            _bodyRenderer.color = c;
        }

        void DropLoot()
        {
            var parent = transform.parent;
            if (behaviour && !behaviour.DropsLoot) return;
            if (coinPrefab)
            {
                int gold = UnityEngine.Random.Range(definition.goldMin, definition.goldMax + 1);
                if (gold > 0) Instantiate(coinPrefab, transform.position + (Vector3)(UnityEngine.Random.insideUnitCircle * 0.3f), Quaternion.identity, parent).InitAmount(gold);
            }
            if (manaOrbPrefab && constants)
            {
                var d = constants.drops;
                int orbs = UnityEngine.Random.Range(d.Int("manaOrbMin", 1), d.Int("manaOrbMax", 2) + 1);
                for (int i = 0; i < orbs; i++)
                    Instantiate(manaOrbPrefab, transform.position + (Vector3)(UnityEngine.Random.insideUnitCircle * 0.5f), Quaternion.identity, parent).InitAmount(d.Int("manaPerOrb", 6));
            }
        }

        void OnDrawGizmosSelected()
        {
            if (!definition) return;
            Gizmos.color = new Color(1f, 0f, 0.27f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, definition.strikeRadius);
        }
    }
}
