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

        private void Update()
        {
            cooldown -= Time.deltaTime;
            var mouse = Mouse.current;
            var keyboard = Keyboard.current;
            var wantsToFire = (mouse != null && mouse.leftButton.isPressed) || (keyboard != null && keyboard.spaceKey.isPressed);
            if (wantsToFire && cooldown <= 0f)
            {
                Fire(GetAimDirection());
                cooldown = fireCooldown;
            }
        }

        private Vector2 GetAimDirection()
        {
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
            renderer.sprite = RuntimeSpriteFactory.CreateSquare(8);
            renderer.color = new Color(1f, 0.75f, 0.25f, 1f);
            renderer.sortingOrder = 12;
            var collider = projectileObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.12f;
            projectileObject.AddComponent<Projectile>().Launch(direction, projectileSpeed, damage);
        }

        internal void ApplyFusion(int fusedDamage, float shotsPerSecond, float fusedProjectileSpeed)
        {
            damage = Mathf.Max(1, fusedDamage);
            projectileSpeed = Mathf.Max(1f, fusedProjectileSpeed);
            fireCooldown = 1f / Mathf.Max(0.5f, shotsPerSecond);
        }
    }
}
