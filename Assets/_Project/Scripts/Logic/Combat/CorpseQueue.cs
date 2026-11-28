using System.Collections.Generic;

namespace FotF.Logic.Combat
{
    /// <summary>
    /// Общий лимит трупов на этаж (GDD §10.2, п.165; constants.corpseLimitPerFloor).
    /// Трупы копятся в порядке смерти; сверх лимита убирается самый старый.
    /// </summary>
    public sealed class CorpseQueue<T>
    {
        readonly Queue<T> _corpses = new Queue<T>();

        public int Limit { get; set; }
        public int Count => _corpses.Count;

        public CorpseQueue(int limit) { Limit = limit; }

        /// <summary>Добавить труп. В evicted — трупы, которые надо убрать (самые старые сверх лимита).</summary>
        public void Add(T corpse, List<T> evicted)
        {
            _corpses.Enqueue(corpse);
            while (Limit >= 0 && _corpses.Count > Limit) evicted.Add(_corpses.Dequeue());
        }

        public void Clear() => _corpses.Clear();
    }
}
