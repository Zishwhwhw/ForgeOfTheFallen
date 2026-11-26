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
        public Wave[] waves = Array.Empty<Wave>();
        [Tooltip("Пауза между волнами, с (калибровать)")]
        public float delayBetweenWaves = 0.8f;
    }
}
