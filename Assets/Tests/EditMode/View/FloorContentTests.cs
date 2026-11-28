using System;
using System.Collections.Generic;
using System.Linq;
using FotF.Logic;
using FotF.Logic.Generation;
using FotF.View.World;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace FotF.Tests.View
{
    /// <summary>Этаж (GDD §8): шаблоны комнат, проводка, раскладка без дыр в тайлсете и без пересечений.</summary>
    public class FloorContentTests
    {
        static RoomTemplate[] Templates() => AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Project/Prefabs/Rooms" })
            .Select(g => AssetDatabase.LoadAssetAtPath<RoomTemplate>(AssetDatabase.GUIDToAssetPath(g))).Where(t => t).ToArray();

        static TerrainTileset Tileset() => AssetDatabase.LoadAssetAtPath<TerrainTileset>("Assets/_Project/Art/Tiles/Castle/tileset_castle_r3.asset");

        static readonly FloorRules Castle = new FloorRules
        {
            CombatMin = 4, CombatMax = 5, MaxBranches = 5,
            Forge = 0.4f, Merchant = 1f, Fountain = 0.25f, Shrine = 0.35f, Cage = 0.25f, Secret = 0.4f, Elite = 0.5f, MiniBoss = 0.3f, ChestRoom = 0.3f,
        };

        [Test]
        public void EveryRoomKind_HasTemplate()
        {
            var t = Templates();
            foreach (RoomKind k in Enum.GetValues(typeof(RoomKind)))
                Assert.IsTrue(t.Any(x => x.Serves(k)), $"нет шаблона для {k}");
        }

        [Test]
        public void Templates_HaveCorridorEntrances_AndWiring()
        {
            foreach (var t in Templates())
            {
                Assert.IsNotNull(t.shape, t.name);
                var b = t.ShapeBounds;
                int cx = b.xMin + b.size.x / 2, cy = b.yMin + b.size.y / 2;
                foreach (var p in new[] { new Vector3Int(b.xMin, cy, 0), new Vector3Int(b.xMax - 1, cy, 0), new Vector3Int(cx, b.yMin, 0), new Vector3Int(cx, b.yMax - 1, 0) })
                    for (int w = -1; w <= 1; w++)
                    {
                        var q = p.x == b.xMin || p.x == b.xMax - 1 ? p + new Vector3Int(0, w, 0) : p + new Vector3Int(w, 0, 0);
                        Assert.IsTrue(t.shape.HasTile(q), $"{t.name}: у края {q} нет пола — коридор упрётся в стену");
                    }
                if (t.Serves(RoomKind.Start)) Assert.IsNotNull(t.heroSpawn, t.name);
                if (t.Serves(RoomKind.Combat) || t.Serves(RoomKind.Elite))
                {
                    Assert.IsNotNull(t.fight, $"{t.name}: нет RoomController");
                    var so = new SerializedObject(t.fight);
                    Assert.IsNotNull(so.FindProperty("encounter").objectReferenceValue, t.name);
                    Assert.IsNotNull(so.FindProperty("chestPrefab").objectReferenceValue, t.name);
                    Assert.GreaterOrEqual(so.FindProperty("spawnPoints").arraySize, 4, t.name);
                    Assert.IsTrue(t.fight.GetComponent<Collider2D>().isTrigger, t.name);
                }
            }
        }

        [Test]
        public void ThreeHundredFloors_NoMissingTiles_NoOverlaps()
        {
            var templates = Templates();
            var set = Tileset();
            var missing = new HashSet<string>();
            for (int seed = 1; seed <= 300; seed++)
            {
                var rng = new SeededRandom(seed);
                var plan = FloorGenerator.Generate(Castle, rng);
                var layout = FloorLayout.Compute(plan, r => { var fits = templates.Where(t => t.Serves(r.Kind)).ToArray(); return fits[(seed + r.Index) % fits.Length]; }, new Vector2Int(30, 24), 1);
                var rooms = layout.Rooms.Values.ToList();
                for (int i = 0; i < rooms.Count; i++)
                    for (int j = i + 1; j < rooms.Count; j++)
                    {
                        var a = rooms[i].Bounds; var b = rooms[j].Bounds;
                        bool apart = a.xMax + 3 <= b.xMin || b.xMax + 3 <= a.xMin || a.yMax + 4 <= b.yMin || b.yMax + 4 <= a.yMin;
                        Assert.IsTrue(apart, $"seed {seed}: комнаты слишком близко {a} {b}");
                    }
                int minX = layout.Walk.Min(c => c.x) - 2, maxX = layout.Walk.Max(c => c.x) + 2;
                int minY = layout.Walk.Min(c => c.y) - 2, maxY = layout.Walk.Max(c => c.y) + 3;
                for (int y = minY; y <= maxY; y++)
                    for (int x = minX; x <= maxX; x++)
                    {
                        string code = DualGridPainter.CellCode(layout.Walk, x, y);
                        if (set.Get(code) == null) missing.Add(code);
                    }
            }
            Assert.IsEmpty(missing, "в тайлсете нет тайлов: " + string.Join(", ", missing));
        }

        [Test]
        public void Painter_Rectangle_MatchesHandMadeGreyBoxRoom()
        {
            // Комната серой коробки: пол x −8..7, y −5..2; стена — ряд над полом (как в GreyBox.unity).
            var walk = new HashSet<Vector2Int>();
            for (int y = -5; y <= 2; y++) for (int x = -8; x <= 7; x++) walk.Add(new Vector2Int(x, y));
            Assert.AreEqual("ttll", DualGridPainter.CellCode(walk, 0, 3));
            Assert.AreEqual("uutt", DualGridPainter.CellCode(walk, 0, 4));
            Assert.AreEqual("utul", DualGridPainter.CellCode(walk, -9, 3));
            Assert.AreEqual("uluu", DualGridPainter.CellCode(walk, -9, -6));
            Assert.AreEqual("llll", DualGridPainter.CellCode(walk, 0, 0));
        }
    }
}
