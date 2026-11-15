using System.IO;
using FrankenArms;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CreateGameScenes
{
    public static void Main()
    {
        var folder = "Assets/Scenes";
        Directory.CreateDirectory(folder);
        CreateMenuScene(folder + "/MainMenu.unity");
        CreateLobbyScene(folder + "/Lobby.unity");
        CreateRunScene(folder + "/Run.unity");
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Lobby.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Run.unity", true)
        };
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", OpenSceneMode.Single);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }

    private static void CreateMenuScene(string path)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCamera("MainMenu Camera");
        var root = new GameObject("Main Menu");
        root.AddComponent<MainMenuController>();
        SetBackground(scene, new Color(0.025f, 0.018f, 0.05f, 1f));
        EditorSceneManager.SaveScene(scene, path);
    }

    private static void CreateLobbyScene(string path)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCamera("Lobby Camera");
        var root = new GameObject("Lobby Manor");
        root.AddComponent<LobbyController>();
        SetBackground(scene, new Color(0.045f, 0.035f, 0.065f, 1f));
        EditorSceneManager.SaveScene(scene, path);
    }

    private static void CreateRunScene(string path)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCamera("Run Camera");
        SetBackground(scene, new Color(0.015f, 0.02f, 0.04f, 1f));
        EditorSceneManager.SaveScene(scene, path);
    }

    private static void CreateCamera(string name)
    {
        var cameraObject = new GameObject(name);
        var camera = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        camera.tag = "MainCamera";
        camera.transform.position = new Vector3(0f, 0f, -10f);
    }

    private static void SetBackground(Scene scene, Color color)
    {
        RenderSettings.skybox = null;
        var camera = Camera.main;
        if (camera != null)
        {
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = color;
        }
    }
}
