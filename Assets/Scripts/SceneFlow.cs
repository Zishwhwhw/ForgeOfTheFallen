using UnityEngine;
using UnityEngine.SceneManagement;

namespace FrankenArms
{
    public static class SceneFlow
    {
        public const string MainMenu = "MainMenu";
        public const string Lobby = "Lobby";
        public const string Run = "Run";

        public static void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(MainMenu);
        }

        public static void GoToLobby()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(Lobby);
        }

        public static void StartRun()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(Run);
        }
    }
}
