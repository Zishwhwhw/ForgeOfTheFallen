using UnityEngine;

namespace FrankenArms
{
    public sealed class MainMenuController : MonoBehaviour
    {
        private void OnGUI()
        {
            var previous = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 1280f, Screen.height / 720f, 1f));
            var center = 640f;
            GUI.Box(new Rect(center - 250f, 110f, 500f, 360f), "FRANKENARMS");
            GUI.Label(new Rect(center - 205f, 170f, 410f, 30f), "A ROOM-BY-ROOM ARSENAL ROGUELITE");
            GUI.Label(new Rect(center - 205f, 215f, 410f, 24f), "Experiment. Survive. Bring something home.");
            if (GUI.Button(new Rect(center - 170f, 280f, 340f, 48f), "ENTER LOBBY / MANOR"))
                SceneFlow.GoToLobby();
            if (GUI.Button(new Rect(center - 170f, 342f, 340f, 36f), "QUIT"))
                Application.Quit();
            if (GUI.Button(new Rect(center - 170f, 388f, 340f, 30f), LocalizationService.T("language") + ": " + LocalizationService.CurrentLanguage))
                LocalizationService.CycleLanguage();
            GUI.Label(new Rect(center - 205f, 420f, 410f, 24f), "Offline single-player // v0.1 vertical slice");
            GUI.matrix = previous;
        }
    }
}
