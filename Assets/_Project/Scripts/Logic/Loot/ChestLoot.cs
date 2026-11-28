using System.Collections.Generic;
using FotF.Data;

namespace FotF.Logic.Loot
{
    public enum LootKind { Gold, Potion, Weapon, Bomb, Heart }

    public struct LootRoll
    {
        public LootKind Kind;
        public int Amount;          // золото
        public string ItemId;       // зелье/сердце/бомба — id расходника; оружие — id оружия
    }

    /// <summary>Числа сундука (constants.chestLoot + gold). Доли внутри веток — черновик (calibrate).</summary>
    public struct ChestLootTable
    {
        public float GoldOrPotion, Weapon, BombOrRare;
        public int MaxWeaponsPerFloor, ChestGoldMin, ChestGoldMax;
        public float PotionShare, PotionSmall, PotionMana, PotionMixed, PotionBig;
        public float RareBomb, RareHeart, RareWeapon;
        public float WeaponCommon, WeaponRare, WeaponEpic;
    }

    /// <summary>
    /// Сундук после боевой комнаты (GDD §7.9): золото/зелье ~60%, оружие ~30%, бомба/редкость ~10%;
    /// оружия не больше MaxWeaponsPerFloor за этаж — сверх лимита ветка оружия даёт золото/зелье.
    /// </summary>
    public static class ChestLoot
    {
        public const string PotionSmall = "item.potion_hp_small", PotionBig = "item.potion_hp_big",
            PotionMana = "item.potion_mana", PotionMixed = "item.potion_mixed", Bomb = "item.bomb", Heart = "item.chest_heart";

        public static LootRoll Roll(in ChestLootTable t, IReadOnlyList<(string id, Rarity rarity)> weaponPool, int weaponsThisFloor, IRandom rng)
        {
            bool weaponAllowed = weaponsThisFloor < t.MaxWeaponsPerFloor && weaponPool.Count > 0;
            float total = t.GoldOrPotion + t.Weapon + t.BombOrRare;
            float r = rng.NextFloat() * (total > 0f ? total : 1f);
            if (r < t.GoldOrPotion) return GoldOrPotion(t, rng);
            if (r < t.GoldOrPotion + t.Weapon) return weaponAllowed ? Weapon(t, weaponPool, rng, false) : GoldOrPotion(t, rng);

            // Бомба / редкость: бомба, сердце (+20 HP) или оружие посильнее.
            float rt = t.RareBomb + t.RareHeart + t.RareWeapon;
            float x = rng.NextFloat() * (rt > 0f ? rt : 1f);
            if (x < t.RareBomb) return new LootRoll { Kind = LootKind.Bomb, ItemId = Bomb };
            if (x < t.RareBomb + t.RareHeart || !weaponAllowed) return new LootRoll { Kind = LootKind.Heart, ItemId = Heart };
            return Weapon(t, weaponPool, rng, true);
        }

        static LootRoll GoldOrPotion(in ChestLootTable t, IRandom rng)
        {
            if (rng.NextFloat() >= t.PotionShare)
                return new LootRoll { Kind = LootKind.Gold, Amount = rng.NextInt(t.ChestGoldMin, t.ChestGoldMax + 1) };
            float s = t.PotionSmall + t.PotionMana + t.PotionMixed + t.PotionBig;
            float x = rng.NextFloat() * (s > 0f ? s : 1f);
            string id = x < t.PotionSmall ? PotionSmall : x < t.PotionSmall + t.PotionMana ? PotionMana
                      : x < t.PotionSmall + t.PotionMana + t.PotionMixed ? PotionMixed : PotionBig;
            return new LootRoll { Kind = LootKind.Potion, ItemId = id };
        }

        /// <summary>Оружие по весу редкости; rare=true — без обычных. Легендарное и сплавы в сундуках не бывают.</summary>
        static LootRoll Weapon(in ChestLootTable t, IReadOnlyList<(string id, Rarity rarity)> pool, IRandom rng, bool rareOnly)
        {
            float c = rareOnly ? 0f : t.WeaponCommon, rr = t.WeaponRare, e = t.WeaponEpic;
            float x = rng.NextFloat() * (c + rr + e > 0f ? c + rr + e : 1f);
            var want = x < c ? Rarity.Common : x < c + rr ? Rarity.Rare : Rarity.Epic;
            for (int step = 0; step < 3; step++)
            {
                var fits = new List<string>();
                foreach (var w in pool) if (w.rarity == want) fits.Add(w.id);
                if (fits.Count > 0) return new LootRoll { Kind = LootKind.Weapon, ItemId = fits[rng.NextInt(0, fits.Count)] };
                want = want == Rarity.Epic ? Rarity.Rare : want == Rarity.Rare ? Rarity.Common : Rarity.Rare;
            }
            return new LootRoll { Kind = LootKind.Weapon, ItemId = pool[rng.NextInt(0, pool.Count)].id };
        }

        /// <summary>Пул сундука: без легендарных и сплавов.</summary>
        public static bool InChestPool(Rarity r) => r == Rarity.Common || r == Rarity.Rare || r == Rarity.Epic;
    }
}
