using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace FrankenArms
{
    public sealed class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 5;
        private int currentHealth;
        private float invulnerability;
        private bool defeated;
        private PlayerArmor armor;

        internal int CurrentHealth { get { return currentHealth; } }
        internal int MaxHealth { get { return maxHealth; } }
        internal bool IsDefeated { get { return defeated; } }

        private void Awake()
        {
            currentHealth = maxHealth;
            armor = GetComponent<PlayerArmor>();
        }

        private void Update()
        {
            invulnerability -= Time.deltaTime;
            if (!defeated || Keyboard.current == null || !Keyboard.current.rKey.wasPressedThisFrame) return;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void OnGUI()
        {
            if (!defeated) return;
            var panel = new Rect(Screen.width * 0.5f - 190f, Screen.height * 0.5f - 120f, 380f, 240f);
            GUI.Box(panel, "RUN INTERRUPTED");
            GUI.Label(new Rect(panel.x + 35f, panel.y + 48f, 310f, 24f), "Your progress is safe in the Manor.");
            GUI.Label(new Rect(panel.x + 35f, panel.y + 72f, 310f, 20f), string.Format("TIME {0:00}:{1:00}  KILLS {2}  PICKUPS {3}", Mathf.FloorToInt(RunStats.ElapsedSeconds / 60f), Mathf.FloorToInt(RunStats.ElapsedSeconds % 60f), RunStats.Kills, RunStats.MetaCollected));
            if (GUI.Button(new Rect(panel.x + 35f, panel.y + 112f, 310f, 38f), "RESTART RUN"))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            if (GUI.Button(new Rect(panel.x + 35f, panel.y + 162f, 310f, 38f), "RETURN TO LOBBY"))
            {
                Time.timeScale = 1f;
                SceneFlow.GoToLobby();
            }
        }

        internal void TakeDamage(int amount)
        {
            if (defeated || invulnerability > 0f) return;
            if (armor != null && armor.AbsorbDamage(amount))
            {
                invulnerability = 0.8f;
                return;
            }
            currentHealth -= amount;
            invulnerability = 0.8f;
            ScreenShake.Pulse(0.16f, 0.1f);
            var popupObject = new GameObject("Player Damage Number");
            popupObject.transform.position = transform.position + new Vector3(0f, 0.45f, 0f);
            popupObject.AddComponent<FloatingDamageText>().Initialize(amount, false);
            if (currentHealth <= 0)
            {
                defeated = true;
                var room = FindAnyObjectByType<CombatRoom>();
                RunProgress.RecordDailyResult(room == null ? 0 : room.RoomIndex, RunStats.Kills);
                Debug.Log("Run ended: player defeated");
            }
        }

        internal void GrantInvulnerability(float seconds)
        {
            if (seconds > invulnerability)
                invulnerability = seconds;
        }

        internal void Heal(int amount)
        {
            if (defeated || amount <= 0) return;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        }

        internal bool TradeMaxHealth(int amount)
        {
            if (defeated || amount <= 0 || maxHealth - amount < 1) return false;
            maxHealth -= amount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            return true;
        }

        internal void Configure(int value)
        {
            maxHealth = Mathf.Max(1, value);
            currentHealth = maxHealth;
            defeated = false;
            invulnerability = 0f;
        }

    }
}
