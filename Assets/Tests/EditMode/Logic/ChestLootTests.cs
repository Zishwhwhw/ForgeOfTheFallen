using System.Collections.Generic;
using System.Linq;
using FotF.Data;
using FotF.Logic;
using FotF.Logic.Loot;
using NUnit.Framework;

namespace FotF.Tests.Logic
{
    /// <summary>Сундук (GDD §7.9): доли веток, лимит оружия на этаж, редкость, золото 10–25.</summary>
    public class ChestLootTests
    {
        static readonly ChestLootTable T = new ChestLootTable
        {
            GoldOrPotion = 0.6f, Weapon = 0.3f, BombOrRare = 0.1f, MaxWeaponsPerFloor = 2, ChestGoldMin = 10, ChestGoldMax = 25,
            PotionShare = 0.5f, PotionSmall = 0.5f, PotionMana = 0.25f, PotionMixed = 0.15f, PotionBig = 0.1f,
            RareBomb = 0.4f, RareHeart = 0.3f, RareWeapon = 0.3f, WeaponCommon = 0.55f, WeaponRare = 0.35f, WeaponEpic = 0.1f,
        };
        static readonly List<(string, Rarity)> Pool = new List<(string, Rarity)>
            { ("w.common", Rarity.Common), ("w.rare", Rarity.Rare), ("w.epic", Rarity.Epic) };

        [Test]
        public void BranchShares_MatchGdd()
        {
            var rng = new SeededRandom(1);
            var rolls = Enumerable.Range(0, 20000).Select(_ => ChestLoot.Roll(T, Pool, 0, rng)).ToList();
            float gp = rolls.Count(r => r.Kind == LootKind.Gold || r.Kind == LootKind.Potion) / 20000f;
            float w = rolls.Count(r => r.Kind == LootKind.Weapon) / 20000f;
            Assert.That(gp, Is.InRange(0.57f, 0.63f));
            Assert.That(w, Is.InRange(0.30f, 0.36f), "30% + часть «редкости»");
            Assert.IsTrue(rolls.Where(r => r.Kind == LootKind.Gold).All(r => r.Amount >= 10 && r.Amount <= 25));
        }

        [Test]
        public void WeaponCap_NoWeaponsAfterLimit()
        {
            var rng = new SeededRandom(2);
            Assert.IsFalse(Enumerable.Range(0, 5000).Any(_ => ChestLoot.Roll(T, Pool, 2, rng).Kind == LootKind.Weapon));
        }

        [Test]
        public void RareBranch_NeverGivesCommonWeapon()
        {
            var t = T; t.GoldOrPotion = 0; t.Weapon = 0; t.BombOrRare = 1; t.RareBomb = 0; t.RareHeart = 0; t.RareWeapon = 1;
            var rng = new SeededRandom(3);
            Assert.IsTrue(Enumerable.Range(0, 2000).Select(_ => ChestLoot.Roll(t, Pool, 0, rng)).All(r => r.ItemId != "w.common"));
        }

        [Test]
        public void MissingRarity_FallsBackToExisting()
        {
            var rng = new SeededRandom(4);
            var onlyCommon = new List<(string, Rarity)> { ("w.common", Rarity.Common) };
            var t = T; t.GoldOrPotion = 0; t.BombOrRare = 0;
            Assert.IsTrue(Enumerable.Range(0, 500).Select(_ => ChestLoot.Roll(t, onlyCommon, 0, rng)).All(r => r.ItemId == "w.common"));
        }

        [Test]
        public void ChestPool_ExcludesLegendaryAndHybrids()
        {
            Assert.IsFalse(ChestLoot.InChestPool(Rarity.Legendary));
            Assert.IsFalse(ChestLoot.InChestPool(Rarity.Franken));
            Assert.IsTrue(ChestLoot.InChestPool(Rarity.Epic));
        }
    }
}
