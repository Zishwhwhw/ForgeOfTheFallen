using UnityEngine;

namespace FrankenArms
{
    /// <summary>
    /// Resource used only by found/fused weapons. The hero's base weapon stays
    /// free, matching the GDD resource rule.
    /// </summary>
    public sealed class PlayerMana : MonoBehaviour
    {
        [SerializeField] private int maxMana = 100;
        private int currentMana;

        internal int CurrentMana { get { return currentMana; } }
        internal int MaxMana { get { return maxMana; } }

        private void Awake()
        {
            currentMana = maxMana;
        }

        internal bool TrySpend(int amount)
        {
            if (amount <= 0) return true;
            if (currentMana < amount) return false;
            currentMana -= amount;
            return true;
        }

        internal void Restore(int amount)
        {
            currentMana = Mathf.Clamp(currentMana + Mathf.Max(0, amount), 0, maxMana);
        }

        internal void IncreaseMaximum(int amount)
        {
            if (amount <= 0) return;
            maxMana += amount;
            currentMana = Mathf.Min(maxMana, currentMana + amount);
        }

        internal void Configure(int value)
        {
            maxMana = Mathf.Max(0, value);
            currentMana = maxMana;
        }
    }
}
