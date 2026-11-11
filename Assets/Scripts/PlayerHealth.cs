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

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        private void Update()
        {
            invulnerability -= Time.deltaTime;
            if (!defeated || Keyboard.current == null || !Keyboard.current.rKey.wasPressedThisFrame) return;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        internal void TakeDamage(int amount)
        {
            if (defeated || invulnerability > 0f) return;
            currentHealth -= amount;
            invulnerability = 0.8f;
            if (currentHealth <= 0)
            {
                defeated = true;
                Debug.Log("Run ended: player defeated");
            }
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(20f, 218f, 240f, 24f), defeated ? "DEFEATED // Press R to restart" : string.Format("HP {0}/{1}", currentHealth, maxHealth));
        }
    }
}
