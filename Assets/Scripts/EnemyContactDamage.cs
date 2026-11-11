using UnityEngine;

namespace FrankenArms
{
    public sealed class EnemyContactDamage : MonoBehaviour
    {
        private void OnCollisionEnter2D(Collision2D collision)
        {
            var player = collision.collider.GetComponent<PlayerHealth>();
            if (player != null) player.TakeDamage(1);
        }
    }
}
