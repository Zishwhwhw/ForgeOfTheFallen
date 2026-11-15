using UnityEngine;

namespace FrankenArms
{
    /// <summary>
    /// Compact, readable prototype HUD. It keeps critical state in the safe
    /// area and leaves the center of the arena clear for combat.
    /// </summary>
    public sealed class RunHUD : MonoBehaviour
    {
        private PlayerHealth playerHealth;
        private PlayerCombat playerCombat;
        private PlayerDash playerDash;
        private PlayerMana playerMana;
        private PlayerArmor playerArmor;
        private StatusEffectController playerStatus;
        private CombatRoom room;
        private float runStartTime;
        private float fps;
        private float fpsTimer;
        private int fpsFrames;

        private void Start()
        {
            playerHealth = FindAnyObjectByType<PlayerHealth>();
            playerCombat = FindAnyObjectByType<PlayerCombat>();
            playerDash = FindAnyObjectByType<PlayerDash>();
            playerMana = FindAnyObjectByType<PlayerMana>();
            playerArmor = FindAnyObjectByType<PlayerArmor>();
            playerStatus = FindAnyObjectByType<StatusEffectController>();
            room = FindAnyObjectByType<CombatRoom>();
            runStartTime = Time.time;
        }

        private void Update()
        {
            fpsFrames++;
            fpsTimer += Time.unscaledDeltaTime;
            if (fpsTimer < 0.5f) return;
            fps = fpsFrames / fpsTimer;
            fpsFrames = 0;
            fpsTimer = 0f;
        }

        private void OnGUI()
        {
            if (playerHealth == null || playerCombat == null || room == null) return;


            var safe = Screen.safeArea;
            // Game View's safeArea can be reported in a different origin by
            // the Editor preview; keep the prototype HUD in a conservative
            // bottom-left inset and reserve the top for the forge panel.
            var panel = new Rect(18f, Screen.height - 218f, 300f, 188f);
            GUI.Box(panel, "FRANKENARMS // RUN");
            var hp = playerArmor == null
                ? string.Format("HP  {0}/{1}", playerHealth.CurrentHealth, playerHealth.MaxHealth)
                : string.Format("HP  {0}/{1}   ARM {2}/{3}", playerHealth.CurrentHealth, playerHealth.MaxHealth, playerArmor.CurrentArmor, playerArmor.MaxArmor);
            GUI.Label(new Rect(panel.x + 14f, panel.y + 28f, 225f, 20f), playerHealth.IsDefeated ? "DEFEATED // PRESS R" : hp);
            GUI.Label(new Rect(panel.x + 14f, panel.y + 50f, 225f, 20f), string.Format("{0} {1}  //  {2}", LocalizationService.T("room"), room.RoomIndex, room.RoomKind));
            GUI.Label(new Rect(panel.x + 14f, panel.y + 72f, 225f, 20f), "NEXT  " + room.NextNodeLabel);
            GUI.Label(new Rect(panel.x + 14f, panel.y + 94f, 225f, 20f), string.Format("{0} {1}   {2} {3}", LocalizationService.T("threats"), room.RemainingEnemies, LocalizationService.T("credits"), RunProgress.Credits));
            GUI.Label(new Rect(panel.x + 14f, panel.y + 116f, 225f, 20f), string.Format("ARM DMG {0}  RATE {1:0.0}/s", playerCombat.Damage, playerCombat.ShotsPerSecond));
            var dashText = playerDash == null || playerDash.IsReady ? "DASH READY" : string.Format("DASH {0:0.0}s", playerDash.CooldownRemaining);
            GUI.Label(new Rect(panel.x + 14f, panel.y + 138f, 225f, 20f), dashText + "  [SHIFT]");
            if (playerMana != null)
                GUI.Label(new Rect(panel.x + 14f, panel.y + 160f, 260f, 20f), string.Format("{0} {1}/{2}  {3}", LocalizationService.T("mana"), playerMana.CurrentMana, playerMana.MaxMana, playerCombat.UsesMana ? "FUSED WEAPON" : "BASE WEAPON"));
            if (playerStatus != null && playerStatus.HasStatus)
                GUI.Label(new Rect(panel.x + 14f, panel.y + 182f, 260f, 20f), "STATUS  " + playerStatus.ShortLabel);

            var top = 18f;
            GUI.Label(new Rect(Screen.width - 500f, top, 480f, 20f), string.Format("{0}  {1}    TIME  {2:00}:{3:00}", LocalizationService.T("best_room"), RunProgress.BestRoom, Mathf.FloorToInt((Time.time - runStartTime) / 60f), Mathf.FloorToInt((Time.time - runStartTime) % 60f)));
            GUI.Label(new Rect(Screen.width - 500f, top + 22f, 480f, 20f), "WASD / LEFT STICK move   SHIFT / LB dash   LMB / RT fire");
            var manaStatus = playerMana == null ? "MANA --" : string.Format("MANA {0}/{1}", playerMana.CurrentMana, playerMana.MaxMana);
            GUI.Label(new Rect(Screen.width - 500f, top + 44f, 480f, 20f), "F forge interact   1/2/3 route   " + manaStatus + "   " + RunMode.DisplayName + "   P " + ProjectileBudget.Active + "/" + ProjectileBudget.MaxActive + "   FPS " + Mathf.RoundToInt(fps));
            if (RunMode.Daily)
                GUI.Label(new Rect(Screen.width - 500f, top + 66f, 480f, 20f), RunMode.DailyModifierA + " // " + RunMode.DailyModifierB);
        }
    }
}
