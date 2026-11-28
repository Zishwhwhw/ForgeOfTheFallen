using FotF.View.Heroes;
using UnityEngine;

namespace FotF.View.Enemies
{
    /// <summary>
    /// Летучая мышь и химера «крыса + мышь» (GDD §7.4–7.5): хаотично кружит вокруг героя, затем с телеграфом
    /// пикирует по прямой. Числа: special.values (orbitRadius, diveCooldown, diveSpeed, diveSeconds, diveDamage).
    /// </summary>
    public sealed class DiveBehaviour : EnemyBehaviour
    {
        enum Phase { Circle, Windup, Dive, Recover }
        Phase _phase;
        float _t, _cooldown, _wobble;
        Vector2 _diveDir;
        bool _hitThisDive;
        static readonly System.Collections.Generic.List<Collider2D> Hits = new System.Collections.Generic.List<Collider2D>();

        protected override void Awake()
        {
            base.Awake();
            _cooldown = Random.Range(1f, 2.5f);
            _wobble = Random.Range(0f, 10f);
        }

        public override bool Tick(float dt, out Vector2 wish)
        {
            wish = Vector2.zero;
            var to = ToTarget(out float dist);
            if (dist == float.MaxValue) { Face(Vector2.down, "idle"); return true; }
            var tele = Owner.Telegraph;
            switch (_phase)
            {
                case Phase.Circle:
                    _cooldown -= dt; _wobble += dt;
                    float r = Special("orbitRadius", 3f);
                    Vector2 around = new Vector2(Mathf.Cos(_wobble * 1.7f), Mathf.Sin(_wobble * 2.3f)) * r;   // хаотичная орбита
                    Vector2 goal = (Vector2)Owner.Target.transform.position + around - (Vector2)transform.position;
                    wish = goal.normalized * Owner.MoveSpeed;
                    Face(wish, "walk");
                    if (_cooldown <= 0f) { _phase = Phase.Windup; _t = 0f; _diveDir = to / dist; }
                    break;
                case Phase.Windup:
                    _t += dt;
                    if (tele) { tele.enabled = true; tele.transform.right = _diveDir; }
                    Face(_diveDir, "windup");
                    if (_t >= Mathf.Max(0.4f, Owner.Definition.windupSeconds)) { _phase = Phase.Dive; _t = 0f; _hitThisDive = false; if (tele) tele.enabled = false; }
                    break;
                case Phase.Dive:
                    _t += dt;
                    wish = _diveDir * Special("diveSpeed", 10f);
                    Face(_diveDir, "attack");
                    if (!_hitThisDive) TryHit();
                    if (_t >= Special("diveSeconds", 0.35f)) { _phase = Phase.Recover; _t = 0f; }
                    break;
                case Phase.Recover:
                    _t += dt;
                    if (_t >= Owner.Definition.recoverSeconds) { _phase = Phase.Circle; _cooldown = Special("diveCooldown", 2.5f); }
                    break;
            }
            return true;
        }

        void TryHit()
        {
            var filter = new ContactFilter2D { useTriggers = false, useLayerMask = true, layerMask = 1 << LayerMask.NameToLayer("Hero") };
            Physics2D.OverlapCircle(transform.position, 0.6f, filter, Hits);
            foreach (var c in Hits)
            {
                var hero = c.GetComponentInParent<HeroController>();
                if (hero == null) continue;
                int dmg = (int)Special("diveDamage", Owner.Definition.attackDamage);
                float poison = Special("poisonChance", 0f);
                Owner.HitHero(hero, dmg, _diveDir, poison > 0f ? new[] { Data.Element.Poison } : null, poison);
                _hitThisDive = true;
                break;
            }
        }

        public override void OnDisabled()
        {
            if (Owner.Telegraph) Owner.Telegraph.enabled = false;
            if (_phase == Phase.Windup || _phase == Phase.Dive) { _phase = Phase.Recover; _t = 0f; }
        }
    }
}
