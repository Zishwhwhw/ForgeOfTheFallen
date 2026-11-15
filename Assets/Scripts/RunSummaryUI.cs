using UnityEngine;
using UnityEngine.InputSystem;

namespace FrankenArms
{
    public sealed class RunSummaryUI : MonoBehaviour
    {
        private CombatRoom room;
        private bool resultRecorded;

        internal void Initialize(CombatRoom target)
        {
            room = target;
        }

        private void Update()
        {
            if (room == null || !room.IsCleared || Keyboard.current == null) return;
            if (room.RoomIndex >= 24) return;
            if (Keyboard.current.digit1Key.wasPressedThisFrame) room.StartNextRoom(1);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) room.StartNextRoom(2);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) room.StartNextRoom(3);
        }

        private void OnGUI()
        {
            if (room == null || !room.IsCleared) return;
            if (!resultRecorded && room.RoomIndex >= 24)
            {
                resultRecorded = true;
                RunProgress.RecordDailyResult(room.RoomIndex, RunStats.Kills);
                if (RunMode.Daily) Achievements.Unlock("daily_clear");
            }
            var finalFloor = room.RoomIndex >= 24;
            GUI.Box(new Rect(Screen.width - 360f, 20f, 335f, finalFloor ? 220f : 190f), finalFloor ? "RUN COMPLETE" : "ROOM CLEARED");
            GUI.Label(new Rect(Screen.width - 320f, 52f, 280f, 24f), string.Format("Room {0} complete", room.RoomIndex));
            GUI.Label(new Rect(Screen.width - 320f, 76f, 280f, 20f), string.Format("TIME {0:00}:{1:00}  KILLS {2}", Mathf.FloorToInt(RunStats.ElapsedSeconds / 60f), Mathf.FloorToInt(RunStats.ElapsedSeconds % 60f), RunStats.Kills));
            if (!finalFloor)
            {
                GUI.Label(new Rect(Screen.width - 340f, 100f, 300f, 24f), "1 SAFE   2 RISK   3 FORGE");
                GUI.Label(new Rect(Screen.width - 340f, 124f, 300f, 24f), "Choose a route");
                GUI.Label(new Rect(Screen.width - 340f, 148f, 300f, 24f), string.Format("Credits: {0}  Pickups: {1}", RunProgress.Credits, RunStats.MetaCollected));
            }
            else
            {
                GUI.Label(new Rect(Screen.width - 340f, 100f, 300f, 24f), "The 24-floor route is complete");
                GUI.Label(new Rect(Screen.width - 340f, 124f, 300f, 24f), string.Format("Credits {0}  Meta pickups {1}", RunStats.CreditsCollected, RunStats.MetaCollected));
                GUI.Label(new Rect(Screen.width - 340f, 148f, 300f, 24f), string.Format("Crystals {0}  Blueprints {1}  Diary {2}", MetaProgression.Crystals, MetaProgression.Blueprints, MetaProgression.DiaryEntries));
                if (GUI.Button(new Rect(Screen.width - 340f, 180f, 300f, 32f), "RETURN TO LOBBY"))
                    SceneFlow.GoToLobby();
            }
        }
    }
}
