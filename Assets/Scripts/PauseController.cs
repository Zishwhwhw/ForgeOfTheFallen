using UnityEngine;
using UnityEngine.InputSystem;

namespace FrankenArms
{
    /// <summary>GDD pause/settings overlay for an active run.</summary>
    public sealed class PauseController : MonoBehaviour
    {
        private bool paused;
        private bool settings;
        private HeroSelectionController heroSelection;

        private void Start()
        {
            heroSelection = FindAnyObjectByType<HeroSelectionController>();
        }

        private void Update()
        {
            if (heroSelection != null && !heroSelection.IsSelected) return;
            var keyboardPressed = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
            var gamepadPressed = Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame;
            if (keyboardPressed || gamepadPressed)
                SetPaused(!paused);
        }

        private void SetPaused(bool value)
        {
            paused = value;
            settings = false;
            Time.timeScale = paused ? 0f : 1f;
        }

        private void OnGUI()
        {
            if (!paused) return;
            var shade = new Color(0.02f, 0.02f, 0.05f, 0.82f);
            var old = GUI.color;
            GUI.color = shade;
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = old;

            var panel = new Rect(Screen.width * 0.5f - 190f, Screen.height * 0.5f - 180f, 380f, 360f);
            GUI.Box(panel, settings ? "SETTINGS" : "PAUSED");
            if (!settings)
            {
                if (GUI.Button(new Rect(panel.x + 45f, panel.y + 70f, 290f, 42f), "CONTINUE")) SetPaused(false);
                if (GUI.Button(new Rect(panel.x + 45f, panel.y + 125f, 290f, 42f), "SETTINGS")) settings = true;
                if (GUI.Button(new Rect(panel.x + 45f, panel.y + 180f, 290f, 42f), "SURRENDER TO LOBBY")) SceneFlow.GoToLobby();
                GUI.Label(new Rect(panel.x + 45f, panel.y + 250f, 290f, 24f), "ESC  resume / pause");
                return;
            }

            GUI.Label(new Rect(panel.x + 45f, panel.y + 65f, 290f, 24f), "MASTER  " + Mathf.RoundToInt(GameSettings.MasterVolume * 100f) + "%");
            if (GUI.Button(new Rect(panel.x + 45f, panel.y + 92f, 135f, 32f), "MASTER -")) GameSettings.SetMaster(GameSettings.MasterVolume - 0.1f);
            if (GUI.Button(new Rect(panel.x + 200f, panel.y + 92f, 135f, 32f), "MASTER +")) GameSettings.SetMaster(GameSettings.MasterVolume + 0.1f);
            GUI.Label(new Rect(panel.x + 45f, panel.y + 135f, 290f, 24f), "SFX  " + Mathf.RoundToInt(GameSettings.SfxVolume * 100f) + "%");
            if (GUI.Button(new Rect(panel.x + 45f, panel.y + 162f, 135f, 32f), "SFX -")) GameSettings.SetSfx(GameSettings.SfxVolume - 0.1f);
            if (GUI.Button(new Rect(panel.x + 200f, panel.y + 162f, 135f, 32f), "SFX +")) GameSettings.SetSfx(GameSettings.SfxVolume + 0.1f);
            if (GUI.Button(new Rect(panel.x + 45f, panel.y + 210f, 290f, 32f), "SCREEN SHAKE: " + (GameSettings.ScreenShake ? "ON" : "OFF"))) GameSettings.SetScreenShake(!GameSettings.ScreenShake);
            if (GUI.Button(new Rect(panel.x + 45f, panel.y + 260f, 290f, 32f), "BACK")) settings = false;
        }
    }
}
