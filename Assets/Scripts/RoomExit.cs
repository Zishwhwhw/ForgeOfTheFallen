using UnityEngine;

namespace FrankenArms
{
    public sealed class RoomExit : MonoBehaviour
    {
        private CombatRoom room;
        private Collider2D trigger;

        internal void Initialize(CombatRoom owner)
        {
            room = owner;
            trigger = gameObject.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.enabled = false;
        }

        internal void Open()
        {
            if (trigger != null) trigger.enabled = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (trigger == null || !trigger.enabled || other.GetComponent<PlayerMovement>() == null) return;
            room.StartNextRoom();
        }
    }
}
