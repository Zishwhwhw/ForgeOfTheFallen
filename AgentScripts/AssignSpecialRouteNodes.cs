using UnityEditor;
using UnityEngine;

public static class AssignSpecialRouteNodes
{
    public static void Execute()
    {
        Set("Assets/Resources/Content/Routes/Nodes/Floor12.asset", 5);
        Set("Assets/Resources/Content/Routes/Nodes/Floor17.asset", 6);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Assigned Floor12=Shop and Floor17=Shrine in the authored route.");
    }

    private static void Set(string path, int nodeType)
    {
        var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
        if (asset == null) return;
        var serialized = new SerializedObject(asset);
        serialized.FindProperty("nodeType").enumValueIndex = nodeType;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
    }
}
