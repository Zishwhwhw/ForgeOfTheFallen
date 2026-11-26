using UnityEditor;
using UnityEngine;

namespace FotF.EditorTools.Art
{
    /// <summary>
    /// Настройки импорта из art bible §7 для всего, что лежит в Assets/_Project/Art.
    /// Применяются при каждом импорте, вручную их не меняют.
    /// </summary>
    public sealed class PixelArtImportPostprocessor : AssetPostprocessor
    {
        public const string ArtRoot = "Assets/_Project/Art/";
        public const int PixelsPerUnit = 32;

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(ArtRoot)) return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
        }
    }
}
