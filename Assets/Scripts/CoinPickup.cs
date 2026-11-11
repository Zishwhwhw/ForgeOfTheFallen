using UnityEngine;

namespace FrankenArms
{
    public sealed class CoinPickup : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player") && other.GetComponent<PlayerMovement>() == null) return;
            RunProgress.AddCredits(10);
            Debug.Log("Room reward collected: +10 credits");
            Destroy(gameObject);
        }
    }
}
