using System.Collections.Generic;
using FotF.Logic.Generation;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace FotF.View.World
{
    /// <summary>
    /// Шаблон комнаты этажа (ARCHITECTURE §3–4): префаб, собранный в редакторе. Форма — тайлмап Shape
    /// (закрашенные клетки = пол; рендер выключен), стены рисует DualGridPainter. Двери — по центрам сторон
    /// границы формы (там, где к комнате подходит коридор). Боевая комната — с RoomController.
    /// </summary>
    public sealed class RoomTemplate : MonoBehaviour
    {
        [Tooltip("Для каких типов комнат годится шаблон")]
        public RoomKind[] kinds = { RoomKind.Combat };
        [Tooltip("Форма пола: закрашенные клетки. Центры всех четырёх сторон границы должны быть полом")]
        public Tilemap shape;
        [Tooltip("Бой: волны, двери, сундук")]
        public RoomController fight;
        [Tooltip("Старт: где появляется герой")]
        public Transform heroSpawn;
        [Tooltip("Где появится портал, если комната зачищена последней")]
        public Transform portalPoint;

        public BoundsInt ShapeBounds
        {
            get { shape.CompressBounds(); return shape.cellBounds; }
        }

        /// <summary>Клетки пола в координатах шаблона.</summary>
        public IEnumerable<Vector2Int> LocalCells()
        {
            foreach (var p in ShapeBounds.allPositionsWithin)
                if (shape.HasTile(p)) yield return new Vector2Int(p.x, p.y);
        }

        public bool Serves(RoomKind kind) => System.Array.IndexOf(kinds, kind) >= 0;
    }
}
