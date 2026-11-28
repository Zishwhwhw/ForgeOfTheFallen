using System.Collections.Generic;
using FotF.Logic.Combat;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FotF.View.Enemies
{
    /// <summary>
    /// Трупы этажа (GDD §10.2, п.165): лежат до конца этажа, общий лимит — самые старые убираются.
    /// Этаж = загруженная сцена; при выгрузке сцены список очищается.
    /// </summary>
    public static class CorpseRegistry
    {
        static readonly CorpseQueue<GameObject> Queue = new CorpseQueue<GameObject>(300);
        static readonly List<GameObject> Evicted = new List<GameObject>();

        public static int Count => Queue.Count;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetOnLoad()
        {
            Queue.Clear();
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        static void OnSceneUnloaded(Scene _) => Queue.Clear();

        public static void Add(GameObject corpse, int limit)
        {
            Queue.Limit = limit;
            Evicted.Clear();
            Queue.Add(corpse, Evicted);
            foreach (var go in Evicted) if (go) Object.Destroy(go);
        }
    }
}
