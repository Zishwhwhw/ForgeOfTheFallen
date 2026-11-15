using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public static class ArtImportAndSlice
{
    public static void Main()
    {
        const string heroPath = "Assets/Resources/Art/Generated/hero-gunsmith-v1.png";
        const string rosterPath = "Assets/Resources/Art/Generated/enemy-roster-v1.png";

        AssetDatabase.Refresh();
        ConfigureSingle(heroPath);
        ConfigureRoster(rosterPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void ConfigureSingle(string path)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 1024f;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();
    }

    private static void ConfigureRoster(string path)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 1024f;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();

        var factory = new SpriteDataProviderFactories();
        factory.Init();
        var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();

        var editProvider = provider.GetDataProvider<ISpriteFrameEditCapability>();
        if (editProvider == null)
            throw new System.InvalidOperationException("Sprite edit capability unavailable; slicing aborted.");

        var capability = editProvider.GetEditCapability();
        if (!capability.HasCapability(EEditCapability.CreateAndDeleteSprite))
            throw new System.InvalidOperationException("Sprite slice capability unavailable; slicing aborted.");

        var rects = new SpriteRect[3];
        for (var i = 0; i < rects.Length; i++)
        {
            var rect = new SpriteRect
            {
                name = i == 0 ? "enemy-slime" : i == 1 ? "enemy-chaser" : "enemy-elite",
                rect = new Rect(i * 724f, 0f, 724f, 724f),
                pivot = new Vector2(0.5f, 0.45f),
                alignment = SpriteAlignment.Custom,
                spriteID = GUID.Generate()
            };
            rects[i] = rect;
        }

        provider.SetSpriteRects(rects);
        provider.Apply();
        importer.SaveAndReimport();
    }
}
