using UnityEngine;

namespace FrankenArms
{
    public sealed class EnemyTarget : MonoBehaviour
    {
        [SerializeField] private int health = 3;
        private int maxHealth = 3;
        private SpriteRenderer renderer;
        private Color baseColor;
        private CombatRoom room;
        private StatusEffectController status;

        internal void Initialize(CombatRoom owner)
        {
            room = owner;
        }

        internal void Initialize(CombatRoom owner, int startingHealth)
        {
            room = owner;
            health = Mathf.Max(1, startingHealth);
            maxHealth = health;
        }

        internal float HealthRatio { get { return maxHealth <= 0 ? 0f : Mathf.Clamp01((float)health / maxHealth); } }

        private void Awake()
        {
            renderer = GetComponent<SpriteRenderer>();
            baseColor = renderer != null ? renderer.color : Color.white;
            status = GetComponent<StatusEffectController>();
        }

        internal void TakeDamage(int amount)
        {
            if (status == null) status = GetComponent<StatusEffectController>();
            health -= status == null ? amount : status.AmplifyDamage(amount);
            var popupObject = new GameObject("Damage Number");
            popupObject.transform.position = transform.position + new Vector3(0f, 0.45f, 0f);
            popupObject.AddComponent<FloatingDamageText>().Initialize(amount, amount >= 2);
            if (amount >= 2)
            {
                GameFeel.HitStop(0.035f);
                ScreenShake.Pulse(0.08f, 0.06f);
            }
            if (renderer != null) renderer.color = Color.white;
            CancelInvoke(nameof(RestoreColor));
            Invoke(nameof(RestoreColor), 0.08f);
            if (health <= 0)
            {
                RunStats.RecordKill();
                Achievements.Unlock("first_kill");
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
