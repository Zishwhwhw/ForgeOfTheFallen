using UnityEngine;

namespace FotF.Data
{
    /// <summary>Общие числа среза (GDD §4.1, §7.9, §8): крит, экономика, лут, лимиты.</summary>
    [CreateAssetMenu(menuName = "FotF/Data/Balance Constants", fileName = "constants_")]
    public sealed class BalanceConstants : ScriptableObject
    {
        public string id = "constants.slice";
        [Range(0f, 1f)] public float critChance = 0.05f;
        public float critMultiplier = 2f;
        [Tooltip("Цена оружия по редкости: Common, Rare, Epic")]
        public ValueMap weaponPrice = new ValueMap();
        [Range(0f, 1f)] public float weaponSellbackFraction = 0.5f;
        public ValueMap gold = new ValueMap();
        public ValueMap chestLoot = new ValueMap();
        public ValueMap healing = new ValueMap();
        [Range(0f, 1f)] public float miniBossRoomChance = 0.3f;
        [Tooltip("Общий лимит трупов на этаж (GDD §10.2, п.165)")]
        public int corpseLimitPerFloor = 300;
        public int passiveChoicesPerFloor = 3;
        [Tooltip("Дроп с врагов: manaOrbMin, manaOrbMax, manaPerOrb (черновик)")]
        public ValueMap drops = new ValueMap();
    }
}
