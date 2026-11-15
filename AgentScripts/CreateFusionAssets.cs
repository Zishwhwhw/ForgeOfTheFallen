using System.IO;
using UnityEditor;
using UnityEngine;

public static class CreateFusionAssets
{
    public static void Main()
    {
        var folder = "Assets/Resources/Content/Fusions";
        Directory.CreateDirectory(folder);
        Create(folder, "EmberArc", "Ember Arc", 3, 5.5f, 9.5f, new Color(1f, 0.45f, 0.2f, 1f), "balanced fire arc");
        Create(folder, "RapidEmber", "Rapid Ember", 2, 9f, 10f, new Color(1f, 0.75f, 0.2f, 1f), "rapid pressure");
        Create(folder, "HeavyArc", "Heavy Arc", 5, 2.5f, 12f, new Color(0.55f, 0.8f, 1f, 1f), "heavy impact");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }

    private static void Create(string folder, string fileName, string displayName, int damage, float rate, float speed, Color tint, string role)
    {
        var path = folder + "/" + fileName + ".asset";
        var recipe = AssetDatabase.LoadAssetAtPath<FrankenArms.WeaponFusionRecipe>(path);
        if (recipe == null)
        {
            recipe = ScriptableObject.CreateInstance<FrankenArms.WeaponFusionRecipe>();
            AssetDatabase.CreateAsset(recipe, path);
        }
        recipe.displayName = displayName;
        recipe.damage = damage;
        recipe.shotsPerSecond = rate;
        recipe.projectileSpeed = speed;
        recipe.tint = tint;
        recipe.role = role;
        EditorUtility.SetDirty(recipe);
    }
}
