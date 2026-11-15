using UnityEngine;
using UnityEngine.InputSystem;

namespace FrankenArms
{
    /// <summary>First GDD active-ability slice for the three playable heroes.</summary>
    public sealed class HeroActiveAbilityController : MonoBehaviour
    {
        [SerializeField] private float cooldown = 6f;
        private HeroSelectionController selection;
        private PlayerHealth health;
        private string heroName;
        private float nextUseTime;
        private string status = "ABILITY: Q";

        private void Awake()
        {
            selection = GetComponent<HeroSelectionController>();
            health = GetComponent<PlayerHealth>();
        }

        private void Update()
        {
            if (selection == null || !selection.IsSelected || health == null || health.IsDefeated) return;
            if (selection.SelectedHero != null) heroName = selection.SelectedHero.displayName;
            var keyboardPressed = Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame;
            var gamepadPressed = Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;
            if (keyboardPressed || gamepadPressed) TryUse();
        }

        private void TryUse()
        {
            if (Time.time < nextUseTime) return;
            nextUseTime = Time.time + cooldown;
            RunStats.RecordAbility();
            GameAudio.Play("ButtonPress", 0.55f);
            switch (heroName)
            {
                case "Knight":
                    health.GrantInvulnerability(1.25f);
                    DamageNearby(1.5f, 1);
                    status = "KNIGHT: LAST STAND";
                    break;
                case "Mage":
                    DamageNearby(3.2f, 2);
                    status = "MAGE: ARCANE BURST";
                    break;
                case "Mechanic":
                    var turret = new GameObject("Hero Turret");
                    turret.transform.position = transform.position + Vector3.right * 0.65f;
                    turret.AddComponent<HeroAbilityTurret>().Initialize(8f);
                    status = "MECHANIC: TURRET DEPLOYED";
                    break;
                default:
                    status = "ABILITY: NOT IMPLEMENTED";
                    break;
            }
        }

        private void DamageNearby(float radius, int damage)
        {
            var targets = FindObjectsByType<EnemyTarget>();
            foreach (var target in targets)
                if (target != null && Vector2.Distance(transform.position, target.transform.position) <= radius)
                    target.TakeDamage(damage);
        }

        private void OnGUI()
        {
            if (selection == null || !selection.IsSelected) return;
            var remaining = Mathf.Max(0f, nextUseTime - Time.time);
            GUI.Label(new Rect(Screen.width - 300f, Screen.height - 54f, 280f, 22f),
                remaining <= 0f ? "ABILITY [Q]: READY" : $"ABILITY [Q]: {remaining:0.0}s");
            GUI.Label(new Rect(Screen.width - 420f, Screen.height - 78f, 400f, 22f), status);
        }
    }

    internal sealed class HeroAbilityTurret : MonoBehaviour
    {
        private float expiresAt;
        private float nextShot;

        internal void Initialize(float lifetime)
        {
            expiresAt = Time.time + lifetime;
            var renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeSpriteFactory.CreateDoor();
            renderer.color = new Color(0.25f, 0.9f, 0.95f, 1f);
            renderer.sortingOrder = 11;
            var collider = gameObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.3f;
        }

        private void Update()
        {
            if (Time.time >= expiresAt)
            {
                Destroy(gameObject);
                return;
            }
            var player = GameObject.Find("Player");
            if (player != null)
                transform.position = Vector2.Lerp(transform.position, player.transform.position + new Vector3(0.7f, 0.45f, 0f), Time.deltaTime * 5f);
            if (Time.time < nextShot) return;

            var targets = FindObjectsByType<EnemyTarget>();
            EnemyTarget closest = null;
            var distance = 3.5f;
            foreach (var target in targets)
            {
                var current = Vector2.Distance(transform.position, target.transform.position);
                if (current < distance)
                {
                    distance = current;
                    closest = target;
                }
            }
            if (closest != null)
            {
                closest.TakeDamage(1);
                nextShot = Time.time + 0.8f;
            }
        }
    }
}
