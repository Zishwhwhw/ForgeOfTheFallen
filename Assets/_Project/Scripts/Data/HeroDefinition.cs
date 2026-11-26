using UnityEngine;

namespace FotF.Data
{
    /// <summary>Описание героя (GDD §4). Цифры — для калибровки, финал в Docs/COMBAT_BALANCE.md.</summary>
    [CreateAssetMenu(menuName = "FotF/Data/Hero", fileName = "hero_")]
    public sealed class HeroDefinition : ScriptableObject
    {
        public string id;
        public string nameKey;

        [Header("Живучесть (GDD §4.1–4.2)")]
        public int maxHp = 120;
        public int maxArmor = 60;
        public int maxMana = 150;
        [Tooltip("Броня начинает восстанавливаться через N секунд без урона (GDD: 3 с)")]
        public float armorRegenDelay = 3f;
        [Tooltip("Скорость восстановления брони, ед./с (калибровать)")]
        public float armorRegenPerSecond = 30f;

        [Header("Движение (калибровать)")]
        public float moveSpeed = 5f;

        [Header("Рывок (GDD §4.1: 1 заряд, перезарядка ~1 с, i-frames)")]
        public int dashCharges = 1;
        public float dashCooldown = 1f;
        [Tooltip("Длительность рывка, с (калибровать)")]
        public float dashDuration = 0.18f;
        [Tooltip("Дальность рывка, юниты (калибровать)")]
        public float dashDistance = 3f;
        [Tooltip("Неуязвимость во время и сразу после рывка, с (калибровать)")]
        public float dashInvulnerability = 0.25f;

        [Header("Старт")]
        public WeaponDefinition startWeapon;
    }
}
