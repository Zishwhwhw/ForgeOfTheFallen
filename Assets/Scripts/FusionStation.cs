using UnityEngine;
using UnityEngine.InputSystem;

namespace FrankenArms
{
    public sealed class FusionStation : MonoBehaviour
    {
        private sealed class Candidate
        {
            internal string Name;
            internal int Damage;
            internal float Rate;
            internal float Speed;
            internal Color Tint;
        }

        private Candidate[] candidates;
        private PlayerCombat combat;
        private SpriteRenderer playerRenderer;
        private bool open = true;
        private string status = "Choose a fusion result";

        internal void Initialize(PlayerCombat target, SpriteRenderer targetRenderer)
        {
            combat = target;
            playerRenderer = targetRenderer;
            candidates = new[]
            {
                new Candidate { Name = "Ember Arc", Damage = 3, Rate = 5.5f, Speed = 9.5f, Tint = new Color(1f, 0.45f, 0.2f, 1f) },
                new Candidate { Name = "Rapid Ember", Damage = 2, Rate = 9f, Speed = 10f, Tint = new Color(1f, 0.75f, 0.2f, 1f) },
                new Candidate { Name = "Heavy Arc", Damage = 5, Rate = 2.5f, Speed = 12f, Tint = new Color(0.55f, 0.8f, 1f, 1f) }
            };
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.fKey.wasPressedThisFrame) open = !open;
            if (!open || candidates == null) return;
            if (keyboard.digit1Key.wasPressedThisFrame) Choose(0);
            if (keyboard.digit2Key.wasPressedThisFrame) Choose(1);
            if (keyboard.digit3Key.wasPressedThisFrame) Choose(2);
        }

        private void Choose(int index)
        {
            var candidate = candidates[index];
            combat.ApplyFusion(candidate.Damage, candidate.Rate, candidate.Speed);
            if (playerRenderer != null) playerRenderer.color = candidate.Tint;
            status = "Equipped: " + candidate.Name;
        }

        private void OnGUI()
        {
            if (!open || candidates == null) return;
            var panel = new Rect(20f, 20f, 360f, 185f);
            GUI.Box(panel, "FRANKEN ARM // FUSION STATION");
            GUI.Label(new Rect(35f, 50f, 330f, 24f), status);
            for (var i = 0; i < candidates.Length; i++)
            {
                var candidate = candidates[i];
                var y = 78f + i * 32f;
                if (GUI.Button(new Rect(35f, y, 330f, 25f), string.Format("{0}. {1}  |  DMG {2}  |  {3:0.0}/s", i + 1, candidate.Name, candidate.Damage, candidate.Rate)))
                    Choose(i);
            }
            GUI.Label(new Rect(35f, 175f, 330f, 20f), "Press F to close the station");
        }
    }
}
