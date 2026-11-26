using UnityEngine;

namespace FotF.View.Combat
{
    /// <summary>Белая вспышка спрайта при попадании (GDD §10.2: 0.05 с, всегда).</summary>
    public sealed class SpriteFlash : MonoBehaviour
    {
        [SerializeField] SpriteRenderer target;
        [SerializeField] Material flashMaterial;
        [SerializeField] float seconds = 0.05f;

        Material _normal; float _left;

        void Awake() { if (target) _normal = target.sharedMaterial; }

        public void Flash()
        {
            if (!target || !flashMaterial) return;
            target.sharedMaterial = flashMaterial; _left = seconds;
        }

        void Update()
        {
            if (_left > 0f && (_left -= Time.unscaledDeltaTime) <= 0f) target.sharedMaterial = _normal;
        }
    }
}
