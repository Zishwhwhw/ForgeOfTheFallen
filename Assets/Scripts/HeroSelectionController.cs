using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace FrankenArms
{
    public sealed class HeroSelectionController : MonoBehaviour
    {
        private HeroDefinition[] heroes;
        private SpriteRenderer playerRenderer;
        private PlayerMovement movement;
        private PlayerCombat combat;
        private PlayerHealth health;
        private PlayerArmor armor;
        private PlayerMana mana;
        private bool selected;
        private string status = "Choose your hero";

        internal bool IsSelected { get { return selected; } }
        internal HeroDefinition SelectedHero { get; private set; }

        private void Awake()
        {
            playerRenderer = GetComponent<SpriteRenderer>();
            movement = GetComponent<PlayerMovement>();
            combat = GetComponent<PlayerCombat>();
            health = GetComponent<PlayerHealth>();
            armor = GetComponent<PlayerArmor>();
            mana = GetComponent<PlayerMana>();
            heroes = Resources.LoadAll<HeroDefinition>("Content/Heroes");
            Array.Sort(heroes, CompareHeroOrder);
            Time.timeScale = 0f;
        }

        private static int CompareHeroOrder(HeroDefinition left, HeroDefinition right)
        {
            return SelectionOrder(left.displayName).CompareTo(SelectionOrder(right.displayName));
        }

        private static int SelectionOrder(string name)
        {
            switch (name)
            {
                case "Knight": return 0;
                case "Mage": return 1;
                case "Mechanic": return 2;
                default: return 100;
            }
        }

        private void Update()
        {
            if (selected || Keyboard.current == null) return;
            if (Keyboard.current.digit1Key.wasPressedThisFrame) Choose(0);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) Choose(1);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) Choose(2);
        }

        private void Choose(int index)
        {
            if (heroes == null || index < 0 || index >= heroes.Length) return;
            var hero = heroes[index];
            SelectedHero = hero;
            movement.ConfigureSpeed(hero.moveSpeed);
            combat.ConfigureBaseWeapon(hero.baseDamage, hero.baseShotsPerSecond);
            health.Configure(hero.maxHealth);
            armor.Configure(hero.maxArmor);
            mana.Configure(hero.maxMana);
            if (playerRenderer != null) playerRenderer.color = hero.tint;
            status = hero.displayName + " // " + hero.role;
            selected = true;
            Time.timeScale = 1f;
        }

        private void OnGUI()
        {
            if (selected || heroes == null || heroes.Length < 3) return;
            GUI.Box(new Rect(Screen.width * 0.5f - 245f, 150f, 490f, 245f), "CHOOSE YOUR HERO");
            GUI.Label(new Rect(Screen.width * 0.5f - 220f, 180f, 440f, 24f), status);
            for (var i = 0; i < 3; i++)
            {
                var hero = heroes[i];
                var y = 215f + i * 48f;
                var label = string.Format("{0}. {1} // {2} // HP {3} ARM {4} MANA {5}", i + 1, hero.displayName, hero.role, hero.maxHealth, hero.maxArmor, hero.maxMana);
                if (GUI.Button(new Rect(Screen.width * 0.5f - 220f, y, 440f, 34f), label)) Choose(i);
            }
            GUI.Label(new Rect(Screen.width * 0.5f - 220f, 365f, 440f, 22f), "1/2/3 or click // choice starts the first room");
            GUI.Label(new Rect(Screen.width * 0.5f - 220f, 389f, 440f, 22f), "Roster abilities: Dash / Skill / Ultimate-Passive");
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
        }
    }
}
