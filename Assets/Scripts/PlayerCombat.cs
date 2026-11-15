using UnityEngine;
using UnityEngine.InputSystem;

namespace FrankenArms
{
    public sealed class PlayerCombat : MonoBehaviour
    {
        [SerializeField] private float projectileSpeed = 9f;
        [SerializeField] private float fireCooldown = 0.18f;
        [SerializeField] private int damage = 1;
        private float cooldown;
        private PlayerMana mana;
        private bool usesMana;
        private const int ManaPerShot = 1;

        internal int Damage { get { return damage; } }
        internal float ShotsPerSecond { get { return 1f / Mathf.Max(0.01f, fireCooldown); } }

        private void Update()
        {
            if (Time.timeScale <= 0f) return;
            if (mana == null) mana = GetComponent<PlayerMana>();
            cooldown -= Time.deltaTime;
            var mouse = Mouse.current;
            var keyboard = Keyboard.current;
            var gamepad = Gamepad.current;
            var wantsToFire = (mouse != null && mouse.leftButton.isPressed)
                || (keyboard != null && keyboard.spaceKey.isPressed)
                || (gamepad != null && gamepad.rightTrigger.isPressed);
            if (wantsToFire && cooldown <= 0f)
            {
                if (!usesMana || mana == null || mana.TrySpend(ManaPerShot))
                {
                    Fire(GetAimDirection());
                    GameAudio.Play("Shot", 0.45f);
                }
                cooldown = fireCooldown;
            }
        }

        private Vector2 GetAimDirection()
        {
            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                var stick = gamepad.rightStick.ReadValue();
                if (stick.sqrMagnitude > 0.09f) return stick.normalized;
            }
            var mouse = Mouse.current;
            if (mouse == null || Camera.main == null) return Vector2.right;
            var screen = mouse.position.ReadValue();
            var world = Camera.main.ScreenToWorldPoint(new Vector3(screen.x, screen.y, 10f));
            var direction = (Vector2)(world - transform.position);
            return direction.sqrMagnitude < 0.01f ? Vector2.right : direction.normalized;
        }

        private void Fire(Vector2 direction)
        {
            var projectileObject = new GameObject("Projectile");
            projectileObject.transform.position = transform.position + (Vector3)(direction * 0.55f);
            var renderer = projectileObject.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeSpriteFactory.CreateProjectile();
            renderer.color = new Color(1f, 0.75f, 0.25f, 1f);
            renderer.sortingOrder = 12;
            // Trigger callbacks require at least one Rigidbody2D on the pair.
            // Projectiles are moved manually, so use a kinematic body and continuous detection.
            var body = projectileObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var collider = projectileObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.12f;
            var shotDamage = damage;
            var critical = usesMana && Random.value < 0.12f;
            if (critical) shotDamage *= 2;
            projectileObject.AddComponent<Projectile>().Launch(direction, projectileSpeed, shotDamage);
            if (critical) GameAudio.Play("Explosion", 0.35f);
            var flash = new GameObject("Muzzle Flash");
            flash.transform.position = projectileObject.transform.position;
            var flashRenderer = flash.AddComponent<SpriteRenderer>();
            flashRenderer.sprite = RuntimeSpriteFactory.CreateProjectile();
            flashRenderer.sortingOrder = 13;
            flash.AddComponent<ImpactBurst>().Initialize(new Color(1f, 0.85f, 0.3f, 1f), 0.12f, 0.45f);
        }

        internal void ApplyFusion(int fusedDamage, float shotsPerSecond, float fusedProjectileSpeed)
        {
            damage = Mathf.Max(1, fusedDamage);
            projectileSpeed = Mathf.Max(1f, fusedProjectileSpeed);
            fireCooldown = 1f / Mathf.Max(0.5f, shotsPerSecond);
            usesMana = true;
        }

        internal void ConfigureBaseWeapon(int baseDamage, float shotsPerSecond)
        {
            damage = Mathf.Max(1, baseDamage);
            fireCooldown = 1f / Mathf.Max(0.5f, shotsPerSecond);
            usesMana = false;
        }

        internal bool UsesMana { get { return usesMana; } }
    }
}
