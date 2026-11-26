using FotF.View.Heroes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FotF.View.UI
{
    /// <summary>HUD серой коробки: HP / броня / мана и оружие в руках. Только отображает State.</summary>
    public sealed class HeroHudView : MonoBehaviour
    {
        [SerializeField] HeroController hero;
        [SerializeField] Image hpFill, armorFill, manaFill;
        [SerializeField] TextMeshProUGUI hpText, armorText, manaText, weaponText, dashText;

        void LateUpdate()
        {
            if (hero == null) return;
            var s = hero.State;
            Set(hpFill, hpText, s.Hp, s.MaxHp);
            Set(armorFill, armorText, s.Armor, s.MaxArmor);
            Set(manaFill, manaText, s.Mana, s.MaxMana);
            if (weaponText) weaponText.text = $"[{s.ActiveSlot + 1}] {s.ActiveWeapon.WeaponId}   (Q — сменить)";
            if (dashText) dashText.text = s.DashCharges > 0 ? "Рывок: готов" : $"Рывок: {s.DashRechargeLeft:0.0} с";
        }

        static void Set(Image fill, TextMeshProUGUI text, int value, int max)
        {
            if (fill) fill.fillAmount = max > 0 ? value / (float)max : 0f;
            if (text) text.text = $"{value}/{max}";
        }
    }
}
