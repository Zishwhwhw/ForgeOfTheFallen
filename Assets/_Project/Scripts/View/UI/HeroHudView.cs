using FotF.View.Heroes;
using FotF.View.World;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FotF.View.UI
{
    /// <summary>HUD серой коробки: HP / броня / мана, оружие, золото, подсказка E, объявления комнаты, смерть.</summary>
    public sealed class HeroHudView : MonoBehaviour
    {
        [SerializeField] HeroController hero;
        [SerializeField] RoomController room;
        [SerializeField] Image hpFill, armorFill, manaFill;
        [SerializeField] TextMeshProUGUI hpText, armorText, manaText, weaponText, dashText;
        [SerializeField] TextMeshProUGUI goldText, promptText, announceText, deathText;
        [SerializeField] float announceSeconds = 2f;

        float _announceLeft;

        void OnEnable() { if (room) room.Announce += OnAnnounce; }
        void OnDisable() { if (room) room.Announce -= OnAnnounce; }

        /// <summary>Короткое объявление по центру сверху (комнаты, этаж).</summary>
        public void ShowAnnouncement(string text) => OnAnnounce(text);

        void OnAnnounce(string text)
        {
            if (!announceText) return;
            announceText.text = text; announceText.enabled = true; _announceLeft = announceSeconds;
        }

        void LateUpdate()
        {
            if (hero == null) return;
            var s = hero.State;
            Set(hpFill, hpText, s.Hp, s.MaxHp);
            Set(armorFill, armorText, s.Armor, s.MaxArmor);
            Set(manaFill, manaText, s.Mana, s.MaxMana);
            if (weaponText) { var w = s.ActiveWeapon.Instance; weaponText.text = w == null ? "(руки пусты)" : $"[{s.ActiveSlot + 1}] {w.DisplayName}  урон {w.Damage}  мана {w.ManaCost}{(w.IsHybrid ? "  ФРАНКЕН" : "")}{(w.IsJackpot ? " ★" : "")}   (Q — сменить)"; }
            if (dashText) dashText.text = s.DashCharges > 0 ? "Рывок: готов" : $"Рывок: {s.DashRechargeLeft:0.0} с";
            if (goldText) goldText.text = s.Bombs > 0 ? $"Золото: {s.Gold}   Бомбы: {s.Bombs}" : $"Золото: {s.Gold}";
            if (promptText) { var f = hero.Focus; promptText.text = f != null && !s.IsDead ? f.Prompt : ""; }
            if (deathText) deathText.enabled = s.IsDead;
            if (announceText && _announceLeft > 0f && (_announceLeft -= Time.unscaledDeltaTime) <= 0f) announceText.enabled = false;
        }

        static void Set(Image fill, TextMeshProUGUI text, int value, int max)
        {
            if (fill) fill.fillAmount = max > 0 ? value / (float)max : 0f;
            if (text) text.text = $"{value}/{max}";
        }
    }
}

