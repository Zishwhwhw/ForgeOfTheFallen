using UnityEngine;

namespace FotF.View.Combat
{
    /// <summary>Снаряд: летит прямо, попадает в IDamageable другой команды, гаснет о стену или по времени.</summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public sealed class ProjectileView : MonoBehaviour
    {
        [SerializeField] LayerMask wallMask;

        Rigidbody2D _body;
        HitInfo _hit;
        float _lifeLeft;

        void Awake() { _body = GetComponent<Rigidbody2D>(); }

        public void Launch(Vector2 direction, float speed, float lifetime, in HitInfo hit)
        {
            _hit = hit; _lifeLeft = lifetime;
            transform.right = direction;
            _body.linearVelocity = direction.normalized * speed;
        }

        void Update()
        {
            _lifeLeft -= Time.deltaTime;
            if (_lifeLeft <= 0f) Destroy(gameObject);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (((1 << other.gameObject.layer) & wallMask) != 0) { Destroy(gameObject); return; }
            var target = other.GetComponentInParent<IDamageable>();
            if (target == null || target.Team == _hit.Source) return;
            target.TakeHit(new HitInfo(_hit.Amount, _hit.IsCrit, _body.linearVelocity.normalized, _hit.StunSeconds, _hit.Source));
            Destroy(gameObject);
        }
    }
}
