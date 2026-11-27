using System;
using FotF.Data;
using FotF.Logic;
using FotF.Logic.Combat;
using FotF.State;
using FotF.View.Animation;
using FotF.View.Combat;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FotF.View.Heroes
{
    /// <summary>
    /// Герой на сцене. Input → команды в Logic (A5); View только исполняет движение и показывает состояние (A4).
    /// Все числа берутся из HeroDefinition / WeaponDefinition (A2).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class HeroController : MonoBehaviour, IDamageable
    {
        [Header("Данные")]
        [SerializeField] HeroDefinition hero;
        [Tooltip("Второе оружие для серой коробки (в забеге приходит из сундука)")]
        [SerializeField] WeaponDefinition secondWeapon;

        [Header("Ввод")]
        [SerializeField] InputActionAsset controls;

        [Header("Вид")]
        [SerializeField] DirectionalSpriteAnimator body;
        [SerializeField] WeaponView weapon;
        [SerializeField] Camera worldCamera;
        [SerializeField] SpriteFlash flash;
        [Tooltip("Радиус взаимодействия клавишей E, юниты")]
        [SerializeField] float interactRadius = 1.4f;
        [SerializeField] int seed = 12345;

        public HeroState State { get; } = new HeroState();
        public event Action<HitInfo, DamageResult> Damaged;
        public event Action Died;
        public Team Team => Team.Player;

        Rigidbody2D _rb;
        InputAction _move, _aim, _fire, _dash, _swap, _interact;
        readonly System.Collections.Generic.List<Collider2D> _near = new System.Collections.Generic.List<Collider2D>();

        /// <summary>Ближайший объект для E (для подсказки в HUD).</summary>
        public World.IInteractable Focus { get; private set; }
        VitalsConfig _vitals;
        DashConfig _dashCfg;
        IRandom _rng;
        Vector2 _moveInput, _aimDir = Vector2.down, _dashDir;

        public Vector2 AimDirection => _aimDir;
        public Vector2 AimWorldPoint { get; private set; }

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rng = new SeededRandom(seed);
            _vitals = new VitalsConfig(hero.maxHp, hero.maxArmor, hero.maxMana, hero.armorRegenDelay, hero.armorRegenPerSecond);
            _dashCfg = new DashConfig(hero.dashCharges, hero.dashCooldown, hero.dashDuration, hero.dashInvulnerability);
            Vitals.Init(State, _vitals);
            Dash.Init(State, _dashCfg);
            SetSlot(0, WeaponFactory.Create(hero.startWeapon));
            SetSlot(1, WeaponFactory.Create(secondWeapon));
            if (worldCamera == null) worldCamera = Camera.main;

            var map = controls.FindActionMap("Player", true);
            _move = map.FindAction("Move", true); _aim = map.FindAction("Aim", true);
            _fire = map.FindAction("Fire", true); _dash = map.FindAction("Dash", true); _swap = map.FindAction("Swap", true);
            _interact = map.FindAction("Interact", true);
        }

        void OnEnable() { controls.FindActionMap("Player").Enable(); }
        void OnDisable() { controls.FindActionMap("Player").Disable(); }

        /// <summary>Положить экземпляр в слот (null — очистить).</summary>
        public void SetSlot(int slot, WeaponInstance w)
        {
            State.Weapons[slot].Instance = w;
            State.Weapons[slot].WeaponId = w?.DefinitionId;
            State.Weapons[slot].CooldownLeft = 0f;
            if (slot == State.ActiveSlot) weapon.Show(w);
        }

        /// <summary>Подобрать с пола (GDD §5.1): в пустой слот, иначе текущее оружие возвращается вызывающему (упадёт на пол).</summary>
        public WeaponInstance PickUp(WeaponInstance w)
        {
            int empty = State.Weapons[0].IsEmpty ? 0 : State.Weapons[1].IsEmpty ? 1 : -1;
            if (empty >= 0) { SetSlot(empty, w); State.ActiveSlot = empty; weapon.Show(w); return null; }
            var dropped = State.ActiveWeapon.Instance;
            SetSlot(State.ActiveSlot, w);
            return dropped;
        }

        /// <summary>Отдать оружие из рук (на плиту кузницы). Слот пустеет; активным становится другой, если он не пуст.</summary>
        public WeaponInstance TakeActiveWeapon()
        {
            var w = State.ActiveWeapon.Instance;
            if (w == null) return null;
            SetSlot(State.ActiveSlot, null);
            if (!State.Weapons[1 - State.ActiveSlot].IsEmpty) { State.ActiveSlot = 1 - State.ActiveSlot; weapon.Show(State.ActiveWeapon.Instance); }
            return w;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            Vitals.Tick(State, _vitals, dt);
            Dash.Tick(State, _dashCfg, dt);
            foreach (var s in State.Weapons) WeaponAttack.Tick(s, dt);
            if (State.IsDead)
            {
                // GDD §3.1: быстрый перезапуск; в срезе — панель итогов и «В убежище», здесь — рестарт сцены
                if (_interact.WasPressedThisFrame())
                    UnityEngine.SceneManagement.SceneManager.LoadScene(gameObject.scene.buildIndex >= 0 ? gameObject.scene.buildIndex : 0);
                return;
            }

            Focus = FindInteractable();
            if (Focus != null && _interact.WasPressedThisFrame()) Focus.Interact(this);

            _moveInput = Vector2.ClampMagnitude(_move.ReadValue<Vector2>(), 1f);
            Vector2 screen = _aim.ReadValue<Vector2>();
            AimWorldPoint = worldCamera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -worldCamera.transform.position.z));
            Vector2 toAim = AimWorldPoint - (Vector2)transform.position;
            if (toAim.sqrMagnitude > 0.01f) _aimDir = toAim.normalized;
            body.SetFacing(_aimDir);
            weapon.SetHand(HandLocal(), body.CurrentFacing == DirectionalSpriteAnimator.Facing.Up);
            weapon.SetBob(body.CurrentBobPixels);
            weapon.Aim(_aimDir);

            if (_swap.WasPressedThisFrame())
            {
                WeaponAttack.Swap(State);
                weapon.Show(State.ActiveWeapon.Instance);
            }
            if (_dash.WasPressedThisFrame() && Dash.TryStart(State, _dashCfg))
                _dashDir = _moveInput.sqrMagnitude > 0.01f ? _moveInput.normalized : _aimDir;

            var w = State.ActiveWeapon.Instance;
            if (w != null && _fire.IsPressed() && !State.IsDashing)
            {
                var stats = new WeaponStats(w.Damage, w.AttacksPerSecond, w.ManaCost, w.CritChance);
                if (WeaponAttack.TryAttack(State, State.ActiveWeapon, stats) == WeaponAttack.Result.Fired)
                    weapon.Attack(_aimDir, Team, _rng);
            }

            body.Play(State.IsDashing ? "dash" : _moveInput.sqrMagnitude > 0.01f ? "run" : "idle");
        }

        void FixedUpdate()
        {
            if (State.IsDead) { _rb.linearVelocity = Vector2.zero; return; }
            _rb.linearVelocity = State.IsDashing
                ? _dashDir * (hero.dashDistance / Mathf.Max(0.01f, hero.dashDuration))
                : _moveInput * hero.moveSpeed;
        }

        public void TakeHit(in HitInfo hit)
        {
            if (State.IsDead) return;
            var result = Vitals.ApplyDamage(State, hit.Amount, _vitals);
            if (!result.Blocked && flash) flash.Flash();
            Damaged?.Invoke(hit, result);
            if (result.Killed) { body.Play("idle", true); Died?.Invoke(); }
        }

        /// <summary>Точка руки с оружием по направлению взгляда (HeroDefinition, пиксели → юниты; влево — зеркало бока).</summary>
        Vector2 HandLocal()
        {
            const float ppu = 32f;
            switch (body.CurrentFacing)
            {
                case DirectionalSpriteAnimator.Facing.Up: return hero.handNorthPixels / ppu;
                case DirectionalSpriteAnimator.Facing.Side:
                    var p = hero.handSidePixels / ppu;
                    return body.IsFlipped ? new Vector2(-p.x, p.y) : p;
                default: return hero.handSouthPixels / ppu;
            }
        }

        public void AddGold(int amount) => State.Gold += Mathf.Max(0, amount);

        World.IInteractable FindInteractable()
        {
            var filter = new ContactFilter2D { useTriggers = true };
            Physics2D.OverlapCircle(transform.position, interactRadius, filter, _near);
            World.IInteractable best = null; float bestD = float.MaxValue;
            foreach (var c in _near)
            {
                var it = c.GetComponentInParent<World.IInteractable>();
                if (it == null || !it.CanInteract) continue;
                float d = (it.Position - transform.position).sqrMagnitude;
                if (d < bestD) { bestD = d; best = it; }
            }
            return best;
        }
    }
}
