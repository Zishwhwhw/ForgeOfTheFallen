using UnityEngine;
using UnityEngine.InputSystem;

namespace FrankenArms
{
    /// <summary>
    /// Short directional dodge. The dash owns its fixed-step movement while
    /// active, and grants the player a small fairness window against contact
    /// damage.
    /// </summary>
    public sealed class PlayerDash : MonoBehaviour
    {
        [SerializeField] private float dashSpeed = 11f;
        [SerializeField] private float dashDuration = 0.16f;
        [SerializeField] private float cooldownDuration = 1.05f;
        [SerializeField] private float invulnerabilityDuration = 0.20f;

        private Rigidbody2D body;
        private PlayerMovement movement;
        private PlayerHealth health;
        private Vector2 direction;
        private float dashRemaining;
        private float cooldownRemaining;

        internal bool IsDashing { get { return dashRemaining > 0f; } }
        internal bool IsReady { get { return cooldownRemaining <= 0f && !IsDashing; } }
        internal float CooldownRemaining { get { return cooldownRemaining; } }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            movement = GetComponent<PlayerMovement>();
            health = GetComponent<PlayerHealth>();
        }

        private void Update()
        {
            if (Time.timeScale <= 0f) return;
            if (cooldownRemaining > 0f)
                cooldownRemaining -= Time.deltaTime;

            var keyboard = Keyboard.current;
            var gamepad = Gamepad.current;
            if ((keyboard != null && keyboard.leftShiftKey.wasPressedThisFrame)
                || (gamepad != null && gamepad.leftShoulder.wasPressedThisFrame))
                TryStartDash();
        }

        private void FixedUpdate()
        {
            if (!IsDashing) return;

            body.MovePosition(body.position + direction * dashSpeed * Time.fixedDeltaTime);
            dashRemaining -= Time.fixedDeltaTime;
        }

        private void TryStartDash()
        {
            if (!IsReady || movement == null || health == null || health.IsDefeated) return;

            var input = movement.CurrentInput;
            direction = input.sqrMagnitude > 0.01f ? input.normalized : movement.LastMoveDirection;
            if (direction.sqrMagnitude < 0.01f) direction = Vector2.right;

            dashRemaining = dashDuration;
            cooldownRemaining = cooldownDuration;
            health.GrantInvulnerability(invulnerabilityDuration);
            GameAudio.Play("Whoosh", 0.55f);
        }
    }
}
