using FotF.Data;
using FotF.Logic.Combat;
using FotF.View.Combat;
using FotF.View.Heroes;
using UnityEngine;

namespace FotF.View.World
{
    /// <summary>
    /// Лут на полу (GDD §7.9). Золото и сферы маны собираются касанием и притягиваются к герою после зачистки
    /// комнаты; зелья, сердце и бомба — по E.
    /// </summary>
    public sealed class LootPickupView : MonoBehaviour, IInteractable
    {
        public enum Kind { Gold, Mana, Item }

        [SerializeField] Kind kind;
        [SerializeField] SpriteRenderer icon;
        [SerializeField] DamageNumberView floatingTextPrefab;
        [SerializeField] float pickupRadius = 0.6f;
        [Tooltip("Золото и мана подтягиваются, если герой ближе")]
        [SerializeField] float attractRadius = 1.5f;
        [SerializeField] float magnetSpeed = 12f;

        int _amount;
        ConsumableDefinition _item;
        bool _magnet, _taken;
        HeroController _hero;

        public bool CanInteract => kind == Kind.Item && !_taken;
        public string Prompt => _item ? $"E — {(_item.id == "item.bomb" ? "взять" : "выпить")}: {ItemName(_item)}" : "";
        public Vector3 Position => transform.position;

        public void InitAmount(int amount) { _amount = amount; }

        public void InitItem(ConsumableDefinition item)
        {
            _item = item;
            if (icon && item && item.icon) icon.sprite = item.icon;
        }

        /// <summary>После зачистки комнаты золото и сферы летят к герою.</summary>
        public void Magnetize() { if (kind != Kind.Item) _magnet = true; }

        void Update()
        {
            if (_taken || kind == Kind.Item) return;
            if (_hero == null) _hero = FindAnyObjectByType<HeroController>();
            if (_hero == null || _hero.State.IsDead) return;
            Vector3 to = _hero.transform.position + Vector3.up * 0.4f - transform.position;
            float d = to.magnitude;
            if (_magnet || d < attractRadius) transform.position += to.normalized * Mathf.Min(d, magnetSpeed * Time.deltaTime);
            if (d < pickupRadius) Collect(_hero);
        }

        void Collect(HeroController hero)
        {
            _taken = true;
            if (kind == Kind.Gold) hero.AddGold(_amount);
            else if (kind == Kind.Mana) Vitals.AddMana(hero.State, _amount);
            Destroy(gameObject);
        }

        public void Interact(HeroController hero)
        {
            if (!CanInteract || _item == null) return;
            _taken = true;
            if (_item.id == "item.bomb") hero.State.Bombs++;
            else
            {
                Vitals.Heal(hero.State, _item.heal);
                Vitals.AddMana(hero.State, _item.mana);
            }
            if (floatingTextPrefab && _item.heal > 0) Instantiate(floatingTextPrefab, transform.position + Vector3.up, Quaternion.identity).Show(_item.heal, false);
            Destroy(gameObject);
        }

        static string ItemName(ConsumableDefinition i) => i.id switch
        {
            "item.potion_hp_small" => "малое зелье здоровья", "item.potion_hp_big" => "большое зелье здоровья",
            "item.potion_mana" => "зелье маны", "item.potion_mixed" => "смешанное зелье", "item.chest_heart" => "сердце (+20 HP)",
            "item.bomb" => "бомба", _ => i.id,
        };
    }
}
