using UnityEngine;

namespace FrankenArms
{
    public sealed class ImpactBurst : MonoBehaviour
    {
        private float remaining;
        private SpriteRenderer renderer;
        private Vector3 initialScale;

        internal void Initialize(Color color, float duration, float scale)
        {
            renderer = GetComponent<SpriteRenderer>();
            if (renderer != null) renderer.color = color;
            remaining = duration;
            initialScale = Vector3.one * scale;
            transform.localScale = initialScale * 0.35f;
        }

        private void Update()
        {
            remaining -= Time.deltaTime;
            var normalized = Mathf.Clamp01(remaining / 0.12f);
            transform.localScale = Vector3.Lerp(initialScale, Vector3.one * 0.15f, 1f - normalized);
            if (renderer != null) renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, normalized);
            if (remaining <= 0f) Destroy(gameObject);
        }
    }
}
