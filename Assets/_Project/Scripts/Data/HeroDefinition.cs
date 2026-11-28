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

        [Header("Ульта и открытие (GDD §4.1, §4.3)")]
        [Tooltip("Перезарядка ульты, с")]
        public float ultimateCooldown = 60f;
        [Tooltip("Доступен с начала игры (иначе — открытие за кристаллы)")]
        public bool unlockedFromStart = true;

        [Header("Рука с оружием (Art Bible §5.1), пиксели от точки опоры у ног")]
        [Tooltip("Лицом к камере: правая рука героя — слева от зрителя")]
        public Vector2 handSouthPixels = new Vector2(-9, 12);
        [Tooltip("Боком, взгляд вправо (влево — зеркально): передняя рука")]
        public Vector2 handSidePixels = new Vector2(4, 12);
        [Tooltip("Спиной: правая рука — справа от зрителя, оружие за телом")]
        public Vector2 handNorthPixels = new Vector2(8, 12);

        [Header("Старт")]
        public WeaponDefinition startWeapon;
    }
}
