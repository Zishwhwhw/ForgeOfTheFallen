using UnityEngine;

namespace FrankenArms
{
    public sealed class RoomDoor : MonoBehaviour
    {
        private Collider2D doorCollider;
        private SpriteRenderer renderer;

        internal void Lock()
        {
            doorCollider = gameObject.AddComponent<BoxCollider2D>();
            doorCollider.isTrigger = false;
            renderer = GetComponent<SpriteRenderer>();
            if (renderer != null) renderer.color = new Color(0.85f, 0.15f, 0.2f, 1f);
        }

        internal void Open()
        {
            if (doorCollider != null) Destroy(doorCollider);
            if (renderer != null) renderer.color = new Color(0.25f, 0.85f, 0.45f, 1f);
        }
    }
}
