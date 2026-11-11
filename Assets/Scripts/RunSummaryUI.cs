using UnityEngine;
using UnityEngine.InputSystem;

namespace FrankenArms
{
    public sealed class RunSummaryUI : MonoBehaviour
    {
        private CombatRoom room;

        internal void Initialize(CombatRoom target)
        {
            room = target;
        }

        private void Update()
        {
            if (room == null || !room.IsCleared || Keyboard.current == null) return;
            if (Keyboard.current.nKey.wasPressedThisFrame) room.StartNextRoom();
        }

        private void OnGUI()
        {
            if (room == null || !room.IsCleared) return;
            GUI.Box(new Rect(Screen.width - 340f, 20f, 315f, 105f), "ROOM CLEARED");
            GUI.Label(new Rect(Screen.width - 320f, 52f, 280f, 24f), string.Format("Room {0} complete", room.RoomIndex));
            GUI.Label(new Rect(Screen.width - 320f, 76f, 280f, 24f), "Press N to enter the next room");
            GUI.Label(new Rect(Screen.width - 320f, 100f, 280f, 24f), string.Format("Credits: {0}", RunProgress.Credits));
        }
    }
}
