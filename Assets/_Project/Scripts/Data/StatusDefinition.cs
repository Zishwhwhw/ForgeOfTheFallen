using UnityEngine;

namespace FotF.Data
{
    /// <summary>Статус стихии (GDD §7.3). Поля, не нужные статусу, остаются по умолчанию.</summary>
    [CreateAssetMenu(menuName = "FotF/Data/Status", fileName = "status_")]
    public sealed class StatusDefinition : ScriptableObject
    {
        public string id;
        public Element element;

        [Header("Калибровать")]
        [Tooltip("Урон за тик (горение, яд)")]
        public float tickDamage;
        public float tickSeconds = 0.5f;
        public float durationSeconds = 3f;
        [Range(0f, 1f), Tooltip("Замедление (холод)")]
        public float slowFraction;
        [Tooltip("Оглушение (шок)")]
        public float stunSeconds;

        [Header("Наложение")]
        public Stacking stacking;
        [Min(1)] public int maxStacks = 1;
        [Tooltip("Заморозка при N стаках холода (0 — нет)")]
        public int freezeAtStacks;
        public float freezeSeconds;
        [Range(0f, 1f), Tooltip("Сопротивление боссов (калибровать)")]
        public float bossResist;
        public Sprite icon;
    }
}
