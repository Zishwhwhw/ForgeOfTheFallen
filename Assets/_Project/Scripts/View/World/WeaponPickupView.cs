using FotF.Data;
using FotF.State;
using FotF.View.Combat;
using FotF.View.Heroes;
using TMPro;
using UnityEngine;

namespace FotF.View.World
{
    /// <summary>
    /// Оружие на полу (GDD §5.1): E — подобрать. Если оба слота заняты, текущее оружие падает на пол (лежит до конца этажа).
    /// </summary>
    public sealed class WeaponPickupView : MonoBehaviour, IInteractable
    {
        [SerializeField] WeaponCatalog catalog;
        [Tooltip("Оружие, которое лежит здесь с начала сцены (пусто — экземпляр приходит из кода, например из кузницы)")]
        [SerializeField] WeaponDefinition initialWeapon;
        [SerializeField] SpriteRenderer icon;
        [SerializeField] SpriteRenderer detail;
        [SerializeField] TextMeshPro label;
        [SerializeField] float bobHeight = 0.08f;

        WeaponInstance _weapon;
        Vector3 _base;

        public WeaponInstance Weapon => _weapon;
        public bool CanInteract => _weapon != null;
        public string Prompt => _weapon == null ? "" : $"E — взять: {_weapon.DisplayName}{(_weapon.IsHybrid ? $"  (родители: {_weapon.ParentA} + {_weapon.ParentB})" : "")}";
        public Vector3 Position => transform.position;

        void Start() { if (_weapon == null && initialWeapon) Init(WeaponFactory.Create(initialWeapon)); }

        public void Init(WeaponInstance w)
        {
            _weapon = w; _base = transform.position;
            var cat = catalog;
            var b = cat ? cat.Find(w.DefinitionId) : null; var d = w.IsHybrid && cat ? cat.Find(w.DetailId) : null;
            if (icon) { icon.sprite = b ? b.sprite : null; icon.color = w.IsHybrid ? new Color32(0xff, 0xc8, 0x9a, 0xff) : Color.white; }
            if (detail) { detail.sprite = d ? d.sprite : null; detail.enabled = d != null; }
            if (label) label.text = w.DisplayName;
        }

        void Update()
        {
            if (_weapon == null) return;
            transform.position = _base + Vector3.up * (Mathf.Sin(Time.time * 3f) * bobHeight);
        }

        public void Interact(HeroController hero)
        {
            if (_weapon == null) return;
            var dropped = hero.PickUp(_weapon);
            if (dropped == null) { Destroy(gameObject); return; }
            Init(dropped);                       // обмен: текущее оружие героя ложится на это место
        }
    }
}

