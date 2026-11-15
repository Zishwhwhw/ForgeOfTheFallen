using UnityEngine;
using UnityEngine.InputSystem;

namespace FrankenArms
{

public sealed class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    private Rigidbody2D body;
    private PlayerDash dash;
    private StatusEffectController status;
    private Vector2 currentInput;
    private Vector2 lastMoveDirection = Vector2.right;

    internal Vector2 CurrentInput { get { return currentInput; } }
    internal Vector2 LastMoveDirection { get { return lastMoveDirection; } }

    internal void ConfigureSpeed(float value)
    {
        speed = Mathf.Max(1f, value);
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        status = GetComponent<StatusEffectController>();
    }

    private void FixedUpdate()
    {
        if (status == null) status = GetComponent<StatusEffectController>();
        var input = Vector2.zero;
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input.x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input.x += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) input.y -= 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) input.y += 1f;
        }
        var gamepad = Gamepad.current;
        if (gamepad != null)
        {
            var stick = gamepad.leftStick.ReadValue();
            if (stick.sqrMagnitude > input.sqrMagnitude) input = stick;
        }

        currentInput = input.sqrMagnitude > 1f ? input.normalized : input;
        if (currentInput.sqrMagnitude > 0.01f)
            lastMoveDirection = currentInput.normalized;

        if (dash == null) dash = GetComponent<PlayerDash>();
        if (dash != null && dash.IsDashing) return;

        var speedMultiplier = status == null ? 1f : status.MoveSpeedMultiplier;
        body.MovePosition(body.position + currentInput * speed * speedMultiplier * Time.fixedDeltaTime);
    }
}
}
