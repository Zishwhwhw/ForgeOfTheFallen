using UnityEngine;

namespace FotF.View.Combat
{
    /// <summary>
    /// Манекен для серой коробки: получает попадания, показывает цифры урона и белую вспышку (GDD §10.2).
    /// Не умирает. Будет заменён врагами в поставке 2.
    /// </summary>
    public sealed class TrainingDummyView : MonoBehaviour, IDamageable
    {
        [SerializeField] SpriteRenderer body;
        [SerializeField] DamageNumberView damageNumberPrefab;
        [SerializeField] Vector3 numberOffset = new Vector3(0f, 1.2f, 0f);
        [Tooltip("Белая вспышка, с (GDD §10.2: 0.05)")]
        [SerializeField] float flashSeconds = 0.05f;
        [SerializeField] Material flashMaterial;

        public Team Team => Team.Enemy;
        public int TotalDamage { get; private set; }

        Material _normal; float _flashLeft;

        void Awake() { if (body) _normal = body.sharedMaterial; }

        public void TakeHit(in HitInfo hit)
        {
            TotalDamage += hit.Amount;
            if (damageNumberPrefab)
            {
                var n = Instantiate(damageNumberPrefab, transform.position + numberOffset + (Vector3)(Random.insideUnitCircle * 0.2f), Quaternion.identity);
                n.Show(hit.Amount, hit.IsCrit);
            }
            if (body && flashMaterial) { body.sharedMaterial = flashMaterial; _flashLeft = flashSeconds; }
        }

        void Update()
        {
            if (_flashLeft > 0f && (_flashLeft -= Time.deltaTime) <= 0f && body) body.sharedMaterial = _normal;
        }
    }
}
