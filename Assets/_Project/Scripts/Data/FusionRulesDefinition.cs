using UnityEngine;

namespace FotF.Data
{
    /// <summary>Числа сплава (GDD §6.2). Шанс джекпота и порог −10% — из GDD; вес доминанты — калибровать.</summary>
    [CreateAssetMenu(menuName = "FotF/Data/Fusion Rules", fileName = "fusion_rules")]
    public sealed class FusionRulesDefinition : ScriptableObject
    {
        public string id = "fusion.rules";

        [Range(0.5f, 1f), Tooltip("Доля статов доминирующего родителя при доминанте A/B (калибровать)")]
        public float dominantWeight = 0.7f;
        [Range(0f, 0.1f), Tooltip("Шанс идеального сплава (GDD: 2–3%)")]
        public float jackpotChance = 0.025f;
        [Range(0f, 1f), Tooltip("Гибрид не слабее лучшего родителя минус 10% (GDD п.152)")]
        public float floorOfBestParent = 0.9f;
        [Tooltip("Длительность ритуала, с (GDD §6.4: 3–4 с, пропускаемо)")]
        public float ritualSeconds = 3.2f;
        [Tooltip("Сколько раз можно перековать гибрид (GDD §6.3)")]
        public int reforgeLimit = 1;
        [Tooltip("Цена перековки, золото (калибровать)")]
        public int reforgePrice;
        [Tooltip("Реплики Франкена из темноты (GDD п.155: 15–20 реплик × RU/EN/PL). Пока черновые, текст — за владельцем")]
        [TextArea] public string[] frankenLines = { "…Хм. Сойдёт.", "…Ещё одно уродство. Прекрасно.", "…Швы держат. Пока." };
    }
}
