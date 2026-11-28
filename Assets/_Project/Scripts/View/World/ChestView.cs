using FotF.Data;
using FotF.Logic.Loot;
using FotF.View.Combat;
using FotF.View.Heroes;
using UnityEngine;

namespace FotF.View.World
{
    /// <summary>
    /// Сундук после боевой комнаты (GDD §7.9): содержимое — по таблице ChestLoot (через FloorRunner этажа:
    /// там счёт оружия на этаж). Без этажа (сцена GreyBox) — только золото.
    /// </summary>
    public sealed class ChestView : MonoBehaviour, IInteractable
    {
        [SerializeField] SpriteRenderer body;
        [SerializeField] Sprite openedSprite;
        [SerializeField] DamageNumberView floatingTextPrefab;
        [Tooltip("Золото из сундука без этажа (GDD §7.9: 10–25)")]
        [SerializeField] Vector2Int goldRange = new Vector2Int(10, 25);
        [SerializeField] LootPickupView itemPrefab;
        [SerializeField] WeaponPickupView weaponPrefab;

        bool _opened;
        public bool CanInteract => !_opened;
        public string Prompt => "E — открыть сундук";
        public Vector3 Position => transform.position;

        public void Interact(HeroController hero)
        {
            if (_opened) return;
            _opened = true;
            if (openedSprite && body) body.sprite = openedSprite;
            else if (body) body.color = new Color(0.7f, 0.7f, 0.7f, 1f);

            var runner = FindAnyObjectByType<FloorRunner>();
            var roll = runner ? runner.RollChest() : new LootRoll { Kind = LootKind.Gold, Amount = Random.Range(goldRange.x, goldRange.y + 1) };
            Vector3 drop = transform.position + Vector3.down * 0.9f;
            switch (roll.Kind)
            {
                case LootKind.Gold:
                    hero.AddGold(roll.Amount);
                    if (floatingTextPrefab) Instantiate(floatingTextPrefab, transform.position + Vector3.up, Quaternion.identity).Show(roll.Amount, true);
                    break;
                case LootKind.Weapon:
                    var def = runner ? runner.Catalog.Find(roll.ItemId) : null;
                    if (def && weaponPrefab) Instantiate(weaponPrefab, drop, Quaternion.identity, transform.parent).Init(WeaponFactory.Create(def));
                    break;
                default:
                    var item = runner ? runner.Consumable(roll.ItemId) : null;
                    if (item && itemPrefab) Instantiate(itemPrefab, drop, Quaternion.identity, transform.parent).InitItem(item);
                    break;
            }
        }
    }
}
