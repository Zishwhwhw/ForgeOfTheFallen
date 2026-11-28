using UnityEngine;

namespace FotF.Data
{
    /// <summary>Пассивка (GDD §7.2): эффект по имени + параметры.</summary>
    [CreateAssetMenu(menuName = "FotF/Data/Passive", fileName = "passive_")]
    public sealed class PassiveDefinition : ScriptableObject
    {
        public string id;
        public string nameKey;
        public string displayName;
        [Tooltip("Имя эффекта, который реализует код")]
        public string effect;
        public ValueMap values = new ValueMap();
        public Sprite icon;
    }
}
