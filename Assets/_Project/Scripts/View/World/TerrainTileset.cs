using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace FotF.View.World
{
    /// <summary>
    /// Тайлсет dual-grid (R3 Замка): имя тайла заканчивается кодом углов TL TR BL BR,
    /// u — пустота, t — верх стены, l — пол (например tile_castle_r3_uutt). Нужен DualGridPainter.
    /// </summary>
    [CreateAssetMenu(menuName = "FotF/World/Terrain Tileset", fileName = "tileset_")]
    public sealed class TerrainTileset : ScriptableObject
    {
        public TileBase[] tiles = System.Array.Empty<TileBase>();
        [Tooltip("Невидимый тайл коллизии стен")]
        public TileBase collisionTile;

        Dictionary<string, TileBase> _byCode;

        public TileBase Get(string code)
        {
            if (_byCode == null)
            {
                _byCode = new Dictionary<string, TileBase>();
                foreach (var t in tiles)
                    if (t && t.name.Length >= 4) _byCode[t.name.Substring(t.name.Length - 4)] = t;
            }
            return _byCode.TryGetValue(code, out var tile) ? tile : null;
        }

        void OnValidate() => _byCode = null;
    }
}
