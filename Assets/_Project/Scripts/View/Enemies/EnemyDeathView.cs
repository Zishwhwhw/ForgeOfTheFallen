using FotF.Data;
using FotF.View.Animation;
using FotF.View.Combat;
using UnityEngine;
using UnityEngine.Rendering;

namespace FotF.View.Enemies
{
    /// <summary>
    /// Смерть обычного врага (GDD §10.2, решение 27.11): белое мерцание → сплющивание к ногам →
    /// плоский затемнённый труп до конца этажа (общий лимит — CorpseRegistry). Без покадровой анимации.
    /// Числа — в GameFeelDefinition.
    /// </summary>
    public sealed class EnemyDeathView : MonoBehaviour
    {
        [SerializeField] GameFeelDefinition feel;
        [Tooltip("Спрайт тела: его масштабируем (pivot у ног) и затемняем")]
        [SerializeField] SpriteRenderer body;
        [SerializeField] DirectionalSpriteAnimator animator;
        [SerializeField] SpriteFlash flash;
        [SerializeField] SortingGroup sortingGroup;
        [Tooltip("Порядок трупа: над полом (-10), под живыми (0)")]
        [SerializeField] int corpseSortingOrder = -5;

        enum Phase { Alive, Flash, Squash, Corpse }
        Phase _phase = Phase.Alive;
        float _t;
        Vector3 _startScale;

        public GameFeelDefinition Feel => feel;
        public bool IsCorpse => _phase == Phase.Corpse;

        /// <summary>Запустить смерть. Повторный вызов ничего не делает.</summary>
        public void Play()
        {
            if (_phase != Phase.Alive || !feel || !body) return;
            if (animator) animator.enabled = false;           // кадр замирает: покадровой смерти у мобов нет
            _startScale = body.transform.localScale;
            _phase = Phase.Flash; _t = 0f;
            if (flash) flash.Flash(feel.deathFlashSeconds);
        }

        void Update()
        {
            if (_phase == Phase.Alive || _phase == Phase.Corpse) return;
            _t += Time.deltaTime;
            if (_phase == Phase.Flash)
            {
                if (_t < feel.deathFlashSeconds) return;
                _phase = Phase.Squash; _t = 0f;
            }
            float k = feel.deathSquashSeconds > 0f ? Mathf.Clamp01(_t / feel.deathSquashSeconds) : 1f;
            k = 1f - (1f - k) * (1f - k);                      // ease-out: быстро плющится, мягко ложится
            body.transform.localScale = new Vector3(
                _startScale.x * Mathf.Lerp(1f, feel.deathSquashScaleX, k),
                _startScale.y * Mathf.Lerp(1f, feel.deathSquashScaleY, k),
                _startScale.z);
            if (k >= 1f) BecomeCorpse();
        }

        void BecomeCorpse()
        {
            _phase = Phase.Corpse;
            body.color = Color.Lerp(Color.white, feel.corpseTint, feel.corpseDarken);
            if (sortingGroup) sortingGroup.sortingOrder = corpseSortingOrder;
            CorpseRegistry.Add(gameObject, feel.corpseLimitPerFloor);
            enabled = false;
        }
    }
}
