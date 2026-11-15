using UnityEditor;

public static class RefreshScripts
{
    public static void Main()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
    }
}
