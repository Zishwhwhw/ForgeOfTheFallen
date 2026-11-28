using FotF.View.Heroes;
using UnityEngine;

namespace FotF.View.Enemies
{
    /// <summary>
    /// Мини-босс Капитан стражи (GDD §7.6): HP 400, урон 18–25. Выпад по линии и круговой взмах — оба с красной зоной
    /// на полу (п.86); на 50% HP зовёт 2 мечников. Числа: attackDamage..special.maxDamage, summonAtHpFraction, summonCount,
    /// summon; длины атак — special (lungeDistance, lungeSeconds, spinRadius, windup — EnemyDefinition.windupSeconds).
    /// </summary>
    public sealed class CaptainBehaviour : EnemyBehaviour
    {
        [Tooltip("Красная линия выпада (спрайт, растягивается по X)")]
        [SerializeField] SpriteRenderer lineZone;
        [Tooltip("Красный круг взмаха")]
        [SerializeField] SpriteRenderer circleZone;
        [SerializeField] EnemyController summonPrefab;

        enum Phase { Chase, LungeWindup, Lunge, SpinWindup, Recover }
        Phase _phase;
        float _t, _cooldown = 1.2f;
        Vector2 _dir;
        bool _summoned, _hit, _nextSpin;
        static readonly System.Collections.Generic.List<Collider2D> Hits = new System.Collections.Generic.List<Collider2D>();

        public override bool Tick(float dt, out Vector2 wish)
        {
            wish = Vector2.zero;
            var to = ToTarget(out float dist);
            if (dist == float.MaxValue) { Zones(false, false); Face(Vector2.down, "idle"); return true; }
            if (!_summoned && Owner.State.Hp <= Owner.State.MaxHp * Special("summonAtHpFraction", 0.5f)) Summon();
            float windup = Mathf.Max(0.6f, Owner.Definition.windupSeconds);
            switch (_phase)
            {
                case Phase.Chase:
                    _cooldown -= dt;
                    wish = to / dist * Owner.MoveSpeed;
                    Face(to, "walk");
                    if (_cooldown <= 0f && dist < Special("lungeDistance", 5f))
                    {
                        _nextSpin = !_nextSpin;
                        _phase = _nextSpin && dist < Special("spinRadius", 2.2f) + 0.5f ? Phase.SpinWindup : Phase.LungeWindup;
                        _t = 0f; _dir = to / dist;
                    }
                    break;
                case Phase.LungeWindup:
                    _t += dt; Zones(true, false);
                    if (lineZone) { lineZone.transform.right = _dir; lineZone.transform.localScale = new Vector3(Special("lungeDistance", 5f), 0.9f, 1f); }
                    Face(_dir, "windup");
                    if (_t >= windup) { _phase = Phase.Lunge; _t = 0f; _hit = false; Zones(false, false); }
                    break;
                case Phase.Lunge:
                    _t += dt;
                    float lungeSeconds = Special("lungeSeconds", 0.3f);
                    wish = _dir * (Special("lungeDistance", 5f) / lungeSeconds);
                    Face(_dir, "attack");
                    if (!_hit) _hit = HitInRadius(0.8f, _dir);
                    if (_t >= lungeSeconds) { _phase = Phase.Recover; _t = 0f; }
                    break;
                case Phase.SpinWindup:
                    _t += dt; Zones(false, true);
                    if (circleZone) circleZone.transform.localScale = Vector3.one * Special("spinRadius", 2.2f) * 2f;
                    Face(_dir, "windup");
                    if (_t >= windup) { HitInRadius(Special("spinRadius", 2.2f), Vector2.zero); Zones(false, false); _phase = Phase.Recover; _t = 0f; }
                    break;
                case Phase.Recover:
                    _t += dt;
                    Face(_dir, "attack");
                    if (_t >= Owner.Definition.recoverSeconds) { _phase = Phase.Chase; _cooldown = 0.8f; }
                    break;
            }
            return true;
        }

        bool HitInRadius(float radius, Vector2 dir)
        {
            var filter = new ContactFilter2D { useTriggers = false, useLayerMask = true, layerMask = 1 << LayerMask.NameToLayer("Hero") };
            Physics2D.OverlapCircle(transform.position, radius, filter, Hits);
            foreach (var c in Hits)
            {
                var hero = c.GetComponentInParent<HeroController>();
                if (hero == null) continue;
                int dmg = Random.Range(Owner.Definition.attackDamage, (int)Special("maxDamage", Owner.Definition.attackDamage) + 1);
                Owner.HitHero(hero, dmg, dir.sqrMagnitude > 0f ? dir : ((Vector2)(hero.transform.position - transform.position)).normalized);
                return true;
            }
            return false;
        }

        void Summon()
        {
            _summoned = true;
            if (!summonPrefab) return;
            int n = (int)Special("summonCount", 2);
            for (int i = 0; i < n; i++) Owner.SpawnChild(summonPrefab, transform.position + (Vector3)(Random.insideUnitCircle.normalized * 1.5f));
        }

        void Zones(bool line, bool circle)
        {
            if (lineZone) lineZone.enabled = line;
            if (circleZone) circleZone.enabled = circle;
        }

        public override void OnDisabled() { Zones(false, false); if (_phase != Phase.Chase) { _phase = Phase.Recover; _t = 0f; } }
        public override void OnDeath() => Zones(false, false);
    }
}
