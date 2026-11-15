using UnityEngine;

namespace FrankenArms
{
    public enum StatusEffectType
    {
        None,
        Burning,
        Poison,
        Frozen,
        Marked,
        Cursed
    }

    public sealed class StatusEffectController : MonoBehaviour
    {
        private float burning;
        private float poison;
        private float frozen;
        private float marked;
        private float curse;
        private float tickTimer;
        private PlayerHealth playerHealth;
        private EnemyTarget enemyTarget;

        internal bool HasStatus { get { return burning > 0f || poison > 0f || frozen > 0f || marked > 0f || curse > 0f; } }
        internal bool IsMarked { get { return marked > 0f; } }
        internal float MoveSpeedMultiplier { get { return frozen > 0f ? 0.45f : 1f; } }
        internal string ShortLabel
        {
            get
            {
                if (frozen > 0f) return "FROZEN";
                if (burning > 0f) return "BURNING";
                if (poison > 0f) return "POISONED";
                if (marked > 0f) return "MARKED";
                if (curse > 0f) return "CURSED";
                return string.Empty;
            }
        }

        private void Awake()
        {
            playerHealth = GetComponent<PlayerHealth>();
            enemyTarget = GetComponent<EnemyTarget>();
            tickTimer = 1f;
        }

        private void Update()
        {
            if (playerHealth == null) playerHealth = GetComponent<PlayerHealth>();
            if (enemyTarget == null) enemyTarget = GetComponent<EnemyTarget>();
            burning -= Time.deltaTime;
            poison -= Time.deltaTime;
            frozen -= Time.deltaTime;
            marked -= Time.deltaTime;
            curse -= Time.deltaTime;
            tickTimer -= Time.deltaTime;
            if (tickTimer <= 0f)
            {
                if (burning > 0f || poison > 0f)
                {
                    if (playerHealth != null) playerHealth.TakeDamage(1);
                    if (enemyTarget != null) enemyTarget.TakeDamage(1);
                }
                tickTimer = 1f;
            }
        }

        internal void Apply(StatusEffectType type, float duration)
        {
            switch (type)
            {
                case StatusEffectType.Burning: burning = Mathf.Max(burning, duration); break;
                case StatusEffectType.Poison: poison = Mathf.Max(poison, duration); break;
                case StatusEffectType.Frozen: frozen = Mathf.Max(frozen, duration); break;
                case StatusEffectType.Marked: marked = Mathf.Max(marked, duration); break;
                case StatusEffectType.Cursed: curse = Mathf.Max(curse, duration); break;
            }
        }

        internal int AmplifyDamage(int amount)
        {
            return marked > 0f ? Mathf.CeilToInt(amount * 1.25f) : amount;
        }
    }
}
