using System.Linq;
using FotF.Logic;
using FotF.Logic.Generation;
using NUnit.Framework;

namespace FotF.Tests.Logic
{
    /// <summary>Мини-карта (GDD §3.2): туман, соседние видны, значки только у вывесочных типов, секреты скрыты.</summary>
    public class FloorMapFogTests
    {
        static FloorPlan Plan()
        {
            // Старт — Бой — Бой, от первого боя: торговец, кузница, секрет.
            var p = new FloorPlan();
            FloorRoom Add(RoomKind k, int x, bool main) { var r = new FloorRoom { Index = p.Rooms.Count, Kind = k, Cell = new Cell(x, 0), OnMainPath = main }; p.Rooms.Add(r); return r; }
            void Link(FloorRoom a, FloorRoom b) { a.Links.Add(b.Index); b.Links.Add(a.Index); }
            var s = Add(RoomKind.Start, 0, true); var c1 = Add(RoomKind.Combat, 1, true); var c2 = Add(RoomKind.Combat, 2, true);
            var m = Add(RoomKind.Merchant, 10, false); var f = Add(RoomKind.Forge, 11, false); var sec = Add(RoomKind.Secret, 12, false);
            Link(s, c1); Link(c1, c2); Link(c1, m); Link(c1, f); Link(c1, sec);
            return p;
        }

        [Test]
        public void AtStart_OnlyStartAndNeighbours()
        {
            var p = Plan(); var fog = new FloorMapFog(p); fog.Enter(0);
            Assert.AreEqual(FloorMapFog.Visibility.Visited, fog.Of(p.Rooms[0]));
            Assert.AreEqual(FloorMapFog.Visibility.Seen, fog.Of(p.Rooms[1]));
            Assert.AreEqual(FloorMapFog.Visibility.Hidden, fog.Of(p.Rooms[2]));
            Assert.AreEqual(FloorMapFog.Visibility.Hidden, fog.Of(p.Rooms[3]));
        }

        [Test]
        public void Neighbour_Merchant_ShowsIcon_Forge_DoesNot_Secret_Hidden()
        {
            var p = Plan(); var fog = new FloorMapFog(p); fog.Enter(0); fog.Enter(1);
            Assert.IsTrue(fog.ShowsIcon(p.Rooms[3]), "торговец — значок на двери");
            Assert.AreEqual(FloorMapFog.Visibility.Seen, fog.Of(p.Rooms[4]));
            Assert.IsFalse(fog.ShowsIcon(p.Rooms[4]), "кузница скрыта до захода");
            Assert.AreEqual(FloorMapFog.Visibility.Hidden, fog.Of(p.Rooms[5]), "секрет не виден");
            fog.Enter(4);
            Assert.IsTrue(fog.ShowsIcon(p.Rooms[4]), "после захода — значок");
        }

        [Test]
        public void Enter_SameRoomTwice_NoChange()
        {
            var fog = new FloorMapFog(Plan());
            Assert.IsTrue(fog.Enter(0)); Assert.IsFalse(fog.Enter(0)); Assert.IsTrue(fog.Enter(1));
        }

        [Test]
        public void GeneratedFloor_StartNeighboursAreVisible()
        {
            var p = FloorGenerator.Generate(new FloorRules { CombatMin = 4, CombatMax = 5, MaxBranches = 5, Merchant = 1f }, new SeededRandom(3));
            var fog = new FloorMapFog(p); fog.Enter(p.Start.Index);
            foreach (var i in p.Start.Links) Assert.AreNotEqual(FloorMapFog.Visibility.Hidden, fog.Of(p.Rooms[i]));
            Assert.IsTrue(p.Rooms.Any(r => fog.Of(r) == FloorMapFog.Visibility.Hidden));
        }
    }
}
