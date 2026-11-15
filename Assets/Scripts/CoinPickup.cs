using UnityEngine;

namespace FrankenArms
{
    public sealed class CoinPickup : MonoBehaviour
    {
        private int credits = 10;
        private int manaRestore = 20;

        internal void Initialize(int rewardCredits, int rewardMana)
        {
            credits = Mathf.Max(0, rewardCredits);
            manaRestore = Mathf.Max(0, rewardMana);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player") && other.GetComponent<PlayerMovement>() == null) return;
            RunProgress.AddCredits(credits);
            RunStats.RecordCredits(credits);
            var mana = other.GetComponent<PlayerMana>();
            if (mana != null) mana.Restore(manaRestore);
            GameAudio.Play("Bonus", 0.7f);
            Debug.Log("Room reward collected: +" + credits + " credits");
            Destroy(gameObject);
        }
    }
}
