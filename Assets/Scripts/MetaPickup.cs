using UnityEngine;

namespace FrankenArms
{
    public enum MetaPickupType
    {
        Crystal,
        Blueprint,
        Diary
    }

    /// <summary>Physical persistent reward pickup used by boss and special rooms.</summary>
    public sealed class MetaPickup : MonoBehaviour
    {
        private MetaPickupType type;

        internal void Initialize(MetaPickupType pickupType)
        {
            type = pickupType;
            var renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeSpriteFactory.CreateCoin();
            renderer.color = pickupType == MetaPickupType.Crystal
                ? new Color(0.25f, 0.9f, 1f, 1f)
                : pickupType == MetaPickupType.Blueprint
                    ? new Color(1f, 0.65f, 0.18f, 1f)
                    : new Color(0.75f, 0.45f, 1f, 1f);
            renderer.sortingOrder = 13;
            var collider = gameObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.28f;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player") && other.GetComponent<PlayerMovement>() == null) return;
            switch (type)
            {
                case MetaPickupType.Crystal:
                    MetaProgression.AddCrystals(1);
                    Debug.Log("Meta pickup: crystal +1");
                    break;
                case MetaPickupType.Blueprint:
                    MetaProgression.AddBlueprint();
                    Debug.Log("Meta pickup: blueprint +1");
                    break;
                case MetaPickupType.Diary:
                    MetaProgression.AddDiaryEntry();
                    Debug.Log("Meta pickup: diary entry +1");
                    break;
            }
            RunStats.RecordMeta();
            Achievements.Unlock("first_meta");
            GameAudio.Play("Bonus", 0.85f);
            Destroy(gameObject);
        }
    }
}
