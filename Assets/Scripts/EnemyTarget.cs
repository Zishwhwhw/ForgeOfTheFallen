using UnityEngine;

namespace FrankenArms
{
    public sealed class EnemyTarget : MonoBehaviour
    {
        [SerializeField] private int health = 3;
        private SpriteRenderer renderer;
        private Color baseColor;
        private CombatRoom room;

        internal void Initialize(CombatRoom owner)
        {
            room = owner;
        }

        private void Awake()
        {
            renderer = GetComponent<SpriteRenderer>();
            baseColor = renderer != null ? renderer.color : Color.white;
        }

        internal void TakeDamage(int amount)
        {
            health -= amount;
            if (renderer != null) renderer.color = Color.white;
            CancelInvoke(nameof(RestoreColor));
            Invoke(nameof(RestoreColor), 0.08f);
            if (health <= 0)
            {
                room?.EnemyDefeated();
                Destroy(gameObject);
            }
        }

        private void RestoreColor()
        {
            if (renderer != null) renderer.color = baseColor;
        }
    }
}
