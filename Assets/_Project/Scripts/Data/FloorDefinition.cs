using UnityEngine;

namespace FotF.Data
{
    /// <summary>
    /// Правила этажа биома (GDD §3.1, §8; решения владельца 28.11): число боевых комнат, шансы ответвлений.
    /// Числа — из Balance/floors.json. Мини-босс — BalanceConstants.miniBossRoomChance.
    /// </summary>
    [CreateAssetMenu(menuName = "FotF/Data/Floor", fileName = "floor_")]
    public sealed class FloorDefinition : ScriptableObject
    {
        public string id;
        [Tooltip("Этажей до босса")]
        public int floorsPerBiome = 4;
        [Min(1)] public int combatRoomsMin = 4;
        [Min(1)] public int combatRoomsMax = 5;

        [Header("Шансы ответвлений (калибровать)")]
        [Range(0f, 1f)] public float forgeChance = 0.4f;
        [Range(0f, 1f)] public float merchantChance = 1f;
        [Range(0f, 1f)] public float fountainChance = 0.25f;
        [Range(0f, 1f)] public float shrineChance = 0.35f;
        [Range(0f, 1f)] public float cageChance = 0.25f;
        [Range(0f, 1f)] public float secretChance = 0.4f;
        [Range(0f, 1f)] public float eliteChance = 0.5f;
        [Range(0f, 1f)] public float chestRoomChance = 0.3f;
        [Tooltip("Не больше ответвлений на этаж")]
        public int maxBranches = 5;
    }
}
