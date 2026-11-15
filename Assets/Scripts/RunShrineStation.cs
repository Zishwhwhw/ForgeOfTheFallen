using UnityEngine;
using UnityEngine.InputSystem;

namespace FrankenArms
{
    /// <summary>GDD risk altar: max HP trade for max mana, once per shrine.</summary>
    public sealed class RunShrineStation : MonoBehaviour
    {
        private PlayerHealth health;
        private PlayerMana mana;
        private Transform player;
        private CombatRoom room;
        private bool used;
        private string status = "SHRINE: press V to inspect";
        private const float InteractionRange = 2.35f;

        internal void Initialize(PlayerHealth targetHealth, PlayerMana targetMana, Transform playerTransform)
        {
            health = targetHealth;
            mana = targetMana;
            player = playerTransform;
            room = FindAnyObjectByType<CombatRoom>();
        }

        private void Update()
        {
            if (used || player == null || !IsAvailable() || Vector2.Distance(player.position, transform.position) > InteractionRange) return;
            if (Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame)
            {
                if (health != null && mana != null && health.TradeMaxHealth(1))
                {
                    mana.IncreaseMaximum(25);
                    GameAudio.Play("Whoosh", 0.5f);
                    status = "SHRINE: MAX HP -1 // MAX MANA +25";
                    used = true;
                }
                else status = "SHRINE: not enough HP to offer";
            }
        }

        private void OnGUI()
        {
            if (player == null || !IsAvailable() || Vector2.Distance(player.position, transform.position) > InteractionRange) return;
            GUI.Label(new Rect(20f, Screen.height - 156f, 520f, 22f), used ? status : "SHRINE [V] OFFER 1 MAX HP FOR 25 MAX MANA");
        }

        private bool IsAvailable() { return room == null || room.CurrentNodeType == RoomNodeType.Shrine; }
    }
}
