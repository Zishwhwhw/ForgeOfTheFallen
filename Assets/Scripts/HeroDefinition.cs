using UnityEngine;

namespace FrankenArms
{
    [CreateAssetMenu(menuName = "FrankenArms/Hero Definition")]
    public sealed class HeroDefinition : ScriptableObject
    {
        public string displayName;
        public string role;
        [TextArea] public string description;
        public int maxHealth = 8;
        public int maxArmor = 3;
        public int maxMana = 100;
        public float moveSpeed = 5f;
        public int baseDamage = 1;
        public float baseShotsPerSecond = 5.5f;
        public Color tint = Color.white;
        public string dashAbility;
        public string activeAbility;
        public string ultimateOrPassive;
    }
}
