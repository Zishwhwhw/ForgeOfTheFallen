using UnityEngine;

namespace FotF.View.Enemies
{
    /// <summary>
    /// Слизень (GDD §7.4): 50 → 2×20 → 4×8 — при смерти делится на меньших (special.values.stages).
    /// Прыгает: движется рывками. Дроп — только у последней стадии (черновик: иначе золота втрое больше).
    /// </summary>
    public sealed class SplitBehaviour : EnemyBehaviour
    {
        [Tooltip("Префаб для осколков (обычно этот же слизень)")]
        [SerializeField] EnemyController splitPrefab;
        [SerializeField] float hopSeconds = 0.35f, restSeconds = 0.45f;

        int _stage;
        float _hopClock;
        (int count, int hp)[] _stages;

        protected override void Awake()
        {
            base.Awake();
            _stages = ParseStages();
        }

        /// <summary>Стадия осколка: 0 — исходный слизень.</summary>
        public void SetStage(int stage)
        {
            _stage = stage;
            transform.localScale = Vector3.one * Mathf.Pow(0.75f, stage);
        }

        public override bool DropsLoot => _stages == null || _stage >= _stages.Length;

        public override bool Tick(float dt, out Vector2 wish)
        {
            wish = Vector2.zero;
            _hopClock += dt;
            float cycle = hopSeconds + restSeconds;
            bool hopping = _hopClock % cycle < hopSeconds;
            if (hopping) return false;                         // в прыжке — обычное преследование/удар
            var to = ToTarget(out float dist);
            if (dist < Owner.Definition.attackRange) return false;   // рядом — бьёт без паузы
            Face(to, "idle");
            return true;                                       // пауза между прыжками
        }

        public override void OnDeath()
        {
            if (_stages == null || _stage >= _stages.Length || !splitPrefab) return;
            var (count, hp) = _stages[_stage];
            for (int i = 0; i < count; i++)
            {
                var offset = (Vector3)(Random.insideUnitCircle.normalized * 0.5f);
                var child = Owner.SpawnChild(splitPrefab, transform.position + offset);
                child.OverrideMaxHp(hp);
                if (child.TryGetComponent(out SplitBehaviour s)) s.SetStage(_stage + 1);
            }
        }

        (int, int)[] ParseStages()
        {
            var d = Owner.Definition;
            var text = d != null && d.special != null ? d.special.values.Text("stages") : null;
            if (string.IsNullOrEmpty(text)) return new[] { (2, 20), (4, 8) };
            var list = new System.Collections.Generic.List<(int, int)>();
            foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(text, "\"count\"\\s*:\\s*(\\d+)\\s*,\\s*\"maxHp\"\\s*:\\s*(\\d+)"))
                list.Add((int.Parse(m.Groups[1].Value), int.Parse(m.Groups[2].Value)));
            return list.Count > 0 ? list.ToArray() : new[] { (2, 20), (4, 8) };
        }
    }
}
