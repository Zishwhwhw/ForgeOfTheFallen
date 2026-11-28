using FotF.View.Combat;
using UnityEngine;

namespace FotF.View.Enemies
{
    /// <summary>
    /// Особое поведение врага (GDD §7.4–7.6) поверх общего EnemyController (HP, статусы, смерть, дроп).
    /// Компонент на префабе врага; числа — в EnemyDefinition (special.values) и полях компонента.
    /// </summary>
    [RequireComponent(typeof(EnemyController))]
    public abstract class EnemyBehaviour : MonoBehaviour
    {
        protected EnemyController Owner { get; private set; }

        protected virtual void Awake() { Owner = GetComponent<EnemyController>(); }

        /// <summary>Кадр ИИ. true — поведение само решило движение (wish); false — обычный ближний бой.</summary>
        public abstract bool Tick(float dt, out Vector2 wish);

        /// <summary>Попадание поглощено (щит спереди, неуязвимая статуя).</summary>
        public virtual bool Blocks(in HitInfo hit) => false;
        public virtual void OnBlocked(in HitInfo hit) { }
        public virtual void OnHit(in HitInfo hit) { }
        public virtual void OnDeath() { }
        /// <summary>Заморожен / в шоке: прервать замах, луч и т.п.</summary>
        public virtual void OnDisabled() { }
        /// <summary>false — без дропа (например, осколки слизня роняют меньше: решает поведение).</summary>
        public virtual bool DropsLoot => true;

        protected float Special(string key, float fallback)
        {
            var d = Owner.Definition;
            return d != null && d.special != null ? d.special.values.Number(key, fallback) : fallback;
        }

        protected Vector2 ToTarget(out float distance)
        {
            var t = Owner.Target;
            if (t == null || t.State.IsDead) { distance = float.MaxValue; return Vector2.zero; }
            Vector2 to = (Vector2)t.transform.position - (Vector2)transform.position;
            distance = to.magnitude;
            return to;
        }

        protected void Face(Vector2 dir, string clip)
        {
            if (!Owner.Body) return;
            Owner.Body.SetFacing(dir);
            Owner.Body.Play(clip);
        }
    }
}
