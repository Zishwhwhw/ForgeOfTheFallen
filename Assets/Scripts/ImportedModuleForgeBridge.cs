using UnityEngine;
using UnityEngine.InputSystem;

namespace FrankenArms
{
    /// <summary>
    /// Adapts the modular Grip/Barrel assets from the parent 2dRoguelike project
    /// to the lightweight runtime combat slice. It deliberately does not import
    /// the parent's conflicting WeaponController or scene flow.
    /// </summary>
    public sealed class ImportedModuleForgeBridge : MonoBehaviour
    {
        private WeaponGripSO[] grips;
        private WeaponBarrelSO[] barrels;
        private PlayerCombat combat;
        private SpriteRenderer playerRenderer;
        private Transform player;
        private CombatRoom room;
        private string status = "Imported modules ready";
        private const float InteractionRange = 2.35f;

        internal void Initialize(PlayerCombat target, SpriteRenderer targetRenderer, Transform playerTransform)
        {
            combat = target;
            playerRenderer = targetRenderer;
            player = playerTransform;
            room = FindAnyObjectByType<CombatRoom>();
            grips = Resources.LoadAll<WeaponGripSO>("ImportedModules");
            barrels = Resources.LoadAll<WeaponBarrelSO>("ImportedModules");
            if (grips.Length == 0 || barrels.Length == 0)
                status = "Imported module data unavailable";
            else if (!MetaProgression.ModulePackUnlocked)
                status = "Basic module pack active // unlock full pack in Lobby";
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || player == null || combat == null || !IsAvailable()) return;
            if (Vector2.Distance(player.position, transform.position) > InteractionRange) return;

            if (keyboard.digit4Key.wasPressedThisFrame) Equip(0);
            if (keyboard.digit5Key.wasPressedThisFrame) Equip(1);
            if (keyboard.digit6Key.wasPressedThisFrame) Equip(2);
        }

        private void Equip(int index)
        {
            if (grips == null || barrels == null || grips.Length == 0 || barrels.Length == 0) return;
            var gripCount = MetaProgression.ModulePackUnlocked ? grips.Length : Mathf.Min(1, grips.Length);
            var barrelCount = MetaProgression.ModulePackUnlocked ? barrels.Length : Mathf.Min(1, barrels.Length);
            var grip = grips[index % gripCount];
            var barrel = barrels[index % barrelCount];
            var damage = Mathf.Max(1, Mathf.RoundToInt(barrel.baseDamage * grip.damageMultiplier));
            var shotsPerSecond = 1f / Mathf.Max(0.05f, grip.fireRate);
            combat.ApplyFusion(damage, shotsPerSecond, barrel.projectileSpeed);
            Achievements.Unlock("first_fusion");
            if (playerRenderer != null)
                playerRenderer.color = Color.Lerp(grip.gripColor, barrel.barrelColor, 0.5f);
            status = $"Equipped: {grip.gripName} + {barrel.barrelName}";
        }

        private bool IsAvailable()
        {
            return room == null || room.CurrentNodeType == RoomNodeType.Forge || room.RoomKind.Contains("FORGE");
        }

        private void OnGUI()
        {
            if (!IsAvailable() || player == null || Vector2.Distance(player.position, transform.position) > InteractionRange) return;
            GUI.Label(new Rect(20f, Screen.height - 54f, 520f, 22f),
                "IMPORTED MODULES: [4] [5] [6] equip Grip + Barrel");
            GUI.Label(new Rect(20f, Screen.height - 78f, 520f, 22f), status);
        }
    }
}
