using UnityEngine;

namespace FotF.Data
{
    /// <summary>Описание врага (GDD §7.4). Поля «калибровать» в GDD не заданы.</summary>
    [CreateAssetMenu(menuName = "FotF/Data/Enemy", fileName = "enemy_")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        public string id;
        public string nameKey;

        [Header("GDD §7.4")]
        public int maxHp = 40;
        public int attackDamage = 12;
        [Tooltip("Замах перед ударом, с (телеграф: GDD п.86, 0.4–0.8 с)")]
        public float windupSeconds = 0.5f;

        [Header("Калибровать")]
        public float moveSpeed = 2.2f;
        [Tooltip("Начинает замах на этой дистанции, юниты")]
        public float attackRange = 1.1f;
        [Tooltip("Радиус удара, юниты")]
        public float strikeRadius = 1.3f;
        [Tooltip("Угол удара, градусы")]
        public float strikeArcDegrees = 110f;
        [Tooltip("Пауза после удара, с")]
        public float recoverSeconds = 0.6f;
        [Tooltip("Видит героя с дистанции, юниты")]
        public float aggroRange = 12f;

        [Header("Награда (GDD §7.9: обычный враг 1–3 золота)")]
        public int goldMin = 1;
        public int goldMax = 3;
    }
}
