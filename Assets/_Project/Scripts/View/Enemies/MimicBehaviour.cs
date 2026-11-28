using FotF.View.Animation;
using FotF.View.Combat;
using UnityEngine;

namespace FotF.View.Enemies
{
    /// <summary>
    /// Мимик (GDD §7.4): притворяется сундуком и «дышит»; просыпается, если герой подошёл или ударил.
    /// Дальше дерётся как мечник. Дроп: золото + сферы, без награды сундука (обычный дроп врага).
    /// </summary>
    public sealed class MimicBehaviour : EnemyBehaviour
    {
        [SerializeField] SpriteRenderer bodyRenderer;
        [SerializeField] Sprite chestSprite;
        [SerializeField] float revealDistance = 1.8f;

        bool _revealed;
        DirectionalSpriteAnimator _animator;
        Vector3 _scale;

        protected override void Awake()
        {
            base.Awake();
            _animator = Owner.Body;
            _scale = bodyRenderer ? bodyRenderer.transform.localScale : Vector3.one;
            if (_animator) _animator.enabled = false;
            if (bodyRenderer && chestSprite) bodyRenderer.sprite = chestSprite;
        }

        public override bool Tick(float dt, out Vector2 wish)
        {
            wish = Vector2.zero;
            if (_revealed) return false;
            ToTarget(out float dist);
            if (bodyRenderer) bodyRenderer.transform.localScale = new Vector3(_scale.x, _scale.y * (1f + Mathf.Sin(Time.time * 2.2f) * 0.04f), _scale.z); // дышит
            if (dist < revealDistance) Reveal();
            return true;
        }

        public override void OnHit(in HitInfo hit) => Reveal();

        void Reveal()
        {
            if (_revealed) return;
            _revealed = true;
            if (bodyRenderer) bodyRenderer.transform.localScale = _scale;
            if (_animator) _animator.enabled = true;
        }
    }
}
