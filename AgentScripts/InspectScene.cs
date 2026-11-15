using FrankenArms;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class InspectScene
{
    public static void Main()
    {
        Debug.Log(string.Format("Scene={0} mainMenu={1} lobby={2} bootstrap={3} player={4}",
            SceneManager.GetActiveScene().name,
            Object.FindAnyObjectByType<MainMenuController>() != null,
            Object.FindAnyObjectByType<LobbyController>() != null,
            Object.FindAnyObjectByType<FrankenArmsBootstrap>() != null,
            Object.FindAnyObjectByType<PlayerMovement>() != null));
    }
}
