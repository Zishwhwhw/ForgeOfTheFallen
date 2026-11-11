using UnityEngine;
using UnityEngine.InputSystem;

namespace FrankenArms
{
    public sealed class WeaponFusionDemo : MonoBehaviour
    {
        private bool fused;
        private SpriteRenderer playerRenderer;

        private void Awake()
        {
            playerRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (fused || Keyboard.current == null || !Keyboard.current.fKey.wasPressedThisFrame) return;
            fused = true;
            if (playerRenderer != null) playerRenderer.color = new Color(0.95f, 0.35f, 0.85f, 1f);
            Debug.Log("Weapon fused: Ember Pistol + Arc Grip -> Ember Arc");
        }
    }
}
