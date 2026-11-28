using System;
using UnityEngine;

namespace FotF.Data
{
    /// <summary>Боевая встреча комнаты: 2–3 волны (GDD §3.1). Точки спавна — в сцене/префабе комнаты.</summary>
    [CreateAssetMenu(menuName = "FotF/Data/Encounter", fileName = "encounter_")]
    public sealed class EncounterDefinition : ScriptableObject
    {
        [Serializable]
        public sealed class Group
        {
            public EnemyDefinition enemy;
            [Min(1)] public int count = 1;
        }

        [Serializable]
        public sealed class Wave
        {
            public Group[] groups = Array.Empty<Group>();
        }

        public string id;
        [Tooltip("Для какой комнаты: обычный бой, элита, мини-босс")]
        public EncounterPool pool;
        [Tooltip("Этажи, на которых встреча может выпасть")]
        public int minFloor = 1;
        public int maxFloor = 4;
        public Wave[] waves = Array.Empty<Wave>();
        [Tooltip("Пауза между волнами, с (калибровать)")]
        public float delayBetweenWaves = 0.8f;
    }
}
