using UnityEngine;
using UnityEngine.InputSystem;

namespace FrankenArms
{
    public sealed class FusionStation : MonoBehaviour
    {
        private WeaponFusionRecipe[] candidates;
        private PlayerCombat combat;
        private SpriteRenderer playerRenderer;
        private Transform player;
        private CombatRoom room;
        private bool open;
        private const float InteractionRange = 2.35f;
        private string status = "Walk to the forge and press F";

        internal void Initialize(PlayerCombat target, SpriteRenderer targetRenderer, Transform playerTransform)
        {
            combat = target;
            playerRenderer = targetRenderer;
            player = playerTransform;
            room = FindAnyObjectByType<CombatRoom>();
            candidates = Resources.LoadAll<WeaponFusionRecipe>("Content/Fusions");
            if (candidates.Length == 0)
                status = "No fusion recipes installed";
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (!IsAvailable()) { open = false; return; }
            var inRange = player != null && Vector2.Distance(player.position, transform.position) <= InteractionRange;
            if (!inRange)
            {
                open = false;
                return;
            }
            if (keyboard.fKey.wasPressedThisFrame) open = !open;
            if (!open || candidates == null) return;
            if (keyboard.digit1Key.wasPressedThisFrame) Choose(0);
            if (keyboard.digit2Key.wasPressedThisFrame) Choose(1);
            if (keyboard.digit3Key.wasPressedThisFrame) Choose(2);
        }

        private void Choose(int index)
        {
            var candidate = candidates[index];
            combat.ApplyFusion(candidate.damage, candidate.shotsPerSecond, candidate.projectileSpeed);
            Achievements.Unlock("first_fusion");
            if (playerRenderer != null) playerRenderer.color = candidate.tint;
            GameAudio.Play("ButtonPress", 0.6f);
            status = "Equipped: " + candidate.displayName;
        }

        private bool IsAvailable()
        {
            return room == null || room.CurrentNodeType == RoomNodeType.Forge || room.RoomKind.Contains("FORGE");
        }

        private void OnGUI()
        {
            var inRange = player != null && Vector2.Distance(player.position, transform.position) <= InteractionRange;
            if (!IsAvailable()) return;
            if (!inRange)
            {
                GUI.Label(new Rect(20f, Screen.height - 30f, 380f, 24f), "FORGE: move closer   [F] interact");
                return;
            }
            if (!open || candidates == null)
            {
                GUI.Label(new Rect(20f, Screen.height - 30f, 380f, 24f), "FORGE: [F] open fusion choices");
                return;
            }
            var panel = new Rect(20f, 20f, 360f, 185f);
            GUI.Box(panel, "FRANKEN ARM // FUSION STATION");
            GUI.Label(new Rect(35f, 50f, 330f, 24f), status);
            for (var i = 0; i < candidates.Length; i++)
            {
                var candidate = candidates[i];
                var y = 78f + i * 32f;
                if (GUI.Button(new Rect(35f, y, 330f, 25f), string.Format("{0}. {1}  |  DMG {2}  |  {3:0.0}/s", i + 1, candidate.displayName, candidate.damage, candidate.shotsPerSecond)))
                    Choose(i);
            }
            GUI.Label(new Rect(35f, 175f, 330f, 20f), "Press F to close the station");
        }
    }
}
