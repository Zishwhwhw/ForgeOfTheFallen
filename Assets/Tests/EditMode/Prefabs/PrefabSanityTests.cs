using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace FotF.Tests.Prefabs
{
    /// <summary>
    /// Регрессия 27.11.2026: HeroController сохранился в префабе выключенным, управление не работало.
    /// Все скрипты в префабах проекта должны быть включены и без потерянных ссылок на скрипты.
    /// </summary>
    public class PrefabSanityTests
    {
        /// <summary>Регрессия 27.11.2026: слои не сохранились в TagManager, удары и пули никого не находили.</summary>
        [Test]
        public void GameplayLayers_Exist()
        {
            foreach (var name in new[] { "Walls", "Hero", "Enemy", "Projectile" })
                Assert.GreaterOrEqual(LayerMask.NameToLayer(name), 0, $"нет слоя {name} в Project Settings → Tags and Layers");
        }

        [Test]
        public void HeroAndEnemyPrefabs_AreOnTheirLayers()
        {
            Assert.AreEqual(LayerMask.NameToLayer("Hero"), AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Heroes/hero_knight.prefab").layer);
            Assert.AreEqual(LayerMask.NameToLayer("Enemy"), AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemies/enemy_skeleton_swordsman.prefab").layer);
        }

        [TestCase("Assets/_Project/Prefabs/Interactables/prop_forge.prefab", "ForgeView", "rules,catalog,plateA,plateB,dropPoint,pickupPrefab")]
        [TestCase("Assets/_Project/Prefabs/Interactables/pickup_weapon.prefab", "WeaponPickupView", "catalog,icon,detail,label")]
        [TestCase("Assets/_Project/Prefabs/Heroes/hero_knight.prefab", "WeaponView", "catalog,weaponRenderer,detailRenderer")]
        public void FusionPrefabs_HaveRequiredReferences(string path, string component, string fields)
        {
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.IsNotNull(root, path);
            MonoBehaviour mb = null;
            foreach (var c in root.GetComponentsInChildren<MonoBehaviour>(true)) if (c != null && c.GetType().Name == component) mb = c;
            Assert.IsNotNull(mb, $"{path}: нет {component}");
            var so = new SerializedObject(mb);
            foreach (var f in fields.Split(','))
                Assert.IsNotNull(so.FindProperty(f).objectReferenceValue, $"{path}: {component}.{f} не назначен");
        }

        [Test]
        public void AllProjectPrefabs_HaveEnabledScripts_AndNoMissingScripts()
        {
            var problems = new System.Text.StringBuilder();
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Project/Prefabs" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                {
                    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) > 0) problems.AppendLine($"{path}: missing script on {t.name}");
                    foreach (var mb in t.GetComponents<MonoBehaviour>())
                        if (mb != null && !mb.enabled) problems.AppendLine($"{path}: {mb.GetType().Name} on {t.name} is disabled");
                }
            }
            Assert.IsEmpty(problems.ToString(), problems.ToString());
        }
    }
}
