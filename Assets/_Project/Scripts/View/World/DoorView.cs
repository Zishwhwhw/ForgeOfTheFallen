using UnityEngine;

namespace FotF.View.World
{
    /// <summary>Дверь комнаты: запирается на время боя (GDD §3.1).</summary>
    public sealed class DoorView : MonoBehaviour
    {
        [SerializeField] Collider2D blocker;
        [SerializeField] SpriteRenderer lockedVisual;
        [SerializeField] bool lockedOnStart;

        void Awake() => SetLocked(lockedOnStart);

        public void SetLocked(bool locked)
        {
            if (blocker) blocker.enabled = locked;
            if (lockedVisual) lockedVisual.enabled = locked;
        }
    }
}
