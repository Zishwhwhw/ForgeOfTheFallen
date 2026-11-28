using FotF.View.Combat;
using UnityEngine;

namespace FotF.View.Enemies
{
    /// <summary>
    /// Культист-ритуалист (GDD §7.4): держится позади, видимым лучом лечит ближайшего союзника и даёт ему щит.
    /// Удар или шок прерывают луч. Числа: special.values (healPerSecond, shield, beamRange, keepDistance, interruptSeconds).
    /// </summary>
    public sealed class HealBeamBehaviour : EnemyBehaviour
    {
        [SerializeField] LineRenderer beam;

        EnemyController _ally;
        float _interrupted, _healAcc, _search;

        public override bool Tick(float dt, out Vector2 wish)
        {
            wish = Vector2.zero;
            var to = ToTarget(out float dist);
            float keep = Special("keepDistance", 6f);
            if (dist < keep) wish = -to.normalized * Owner.MoveSpeed;         // держится позади
            _interrupted -= dt;
            _search -= dt;
            if (_search <= 0f || _ally == null || _ally.State.IsDead) { _ally = FindAlly(); _search = 0.5f; }

            bool beaming = _ally != null && _interrupted <= 0f;
            if (beam)
            {
                beam.enabled = beaming;
                if (beaming) { beam.SetPosition(0, transform.position + Vector3.up * 0.6f); beam.SetPosition(1, _ally.transform.position + Vector3.up * 0.5f); }
            }
            if (beaming)
            {
                _healAcc += Special("healPerSecond", 6f) * dt;
                if (_healAcc >= 1f) { int h = (int)_healAcc; _healAcc -= h; _ally.Heal(h); }
                _ally.State.Shield = Mathf.Max(_ally.State.Shield, (int)Special("shield", 10f));
                Face(_ally.transform.position - transform.position, "windup");
            }
            else if (_ally == null && dist < float.MaxValue && dist > keep + 1f) wish = to.normalized * Owner.MoveSpeed * 0.5f;
            if (!beaming) Face(wish.sqrMagnitude > 0.01f ? wish : (Vector2)to, wish.sqrMagnitude > 0.01f ? "walk" : "idle");
            return true;
        }

        EnemyController FindAlly()
        {
            if (!transform.parent) return null;
            float range = Special("beamRange", 7f);
            EnemyController best = null; float bestD = range * range;
            foreach (var e in transform.parent.GetComponentsInChildren<EnemyController>())
            {
                if (e == Owner || e.State.IsDead || e.Behaviour is HealBeamBehaviour) continue;
                float d = ((Vector2)(e.transform.position - transform.position)).sqrMagnitude;
                if (d < bestD) { bestD = d; best = e; }
            }
            return best;
        }

        public override void OnHit(in HitInfo hit) => Interrupt();
        public override void OnDisabled() => Interrupt();
        public override void OnDeath() { if (beam) beam.enabled = false; }

        void Interrupt()
        {
            _interrupted = Special("interruptSeconds", 1.5f);
            if (beam) beam.enabled = false;
        }
    }
}
