using UnityEngine;
using UnityEngine.InputSystem;

namespace FrankenArms
{
    public sealed class WeaponFusionDemo : MonoBehaviour
    {
        private bool fused;
        private SpriteRenderer playerRenderer;
        private WeaponPartDefinition grip;
        private WeaponPartDefinition barrel;
        private PlayerCombat combat;

        private void Awake()
        {
            playerRenderer = GetComponent<SpriteRenderer>();
            combat = GetComponent<PlayerCombat>();
            grip = CreatePart("Arc Grip", WeaponPartKind.Grip, 1, 8f, 8f, new Color(0.35f, 0.85f, 1f, 1f));
            barrel = CreatePart("Ember Pistol", WeaponPartKind.Barrel, 2, 3f, 11f, new Color(1f, 0.45f, 0.15f, 1f));
        }

        private void Update()
        {
            if (fused || Keyboard.current == null || !Keyboard.current.fKey.wasPressedThisFrame) return;
            fused = true;
            var fusedDamage = barrel.damage + grip.damage;
            var fusedRate = (barrel.shotsPerSecond + grip.shotsPerSecond) * 0.5f;
            var fusedSpeed = (barrel.projectileSpeed + grip.projectileSpeed) * 0.5f;
            combat.ApplyFusion(fusedDamage, fusedRate, fusedSpeed);
            if (playerRenderer != null) playerRenderer.color = Color.Lerp(barrel.tint, grip.tint, 0.5f);
            Debug.Log($"Weapon fused: {barrel.displayName} + {grip.displayName} -> Ember Arc (damage {fusedDamage}, rate {fusedRate:0.0}/s)");
        }

        private static WeaponPartDefinition CreatePart(string name, WeaponPartKind kind, int damage, float rate, float speed, Color tint)
        {
            var part = ScriptableObject.CreateInstance<WeaponPartDefinition>();
            part.name = name;
            part.displayName = name;
            part.kind = kind;
            part.damage = damage;
            part.shotsPerSecond = rate;
            part.projectileSpeed = speed;
            part.tint = tint;
            return part;
        }
    }
}
