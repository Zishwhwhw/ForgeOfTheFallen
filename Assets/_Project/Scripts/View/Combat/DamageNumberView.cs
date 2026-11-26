using TMPro;
using UnityEngine;

namespace FotF.View.Combat
{
    /// <summary>Всплывающая цифра урона (GDD §10.2: крит оранжевый и крупнее).</summary>
    public sealed class DamageNumberView : MonoBehaviour
    {
        [SerializeField] TextMeshPro label;
        [SerializeField] Color normalColor = new Color32(0xff, 0xff, 0xff, 0xff);
        [SerializeField] Color critColor = new Color32(0xf7, 0x76, 0x22, 0xff);
        [SerializeField] float critScale = 1.5f;
        [SerializeField] float lifetime = 0.6f;
        [SerializeField] float rise = 0.8f;

        float _t; Vector3 _start;

        public void Show(int amount, bool crit)
        {
            label.text = amount.ToString();
            label.color = crit ? critColor : normalColor;
            transform.localScale = Vector3.one * (crit ? critScale : 1f);
            _start = transform.position;
        }

        void Update()
        {
            _t += Time.deltaTime;
            float k = _t / lifetime;
            transform.position = _start + Vector3.up * (rise * Mathf.Sqrt(k));
            var c = label.color; c.a = 1f - k * k; label.color = c;
            if (_t >= lifetime) Destroy(gameObject);
        }
    }
}
