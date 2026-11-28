using System;
using System.Collections.Generic;
using FotF.Logic.Generation;
using UnityEngine;

namespace FotF.View.World
{
    /// <summary>
    /// Раскладка этажа на тайлах (без сцены): где стоит шаблон каждой комнаты, клетки пола комнат и коридоров,
    /// места дверей. Комната центрируется в клетке сетки; коридор идёт по линии центров соседних комнат.
    /// </summary>
    public sealed class FloorLayout
    {
        public struct Placed { public RoomTemplate Template; public Vector2Int Offset; public BoundsInt Bounds; }
        public struct Door { public FloorRoom Room; public Vector2 Position; public bool Sideways; }

        public readonly HashSet<Vector2Int> Walk = new HashSet<Vector2Int>();
        public readonly Dictionary<FloorRoom, Placed> Rooms = new Dictionary<FloorRoom, Placed>();
        public readonly List<Door> Doors = new List<Door>();

        public static FloorLayout Compute(FloorPlan plan, Func<FloorRoom, RoomTemplate> pick, Vector2Int pitch, int halfWidth)
        {
            var L = new FloorLayout();
            foreach (var room in plan.Rooms)
            {
                if (room.Hidden) continue;                                   // секреты — с бомбами (следующая поставка)
                var t = pick(room);
                if (t == null) continue;
                var sb = t.ShapeBounds;
                var center = new Vector2Int(room.Cell.X * pitch.x, room.Cell.Y * pitch.y);
                var offset = center - new Vector2Int(sb.xMin + sb.size.x / 2, sb.yMin + sb.size.y / 2);
                foreach (var c in t.LocalCells()) L.Walk.Add(c + offset);
                L.Rooms[room] = new Placed { Template = t, Offset = offset, Bounds = new BoundsInt(sb.xMin + offset.x, sb.yMin + offset.y, 0, sb.size.x, sb.size.y, 1) };
            }
            foreach (var (a0, b0) in plan.Links())
            {
                if (!L.Rooms.ContainsKey(a0) || !L.Rooms.ContainsKey(b0)) continue;
                var (a, b) = b0.Cell.X < a0.Cell.X || b0.Cell.Y < a0.Cell.Y ? (b0, a0) : (a0, b0);   // a — левее/ниже
                var ba = L.Rooms[a].Bounds; var bb = L.Rooms[b].Bounds;
                if (b.Cell.X != a.Cell.X)
                {
                    int y = a.Cell.Y * pitch.y;
                    for (int x = ba.xMax; x < bb.xMin; x++)
                        for (int w = -halfWidth; w <= halfWidth; w++) L.Walk.Add(new Vector2Int(x, y + w));
                    L.Doors.Add(new Door { Room = a, Position = new Vector2(ba.xMax + 0.5f, y + 0.5f), Sideways = true });
                    L.Doors.Add(new Door { Room = b, Position = new Vector2(bb.xMin - 0.5f, y + 0.5f), Sideways = true });
                }
                else
                {
                    int x = a.Cell.X * pitch.x;
                    for (int y = ba.yMax; y < bb.yMin; y++)
                        for (int w = -halfWidth; w <= halfWidth; w++) L.Walk.Add(new Vector2Int(x + w, y));
                    L.Doors.Add(new Door { Room = a, Position = new Vector2(x + 0.5f, ba.yMax + 0.5f), Sideways = false });
                    L.Doors.Add(new Door { Room = b, Position = new Vector2(x + 0.5f, bb.yMin - 0.5f), Sideways = false });
                }
            }
            return L;
        }
    }
}
