using FotF.View.Combat;
using FotF.View.Heroes;
using UnityEngine;

namespace FotF.View.World
{
    /// <summary>
    /// Сундук после зачистки комнаты (GDD §7.9). В серой коробке даёт только золото;
    /// оружие из сундука — поставка 3 (подбор и слоты).
    /// </summary>
    public sealed class ChestView : MonoBehaviour, IInteractable
    {
        [SerializeField] SpriteRenderer body;
        [SerializeField] Sprite openedSprite;
        [SerializeField] DamageNumberView floatingTextPrefab;
        [Tooltip("Золото из сундука (GDD §7.9: 10–25)")]
        [SerializeField] Vector2Int goldRange = new Vector2Int(10, 25);

        bool _opened;
        public bool CanInteract => !_opened;
        public string Prompt => "E — открыть сундук";
        public Vector3 Position => transform.position;

        public void Interact(HeroController hero)
        {
            if (_opened) return;
            _opened = true;
            int gold = Random.Range(goldRange.x, goldRange.y + 1);
            hero.AddGold(gold);
            if (openedSprite && body) body.sprite = openedSprite;
            else if (body) body.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            if (floatingTextPrefab) Instantiate(floatingTextPrefab, transform.position + Vector3.up, Quaternion.identity).Show(gold, true);
        }
    }
}
