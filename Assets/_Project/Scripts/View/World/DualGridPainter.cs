using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace FotF.View.World
{
    /// <summary>
    /// Рисует пол и стены этажа по маске проходимых клеток (комнаты любой формы + коридоры).
    /// Угол (вершина) — «пол», если касается проходимой клетки; «верх стены» — на одну вершину выше пола;
    /// иначе пустота. Тайл клетки выбирается по 4 углам. Коллизия — все нарисованные непроходимые клетки.
    /// </summary>
    public static class DualGridPainter
    {
        public enum Corner { Void, WallTop, Floor }

        public static Corner CornerAt(HashSet<Vector2Int> walk, int x, int y)
        {
            if (IsFloor(walk, x, y)) return Corner.Floor;
            return IsFloor(walk, x, y - 1) ? Corner.WallTop : Corner.Void;
        }

        static bool IsFloor(HashSet<Vector2Int> walk, int x, int y) =>
            walk.Contains(new Vector2Int(x - 1, y - 1)) || walk.Contains(new Vector2Int(x, y - 1)) ||
            walk.Contains(new Vector2Int(x - 1, y)) || walk.Contains(new Vector2Int(x, y));

        static char Code(Corner c) => c == Corner.Floor ? 'l' : c == Corner.WallTop ? 't' : 'u';

        /// <summary>Код углов клетки: TL TR BL BR.</summary>
        public static string CellCode(HashSet<Vector2Int> walk, int x, int y) => new string(new[]
        {
            Code(CornerAt(walk, x, y + 1)), Code(CornerAt(walk, x + 1, y + 1)),
            Code(CornerAt(walk, x, y)), Code(CornerAt(walk, x + 1, y)),
        });

        /// <summary>Нарисовать. Возвращает коды углов, для которых в тайлсете нет тайла (для проверки арта).</summary>
        public static HashSet<string> Paint(HashSet<Vector2Int> walk, Tilemap floor, Tilemap collision, TerrainTileset set)
        {
            var missing = new HashSet<string>();
            if (walk.Count == 0) return missing;
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            foreach (var c in walk) { minX = Mathf.Min(minX, c.x); minY = Mathf.Min(minY, c.y); maxX = Mathf.Max(maxX, c.x); maxY = Mathf.Max(maxY, c.y); }

            var positions = new List<Vector3Int>();
            var floorTiles = new List<TileBase>();
            var wallPositions = new List<Vector3Int>();
            for (int y = minY - 2; y <= maxY + 3; y++)
                for (int x = minX - 2; x <= maxX + 2; x++)
                {
                    string code = CellCode(walk, x, y);
                    // Пустая клетка рядом со стеной тоже закрашивается тёмной заливкой, чтобы за стеной не было «дыр».
                    if (code == "uuuu" && !NearWall(walk, x, y)) continue;
                    var tile = set.Get(code);
                    if (tile == null) { missing.Add(code); tile = set.Get("uuuu"); }
                    var p = new Vector3Int(x, y, 0);
                    positions.Add(p); floorTiles.Add(tile);
                    if (!walk.Contains(new Vector2Int(x, y))) wallPositions.Add(p);
                }
            floor.SetTiles(positions.ToArray(), floorTiles.ToArray());
            if (collision && set.collisionTile)
            {
                var cols = new TileBase[wallPositions.Count];
                for (int i = 0; i < cols.Length; i++) cols[i] = set.collisionTile;
                collision.SetTiles(wallPositions.ToArray(), cols);
            }
            return missing;
        }

        static bool NearWall(HashSet<Vector2Int> walk, int x, int y)
        {
            for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                    if (CellCode(walk, x + dx, y + dy) != "uuuu") return true;
            return false;
        }
    }
}
