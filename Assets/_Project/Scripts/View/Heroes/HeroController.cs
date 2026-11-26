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
        [SerializeField] int seed = 12345;

        public HeroState State { get; } = new HeroState();
        public event Action<HitInfo, DamageResult> Damaged;
        public event Action Died;
        public Team Team => Team.Player;

        Rigidbody2D _rb;
        InputAction _move, _aim, _fire, _dash, _swap;
        VitalsConfig _vitals;
        DashConfig _dashCfg;
        IRandom _rng;
        readonly WeaponDefinition[] _slots = new WeaponDefinition[2];
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
            Equip(0, hero.startWeapon);
            Equip(1, secondWeapon);
            if (worldCamera == null) worldCamera = Camera.main;

            var map = controls.FindActionMap("Player", true);
            _move = map.FindAction("Move", true); _aim = map.FindAction("Aim", true);
            _fire = map.FindAction("Fire", true); _dash = map.FindAction("Dash", true); _swap = map.FindAction("Swap", true);
        }

        void OnEnable() { controls.FindActionMap("Player").Enable(); }
        void OnDisable() { controls.FindActionMap("Player").Disable(); }

        public void Equip(int slot, WeaponDefinition def)
        {
            _slots[slot] = def;
            State.Weapons[slot].WeaponId = def ? def.id : null;
            State.Weapons[slot].CooldownLeft = 0f;
            if (slot == State.ActiveSlot) weapon.Show(def);
        }

        void Update()
        {
            float dt = Time.deltaTime;
            Vitals.Tick(State, _vitals, dt);
            Dash.Tick(State, _dashCfg, dt);
            foreach (var s in State.Weapons) WeaponAttack.Tick(s, dt);
            if (State.IsDead) return;

            _moveInput = Vector2.ClampMagnitude(_move.ReadValue<Vector2>(), 1f);
            Vector2 screen = _aim.ReadValue<Vector2>();
            AimWorldPoint = worldCamera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -worldCamera.transform.position.z));
            Vector2 toAim = AimWorldPoint - (Vector2)transform.position;
            if (toAim.sqrMagnitude > 0.01f) _aimDir = toAim.normalized;
            weapon.Aim(_aimDir);

            if (_swap.WasPressedThisFrame())
            {
                WeaponAttack.Swap(State);
                weapon.Show(_slots[State.ActiveSlot]);
            }
            if (_dash.WasPressedThisFrame() && Dash.TryStart(State, _dashCfg))
                _dashDir = _moveInput.sqrMagnitude > 0.01f ? _moveInput.normalized : _aimDir;

            var def = _slots[State.ActiveSlot];
            if (def != null && _fire.IsPressed() && !State.IsDashing)
            {
                var stats = new WeaponStats(def.damage, def.attacksPerSecond, def.manaCost, def.critChance);
                if (WeaponAttack.TryAttack(State, State.ActiveWeapon, stats) == WeaponAttack.Result.Fired)
                    weapon.Attack(_aimDir, Team, _rng);
            }

            body.SetFacing(_aimDir);
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
            var result = Vitals.ApplyDamage(State, hit.Amount, _vitals);
            Damaged?.Invoke(hit, result);
            if (result.Killed) Died?.Invoke();
        }
    }
}
