using UnityEngine;

namespace FrankenArms
{
    /// <summary>Lightweight world-space damage number for the pixel combat slice.</summary>
    public sealed class FloatingDamageText : MonoBehaviour
    {
        private TextMesh textMesh;
        private float lifetime = 0.65f;
        private Color baseColor;

        internal void Initialize(int amount, bool critical)
        {
            textMesh = gameObject.AddComponent<TextMesh>();
            textMesh.text = amount.ToString();
            textMesh.fontSize = 34;
            textMesh.characterSize = 0.08f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            baseColor = critical ? new Color(1f, 0.72f, 0.18f) : new Color(1f, 0.9f, 0.7f);
            textMesh.color = baseColor;
            gameObject.transform.localScale = Vector3.one;
        }

        private void Update()
        {
            lifetime -= Time.unscaledDeltaTime;
            transform.position += Vector3.up * (Time.unscaledDeltaTime * 0.85f);
            if (textMesh != null)
            {
                var color = baseColor;
                color.a = Mathf.Clamp01(lifetime / 0.35f);
                textMesh.color = color;
            }
            if (lifetime <= 0f) Destroy(gameObject);
        }
    }
}
