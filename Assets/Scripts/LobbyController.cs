using UnityEngine;

namespace FrankenArms
{
    public sealed class LobbyController : MonoBehaviour
    {
        private bool achievementsOpen;

        private void OnGUI()
        {
            var previous = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 1280f, Screen.height / 720f, 1f));
            var center = 640f;
            GUI.Box(new Rect(center - 300f, 70f, 600f, 570f), "THE MANOR // LOBBY");
            GUI.Label(new Rect(center - 255f, 125f, 510f, 28f), "Prepare the next experiment");
            GUI.Label(new Rect(center - 255f, 175f, 510f, 24f), "CRYSTALS  " + MetaProgression.Crystals + "    BLUEPRINTS  " + MetaProgression.Blueprints);
            GUI.Label(new Rect(center - 255f, 205f, 510f, 24f), "DIARY ENTRIES  " + MetaProgression.DiaryEntries + "    CREDITS  " + RunProgress.Credits);
            if (GUI.Button(new Rect(center - 255f, 228f, 510f, 20f), "ACHIEVEMENTS  " + Achievements.UnlockedCount + "/" + Achievements.Count))
                achievementsOpen = !achievementsOpen;
            GUI.Label(new Rect(center - 255f, 250f, 510f, 24f), "Roster: Knight / Mage / Mechanic available");
            GUI.Label(new Rect(center - 255f, 278f, 510f, 24f), "Pet: Dog // follows, marks and bites nearby threats");
            var modeLabel = RunMode.DisplayName + (RunMode.Daily ? " // SEED " + RunMode.DailySeed : RunMode.Hardcore ? " // ENEMIES x2 / REWARDS x2" : string.Empty);
            if (GUI.Button(new Rect(center - 190f, 305f, 380f, 30f), "MODE: " + modeLabel))
                RunMode.Toggle();
            if (RunMode.Daily)
            {
                GUI.Label(new Rect(center - 255f, 335f, 510f, 20f), "MODIFIERS: " + RunMode.DailyModifierA + " // " + RunMode.DailyModifierB);
                GUI.Label(new Rect(center - 255f, 355f, 510f, 20f), "DAILY BEST: FLOOR " + RunProgress.DailyBestRoom + " // KILLS " + RunProgress.DailyBestKills);
            }
            var garden = MetaProgression.GardenUnlocked ? "GARDEN UNLOCKED" : "UNLOCK GARDEN // 3 CRYSTALS";
            var workshop = MetaProgression.WorkshopUnlocked ? "WORKSHOP UNLOCKED" : "UNLOCK WORKSHOP // 5 CRYSTALS";
            var upgradeY = RunMode.Daily ? 385f : 345f;
            if (GUI.Button(new Rect(center - 190f, upgradeY, 380f, 34f), garden) && !MetaProgression.GardenUnlocked)
                MetaProgression.UnlockGarden();
            if (GUI.Button(new Rect(center - 190f, upgradeY + 40f, 380f, 34f), workshop) && !MetaProgression.WorkshopUnlocked)
                MetaProgression.UnlockWorkshop();
            var modulePack = MetaProgression.ModulePackUnlocked ? "MODULE PACK UNLOCKED" : "UNLOCK MODULE PACK // 1 BLUEPRINT";
            if (GUI.Button(new Rect(center - 190f, upgradeY + 80f, 380f, 34f), modulePack) && !MetaProgression.ModulePackUnlocked)
                MetaProgression.UnlockModulePack();
            if (GUI.Button(new Rect(center - 190f, upgradeY + 130f, 380f, 56f), "START NEW RUN"))
                SceneFlow.StartRun();
            if (GUI.Button(new Rect(center - 190f, upgradeY + 200f, 380f, 36f), "BACK TO MAIN MENU"))
                SceneFlow.GoToMainMenu();
            if (achievementsOpen)
            {
                GUI.Box(new Rect(970f, 120f, 285f, 250f), "ACHIEVEMENT GALLERY");
                for (var i = 0; i < Achievements.Count; i++)
                {
                    var y = 155f + i * 38f;
                    GUI.Label(new Rect(990f, y, 245f, 18f), (Achievements.IsUnlockedAt(i) ? "● " : "○ ") + Achievements.LabelAt(i));
                    GUI.Label(new Rect(1005f, y + 17f, 240f, 16f), Achievements.DescriptionAt(i));
                }
            }
            GUI.matrix = previous;
        }
    }
}
