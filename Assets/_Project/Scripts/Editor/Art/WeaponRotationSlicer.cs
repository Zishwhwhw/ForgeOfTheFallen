using System.Linq;
using FotF.Data;
using UnityEditor;
using UnityEngine;

namespace FotF.EditorTools.Art
{
    /// <summary>
    /// Кадры RotSprite (Tools/art/bake_weapon_rotations.py: Rotated/&lt;оружие&gt;/&lt;оружие&gt;_rot_NN.png) →
    /// опора в пикселе хвата (центр кадра) → WeaponDefinition.rotatedSprites по порядку углов.
    /// </summary>
    public static class WeaponRotationSlicer
    {
        const string RotatedDir = "Assets/_Project/Art/Sprites/Weapons/Rotated";

        [MenuItem("FotF/Art/Assign Weapon Rotations")]
        public static void AssignAll()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:WeaponDefinition"))
            {
                var def = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (def == null || def.sprite == null) continue;
                string dir = $"{RotatedDir}/{def.sprite.texture.name}";
                if (!AssetDatabase.IsValidFolder(dir)) continue;
                var paths = AssetDatabase.FindAssets("t:Texture2D", new[] { dir })
                    .Select(AssetDatabase.GUIDToAssetPath).OrderBy(p => p).ToArray();
                foreach (var p in paths) SetGripPivot(p);
                def.rotatedSprites = paths.Select(AssetDatabase.LoadAssetAtPath<Sprite>).ToArray();
                EditorUtility.SetDirty(def);
                AssetDatabase.SaveAssetIfDirty(def);
            }
        }

        static void SetGripPivot(string path)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            int n = tex.height;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMode = (int)SpriteImportMode.Single;
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            // Хват в пикселе (n/2 слева, n/2 сверху) → опора в центре этого пикселя.
            settings.spritePivot = new Vector2((n / 2 + 0.5f) / n, (n - n / 2 - 0.5f) / n);
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }
    }
}
