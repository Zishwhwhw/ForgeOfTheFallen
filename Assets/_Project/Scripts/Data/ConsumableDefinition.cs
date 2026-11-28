using UnityEngine;

namespace FotF.Data
{
    /// <summary>Расходник (GDD §7.9): зелья, бомба.</summary>
    [CreateAssetMenu(menuName = "FotF/Data/Consumable", fileName = "item_")]
    public sealed class ConsumableDefinition : ScriptableObject
    {
        public string id;
        public string nameKey;
        public int heal;
        public int mana;
        [Tooltip("Цена у торговца (калибровать)")]
        public int price;
        [Tooltip("Урон по области (бомба)")]
        public int areaDamage;
        public bool breaksSecretWalls;
        public Sprite icon;
    }
}
