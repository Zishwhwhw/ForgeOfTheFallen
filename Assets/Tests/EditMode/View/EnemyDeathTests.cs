using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace FotF.Tests.View
{
    /// <summary>Смерть врага (GDD §10.2): префаб врага проведён. Числа сверяет BalanceDataTests.</summary>
    public class EnemyDeathTests
    {
        const string EnemyPrefab = "Assets/_Project/Prefabs/Enemies/enemy_skeleton_swordsman.prefab";

        [Test]
        public void EnemyPrefab_HasWiredDeathView()
        {
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefab);
            var death = root.GetComponentInChildren<FotF.View.Enemies.EnemyDeathView>(true);
            Assert.IsNotNull(death, "нет EnemyDeathView");
            var so = new SerializedObject(death);
            foreach (var f in new[] { "feel", "constants", "body", "animator", "flash", "sortingGroup" })
                Assert.IsNotNull(so.FindProperty(f).objectReferenceValue, $"EnemyDeathView.{f} не назначен");
            var ctrl = new SerializedObject(root.GetComponent<FotF.View.Enemies.EnemyController>());
            Assert.AreEqual(death, ctrl.FindProperty("deathView").objectReferenceValue, "EnemyController.deathView не назначен");
        }
    }
}
