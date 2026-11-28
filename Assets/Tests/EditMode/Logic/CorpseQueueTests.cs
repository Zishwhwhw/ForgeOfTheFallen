using System.Collections.Generic;
using FotF.Logic.Combat;
using NUnit.Framework;

namespace FotF.Tests.Logic
{
    /// <summary>GDD §10.2, п.165: трупы лежат до конца этажа, сверх общего лимита убирается самый старый.</summary>
    public class CorpseQueueTests
    {
        [Test]
        public void UnderLimit_NothingEvicted()
        {
            var q = new CorpseQueue<int>(3); var ev = new List<int>();
            q.Add(1, ev); q.Add(2, ev); q.Add(3, ev);
            Assert.IsEmpty(ev);
            Assert.AreEqual(3, q.Count);
        }

        [Test]
        public void OverLimit_EvictsOldestFirst()
        {
            var q = new CorpseQueue<int>(2); var ev = new List<int>();
            q.Add(1, ev); q.Add(2, ev); q.Add(3, ev);
            CollectionAssert.AreEqual(new[] { 1 }, ev);
            ev.Clear(); q.Add(4, ev);
            CollectionAssert.AreEqual(new[] { 2 }, ev);
            Assert.AreEqual(2, q.Count);
        }

        [Test]
        public void LoweredLimit_EvictsDownToLimit()
        {
            var q = new CorpseQueue<int>(5); var ev = new List<int>();
            for (int i = 0; i < 5; i++) q.Add(i, ev);
            q.Limit = 2; q.Add(5, ev);
            CollectionAssert.AreEqual(new[] { 0, 1, 2, 3 }, ev);
        }

        [Test]
        public void Clear_EmptiesFloor()
        {
            var q = new CorpseQueue<int>(5); var ev = new List<int>();
            q.Add(1, ev); q.Clear();
            Assert.AreEqual(0, q.Count);
        }
    }
}
