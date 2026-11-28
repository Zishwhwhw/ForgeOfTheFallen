using System.Linq;
using FotF.Data;
using FotF.View.Enemies;
using NUnit.Framework;
using UnityEditor;

namespace FotF.Tests.Data
{
    /// <summary>Встречи этажей: у каждого врага встречи есть префаб; на каждом этаже есть обычные, элитные и мини-босс.</summary>
    public class EncounterDataTests
    {
        static EncounterDefinition[] Encounters() => AssetDatabase.FindAssets("t:EncounterDefinition", new[] { "Assets/_Project/Data" })
            .Select(g => AssetDatabase.LoadAssetAtPath<EncounterDefinition>(AssetDatabase.GUIDToAssetPath(g))).ToArray();

        static EnemyController[] Prefabs() => AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Project/Prefabs/Enemies" })
            .Select(g => AssetDatabase.LoadAssetAtPath<EnemyController>(AssetDatabase.GUIDToAssetPath(g))).Where(p => p).ToArray();

        [Test]
        public void EveryEncounterEnemy_HasPrefab()
        {
            var prefabs = Prefabs();
            foreach (var e in Encounters())
                foreach (var w in e.waves)
                    foreach (var g in w.groups)
                        Assert.IsTrue(prefabs.Any(p => p.Definition == g.enemy), $"{e.id}: нет префаба для {g.enemy?.id}");
        }

        [Test]
        public void EveryFloor_HasCombatEliteAndMiniBoss()
        {
            var all = Encounters();
            for (int floor = 1; floor <= 4; floor++)
                foreach (EncounterPool pool in System.Enum.GetValues(typeof(EncounterPool)))
                    Assert.IsTrue(all.Any(e => e.pool == pool && floor >= e.minFloor && floor <= e.maxFloor && e.id != "encounter.greybox"), $"этаж {floor}: нет встречи {pool}");
        }

        [Test]
        public void EveryEnemyDefinition_HasPrefab()
        {
            var prefabs = Prefabs();
            foreach (var g in AssetDatabase.FindAssets("t:EnemyDefinition", new[] { "Assets/_Project/Data" }))
            {
                var d = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(AssetDatabase.GUIDToAssetPath(g));
                Assert.IsTrue(prefabs.Any(p => p.Definition == d), $"нет префаба врага {d.id}");
            }
        }
    }
}
