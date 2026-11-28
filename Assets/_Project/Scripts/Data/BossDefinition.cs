using System;
using UnityEngine;

namespace FotF.Data
{
    /// <summary>Босс (GDD §7.7): фазы по порогам HP, атаки с телеграфом, ярость, награда.</summary>
    [CreateAssetMenu(menuName = "FotF/Data/Boss", fileName = "boss_")]
    public sealed class BossDefinition : ScriptableObject
    {
        [Serializable]
        public sealed class Attack
        {
            public string id;
            public int damage;
            [Tooltip("Замах/телеграф, с (калибровать)")]
            public float windupSeconds = 1f;
            public AttackShape shape;
            [Tooltip("Сколько снарядов/волн (веер шлемов и т.п.)")]
            public int count = 1;
        }

        [Serializable]
        public sealed class Minions
        {
            public int count;
            public int maxHp;
        }

        [Serializable]
        public sealed class Phase
        {
            [Tooltip("Фаза длится, пока доля HP выше этого порога")]
            [Range(0f, 1f)] public float untilHpFraction;
            public Minions minions = new Minions();
            public Attack[] attacks = Array.Empty<Attack>();
        }

        [Serializable]
        public sealed class Reward
        {
            [Range(0f, 1f)] public float healFraction = 0.5f;
            public int passiveChoices = 3;
            [Range(0f, 1f), Tooltip("Шанс выпадения оружия босса (калибровать)")]
            public float fistDropChance;
            [Tooltip("Кристаллы (калибровать)")]
            public int crystals;
        }

        public string id;
        public string nameKey;
        public int maxHp = 1500;
        [Range(0f, 1f), Tooltip("Ярость ниже этой доли HP")]
        public float rageAtHpFraction = 0.15f;
        public float rageSpeedMultiplier = 1.25f;
        public Phase[] phases = Array.Empty<Phase>();
        public Reward reward = new Reward();
    }
}
