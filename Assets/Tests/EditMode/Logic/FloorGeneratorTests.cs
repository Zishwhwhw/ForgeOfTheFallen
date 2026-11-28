using System.Linq;
using FotF.Logic;
using FotF.Logic.Generation;
using NUnit.Framework;

namespace FotF.Tests.Logic
{
    /// <summary>Генератор этажа: гарантии GDD §8.2 на многих seed, детерминизм, число боевых (решение 28.11: 4–5).</summary>
    public class FloorGeneratorTests
    {
        static readonly FloorRules Castle = new FloorRules
        {
            CombatMin = 4, CombatMax = 5, MaxBranches = 5,
            Forge = 0.4f, Merchant = 1f, Fountain = 0.25f, Shrine = 0.35f, Cage = 0.25f, Secret = 0.4f, Elite = 0.5f, MiniBoss = 0.3f, ChestRoom = 0.3f,
        };

        [Test]
        public void ThousandSeeds_AllPassGuarantees()
        {
            for (int seed = 0; seed < 1000; seed++)
            {
                var plan = FloorGenerator.Generate(Castle, new SeededRandom(seed));
                Assert.IsTrue(FloorGenerator.Validate(plan, Castle, out var err), $"seed {seed}: {err}");
            }
        }

        [Test]
        public void SameSeed_SamePlan_DifferentSeed_UsuallyDifferent()
        {
            string Sig(int s) => string.Join(";", FloorGenerator.Generate(Castle, new SeededRandom(s)).Rooms.Select(r => $"{r.Kind}{r.Cell}"));
            Assert.AreEqual(Sig(42), Sig(42));
            Assert.Greater(Enumerable.Range(0, 20).Select(Sig).Distinct().Count(), 15);
        }

        [Test]
        public void CombatCount_IsFourOrFive_AndBothHappen()
        {
            var counts = Enumerable.Range(0, 300).Select(s => FloorGenerator.Generate(Castle, new SeededRandom(s)).Rooms.Count(r => r.Kind == RoomKind.Combat)).ToList();
            Assert.IsTrue(counts.All(c => c == 4 || c == 5));
            Assert.Contains(4, counts); Assert.Contains(5, counts);
        }

        [Test]
        public void Branches_AreLeaves_OffMainPath_AndRespectLimit()
        {
            for (int seed = 0; seed < 300; seed++)
            {
                var plan = FloorGenerator.Generate(Castle, new SeededRandom(seed));
                var branches = plan.Rooms.Where(r => !r.OnMainPath).ToList();
                Assert.LessOrEqual(branches.Count, Castle.MaxBranches);
                foreach (var b in branches)
                {
                    Assert.AreEqual(1, b.Links.Count, $"seed {seed}: {b.Kind}");
                    Assert.IsTrue(plan.Rooms[b.Links[0]].OnMainPath);
                    Assert.IsFalse(b.RequiredForPortal, "ответвление не обязательно для портала");
                }
            }
        }

        [Test]
        public void ForgeAppears_RoughlyAtItsChance()
        {
            int withForge = Enumerable.Range(0, 1000).Count(s => FloorGenerator.Generate(Castle, new SeededRandom(s)).Rooms.Any(r => r.Kind == RoomKind.Forge));
            Assert.That(withForge, Is.InRange(300, 500), "GDD: кузница на 30–50% этажей");
        }

        [Test]
        public void EliteAndMiniBoss_NeverHangOffStart()
        {
            for (int seed = 0; seed < 500; seed++)
            {
                var plan = FloorGenerator.Generate(Castle, new SeededRandom(seed));
                foreach (var r in plan.Rooms.Where(r => r.Kind == RoomKind.Elite || r.Kind == RoomKind.MiniBoss))
                    Assert.AreEqual(RoomKind.Combat, plan.Rooms[r.Links[0]].Kind);
            }
        }

        [Test]
        public void MainPath_IsAChain_FromStart()
        {
            var plan = FloorGenerator.Generate(Castle, new SeededRandom(7));
            var main = plan.Rooms.Where(r => r.OnMainPath).OrderBy(r => r.Depth).ToList();
            for (int i = 0; i < main.Count; i++) Assert.AreEqual(i, main[i].Depth);
            for (int i = 1; i < main.Count; i++) Assert.IsTrue(plan.Linked(main[i - 1], main[i]));
        }
    }
}
