using UnityEngine;
using UnityEngine.InputSystem;

namespace FrankenArms
{
    /// <summary>Small authored shop interaction for the GDD between-room economy.</summary>
    public sealed class RunShopStation : MonoBehaviour
    {
        private PlayerHealth health;
        private PlayerMana mana;
        private Transform player;
        private CombatRoom room;
        private string status = "SHOP: press B to open";
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
            if (player == null || !IsAvailable() || Vector2.Distance(player.position, transform.position) > InteractionRange) return;
            if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame) BuyHealth();
            if (Keyboard.current != null && Keyboard.current.nKey.wasPressedThisFrame) BuyMana();
        }

        private void BuyHealth()
        {
            if (health == null || !RunProgress.TrySpendCredits(5))
            {
                status = "SHOP: need 5 credits for HEART TONIC";
                return;
            }
            health.Heal(2);
            GameAudio.Play("Bonus", 0.6f);
            status = "SHOP: HEART TONIC +2 HP";
        }

        private void BuyMana()
        {
            if (mana == null || !RunProgress.TrySpendCredits(5))
            {
                status = "SHOP: need 5 credits for MANA CELL";
                return;
            }
            mana.Restore(25);
            GameAudio.Play("Bonus", 0.6f);
            status = "SHOP: MANA CELL +25 MANA";
        }

        private void OnGUI()
        {
            if (player == null || !IsAvailable() || Vector2.Distance(player.position, transform.position) > InteractionRange) return;
            GUI.Label(new Rect(20f, Screen.height - 108f, 520f, 22f), "SHOP [B] HEART TONIC +2 HP / [N] MANA CELL +25");
            GUI.Label(new Rect(20f, Screen.height - 132f, 520f, 22f), status);
        }

        private bool IsAvailable() { return room == null || room.CurrentNodeType == RoomNodeType.Shop; }
    }
}
