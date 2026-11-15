using UnityEngine;

namespace FrankenArms
{
    public sealed class PlayerArmor : MonoBehaviour
    {
        [SerializeField] private int maxArmor = 2;
        private int currentArmor;
        private float recoveryDelay;

        internal int CurrentArmor { get { return currentArmor; } }
        internal int MaxArmor { get { return maxArmor; } }

        private void Awake()
        {
            currentArmor = maxArmor;
        }

        private void Update()
        {
            if (recoveryDelay > 0f) recoveryDelay -= Time.deltaTime;
            if (recoveryDelay <= 0f && currentArmor < maxArmor)
                currentArmor = maxArmor;
        }

        internal bool AbsorbDamage(int amount)
        {
            if (currentArmor <= 0 || amount <= 0) return false;
            currentArmor = 0;
            recoveryDelay = 3f;
            return true;
        }

        internal void Configure(int value)
        {
            maxArmor = Mathf.Max(0, value);
            currentArmor = maxArmor;
            recoveryDelay = 0f;
        }
    }
}
