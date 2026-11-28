using FotF.View.Combat;
using FotF.View.World;
using UnityEngine;

namespace FotF.View.Enemies
{
    /// <summary>
    /// Гаргулья (GDD §7.4): пока в комнате есть другие враги (и впереди ещё волны) — неуязвимая статуя, плюётся камнями;
    /// потом оживает и пикирует (DiveBehaviour на том же префабе). Химера «щитоносец + гаргулья» — ходит и бьёт,
    /// оставаясь неуязвимой, затем пикирует.
    /// </summary>
    public sealed class StatueBehaviour : EnemyBehaviour
    {
        [SerializeField] ProjectileView stonePrefab;
        [Tooltip("Химера: ходит и бьёт, пока неуязвима (иначе стоит и плюётся)")]
        [SerializeField] bool walksWhileInvulnerable;
        [SerializeField] DiveBehaviour awakened;
        [SerializeField] SpriteRenderer bodyRenderer;
        [SerializeField] Color statueTint = new Color(0.55f, 0.58f, 0.65f, 1f);

        RoomController _room;
        float _spit = 1.5f;
        bool _awake;
        Color _tint = Color.white;

        protected override void Awake()
        {
            base.Awake();
            _room = GetComponentInParent<RoomController>();
            if (bodyRenderer) { _tint = bodyRenderer.color; bodyRenderer.color = statueTint; }
            if (awakened) awakened.enabled = false;
        }

        bool OthersAlive()
        {
            if (_room != null && _room.HasMoreWaves) return true;
            if (!transform.parent) return false;
            foreach (var e in transform.parent.GetComponentsInChildren<EnemyController>())
                if (e != Owner && !e.State.IsDead) return true;
            return false;
        }

        public override bool Tick(float dt, out Vector2 wish)
        {
            wish = Vector2.zero;
            if (!_awake && !OthersAlive()) Wake();
            if (_awake) return awakened ? awakened.Tick(dt, out wish) : false;
            if (walksWhileInvulnerable) return false;
            var to = ToTarget(out float dist);
            _spit -= dt;
            if (_spit <= 0f && dist < float.MaxValue && stonePrefab)
            {
                _spit = Special("spitCooldown", 2f);
                var dir = to / dist;
                var p = Instantiate(stonePrefab, transform.position + Vector3.up * 0.7f, Quaternion.identity);
                p.Launch(dir, Special("projectileSpeed", 7f), 2.5f, new HitInfo(Owner.Definition.attackDamage, false, dir, 0f, Team.Enemy));
            }
            Face(to, "idle");
            return true;
        }

        void Wake()
        {
            _awake = true;
            if (bodyRenderer) bodyRenderer.color = _tint;
        }

        public override bool Blocks(in HitInfo hit) => !_awake;
        public override void OnDisabled() { if (_awake && awakened) awakened.OnDisabled(); }
    }
}
